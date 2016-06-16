using System;
using System.Collections.Generic;
using System.Linq;

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
		
        public static string[] Matches(
            IDatabase db, 
            string providerName, 
            bool filterContainers, 
            bool filterLeaves,  
            bool matchAllOnEmptyInput, 
            string basePathMatch,
            params string[] input)
        {
            // handle empty:
            if (input == null || input.Length == 0)
                return new string[] { };

			Provider provider;
			if (!db.Providers.TryGetValue (providerName, out provider))
				return new string[] {};

            if (matchAllOnEmptyInput && (input == null || input.Length == 0 || string.IsNullOrWhiteSpace(input[0])))
            {
                return provider.GetAllEntries().ToArray();
            }

            var lastInput = input[input.Length - 1];

            bool matchBasePath = !string.IsNullOrWhiteSpace(basePathMatch) && lastInput.StartsWith("**", StringComparison.Ordinal);
            string[] basePathMatchSplit = null;
            if (matchBasePath)
            {
                basePathMatchSplit = basePathMatch.Split(new char[] { '\\', '/' },StringSplitOptions.RemoveEmptyEntries);
                lastInput = lastInput.Substring("**".Length);
            }


            bool exactMatch = lastInput.Length > 1 && lastInput[0] == '=';
            bool tryPrefixMatch = lastInput.Length > 1 && (lastInput[0] == '^');
            if (tryPrefixMatch || exactMatch)
            {
                lastInput = lastInput.Substring(1); // remove '^'
            }
            bool trySuffixMatch = lastInput.Length > 1 && lastInput[lastInput.Length - 1] == '$';            
            if (trySuffixMatch)
            {
                lastInput = lastInput.Substring(0, lastInput.Length - 1); // remove '$'
            }

            // exact match overrides other matches:
            if (exactMatch)
            {
                tryPrefixMatch = trySuffixMatch = false;
            }

            var listExact = new SortedList<double, string>(new DuplicateKeyComparer<double>());
            var list = new SortedList<double, string>(new DuplicateKeyComparer<double>());

            // if no direct match, loop through and find best match:
            foreach (var kv in provider.LastEntries)
            {
                int score = 0;

                var name = kv.Value.Name;
                bool exact = false;
                if (tryPrefixMatch)
                {
                    if (name.Length >= lastInput.Length)
                        exact = string.Compare(lastInput, name.Substring(0,lastInput.Length), ignoreCase: true) == 0;
                    if (!exact)
                        score = -1; // prevent entries that don't match
                }
                if (trySuffixMatch)
                {
                    if (name.Length >= lastInput.Length)
                        exact = (!tryPrefixMatch || exact) && 
                            string.Compare(lastInput, name.Substring(name.Length - lastInput.Length), ignoreCase: true) == 0;
                    if (!exact)
                        score = -1; // prevent entries that don't match
                }

                if (!tryPrefixMatch && !trySuffixMatch)
                {
                    exact = string.Compare(lastInput, name, true) == 0;
                    if (!exact && !exactMatch)
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

                    // match going forward if we match the base path:
                    if (matchBasePath)
                    {
                        bool addEntry = basePathMatchSplit.Length <= entryPathSplit.Length - 1;
                        if (addEntry)
                        {
                            // -1 as we already compared the last entry:
                            for (int i = 0; i < basePathMatchSplit.Length; i++)
                            {
                                addEntry = string.Compare(basePathMatchSplit[i], entryPathSplit[i], true) == 0;
                                if (!addEntry)
                                    break;
                            }
                        }

                        // only add it the base path matches:
                        if (addEntry)
                        {
                            if (exact)
                                listExact.Add(score + entry.CalculateFrecency(), entry.FullPath);
                            else if (score >= 0)
                                list.Add(score + entry.CalculateFrecency(), entry.FullPath);
                        }
                    }
                    else
                    { 
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
                            listExact.Add(score + entry.CalculateFrecency(), entry.FullPath);
                        else if (score >= 0)
                            list.Add(score + entry.CalculateFrecency(), entry.FullPath);
                    }
                }
            }

            if (exactMatch)
                return listExact.Values.ToArray();
            else
                return listExact.Values.Concat(list.Values).ToArray();
        }
    }
}
    