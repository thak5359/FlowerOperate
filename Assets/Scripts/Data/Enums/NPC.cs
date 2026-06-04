using UnityEngine;

public enum NPC 
{
    None = 0,
    Hwaja = 1,
    YeongJoon = 2,
    YeongSook = 3,
    Mago = 4,
    Hex=5,
    Yuuna = 99
}

public class NpcClass : MonoBehaviour
{
    public NPC npcName;
    public SpriteRenderer npcSpriteRenderer {get; private set;}

    void Awake()
    {
        npcSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}
