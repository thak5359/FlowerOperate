using UnityEngine;
using Fungus;
using System.Collections;

[CommandInfo("Portrait",
             "Portrait Anim",
             "지정된 캐릭터의 Holder를 찾아 애니메이션을 실행하며, 즉시 다음 명령어로 넘어갑니다.")]
[AddComponentMenu("")]
public class PortraitAnim : Command
{
    [Tooltip("움직일 캐릭터를 선택하세요.")]
    public Character character;

    public enum AnimType { Nod, Surprise,Jump, JumpJump, Panic, Fidget }
    [Tooltip("애니메이션 종류를 선택하세요.")]
    public AnimType animType = AnimType.Nod;


    public override void OnEnter()
    {
        if (character != null)
        {
            GameObject holder = GameObject.Find(character.name + " holder");
            if (holder != null)
            {
                // 코루틴은 시작만 하고
                StartCoroutine(ApplyiTweenCoroutine(holder));
            }
        }

        // 코루틴이 끝나길 기다리지 않고 즉시 다음 명령어로 진행합니다.
        Continue();
    }

    private IEnumerator ApplyiTweenCoroutine(GameObject target)
    {
        switch (animType)
        {
            // 아래로 내려갔다 올라옴
            case AnimType.Nod:
                float nodAmount = -50f;
                float halfTime = 0.5f;

                // 1. 내려가기
                iTween.MoveBy(target, iTween.Hash(
                    "y", nodAmount,
                    "time", halfTime,
                    "easeType", iTween.EaseType.easeInOutQuad,
                    "islocal", true
                ));

                yield return new WaitForSeconds(halfTime);

                // 2. 올라오기
                iTween.MoveBy(target, iTween.Hash(
                    "y", -nodAmount,
                    "time", halfTime,
                    "easeType", iTween.EaseType.easeInOutQuad,
                    "islocal", true
                ));

                // 여기서는 더 이상 Continue()를 호출하지 않습니다.
                break;
            case AnimType.Jump:
                float jumpHeight1 = 50f; 
                float stepTime1 = 0.2f;

                    iTween.MoveBy(target, iTween.Hash(
                        "y", jumpHeight1,
                        "time", stepTime1,
                        "easeType", iTween.EaseType.easeOutQuad,
                        "islocal", true
                    ));
                    yield return new WaitForSeconds(stepTime1);

                    iTween.MoveBy(target, iTween.Hash(
                        "y", -jumpHeight1,
                        "time", stepTime1,
                        "easeType", iTween.EaseType.easeInQuad,
                        "islocal", true
                    ));
                    yield return new WaitForSeconds(stepTime1);
                break;

            // 두번 점프!
            case AnimType.JumpJump:
                float jumpHeight2 = 50f; // 540 카메라 사이즈 기준 적당한 점프 높이
                float stepTime2 = 0.25f; // 총 1초를 4단계(위-아래-위-아래)로 나눔

                for (int i = 0; i < 2; i++)
                {
                    // 1. 위로 점프 (상승 시에는 easeOutQuad가 자연스럽습니다)
                    iTween.MoveBy(target, iTween.Hash(
                        "y", jumpHeight2,
                        "time", stepTime2,
                        "easeType", iTween.EaseType.easeOutQuad,
                        "islocal", true
                    ));
                    yield return new WaitForSeconds(stepTime2);

                    // 2. 아래로 착지 (하강 시에는 중력 느낌을 위해 easeInQuad 사용)
                    iTween.MoveBy(target, iTween.Hash(
                        "y", -jumpHeight2,
                        "time", stepTime2,
                        "easeType", iTween.EaseType.easeInQuad,
                        "islocal", true
                    ));
                    yield return new WaitForSeconds(stepTime2);
                }
                break;

            case AnimType.Panic:
                // X축(좌우)으로만 격하게 흔들기
                // 540 카메라 사이즈 기준, 30f 정도면 꽤 격렬한 떨림입니다.
                iTween.ShakePosition(target, iTween.Hash(
                    "x", 10f,
                    "time", 1.0f,
                    "islocal", false
                ));

                // ShakePosition은 스스로 지속시간을 가지므로, 
                // 애니메이션 도중 다른 로직이 겹치지 않게 대기만 해줍니다.
                yield return new WaitForSeconds(1.0f);
                break;

            case AnimType.Surprise:
                // 1. 위로 툭! (Punch 활용으로 반동을 줌)
                iTween.PunchPosition(target, iTween.Hash(
                    "y", 60f,
                    "time", 0.5f,
                    "islocal", false
                ));
                // 2. 동시에 살짝 뒤로 밀렸다가 돌아오는 느낌을 위해 약간의 대기
                yield return new WaitForSeconds(0.5f);
                break;

            case AnimType.Fidget:
                float moveDist = 5f; // 좌우로 움직일 폭
                float moveTime = 0.03f; // 각 움직임의 시간

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    // 1. 좌로 이동
                    {
                        iTween.MoveBy(target, iTween.Hash("x", -moveDist, "time", moveTime, "easeType", iTween.EaseType.easeInOutQuad, "islocal", false));
                        yield return new WaitForSeconds(moveTime);

                        // 2. 우로 이동 (좌측 끝에서 우측 끝으로 가야 하므로 거리를 두 배로)
                        iTween.MoveBy(target, iTween.Hash("x", moveDist * 2f, "time", moveTime * 2f, "easeType", iTween.EaseType.easeInOutQuad, "islocal", false));
                        yield return new WaitForSeconds(moveTime * 2f);
                    }

                    // 3. 다시 원점으로 복귀
                    iTween.MoveBy(target, iTween.Hash("x", -moveDist, "time", moveTime, "easeType", iTween.EaseType.easeInOutQuad, "islocal", false));
                    yield return new WaitForSeconds(moveTime);

                    // 4. 중간 텀 (0.2초) - 두 번째 반복 전이나 마지막에 잠깐 멈춤
                    yield return new WaitForSeconds(0.2f);
                }
                break;
        }
    }


    public override Color GetButtonColor()
    {
        return new Color32(230, 200, 250, 255);
    }
}