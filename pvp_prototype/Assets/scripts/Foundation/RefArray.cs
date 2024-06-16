using System;
using System.Buffers;
using System.Collections.Generic;

public class RefArrayItemReference {
    public int index;
}

public enum RefArrayIncreaseType {
    Exponential,
    Incremental,
}

public class RefArray<T, LookupType1, LookupType2> : RefArrayImpl<T, RefArray<T, LookupType1, LookupType2>.Ref> where T : struct {
    public class Ref : RefArrayItemReference {
        public LookupType1 key1;
        public LookupType2 key2;
    }
    public RefArray(int minSize = 0, RefArrayIncreaseType increaseType = RefArrayIncreaseType.Exponential) : base(minSize, increaseType) {}

    // public ref T this[Ref key]  => ref items[key.index];
    public ref T this[LookupType1 key]  => ref items[refByKey1[key].index];
    public ref T this[LookupType2 key]  => ref items[refByKey2[key].index];
    private readonly Dictionary<LookupType1, Ref> refByKey1 = new();
    private readonly Dictionary<LookupType2, Ref> refByKey2 = new();

    public Ref GetReference(LookupType1 key) => refByKey1[key];
    public Ref GetReference(LookupType2 key) => refByKey2[key];
    public void Get(RefArrayItemReference reference, out LookupType1 key) => key = (reference as Ref).key1;
    public void Get(RefArrayItemReference reference, out LookupType2 key) => key = (reference as Ref).key2;

    public Ref Add(LookupType1 key1, LookupType2 key2, T t = default) {
        Ref reference = Add(t);
        reference.key1 = key1;
        reference.key2 = key2;
        refByKey1[key1] = reference;
        refByKey2[key2] = reference;
        return reference;
    }

    public void Remove(LookupType1 key) => Remove(GetReference(key));
    public void Remove(LookupType2 key) => Remove(GetReference(key));

    public new void Remove(Ref reference) {
        refByKey1.Remove(reference.key1);
        refByKey2.Remove(reference.key2);
        base.Remove(reference);
    }
}

public class RefArray<T, LookupType> : RefArrayImpl<T, RefArray<T, LookupType>.Ref> where T : struct {
    public class Ref : RefArrayItemReference {
        public LookupType key;
    }
    public RefArray(int minSize = 0, RefArrayIncreaseType increaseType = RefArrayIncreaseType.Exponential) : base(minSize, increaseType) {}


    public ref T this[LookupType key]  => ref items[refByKey[key].index];
    private readonly Dictionary<LookupType, Ref> refByKey = new();
    public Ref GetReference(LookupType key) => refByKey[key];
    public LookupType Get(RefArrayItemReference reference) => (reference as Ref).key;

    public Ref Add(LookupType key, T t = default) 
    {
        Ref reference = Add(t);
        reference.key = key;
        refByKey[key] = reference;
        return reference;
    }
    public void Remove(LookupType key) => Remove(GetReference(key));
    public new void Remove(Ref reference)
    {
        refByKey.Remove(reference.key);
        base.Remove(reference);
    }
}

public class RefArray<T> : RefArrayImpl<T, RefArray<T>.Ref> where T : struct {
    public class Ref : RefArrayItemReference {}
    public RefArray(int minSize = 0, RefArrayIncreaseType increaseType = RefArrayIncreaseType.Exponential) : base(minSize, increaseType) {}
    public new Ref Add(T t = default) => base.Add(t);
    public new void Remove(Ref reference) => base.Remove(reference);
}

public class RefArrayImpl<T, RefT> where T : struct where RefT : RefArrayItemReference, new() {
    protected T[] items;
    protected RefArrayItemReference[] itemReferences;
    protected int itemCount;
    protected readonly RefArrayIncreaseType increaseType;

    public RefArrayImpl(int minSize = 0, RefArrayIncreaseType increaseType = RefArrayIncreaseType.Exponential) {
        this.increaseType = increaseType;
        items = ArrayPool<T>.Shared.Rent(minSize);
        itemReferences = ArrayPool<RefArrayItemReference>.Shared.Rent(minSize);
    }

    ~RefArrayImpl()
    {
        ArrayPool<T>.Shared.Return(items);
        ArrayPool<RefArrayItemReference>.Shared.Return(itemReferences);
    }

    public ref T this[int index]  => ref items[index];
    public ref T this[RefT reference] => ref items[reference.index];

    protected RefT Add(T t = default) {
        if(itemCount >= items.Length) {
            int newCount = Math.Max(increaseType == RefArrayIncreaseType.Exponential ? itemCount * 2 : itemCount+1, 1);
            var newArray = ArrayPool<T>.Shared.Rent(newCount);
            Array.Copy(items, newArray, itemCount);
            ArrayPool<T>.Shared.Return(items);
            items = newArray;

            var newRefArray = ArrayPool<RefT>.Shared.Rent(newCount);
            Array.Copy(itemReferences, newRefArray, itemCount);
            ArrayPool<RefArrayItemReference>.Shared.Return(itemReferences);
            itemReferences = newRefArray;
        }

        items[itemCount] = t;
        var refT = itemReferences[itemCount] ??= new RefT();
        refT.index = itemCount;
        ++itemCount;

        return refT as RefT;
    }

    public void SwapDelete(RefT reference) {
        int idx = reference.index;
        int lastIdx = itemCount-1;
        items[idx] = items[lastIdx];
        (itemReferences[idx], itemReferences[lastIdx]) = (itemReferences[lastIdx], itemReferences[idx]);
        itemReferences[idx].index = idx;
        itemReferences[lastIdx].index = -1;
        --itemCount;
    }

    protected void Remove(RefT reference) {
        int firstIdx = reference.index;
        for(int i=firstIdx; i<itemCount-1; ++i) {
            int nextIdx = i+1;
            items[i] = items[nextIdx];
            (itemReferences[i], itemReferences[nextIdx]) = (itemReferences[nextIdx], itemReferences[i]);
            itemReferences[i].index = i;
        }
        --itemCount;
        itemReferences[itemCount].index = -1;
    }

    public Span<T> AsSpan() => new(items, 0, itemCount);
    public Span<RefArrayItemReference> ReferencesAsSpan() => new(itemReferences, 0, itemCount);
}