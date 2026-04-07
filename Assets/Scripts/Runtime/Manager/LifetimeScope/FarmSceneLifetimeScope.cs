using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private IngameSettingMenuManager pauseMenu;

    protected override void Configure(IContainerBuilder builder)
    {
        //KeyMapper랑 Changer는 씬 의존적인 스크립트 이기에 SceneLifetimeScope에 존재해야함.
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<UseAreamanager>().As<IUseItem>().AsSelf();

        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();


        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<IngameSettingMenuManager>(pauseMenu);
    }
}
