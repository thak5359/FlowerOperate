using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungus
{
    [CommandInfo("InputAction", "OpenChatBox", "대화가 나오는 도중 조작이 불가능하게 합니다.")]
    public class OpenChatBox : Command
    {
        public override void OnEnter()
        {
            IMapChangable input = IAmapManager.Instance;
            input.changeIAmapChatBox();
            
            Continue();
        }
    }
}