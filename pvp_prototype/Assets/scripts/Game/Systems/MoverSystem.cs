public class MoverSystem : IExtensionSystem
{
    public void Update()
    {
        ExtensionHandler<MoverExtension, MoverExtensionData>.Update();
    }
}
