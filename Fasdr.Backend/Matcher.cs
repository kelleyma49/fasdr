using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuzzyString;

namespace Fasdr.Backend
{
    public static class Matcher
    {
        public static IEnumerable<string> Matches(IDatabase db,string input,string provider)
        {
            var results = new Dictionary<string,double>();
            var opts = new List<FuzzyStringComparisonOptions>{ FuzzyStringComparisonOptions.UseLevenshteinDistance};
			foreach (var e in db.Providers[provider].Entries)
            {
                if (ComparisonMetrics.ApproximatelyEquals(input, e.Key, opts, FuzzyStringComparisonTolerance.Weak))
                    results.Add(e.Key,e.Value.Weight);
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
    