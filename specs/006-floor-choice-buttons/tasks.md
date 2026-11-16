# Tasks: 바닥에서 올라오는 선택지와 진행 버튼

**Input**: `specs/006-floor-choice-buttons/`의 [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/floor-buttons.md](contracts/floor-buttons.md), [quickstart.md](quickstart.md).

**Branch**: 기존 `main` 유지. 현재 문서는 작업 목록이며 체크되지 않은 항목을 구현/검증 완료로 취급하지 않는다.

**Tests**: 명세의 사용자 시나리오/성공 기준과 계획의 검증 클래스를 작업으로 포함한다. 상태/입력 검사는 구현 전에 실패를 확인한다. 시각 검사는 실제 실행 결과로 별도 확인한다.

**Organization**: Setup -> 공통 기반 -> US1(P1) -> US2(P1) -> US3(P2) -> 공통 검증. 파일 경로는 저장소 루트 기준이다. 신규 Unity 자산과 폴더는 `.meta`를 함께 보관한다. `[P]`는 명시한 선행 작업 후 서로 다른 파일에서 병렬로 수행할 수 있는 작업이다.

## 공통 제약

Unity 6000.3.26f1, 기존 uGUI/TMP/XRI/Input System/OpenXR/URP를 유지한다. 새 패키지, 교육 내용, 문구, 음성, Domain 판단, 물리 장애물은 추가하지 않는다. 사용자 확정 외형은 판만 상승/하강하는 C안이며 특수 효과/기둥/받침대/개폐구는 제외한다. 크기/시간은 plan의 시작값이며 최종 외관 승인이 아니다.

검증 자료 경로를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/`로 정하고 아래에서 `verification.md`, `execution.mp4`라고 하면 이 작업별 폴더의 파일을 뜻한다. 기존 폴더는 덮어쓰지 않는다. 구현 중에도 각 시나리오의 실제 실행 자료를 축적한다. 실행 환경/결과/미검증을 구분하며 자료는 Git 추적/제출 자산에서 제외한다.

## Phase 1: Setup

**Goal**: 기존 설정과 검사 경로를 확보한다. 프로젝트 재생성/의존성 변경은 없다.

- [X] T001 `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `Assets/_02LWS/Scenes/FireNotification.unity`의 현재 상태와 사용자 변경을 확인하고 기존 컴파일/SharedUIContractAudit/RouteDomainChecks 결과, 세 체험의 버튼 문구/실행 시점을 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/verification.md`에 기준 자료로 기록한다. 실패는 기존 문제와 이번 변경을 구분한다.
- [X] T002 `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에 기존 RouteIntegrationChecks와 같은 Editor 전용 구성/비동기 실행 검사 진입점 `ValidateConfiguration()`, `RunPlayMode()`, `Running/Passed/Failed/EvidenceDirectory`를 마련한다. 예외/타임아웃을 실패로 기록하고 실제 Editor/합성 입력/실제 레이 결과를 분리한다.

## Phase 2: Foundational

**Goal**: 공용 위치/표시 인터페이스와 두 Canvas 역할을 준비한다. 모든 사용자 시나리오의 선행 조건이다.

- [X] T003 [P] `Assets/_02LWS/Scripts/Runtime/FloorButtonAnchor.cs`에 data-model의 위치/조정값을 구현한다. 제약을 그대로 적용한다: transform은 “머리/컨트롤러 자식 금지, scale=1”, id는 “씬 내 고유 식별자, 빈 값/중복 금지”, raisedBottomHeight/floorClearance는 “0 이상”, riseSeconds/lowerSeconds는 “유한한 양수”, “모든 float는 유한해야 한다. 너비/높이는 양수이고 간격은 두 항목을 담을 수 있는 범위다.” minimumHeight는 “preferred height보다 작게 자르지 않음”으로 표시 측과 연동한다. plan의 시작값과 방향 gizmo를 제공한다.
- [X] T004 [P] `Assets/_02LWS/Prefabs/UI/FloorButtonPanel.prefab`에 World Space Canvas, 고정 FloorViewport/RectMask2D, MovingPanel, 기존 테마/TMP/FreshPressButton을 사용하는 재사용 버튼 2개를 만든다. TrackedDeviceGraphicRaycaster/Editor 마우스용 GraphicRaycaster/CanvasGroup을 포함하고 raycastTarget은 실제 항목에만 부여한다. 콜라이더/특수 셰이더/새 입력 자산을 추가하지 않는다.
- [X] T005 `Assets/_02LWS/Scripts/Runtime/FloorButtonPresenter.cs`에 contracts C1의 `Present(owner,ticket,anchor,actions,select)`, `SetBlocked(bool)`, `ResetDisplay()`와 요청/표시 스냅샷/최신 pending 저장 경계를 만든다. data-model의 “표시 스냅샷 1개와 최신 대기 요청 최대 1개”, “progress: 0..1”, “다른 owner/실행의 동일 ID는 같은 요청이 아니다”를 적용한다. 빈 요청/잘못된 앵커/비활성 현재 공간을 구별하고 초기 상태는 Hidden으로 둔다.
- [X] T006 `Assets/_02LWS/Scenes/FireNotification.unity`의 SharedSystems에 공용 SharedFloorButtons 인스턴스를 연결하고, `Assets/_02LWS/Scripts/Runtime/SharedUIContractAudit.cs`와 `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`를 갱신해 지정 Canvas 2개/Presenter 1개/내레이터 1개/EventSystem 1개, 콘텐츠별 Canvas/입력 0개를 검사한다. 바닥판은 ComfortFollowUI 밖에 두고 XR 카메라/기존 입력/벽과 바닥 occlusion mask를 연결한다. 비저장 Editor 미리보기는 저장 구성 검사에서 제외한다.

**Checkpoint**: 공통 인터페이스 컴파일과 저장 씬 구성 검사 통과. 이 시점은 바닥 연출/조작 완료가 아니다.

## Phase 3: US1 - 사건과 바닥에서 올라오는 버튼을 함께 보기 (P1)

**Goal**: 모든 대상 단계의 판을 고정 위치에 표시하고 사건/문구를 함께 확인한다.

**Independent Test**: 기존 선택 단계에 진입해 숨김 -> 상승 -> 완전 표시를 확인한다. 같은 버튼이 기존 화면에 없고 반대 방향을 보고 진입해도 등장하며 이동/고개 회전 후 공간 위치가 고정돼야 한다. US1만으로 클릭 동작/전체 체험 완료를 선언하지 않는다.

### 검증 정의

- [X] T007 [US1] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에 Q1/Q2/Q4의 실패 검사를 먼저 추가한다. 숨김/상승/Ready, 동일 요청 반복 시 상승 재시작 0, 빈 요청의 빈 판 0, 단일/두 문구, 배율 1/1.5, 반대 방향 진입, 고정 위치, 이전 Actions 중복 0을 검증하고 미구현 상태의 실패를 확인한다.

### 구현

- [X] T008 [P] [US1] `Assets/_02LWS/Scripts/Runtime/FloorButtonPresenter.cs`에 Hidden/Rising/Ready/Lowering 이동과 고정 앵커 적용을 구현한다. 수직 이동은 바닥 mask 안에서 수행하고 실제 Canvas 단위 변환을 한 곳에서 처리한다. 한 항목 가운데/두 항목 좌우 배치, TMP preferred size와 배율 1/1.5, 동등한 스타일을 적용하며 머리/시선 추종과 프레임별 생성/파괴를 금지한다. 클릭 수락은 US2 전까지 비활성으로 둔다.
- [X] T009 [P] [US1] `Assets/_02LWS/Editor/FloorButtonAnchorEditor.cs`에 앵커의 위치/방향/기울기/크기/시간 조정과 숨김/완전 상승/해제 미리보기를 구현한다. 실제 판 프리팹을 Editor 전용 비저장 객체로 표시하고 교육/음성/입력을 실행하지 않으며 선택 변경/Play Mode 진입/재로드에 정리한다.
- [X] T010 [US1] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`에 위치를 받는 SetActions 오버로드와 공용 Presenter 위임을 연결한다. `IsFloorExitComplete(owner)` 경계도 선언하되 정상 수락 판의 완료 구현 전에는 false로 두어 T013의 연결이 컴파일되게 한다. 기존 오버로드는 빈 요청에 사용하고 위치 없는 비어 있지 않은 요청은 구성 오류로 처리한다. 기존 Actions 행 생성/예약 높이를 제거하되 status/subtitle 레이아웃과 설정을 유지한다. ContentUIAction의 “묶음 내 ID 유일, 공백 문구/null 콜백 금지. 행동 선택 최대 2개/진행 버튼 1개”를 유지한다.
- [X] T011 [P] [US1] `Assets/_02LWS/Scripts/Runtime/FireNotificationAdapter.cs`에 선택/다음 앵커 참조를 추가하고 AwaitAction shout/wait와 AwaitNext next의 기존 요청 시점/순서/문구를 유지해 위치를 전달한다. 기본으로 같은 앵커를 사용할 수 있게 하며 기존 교육 콜백은 변경하지 않는다.
- [X] T012 [P] [US1] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`에 준비 앵커 참조를 추가하고 AwaitingReady의 ready 요청에 전달한다. 기준 높이는 기존 Ready()가 실제 호출될 때 읽고 저장하도록 유지하며 상승 완료에 준비를 실행하지 않는다.
- [X] T013 [P] [US1] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`에 A 준비 앵커와 targetId 매핑을 추가해 ready/use/another/next 요청에 전달한다. “경로 a-left/a-right/a-lift/b-left/b-right/b-lift의 누락/중복은 오류다.”를 검사하고 “저장 구성 검사에서 비활성 B 공간 참조는 허용하지만 현재 표시 요청은 활성 공간에 속해야 한다.”를 적용한다. A 다음 연습은 완료 계단 앵커를 사용하며 B 준비/다음 버튼은 새로 만들지 않는다. next 요청 수락 후 실제 Transition은 현재 owner/run/execution과 `view.IsFloorExitComplete(this)`가 일치하는 정상 하강 완료까지 기다리도록 연결한다. 클릭/완료 신호는 US2에서 완성한다.
- [X] T014 [US1] `Assets/_02LWS/Scenes/FireNotification.unity`에 화재 1개/연기 준비 1개/경로 A 준비 1개/경로 판단 6개의 초기 FloorButtonAnchor를 배치하고 어댑터 참조를 연결한다. a-w3 준비와 6개 대상 확인 지점의 사건/계단/문/바닥/통로/텔레포트 지점 가림을 조정하고 기존 ContentPanel/Actions 중복 템플릿/버튼을 제거한다. 화재 다음의 가림이 있으면 선택 앵커와 분리한다.
- [X] T015 [US1] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`의 Q1/Q2/Q4와 저장 씬 재열기/미리보기 정리 검사를 실행한다. US2 전에는 기존 검사 도우미로 필요한 단계에 진입해 표시만 확인하고 UI 클릭 합격으로 기록하지 않는다. `specs/006-floor-choice-buttons/spec.md`의 배치 검사 기준 표와 quickstart 관찰 지점 매핑을 사용해 좌표/방향/자세별 머리 높이를 먼저 기록하고 기본 100%/확대 150%에서 반복한다. 화재/경로는 기존 지원 서기/앉기, 연기 준비는 서기를 검사하며 미지원 자세는 근거와 제외 사유를 적는다. 사건 가림/관통/잘림/겹침 0을 실제 화면으로 확인해 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/verification.md`에 기록한다.

**Checkpoint**: 위치/표시 검토용 MVP. 배치 검토 전 외관을 사용자 승인으로 표시하지 않는다.

## Phase 4: US2 - 기존 행동을 공간의 선택지로 고르기 (P1)

**Goal**: 레이/트리거로 한 번 실행하고 하강하며 기존 교육 결과/오답 재선택/진행을 보존한다.

**Independent Test**: 양쪽 행동을 각각 선택해 기존 결과/오답 재선택을 비교한다. 준비 완료의 실제 클릭 시점 높이, 다음/다음 연습의 기존 전환 조건, 중복 실행 0을 확인한다.

### 검증 정의

- [X] T016 [US2] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에 C1/C2/Q3 실패 검사를 먼저 추가한다. non-owner/이전 ticket, Rising/Lowering 입력, 클릭 잠금 후 동기 빈 요청+새 요청, 최신 pending 1개, 좌우 동시/연타, 기존 컨트롤러 거부 후 같은 단계 재제공, 오답 재등장, 유효 입력 없는 자동 진행 0과 클릭 순간 높이 저장을 고정한다. 다음 연습은 현재 A에서 하강 완료 전 이동 0/정상 완료 후 1회이며 ResetDisplay는 완료가 아닌지 검사하고 미구현 실패를 확인한다.

### 구현

- [X] T017 [US2] `Assets/_02LWS/Scripts/Runtime/FloorButtonPresenter.cs`에 Ready 클릭의 원자적 consumed/전체 항목 잠금 -> 하강 시작 -> select(ticket,id) 즉시 1회 호출을 구현한다. 내려가는 문구/위치를 불변 스냅샷으로 보존하고 동기 재진입에도 최신 pending만 보관해 하강 후 표시한다. 현재 수락 ticket의 정상 하강 완료 표식을 제공하고 ResetDisplay는 스냅샷/완료 표식을 폐기한다. 준비 완료와 다른 버튼 콜백을 지연하지 않으며 다음 연습의 실제 공간 이동만 완료 표식 이후로 제한한다.
- [X] T018 [US2] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`에 owner 활성/Blocked/메뉴/로딩/ticket/현재 행동의 실행 관문을 완성한다. 같은 요청 반복은 무시하고 실행 문맥 변경/빈 요청 뒤 재요청은 새 ticket을 발급한다. 콜백이 기존 조건 검사에서 거부돼 같은 단계가 남으면 하강 후 최신 요청을 재확인해 새 ticket으로 제공하고 이전 listener는 거부한다. `IsFloorExitComplete(owner)`는 현재 owner의 마지막 수락 판의 정상 하강 완료만 반환하게 하고 빈 요청/단순 Hidden/ResetDisplay를 완료로 처리하지 않는다.
- [X] T019 [US2] `Assets/_02LWS/Scripts/Runtime/FloorButtonPresenter.cs`에서 기존 FreshPressButton.CanSelect를 Ready/미처리/가용 조건에 연결하고 Rising -> Ready에 InvalidatePress를 호출한다. 상승 중 누른 채 유지한 입력과 이전 클릭이 새 묶음에 이어지지 않게 양손 해제 후 새 누름만 수락하며 `Assets/_02LWS/Scripts/Interaction/FreshPressButton.cs`는 가능한 그대로 재사용한다.
- [X] T020 [US2] `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`의 공용 글자 배율/테마 적용과 XR 활성 여부에 따른 마우스 raycaster 정책을 바닥판에도 연결한다. 설정 변경 시 판 크기를 재계산하되 위치 재정렬/텔레포트로 바닥 앵커가 이동하지 않게 한다. 자막/메뉴/확인/결과 버튼 동작은 보존한다.
- [X] T021 [US2] `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`의 버튼 조회를 공용 바닥 actionRoot로 갱신하고 Ready/새 입력 해제를 기다린 뒤 기존 pointer 검사를 수행하게 한다. 기존 교육 결과/오래된 콜백 검사는 유지하며 인위적인 시간 대기만으로 상승 완료를 추정하지 않는다.
- [X] T022 [US2] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에 한 버튼/두 버튼 각각의 양손 실제 TrackedDeviceGraphicRaycaster 적중 -> 기존 이벤트 모듈 -> 콜백 1회 검사를 추가한다. 벽 가림 클릭 차단과 3D occlusion mask를 확인하고 pointer 직접 호출 검사는 실제 레이 검사와 구분한다.
- [X] T023 [US2] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`와 `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`를 실행해 전 선택 항목/오답/연기 준비/화재 다음/경로 다음 연습을 회귀 확인한다. 상승 때와 클릭 때 머리 높이를 달리해 클릭 순간 저장을 검증한다. next는 A에서 정상 하강을 끝내기 전 구역 이동 0/완료 후 이동 1회이고 다른 행동의 기존 실행 시점/결과/순서는 유지돼야 한다. 중복/유효 입력 없는 자동 진행 0을 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/verification.md`에 기록한다.

**Checkpoint**: 정상 플레이의 표시/선택/하강/재선택을 검증할 수 있다. 중단/전환의 전체 합격은 US3에서 확인한다.

## Phase 5: US3 - 중단과 체험 전환 뒤 올바른 선택지만 보기 (P2)

**Goal**: 일시정지 상태를 보존하고 재시작/체험 및 구역 전환에 잔류 표시/입력을 제거한다.

**Independent Test**: Rising/Ready/Lowering 각각에서 pause/resume, restart/exit/다른 체험/B 전환을 수행한다. 재개 위치/안내 재생 지점 보존, 이전 판/입력 오실행 0이어야 한다.

### 검증 정의

- [X] T024 [US3] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에 C4/Q5 실패 검사를 먼저 추가한다. 세 표시 상태의 Menu pause/resume, 정지 중 누름/유지 후 재개, Esc, 추적/focus 차단 복구, restart/exit/owner 변경/B 전환 후 이전 listener 호출, 이동 후 공간 위치 고정을 검증하고 미구현 실패를 확인한다.

### 구현

- [X] T025 [US3] `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`와 `Assets/_02LWS/Scripts/Runtime/FloorButtonPresenter.cs`에 메뉴/owner.Blocked/로딩 가용성을 연결한다. 이동 delta=0과 phase/progress 보존, 모달 아래 렌더링/raycast 숨김, 정지 진입/해제 InvalidatePress, 해제 후 새 누름을 구현한다. 전역 timescale을 바꾸거나 기존 음성을 처음부터 재생하지 않는다.
- [X] T026 [US3] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`의 Bind/Clear/비활성 경계와 세 어댑터의 Stop/Restart 경로에서 ResetDisplay와 ticket 무효화를 보장한다. `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`의 로딩/owner 전환 중 이전 입력도 막고 이전 판/대기 요청/콜백을 즉시 정리한다. 같은 owner로 재시작해도 새 실행을 구별한다.
- [X] T027 [US3] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`의 정상 A -> B Transition은 T013/T017/T018의 현재 수락 판 하강 완료 후에만 수행하도록 완성한다. 하강 중 pause/차단은 A 공간과 대기 이동을 보존하고 재시작/종료/owner 또는 실행 변경은 대기 이동을 취소한다. 정상 하강을 마친 뒤 잔류 상태를 리셋하고 한 번만 이동하며 A 판 잔류/B 준비 또는 다음 버튼 추가가 없어야 한다. 강제 리셋을 하강 완료 신호로 처리하지 않는다.
- [X] T028 [US3] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`의 Q5/Q6와 기존 경로 회귀 검사를 실행한다. 왼쪽 Menu/PC Esc/정지 중 설정/음성 재개 지점/시스템 복구/모든 정리 경계를 확인하고 잔류/오실행 0 결과를 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/verification.md`에 기록한다.

**Checkpoint**: 세 사용자 시나리오의 동작 검증을 완료할 수 있다. 기기/외관/소리 확인은 아래 결과와 구분한다.

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T029 `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에서 Q1~Q7 전수 검사와 제외 UI의 상승/하강 오적용 0, 좌우 입력/확대 글자/가림/preview 정리를 최종 실행한다. `Assets/_02LWS/Scenes/FireNotification.unity`를 저장/재열기해 감사 오류/중복 Canvas/임시 객체/누락 스크립트 0을 확인하고 수정이 생긴 경로만 재검사한다.
- [X] T030 `specs/006-floor-choice-buttons/quickstart.md`의 녹화 절차로 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/execution.mp4`를 실제 1920x1080 이상/30fps 이상으로 녹화하고 세 체험의 상승/선택/하강, 오답/진행/정지 구간을 남긴다. Unity 게임 소리만 포함하며 신규/변경 녹음 설정은 다른 앱 유음 중 Unity 무음/유음으로 분리를 검증한다. 저장 영상을 직접 재생해 문구 식별/게임 소리/동기를 확인한다. 불가 항목은 헌법의 최선 영상/단계별 실제 스크린샷/분리 녹음 대체와 사유/미검증을 verification.md에 기록하고 기준 충족으로 표시하지 않는다.
- [X] T031 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/verification.md`에 전 앵커의 실제 높이/너비/기울기/시간과 제작자의 배치 검토/수정 요청을 기록하고 `Assets/_02LWS/Scenes/FireNotification.unity`의 조정 후 가림/관통/잘림을 재확인한다. 사용자의 실제 검토가 없으면 미검토로 남기며 외관 승인 완료를 만들지 않는다.
- [X] T032 작업별 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/verification.md`에 Quest 실기의 전 위치 가독성/거리/기존 지원 자세/실제 양손 입력/Menu와 동일 기기/설정/구간의 변경 전후 CPU/GPU frame time을 기록한다. 기기가 없거나 실행할 수 없으면 사유와 Quest 미검증을 명시하며 Editor 결과로 기기 합격을 대신하지 않는다.
- [X] T033 `Assets/_02LWS/Scenes/FireNotification.unity`, `Assets/_02LWS/Prefabs/UI/FloorButtonPanel.prefab`와 신규 스크립트의 직렬화 참조/.meta/제출 범위를 확인한다. 검증 자료가 Assets/제출 자산에 섞이지 않았는지, `.gitignore`의 Artifacts 제외가 적용되는지 git check-ignore/git ls-files로 검사하고 신규 패키지/외부 프로젝트 자산 참조가 없는지 기록한다. 이 작업에서 별도 제출 패키지를 만들지는 않는다.
- [X] T034 `specs/006-floor-choice-buttons/quickstart.md`를 실제 검증 진입점/결과에 맞춰 갱신하고 `specs/006-floor-choice-buttons/tasks.md`의 완료 여부를 증거와 대조한다. 문서/자산 diff 검사 후 대표 execution.mp4 또는 대체 파일을 직접 링크하고 Editor/Quest/소리/제작자 검토의 한계를 각각 보고한다. 미검증/미검토를 검증 통과로 바꾸지 않는다. 커밋/push는 별도 요청 없이 수행하지 않는다.

## Dependencies & Execution Order

```text
T001 -> T002 -> (T003 || T004) -> T005 -> T006
  -> US1: T007 -> (T008 || T009) -> T010
     -> (T011 || T012 || T013) -> T014 -> T015
  -> US2: T016 -> T017 -> T018 -> T019 -> T020 -> T021 -> T022 -> T023
  -> US3: T024 -> T025 -> T026 -> T027 -> T028
  -> T029 -> T030 -> T031 -> T032 -> T033 -> T034
```

각 시나리오는 표시/조작/중단을 같은 공용 파일에서 단계적으로 완성하므로 시나리오 전체를 병렬 구현하지 않는다.

US1은 공통 기반 이후 표시를 독립 검증할 수 있다. US2는 US1의 표시/위치를, US3는 US2의 입력/하강을 전제로 한다. 뒤 시나리오의 완료를 앞 시나리오의 검증 조건으로 삼지 않는다. 공용 파일/같은 씬/실행 중인 Editor 조작은 순차 처리한다.

## Parallel Examples

| 구간 | 병렬 가능한 작업 | 합류 조건 |
|---|---|---|
| 기반 | T003 Anchor 스크립트 / T004 판 프리팹 | T005 전 둘 다 완료, 프리팹의 실제 씬 연결은 T006에서 순차 실행 |
| US1 표시 | T008 Presenter 연출 / T009 Editor 미리보기 | T007 이후, T003~T006의 공통 인터페이스 사용, T010 전 합류 |
| US1 콘텐츠 | T011 화재 / T012 연기 / T013 경로 | T010 이후, 다른 어댑터 파일만 수정, 씬 연결은 T014에서 한 번에 수행 |
| US2 | 병렬 표시 없음 | Presenter/SharedContentUI/같은 검증 파일과 실행 상태를 순차 변경 |
| US3 | 병렬 표시 없음 | pause/reset/구역 전환과 같은 실행 상태를 순차 검증 |

`[P]` 7개는 병렬 가능성 표시이며 실제 에이전트 실행/별도 작업자를 요구하는 지시가 아니다. 하나의 작업자도 같은 의존성 순서로 처리할 수 있다.

## Implementation Strategy

**표시 MVP**: Phase 1~3을 완료해 판의 상승과 사건별 배치를 실제 화면에서 확인한다. US1만으로 선택 가능한 완성 체험을 배포하거나 기존 기능 유지 완료로 표시하지 않는다.

**기능 확장**: US2에서 기존 행동/진행/오답/하강을 연결하고, US3에서 정지/리셋/전환을 마무리한다. 각 checkpoint는 해당 이야기의 독립 검증이며 전체 기능 완료는 마지막 공통 검증까지 요구한다.

**작업 완료 기준**: 각 검사는 실패 -> 구현 -> 통과 증거를 남긴다. 외관 검토/Quest 등 외부 조건이 없으면 가능한 확인과 미검증 기록을 분리한다. 검증/제작자 승인 없이 전체 수용 기준 충족을 선언하지 않는다.

## Requirement Coverage

| 요구사항/계약 | 작업 |
|---|---|
| FR-001, FR-002 / C3 등장과 중복 제거 | T004, T008, T010~T015 |
| FR-003, FR-004, FR-005 / C3 위치/시선 | T003, T011~T015 |
| FR-006 / C1/C2 교육 보존 | T010~T013, T016~T018, T023 |
| FR-007, FR-008 / C2 입력/하강/재선택 | T016~T019, T021~T023 |
| FR-009 / C4 생명주기 | T024~T028 |
| FR-010 / C3 가독성/가림 | T003, T008, T014, T015, T020, T022, T029, T031, T032 |
| FR-011 / C4 제외 UI/Menu | T010, T020, T025, T028, T029 |
| FR-012 / C5 제작/감사 | T003, T006, T009, T015, T031 |
| FR-013 / C2 실행 시점 | T012, T013, T016~T019, T023, T027 |
| SC-001~SC-007 | T015, T023, T028~T032 |
| 헌법 I/II/III | T001, T015, T023, T028, T030, T033, T034 |

**Count**: 총 34개. Setup 2개, Foundational 4개, US1 9개, US2 8개, US3 5개, 공통 마무리 6개. 모든 체크박스는 최초 미완료 상태다.

## Phase 7: Convergence

현재 명세/계획/기존 T001~T034와 소스 및 로컬 검증 자료를 대조한 결과다. 기존 체크박스는 변경하지 않는다. 아래 작업은 검증 도구와 누락된 검사 범위를 보완하며 새 교육 행동이나 외관 결정을 추가하지 않는다. Quest/제작자 외관 검토/청취의 미검증 기록을 합격으로 바꾸지 않는다.

- [X] T035 [MEDIUM] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`와 `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`의 검사 결과 저장을 실행별 고유 경로로 분리하라. 현재 `RunPlayMode`는 설정된 EvidenceDirectory를 재사용하고 floor-checks.txt/integration.txt/xr-rays.txt/fresh-input.txt를 WriteAllText로 덮어쓸 수 있다. 공통 Artifacts/Validation/VisualEvidence 아래에서 기본 경로와 명시 경로 모두 기존 결과가 있으면 접미사 또는 새 실행 하위 폴더를 사용하고 실제 결과 경로를 노출하라. 같은 요청 경로로 두 번 실행해 이전 결과의 바이트가 보존되고 두 실행을 구별할 수 있음을 검사하라. per tasks: 공통 제약의 기존 검증 폴더 덮어쓰기 금지, T002/T030, Constitution III 저장 규칙 (partial, F1).

- [X] T036 [MEDIUM] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`의 누락된 입력/수명 회귀 검사를 보완하라. 반대 방향을 먼저 본 상태에서 단계에 진입해 정상 상승하는지 검사하고, non-owner의 SetActions/Clear 및 owner 교체 후 이전 listener를 거부하는지 확인하라. 실제 Input System의 Esc 입력으로 정지/재개를 검사하고 Rising/Ready/Lowering 각각에서 재시작/종료 후 잔류 표시와 이전 입력 실행이 없는지 확인하라. 다음 연습을 실제 수락해 Lowering에 들어간 뒤 사전 ResetFloorDisplay 없이 재시작/종료하여 대기 B 이동이 취소되는지 각각 검사하라. 현재 Menu 검사는 정지 후 관찰한 임의 상태를 기준으로 삼으므로 요청한 표시 상태에서 정지가 걸렸다는 선행 조건도 명시적으로 단언하라. 구성/교육 단계 진입 보조와 실제 입력 검사를 구분하고 새 실행 자료에 결과를 남겨라. per T007/T016/T024/T027/T028, US1/AC3, US3/AC1~3, FR-004/FR-007/FR-009/FR-013, SC-004/SC-005 (partial, F2).

- [X] T037 [MEDIUM] `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`에 `FloorButtonAnchor`의 비영 기울기를 조정한 배치 검사를 추가하고 `FloorButtonPresenter.cs`/`FloorButtonAnchorEditor.cs`의 실제 표시와 미리보기를 대조하라. 현재 배치 기록은 모든 앵커 0도이며, 계획에서 요구한 기울인 판의 바닥 경계/가림 검증이 없다. 대표 pitch/roll을 검사 입력으로 정하고 한 버튼/두 버튼, 글자 100%/150%의 숨김/상승/Ready/하강에서 mask의 바닥 경계, 문구/선택 영역 잘림, 바닥 관통과 사건 가림을 실제 월드 좌표 및 실행 화면으로 확인하라. 결함이 재현되면 명세의 고정 위치/판만 이동/기존 설정 조건 안에서 수정하고 원래 저장 앵커 설정은 보존하라. 검사 각도를 사용자 확정 외형이나 새로운 지원 기준으로 취급하지 말라. per plan: 외형/Editor 조정의 기울인 판 검사, T009/T015/T029, FR-010/FR-012, SC-002 (partial, F3).
