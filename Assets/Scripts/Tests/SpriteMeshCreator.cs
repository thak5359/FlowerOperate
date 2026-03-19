using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SpriteMeshCreator : MonoBehaviour
{
    public Sprite sourceSprite; // 원본 스프라이트

    void Start()
    {
        if (sourceSprite == null) return;

        CreateMeshFromSprite();
    }

    private void CreateMeshFromSprite()
    {
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedSpriteMesh";

        // 1. 정점(Vertices) 추출: Sprite는 Vector2를 쓰므로 Vector3로 변환해줍니다.
        Vector2[] vertices2D = sourceSprite.vertices;
        Vector3[] vertices3D = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices2D.Length; i++)
        {
            vertices3D[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }

        // 2. 삼각형(Triangles) 인덱스 추출: Sprite는 ushort를 쓰므로 int로 변환합니다.
        ushort[] trianglesShort = sourceSprite.triangles;
        int[] trianglesInt = new int[trianglesShort.Length];
        for (int i = 0; i < trianglesShort.Length; i++)
        {
            trianglesInt[i] = (int)trianglesShort[i];
        }

        // 3. 메쉬 데이터 할당
        mesh.vertices = vertices3D;
        mesh.triangles = trianglesInt;
        mesh.uv = sourceSprite.uv; // UV는 그대로 사용 가능

        // 4. 최적화 및 법선 계산
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // 5. 컴포넌트에 적용
        GetComponent<MeshFilter>().mesh = mesh;

        // 6. 텍스처 적용을 위한 머티리얼 세팅
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.mainTexture = sourceSprite.texture;
    }
}