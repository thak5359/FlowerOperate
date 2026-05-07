using VContainer;
using VContainer.Unity;

public class JBLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SaveLoadManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<PlayerItemDataManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<StorageManager>()
            .As<PlayerItemDataManager>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<PlotManager>()
            .As<PlayerItemDataManager>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<ProgressManager>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<HotbarManager>().AsSelf();
    }
}
