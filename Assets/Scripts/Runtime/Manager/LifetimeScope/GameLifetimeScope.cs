using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

//전체 설정
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private AudioMixer masterMixer;

    //Root LifetimeScope를 상속받아서 설정을 관리하기!
    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("<color=green>@@@ GameLifetimeScope: Configure 실행됨! @@@</color>");

        builder.RegisterEntryPoint<ActionMapChanger>().As<IMapChangable>().AsSelf();
        builder.RegisterEntryPoint<FungusDependencyResolver>().AsSelf();
        builder.RegisterEntryPoint<SettingManager>().WithParameter(masterMixer).AsSelf();
        builder.RegisterEntryPoint<ItemManagerHeavilyModified>().AsSelf();

        builder.RegisterComponent<PlayerInput>(playerInput);
    }
}
