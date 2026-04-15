using UnityEngine;
using VContainer;
namespace Fungus
{
    [CommandInfo("InputAction", "OpenChatBox", "대화가 나오는 도중 조작이 불가능하게 합니다.")]
    public class OpenChatBox : Command
    {
        // VContainer가 자동으로 주입해줍니다.
        [Inject] private IMapChangable _input;

        public override void OnEnter()
        {
            if (_input != null)
            {
                _input.changeIAmapChatBox();
            }
            else
            {
                Debug.LogError("OpenChatBox: IMapChangable 주입에 실패했습니다!");
            }

            Continue();
        }
    }
}