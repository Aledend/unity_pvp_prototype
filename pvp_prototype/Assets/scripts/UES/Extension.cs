using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtensionInitContext {
    public bool isAuthor;
    public bool isHusk;
}

public class ExtensionMetadata
{
    public Unit unit;
    public Action<ExtensionMetadata> destroyHandle;
    public RefArrayItemReference mainDataReference;
    public IExtension instance;
}

public class ExtensionList {
    private static readonly Dictionary<Unit, List<ExtensionMetadata>> unitExtensions = new();
    
    public static void RegisterExtension(Unit unit, ExtensionMetadata reference) {
        if(unitExtensions.TryGetValue(unit, out var extensionList)) {
            extensionList.Add(reference);
        } else {
            unitExtensions[unit] = new() {reference};
        }
    }

    public static void UnregisterExtension(Unit unit, ExtensionMetadata reference) {
        var list = unitExtensions[unit];
        int idx = list.IndexOf(reference);
        int lastIdx = list.Count - 1;
        (list[idx], list[lastIdx]) = (list[lastIdx], list[idx]);
        list.RemoveAt(lastIdx);
        if(unitExtensions.Count == 0) {
            unitExtensions.Remove(unit);
        }
    }

    public static void DestroyAllExtensions(Unit unit) {
        if(TryGetExtensions(unit, out var references)) {
            int index = references.Count-1;
            do {
                var reference = references[index];
                reference.destroyHandle(reference);
                --index;
            } while(index >= 0);
        }
    }

    public static bool TryGetExtensions(Unit unit, out List<ExtensionMetadata> extensions) => unitExtensions.TryGetValue(unit, out extensions);
}

public class ExtensionDataList<ExtensionDataType> where ExtensionDataType : struct
{
    private static readonly RefArray<ExtensionDataType, Unit, GameObject> extensionDataReferences = new(4);

    public ref ExtensionDataType Get(RefArrayItemReference idxRef) => ref extensionDataReferences[idxRef as RefArray<ExtensionDataType, Unit, GameObject>.Ref];
    public ref ExtensionDataType Get(Unit unit) => ref extensionDataReferences[unit];
    public void Get(RefArrayItemReference reference, out Unit unit) => extensionDataReferences.Get(reference, out unit);
    public void Get(RefArrayItemReference reference, out GameObject gameObject) => extensionDataReferences.Get(reference, out gameObject);
    public RefArrayItemReference GetReference(Unit unit) => extensionDataReferences.GetReference(unit);
    public RefArrayItemReference Generate(Unit unit) => Generate(unit, default);
    public RefArrayItemReference Generate(Unit unit, in ExtensionDataType extensionData = default) => extensionDataReferences.Add(unit, unit.gameObject, extensionData);

    public void Delete(Unit unit) => extensionDataReferences.Remove(unit);
    public void Delete(RefArrayItemReference reference) => extensionDataReferences.Remove(reference as RefArray<ExtensionDataType, Unit, GameObject>.Ref);

    public Span<ExtensionDataType> AsSpan() {
        return extensionDataReferences.AsSpan();
    }

    public Span<RefArrayItemReference> ReferencesAsSpan() {
        return extensionDataReferences.ReferencesAsSpan();
    }
}

public interface IExtension {}
public abstract class Extension<ExtensionData> : IExtension where ExtensionData : struct
{
    public ref ExtensionData GetData<ExtensionType>(Unit unit) where ExtensionType : Extension<ExtensionData>, new() => ref ExtensionHandler<ExtensionType, ExtensionData>.GetData(unit);
    public ref ExtraExtensionData GetExtraData<ExtraExtensionData>(Unit unit) where ExtraExtensionData : struct => ref ExtraDataHandler<ExtraExtensionData>.GetData(unit);
    public RefArrayItemReference RentExtraData<ExtraExtensionData>(Unit unit) where ExtraExtensionData : struct => ExtraDataHandler<ExtraExtensionData>.Rent(unit);
    public void ReturnExtraData<ExtraExtensionData>(Unit unit) where ExtraExtensionData : struct => ExtraDataHandler<ExtraExtensionData>.Return(unit);
    public abstract void Destroy(Unit unit);

    public abstract void Init(Unit unit, ExtensionInitContext extensionInitContext, ref ExtensionData extensionData);

    public abstract void Update(ref ExtensionData extensionData);
}

public class ExtraDataHandler<ExtensionDataType> where ExtensionDataType : struct {
    private static readonly ExtensionDataList<ExtensionDataType> extensionDataList = new();
    public static RefArrayItemReference Rent(Unit unit) => extensionDataList.Generate(unit);
    public static void Return(Unit unit) => extensionDataList.Delete(unit);
    public static ref ExtensionDataType GetData(Unit unit) => ref extensionDataList.Get(unit);
}

public class ExtensionHandlerInheritanceLayer<ExtensionDataType> {
    protected static readonly LookupArray<ExtensionMetadata, Unit, GameObject> extensionMetadataList = new();
}

public class ExtensionHandler<ExtensionType, ExtensionDataType> : ExtensionHandlerInheritanceLayer<ExtensionDataType> where ExtensionType : Extension<ExtensionDataType>, new() where ExtensionDataType : struct {
    private static readonly ExtensionDataList<ExtensionDataType> extensionDataList = new();
    public static ExtensionMetadata AddExtension(Unit unit, ExtensionInitContext extensionInitContext) {
        if(Has(unit, out ExtensionMetadata extensionMetadata))
            return extensionMetadata;
        
        RefArrayItemReference dataReference = extensionDataList.Generate(unit);

        extensionMetadata = extensionMetadataList.Add(unit, unit.gameObject);
        extensionMetadata.unit = unit;
        extensionMetadata.destroyHandle = DestroyExtension;
        extensionMetadata.mainDataReference = dataReference;
        extensionMetadata.instance = staticInstance;

        ExtensionList.RegisterExtension(unit, extensionMetadata);
        
        staticInstance.Init(unit, extensionInitContext, ref GetData(dataReference));
        return extensionMetadata;
    }

    public static void DestroyExtension(ExtensionMetadata extensionMetadata) {
        Unit unit = extensionMetadata.unit;
        (extensionMetadata.instance as ExtensionType).Destroy(unit);
        ExtensionList.UnregisterExtension(unit, extensionMetadata);
        extensionDataList.Delete(extensionMetadata.mainDataReference);
        extensionMetadataList.SwapDelete(extensionMetadata);
    }

    public static ref ExtensionDataType GetData(RefArrayItemReference reference) => ref extensionDataList.Get(reference);
    public static ref ExtensionDataType GetData(Unit unit) => ref extensionDataList.Get(unit);
    public static bool Has(Unit unit) => extensionMetadataList.Has(unit);
    public static bool Has(Unit unit, out ExtensionMetadata reference) => extensionMetadataList.Has(unit, out reference);
    public static bool Has(GameObject gameObject) => extensionMetadataList.Has(gameObject);
    public static bool Has(GameObject gameObject, out ExtensionMetadata reference) => extensionMetadataList.Has(gameObject, out reference);
    public static void Get(RefArrayItemReference reference, out Unit unit) => extensionDataList.Get(reference, out unit);
    public static void Get(RefArrayItemReference reference, out GameObject gameObject) => extensionDataList.Get(reference, out gameObject);
    public static Span<RefArrayItemReference> References => extensionDataList.ReferencesAsSpan();

    private static readonly ExtensionType staticInstance = new();
    public static ExtensionType StaticInstance(Unit unit) {
        var metadata = extensionMetadataList.Get(unit);
        return metadata.instance as ExtensionType;
    }

    public static void Update() {
        var extensionReferences = extensionDataList.ReferencesAsSpan();
        var datas = extensionDataList.AsSpan();
        for(int i=0; i<extensionReferences.Length; ++i) {
            var reference = extensionReferences[i];
            Get(reference, out Unit unit);
            var instance = extensionMetadataList.Get(unit).instance as ExtensionType;
            ref var data = ref datas[i];
            instance.Update(ref data);
        }
    }
}