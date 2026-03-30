using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class TittleLifetimeScope : LifetimeScope
{

    [SerializeField] private SettingMenuManager smm;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();

        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<SettingMenuManager>(smm);
    }
}
