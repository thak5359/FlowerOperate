using UnityEngine;
using Fungus;
using System.Collections.Generic;

namespace Fungus
{
    [CommandInfo("Visual", "Change BG (Custom)", "BGImageController를 통해 배경을 교체합니다.")]
    public class ChangeBG : Command
    {
        [Tooltip("배경 컨트롤러를 지정하세요.")]
        [SerializeField] protected BGImageController targetController;

        [Tooltip("활성화할 이미지의 이름입니다.")]
        [SerializeField] protected string targetImageName;

        public override void OnEnter()
        {
            if (targetController != null && !string.IsNullOrEmpty(targetImageName))
            {
                targetController.ChangeBackground(targetImageName);
            }
            Continue();
        }

        public override string GetSummary()
        {
            return string.IsNullOrEmpty(targetImageName) ? "None" : targetImageName;
        }

        public override Color GetButtonColor() => new Color32(173, 216, 230, 255);
    }
}