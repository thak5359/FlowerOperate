// SpriteToMesh.cs
using UnityEngine;

public class SpriteToMesh : MonoBehaviour
{
    public Sprite sourceSprite;

    // 에디터 버튼에서 호출할 세팅 함수
    public void SetupComponents()
    {
        // 1. MeshFilter 추가 및 확인
        if (!TryGetComponent<MeshFilter>(out var filter))
            filter = gameObject.AddComponent<MeshFilter>();

        // 2. MeshRenderer 추가 및 확인
        if (!TryGetComponent<MeshRenderer>(out var renderer))
            renderer = gameObject.AddComponent<MeshRenderer>();

        // 3. 기본 머티리얼 세팅 (스프라이트 전용 셰이더 권장)
        if (renderer.sharedMaterial == null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        Debug.Log("<color=cyan>[Project FO]</color> 파트너, 컴포넌트 세팅이 완료되었습니다!");
    }
}