using System.Collections.Generic;

using ItemSlotReference = RefArrayItemReference;
public struct ItemSlot {
    public Unit itemUnit;
    public bool wielded;
    public bool supportsMultiWield;
}

public struct ItemWielderData {
    public RefArray<ItemSlot> slots;
    public List<ItemSlotReference> wieldedSlots;
}

public class ItemWielderExtension : Extension<ItemWielderData>
{
    public override void Init(Unit unit, ExtensionInitContext extensionInitContext, ref ItemWielderData extensionData)
    {
        extensionData.slots = new(0, RefArrayIncreaseType.Incremental);
    }

    public override void Update(ref ItemWielderData extensionData) {}
    public override void Destroy(Unit unit) {}

    public ItemSlotReference AddSlot(Unit unit, ItemDefinition itemDefinition) {
        ref var data = ref ExtensionHandler<ItemWielderExtension, ItemWielderData>.GetData(unit);
        return data.slots.Add(CreateSlotFromItem(unit, itemDefinition));
    }

    public ref ItemSlot GetSlot(Unit unit, ItemSlotReference reference) {
        ref var data = ref ExtensionHandler<ItemWielderExtension, ItemWielderData>.GetData(unit);
        return ref data.slots[reference];
    }

    public void WieldSlot(Unit unit, ItemSlotReference slotReference) {
        ref var data = ref ExtensionHandler<ItemWielderExtension, ItemWielderData>.GetData(unit);
        ref var slotData = ref data.slots[slotReference];
        if(!slotData.supportsMultiWield)
            UnwieldAllSlots(unit);

        slotData.wielded = true;
    }

    public void UnwieldSlot(Unit unit, ItemSlotReference slotReference) {
        ref var data = ref ExtensionHandler<ItemWielderExtension, ItemWielderData>.GetData(unit);
        data.wieldedSlots.Remove(slotReference);
        data.slots[slotReference].wielded = false;
    }

    public void UnwieldAllSlots(Unit unit) {
        ref var data = ref ExtensionHandler<ItemWielderExtension, ItemWielderData>.GetData(unit);
        var wieldedSlots = data.wieldedSlots;
        for(int i=wieldedSlots.Count-1; i >= 0; --i) {
            UnwieldSlot(unit, wieldedSlots[i]);
        }
    }

    private ItemSlot CreateSlotFromItem(Unit ownerUnit, ItemDefinition itemDefinition) {
        ItemSlot slot = new ItemSlot
        {
            supportsMultiWield = itemDefinition.supportsMultiWield,
            itemUnit = Managers.unitSpawner.SpawnUnit(itemDefinition.itemSpawnTemplate),
            wielded = false,
        };

        ownerUnit.LinkChild(slot.itemUnit, NodeName.ItemHeldCloseRight);

        return slot;
    }

    public List<ItemSlotReference> WieldedSlots(Unit unit) => ExtensionHandler<ItemWielderExtension, ItemWielderData>.GetData(unit).wieldedSlots;
}

public class HuskItemWielderExtension : ItemWielderExtension {}