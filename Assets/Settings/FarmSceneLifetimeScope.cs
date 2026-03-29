using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmSceneLifetimeScope : LifetimeScope
{

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private PauseMenu pauseMenu;
    

    protected override void Configure(IContainerBuilder builder)
    {
        //Don'tDestory로 별도로 분리할 것들

        builder.RegisterEntryPoint<ActionMapChanger>().As<IMapChangable>().AsSelf();
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<FungusDependencyResolver>().AsSelf();
        builder.Register<ItemManager>(Lifetime.Singleton).AsSelf();

        //농장에서만 쓰이는 놈들
        builder.RegisterEntryPoint<UseAreamanager>().As<IUseItem>().AsSelf();

        builder.RegisterComponent<PlayerInput>(playerInput);
        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<PauseMenu>(pauseMenu);
    }
}
