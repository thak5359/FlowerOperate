using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

//��ü ���
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private AudioMixer masterMixer;
    //Root LifetimeScope�� ����ؼ���� ������ ����ϱ�!
    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("<color=green>@@@ GameLifetimeScope: Configure 실행됨! @@@</color>");

        builder.RegisterEntryPoint<ActionMapChanger>().As<IMapChangable>().AsSelf();
        builder.RegisterEntryPoint<FungusDependencyResolver>().AsSelf();
        builder.RegisterEntryPoint<SettingManager>().WithParameter(masterMixer).AsSelf();

        builder.RegisterComponent<PlayerInput>(playerInput);
    }
}