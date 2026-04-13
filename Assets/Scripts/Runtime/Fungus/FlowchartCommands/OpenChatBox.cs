using UnityEngine;
using VContainer;

namespace Fungus
{
    [CommandInfo("InputAction", "OpenChatBox", "대화가 나오는 도중 조작이 불가능하게 합니다.")]
    public class OpenChatBox : Command
    {
        IMapChangable input;


       [Inject]
        void Construction(ActionMapChanger inputManager)
        {
             input = inputManager;
        }

        public override void OnEnter()
        {
            if (input != null)
                input.changeIAmapChatBox();
            else Debug.LogAssertion("input 없음!");
                Continue();
        }
    }
}