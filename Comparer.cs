using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonDelta
{
    static class Comparer
    {
        public static (int first, int last)[] LongestCommonSubsequence(List<object> first, List<object> last)
        {
            int i, j, k;
            int firstCount = first.Count;
            int lastCount = last.Count;
            int[] z = new int[(firstCount + 1) * (lastCount + 1)];
            int[,] c = new int[firstCount + 1, lastCount + 1];

            for (i = 0; i <= firstCount; ++i)
                c[i, 0] = z[i * (lastCount + 1)];

            for (i = 1; i <= firstCount; ++i)
            {
                for (j = 1; j <= lastCount; ++j)
                {
                    if (JsonComparer.Comparer.Equals(first[i - 1], last[j - 1]))
                        c[i, j] = c[i - 1, j - 1] + 1;
                    else
                        c[i, j] = Math.Max(c[i - 1, j], c[i, j - 1]);
                }
            }

            var output = new (int first, int last)[c[firstCount, lastCount]];

            for (i = firstCount, j = lastCount, k = c[firstCount, lastCount] - 1; k >= 0;)
            {
                if (JsonComparer.Comparer.Equals(first[i - 1], last[j - 1]))
                {
                    --i;
                    --j;
                    --k;
                    output[k] = (i, j);
                }
                else if (c[i, j - 1] > c[i - 1, j])
                    --j;
                else
                    --i;
            }

            return output;
        }
    }
    
    class JsonComparer : IEqualityComparer<object>
    {
        internal static readonly JsonComparer Comparer = new JsonComparer();

        public new bool Equals(object x, object y) => !(x is null || y is null) 
                                                   && x.GetType() == y.GetType() 
                                                   && (x.Equals(y) || ListsEqual(x, y) || PropertiesEqual(x, y));

        public int GetHashCode(object obj) => base.GetHashCode();

        bool ListsEqual(object x, object y) => x is List<object> xArray 
                                            && y is List<object> yArray 
                                            && xArray.Count == yArray.Count 
                                            && xArray.SequenceEqual(yArray, Comparer);
        
        bool PropertiesEqual(object x, object y) => x is Dictionary<string, object> xDictionary 
                                                 && y is Dictionary<string, object> yDictionary 
                                                 && xDictionary.Count == yDictionary.Count 
                                                 && xDictionary.SequenceEqual(yDictionary, KeyValueComparer.Comparer);
    }
    
    class KeyValueComparer : IEqualityComparer<KeyValuePair<string, object>>
    {
        internal static readonly KeyValueComparer Comparer = new KeyValueComparer();
        
        public bool Equals(KeyValuePair<string, object> x, KeyValuePair<string, object> y) => StringComparer.Ordinal.Equals(x.Key, y.Key) 
                                                                                           && JsonComparer.Comparer.Equals(x.Value, y.Value);

        public int GetHashCode(KeyValuePair<string, object> obj) => base.GetHashCode();
    }
}
