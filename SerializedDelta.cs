using System;
using System.Collections.Generic;

namespace JsonDelta
{
    public class SerializedDelta
    {
        public object Key;
        public string DeltaType;
        public object Content;

        public IJsonDelta Deserialize()
        {
            switch (DeltaType)
            {
                case nameof(Identity):
                    return new Identity();
                case nameof(DeltaRemove): 
                    return new DeltaRemove();
                case nameof(DeltaInsert): 
                    return new DeltaInsert(MiniJSON.Json.Deserialize((string)Content));
                case nameof(DeltaReplace): 
                    return new DeltaReplace(MiniJSON.Json.Deserialize((string)Content));
                case nameof(DeltaArray):
                    var array = new DeltaArray();
                    foreach (var change in (SerializedDelta[])Content)
                        array.Add((int)change.Key, change.Deserialize());
                    return array;
                case nameof(DeltaObject):
                    var obj = new DeltaObject();
                    foreach (var change in (SerializedDelta[])Content)
                        obj.Add((string)change.Key, change.Deserialize());
                    return obj;
                default: throw new Exception($"Delta type {DeltaType} is not supported.");
            }
        }

        public static bool TryDeserializeJson(string json, out SerializedDelta delta)
        {
            var deserialized = MiniJSON.Json.Deserialize(json);
            if (deserialized is Dictionary<string, object> dictionary
                && dictionary.TryGetValue("Key", out var key)
                && dictionary.TryGetValue("DeltaType", out var deltaType)
                && dictionary.TryGetValue("Content", out var content))
            {
                delta = new SerializedDelta() {
                                                  Content = content,
                                                  Key = key,
                                                  DeltaType = deltaType.ToString()
                                              };
                return true;
            }

            delta = null;
            return false;

        }
    }
}
