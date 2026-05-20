using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

public class FarmSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private IngameSettingMenuController pauseMenu;
    [SerializeField] private InventoryUIController inventoryUI;
    [SerializeField] private PlayerOwnItemDataManager playerItemDataManager;
    [SerializeField] private PlotManager plotManager;


    protected override void Configure(IContainerBuilder builder)
    {
        //KeyMapper랑 Changer는 씬 의존적인 스크립트 이기에 SceneLifetimeScope에 존재해야함.
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<UseAreaManager>().As<IUseItem>().AsSelf();

        //밑에 두 코드는 테스트용, 나중에 상점 씬 생기면 삭제해야함.
        //builder.RegisterEntryPoint<ItemManagerHeavilyModified>().AsSelf();

        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();
        builder.Register<PlayerStateManager>(Lifetime.Singleton).AsSelf();
        builder.RegisterComponent<PlotManager>(plotManager).As<IPlotManager>().AsSelf();
        builder.RegisterComponent<PlayerController>(playerController);
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<IngameSettingMenuController>(pauseMenu);
        builder.RegisterComponent<InventoryUIController>(inventoryUI).AsSelf();
        builder.RegisterComponent<PlayerOwnItemDataManager>(playerItemDataManager).AsSelf();
        

        builder.RegisterComponentInHierarchy<SaveLoadManager>().AsSelf();
    }
}
