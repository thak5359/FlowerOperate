using VContainer;
using VContainer.Unity;

namespace MyGame
{
    public class LifeTImeScopeScript : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IAmapManager>(Lifetime.Singleton);
        }
    }
}