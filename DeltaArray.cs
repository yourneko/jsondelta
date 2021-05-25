using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonDelta
{
    public class DeltaArray : Delta<int>
    {
        public override bool IsValidTarget(object target) => target is List<object>;

        protected override string DeltaTypeName => nameof(DeltaArray);

        internal static IJsonDelta Compare(List<object> first, List<object> last)
                {
                    var matches = Comparer.LongestCommonSubsequence(first, last);
                    var firstRemoved = Enumerable.Range(0, first.Count)
                                                 .Except(matches.Select(x => x.first))
                                                 .OrderByDescending(x => x)
                                                 .Select(x => (x, (IJsonDelta)new DeltaRemove()))
                                                 .ToArray();
                    var lastAdded = Enumerable.Range(0, last.Count)
                                              .Except(matches.Select(x => x.last))
                                              .Select(x => (x, (IJsonDelta)new DeltaInsert(last[x])))
                                              .ToArray();
        
                    var result = new DeltaArray();
                    foreach (var delta in firstRemoved)
                        result.changes.Add(delta);
                    foreach (var delta in lastAdded)
                        result.changes.Add(delta);
        
                    // todo - recursively find edited arrays & objects
                    return result.changes.Count > 0
                               ? result
                               : new Identity();
                }
        
        protected override object Select(object target, int key) => ((List<object>)target)[key];

        protected override object Insert(object target, int key, object obj)
        {
            if (target is List<object> array)
                array.Insert(key, obj);
            else
                throw new InvalidOperationException();
            return this;
        }

        protected override object Remove(object target, int key) 
        {
            if (target is List<object> array)
                array.RemoveAt(key);
            else
                throw new InvalidOperationException();
            return this;
        }
    }
}
