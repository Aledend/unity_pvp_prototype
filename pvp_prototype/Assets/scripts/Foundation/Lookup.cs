using System.Collections.Generic;

public class Lookup<LookupKey1, LookupKey2, Value> {
    public Value this[LookupKey1 key] => Get(key);
    public Value this[LookupKey2 key] => Get(key);
    private readonly Dictionary<LookupKey1, Value> instanceByKey1 = new();
    private readonly Dictionary<LookupKey2, Value> instanceByKey2 = new();
    private readonly Dictionary<LookupKey1, LookupKey2> key2ByKey1 = new();
    private readonly Dictionary<LookupKey2, LookupKey1> key1ByKey2 = new();

    public Value Get(LookupKey1 key) => instanceByKey1[key];
    public Value Get(LookupKey2 key) => instanceByKey2[key];

    public bool Has(LookupKey1 key) => instanceByKey1.ContainsKey(key);
    public bool Has(LookupKey2 key) => instanceByKey2.ContainsKey(key);
    public bool Has(LookupKey1 key, out Value instance) => instanceByKey1.TryGetValue(key, out instance);
    public bool Has(LookupKey2 key, out Value instance) => instanceByKey2.TryGetValue(key, out instance);

    public void Set(LookupKey1 key1, LookupKey2 key2, Value value)
    {
        instanceByKey1[key1] = value;
        instanceByKey2[key2] = value;
        key1ByKey2[key2] = key1;
        key2ByKey1[key1] = key2;
    }

    public void Remove(LookupKey1 key1)
    {
        LookupKey2 key2 = key2ByKey1[key1];
        Remove(key1, key2);
    }

    public void Remove(LookupKey2 key2)
    {
        LookupKey1 key1 = key1ByKey2[key2];
        Remove(key1, key2);
    }

    public void Remove(LookupKey1 key1, LookupKey2 key2)
    {
        key2ByKey1.Remove(key1);
        key1ByKey2.Remove(key2);
        instanceByKey1.Remove(key1);
        instanceByKey2.Remove(key2);
    }
}
