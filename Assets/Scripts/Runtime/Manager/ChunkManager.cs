// 수정 위치: 전체 스크립트 (ChunkActionResult, ChunkDataIngame, ChunkManager 전체)
// 작업 내용: 서브 프로그래머의 이해를 돕기 위한 #region 그룹화, /// <summary> 문서화 및 연동 가이드 주석 추가

using Cysharp.Threading.Tasks;
using MemoryPack;
using Unity.Collections;
using UnityEngine;
using VContainer;
using static Constant;

#region Structs : Chunk Data & Result
/// <summary>
/// 청크 관련 작업(해금, 상태 변경 등)의 성공/실패 여부와 에러 메시지를 반환하는 구조체입니다.
/// </summary>
public struct ChunkActionResult
{
    private ResultType result;
    public ResultType Result => result;

    public FixedString128Bytes errorMessage { get; private set; }

    public ChunkActionResult(ResultType input_result, FixedString128Bytes input_errorCode = default)
    {
        result = input_result;
        errorMessage = input_errorCode;
    }

    /// <summary>
    /// 여러 개의 청크 상태 변경 결과를 하나로 병합할 때 사용합니다.
    /// 하나라도 에러가 있다면 최종 결과는 Error가 됩니다.
    /// </summary>
    public void Combine(ChunkActionResult resultB)
    {
        if (resultB.Result == ResultType.Error)
        {
            result = ResultType.Error;
            errorMessage = resultB.errorMessage;
        }
        else if (resultB.Result == ResultType.Failed && result != ResultType.Error)
        {
            result = ResultType.Failed;
        }
    }
}

/// <summary>
/// 인게임에서 관리되는 개별 청크의 상태 데이터입니다. (저장/로드 가능)
/// </summary>
[MemoryPackable]
public partial struct ChunkDataIngame
{
    [MemoryPackInclude]
    private int chunkNo;
    public int ChunkNo => chunkNo;

    [MemoryPackInclude]
    private ChunkType chunkType;
    public ChunkType ChunkType => chunkType;

    [MemoryPackInclude]
    private ChunkUnlockState unlockState;
    public ChunkUnlockState UnlockState => unlockState;

    [MemoryPackInclude]
    private ChunkLevel chunkLevel;
    public ChunkLevel ChunkLevel => chunkLevel;


    public ChunkDataIngame(int input_ChunkNO, ChunkType input_Type, ChunkUnlockState input_state)
    {
        chunkNo = input_ChunkNO;
        chunkType = input_Type;
        unlockState = input_state;
        chunkLevel = ChunkLevel.Lv1;
    }

    /// <summary>
    /// 청크를 '해금 가능(Unlockable)' 상태로 변경합니다. (주로 인접 청크가 열렸을 때 호출됨)
    /// </summary>
    public ChunkActionResult SetUnlockable()
    {
        switch (unlockState)
        {
            case ChunkUnlockState.Locked:
                {
                    unlockState = ChunkUnlockState.Unlockable;
                    return new ChunkActionResult(ResultType.Success, NewResultMessage("청크 해금 가능 상태로 전환 성공!"));
                }
            case ChunkUnlockState.Unlockable:
                return new ChunkActionResult(ResultType.Failed, NewResultMessage("청크 해금 가능 상태로 전환 실패: 이미 해금 가능한 상태임"));

            default:
                return new ChunkActionResult(ResultType.Error, NewResultMessage($"Unexpected Error has been occured ON SetUnlockable {chunkNo}, {unlockState}"));
        }
    }

    /// <summary>
    /// 청크를 최종적으로 '해금(Unlocked)' 상태로 변경합니다.
    /// </summary>
    public ChunkActionResult Unlock()
    {
        switch (unlockState)
        {
            case ChunkUnlockState.Unlockable:
                {
                    unlockState = ChunkUnlockState.Unlocked; // 수정: Unlockable 상태에서 Unlocked로 변경되도록 논리 수정
                    return new ChunkActionResult(ResultType.Success, NewResultMessage("청크 해금 성공!"));
                }
            case ChunkUnlockState.Unknown:
            case ChunkUnlockState.Locked:
            case ChunkUnlockState.Unlocked:
                return new ChunkActionResult(ResultType.Failed, NewResultMessage("청크 해금 실패: 해금할 수 없는 상태이거나 이미 해금됨"));

            default:
                Debug.LogAssertion($"Unexpected Error has been occured ON Unlock {chunkNo}, {unlockState}");
                return new ChunkActionResult(ResultType.Error, NewResultMessage("청크 해금 실패!"));
        }
    }

    /// <summary>
    /// 청크가 해금된 상태라면, 청크의 레벨업을 시도합니다. 
    /// </summary>
    public ChunkActionResult LevelUP()
    {
        
        if (unlockState != ChunkUnlockState.Unlocked)
            return new ChunkActionResult(ResultType.Error, NewResultMessage($"Unexpected Error: Level has been called but Chunk is not Unlocked"));

        switch (chunkLevel)
        {
            case ChunkLevel.Lv1:
            case ChunkLevel.Lv2:
            case ChunkLevel.Lv3:
                {
                    chunkLevel.Next<ChunkLevel>();
                    return new ChunkActionResult(ResultType.Success);

                    
                }
            case ChunkLevel.Lv4: return new ChunkActionResult(ResultType.Failed, NewResultMessage($"Already Level is Max But LevelUp Callled. ChunkNO : {chunkNo}"));
            default: return new ChunkActionResult(ResultType.Error, NewResultMessage($"Unexpected ChunkLevel : {ChunkLevel}, Chunk NO : {chunkNo}, Chunk Type : {chunkType}"));
        }
    }


    private FixedString128Bytes NewResultMessage(string message)
    {
        //Debug.Log(message);
        return new FixedString128Bytes(message);
    }
}
#endregion


/// <summary>
/// 인게임의 모든 청크 상태(데이터)를 중앙에서 관리하는 매니저 클래스입니다.
/// VContainer에 등록되어 다른 시스템(예: 플레이어 상호작용, 씬 오브젝트 제어)에서 Inject 받아 사용합니다.
/// 
/// [서브 프로그래머를 위한 작업 가이드]
/// 1. 인게임의 벽(Wall)이나 상호작용 오브젝트는 이 매니저를 Inject 받아 상태를 확인해야 합니다.
/// 2. 플레이어가 청크를 해금하면 UnlockChunk()를 호출하세요.
/// 3. 해금 성공 시, 실제 씬에 있는 벽 Collider의 IsTrigger를 true로 바꾸어 이동을 허용하고, 
///    해당 영역의 LayerMask를 갱신하여 농사/건축 등의 상호작용이 가능하도록 물리적 처리를 추가해주세요.
/// </summary>
public class ChunkManager : MonoBehaviour
{
    #region Scriptable Object Datasets (SO 원본 데이터)
    private FarmChunkDataSet _FarmChunkDataSet;
    private FieldChunkDataSet _FieldChunkDataSet;
    private ForestChunkDataSet _ForestChunkDataSet;
    private MineChunkDataSet _MineChunkDataSet;
    #endregion

    #region In-Game Chunk Data Arrays (인게임 가변 데이터)
    private ChunkDataIngame[] _FarmChunkDatas;
    private ChunkDataIngame[] _FieldChunkDatas;
    private ChunkDataIngame[] _ForestChunkDatas;
    private ChunkDataIngame[] _MineChunkDatas;

    private int cachedDatasetLength;
    #endregion

    #region Getter
    public ref ChunkDataIngame[] GetFarmChunkDatas => ref _FarmChunkDatas;
    public ref ChunkDataIngame[] GetFieldChunkDatas => ref _FieldChunkDatas;
    public ref ChunkDataIngame[] GetForestChunkDatas => ref _ForestChunkDatas;
    public ref ChunkDataIngame[] GetMineChunkDatas => ref _MineChunkDatas;

    #endregion

    private SaveLoadManager _saveLoadManager;

    // [Inject]
    // public void Construct(SaveLoadManager saveLoadManager)
    // {
    //     _saveLoadManager = saveLoadManager;
    //     _saveLoadManager.RegisterChunkManager(this);
    // }

    #region Initialization
    private void Awake()
    {
        initalizeScriptableObjectDataset().Forget();
    }

    /// <summary>
    /// Addressable을 통해 각 청크 타입별 SO 데이터를 로드하고 인게임 데이터를 초기화합니다.
    /// </summary>
    private async UniTaskVoid initalizeScriptableObjectDataset()
    {
        _FarmChunkDataSet = await AddressableManager.LoadAssetAsync<FarmChunkDataSet>(FARM_CHUNK_DATASET);
        _FieldChunkDataSet = await AddressableManager.LoadAssetAsync<FieldChunkDataSet>(FIELD_CHUNK_DATASET);
        _ForestChunkDataSet = await AddressableManager.LoadAssetAsync<ForestChunkDataSet>(FOREST_CHUNK_DATASET);
        _MineChunkDataSet = await AddressableManager.LoadAssetAsync<MineChunkDataSet>(MINE_CHUNK_DATASET    );

        if (_FarmChunkDataSet != null) initializeChunkDatas_Farm();
        if (_FieldChunkDataSet != null) initializeChunkDatas_Field();
        if (_ForestChunkDataSet != null) initializeChunkDatas_Forest();
        if (_MineChunkDataSet != null) initializeChunkDatas_Mine();
    }

    #region 청크 데이터 초기화
    private void initializeChunkDatas_Farm()
    {
        cachedDatasetLength = _FarmChunkDataSet.GetLength();
        _FarmChunkDatas = new ChunkDataIngame[cachedDatasetLength];

        for (int i = 1; i <= cachedDatasetLength; i++)
        {
            _FarmChunkDatas[i - 1] = new ChunkDataIngame(i, ChunkType.Farm, ChunkUnlockState.Locked);
        }
        // 첫 번째 청크는 기본적으로 개방되도록 처리
        _FarmChunkDatas[0].SetUnlockable();
        _FarmChunkDatas[0].Unlock();
    }

    private void initializeChunkDatas_Field()
    {
        cachedDatasetLength = _FieldChunkDataSet.GetLength();
        _FieldChunkDatas = new ChunkDataIngame[cachedDatasetLength];

        for (int i = 1; i <= cachedDatasetLength; i++)
        {
            _FieldChunkDatas[i - 1] = new ChunkDataIngame(i, ChunkType.Field, ChunkUnlockState.Locked);
        }
        _FieldChunkDatas[0].SetUnlockable();
        _FieldChunkDatas[0].Unlock();
    }

    private void initializeChunkDatas_Forest()
    {
        cachedDatasetLength = _ForestChunkDataSet.GetLength();
        _ForestChunkDatas = new ChunkDataIngame[cachedDatasetLength];

        for (int i = 1; i <= cachedDatasetLength; i++)
        {
            _ForestChunkDatas[i - 1] = new ChunkDataIngame(i, ChunkType.Forest, ChunkUnlockState.Locked);
        }
        _ForestChunkDatas[0].SetUnlockable();
        _ForestChunkDatas[0].Unlock();
    }

    private void initializeChunkDatas_Mine()
    {
        cachedDatasetLength = _MineChunkDataSet.GetLength();
        _MineChunkDatas = new ChunkDataIngame[cachedDatasetLength];

        for (int i = 1; i <= cachedDatasetLength; i++)
        {
            _MineChunkDatas[i - 1] = new ChunkDataIngame(i, ChunkType.Mine, ChunkUnlockState.Locked);
        }
        _MineChunkDatas[0].SetUnlockable();
        _MineChunkDatas[0].Unlock();
    }
    #endregion

    #endregion

    #region Chunk Operations (해금 로직)
    /// <summary>
    /// 특정 청크를 해금 시도하고, 성공 시 인접한 4방향의 청크들을 '해금 가능(Unlockable)' 상태로 업데이트합니다.
    /// 
    /// TODO (Sub-Programmer): 
    /// 이 메서드가 Success를 반환하면, 씬에 존재하는 해당 청크 번호의 GameObject를 찾아
    /// Collider.IsTrigger = true 로 설정하여 벽을 없애고(또는 통과 가능하게 하고), 
    /// 플레이어 레이캐스트용 LayerMask를 갱신하여 밭갈기 등의 행동이 가능해지도록 처리하는 스크립트를 주입받아 해결하기!
    /// </summary>
    /// <param name="input_Id">해금하려는 청크의 번호</param>
    /// <param name="input_type">해금하려는 청크의 구역 타입 (Farm, Field 등)</param>
    /// <returns>해금 성공/실패 여부 및 결과 메시지(디버그로 출력됨)</returns>
    public ChunkActionResult UnlockChunk(int input_Id, ChunkType input_type)
    {
        ChunkActionResult result;

        switch (input_type)
        {
            case ChunkType.Farm:
                {
                    result = _FarmChunkDatas[input_Id - 1].Unlock();

                    if (result.Result == ResultType.Success)
                    {
                        ChunkData cachedChunkData = _FarmChunkDataSet.getChunk(ref input_Id);

                        // 인접한 4개의 청크 번호를 순회하며 상태 업데이트
                        for (int i = 0; i < 4; i++)
                        {
                            int neighborId = cachedChunkData.ContinguousChunk[i];
                            if (neighborId != 0)
                            {
                                result.Combine(_FarmChunkDatas[neighborId - 1].SetUnlockable());
                            }
                        }
                    }
                    return result;
                }

            case ChunkType.Field:
                {
                    result = _FieldChunkDatas[input_Id - 1].Unlock();

                    if (result.Result == ResultType.Success)
                    {
                        ChunkData cachedChunkData = _FieldChunkDataSet.getChunk(ref input_Id);

                        for (int i = 0; i < 4; i++)
                        {
                            int neighborId = cachedChunkData.ContinguousChunk[i];
                            if (neighborId != 0)
                            {
                                result.Combine(_FieldChunkDatas[neighborId - 1].SetUnlockable());
                            }
                        }
                    }
                    return result;
                }

            case ChunkType.Forest:
                {
                    result = _ForestChunkDatas[input_Id - 1].Unlock();

                    if (result.Result == ResultType.Success)
                    {
                        ChunkData cachedChunkData = _ForestChunkDataSet.getChunk(ref input_Id);

                        for (int i = 0; i < 4; i++)
                        {
                            int neighborId = cachedChunkData.ContinguousChunk[i];
                            if (neighborId != 0)
                            {
                                result.Combine(_ForestChunkDatas[neighborId - 1].SetUnlockable());
                            }
                        }
                    }
                    return result;
                }

            case ChunkType.Mine:
                {
                    result = _MineChunkDatas[input_Id - 1].Unlock();

                    if (result.Result == ResultType.Success)
                    {
                        ChunkData cachedChunkData = _MineChunkDataSet.getChunk(ref input_Id);

                        for (int i = 0; i < 4; i++)
                        {
                            int neighborId = cachedChunkData.ContinguousChunk[i];
                            if (neighborId != 0)
                            {
                                result.Combine(_MineChunkDatas[neighborId - 1].SetUnlockable());
                            }
                        }
                    }
                    return result;
                }

            default:
                return new ChunkActionResult(ResultType.Error, NewResultMessage($"Unexpected Error on UnlockChunk. target ChunkType :{input_type}, targetNo : {input_Id} "));
        }
    }

    /// <summary>
    /// 현재 결과 문자열을 로그에 남기고 고정 크기의 문자열로 반환합니다!
    /// </summary>
    /// <param name="message">출력하고 동시에 반환받을 메세지</param>
    /// <returns>입력만 문자열을 반홥합니다</returns>
    private FixedString128Bytes NewResultMessage(string message)
    {
        Debug.Log(message);
        return new FixedString128Bytes(message);
    }
    #endregion
}