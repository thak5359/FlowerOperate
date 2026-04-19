using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private IngameSettingMenuController pauseMenu;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ProgressManager progressManager;
    [SerializeField] private InventoryUIController inventoryUI;
    // [SerializeField] private //TODO 인벤토리 매니저 추가하기

    protected override void Configure(IContainerBuilder builder)
    {
        //KeyMapper랑 Changer는 씬 의존적인 스크립트 이기에 SceneLifetimeScope에 존재해야함.
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<UseAreaManager>().As<IUseItem>().AsSelf();
        builder.RegisterEntryPoint<ItemManagerHeavilyModified>().AsSelf();
        builder.RegisterEntryPoint<ProgressManager>().AsSelf();
        //builder.RegisterEntryPoint<PlotManager>().AsImplementedInterfaces().AsSelf();

        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<IngameSettingMenuController>(pauseMenu);
        builder.RegisterComponent<InventoryManager>(inventoryManager).AsSelf();
        builder.RegisterComponent<InventoryUIController>(inventoryUI).AsSelf();
    }
}
