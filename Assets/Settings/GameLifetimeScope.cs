using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEditor.U2D.Sprites;
using UnityEngine.InputSystem;

public class GameLifetimeScope : LifetimeScope
{

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private PauseMenu pauseMenu;
    

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ActionMapChanger>().As< IMapChangable>().AsSelf();
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<PlayerInput>(playerInput);
        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<PauseMenu>(pauseMenu);
    }
}
