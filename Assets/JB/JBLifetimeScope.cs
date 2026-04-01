using VContainer;
using VContainer.Unity;

public class JBLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SaveLoadManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<InventoryManager>()
            .As<ItemStorageParent>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<StorageManager>()
            .As<ItemStorageParent>()
            .AsSelf();
    }
}
