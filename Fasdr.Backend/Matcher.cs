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

        public static IEnumerable<string> Matches(IDatabase db, string providerName, bool filterContainers, bool filterLeaves,  params string[] input)
        {
            // handle empty:
            if (input == null || input.Length == 0)
                return new string[] { };

			Provider provider;
			if (!db.Providers.TryGetValue (providerName, out provider))
				return new string[] {};

            // see if we have a direct match:
            var lastInput = input[input.Length - 1];
            var list = new SortedList<double, string>(new DuplicateKeyComparer<double>());

            // if no direct match, loop through and find best match:
            foreach (var kv in provider.LastEntries)
            {
                int score;
                    
                fts.FuzzyMatcher.FuzzyMatch(lastInput,kv.Value.Name,out score);

                foreach (var id in kv.Value.Ids)
                {
                    // get score from rest of path parts:
                    var entry = provider.Entries[id];
					if (!entry.IsValid || (entry.IsLeaf && filterLeaves))
                        continue;
                    else if (!entry.IsLeaf && filterContainers)
                        continue;
                    //Console.WriteLine($"entry: {entry.FullPath} leaf: {entry.IsLeaf}");

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
                        int subScore;
                        fts.FuzzyMatcher.FuzzyMatch(input[i], entryPathSplit[start], out subScore);
                        start--;
                        score += subScore;
                    }

                    if (score >= 0)
                        list.Add(score + entry.CalculateFrecency(), entry.FullPath);
                }
            }

            return list.Values.ToArray();
        }
    }
}
    