using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

public class ExtensionInitContext {
    public bool isServer;
}


public class ExtensionDataReference {
    public int idx;
    public Unit unit;
    public GameObject gameObject;
    public Action<Unit> removeHandle;
    public ExtensionDataReference(int idx, Unit unit, Action<Unit> removeHandle) {
        this.idx = idx;
        this.unit = unit;
        gameObject = unit.gameObject;
        this.removeHandle = removeHandle;
    }
}

public interface IExtensionDataList {
    public abstract bool Has(Unit unit);
    public abstract bool Has(Unit unit, out ExtensionDataReference reference);
    public abstract bool Has(GameObject gameObject);
    public abstract bool Has(GameObject gameObject, out ExtensionDataReference reference);
    public ExtensionDataReference Generate(Unit unit, Action<Unit> removeHandle);
    public void Delete(ExtensionDataReference reference);
}

public class ExtensionDataList<ExtensionDataType> : IExtensionDataList where ExtensionDataType : struct
{
    private static ExtensionDataType[] extensionDatas = ArrayPool<ExtensionDataType>.Shared.Rent(minDataCount);
    private static ExtensionDataReference[] extensionDataReferences = ArrayPool<ExtensionDataReference>.Shared.Rent(minDataCount);
    private static readonly Dictionary<Unit, ExtensionDataReference> referenceByUnit = new();
    private static readonly Dictionary<GameObject, ExtensionDataReference> referenceByGameObject = new();
    private static readonly Dictionary<ExtensionDataReference, Unit> unitByReference = new();
    private static int dataCount;
    const int minDataCount = 8;

    public ref ExtensionDataType Get(ExtensionDataReference idxRef) => ref extensionDatas[idxRef.idx];
    public ref ExtensionDataType Get(Unit unit) => ref extensionDatas[referenceByUnit[unit].idx];
    public ref ExtensionDataType Get(GameObject gameObject) => ref extensionDatas[referenceByGameObject[gameObject].idx];
    public ref ExtensionDataType GetByIteratorIndex(int idx) => ref extensionDatas[idx];
    public Unit GetUnit(ExtensionDataReference reference) => unitByReference[reference];
    public bool Has(Unit unit) => referenceByUnit.ContainsKey(unit);
    public bool Has(Unit unit, out ExtensionDataReference reference) => referenceByUnit.TryGetValue(unit, out reference);
    public bool Has(GameObject gameObject) => referenceByGameObject.ContainsKey(gameObject);
    public bool Has(GameObject gameObject, out ExtensionDataReference reference) => referenceByGameObject.TryGetValue(gameObject, out reference);
    public ExtensionDataReference Generate(Unit unit, Action<Unit> removeHandle) => Generate(unit, removeHandle, default);
    public ExtensionDataReference Generate(Unit unit, Action<Unit> removeHandle, in ExtensionDataType extensionData = default) {
        int idx = dataCount;
        if(idx >= extensionDatas.Length) {
            int newLength = dataCount * 2;
            var newArray = ArrayPool<ExtensionDataType>.Shared.Rent(newLength);
            Array.Copy(extensionDatas, newArray, dataCount);
            ArrayPool<ExtensionDataType>.Shared.Return(extensionDatas);
            extensionDatas = newArray;

            var newRefArray = ArrayPool<ExtensionDataReference>.Shared.Rent(newLength);
            Array.Copy(extensionDataReferences, newRefArray, dataCount);
            ArrayPool<ExtensionDataReference>.Shared.Return(extensionDataReferences);
            extensionDataReferences = newRefArray;
        }

        var reference = new ExtensionDataReference(idx, unit, removeHandle);
        referenceByUnit[unit] = reference;
        referenceByGameObject[unit.gameObject] = reference;
        unitByReference[reference] = unit;

        extensionDatas[idx] = extensionData;
        extensionDataReferences[idx] = reference;
        dataCount = idx+1;
        return reference;
    }

    public void Delete(Unit unit) => Delete(referenceByUnit[unit]);

    public void Delete(ExtensionDataReference reference) {
        int idx = reference.idx;
        int lastIdx = dataCount-1;

        if(idx != lastIdx) {
            extensionDatas[idx] = extensionDatas[lastIdx];
            (extensionDataReferences[idx], extensionDataReferences[lastIdx]) = (extensionDataReferences[lastIdx], extensionDataReferences[idx]);
            extensionDataReferences[idx].idx = idx;
        }
        extensionDataReferences[lastIdx].idx = -1;

        referenceByUnit.Remove(reference.unit);
        referenceByGameObject.Remove(reference.gameObject);
        unitByReference.Remove(reference);

        --dataCount;

        int resizeLimit = extensionDatas.Length / 4;
        if(Math.Max(dataCount, minDataCount) < resizeLimit) {
            var newArray = ArrayPool<ExtensionDataType>.Shared.Rent(resizeLimit);
            Array.Copy(extensionDatas, newArray, dataCount);
            ArrayPool<ExtensionDataType>.Shared.Return(extensionDatas);
            extensionDatas = newArray;

            var newRefArray = ArrayPool<ExtensionDataReference>.Shared.Rent(resizeLimit);
            Array.Copy(extensionDataReferences, newRefArray, dataCount);
            ArrayPool<ExtensionDataReference>.Shared.Return(extensionDataReferences);
            extensionDataReferences = newRefArray;
        }
    }

    public Span<ExtensionDataType> AsSpan() {
        return new Span<ExtensionDataType>(extensionDatas, 0, dataCount);
    }
}

public interface IExtension<ExtensionData> {
    public abstract void Init(Unit unit, ref ExtensionData extensionData);
    public abstract void Update(ref ExtensionData extensionData);
    public abstract void Destroy(Unit unit);
}

public static class ExtensionHandler<ExtensionType, ExtensionDataType> where ExtensionType : IExtension<ExtensionDataType>, new() where ExtensionDataType : struct {
    public static ExtensionDataList<ExtensionDataType> ExtensionDataList = new();
    public static ExtensionDataReference AddExtension(Unit unit) {
        var reference = ExtensionDataList.Generate(unit, RemoveExtension);
        Instance.Init(unit, ref GetData(reference));
        return reference;
    }
    public static void RemoveExtension(Unit unit) {
        Instance.Destroy(unit);
        ExtensionDataList.Delete(unit);
    }
    public static ref ExtensionDataType GetData(ExtensionDataReference idxRef) => ref ExtensionDataList.Get(idxRef);
    public static ref ExtensionDataType GetData(Unit unit) => ref ExtensionDataList.Get(unit);
    public static ref ExtensionDataType GetData(GameObject gameObject) => ref ExtensionDataList.Get(gameObject);
    public static ref ExtensionDataType GetByIteratorIndex(int idx) => ref ExtensionDataList.GetByIteratorIndex(idx);
    public static bool Has(Unit unit) => ExtensionDataList.Has(unit);
    public static bool Has(Unit unit, out ExtensionDataReference reference) => ExtensionDataList.Has(unit, out reference);
    public static bool Has(GameObject gameObject) => ExtensionDataList.Has(gameObject);
    public static bool Has(GameObject gameObject, out ExtensionDataReference reference) => ExtensionDataList.Has(gameObject, out reference);
   

    private static ExtensionType staticInstance;
    public static ExtensionType Instance => staticInstance ??= new();
    public static void Update() {
        var instance = Instance;
        var dataReferences = ExtensionDataList.AsSpan();
        for(int i=0; i<dataReferences.Length; ++i) {
            ref var data = ref dataReferences[i];
            instance.Update(ref data);
        }
    }
}