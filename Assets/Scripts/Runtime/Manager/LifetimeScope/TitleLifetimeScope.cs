using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class TittleLifetimeScope : LifetimeScope
{
    [SerializeField] private TitleMenuController tmm;
    [SerializeField] private TitleSettingMenuController tsmm;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<TitleMenuController>(tmm);
        builder.RegisterComponent<TitleSettingMenuController>(tsmm);
    }
}
