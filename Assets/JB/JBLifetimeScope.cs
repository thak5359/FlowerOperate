using VContainer;
using VContainer.Unity;

public class JBLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SaveLoadManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<PlayerOwnItemDataManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<PlotManager>()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterComponentInHierarchy<HotbarManager>().AsSelf();
    }
}
