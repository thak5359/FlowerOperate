using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private IngameSettingMenuController pauseMenu;
    [SerializeField] private InventoryUIController inventoryUI;
    [SerializeField] private PlayerItemDataManager playerItemDataManager;
    // [SerializeField] private //TODO 인벤토리 매니저 추가하기

    protected override void Configure(IContainerBuilder builder)
    {
        //KeyMapper랑 Changer는 씬 의존적인 스크립트 이기에 SceneLifetimeScope에 존재해야함.
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<UseAreaManager>().As<IUseItem>().AsSelf();

        //밑에 두 코드는 테스트용, 나중에 상점 씬 생기면 삭제해야함.
        //builder.RegisterEntryPoint<ItemManagerHeavilyModified>().AsSelf();

        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();
        builder.Register<ProgressManager>(Lifetime.Singleton).AsSelf();  

        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<IngameSettingMenuController>(pauseMenu);
        builder.RegisterComponent<InventoryUIController>(inventoryUI).AsSelf();
        builder.RegisterComponent<PlayerItemDataManager>(playerItemDataManager).AsSelf();
    }
}
