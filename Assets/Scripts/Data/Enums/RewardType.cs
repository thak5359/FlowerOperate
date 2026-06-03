[System.Flags]
public enum RewardType
{
    Unknown = 0,
    Currency = 1 << 0,
    Item = 1 << 1,
    Reputation = 1 << 2,
    AbilityUnlock = 1 << 3, // 오타 수정 (Abilitynlock -> AbilityUnlock)
    ShopUnlock = 1 << 4,
}