using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonDelta
{
    public class DeltaObject : Delta<string>
    {
        public override bool IsValidTarget(object target) => target is Dictionary<string, object>;

        protected override string DeltaTypeName => nameof(DeltaObject);
        
        internal static IJsonDelta Compare(Dictionary<string, object> first, Dictionary<string, object> last)
        {
            var result = new DeltaObject();
            
            var firstKeys = first.Select(x => x.Key).ToArray();
            var lastKeys = last.Select(x => x.Key).ToArray();

            var removedKeys = firstKeys.Except(lastKeys).ToArray();
            var addedKeys = lastKeys.Except(firstKeys).ToArray();
            var keysKept = firstKeys.Intersect(lastKeys).ToArray();

            foreach (var rm in removedKeys)
                result.changes.Add((rm, new DeltaRemove()));

            foreach (var add in addedKeys)
                result.changes.Add((add, new DeltaInsert(last[add])));

            foreach (var key in keysKept)
            {
                var delta = CompareElement(first[key], last[key]);
                if (!(delta is Identity))
                    result.changes.Add((key, delta));
            }

            return result.changes.Any() 
                       ? result 
                       : new Identity();
        }

        protected override object Select(object target, string key) => ((Dictionary<string, object>)target)[key];

        protected override object Insert(object target, string key, object obj)
        {
            if (target is Dictionary<string, object> properties)
                properties.Add(key, obj);
            else
                throw new InvalidOperationException();
            return this;
        }

        protected override object Remove(object target, string key)
        {
            if (target is Dictionary<string, object> properties)
                properties.Remove(key);
            else
                throw new InvalidOperationException();
            return this;
        }
    }
}
