using UnityEngine;
using Fungus;

namespace Fungus
{
    [CommandInfo("Narrative", "Say Layered (Custom)", "대사와 함께 캐릭터의 표정을 변경합니다.")]
    public class SayLayered : Say
    {
        // 데이터는 string으로 저장되지만, 인스펙터에서는 콤보박스로 보일 겁니다.
        [Tooltip("교체할 표정의 이름")]
        public string facePortraitName;

        public override void OnEnter()
        {
            LayeredCharacter layeredChar = _Character as LayeredCharacter;
            if (layeredChar != null)
            {
                // 표정 이름만 전달하면 내부에서 피벗을 계산하여 적용합니다.
                layeredChar.ApplyFace(facePortraitName);
            }
            base.OnEnter();
        }

        public override string GetSummary()
        {
            string displayFace = string.IsNullOrEmpty(facePortraitName) ? "이전 표정 유지" : facePortraitName;
            return $"[{displayFace}] {base.GetSummary()}";
        }
    }
}