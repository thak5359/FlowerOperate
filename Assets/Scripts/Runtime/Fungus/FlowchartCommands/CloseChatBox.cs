using UnityEngine;
using VContainer;



namespace Fungus
{
    [CommandInfo("InputAction", "CloseChatBox", "대화가 종료된 뒤 조작기능을 돌립니다.")]
    public class CloseChatBox : Command
    {
        IMapChangable input;

        // 수정 위치: OnEnter 내부에서 의존성 확인 로직 추가
        public override void OnEnter()
        {
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
                input.changeIAmapPrev();
            }
            else
            {
                Debug.LogError("Fungus Command: ActionMapChanger를 찾을 수 없습니다!");
            }

            Continue();
        }


    }
}
