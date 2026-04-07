using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EmoteType { Angry, Chat, Dot, Exclaim, Heart, Laugh, Music, Twinkle, Surprise, Sweat, Shy, Question, Upset }

[CommandInfo("Custom", "Play Emote Preset", "블루 아카이브 스타일의 연출을 포함한 이모트 프리셋입니다.")]
public class PlayEmotePreset : Command
{
    [SerializeField] protected Character targetCharacter;
    [SerializeField] protected EmoteType emoteType;

    protected Vector2 defaultOffset = new Vector2(-100f, 650f); //
    protected float duration = 2.0f;

    protected virtual Sprite LoadSpriteFromAtlas(string spriteName)
    {
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Emoji");
        foreach (var s in allSprites)
        {
            if (s.name == spriteName) return s;
        }
        return null;
    }

    public override void OnEnter()
    {
        if (targetCharacter == null) { Continue(); return; }

        string targetHolderName = targetCharacter.name + " holder";
        GameObject holderObj = GameObject.Find(targetHolderName);
        Transform anchor = (holderObj != null) ? holderObj.transform : targetCharacter.transform;

        foreach (Transform child in anchor)
        {
            if (child.name.StartsWith("EmoteGroup_"))
            {
                Destroy(child.gameObject);
            }
        }

        List<Sprite> spritesToUse = new List<Sprite>();

        switch (emoteType)
        {
            case EmoteType.Angry:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_18"));
                break;
            case EmoteType.Chat: 
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_15"));
                break;
            case EmoteType.Dot:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_0"));
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_9"));
                break;
            case EmoteType.Exclaim:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_11"));
                break;
            case EmoteType.Heart:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_0"));
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_8"));
                break;
            case EmoteType.Laugh:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_16"));
                break;
            case EmoteType.Music:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_6"));
                break;
            case EmoteType.Twinkle:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_14"));
                break;
            case EmoteType.Surprise:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_13"));
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_12"));
                break;
            case EmoteType.Sweat:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_17"));
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_10"));
                break;
            case EmoteType.Shy:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_0"));
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_4"));
                break;
            case EmoteType.Question:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_5"));
                break;
            case EmoteType.Upset:
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_0"));
                spritesToUse.Add(LoadSpriteFromAtlas("Emoji 1_7"));
                break;
        }

        if (spritesToUse.Count > 0 && spritesToUse[0] != null)
        {
            StartCoroutine(ExecuteEmoteAnimation(anchor, spritesToUse));
        }
        Continue();
    }

    protected virtual IEnumerator ExecuteEmoteAnimation(Transform anchor, List<Sprite> emoteSprites)
    {
        float currentDuration = (emoteType == EmoteType.Angry) ? 1.1f : duration;
        GameObject parentObj = new GameObject("EmoteGroup_" + emoteType.ToString());
        parentObj.layer = LayerMask.NameToLayer("UI");
        RectTransform parentRect = parentObj.AddComponent<RectTransform>();
        parentObj.transform.SetParent(anchor, false);
        float flipFactor = 1f; // 애니메이션 루프에서 사용할 반전 변수
        Vector3 worldScale = anchor.lossyScale;

        if (worldScale.x < 0)
        {
            flipFactor = -1f;
            parentRect.localScale = new Vector3(-1f, 1f, 1f);
            parentRect.anchoredPosition = new Vector2(-defaultOffset.x, defaultOffset.y);
        }
        else
        {
            flipFactor = 1f;
            parentRect.localScale = Vector3.one;
            parentRect.anchoredPosition = defaultOffset;
        }
        if (emoteType == EmoteType.Exclaim || emoteType == EmoteType.Surprise || emoteType == EmoteType.Question)
            parentRect.pivot = new Vector2(0.5f, 0f);

        List<Image> images = new List<Image>();
        List<RectTransform> childRects = new List<RectTransform>();
        int count = (emoteType == EmoteType.Angry) ? 3 : (emoteType == EmoteType.Sweat) ? 2 : (emoteType == EmoteType.Twinkle) ? 3 : (emoteType == EmoteType.Dot) ? 4 : (emoteType == EmoteType.Chat) ? 3 : emoteSprites.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject childObj = new GameObject($"EmotePart_{i}");
            childObj.layer = LayerMask.NameToLayer("UI");
            childObj.transform.SetParent(parentObj.transform, false);
            RectTransform cRect = childObj.AddComponent<RectTransform>();
            Image cImg = childObj.AddComponent<Image>();

            // 1. [스프라이트 할당 파트] - 이미지를 먼저 정확히 넣어줍니다.
            if (emoteType == EmoteType.Chat || emoteType == EmoteType.Laugh || emoteType == EmoteType.Twinkle || emoteType == EmoteType.Angry)
            {
                cImg.sprite = emoteSprites[0];
            }
            else if (emoteType == EmoteType.Dot)
            {
                cImg.sprite = (i == 0) ? emoteSprites[0] : emoteSprites[1];
            }
            else
            {
                cImg.sprite = (i < emoteSprites.Count) ? emoteSprites[i] : emoteSprites[0];
            }
            cImg.SetNativeSize();
            cImg.color = Color.white; // 명시적으로 컬러 초기화
            if (emoteType == EmoteType.Dot && i > 0) cImg.color = new Color(1, 1, 1, 0);

            // 2. [위치 및 각도 설정 파트] - 각 타입별로 좌표를 분산시킵니다.
            if (emoteType == EmoteType.Chat)
            {
                switch (i)
                {
                    case 0: // 1. 맨 위
                        cRect.anchoredPosition = new Vector2(-15f, 25f); // 약간 왼쪽 위
                        cRect.localScale = new Vector3(0.7f, 0.7f, 1f);
                        cRect.localRotation = Quaternion.Euler(0, 0, -35f);
                        break;

                    case 1: // 2. 중간 (기준점)
                        cRect.anchoredPosition = new Vector2(-40f, 0);
                        cRect.localScale = Vector3.one;
                        cRect.localRotation = Quaternion.Euler(0, 0, -15f);
                        break;
                    case 2: // 3. 맨 아래
                        cRect.anchoredPosition = new Vector2(-30f, -35f); // 약간 오른쪽 아래
                        cRect.localScale = new Vector3(0.8f, 0.8f, 1f);
                        cRect.localRotation = Quaternion.Euler(0, 0, 10f);
                        break;
                }
            }
            else if (emoteType == EmoteType.Angry)
            {
                switch (i)
                {
                    case 0: cRect.anchoredPosition = new Vector2(-25f, 35f); cRect.localRotation = Quaternion.Euler(0, 0, 250f); break;
                    case 1: cRect.anchoredPosition = new Vector2(7f, 50f); cRect.localRotation = Quaternion.Euler(0, 0, 130f); break;
                    case 2: cRect.anchoredPosition = new Vector2(3f, 12f); cRect.localRotation = Quaternion.Euler(0, 0, 0f); break;
                }
            }
            else if (emoteType == EmoteType.Laugh)
            {
                cRect.pivot = new Vector2(2.5f, 1f);
                cRect.anchoredPosition = new Vector2(-10f, 0f);
            }
            else if (emoteType == EmoteType.Sweat)
            {
                cRect.anchoredPosition = (i == 0) ? new Vector2(-30f, 0f) : new Vector2(10f, 40f);
            }
            else if (emoteType == EmoteType.Twinkle)
            {
                if (i == 0) { cRect.anchoredPosition = new Vector2(-40f, 0f); cRect.localScale = new Vector3(0.8f, 0.8f, 1f); }
                else if (i == 1) { cRect.anchoredPosition = new Vector2(20f, 40f); cRect.localScale = new Vector3(0.6f, 0.6f, 1f); }
                else { cRect.anchoredPosition = new Vector2(25f, -25f); cRect.localScale = new Vector3(0.6f, 0.55f, 1f); }
            }
            else if (emoteType == EmoteType.Shy || emoteType == EmoteType.Upset || emoteType == EmoteType.Heart)
            {
                cRect.anchoredPosition = new Vector2(-80f, 0f);
            }
            else if (emoteType == EmoteType.Dot) // Dot 타입을 독립적인 else if로 분리
            {
                if (i == 0)
                {
                    cRect.anchoredPosition = new Vector2(-80f, 0f); // 말풍선 배경
                }
                else
                {
                    float dotSpacing = 40f;
                    float posX = -125f + (i - 1) * dotSpacing; // i가 1, 2, 3일 때 가로로 나열
                    cRect.anchoredPosition = new Vector2(posX, 0f);
                    cRect.localScale = new Vector3(0.8f, 0.8f, 1f);
                }
            }
            else if (emoteType == EmoteType.Surprise)
            {
                float sp = 25f;
                cRect.anchoredPosition = new Vector2(-30f + (i == 0 ? -sp : sp), 0f);
            }
            else
            {
                cRect.anchoredPosition = new Vector2(-30f, 0f);
            }

            images.Add(cImg);
            childRects.Add(cRect);
        }

        // [애니메이션 루프] (생략 - 기존 로직과 동일)
        // 3. 애니메이션 루프 실행 (이하 동일)

        float elapsed = 0f;

        while (elapsed < currentDuration)
        {
            if (parentObj == null) yield break;
            elapsed += Time.deltaTime;
            float progress = elapsed / currentDuration;

            if (emoteType == EmoteType.Laugh)

            {
                if (parentObj == null) yield break;

                float speed = 12f;
                float angleIntensity = 20f;

                float normalizedSin = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
                float rotZ = normalizedSin * angleIntensity; // 결과값: 0 ~ -15

                childRects[0].localRotation = Quaternion.Euler(0, 0, rotZ);
                childRects[0].anchoredPosition = new Vector2(120f, -70f);
            }
            else if (emoteType == EmoteType.Angry)

            {
                if (parentObj == null) yield break;

                if (elapsed < 0.15f)

                {

                    float s = Mathf.Lerp(0.5f, 1.1f, elapsed / 0.15f);

                    parentRect.localScale = new Vector3(s * flipFactor, s, 1f);

                }

                // 0.85s ~ 0.95s: 수축 (팍! 줄어듦)

                else if (elapsed >= 0.85f && elapsed < 0.95f)
                {
                    float shrinkT = (elapsed - 0.85f) / 0.10f; // 0.1초 동안 실행
                    float s = Mathf.Lerp(1.0f, 0.3f, shrinkT); // 1.0에서 0.3으로 급격히 수축
                    parentRect.localScale = new Vector3(s * flipFactor, s, 1f);

                }

                else if (elapsed >= 0.95f)
                {
                    parentRect.localScale = new Vector3(0.3f *flipFactor, 0.3f, 1f);
                }

                else
                {
                    parentRect.localScale = new Vector3(flipFactor, 1, 1);
                }



                // 0.95s ~ 종료: 페이드 아웃 (투명해짐)

                if (elapsed >= 0.95f)
                {
                    float fadeT = (elapsed - 0.95f) / (currentDuration - 0.95f);
                    float alpha = Mathf.Lerp(1f, 0f, fadeT);
                    foreach (var img in images) img.color = new Color(1, 1, 1, alpha);
                }

            }

            else if (emoteType == EmoteType.Sweat)
            {
                if (parentObj == null) yield break;
                float sweatTime = 1.2f;



                if (elapsed < sweatTime)

                {

                    float t = elapsed / sweatTime;

                    float alpha = (t <= 0.5f) ? Mathf.Lerp(0f, 1.0f, t / 0.5f) : Mathf.Lerp(1.0f, 0f, (t - 0.5f) / 0.5f);

                    Color fColor = new Color(1, 1, 1, alpha);



                    childRects[0].anchoredPosition = new Vector2(-30f, Mathf.Lerp(0f, -30f, t)); // 30 유닛 이동

                    childRects[1].anchoredPosition = new Vector2(10f, Mathf.Lerp(40f, 25f, t)); // 15 유닛 이동 (절반)

                    foreach (var img in images) img.color = fColor;

                }

                else foreach (var img in images) img.color = new Color(1, 1, 1, 0);

            }
            else if (emoteType == EmoteType.Music)
            {
                if (parentObj == null) yield break;
                childRects[0].anchoredPosition = new Vector2(-40f * progress - 40f, 10f * Mathf.Sin(progress * 8f));

                images[0].color = new Color(1, 1, 1, 1f - progress);

            }

            else
            {
                if (emoteType == EmoteType.Twinkle)
                {
                    if (parentObj == null) yield break;
                    float speed = 10f;

                    float intensity = 0.2f;

                    // 포인트 1: 모든 스프라이트의 타이밍을 다르게 설정

                    for (int i = 0; i < childRects.Count; i++)

                    {

                        // 각 별마다 i * 0.5f 만큼의 시간 차(Offset)를 부여하여 엇갈리게 반짝임

                        float individualTime = Time.time + (i * 0.8f);

                        float pulse = 1f + Mathf.Sin(individualTime * speed) * intensity;



                        // 초기 설정된 스케일(1.0, 0.65, 0.35)을 유지하며 펄스만 적용

                        Vector3 baseScale = (i == 0) ? Vector3.one : (i == 1) ? new Vector3(0.65f, 0.65f, 1f) : new Vector3(0.35f, 0.35f, 1f);

                        childRects[i].localScale = baseScale * pulse;

                    }

                }

                else if (emoteType == EmoteType.Question)
                {
                    if (parentObj == null) yield break;
                    if (elapsed < 0.5f)
                    {
                        float t = elapsed / 0.5f;
                        float bounceScale = 1f;

                        if (t < 0.5f) bounceScale = Mathf.Lerp(0f, 1.4f, t / 0.5f);
                        else if (t < 0.8f) bounceScale = Mathf.Lerp(1.4f, 0.9f, (t - 0.5f) / 0.3f);
                        else bounceScale = Mathf.Lerp(0.9f, 1.0f, (t - 0.8f) / 0.2f);

                        parentRect.localScale = new Vector3(flipFactor, bounceScale, 1f);
                    }
                    else parentRect.localScale = new Vector3(flipFactor, 1f, 1f);

                }
                else if (emoteType == EmoteType.Shy)
                {
                    if (parentObj == null) yield break;
                    float rotationTime = 1.5f;

                    if (elapsed < rotationTime)

                    {
                        float t = (elapsed / rotationTime) * 3f;
                        float rotZ = 0f;

                        if (t < 1f) rotZ = Mathf.Lerp(10f, -10f, t);
                        else if (t < 2f) rotZ = Mathf.Lerp(-10f, 10f, t - 1f);
                        else rotZ = Mathf.Lerp(10f, 0f, t - 2f);

                        childRects[1].localRotation = Quaternion.Euler(flipFactor, 0, rotZ);
                    }
                    else childRects[1].localRotation = Quaternion.identity;
                }

                else if (emoteType == EmoteType.Exclaim || emoteType == EmoteType.Surprise)
                {
                    if (parentObj == null) yield break;
                    if (elapsed < 0.4f)
                    {
                        float t = elapsed / 0.4f;
                        float bounceScale = (t < 0.7f) ? Mathf.Lerp(0f, 1.3f, t / 0.7f) : Mathf.Lerp(1.3f, 1.0f, (t - 0.7f) / 0.3f);
                        parentRect.localScale = new Vector3(flipFactor, bounceScale, 1f);
                    }
                    else parentRect.localScale = new Vector3(flipFactor, 1f, 1f);
                }

                else if (emoteType == EmoteType.Dot)
                {
                    if (parentObj == null) yield break;
                    for (int dotIdx = 1; dotIdx <= 3; dotIdx++)
                    {
                        if (elapsed > dotIdx * 0.3f)
                        {
                            images[dotIdx].color = new Color(1, 1, 1, 1);
                        }
                    }
                }

                else if (emoteType == EmoteType.Upset)
                {
                    if (parentObj == null) yield break;

                    childRects[0].anchoredPosition = new Vector2(-80f, 0f);
                    childRects[0].localScale = Vector3.one;

                    float speed = 10f;
                    float intensity = 0.1f;

                    float scaleX = 0.7f + Mathf.Sin(Time.time * speed) * intensity;
                    float scaleY = 0.7f + Mathf.Cos(Time.time * speed) * intensity;

                    childRects[1].anchoredPosition = new Vector2(-80f, 0f);
                    childRects[1].localScale = new Vector3(scaleX, scaleY, 1f);
                    // 3. 공통: 종료 0.3초 전 빠른 페이드 아웃

                    if (progress > 0.85f)
                    {
                        float alpha = Mathf.Lerp(1f, 0f, (progress - 0.85f) / 0.15f);

                        foreach (var img in images) img.color = new Color(1, 1, 1, alpha);
                    }
                }

                else if (emoteType == EmoteType.Heart)
                {
                    if (parentObj == null) yield break;
                    float hS = (elapsed < 0.4f) ? ((elapsed / 0.4f < 0.5f) ? Mathf.Lerp(1.0f, 1.2f, (elapsed / 0.4f) / 0.5f) : Mathf.Lerp(1.2f, 1.0f, (elapsed / 0.4f - 0.5f) / 0.5f)) : 1.0f;

                    childRects[1].localScale = new Vector3(hS, hS, 1f);

                }

                if (progress > 0.85f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (progress - 0.85f) / 0.15f);
                    foreach (var img in images) img.color = new Color(1, 1, 1, alpha);
                }
            }
            yield return null;
        }
        Destroy(parentObj);
    }
}

