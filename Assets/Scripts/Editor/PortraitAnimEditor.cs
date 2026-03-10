using UnityEditor;
using Fungus.EditorUtils;
using UnityEngine;

[CustomEditor(typeof(PortraitAnim))]
public class PortraitAnimEditor : CommandEditor
{
    public override void DrawCommandGUI()
    {
        base.DrawCommandGUI(); // 기본 인스펙터 요소(Character, AnimType)를 먼저 그립니다.

        PortraitAnim t = target as PortraitAnim;

        // 선택된 애니메이션에 따라 다른 설명을 보여줍니다.
        string helpText = "";
        MessageType messageType = MessageType.Info;

        switch (t.animType)
        {
            case PortraitAnim.AnimType.Nod:
                helpText = "▶ Nod (끄덕임)\n- 효과: 1.0초 동안 아래로 -30px 이동 후 복귀.\n- 특징: 반동 없이 부드럽게(easeInOut) 정지합니다.";
                break;
            case PortraitAnim.AnimType.JumpJump:
                helpText = "▶ JumpJump (두 번 점프)\n- 효과: 1.0초 동안 50px 높이로 두 번 점프.\n- 특징: 상승(easeOut)과 하강(easeIn)의 속도차로 중력감을 줍니다.";
                break;
            case PortraitAnim.AnimType.Panic:
                helpText = "▶ Panic (격한 흔들림)\n- 효과: 1.0초 동안 좌우(X축)로 20px 바르르 떰.\n- 특징: 월드 좌표 기준(isLocal: false)으로 작동합니다.";
                break;
            case PortraitAnim.AnimType.Surprise:
                helpText = "▶ Surprise (깜짝 놀람)\n- 효과: 0.5초 동안 위로 60px 툭 튀어 오름.\n- 특징: Punch 효과를 사용하여 반동과 함께 제자리로 돌아옵니다.";
                break;
            case PortraitAnim.AnimType.Fidget:
                helpText = "▶ Fidget (꼼지락거림)\n- 효과: 1.5초 동안 약간의 흔들림과 미세한 이동을 반복.\n- 특징: 흔들림과 이동이 동시에 일어나 자연스러운 움직임을 연출합니다.";
                break;
        }

        EditorGUILayout.Space();
        // 수정 불가능한 HelpBox 형태로 설명을 출력합니다.
        EditorGUILayout.HelpBox(helpText, messageType);
    }
}