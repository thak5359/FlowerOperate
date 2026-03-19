using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Fungus
{
    [CommandInfo("InputAction", "CloseChatBox", "대화가 종료된 뒤 조작기능을 돌립니다.")]
    public class CloseChatBox : Command
    {
        public override void OnEnter()
        {
            IMapChangable input = IAmapManager.Instance();
            input.changeIAmapPrev();

            Continue();
        }
    }
}
