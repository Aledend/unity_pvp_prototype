using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

public enum LookupArrayIncreaseType {
    Exponential,
    Incremental,
}

public class LookupArray<T, LookupType1, LookupType2> : LookupArrayImpl<T> where T : class, new() {
    public LookupArray(int minSize = 0, LookupArrayIncreaseType increaseType = LookupArrayIncreaseType.Exponential) : base(minSize, increaseType) {}

    public T this[LookupType1 key]  => Get(key);
    public T this[LookupType2 key]  => Get(key);
    private readonly Dictionary<LookupType1, T> instanceByKey1 = new();
    private readonly Dictionary<T, LookupType1> key1ByInstance = new();
    private readonly Dictionary<LookupType2, T> instanceByKey2 = new();
    private readonly Dictionary<T, LookupType2> key2ByInstance = new();

    public T Get(LookupType1 key) => instanceByKey1[key];
    public T Get(LookupType2 key) => instanceByKey2[key];

    public void Get(T instance, out LookupType1 key) => key = key1ByInstance[instance];
    public void Get(T instance, out LookupType2 key) => key = key2ByInstance[instance];
    public bool Has(LookupType1 key) => instanceByKey1.ContainsKey(key);
    public bool Has(LookupType2 key) => instanceByKey2.ContainsKey(key);
    public bool Has(LookupType1 key, out T instance) => instanceByKey1.TryGetValue(key, out instance);
    public bool Has(LookupType2 key, out T instance) => instanceByKey2.TryGetValue(key, out instance);

    public T Rent(LookupType1 key1, LookupType2 key2) {
        T instance = Rent();
        instanceByKey1[key1] = instance;
        instanceByKey2[key2] = instance;
        key1ByInstance[instance] = key1;
        key2ByInstance[instance] = key2;
        return instance;
    }

    public void Insert(T instance, LookupType1 key1, LookupType2 key2) {
        Debug.Assert(!Has(instance));

        Insert(instance);
        instanceByKey1[key1] = instance;
        instanceByKey2[key2] = instance;
        key1ByInstance[instance] = key1;
        key2ByInstance[instance] = key2;
    }

    public void Return(LookupType1 key) => Return(Get(key));
    public void Return(LookupType2 key) => Return(Get(key));
    public new void Return(T instance)
    {
        Get(instance, out LookupType1 key1);
        Get(instance, out LookupType2 key2);
        instanceByKey1.Remove(key1);
        instanceByKey2.Remove(key2);
        key1ByInstance.Remove(instance);
        key2ByInstance.Remove(instance);
        base.Return(instance);
    }
}

public class LookupArray<T, LookupType> : LookupArrayImpl<T> where T : class, new() {

    public LookupArray(int minSize = 0, LookupArrayIncreaseType increaseType = LookupArrayIncreaseType.Exponential) : base(minSize, increaseType) {}
    public T this[LookupType key]  => instanceByKey[key];
    private readonly Dictionary<LookupType, T> instanceByKey = new();
    private readonly Dictionary<T, LookupType> keyByInstance = new();
    public T Get(LookupType key) => instanceByKey[key];
    public LookupType Get(T instance) => keyByInstance[instance];
    public bool Has(LookupType key) => instanceByKey.ContainsKey(key);
    public bool Has(LookupType key, out T instance) => instanceByKey.TryGetValue(key, out instance);

    public T Rent(LookupType key) 
    {
        T instance = Rent();
        instanceByKey[key] = instance;
        keyByInstance[instance] = key;
        return instance;
    }

    public void Insert(T instance, LookupType key) {
        Debug.Assert(!Has(instance));
        
        Insert(instance);
        instanceByKey[key] = instance;
        keyByInstance[instance] = key;
    }

    public void Return(LookupType key) => Return(Get(key));
    public new void Return(T instance)
    {
        var key = Get(instance);
        instanceByKey.Remove(key);
        keyByInstance.Remove(instance);
        base.Return(instance);
    }
}

public class LookupArrayImpl<T> where T : class, new() {
    protected T[] items;
    protected readonly Dictionary<T, int> indexByInstance = new();
    protected int itemCount;
    protected readonly LookupArrayIncreaseType increaseType;

    public LookupArrayImpl(int minSize = 0, LookupArrayIncreaseType increaseType = LookupArrayIncreaseType.Exponential) {
        this.increaseType = increaseType;
        items = ArrayPool<T>.Shared.Rent(minSize);
    }

    ~LookupArrayImpl()
    {
        ArrayPool<T>.Shared.Return(items);
    }

    public bool Has(T instance) => indexByInstance.ContainsKey(instance);

    protected T Rent() {
        if(itemCount >= items.Length) {
            int newCount = Math.Max(increaseType == LookupArrayIncreaseType.Exponential ? itemCount * 2 : itemCount+1, 1);

            var newArray = ArrayPool<T>.Shared.Rent(newCount);
            Array.Copy(items, newArray, itemCount);
            ArrayPool<T>.Shared.Return(items);
            items = newArray;
        }

        var item = items[itemCount] ??= new();
        indexByInstance[item] = itemCount;
        ++itemCount;

        return item;
    }

    protected void Insert(T instance) {
        if(itemCount >= items.Length) {
            int newCount = Math.Max(increaseType == LookupArrayIncreaseType.Exponential ? itemCount * 2 : itemCount+1, 1);

            var newArray = ArrayPool<T>.Shared.Rent(newCount);
            Array.Copy(items, newArray, itemCount);
            ArrayPool<T>.Shared.Return(items);
            items = newArray;
        }

        var item = items[itemCount] = instance;
        indexByInstance[item] = itemCount;
        ++itemCount;
    }

    public void SwapReturn(T instance) {
        int index = indexByInstance[instance];
        --itemCount;
        (items[index], items[itemCount]) = (items[itemCount], items[index]);
        indexByInstance[items[index]] = index;
        indexByInstance.Remove(instance);
    }

    protected void Return(T instance) {
        int index = indexByInstance[instance];
        for(int i=index; i<itemCount-1; ++i) {
            items[i] = items[i+1];
            indexByInstance[items[i]] = i;
        }
        indexByInstance.Remove(instance);
        --itemCount;
    }

    public Span<T> AsSpan() => new(items, 0, itemCount);
}