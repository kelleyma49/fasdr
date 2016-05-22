using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuoVia.FuzzyStrings;
using System.Collections;

namespace Fasdr.Backend
{
    public static class Matcher
    {
        static readonly int SmallerEntryScorePenalty = -10;

		public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
		{
			public int Compare(TKey x, TKey y)
			{
				int result = y.CompareTo(x);

				if (result == 0)
					return 1;   // Handle equality as being greater
				else
					return result;
			}
		}
		
        public static string[] Matches(IDatabase db, string providerName, bool filterContainers, bool filterLeaves,  bool matchAllOnEmptyInput, params string[] input)
        {
            // handle empty:
            if (input == null || input.Length == 0)
                return new string[] { };

			Provider provider;
			if (!db.Providers.TryGetValue (providerName, out provider))
				return new string[] {};

            if (matchAllOnEmptyInput && (input == null || input.Length == 0 || String.IsNullOrWhiteSpace(input[0])))
            {
                return provider.GetAllEntries().ToArray();
            }
   
            // see if we have a direct match:
            var lastInput = input[input.Length - 1];
            bool endOfMatchOnly = lastInput[lastInput.Length - 1] == '$';
            if (endOfMatchOnly)
            {
                lastInput = lastInput.Substring(0, lastInput.Length - 1); // remove '$'
            }
            var listExact = new SortedList<double, string>(new DuplicateKeyComparer<double>());
            var list = new SortedList<double, string>(new DuplicateKeyComparer<double>());

            // if no direct match, loop through and find best match:
            foreach (var kv in provider.LastEntries)
            {
                int score = 0;

                var name = kv.Value.Name;
                bool exact = false;
                if (endOfMatchOnly)
                {
                    if (name.Length >= lastInput.Length)
                        exact = string.Compare(lastInput, name.Substring(name.Length - lastInput.Length)) == 0;
                    if (!exact)
                        score = -1; // prevent entries that don't match
                }
                else
                {
                    exact = String.Compare(lastInput, name, true) == 0;
                    if (!exact)
                        fts.FuzzyMatcher.FuzzyMatch(lastInput, name, out score);
                }
                    

                foreach (var id in kv.Value.Ids)
                {
                    // get score from rest of path parts:
                    var entry = provider.Entries[id];
					if (!entry.IsValid || (entry.IsLeaf && filterLeaves))
                        continue;
                    else if (!entry.IsLeaf && filterContainers)
                        continue;
                    
                    var entryPathSplit = entry.SplitPath;
                    int start = entryPathSplit.Length - 2;
                    for (int i = input.Length - 2; i >= 0; i--)
                    {
                        // if stored string is smaller than input string, apply penalty:
                        if (start < 0)
                        {
                            score -= SmallerEntryScorePenalty;
                            break;
                              
                        }
                        int subScore = 0;
                        exact = exact && String.Compare(input[i], entryPathSplit[start], true) == 0;
                        if (!exact)
                            fts.FuzzyMatcher.FuzzyMatch(input[i], entryPathSplit[start], out subScore);
                        start--;
                        score += subScore;
                    }

                    if (exact)
                        listExact.Add(score + entry.CalculateFrecency(),entry.FullPath);
                    else if (score >= 0)
                        list.Add(score + entry.CalculateFrecency(), entry.FullPath);
                }
            }

            return listExact.Values.Concat(list.Values).ToArray();
        }
    }
}
    