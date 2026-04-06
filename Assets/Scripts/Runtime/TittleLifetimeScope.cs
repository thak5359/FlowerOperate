using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class TittleLifetimeScope : LifetimeScope
{

    [SerializeField] private SettingMenuManager smm;
    [SerializeField] private TitleMenuManager tmm;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<TitleMenuManager>(tmm);
        builder.RegisterComponent<SettingMenuManager>(smm);
    }
}
