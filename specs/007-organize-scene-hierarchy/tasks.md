# Tasks: 씬 전체 하이라키와 영어 명명 정리

**Input**: `specs/007-organize-scene-hierarchy/`의 spec.md, plan.md, research.md, data-model.md, contracts/hierarchy-contract.md, quickstart.md.

**Prerequisites**: 현재 Git 브랜치 main, Unity 6000.3.26f1, 저장된 FireNotification 씬. 새 패키지와 런타임 API는 추가하지 않는다.

**Tests**: spec FR-008~012, SC-001~005에서 요구한 전후 비교와 기존 동작 검증을 포함한다. 새 검사는 이름 규칙 복제보다 참조 보존, 잘못된 적용 차단과 Undo 복구를 검증한다. 기존 회귀 검사 코드는 통과시키기 위해 변경하지 않는다.

**Organization**: US1 구역/역할 구조, US2 영어 이름, US3 공용 UI 및 기존 체험 보존. 세 이야기 모두 P1이며 최종 씬 적용은 하나의 공유 자원에 쓰므로 직렬 실행한다.

## Format and paths

- 각 작업은 `- [ ] Tnnn [P?] [USn?] 설명과 경로` 형식이다. P는 선행 조건 완료 후 서로 다른 파일과 자원을 다룰 때만 사용한다.
- 아래 경로는 프로젝트 루트 기준이다. 셸에서 읽고 쓸 때는 `E:/projects/T3_LWS/`를 붙인 절대 경로를 사용한다.
- `<run>`은 T001이 생성한 실제 `YYYYMMDD-HHmmss-organize-scene-hierarchy` 폴더명이다. 예시 문자열 그대로 폴더를 만들지 않는다.
- 새 Editor 코드와 Documentation 파일의 `.meta`도 관리한다. 기준 JSON, 테스트 로그, 녹화 보조 코드와 영상은 Artifacts에만 두고 Git에 추가하지 않는다.

## Phase 1: Setup

**Purpose**: 사용자 변경 보호와 정리 전 기준 확보.

- [ ] T001 `Artifacts/Validation/VisualEvidence/<run>/`를 현재 시각으로 먼저 생성하고 `Assets/_02LWS/Scenes/FireNotification.unity`의 저장 상태, Play Mode, Git 변경, 프리팹 연결을 확인하고 사용자 변경 및 구현 대상 파일을 구분한 기록을 `Artifacts/Validation/VisualEvidence/<run>/preflight.txt`에 남긴다. 사용자 변경을 자동 저장하거나 폐기하지 않는다.
- [ ] T002 `Artifacts/Validation/VisualEvidence/<run>/verification.md`와 실제 하이라키/동일 관찰 위치의 변경 전 화면을 준비한다. T001이 생성한 같은 폴더를 사용하고 Unity 전용 소리 녹화 방식 및 기존 분리 검증 재사용 조건을 확인한다.
- [ ] T003 `specs/007-organize-scene-hierarchy/quickstart.md`의 변경 전 정적 검사, 기존 floor/route 검사와 세 체험 주요 경로를 수행하고 `Artifacts/Validation/VisualEvidence/<run>/before-validation.md`에 실행 환경, 입력 방식, 성공 및 기존 실패를 기록한다. 변경 후 동일 조건과 비교할 기준이며 과거 결과로 대체하지 않는다.

## Phase 2: Foundational

**Purpose**: 모든 이야기에서 공유하는 Editor 전용 안전 적용과 비교 기반. 이 단계 완료 전 실제 씬 정리를 적용하지 않는다.

- [ ] T004 `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 data-model.md의 SceneBaseline 수집과 JSON 저장을 구현한다. GameObject/Component GlobalObjectId, 이름/부모/sibling/activeSelf/enabled/tag/layer/static, world Transform, RectTransform, 직렬화 참조와 UnityEvent, 렌더링/조명/Collider/음원 및 프리팹 원본 해시를 포함하고 참조는 “세션 InstanceID 대신 GlobalObjectId 또는 asset GUID/local file ID로 정규화”한다. baseline.json은 `Artifacts/Validation/VisualEvidence/<run>/baseline.json`에 저장한다.
- [ ] T005 `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 OrganizationEntry, GroupEntry, ProtectionRule과 DryRun을 구현한다. 변경 명세에 StructureOnly/NamesOnly/Full 범위를 저장하고 기본값 Full 및 계약 6절의 범위 선택과 적용/검사 일치를 구현한다. “disposition은 MoveAndRename, RenameOnly, Protected 중 하나”, “한 객체가 둘 이상의 목적지에 배정되거나 누락되면 오류”, “같은 부모 아래 targetName은 고유” 조건을 적용한다. 모든 기존 대상을 organization-plan.json에 기록하며 모호한 대상을 Misc로 보내지 않는다. 대상 씬/편집 상태 검사, Capture/Apply의 dirty 거부, Apply의 stale baseline 거부, 읽기 전용 DryRun/Verify 허용 조건은 계약 5절을 따른다.
- [ ] T006 `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 전후 비교를 구현한다. “world position 0.0001m, world rotation 0.01도, world scale 0.0001을 초과하면 실패”, “보호 대상의 로컬 값과 UI RectTransform 속성 및 world corners는 0.0001 이하” 조건을 적용한다. “오브젝트 수는 기존 수 + 승인된 분류 그룹 수”, “기존 컴포넌트의 ID와 개수는 모두 유지”, “전체 컴포넌트 수는 기존 수 + 해당 scope의 승인된 신규 그룹 수”, “신규 그룹의 Transform만 추가를 허용”, “모든 다른 직렬화 값은 동일” 조건으로 comparison.json을 생성한다. 범위별 허용 변경과 검사 범위는 data-model.md의 Validation scope를 따른다. 미적용 기준은 NotEvaluated로 표시하고 참조, 배치, 문구, 활성 상태, UI 레이아웃과 보호 대상 보존은 세 범위 모두 검사한다. 허용된 부모/이름/로컬 좌표/자식 목록 변화 외의 참조와 컴포넌트 차이를 검출한다.
- [ ] T007 `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 계약의 Capture Baseline, Dry Run, Apply Organization, Verify Organization 메뉴를 연결하고 한 Undo 그룹의 적용 및 실패 복구를 구현한다. “Captured → Planned → AppliedUnsaved → VerifiedUnsaved → Saved → ReopenedVerified” 상태를 보관하며 적용은 자동 저장하지 않는다. 부분 범위는 Undo 후 baseline 복구를 확인하고 저장을 금지하며 Full만 저장 단계로 진행한다. 기존 오브젝트 ID, 프리팹 연결과 컴포넌트를 유지하고 새 그룹은 active=true, localPosition=0, localRotation=identity, localScale=1로 만든다.
- [ ] T008 `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`에서 잘못된 씬, Play Mode, 컴파일 중, dirty Apply, stale baseline, 누락/중복 분류, 보호 위반과 직렬화 참조 손실의 거부를 검증한다. 전용 임시 Editor 테스트 객체에서 실패 및 정상 적용의 Undo 복구, ID 보존과 미저장 동작을 확인하고 `Artifacts/Validation/VisualEvidence/<run>/organizer-safety.txt`에 남긴다. 범위 불일치, 부분 적용 저장 시도, 기존 컴포넌트 손실과 승인된 신규 Transform 이외의 추가도 검출하고, 승인된 그룹 수만큼의 Transform 증가는 정상 통과하는지 확인한다. 사용자 씬은 부정 시나리오에 사용하지 않는다.

**Checkpoint**: 기준 수집, 승인할 변경 목록, 비교, 거부와 복구가 준비되어야 한다. 이후에는 같은 baseline에 구조와 이름 규칙을 누적하며 실제 저장 적용은 T021에서 한 번 수행한다.

## Phase 3: User Story 1 - 구역과 역할로 편집 대상 찾기 (Priority: P1, MVP)

**Goal**: 공용 기능, 세 체험과 각 배경을 구역 및 역할로 찾을 수 있는 구조를 만든다.

**Independent Test**: 구조 변경 명세에서 spec SC-001의 모든 대상이 하나의 구역/역할 경로로 연결되는지 확인한다. 임시 적용 후 하이라키에서 검색 없이 대상을 찾고 비교한 뒤 Undo한다. 영어 이름 전체와 공용 UI 기능 회귀를 US1 완료의 선행 조건으로 삼지 않는다.

- [ ] T009 [US1] `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`에 공용/콘텐츠 경계, 구역 우선 분류, 빈 그룹 금지와 SC-001 대상 완전성 검사를 추가한다. 기존 A 직속 목록처럼 정리되지 않은 구조를 실패로 검출하는지 먼저 확인한다.
- [ ] T010 [US1] `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 계약 1~2절의 공용 기능 및 체험 공간 구조 규칙을 구현한다. 기존 SharedSystems, Contents, 네 공간 루트와 제어 컴포넌트는 유지하고 Player, Navigation, Input, Audio, Lighting 및 체험별 존재하는 역할 그룹을 만든다. 공용 UI는 T020 규칙으로 처리하도록 별도 분류하며 현재 부모를 임의 변경하지 않는다.
- [ ] T011 [US1] `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 화재/연기 공간 이동표와 배경 세부 분류를 구현한다. 비상벨, 인물, 효과, 이동 지점과 바닥 기준점을 같은 체험 경계 안에 유지하고 기존 Corridor/CorridorFinish, SmokeRoomFinish 등의 오브젝트를 삭제하지 않고 사용한다. 비상벨 내부, 열린 문 hinge와 효과 내부 좌표를 유지한다.
- [ ] T012 [US1] `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 RouteSpace 두 시나리오의 구역 분류를 구현한다. 교실 블록은 Classroom wall pier부터 두 번째 Skirting까지로 잡고 A 6개, B West/North 각 2개를 검증한다. 계단/엘리베이터/코너/이동 지점/기준점을 계약 목적지로 분류하며 별도 Door transom backing의 최근접 교실과 동률 규칙도 적용한다.
- [ ] T013 [US1] `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`에서 구조 DryRun 결과를 검사하고 `Artifacts/Validation/VisualEvidence/<run>/us1-structure.json`에 모든 대상의 유일 분류, 빈 그룹 0개, 보호 위반 0개와 미분류 0개를 기록한다. 원래 sibling 순서로만 순번을 배정하며 위치로 반복 블록을 재정렬하지 않았는지 확인한다.
- [ ] T014 [US1] `Assets/_02LWS/Scenes/FireNotification.unity`에 StructureOnly 명세로 기존 이름을 유지한 US1 구조 변경만 미저장으로 임시 적용해 대상 탐색, 월드 배치와 참조 보존을 확인한 다음 Undo로 baseline에 복귀한다. 최종 영어 이름과 기존 이름 중복 기준은 NotEvaluated로 표시하되 문구, 활성 상태, UI 레이아웃과 보호 대상 보존은 필수로 검사하고 `Artifacts/Validation/VisualEvidence/<run>/us1-acceptance.md`와 실제 하이라키 화면으로 확인 범위를 기록한다. 원래 씬과 동일하게 복구됐는지 비교한다.

**Checkpoint**: 구역/역할 구조를 독립 검증했으며 실제 씬은 baseline으로 복구되어 있어야 한다.

## Phase 4: User Story 2 - 영어 이름으로 용도 파악하기 (Priority: P1)

**Goal**: 프로젝트 관리 이름과 반복 번호를 통일하면서 한국어 화면 문구 및 보호 대상은 보존한다.

**Independent Test**: 같은 원본 기준으로 이름 변경만 임시 적용하고 이름 규칙, 중복, 한국어 문구, 보호 이름과 ID 보존을 확인한 뒤 Undo한다.

- [ ] T015 [US2] `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`에 PascalCase, Button/Text/Anchor 접미어, “같은 부모 아래” 중복 검사와 보호 대상 예외를 추가한다. 기존 한글/공백/모호한 이름을 검출하되 보호된 XR/뼈/TMP 내부 이름은 실패로 간주하지 않는지 먼저 확인한다.
- [ ] T016 [US2] `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 계약 4절의 공용 UI 이름표를 구현한다. Content1, 설정, 재시작/복귀, 설정 조절, 확인창, 결과와 floor 버튼 이름 및 LabelText를 용도별로 매핑하고 표시 문자열, 이벤트, UI 자식 순서와 RectTransform은 변경하지 않는다.
- [ ] T017 [US2] `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 ScenarioA/B, Stairwell01/02, 교실 및 반복 부품, Waypoint00~06과 위치 기준점 이름을 구현한다. “번호는 원래 sibling 순서로 배정하며 기존 식별자나 월드 좌표 순서로 다시 정렬하지 않는다” 조건을 적용한다. SharedCanvas/FloorCanvas 이름과 콘텐츠/시나리오/지점 ID, XR/Animator 내부 이름을 보존하고 프로젝트 소유 프리팹 이름은 씬 override로만 변경한다.
- [ ] T018 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`에 NamesOnly 명세로 기존 부모와 sibling 순서를 유지한 이름 변경만 미저장으로 임시 적용하고 `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`로 이름, 문구, ID, 연결과 보호 대상을 검사한 뒤 Undo한다. 임시 번호는 현재 부모별 원래 순서로 배정하고 최종 구조 기준은 NotEvaluated로 기록한다. 참조, 배치, 화면 문구, 활성 상태, UI 레이아웃과 보호 대상 보존은 모두 검사한다. 구조 규칙과 합친 Full DryRun에서는 최종 부모별 원래 순서로 번호를 다시 배정해 중복 0개인지 확인하고 `Artifacts/Validation/VisualEvidence/<run>/us2-naming.json`에 기록한다.

**Checkpoint**: 영어 이름 규칙을 독립 검증하고 씬이 baseline으로 복구되었으며 결합한 구조/명명 변경 명세가 준비되어야 한다.

## Phase 5: User Story 3 - 공용 UI 편집과 기존 체험 유지 (Priority: P1)

**Goal**: 공용 UI 경로를 완성하고 구조/명명을 최종 적용한 뒤 기존 세 체험 동작을 보존한다.

**Independent Test**: 공용 UI 역할별 경로를 찾고 저장/재개방 전후 연결 및 배치를 비교한다. 실제 Play Mode에서 체험 선택, 세 체험 진행과 공용 화면의 적용 가능한 경로를 확인한다.

- [ ] T019 [US3] `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`에 SharedCanvas/FloorCanvas 고정 이름, 두 Canvas 분리, Canvas/ComfortFollowUI 조상, floor presenter의 체험 외부 배치, UI 자식 순서/RectTransform, 실행 상태 및 캐릭터/비상벨/리그 내부 보존 검사를 추가한다. 임시 검사 객체의 잘못된 부모/순서 변경을 검출하는지 확인한다.
- [ ] T020 [US3] `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`에 SharedSystems/UI와 계약 1절의 공용 화면 구조를 완성한다. Canvas 내부는 이름만 바꾸고 기존 내부 부모/순서/컴포넌트는 유지한다. SharedFloorButtons는 별도 Canvas 및 좌표 계층을 통째로 유지하며 체험 및 follower 아래로 이동하지 않게 한다. 전체 변경 명세에 누락과 보호 충돌이 없어야 한다.
- [ ] T021 [US3] T010~T020 전체 규칙의 Full 명세로 최신 baseline 해시를 재확인하고 `Assets/_02LWS/Scenes/FireNotification.unity`에 최종 변경 명세를 한 Undo 그룹으로 적용한다. 저장 전 Full 범위의 T006 비교와 모든 명명/구조 검사가 통과해야 저장한다. 원본 프리팹, 런타임 컴포넌트와 다른 사용자 변경은 건드리지 않으며 실패 시 적용분만 복구한다.
- [ ] T022 [US3] 저장한 `Assets/_02LWS/Scenes/FireNotification.unity`를 다시 열어 ID/참조/배치/활성 상태/프리팹 연결/보호 대상을 재검증하고 `Artifacts/Validation/VisualEvidence/<run>/comparison.json`과 hierarchy-after.txt를 저장한다. Full 검증이 통과하고 최종 Full DryRun이 NoChanges인지 확인한다. 기존 컴포넌트의 ID와 개수는 모두 유지되며 전체 컴포넌트 수는 기존 수 + 승인된 신규 그룹 수이고, 추가된 컴포넌트가 그룹당 Transform 하나뿐인지 확인한다.
- [ ] T023 [US3] `specs/007-organize-scene-hierarchy/quickstart.md`의 SharedUIContractAudit, FloorButtonChecks.ValidateConfiguration, RouteIntegrationChecks.ValidateConfiguration, RouteDomainChecks.Run을 실행하고 새 Play에서 전체 FloorButtonChecks.RunPlayMode를 수행한다. `Artifacts/Validation/VisualEvidence/<run>/after-floor.md`에 Running=false, Passed/Failed와 실제 로그 경로를 기록하고 변경 전 결과와 비교한다.
- [ ] T024 [US3] Play를 새로 시작해 `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`의 기존 RunPlayMode를 실행하고 `Artifacts/Validation/VisualEvidence/<run>/after-route.md`에 Running=false와 Passed/Failed, 실제 로그 및 전후 차이를 기록한다. 합성 추적/장치임을 표시하고 종료 후 override를 해제하거나 Play를 중지한다.
- [ ] T025 [US3] `Assets/_02LWS/Scenes/FireNotification.unity`에서 quickstart의 체험 선택, 세 체험 진행/결과/복귀, 설정, 뒤로, 재시작/나가기 확인, 바닥 버튼 상승/준비/하강, 레이 선택/차폐/중복 방지와 글자 100%/150% 경로를 확인한다. `Artifacts/Validation/VisualEvidence/<run>/journeys.md`에 각 경로 결과와 적용 불가능한 항목의 사유를 기록하며 실행 구간은 T028용 영상과 화면으로 확보한다.
- [ ] T026 [US3] 실제 컨트롤러가 연결되면 `Assets/_02LWS/Scripts/Validation/MenuInputChecks.cs`의 ValidateLiveBinding과 실제 왼손 메뉴 입력으로 세 체험의 중지/재개를 확인해 `Artifacts/Validation/VisualEvidence/<run>/menu-input.md`에 남긴다. 미연결/Operator 부재는 미검증으로 기록하며 통과로 간주하거나 새 의존성을 설치하지 않는다. 테스트 종료 후 Play/추적 override를 정리하고 씬 dirty 상태를 확인한다.

**Checkpoint**: 공용 UI 경로, 전체 비교, 저장/재개방, 기존 전체 검사와 세 체험 경로를 확인한다. 신규 실패는 해결해야 하며 기존 환경 실패와 미검증은 숨기지 않는다.

## Phase 6: Polish and Cross-Cutting Concerns

**Purpose**: 사람이 사용할 편집 안내, 헌법 증거와 최종 변경 범위 검토.

- [ ] T027 [P] `Assets/_02LWS/Documentation/SceneHierarchy.md`와 `.meta`를 작성한다. 최종 주요 경로, SC-001 대상 탐색, 공용 UI 경로, 명명 규칙과 보호 예외를 실제 저장 씬 및 hierarchy-after.txt와 대조하고 전체 탐색 항목 누락 0개를 확인한다. T026 후 T028과 병렬 작성 가능하며 씬은 수정하지 않는다.
- [ ] T028 [P] `Artifacts/Validation/VisualEvidence/<run>/execution.mp4`, 전후 하이라키/관찰 위치 화면과 verification.md를 검증한다. T025 등에서 확보한 원본 실행 영상은 1920×1080 이상, 30fps 이상이어야 하며 Unity 게임 소리만 포함해야 한다. 저장 영상의 메타데이터, 실제 화면 재생, 소리 청취와 동기화, 외부 앱/마이크 제외의 설정별 분리 근거를 확인한다. 부족한 항목은 헌법에 따라 최선의 자료와 대체 자료, 이유 및 미검증 범위를 기록한다. T027과 병렬 가능하며 완료된 Unity 씬을 다시 구동하거나 변경하지 않는다.
- [ ] T029 `Assets/_02LWS/Scenes/FireNotification.unity`, `Assets/_02LWS/Editor/SceneHierarchyOrganizer.cs`, `Assets/_02LWS/Editor/SceneHierarchyOrganizerChecks.cs`, `Assets/_02LWS/Documentation/SceneHierarchy.md`와 필요한 meta만 이번 구현 변경인지 검토한다. `Packages/manifest.json`, `Packages/packages-lock.json`, ProjectSettings, 입력 액션, 런타임 코드, 원본 프리팹과 ContentMenu.unity의 의도하지 않은 변경 및 새 Meta SDK/Operator 의존성 0개를 확인한다. `Artifacts/Validation/VisualEvidence/<run>/scope-audit.txt`에 git diff --check, 증거의 git check-ignore 및 git ls-files 결과도 기록한다.
- [ ] T030 `specs/007-organize-scene-hierarchy/quickstart.md`와 spec SC-001~005에 최종 결과를 대조하고 `Artifacts/Validation/VisualEvidence/<run>/verification.md`에 성공, 기존 실패, 미검증 환경과 소리 확인 한계를 확정한다. 완료 보고에 실제 대표 MP4/대체 화면과 편집 안내의 파일 링크를 제공한다. 작업을 수행한 근거가 있는 항목만 이 tasks.md에서 체크하며 자동 커밋, push 또는 제출 패키지 생성은 하지 않는다.

## Dependencies and Execution Order

```text
T001~T003 Setup (T001에서 run 폴더를 먼저 생성)
  → T004~T008 Shared foundation
  → T009~T014 US1 structural rules and isolated acceptance
  → T015~T018 US2 naming rules and isolated acceptance
  → T019~T026 US3 UI integration, atomic scene apply and runtime regression
  → T027 || T028 documentation and offline evidence review
  → T029 scope audit
  → T030 acceptance and report
```

- 동일 씬과 Organizer 소스에 쓰는 작업은 병렬 실행하지 않는다. 모든 테스트 및 자료를 만들기 전에 선행 구현/기준 수집이 완료되어야 한다.
- US1과 US2의 독립 수용 검사는 해당 이야기 변경만 미저장으로 적용하고 Undo한다. 최종 제품 씬의 저장은 두 이야기 및 US3 공용 UI 규칙이 모두 준비된 T021에서 한 번 수행한다.
- US3의 최종 적용은 US1/US2 규칙을 사용한다. 공용 UI 보존 검사 자체는 같은 기반의 임시 객체로 독립 검증할 수 있다.
- T008의 거부/복구 검증, T009/T015/T019의 잘못된 상태 검출은 해당 실제 적용보다 먼저 수행한다. 테스트를 통과시키기 위해 기존 검사나 제품 동작을 약화하지 않는다.
- 모든 기존 검사 종료 조건은 Running=false이며 호출 성공만으로 완료 처리하지 않는다. 실패는 원래 결과를 보존하고 신규 실패와 환경 제약을 나눈다.

## Parallel Examples by Story

| 구간 | 실행 예 | 제약 |
|---|---|---|
| US1 | T010 → T011 → T012 → T013 → T014 | 같은 소스와 씬을 사용하므로 직렬 |
| US2 | T015 → T016 → T017 → T018 | 같은 소스와 씬을 사용하므로 직렬 |
| US3 | T019 → T020 → T021 → T022 → T023 → T024 → T025 → T026 | 씬 변경과 Play Mode 검사는 직렬 |
| 공통 마무리 | T027 SceneHierarchy.md 작성과 T028 저장 영상/소리 검토 | T026 완료 후 다른 파일과 오프라인 자료만 사용하므로 병렬 가능 |

## Implementation Strategy

**MVP**: Setup과 Foundational 이후 US1만 임시 적용해 구역/역할 구조의 편집 효과를 확인할 수 있다. 제품 씬에 영구 저장하는 최소 범위는 세 이야기를 결합한 T021~T026이며 US1 임시 검증을 최종 완료로 보고하지 않는다.

**Incremental delivery**: 안전 기반 → 구조 규칙 및 독립 검사 → 영어 이름 규칙 및 독립 검사 → 공용 UI 통합 및 전체 적용 → 정적/실행 회귀 → 편집 안내/증거/범위 검토 순서다. 각 이야기 종료 시 비교 자료를 남겨 다음 단계가 이전 결과를 훼손하는지 확인한다.

**Requirement coverage**:

| 요구사항 | 주요 작업 |
|---|---|
| FR-001~004, SC-001 | T009~T014, T027 |
| FR-005 | T019~T022 |
| FR-006, SC-002 | T015~T018, T022 |
| FR-007~010, SC-003~004 | T004~T008, T019~T026 |
| FR-011 | T005~T008, T014, T017~T019, T022, T029 |
| FR-012, SC-005 | T002~T003, T023~T030 |

**Completion discipline**: 기준/결과 자료는 Artifacts에 남긴다. 실제 수행한 환경만 확인 완료로 기록하며 Editor 검사로 Quest 실기나 실제 소리 청취를 대신하지 않는다. 체크리스트 상태를 구현 도중 임의로 바꿔 통과 조건을 우회하지 않는다.
