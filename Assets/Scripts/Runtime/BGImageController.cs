using UnityEngine;
using System.Collections.Generic;

public class BGImageController : MonoBehaviour
{
    public List<SpriteRenderer> bg_images = new List<SpriteRenderer>();

    // 캐싱할 변수들
    private Vector3 centerPos = new Vector3(960f, 540f, 0f);
    private Vector3 targetScale = Vector3.one;

    void Awake()
    {
        InitializeImages();
    }

    public void InitializeImages()
    {
        if (bg_images.Count == 0) return;

        // 1. 화면의 1.2배 크기 계산
        float worldHeight = Camera.main.orthographicSize * 2.0f;
        float worldWidth = worldHeight / Screen.height * Screen.width;

        for (int i = 0; i < bg_images.Count; i++)
        {
            var sr = bg_images[i];
            if (sr == null) continue;

            // 중앙 배치 및 1.2배 스케일링
            sr.transform.position = centerPos;

            float sWidth = sr.sprite.bounds.size.x;
            float sHeight = sr.sprite.bounds.size.y;

            targetScale.x = (worldWidth / sWidth) * 1.2f;
            targetScale.y = (worldHeight / sHeight) * 1.2f;
            sr.transform.localScale = targetScale;

            // Awake 규칙: 1번째만 켜고 나머지 끔
            sr.gameObject.SetActive(i == 0);
        }
    }

    // 이름으로 배경 교체
    public void ChangeBackground(string imageName)
    {
        foreach (var sr in bg_images)
        {
            if (sr == null) continue;
            sr.gameObject.SetActive(sr.name == imageName);
        }
    }
}