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

        public static IEnumerable<string> Matches(IDatabase db,string input,string providerName)
        {
            var results = new Dictionary<string,double>();

			// first check - see if we have a direct match:
			var provider = db.Providers[providerName];
			IList<int> ids;
			if (provider.LastEntries.TryGetValue (input.ToLower (), out ids)) 
			{
				var list = new SortedList<double,string>(new DescComparer<double>());
				foreach (var i in ids) 
				{
					var e = provider.Entries [i];
					list.Add (e.Weight, e.FullPath);
				}

				return list.Values;
			}

			// if we don't have a direct match, try a fuzzy comparison:
            var opts = new List<FuzzyStringComparisonOptions>{ FuzzyStringComparisonOptions.UseLevenshteinDistance};
			foreach (var e in provider.Entries)
            {
				if (ComparisonMetrics.ApproximatelyEquals(input, e.Value.FullPath, opts, FuzzyStringComparisonTolerance.Weak))
					results.Add(e.Value.FullPath,e.Value.Weight);
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
    