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

        public static IEnumerable<string> Matches(IDatabase db, string providerName, params string[] input)
        {
            // handle empty:
            if (input == null || input.Length == 0)
                return new string[] { };

			// first check - see if we have a direct match:
			Provider provider;
			if (!db.Providers.TryGetValue (providerName, out provider))
				return new string[] {};

			var results = new Dictionary<string,double>();

			IList<int> ids;
            if (provider.LastEntries.TryGetValue (input[input.Length-1].ToLower(), out ids)) 
			{
				var list = new SortedList<double,string>(new DuplicateKeyComparer<double>());
				foreach (var id in ids) 
				{
					var e = provider.Entries[id];

                    bool addPath = input.Length == 1;
                    if (!addPath)
                    {
                        var pathSplit = e.SplitPath;
                        addPath = pathSplit.Length >= input.Length;
                        if (addPath)
                        {
                            int j = pathSplit.Length - 2;
                            // reverse iterate through elements to see if they match in order:
                            for (int i = input.Length - 2; i >= 0; i--)
                            {
                                bool foundInput = false;
                                for (; j >= 0; j--)
                                {
                                    if (String.Compare(pathSplit[j], input[i], true) == 0)
                                    {
                                        foundInput = true;
                                        break;
                                    }
                                }
                                
                                // input element wasn't found; don't add to results
                                if (!foundInput)
                                {
                                    addPath = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (addPath)
						list.Add (e.CalculateFrecency(), e.FullPath);
				}

                

				return list.Values;
			}


			// if we don't have a direct match, try a fuzzy comparison:
			foreach (var e in provider.Entries)
            {
				if (input[input.Length-1].FuzzyEquals(e.Value.FullPath,0.20))
					results.Add(e.Value.FullPath,e.Value.CalculateFrecency());
            }

            List<KeyValuePair<string, double>> myList = results.ToList();

            myList.Sort((firstPair, nextPair) =>
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
            );

            // sort and return:
            return myList.ConvertAll(new Converter<KeyValuePair<string,double>, string>( p => p.Key));
        }
    }
}
    