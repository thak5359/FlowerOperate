using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

public partial class FarmSceneChunkController : MonoBehaviour
{
    [Header("Please put chunks in here in descending order")]
    [SerializeField] private GameObject[] _FarmChunks;
    [SerializeField] private GameObject[] _FieldChunks;
    [SerializeField] private GameObject[] _ForestChunks;
    // Mine 기획 확정 후 활성화
    // [SerializeField] private GameObject[] _MineChunks;

    private ChunkManager _chunkManager;
    private DisposableBag disposableBag = new();

    [Inject]
    public void Construct(ChunkManager chunkManager)
    {
        _chunkManager = chunkManager;
    }

    private void Start()
    {
        InitializeAsync().Forget();
    }

    private async UniTaskVoid InitializeAsync()
    {
        if (_chunkManager != null)
        {
            await _chunkManager.Initialization.AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
            SyncChunksState();
            _chunkManager.ChunkUnlockedSubject.Subscribe((chunkInfo) =>
            {
                SwitchChunkState(chunkInfo.type, chunkInfo.input1 - 1, true);
            }).AddTo(ref disposableBag);
        }
    }

    /// <summary>
    /// ChunkManager의 데이터와 농장씬의 모든 청크 상태를 동기화 합니다.
    /// </summary>
    public void SyncChunksState()
    {
        SyncChunksState(_chunkManager.GetFarmChunkDatas);
        SyncChunksState(_chunkManager.GetFieldChunkDatas);
        SyncChunksState(_chunkManager.GetForestChunkDatas);

        // Mine 기획 확정 후 활성화
        // SyncChunksState(_chunkManager.GetMineChunkDatas);
    }

    private void SyncChunksState(ChunkDataIngame[] chunkDatas)
    {
        foreach (ChunkDataIngame data in chunkDatas)
        {
            SwitchChunkState(data.ChunkType, data.ChunkNo - 1, data.UnlockState == ChunkUnlockState.Unlocked);
        }
    }

    private void SwitchChunkState(ChunkType type, int idx, bool isUnlocked)
    {
        switch (type)
        {
            case ChunkType.Farm:
                _FarmChunks[idx].SetActive(isUnlocked);
                break;
            case ChunkType.Field:
                _FieldChunks[idx].SetActive(isUnlocked);
                break;
            case ChunkType.Forest:
                _ForestChunks[idx].SetActive(isUnlocked);
                break;
        }
    }

    private void SwitchChunkState(ChunkType type, int idx)
    {
        switch (type)
        {
            case ChunkType.Farm:
                _FarmChunks[idx-1].SetActive(_chunkManager.GetFarmChunkDatas[idx - 1].UnlockState == ChunkUnlockState.Unlocked);
                break;  
            case ChunkType.Field:
                _FieldChunks[idx - 1].SetActive(_chunkManager.GetFieldChunkDatas[idx - 1].UnlockState == ChunkUnlockState.Unlocked);
                break;
            case ChunkType.Forest:
                _ForestChunks[idx - 1].SetActive(_chunkManager.GetForestChunkDatas[idx - 1].UnlockState == ChunkUnlockState.Unlocked);
                break;
        }
    }

    private void OnDestroy()
    {
        disposableBag.Dispose();
    }
}
