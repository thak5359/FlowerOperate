using VContainer;
using VContainer.Unity;

public class JBLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SaveLoadManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<StorageDataManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<StorageManager>()
            .As<StorageDataManager>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<PlotManager>()
            .As<StorageDataManager>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<ProgressManager>()
            .AsSelf();
        builder.RegisterComponentInHierarchy<HotbarManager>().AsSelf();
    }
}
