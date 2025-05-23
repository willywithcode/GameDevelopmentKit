namespace GameFoundation.Scripts.Features.Inventory.DI
{
    using GameFoundation.Scripts.Features.Inventory.Services;
    using VContainer;

    public static class InventoryVContainer
    {
        public static void RegisterInventory(this IContainerBuilder builder)
        {
            builder.Register<InventoryService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}