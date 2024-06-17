public class ControllerSystem : IExtensionSystem
{
    public void Update()
    {
        ExtensionHandler<PlayerControllerExtension, PlayerControllerData>.Update();
    }
}
