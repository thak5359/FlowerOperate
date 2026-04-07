using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

//전체 사용
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private  AudioMixer masterMixer;
    //Root LifetimeScope를 사용해서모든 씬에서 사용하기!
    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("<color=green>@@@ GameLifetimeScope: Configure 시작됨! @@@</color>");

        builder.RegisterEntryPoint<ActionMapChanger>().As<IMapChangable>().AsSelf();
        builder.RegisterEntryPoint<FungusDependencyResolver>().AsSelf();
        builder.RegisterEntryPoint<SettingManager>().WithParameter(masterMixer).AsSelf();
        builder.Register<ItemManager>(Lifetime.Singleton).AsSelf();

        builder.RegisterComponent<PlayerInput>(playerInput);
    }
}