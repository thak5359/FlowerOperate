using UnityEngine;

public class SimpleFaceSwitcher : MonoBehaviour
{
    [Header("설정")]
    public SpriteRenderer faceRenderer;
    public Sprite[] expressions; // 아까 자른 표정 조각들을 여기에 드래그 앤 드롭하세요.

    [Range(0, 15)] // 잘라낸 표정 개수에 맞춰 조절
    public int currentExpressionIndex = 0;

    private int lastIndex = -1;

    void Update()
    {
        // 인스펙터에서 슬라이더를 옮기면 실시간으로 반영됩니다.
        if (currentExpressionIndex != lastIndex)
        {
            if (currentExpressionIndex < expressions.Length)
            {
                faceRenderer.sprite = expressions[currentExpressionIndex];
                lastIndex = currentExpressionIndex;
            }
        }
    }
}