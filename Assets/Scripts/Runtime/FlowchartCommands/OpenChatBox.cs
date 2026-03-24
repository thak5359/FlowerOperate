using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

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
           
            input.changeIAmapChatBox();
            
            Continue();
        }
    }
}