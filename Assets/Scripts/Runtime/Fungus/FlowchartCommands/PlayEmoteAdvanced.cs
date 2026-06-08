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

    protected Vector2 defaultOffset = new Vector2(-100f, 350f); //
    protected float duration = 2.0f;

    protected static Sprite[] cachedSprites;

    protected virtual Sprite LoadSpriteFromAtlas(string spriteName)
    {
        if (cachedSprites == null || cachedSprites.Length == 0)
        {
            cachedSprites = Resources.LoadAll<Sprite>("Emoji");
            if (cachedSprites == null || cachedSprites.Length == 0)
            {
                Debug.LogError("[PlayEmotePreset] Resources.LoadAll<Sprite>(\"Emoji\") returned no sprites. Check if Emoji.png is in a Resources folder and set to Sprite (Multiple).");
                return null;
            }
        }

        // 1. Exact match
        foreach (var s in cachedSprites)
        {
            if (s != null && s.name == spriteName) return s;
        }

        // 2. Fallback: Case-insensitive and ignore space/underscore variations
        string normalizedTarget = spriteName.Replace(" ", "").Replace("_", "").ToLower();
        foreach (var s in cachedSprites)
        {
            if (s != null)
            {
                string normalizedName = s.name.Replace(" ", "").Replace("_", "").ToLower();
                if (normalizedName == normalizedTarget)
                {
                    Debug.LogWarning($"[PlayEmotePreset] Sprite '{spriteName}' matched via fallback name '{s.name}'");
                    return s;
                }
            }
        }

        Debug.LogError($"[PlayEmotePreset] Sprite '{spriteName}' could not be found in the loaded 'Emoji' atlas.");
        return null;
    }

    public override void OnEnter()
    {
        if (targetCharacter == null) 
        { 
            Debug.LogError("[PlayEmotePreset] targetCharacter is null!");
            Continue(); 
            return; 
        }

        string targetHolderName = targetCharacter.name + " holder";
        GameObject holderObj = GameObject.Find(targetHolderName);
        
        // Fallback: If "[Character] holder" not found, try to find the character's GameObject directly in the scene
        if (holderObj == null)
        {
            holderObj = GameObject.Find(targetCharacter.name);
        }

        Transform anchor = null;
        if (holderObj != null)
        {
            anchor = holderObj.transform;
        }
        else
        {
            anchor = targetCharacter.transform;
            if (anchor != null && !anchor.gameObject.scene.IsValid())
            {
                Debug.LogWarning($"[PlayEmotePreset] Active character portrait/holder for '{targetCharacter.name}' was not found in the scene. Emote might be instantiated in the prefab/assets database and won't be visible!");
            }
        }

        if (anchor == null)
        {
            Debug.LogError($"[PlayEmotePreset] Anchor transform for character '{targetCharacter.name}' is null. Cannot play emote.");
            Continue();
            return;
        }

        foreach (Transform child in anchor)
        {
            if (child != null && child.name.StartsWith("EmoteGroup_"))
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
        else
        {
            string loadedStatus = spritesToUse.Count > 0 ? "first sprite is null" : "list is empty";
            Debug.LogError($"[PlayEmotePreset] Failed to start emote animation for '{emoteType}' on '{targetCharacter.name}' ({loadedStatus})");
        }
        Continue();
    }

    protected virtual IEnumerator ExecuteEmoteAnimation(Transform anchor, List<Sprite> emoteSprites)
    {
        float currentDuration = (emoteType == EmoteType.Angry) ? 1.1f : duration;
        GameObject parentObj = new GameObject("EmoteGroup_" + emoteType.ToString());
        
        // Canvas 체크하여 UI 여부 확인
        bool isUI = anchor.GetComponentInParent<Canvas>() != null;
        
        parentObj.layer = LayerMask.NameToLayer(isUI ? "UI" : "Default");
        Transform parentTransform = isUI ? parentObj.AddComponent<RectTransform>() : parentObj.transform;
        parentObj.transform.SetParent(anchor, false);
        
        float flipFactor = 1f; // 애니메이션 루프에서 사용할 반전 변수
        Vector3 worldScale = anchor.lossyScale;

        // PPU 정의 (Emoji.png.meta 기준 150)
        float ppu = 150f;
        
        float posY = defaultOffset.y;

        if (isUI)
        {
            RectTransform parentRect = (RectTransform)parentTransform;
            if (worldScale.x < 0)
            {
                flipFactor = -1f;
                parentRect.localScale = new Vector3(-1f, 1f, 1f);
                parentRect.anchoredPosition = new Vector2(-defaultOffset.x, posY);
            }
            else
            {
                flipFactor = 1f;
                parentRect.localScale = Vector3.one;
                parentRect.anchoredPosition = new Vector2(defaultOffset.x, posY);
            }
            if (emoteType == EmoteType.Exclaim || emoteType == EmoteType.Surprise || emoteType == EmoteType.Question)
                parentRect.pivot = new Vector2(0.5f, 0f);
        }
        else
        {
            Vector3 localOffset = new Vector3(defaultOffset.x / ppu, posY / ppu, 0f);
            if (worldScale.x < 0)
            {
                flipFactor = -1f;
                parentTransform.localScale = new Vector3(-1f, 1f, 1f);
                parentTransform.localPosition = new Vector3(-localOffset.x, localOffset.y, 0f);
            }
            else
            {
                flipFactor = 1f;
                parentTransform.localScale = Vector3.one;
                parentTransform.localPosition = localOffset;
            }
        }

        List<Component> imageComponents = new List<Component>();
        List<Transform> childTransforms = new List<Transform>();
        int count = (emoteType == EmoteType.Angry) ? 3 : (emoteType == EmoteType.Sweat) ? 2 : (emoteType == EmoteType.Twinkle) ? 3 : (emoteType == EmoteType.Dot) ? 4 : (emoteType == EmoteType.Chat) ? 3 : emoteSprites.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject childObj = new GameObject($"EmotePart_{i}");
            childObj.layer = parentObj.layer;
            childObj.transform.SetParent(parentObj.transform, false);
            
            Transform cTransform = isUI ? childObj.AddComponent<RectTransform>() : childObj.transform;
            Component cImg = isUI ? (Component)childObj.AddComponent<Image>() : (Component)childObj.AddComponent<SpriteRenderer>();

            Sprite targetSprite = null;
            if (emoteType == EmoteType.Chat || emoteType == EmoteType.Laugh || emoteType == EmoteType.Twinkle || emoteType == EmoteType.Angry)
            {
                targetSprite = emoteSprites[0];
            }
            else if (emoteType == EmoteType.Dot)
            {
                targetSprite = (i == 0) ? emoteSprites[0] : emoteSprites[1];
            }
            else
            {
                targetSprite = (i < emoteSprites.Count) ? emoteSprites[i] : emoteSprites[0];
            }

            if (isUI)
            {
                Image image = (Image)cImg;
                image.sprite = targetSprite;
                image.SetNativeSize();
                image.color = Color.white;
                if (emoteType == EmoteType.Dot && i > 0) image.color = new Color(1, 1, 1, 0);
            }
            else
            {
                SpriteRenderer sr = (SpriteRenderer)cImg;
                sr.sprite = targetSprite;
                sr.color = Color.white;
                sr.sortingOrder = 100; // 캐릭터 앞에 오도록
                if (emoteType == EmoteType.Dot && i > 0) sr.color = new Color(1, 1, 1, 0);
            }

            if (targetSprite == null)
            {
                Debug.LogError($"[PlayEmotePreset] Sprite for EmotePart_{i} of emote '{emoteType}' is null! Check if the sprite was loaded correctly.");
            }

            // 위치 설정
            if (isUI)
            {
                RectTransform cRect = (RectTransform)cTransform;
                if (emoteType == EmoteType.Chat)
                {
                    switch (i)
                    {
                        case 0:
                            cRect.anchoredPosition = new Vector2(-15f, 25f);
                            cRect.localScale = new Vector3(0.7f, 0.7f, 1f);
                            cRect.localRotation = Quaternion.Euler(0, 0, -35f);
                            break;
                        case 1:
                            cRect.anchoredPosition = new Vector2(-40f, 0);
                            cRect.localScale = Vector3.one;
                            cRect.localRotation = Quaternion.Euler(0, 0, -15f);
                            break;
                        case 2:
                            cRect.anchoredPosition = new Vector2(-30f, -35f);
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
                else if (emoteType == EmoteType.Dot)
                {
                    if (i == 0)
                    {
                        cRect.anchoredPosition = new Vector2(-80f, 0f);
                    }
                    else
                    {
                        float dotSpacing = 40f;
                        float posX = -125f + (i - 1) * dotSpacing;
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
            }
            else
            {
                // World Space 위치 설정 (픽셀 값을 PPU로 나누어 설정)
                if (emoteType == EmoteType.Chat)
                {
                    switch (i)
                    {
                        case 0:
                            cTransform.localPosition = new Vector3(-15f / ppu, 25f / ppu, 0f);
                            cTransform.localScale = new Vector3(0.7f, 0.7f, 1f);
                            cTransform.localRotation = Quaternion.Euler(0, 0, -35f);
                            break;
                        case 1:
                            cTransform.localPosition = new Vector3(-40f / ppu, 0f, 0f);
                            cTransform.localScale = Vector3.one;
                            cTransform.localRotation = Quaternion.Euler(0, 0, -15f);
                            break;
                        case 2:
                            cTransform.localPosition = new Vector3(-30f / ppu, -35f / ppu, 0f);
                            cTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
                            cTransform.localRotation = Quaternion.Euler(0, 0, 10f);
                            break;
                    }
                }
                else if (emoteType == EmoteType.Angry)
                {
                    switch (i)
                    {
                        case 0: cTransform.localPosition = new Vector3(-25f / ppu, 35f / ppu, 0f); cTransform.localRotation = Quaternion.Euler(0, 0, 250f); break;
                        case 1: cTransform.localPosition = new Vector3(7f / ppu, 50f / ppu, 0f); cTransform.localRotation = Quaternion.Euler(0, 0, 130f); break;
                        case 2: cTransform.localPosition = new Vector3(3f / ppu, 12f / ppu, 0f); cTransform.localRotation = Quaternion.Euler(0, 0, 0f); break;
                    }
                }
                else if (emoteType == EmoteType.Laugh)
                {
                    cTransform.localPosition = new Vector3(-10f / ppu, 0f, 0f);
                }
                else if (emoteType == EmoteType.Sweat)
                {
                    cTransform.localPosition = (i == 0) ? new Vector3(-30f / ppu, 0f, 0f) : new Vector3(10f / ppu, 40f / ppu, 0f);
                }
                else if (emoteType == EmoteType.Twinkle)
                {
                    if (i == 0) { cTransform.localPosition = new Vector3(-40f / ppu, 0f, 0f); cTransform.localScale = new Vector3(0.8f, 0.8f, 1f); }
                    else if (i == 1) { cTransform.localPosition = new Vector3(20f / ppu, 40f / ppu, 0f); cTransform.localScale = new Vector3(0.6f, 0.6f, 1f); }
                    else { cTransform.localPosition = new Vector3(25f / ppu, -25f / ppu, 0f); cTransform.localScale = new Vector3(0.6f, 0.55f, 1f); }
                }
                else if (emoteType == EmoteType.Shy || emoteType == EmoteType.Upset || emoteType == EmoteType.Heart)
                {
                    cTransform.localPosition = new Vector3(-80f / ppu, 0f, 0f);
                }
                else if (emoteType == EmoteType.Dot)
                {
                    if (i == 0)
                    {
                        cTransform.localPosition = new Vector3(-80f / ppu, 0f, 0f);
                    }
                    else
                    {
                        float dotSpacing = 40f;
                        float posX = -125f + (i - 1) * dotSpacing;
                        cTransform.localPosition = new Vector3(posX / ppu, 0f, 0f);
                        cTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
                    }
                }
                else if (emoteType == EmoteType.Surprise)
                {
                    float sp = 25f;
                    cTransform.localPosition = new Vector3((-30f + (i == 0 ? -sp : sp)) / ppu, 0f, 0f);
                }
                else
                {
                    cTransform.localPosition = new Vector3(-30f / ppu, 0f, 0f);
                }
            }

            imageComponents.Add(cImg);
            childTransforms.Add(cTransform);
        }

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
                float rotZ = normalizedSin * angleIntensity;

                childTransforms[0].localRotation = Quaternion.Euler(0, 0, rotZ);
                if (isUI)
                {
                    ((RectTransform)childTransforms[0]).anchoredPosition = new Vector2(120f, -70f);
                }
                else
                {
                    childTransforms[0].localPosition = new Vector3(120f / ppu, -70f / ppu, 0f);
                }
            }
            else if (emoteType == EmoteType.Angry)
            {
                if (parentObj == null) yield break;

                if (elapsed < 0.15f)
                {
                    float s = Mathf.Lerp(0.5f, 1.1f, elapsed / 0.15f);
                    parentTransform.localScale = new Vector3(s * flipFactor, s, 1f);
                }
                else if (elapsed >= 0.85f && elapsed < 0.95f)
                {
                    float shrinkT = (elapsed - 0.85f) / 0.10f;
                    float s = Mathf.Lerp(1.0f, 0.3f, shrinkT);
                    parentTransform.localScale = new Vector3(s * flipFactor, s, 1f);
                }
                else if (elapsed >= 0.95f)
                {
                    parentTransform.localScale = new Vector3(0.3f * flipFactor, 0.3f, 1f);
                }
                else
                {
                    parentTransform.localScale = new Vector3(flipFactor, 1f, 1f);
                }

                if (elapsed >= 0.95f)
                {
                    float fadeT = (elapsed - 0.95f) / (currentDuration - 0.95f);
                    float alpha = Mathf.Lerp(1f, 0f, fadeT);
                    foreach (var img in imageComponents) SetAlpha(img, alpha);
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

                    if (isUI)
                    {
                        ((RectTransform)childTransforms[0]).anchoredPosition = new Vector2(-30f, Mathf.Lerp(0f, -30f, t));
                        ((RectTransform)childTransforms[1]).anchoredPosition = new Vector2(10f, Mathf.Lerp(40f, 25f, t));
                    }
                    else
                    {
                        childTransforms[0].localPosition = new Vector3(-30f / ppu, Mathf.Lerp(0f, -30f, t) / ppu, 0f);
                        childTransforms[1].localPosition = new Vector3(10f / ppu, Mathf.Lerp(40f, 25f, t) / ppu, 0f);
                    }
                    foreach (var img in imageComponents) SetAlpha(img, alpha);
                }
                else
                {
                    foreach (var img in imageComponents) SetAlpha(img, 0f);
                }
            }
            else if (emoteType == EmoteType.Music)
            {
                if (parentObj == null) yield break;
                if (isUI)
                {
                    ((RectTransform)childTransforms[0]).anchoredPosition = new Vector2(-40f * progress - 40f, 10f * Mathf.Sin(progress * 8f));
                }
                else
                {
                    childTransforms[0].localPosition = new Vector3((-40f * progress - 40f) / ppu, (10f * Mathf.Sin(progress * 8f)) / ppu, 0f);
                }
                SetAlpha(imageComponents[0], 1f - progress);
            }
            else
            {
                if (emoteType == EmoteType.Twinkle)
                {
                    if (parentObj == null) yield break;
                    float speed = 10f;
                    float intensity = 0.2f;

                    for (int i = 0; i < childTransforms.Count; i++)
                    {
                        float individualTime = Time.time + (i * 0.8f);
                        float pulse = 1f + Mathf.Sin(individualTime * speed) * intensity;

                        Vector3 baseScale = (i == 0) ? Vector3.one : (i == 1) ? new Vector3(0.65f, 0.65f, 1f) : new Vector3(0.35f, 0.35f, 1f);
                        childTransforms[i].localScale = baseScale * pulse;
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

                        parentTransform.localScale = new Vector3(flipFactor, bounceScale, 1f);
                    }
                    else parentTransform.localScale = new Vector3(flipFactor, 1f, 1f);
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

                        childTransforms[1].localRotation = Quaternion.Euler(flipFactor, 0, rotZ);
                    }
                    else childTransforms[1].localRotation = Quaternion.identity;
                }
                else if (emoteType == EmoteType.Exclaim || emoteType == EmoteType.Surprise)
                {
                    if (parentObj == null) yield break;
                    if (elapsed < 0.4f)
                    {
                        float t = elapsed / 0.4f;
                        float bounceScale = (t < 0.7f) ? Mathf.Lerp(0f, 1.3f, t / 0.7f) : Mathf.Lerp(1.3f, 1.0f, (t - 0.7f) / 0.3f);
                        parentTransform.localScale = new Vector3(flipFactor, bounceScale, 1f);
                    }
                    else parentTransform.localScale = new Vector3(flipFactor, 1f, 1f);
                }
                else if (emoteType == EmoteType.Dot)
                {
                    if (parentObj == null) yield break;
                    for (int dotIdx = 1; dotIdx <= 3; dotIdx++)
                    {
                        if (elapsed > dotIdx * 0.3f)
                        {
                            SetAlpha(imageComponents[dotIdx], 1f);
                        }
                    }
                }
                else if (emoteType == EmoteType.Upset)
                {
                    if (parentObj == null) yield break;

                    if (isUI)
                    {
                        ((RectTransform)childTransforms[0]).anchoredPosition = new Vector2(-80f, 0f);
                        ((RectTransform)childTransforms[1]).anchoredPosition = new Vector2(-80f, 0f);
                    }
                    else
                    {
                        childTransforms[0].localPosition = new Vector3(-80f / ppu, 0f, 0f);
                        childTransforms[1].localPosition = new Vector3(-80f / ppu, 0f, 0f);
                    }
                    childTransforms[0].localScale = Vector3.one;

                    float speed = 10f;
                    float intensity = 0.1f;

                    float scaleX = 0.7f + Mathf.Sin(Time.time * speed) * intensity;
                    float scaleY = 0.7f + Mathf.Cos(Time.time * speed) * intensity;

                    childTransforms[1].localScale = new Vector3(scaleX, scaleY, 1f);

                    if (progress > 0.85f)
                    {
                        float alpha = Mathf.Lerp(1f, 0f, (progress - 0.85f) / 0.15f);
                        foreach (var img in imageComponents) SetAlpha(img, alpha);
                    }
                }
                else if (emoteType == EmoteType.Heart)
                {
                    if (parentObj == null) yield break;
                    float hS = (elapsed < 0.4f) ? ((elapsed / 0.4f < 0.5f) ? Mathf.Lerp(1.0f, 1.2f, (elapsed / 0.4f) / 0.5f) : Mathf.Lerp(1.2f, 1.0f, (elapsed / 0.4f - 0.5f) / 0.5f)) : 1.0f;
                    childTransforms[1].localScale = new Vector3(hS, hS, 1f);
                }

                if (progress > 0.85f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (progress - 0.85f) / 0.15f);
                    foreach (var img in imageComponents) SetAlpha(img, alpha);
                }
            }
            yield return null;
        }
        Destroy(parentObj);
    }

    private void SetAlpha(Component comp, float alpha)
    {
        if (comp == null) return;
        if (comp is Image image)
        {
            image.color = new Color(1, 1, 1, alpha);
        }
        else if (comp is SpriteRenderer sr)
        {
            sr.color = new Color(1, 1, 1, alpha);
        }
    }
}
