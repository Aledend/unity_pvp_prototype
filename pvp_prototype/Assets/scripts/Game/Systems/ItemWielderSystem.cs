public class ItemWielderSystem : IExtensionSystem
{
    public void Update()
    {
        ExtensionHandler<ItemWielderExtension, ItemWielderData>.Update();
    }
}
