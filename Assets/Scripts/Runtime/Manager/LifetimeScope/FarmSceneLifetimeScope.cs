using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private IngameSettingMenuManager pauseMenu;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ProgressManager progressManager;
    // [SerializeField] private //TODO 인벤토리 매니저 추가하기

    protected override void Configure(IContainerBuilder builder)
    {
        //KeyMapper랑 Changer는 씬 의존적인 스크립트 이기에 SceneLifetimeScope에 존재해야함.
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<UseAreamanager>().As<IUseItem>().AsSelf();

        //밑에 두 코드는 테스트용, 나중에 상점 씬 생기면 삭제해야함.
        builder.RegisterEntryPoint<ItemManagerHeavilyModified>().AsSelf();
        builder.RegisterEntryPoint<ProgressManager>().AsSelf();

        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<IngameSettingMenuManager>(pauseMenu);
        builder.RegisterComponent<InventoryManager>(inventoryManager).AsSelf();

    }
}
