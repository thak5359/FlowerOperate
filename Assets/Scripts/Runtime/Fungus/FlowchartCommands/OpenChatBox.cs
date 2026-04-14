using UnityEngine;
using VContainer;

namespace Fungus
{
    [CommandInfo("InputAction", "OpenChatBox", "대화가 나오는 도중 조작이 불가능하게 합니다.")]
    public class OpenChatBox : Command
    {
        IMapChangable input;
        // 수정 위치: [Inject] 대신 런타임에 직접 Resolve 하도록 변경
        public override void OnEnter()
        {
            // 현재 씬의 LifetimeScope에서 직접 의존성을 찾아옵니다.
            if (input == null)
            {
                var scope = VContainer.Unity.LifetimeScope.Find<VContainer.Unity.LifetimeScope>();
                if (scope != null)
                {
                    input = scope.Container.Resolve<ActionMapChanger>();
                }
            }

            if (input != null)
            {
                input.changeIAmapChatBox();
            }
            else
            {
                Debug.LogError("Fungus Command: ActionMapChanger를 찾을 수 없습니다!");
            }

            Continue();
        }
    }
}