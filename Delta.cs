using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonDelta
{
    public interface IJsonDelta
    {
        bool IsValidTarget(object target);
        object Apply(object target);
        SerializedDelta GetSerialized();
    }

    public abstract class Delta<T> : IJsonDelta
    {
        protected readonly List<(T key, IJsonDelta delta)> changes = new();

        public object Apply(object target)
        {
            return changes.Aggregate(target,
                                     (current, change) => change.delta switch {
                                                              Identity             => current,
                                                              DeltaInsert insert   => Insert(current, change.key, insert.ObjToInsert),
                                                              DeltaRemove          => Remove(current, change.key),
                                                              DeltaReplace replace => Insert(Remove(current, change.key), change.key, replace.ObjToInsert),
                                                              DeltaArray arrayEdit => arrayEdit.Apply(Select(current, change.key)),
                                                              DeltaObject objEdit  => objEdit.Apply(Select(current, change.key)),
                                                              _                    => throw new NotImplementedException(),
                                                          });
        }
        
        public SerializedDelta GetSerialized() => new() {
                                                            DeltaType = DeltaTypeName,
                                                            Content = changes.Select(SerializeChange),
                                                        };

        public Delta<T> Add(T key, IJsonDelta delta)
        {
            changes.Add((key, delta));
            return this;
        }
        
        public Delta<T> Add(Delta<T> other)
        {
            foreach (var change in other.changes)
                changes.Add(change);
            return this;
        }

        protected static IJsonDelta CompareElement(object first, object last)
        {
            if (first is null)
                return new DeltaInsert(last);
            if (last is null)
                return new DeltaRemove();
            if (first.GetType() != last.GetType())
                return new DeltaReplace(last);
            return first is List<object> fArray && last is List<object> lArray
                       ? DeltaArray.Compare(fArray, lArray)
                       : first is Dictionary<string, object> fProp && last is Dictionary<string, object> lProp
                           ? DeltaObject.Compare(fProp, lProp)
                           : first.Equals(last)
                               ? new Identity()
                               : new DeltaReplace(last);

        }

        SerializedDelta SerializeChange((T key, IJsonDelta delta) change)
        {
            var result = change.delta.GetSerialized();
            result.Key = change.key;
            return result;
        }
        
        public abstract bool IsValidTarget(object target);
        protected abstract object Select(object target, T key);
        protected abstract object Insert(object target, T key, object obj);
        protected abstract object Remove(object target, T key);
        protected abstract string DeltaTypeName { get; }
    }
    
    public class Identity : IJsonDelta
    {
        public bool IsValidTarget(object target) => true;

        public object Apply(object target) => throw new InvalidOperationException();

        public SerializedDelta GetSerialized() => new() {
                                                            DeltaType = nameof(Identity),
                                                            Content = null,
                                                        };
    }
    
    public class DeltaInsert : IJsonDelta
    {
        internal readonly object ObjToInsert;
        
        public bool IsValidTarget(object target) => target is List<object> || target is Dictionary<string, object>;

        public object Apply(object target) => throw new InvalidOperationException();
        
        public SerializedDelta GetSerialized() => new() {
                                                            DeltaType = nameof(DeltaInsert),
                                                            Content = MiniJSON.Json.Serialize(ObjToInsert),
                                                        };

        public DeltaInsert(object obj)
        {
            ObjToInsert = obj;
        }
    }
    
    public class DeltaRemove : IJsonDelta
    {
        public bool IsValidTarget(object target) => target is List<object> || target is Dictionary<string, object>;

        public object Apply(object target) => throw new InvalidOperationException();
        
        public SerializedDelta GetSerialized() => new() {
                                                            DeltaType = nameof(DeltaRemove),
                                                            Content = null,
                                                        };
    }
    
    public class DeltaReplace : IJsonDelta
    {
        internal readonly object ObjToInsert;
        
        public bool IsValidTarget(object target) => target is List<object> || target is Dictionary<string, object>;

        public object Apply(object target) => throw new InvalidOperationException();
        
        public SerializedDelta GetSerialized() => new() {
                                                            DeltaType = nameof(DeltaReplace),
                                                            Content = MiniJSON.Json.Serialize(ObjToInsert),
                                                        };

        public DeltaReplace(object obj)
        {
            ObjToInsert = obj;
        }
    }
}
