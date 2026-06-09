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
    [SerializeField] private PlotManager plotManager;
    [SerializeField] private SaveLoadManager saveLoadManager;
    [SerializeField] private ShopUIController shopUIController;
    [SerializeField] private InfoUIController infoUIController;
    [SerializeField] private ItemGenTest itemGenTest;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ActionKeyMapper>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<UseAreaManager>().As<IUseItem>().AsSelf();


        builder.Register<ActionKeyChanger>(Lifetime.Singleton).AsSelf();
        builder.Register<PlayerStateManager>(Lifetime.Singleton).AsSelf();
        builder.RegisterComponent<PlotManager>(plotManager).AsImplementedInterfaces().AsSelf();
        builder.RegisterComponent<PlayerController>(playerController).AsSelf();
        builder.RegisterComponent<HotbarManager>(hotbarManager);
        builder.RegisterComponent<IngameSettingMenuController>(pauseMenu);
        builder.RegisterComponent<InventoryUIController>(inventoryUI).AsSelf();
        builder.RegisterComponent<ShopUIController>(shopUIController).AsSelf();
        builder.RegisterComponent<InfoUIController>(infoUIController).AsSelf();
        builder.RegisterComponent<ItemGenTest>(itemGenTest).AsSelf();

        // ChunkManager 씬 컴포넌트 자동 주입 등록
        builder.RegisterComponentInHierarchy<ChunkManager>().AsSelf();
    }
}
