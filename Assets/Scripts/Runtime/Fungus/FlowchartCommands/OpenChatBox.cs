using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Fungus
{
    [CommandInfo("InputAction", "OpenChatBox", "대화 중 조작을 불가능하게 합니다.")]
    public class OpenChatBox : Command
    {
        private IMapChangable _input;

        public override void OnEnter()
        {
            // 1. 캐싱된 값이 없다면 현재 씬의 컨테이너에서 찾아옵니다.
            if (_input == null)
            {
                // 현재 활성화된 LifetimeScope를 찾습니다.
                var scope = LifetimeScope.Find<LifetimeScope>();

                if (scope != null && scope.Container != null)
                {
                    // [핵심] 인터페이스로 Resolve 하여 유연성을 높입니다.
                    _input = scope.Container.Resolve<IMapChangable>();
                }
            }

            // 2. 안전하게 명령을 실행합니다.
            if (_input != null)
            {
                _input.changeIAmapChatBox();
                Debug.Log("<color=green>[Fungus]</color> ChatBox Action Map으로 전환 성공!");
            }
            else
            {
                Debug.LogError("<color=red>[Fungus Error]</color> IMapChangable 의존성을 찾을 수 없습니다! LifetimeScope 등록을 확인하세요.");
            }

            Continue();
        }
    }
}