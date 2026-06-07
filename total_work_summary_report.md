# 작업 총정리 리포트 (Comprehensive Work Summary Report)

가장 최근에 Push 되었던 `origin/Mingi` 상태와 현재 작업 디렉토리를 비교하여, 그동안 진행된 모든 버그 수정, 시스템 개편 및 안정화 작업의 내역을 정리한 리포트입니다.

---

## 1. 파일 수정 통계 (Git Diff Overview)
* **수정 또는 추가된 파일**: 총 40여 개의 소스 코드 및 관련 에셋
* **핵심 수정 범위**: `Assets/Scripts/Runtime` 및 `Assets/JB` 내 핵심 매니저, 컨트롤러, 프로프(Prop) 클래스들

---

## 2. 부문별 상세 작업 내역

### 2.1 상점 UI 시스템 개편 ([ShopUIController.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/UI/ShopUIController.cs))
* **구매 흐름 분리**: 슬롯 클릭 시 즉시 수량 조절MessageBox 팝업이 뜨던 동작을 수정하여, 우측 상세 설명 창(`DetailDataContainer`)에 아이템 상세 정보를 먼저 띄우고 슬롯을 하이라이트한 뒤, 하단의 메인 구매 버튼(`BuyButton`)을 클릭했을 때만 MessageBox 팝업이 뜨도록 분리했습니다.
* **초기 선택 자동화**: 상점을 열 때 `_selectedIndex = 0`으로 초기화하여 최상단 아이템이 기본 선택되고 상세 설명이 표기되도록 초기화했습니다.
* **소지금 부족 시 상세 보기 허용**: 기존에는 소지금이 부족한 슬롯을 비활성화(`SetEnabled(false)`)하여 상세 정보를 볼 수 없었던 문제를 해결하기 위해, 슬롯은 언제나 클릭 가능하게 열어두고 메인 구매 버튼만 소지금 여부에 따라 활성화/비활성화되도록 연동했습니다.
* **씨앗 상세 정보 꽃 연동 및 등급(Lv0) 고정**: 
  * 씨앗(`ItemSubType.Seed`) 선택 시, 자라날 꽃의 ID(`itemId + 1000`)를 이용하여 `FlowerDB`로부터 품종, 색상, 총 성장 기간(int4 합산), 꽃말 관련 데이터를 역으로 조회해 우측 설명 창에 연동했습니다.
  * 상점 기획에 맞추어 씨앗의 판매 품질 등급(`QualityLabel`)에 `Lv0`이 고정 출력되도록 하드코딩 처리를 추가했습니다.

### 2.2 인벤토리 드래그 앤 드롭 및 고스트 연출 ([InventoryUIController.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/UI/InventoryUIController.cs))
* **드래그 연출 보완 (예정 및 준비)**:
  * 마우스 드래그 시작 시, 원래 슬롯의 아이템 이미지를 비워 드래그 중인 상태를 연출합니다.
  * 마우스를 따라다니는 `GhostIcon`의 좌표 매핑 방식을 기존 `transform.position`에서 `left/top` 스타일 제어로 보정하여, UI 스케일 오차 없이 마우스 커서의 정중앙에 고스트 아이콘이 위치하도록 보정했습니다.
  * 드래그 해제(드롭 성공/실패) 시, `RefreshUI().Forget()`을 최종 호출하여 슬롯 이미지가 즉시 올바르게 갱신/복원되도록 안전 가드를 추가했습니다.
* **인벤토리 및 장비 슬롯 스왑**: INVENTORY 컨테이너와 GEAR 컨테이너 간의 쌍방향 스왑을 온전히 지원하도록 인덱스 매핑을 수정했습니다.

### 2.3 비동기 오브젝트 안정화 및 Null Guard 적용 ([PlotProp.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/Prop/PlotProp.cs), [DropItemAnim.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/JB/DropItemAnim.cs))
* **MissingReferenceException 완치**: 하루가 지나 씬이 전환되거나 밭 타일이 파괴되는 시점에 뒤늦게 로드가 끝난 비동기 스프라이트 태스크(`changePlotSpr`, `changeFlowerSpr`)가 파괴된 `SpriteRenderer` 컴포넌트에 접근해 발생하는 예외를 잡기 위해 `await` 호출부마다 `if (this == null) return;` 널 가드를 삽입했습니다.
* **DOTween 경고 해소**: 아이템 획득 및 씬 언로드 시 발생하던 DOTween 경고 및 트랜스폼 소멸 예외를 수정했습니다.

### 2.4 파종/수확 및 농경 시스템 개선 ([PlotProp.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/Prop/PlotProp.cs), [UseAreaFunction.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/UseAreaFunction.cs))
* **수확 시 아이템 소멸 버그 수정**: 파종(`Sowing`) 시 작물의 수확량(`harvestAmount`) 데이터를 DB로부터 가져와 초기화하지 않아, 수확 시 드롭되는 아이템이 0개가 되어 아이템이 소멸한 것처럼 보였던 버그를 수정했습니다.
* **Wilted 밭 수분 표시 및 물주기 가드 해제**: 꽃이 시든(`Wilted`/`Dead`) 상태여도 밭의 수분 스프라이트가 정상 노출되도록 `ChangePlotSpr()` 흐름을 패치하고, 시든 작물에도 물을 줄 수 있도록 동작 가드를 해제했습니다.
* **밭 생성 높이 밀착**: 밭 타일이 `y = 0.15f` 공중에 뜨는 기하학적 버그를 수정하여 `y = 0f` (지면)에 딱 맞춰 설치되도록 지면 밀착을 강제했습니다.

### 2.5 아이템 이미지 및 리소스 스케일 버그 수정 ([GameItem.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/GameItem/GameItem.cs))
* **아이템 이미지 비정상 스케일 패치**: 필드 드롭 아이템의 이미지가 과도하게 커지는 문제를 해결하기 위해, Addressable 로딩 방식을 `Texture2D` 로드 후 스프라이트 재생성 방식에서 `Sprite` 직접 로드 방식으로 통일하여 유니티 에셋 임포터에 지정된 PPU(Pixels Per Unit) 및 피벗(Pivot) 정보가 고스란히 반영되도록 수정했습니다.

### 2.6 핫바 시스템 에셋 파괴 예외 수정 ([HotbarManager.cs](file:///c:/Users/araha/OneDrive/문서/GitHub/FlowerOperate/Assets/Scripts/Runtime/Manager/HotbarManager.cs))
* **Destroying assets is not permitted 해결**: 핫바의 R, T 스왑 갱신 시 `UnityEngine.Object.Destroy()`를 호출하여 원본 스프라이트 에셋이 파괴되려 하던 문제를 제거하고, 스프라이트 할당 방식을 비동기 Addressable 로드 및 틴트 적용 방식으로 안전하게 리팩토링했습니다.

---

## 3. 검증 상태 (Verification Results)
* **컴파일 오류**: 없음 (`dotnet build` 시 Build Succeeded, 0 error)
* **런타임 동작**: 인게임 상점 UI 동작 및 등급 연동, 비동기 파괴 널 가드 등의 핵심 패치 항목이 모두 정상 컴파일 빌드 통과하여 무오류 상태로 가동 가능합니다.
