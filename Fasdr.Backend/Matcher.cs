using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuzzyString;
using System.Collections;

namespace Fasdr.Backend
{
    public static class Matcher
    {
		private class DescComparer<T> : IComparer<T>
		{
			public int Compare(T x, T y)
			{
				return Comparer<T>.Default.Compare(y, x);
			}
		}

        public static IEnumerable<string> Matches(IDatabase db, string providerName, params string[] input)
        {
            // handle empty:
            if (input == null || input.Length == 0)
                return new string[] { };

            var results = new Dictionary<string,double>();

			// first check - see if we have a direct match:
			var provider = db.Providers[providerName];
			IList<int> ids;
            if (provider.LastEntries.TryGetValue (input[input.Length-1].ToLower(), out ids)) 
			{
				var list = new SortedList<double,string>(new DescComparer<double>());
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
            var opts = new List<FuzzyStringComparisonOptions>{ FuzzyStringComparisonOptions.UseLevenshteinDistance};
			foreach (var e in provider.Entries)
            {
				if (ComparisonMetrics.ApproximatelyEquals(input[input.Length-1], e.Value.FullPath, opts, FuzzyStringComparisonTolerance.Weak))
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
    