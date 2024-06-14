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
    public Action<Unit, bool> destroyHandle;
    public ExtensionDataReference(int idx, Unit unit, Action<Unit, bool> destroyHandle) {
        this.idx = idx;
        this.unit = unit;
        gameObject = unit.gameObject;
        this.destroyHandle = destroyHandle;
    }
}

public class ExtensionList {
    private static readonly Dictionary<Unit, List<ExtensionDataReference>> unitExtensions = new();
    
    public static void RegisterExtension(Unit unit, ExtensionDataReference reference) {
        if(unitExtensions.TryGetValue(unit, out var extensionList)) {
            extensionList.Add(reference);
        } else {
            unitExtensions[unit] = new() {reference};
        }
    }

    public static void UnregisterExtension(Unit unit, ExtensionDataReference reference) {
        var list = unitExtensions[unit];
        int idx = list.IndexOf(reference);
        int lastIdx = list.Count - 1;
        (list[idx], list[lastIdx]) = (list[lastIdx], list[idx]);
        list.RemoveAt(lastIdx);
    }

    public static void DestroyAllExtensions(Unit unit) {
        if(TryGetExtensions(unit, out var references)) {
            foreach(var reference in references) {
                reference.destroyHandle(unit, true);   
            }
        }
        unitExtensions.Remove(unit);
    }

    public static bool TryGetExtensions(Unit unit, out List<ExtensionDataReference> extensions) => unitExtensions.TryGetValue(unit, out extensions);
}

public class ExtensionDataList<ExtensionDataType> where ExtensionDataType : struct
{
    private static ExtensionDataType[] extensionDatas = ArrayPool<ExtensionDataType>.Shared.Rent(minDataCount);
    private static ExtensionDataReference[] extensionDataReferences = ArrayPool<ExtensionDataReference>.Shared.Rent(minDataCount);
    private static IExtension<ExtensionDataType>[] classReferences = ArrayPool<IExtension<ExtensionDataType>>.Shared.Rent(minDataCount);
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
    public ExtensionDataReference GetReference(Unit unit) => referenceByUnit[unit];
    public IExtension<ExtensionDataType>GetClassInstance(ExtensionDataReference reference) => classReferences[reference.idx];
    public bool Has(Unit unit) => referenceByUnit.ContainsKey(unit);
    public bool Has(Unit unit, out ExtensionDataReference reference) => referenceByUnit.TryGetValue(unit, out reference);
    public bool Has(GameObject gameObject) => referenceByGameObject.ContainsKey(gameObject);
    public bool Has(GameObject gameObject, out ExtensionDataReference reference) => referenceByGameObject.TryGetValue(gameObject, out reference);
    public ExtensionDataReference Generate(Unit unit, Action<Unit, bool> destroyHandle, IExtension<ExtensionDataType> classReference) => Generate(unit, destroyHandle, classReference, default);
    public ExtensionDataReference Generate(Unit unit, Action<Unit, bool> destroyHandle, IExtension<ExtensionDataType> classReference, in ExtensionDataType extensionData = default) {
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

            var newClassArray = ArrayPool<IExtension<ExtensionDataType>>.Shared.Rent(newLength);
            Array.Copy(classReferences, newClassArray, dataCount);
            ArrayPool<IExtension<ExtensionDataType>>.Shared.Return(classReferences);
            classReferences = newClassArray;
        }

        var reference = new ExtensionDataReference(idx, unit, destroyHandle);
        referenceByUnit[unit] = reference;
        referenceByGameObject[unit.gameObject] = reference;
        unitByReference[reference] = unit;

        extensionDatas[idx] = extensionData;
        extensionDataReferences[idx] = reference;
        classReferences[idx] = classReference;
        dataCount = idx+1;
        return reference;
    }

    public ExtensionDataReference Delete(ExtensionDataReference reference) {
        int idx = reference.idx;
        int lastIdx = dataCount-1;

        if(idx != lastIdx) {
            extensionDatas[idx] = extensionDatas[lastIdx];
            classReferences[idx] = classReferences[lastIdx];
            (extensionDataReferences[idx], extensionDataReferences[lastIdx]) = (extensionDataReferences[lastIdx], extensionDataReferences[idx]);
            extensionDataReferences[idx].idx = idx;
        }
        reference.idx = -1;

        referenceByUnit.Remove(reference.unit);
        referenceByGameObject.Remove(reference.gameObject);
        unitByReference.Remove(reference);

        --dataCount;

        int resizeLimit = extensionDatas.Length / 4;
        if(Math.Max(dataCount, minDataCount) < resizeLimit) {
            int resizeTarget = resizeLimit * 2;
            var newArray = ArrayPool<ExtensionDataType>.Shared.Rent(resizeTarget);
            Array.Copy(extensionDatas, newArray, dataCount);
            ArrayPool<ExtensionDataType>.Shared.Return(extensionDatas);
            extensionDatas = newArray;

            var newRefArray = ArrayPool<ExtensionDataReference>.Shared.Rent(resizeTarget);
            Array.Copy(extensionDataReferences, newRefArray, dataCount);
            ArrayPool<ExtensionDataReference>.Shared.Return(extensionDataReferences);
            extensionDataReferences = newRefArray;

            var newClassArray = ArrayPool<IExtension<ExtensionDataType>>.Shared.Rent(resizeTarget);
            Array.Copy(classReferences, newClassArray, dataCount);
            ArrayPool<IExtension<ExtensionDataType>>.Shared.Return(classReferences);
            classReferences = newClassArray;
        }
        return reference;
    }

    public Span<ExtensionDataType> AsSpan() {
        return new Span<ExtensionDataType>(extensionDatas, 0, dataCount);
    }

    public Span<ExtensionDataReference> ReferencesAsSpan() {
        return new Span<ExtensionDataReference>(extensionDataReferences, 0, dataCount);
    }
}

public interface IExtension<ExtensionData> {
    public abstract void Init(Unit unit, ref ExtensionData extensionData);
    public abstract void Update(ref ExtensionData extensionData);
    public abstract void Destroy(Unit unit);
}

public class DerivedExtensionHandler<ExtensionType, ExtensionDataType, Parent> : ExtensionHandler<ExtensionType, ExtensionDataType> where ExtensionType : class, Parent, new() where ExtensionDataType : struct where Parent : IExtension<ExtensionDataType>, new() {}

public class ExtensionHandler<ExtensionType, ExtensionDataType> where ExtensionType : class, IExtension<ExtensionDataType>, new() where ExtensionDataType : struct {
    public static ExtensionDataList<ExtensionDataType> ExtensionDataList = new();
    public static ExtensionDataReference AddExtension(Unit unit) {
        var reference = ExtensionDataList.Generate(unit, DestroyExtension, staticInstance);
        ExtensionList.RegisterExtension(unit, reference);
        staticInstance.Init(unit, ref GetData(reference));
        return reference;
    }
    public static void DestroyExtension(Unit unit, bool skipUnregister = false) {
        var reference = ExtensionDataList.GetReference(unit);
        var instance = ExtensionDataList.GetClassInstance(reference);
        instance.Destroy(unit);
        if(!skipUnregister) {
            ExtensionList.UnregisterExtension(unit, reference);
        }
        ExtensionDataList.Delete(reference);
    }
    public static ref ExtensionDataType GetData(ExtensionDataReference idxRef) => ref ExtensionDataList.Get(idxRef);
    public static ref ExtensionDataType GetData(Unit unit) => ref ExtensionDataList.Get(unit);
    public static ref ExtensionDataType GetData(GameObject gameObject) => ref ExtensionDataList.Get(gameObject);
    public static ref ExtensionDataType GetByIteratorIndex(int idx) => ref ExtensionDataList.GetByIteratorIndex(idx);
    public static bool Has(Unit unit) => ExtensionDataList.Has(unit);
    public static bool Has(Unit unit, out ExtensionDataReference reference) => ExtensionDataList.Has(unit, out reference);
    public static bool Has(GameObject gameObject) => ExtensionDataList.Has(gameObject);
    public static bool Has(GameObject gameObject, out ExtensionDataReference reference) => ExtensionDataList.Has(gameObject, out reference);
   

    private static ExtensionType staticInstance = new();
    public static ExtensionType StaticInstance(Unit unit) {
        var reference = ExtensionDataList.GetReference(unit);
        return ExtensionDataList.GetClassInstance(reference) as ExtensionType;
    }

    public static void Update() {
        var extensionReferences = ExtensionDataList.ReferencesAsSpan();
        var datas = ExtensionDataList.AsSpan();
        for(int i=0; i<extensionReferences.Length; ++i) {
            var reference = extensionReferences[i];
            var instance = ExtensionDataList.GetClassInstance(reference);
            ref var data = ref datas[i];
            instance.Update(ref data);
        }
    }
}