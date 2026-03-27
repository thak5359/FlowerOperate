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
        // TestActionMapChanger
        builder.RegisterEntryPoint<ActionMapChanger>().As< IMapChangable>().AsSelf();
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<FungusDependencyResolver>().AsSelf();


        builder.RegisterComponent<PlayerInput>(playerInput);
        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<PauseMenu>(pauseMenu);

        
    }
}
