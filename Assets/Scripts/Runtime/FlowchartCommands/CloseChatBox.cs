using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;



namespace Fungus
{
    [CommandInfo("InputAction", "CloseChatBox", "대화가 종료된 뒤 조작기능을 돌립니다.")]
    public class CloseChatBox : Command
    {
        IMapChangable input;


        [Inject]
        void Construct(ActionMapChanger inputManager)
        {
            input = inputManager;
        }

        public override void OnEnter()
        {
            if (input != null)
            {
                input.changeIAmapPrev();
            }
            else Debug.LogError("input is Null!");
                Continue();
        }


    }
}
