# Tasks: 콘텐츠 1 — 화재 사실 알리기

**Input**: `specs/001-fire-notification/`의 [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md), [계약](contracts/experience-contract.md), [quickstart.md](quickstart.md), [확정 대본](content-script.md).

**Prerequisites**: 계획 검토 완료 후 사용자 `$speckit-implement` 요청에 따라 구현 중이다. 체크박스는 실제 수행·검증한 작업에만 표시한다. 헌법은 미작성 템플릿이며 예시 규칙을 적용하지 않는다. AGENTS.md는 수정하지 않는다.

**Tests**: 명세의 수락 시나리오·SC-001~008에 따른 실행 검증을 포함한다. 자동 테스트 작성/TDD는 명세나 사용자에게 명시 요청되지 않아 새 자동 테스트 작성 작업은 생성하지 않는다. 기존 자동 테스트가 있으면 최종 회귀 검증에 실행한다. Editor/Simulator 결과로 Quest 2 검증을 대체하지 않는다.

**Organization**: Phase 1·2는 공통 기반, Phase 3~6은 사용자 스토리, Phase 7은 전체 검증·제출이다. 기술 구성은 사용자 위임 범위에서 정한 구현 방안이며 개별 기술을 사용자가 선택한 것으로 간주하지 않는다.

## Format and path conventions

`- [ ] Tnnn [P?] [USn?] 설명 및 정확한 경로` 형식이다. 경로는 저장소 기준이며 소유 구현·리소스는 `Assets/_02LWS/`에 둔다. `[P]`는 아래 명시한 선행 작업 완료 후 서로 다른 파일을 병렬 처리할 수 있다는 뜻이다. 동일 씬·공통 파일 수정은 순차 수행한다. Unity 에셋은 Editor로 만들고 `.meta` GUID를 보존한다.

## Phase 1: Setup — 기존 프로젝트와 무료 자료 준비

**Purpose**: 새 프로젝트나 SDK를 추가하지 않고 기존 Unity 환경에서 제출 경계와 자료 준비 조건을 만든다.

- [X] T001 `Assets/_02LWS/Scripts/Domain/LWS.FireNotification.Domain.asmdef`, `Assets/_02LWS/Scripts/LWS.FireNotification.Runtime.asmdef`와 plan.md의 하위 폴더를 준비하고 Domain은 Unity 표현과 분리한다. Scripts 루트의 Runtime assembly는 Runtime·Presentation·Interaction 하위 폴더를 포함하고 Domain assembly를 참조한다. Domain은 Runtime을 참조하지 않는다. 필요한 XRI·Input System·UI 패키지 assembly 참조도 명시한다.
- [X] T002 기존 Unity 6000.3.26f1·패키지 lock, Android 모듈, OpenXR Quest/Touch profile·Input System·IL2CPP/ARM64·SDK 설정을 점검해 `Assets/_02LWS/Documentation/Environment.md`에 기록한다. 불필요한 패키지 업그레이드와 프로젝트 설정 덮어쓰기는 하지 않는다.
- [X] T003 무료 시각 자료·한글 폰트·경보음·합성 음성 도구를 공식 이용 조건으로 조사하고 앱 배포 및 unitypackage 재배포 가능 여부, 출처·라이선스·무료 한도를 `Assets/_02LWS/Documentation/ThirdPartyNotices.md`에 기록한다. 자체 제작 가능한 단순 모델을 우선 활용하고 적합한 무료 자료가 없으면 결제나 임의 대체 승인 없이 해당 제약을 보고한다.
- [X] T004 T003의 조건을 충족하는 도구로 content-script.md의 확정 4개 안내·오답 피드백·“불이야!”를 사전 합성하여 `Assets/_02LWS/Audio/Voice/`에 cue별 파일로 저장하고 `Assets/_02LWS/Content/VoiceManifest.json`에 cue ID·원문·파일·생성 출처를 기록한다. 한국어 발음과 차분한 안내 말투를 확인하고 실시간 합성·다운로드를 요구하지 않는다.

**Checkpoint**: 무료 배포 가능한 자료 준비 경로와 로컬 음성이 확보되어야 기반 구성을 마칠 수 있다. 자료가 없다는 사실을 무음 자동 완료로 숨기지 않는다.

## Phase 2: Foundational — 모든 스토리의 공통 기반

**Purpose**: 상태·연출 수명·중단·XR 입력을 공유하며 아래 기반 완료 후 스토리 작업을 시작한다.

- [X] T005 [P] `Assets/_02LWS/Scripts/Domain/ExperienceSession.cs`와 `Assets/_02LWS/Scripts/Domain/ExperienceStage.cs`에 9단계·sessionId·executionId·suspended·transitionInFlight와 명령 검증을 구현한다. 제약은 “체험 씬마다 새 세션. 단계 진입/재진입 시 executionId 갱신. 중단 중 명령 거부”이며 현재 단계 외/알 수 없는 명령·만료 완료를 거부하고 메뉴/중단을 완료보다 우선한다.
- [X] T006 [P] `Assets/_02LWS/Scripts/Presentation/GuidanceCue.cs`와 `Assets/_02LWS/Content/GuidanceCatalog.asset`에 대본·cue ID·음성·자막 구간을 별도 데이터로 구성한다. 제약은 “로컬 음성 필수, 한글 글리프 포함. segment 시작/끝 시간이 유효하고 음성과 같은 의미 전달”이며 누락/잘못된 참조는 검증 오류로 보고한다. 추후 문구·파일 변경에 진행 코드 수정이 필요 없게 한다.
- [X] T007 [P] `Assets/_02LWS/Scripts/Presentation/ScenarioBaseline.cs`에 npcTransforms·idleStates·alarmCapRestPose·fireVisualState·playerAnchor와 결과별 시작/종료 상태를 구현한다. 제약은 “최초와 같은 화재 상황 복원 기준. 각 결과의 반응 시작/종료 상태도 정의”이며 반복 복원과 대기 단계 재구성의 결과가 같게 한다.
- [X] T008 T005~T007 후 `Assets/_02LWS/Scripts/Presentation/PresentationRun.cs`와 `Assets/_02LWS/Scripts/Presentation/GuidancePresenter.cs`에 오디오·자막·인물 연출 실행 및 취소를 구현한다. 제약은 “음성·애니메이션·지연 callback 수명. 취소된 실행 완료는 무시”이며 executionId별 완료는 한 번만 소비하고 취소 시 지연 작업·음성·경보·자막·반응을 정리한다.
- [X] T009 T008 후 `Assets/_02LWS/Scripts/Runtime/ExperienceLifecycle.cs`에 pause/focus/OpenXR session focus·착용 신호 합성과 SuspensionSnapshot을 구현한다. 제약은 “같은 프로세스 메모리에만 보존. 메뉴 복귀·앱 재실행에는 사용하지 않음”이다. 첫 중단만 저장하고 모든 중단 요인 해제 때 해당 단계 처음부터 한 번 재진입하며 입력 대기는 새 입력을 기다린다. 활성 세션 없는 시작 콜백과 중복 신호를 처리하고 전역 timeScale만으로 중단하지 않는다.
- [X] T010 [P] `Assets/_02LWS/Prefabs/XR/LwsXROrigin.prefab`, `Assets/_02LWS/Prefabs/UI/LwsEventSystem.prefab`, `Assets/_02LWS/Settings/LwsInputActions.inputactions`를 구성한다. Quest 컨트롤러 가상 손·poke, UI ray+trigger와 world-space Canvas용 XRUIInputModule을 제공하고 hover/dwell 자동 선택을 배제한다. 필요한 기존 샘플 의존성은 내부 복사·재연결하고 라이선스를 보존하며 전역 XRI layer asset은 임의 이동하지 않는다.
- [X] T011 T007·T010 후 `Assets/_02LWS/Prefabs/Environment/Corridor.prefab`, `Assets/_02LWS/Prefabs/Characters/Bystander.prefab`, `Assets/_02LWS/Scripts/Presentation/BystanderPresenter.cs`와 `Assets/_02LWS/Prefabs/Environment/TeleportSurface.prefab`에 공통 복도·화재 표현·인물 기본 행동/취소 가능한 반응 기능·필요한 텔레포테이션을 구성한다. 두 체험에서 사용할 반응 데이터를 제공하되 행동별 결과 시작은 각 스토리가 맡는다. 다른 방/층 이동·안내 캐릭터·NPC 대피 시작은 추가하지 않는다.
- [X] T012 T005~T011 후 `Assets/_02LWS/Scripts/Runtime/FireNotificationController.cs`와 `Assets/_02LWS/Scenes/FireNotification.unity`에 공통 실행 구성을 연결한다. 공통 복도·인물·필요한 텔레포테이션을 배치하고 ScenarioBaseline을 연결한다. 체험 씬 직접 로드도 첫 안내부터 시작하며 XR Origin·AudioListener·EventSystem은 각 하나다. 단계별 입력 활성화·held trigger 해제 후 새 press·비활성화 시 취소를 연결하고, Editor 전용 검증 진입점을 제공하되 배포 빌드에는 단계 건너뛰기 UI를 노출하지 않는다.

**Checkpoint**: 공통 의존성의 누락 참조·컴파일 오류 없이 로컬 cue 재생·취소 및 단계 재진입을 확인한다. 스토리별 실제 동작 연결은 다음 단계에서 수행한다.

## Phase 3: US1 — 큰소리로 화재 알리기 (P1, MVP)

**Goal**: 질문·두 선택지·오답 피드백·“불이야!”·주변 반응을 독립적으로 체험한다.

**Independent Test**: FireNotification 씬 직접 진입 → 첫 안내 → 오답 후 재선택 → 정답 → 플레이어 음성·인물 반응. 벨이나 메뉴가 없어도 검증 가능하다.

- [X] T013 [P] [US1] `Assets/_02LWS/Scripts/Domain/ActionOption.cs`와 `Assets/_02LWS/Content/ActionOptions.asset`을 구성한다. 제약은 “‘큰소리로 알리기’와 ‘다른 사람이 알릴 때까지 기다리기’ 두 항목만 제공. 오답에 확정 피드백의 음성·자막 cue 필수”이며 별도 오답 후보는 추가하지 않는다.
- [X] T014 [P] [US1] `Assets/_02LWS/Scripts/Presentation/ShoutResultPresenter.cs`에 로컬 “불이야!” 음성과 T011의 인물 반응을 한 번 재생·취소하는 큰소리 결과 표현을 구현한다. executionId가 만료된 완료를 무시하고 재진입 시 결과 시작 상태부터 재생한다.
- [X] T015 [US1] T013 후 `Assets/_02LWS/Prefabs/UI/ActionChoicePanel.prefab`과 `Assets/_02LWS/Scripts/Interaction/ActionChoiceInput.cs`에 두 선택지의 ray+새 trigger 입력, 피드백 중 잠금과 재표시를 구현한다.
- [X] T016 [US1] T013~T015 후 `Assets/_02LWS/Scripts/Runtime/ShoutExperience.cs`에 SituationGuidance → AwaitAction → ShoutResult를 연결한다. 오답은 확정 음성·자막 뒤 같은 대기, 정답은 “불이야!”와 인물 반응 한 번만 실행하고 무입력·반복·현재 단계 외 입력으로 진행하지 않는다.
- [X] T017 [US1] `Assets/_02LWS/Scenes/FireNotification.unity`에 복도·인물·선택 패널·ShoutExperience·기본 상태 참조를 연결하고 사용자가 안내와 반응을 볼 수 있게 배치한다. T012에서 배치한 공통 복도·인물·텔레포테이션을 재사용하고 불필요한 이동은 강요하지 않는다.
- [X] T018 [US1] spec.md Story 1의 4개 시나리오와 30초 무입력·hover·반복 trigger·설명/선택/결과 중단 후 단계 재시작을 실행하고 `Assets/_02LWS/Documentation/Validation/US1.md`에 환경·절차·실제 결과를 기록한다. FR-001~003, 009, 011~012, 015~016, 018~019, 021을 확인한다.

**Checkpoint**: 큰소리 알리기 독립 시연 가능. 이것은 전체 콘텐츠 제출 완료가 아니다.

## Phase 4: US2 — 비상벨로 화재 알리기 (P1)

**Goal**: 역할 설명을 듣고 컨트롤러 가상 손으로 벨을 밀어 경보와 반응을 확인한다.

**Independent Test**: T012의 Editor 전용 진입점으로 BellGuidance 시작 → 직접 push → 경보·반응. US1 또는 다음 버튼을 먼저 수행할 필요가 없다.

- [X] T019 [P] [US2] `Assets/_02LWS/Scripts/Interaction/BellPushInput.cs`에 BellState·XRI poke 필터 연결을 구현한다. 제약은 “표면 접촉 후 축 방향 깊이 통과만 press. ray/grab 금지. 복귀 때 접촉 해제 후 재무장”이며 허용 interactor 유형·축·깊이를 검사하고 양손·반복 push도 한 번만 전달한다.
- [X] T020 [P] [US2] `Assets/_02LWS/Scripts/Presentation/AlarmResultPresenter.cs`와 `Assets/_02LWS/Audio/Alarm/CorridorAlarm.wav`에 로컬 경보·인물 반응 재생/취소를 구성한다. 경보 loop와 결과 구간 완료를 분리하고 인물은 행동을 멈춰 반응하며 대피 연출은 추가하지 않는다.
- [X] T021 [US2] T019 후 `Assets/_02LWS/Prefabs/Interaction/FireAlarm.prefab`에 식별 가능한 벨 몸체·움직이는 버튼 캡·collider·XRSimpleInteractable·XRPokeFilter와 BellPushInput을 연결한다. 표면 접촉과 유효 깊이를 구분하고 ray·grab으로 완료되지 않도록 필터·레이어 의미를 구성한다.
- [X] T022 [US2] T019~T021 후 `Assets/_02LWS/Scripts/Runtime/BellExperience.cs`에 BellGuidance → AwaitBellPress → AlarmResult를 연결하고 설명 중 벨 성공을 차단한다. 대기 복귀 시 새 접촉/push, 결과 복귀 시 경보·반응 처음부터 재생하며 현재 executionId만 완료로 소비한다.
- [X] T023 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`에 벨·역할 안내·경보·인물 기본 상태를 연결하고 컨트롤러 손 위치·누름 축/깊이·벨 높이를 실기기에서 보정 가능하게 직렬화한다. 접근 이동이 필요하면 T011~T012의 공통 텔레포테이션을 활용한다. US1의 구현 파일 없이도 공통 인물·복도를 이용해 독립 실행 가능하게 한다.
- [X] T024 [US2] 벨 독립 진입에서 무입력·근접·표면 접촉·ray+trigger·grab은 실패, 유효 push는 한 번 성공하는지 실행하고 `Assets/_02LWS/Documentation/Validation/US2.md`에 기록한다. 설명/벨 대기/경보 결과 중단·손이 벨 안에 남은 채 복귀·양손·빠른 반복을 포함해 FR-005~007, 009, 015, 018~019, 021을 확인한다.

## Phase 5: US3 — 같은 상황에서 두 방법을 순서대로 체험 (P2)

**Goal**: 첫 결과 이후 사용자 선택으로만 같은 상황을 복원하고 벨 체험으로 연결한다.

**Independent Test**: 큰소리 결과 완료 상태 → 다음 미선택 시 유지 → 다음 선택 → 기본 복도 복원 → 벨 설명. 이전 소리·반응이 남으면 실패다.

- [X] T025 [P] [US3] `Assets/_02LWS/Prefabs/UI/NextPanel.prefab`과 `Assets/_02LWS/Scripts/Interaction/NextInput.cs`에 현재 AwaitNext 단계의 새 trigger만 Continue로 전달하는 다음 버튼을 구현한다. 자동 전환·hover 선택·held trigger 재활성화는 거부한다.
- [X] T026 [P] [US3] `Assets/_02LWS/Scripts/Presentation/ScenarioRestorer.cs`에 현재 연출 취소 후 ScenarioBaseline으로 인물·벨·화재·필요한 플레이어 기준 위치를 복원하는 반복 가능한 작업을 구현한다. RestoreSituation 중단 후에는 복원부터 재시작하고 이전 실행 완료를 무시한다.
- [X] T027 [US3] T025~T026 후 `Assets/_02LWS/Scripts/Runtime/MethodSequence.cs`에 ShoutResult 완료 → AwaitNext → RestoreSituation → BellGuidance를 연결한다. 첫 결과 후 확정 안내는 다음 버튼과 함께 제공하고 안내 종료를 진행 조건으로 삼지 않는다. 다음 선택 시 안내도 취소하며 중단 복귀 시 현재 대기/복원만 재구성한다.
- [X] T028 [US3] `Assets/_02LWS/Scenes/FireNotification.unity`에 두 체험과 MethodSequence·다음 패널·복원 참조를 연결해 확정 7개 과정의 순서를 유지하고 분리 검증용 진입점은 Editor에서만 유지한다.
- [X] T029 [US3] `Assets/_02LWS/Documentation/Validation/US3.md`에 다음 미선택·빠른 반복·전환 직전/복원 중 중단·이전 run의 늦은 완료·상황 잔류 여부를 기록한다. FR-004, 008~010, 019와 SC-001~004의 순서·중복·대기 기준을 확인한다.

## Phase 6: US4 — 메뉴 진입·완료·언제든 복귀 (P2)

**Goal**: 콘텐츠 1만 있는 메뉴에서 시작해 완료하거나 도중 나가고, 재진입하면 처음부터 시작한다.

**Independent Test**: 메뉴에서 시작, 완료 상태에서 메뉴 복귀, 9단계 각각 도중 복귀, 재진입 초기화를 각각 검증한다. 전체 순환은 US3 통합 후 검증한다.

- [X] T030 [P] [US4] `Assets/_02LWS/Prefabs/UI/ContentMenuPanel.prefab`과 `Assets/_02LWS/Scripts/Interaction/ContentMenuInput.cs`에 콘텐츠 1 항목만 표시하고 새 ray+trigger 선택을 StartContent로 전달한다. 다른 콘텐츠·준비 중 항목은 표시하지 않는다.
- [X] T031 [P] [US4] `Assets/_02LWS/Prefabs/UI/ReturnToMenuPanel.prefab`과 `Assets/_02LWS/Scripts/Interaction/ReturnToMenuInput.cs`에 체험 모든 단계에서 접근 가능한 메뉴 복귀 및 완료 후 복귀 입력을 구성한다. 현재 단계의 선택 패널 잠금이나 오답 피드백이 메뉴 접근을 막지 않게 한다.
- [X] T032 [US4] `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`에 두 씬 경로·Single 로딩·중복 로드 방지와 체험 정리 후 메뉴 이동을 구현한다. snapshot 폐기·입력 차단·음성/경보/연출 취소 후 로드하고 재진입은 새 세션, 프로세스 시작은 메뉴이며 전역 audio/timeScale을 덮어쓰지 않는다.
- [X] T033 [US4] `Assets/_02LWS/Scripts/Runtime/CompletionExperience.cs`에 AlarmResult 구간 완료 → CompletionGuidance를 연결한다. 경보 loop를 정지하고 확정 완료 음성·자막과 메뉴 버튼을 제공하며 완료 안내 중에도 메뉴 복귀를 허용한다.
- [X] T034 [US4] `Assets/_02LWS/Scenes/ContentMenu.unity`를 구성하고 `Assets/_02LWS/Scenes/FireNotification.unity`에 공통 복귀·완료 UI를 연결한다. 씬별 rig/EventSystem/AudioListener 각 하나를 유지하고 대상 Android Build Profile Scene List에 두 씬을 등록해 메뉴를 첫 씬으로 둔다. 설정 절차는 `Assets/_02LWS/Documentation/Integration.md`에 기록한다.
- [X] T035 [US4] `Assets/_02LWS/Documentation/Validation/US4.md`에 메뉴 → 전체 체험 → 완료 → 메뉴 및 각 단계/오답 피드백 중 복귀 → 재진입을 기록한다. 소리·반응·snapshot 잔류, 중복 rig·씬 중복 로드가 없고 FR-013~014, 017, 019~021 및 SC-005~006을 충족하는지 확인한다.

## Phase 7: Polish & Cross-Cutting Concerns — 전체 검증·패키지 제출

**Purpose**: 네 스토리가 연결된 결과를 실기기·오프라인·Import 환경에서 검증한다. 실제 확인 전 완료 표시하지 않는다.

- [X] T036 `Assets/_02LWS/Content/GuidanceCatalog.asset`, `Assets/_02LWS/Content/VoiceManifest.json`과 `Assets/_02LWS/Documentation/Validation/ContentAudit.md`에서 확정 대본·두 선택지·음성/자막 일치·한글 표시·로컬 참조·무료 이용 조건·출처를 대조하고 누락을 수정한다. 네트워크·온라인 합성·원격 필수 자료가 없어야 한다.
- [ ] T037 `Assets/_02LWS/Documentation/Validation/Quest2Acceptance.md`에 실제 빌드 식별자·Quest 2 OS·단독 실행 결과를 기록하며 quickstart.md 전체 및 FR-001~023/SC-001~010을 항목별 검증한다. 헤드셋 탈착·시스템 메뉴 각각 9단계/오답 피드백, 반복 신호·held trigger·취소와 완료 경합을 확인하고 최초 실행 전 인터넷 차단 상태의 새 설치/데이터 초기화 검증을 포함한다. 기기 접근이 없으면 미완료로 남긴다.
- [ ] T038 `Assets/_02LWS/Documentation/Validation/Performance.md`에 Quest 2에서 제안 기준 72 Hz의 CPU/GPU 시간·경보/인물 반응 스파이크를 실측한다. 필요한 경우 `Assets/_02LWS/Art/`와 `Assets/_02LWS/Prefabs/`의 조명·효과·모델을 조정하고 변경한 동작을 재확인한다. 13.9 ms는 프로파일링 기준이며 사용자 확정 합격 기준으로 바꾸지 않는다.
- [X] T039 `Assets/_02LWS/Documentation/Integration.md`에 Unity/패키지 버전·Android SDK/IL2CPP/ARM64·OpenXR Quest/Touch·Input·URP/폰트·씬 등록·interaction layer 의미 매핑·기존 rig 중복 방지·자산 라이선스를 완성하고 `_02LWS` 밖 프로젝트 에셋 참조를 내부화한다. Package Manager와 ProjectSettings는 unitypackage에 포함되지 않음을 명시한다.
- [X] T040 `Assets/_02LWS/Documentation/Validation/ExportImport.md`에 `_02LWS` Export 목록·GUID 참조·포함 범위를 기록하고 `Artifacts/FireNotification.unitypackage`로 내보낸다. Include dependencies로 폴더 밖 소유 자산이 섞이지 않는지 확인한다. 패키지는 제출 산출물이므로 Assets 밖 Artifacts에 두고 제품 소유 에셋의 경계는 유지한다.
- [ ] T041 별도 같은 Unity 버전 프로젝트에서 T039의 prerequisite 설정 후 `Artifacts/FireNotification.unitypackage`를 Import하고 missing reference/script·외부 샘플 의존·한글·중복 rig·FireNotification 단일 씬 및 Starter Assets prerequisite을 확인한다. 새 Quest 2 빌드의 기본 순환·벨 push·메뉴 재진입·오프라인 결과를 `Assets/_02LWS/Documentation/Validation/ExportImport.md`에 추가한다. 통합 프로젝트 미지정 시 이 선행 검증과 향후 대상 지정 후 재검증을 구분한다.
- [ ] T042 `Assets/_02LWS/Documentation/Validation/AcceptanceSummary.md`에 모든 스토리·FR/SC·실기기·성능·라이선스·Import 증거와 미완료 항목을 모으고 이용 가능한 기존 EditMode/PlayMode 테스트를 실행해 결과를 기록한다. 문서/Export 이후 제품 변경이 있으면 영향 범위 재검증·재Export하여 `Artifacts/FireNotification.unitypackage`와 최종 소스가 일치하게 한다.

## Dependencies & Execution Order

### Phase dependencies

```text
Setup T001~T004
  → Foundational T005~T012
      ├─ US1 T013~T018 ─┐
      └─ US2 T019~T024 ─┴→ US3 T025~T029
                              → US4 T030~T035
                                  → Final T036~T042
```

US1과 US2는 공통 기반 뒤 각각 구현·검증 가능하다. 두 스토리는 서로의 결과 표현이나 체험 로직을 참조하지 않고 기반의 복도·인물·텔레포테이션을 사용한다. 같은 FireNotification.unity를 수정하는 T017과 T023은 순차 수행한다. US3 통합·검증은 US1+US2 완료 후, US4의 전체 순환 검증은 US3 후 진행한다. 기본 실행은 ID 순서다.

### Within-phase dependencies

- Setup: T001 후 T002, T003 후 T004. 자료 이용 조건 확인 전에 외부 자료·음성을 취득해 배포하지 않는다.
- Foundation: T005·T006·T007·T010은 Setup 뒤 병렬 가능. T008은 T005~T007, T009는 T008, T011은 T007·T010, T012는 T005~T011 완료 후 진행한다.
- US1: T013·T014 병렬 → T015(T013 필요) → T016(T013~T015 필요) → T017 → T018.
- US2: T019·T020 병렬 → T021(T019 필요) → T022 → T023 → T024.
- US3: T025·T026 병렬 → T027 → T028 → T029.
- US4: T030·T031 병렬 → T032 → T033 → T034 → T035.
- Final: T036~T042 순차. 검증 실패나 자료 부재는 증거와 함께 미완료로 남기고 미실행 기기 검증을 성공 처리하지 않는다.

## Parallel execution examples per story

병렬 가능 구간의 예시이며 이 문서 수정 중 작업자나 에이전트를 실행하지 않는다.

| Story | 동시에 가능한 작업 | 합류 조건 |
|---|---|---|
| US1 | T013 ActionOption / T014 큰소리 결과 표현 | T015~T017에서 연결 |
| US2 | T019 벨 입력 판정 / T020 경보 결과 표현 | T021~T023에서 연결 |
| US3 | T025 다음 UI / T026 공통 상황 복원 | T027에서 연결 |
| US4 | T030 콘텐츠 선택 UI / T031 공통 복귀 UI | T032~T034에서 연결 |

## Requirement coverage

| Requirement | Main tasks |
|---|---|
| FR-001~003 | T013~T018 |
| FR-004 | T025~T029 |
| FR-005~007 | T019~T024 |
| FR-008 | T027~T029 |
| FR-009~010 | T005, T008, T014, T016, T019, T026~T029 |
| FR-011 | T010~T012, T017, T023 |
| FR-012 | T013, T015~T018 |
| FR-013~014 | T030, T032~T035 |
| FR-015~016 | T004, T006, T008, T011~T012, T016~T017, T022, T033, T036 |
| FR-017 | T031~T035 |
| FR-018 | T002, T010, T023~T024, T034, T037 |
| FR-019 | T005, T007~T009, T012, T014, T018, T024, T026~T029, T032~T035, T037 |
| FR-020 | T003~T004, T006, T036~T037, T041 |
| FR-021 | T010, T015, T019, T025, T030~T031, T035, T037 |
| SC-001~004 | T018, T024, T029, T037 |
| SC-005~006 | T035, T037 |
| SC-007~008 | T037 |
| 폴더 제출·Import | T039~T042 |

## Implementation Strategy

1. **MVP**: Setup + Foundation + US1에서 첫 안내부터 큰소리 결과까지 독립 확인한다. 개발 중 최소 데모이며 최종 제출 범위를 줄이는 것은 아니다.
2. **Incremental delivery**: US2 독립 확인 → US3에서 같은 상황 복원·순서 연결 → US4에서 메뉴부터 완료까지 연결한다.
3. **Final acceptance**: Quest 2 단독·최초 오프라인·9단계 중단·Export/Import 증거를 남긴다. 통합 대상 지정 후 재검증 조건도 인계한다.
4. **Change control**: 조정값·무료 자료는 확정 요건 안에서 AI가 선정한다. 교육 내용·비용·확정 입력 방식을 바꿀 때는 사용자 결정이 필요하다. AGENTS.md는 수정하지 않는다.

## Task generation validation

42개 작업: Setup 4 / Foundation 8 / US1 6 / US2 6 / US3 5 / US4 6 / Final 7. 생성 당시 모두 미실행으로 시작했으며 현재 상태는 각 체크박스와 Validation 문서에서 확인한다. 전후 훅은 `.specify/extensions.yml`의 유무와 등록에 따라 확인한다.

## Phase 8: 2026-11-05 변경 — 단일 씬·주제별 독립 공간

기존 T001~T042는 최초 구현/검증 이력이다. 두 씬 관련 완료 표시는 당시 구조에 대한 기록이며 새 구조 검증 완료를 의미하지 않는다. 현재 범위는 콘텐츠 1과 메뉴이며 나머지 후보 공간은 만들지 않는다. 기존 Quest 2·성능·clean Import 미완료는 유지한다.

- [X] T043 spec.md·plan.md·research.md·data-model.md·contracts/experience-contract.md·quickstart.md를 단일 씬·별도 공간 요구 FR-022/SC-009와 일치시킨다. requirements.md 체크리스트는 읽기 전용으로 유지한다.
- [X] T044 ContentSceneNavigator.cs의 씬 로딩을 공간 활성화·XR 앵커 이동으로 교체하고 FireNotificationController.cs에 재진입 시 새 세션 시작/종료를 연결한다. 중복 선택, 메뉴 복귀 시 취소, 앱 중단 규칙을 보존한다.
- [X] T045 Unity Editor에서 FireNotification.unity에 MenuSpace·FireNotificationSpace와 시작 앵커를 구성하고 공용 rig·EventSystem·AudioListener 각 하나를 연결한다. 기존 ContentMenu 씬은 원본으로 보존하되 기능 Build Scene List에서 제외하고 무관한 씬은 보존한다.
- [X] T046 Play Mode에서 프레임 진행, 메뉴/체험 위치·방향, 중복 선택, 9단계/오답 중 메뉴 복귀, 재진입 새 세션, 소리/반응/입력 잔류, 중단/복귀 및 전체 교육 순환을 검증하고 Assets/_02LWS/Documentation/Validation/SingleScene.md에 실제 결과를 기록한다. 기기 검증과 구분한다.
- [X] T047 Assets/_02LWS/Documentation/Integration.md 및 AcceptanceSummary.md·ExportImport.md의 현재 상태를 갱신하고 의존성·컴파일·씬 구조 검사 후 Artifacts/FireNotification.unitypackage를 다시 Export해 최종 소스와 일치시킨다. 기존 메뉴 원본 씬은 패키지에서 제외한다. Quest 2/성능/clean Import 미검증은 완료 처리하지 않는다.

순서: T043 → T044 → T045 → T046 → T047. FR-022/SC-009는 T043~T047이 담당한다.

변경 검증: Play Mode 149개 검사 통과, 콘솔 오류 0, missing script/버튼 대상 누락 0. 독립 코드 검토에서 새 Critical/Important 결함 없음. 패키지 131개 경로·원본 및 meta 바이트 일치, 포함 씬은 FireNotification 하나. 상세 검사·SHA-256은 `Artifacts/SingleScenePackageAudit.json`과 `Artifacts/SHA256SUMS.txt`에 기록한다. 기존 리그의 외부 Starter Assets 의존 40개는 유지되므로 수신 프로젝트의 prerequisite이며, 자체 완결 패키지·새 APK·Quest·clean Import 성공으로 표시하지 않는다. T037/T038/T041/T042는 미완료다. `.specify/extensions.yml`이 없어 전후 훅은 없다.

## Phase 9: 2026-11-05 변경 — 공용 객체 유지

- [X] T048 FR-023/SC-010과 공용/콘텐츠별 객체 경계를 spec.md·plan.md·research.md·data-model.md·contracts/experience-contract.md·quickstart.md에 반영한다. AGENTS.md와 체크리스트는 수정하지 않는다.
- [X] T049 Unity Editor에서 FireNotification.unity의 기존 XR rig·EventSystem·SpaceNavigation과 기본 Directional Light를 SharedSystems로 묶고 ExperienceLifecycle을 SharedLifecycle로 옮겨 참조를 재연결한다. 기본 조명의 회전·광량과 기존 GUID·공용 객체 수를 보존하고 화재 Point Light·체험 효과/진행은 콘텐츠에 유지한다.
- [X] T050 Play Mode에서 공용 조명·XR/입력·공간 전환·중단 감지의 활성 상태/동일 인스턴스·개수, 메뉴 중단 상태 진입, 기존 전환/중단/초기화 동작과 화면을 검증하여 Assets/_02LWS/Documentation/Validation/SharedSystems.md에 기록한다. 통합 안내를 갱신하고 패키지를 재Export·원본 대조한다. 이전 실기기·성능·수신 프로젝트 미완료는 유지한다.

순서: T048 → T049 → T050. 기존 미완료 T037/T038/T041/T042를 임의로 완료 처리하지 않는다.

공용화 검증: 기존 단일 씬 회귀 149개 + 공용 객체 검사 90개 통과, 콘솔 오류/누락 스크립트 0. 기본 Directional Light의 회전·광량 보존, 메뉴에서도 활성, 공용 lifecycle의 메뉴 중단 상태 반영 확인. 최종 패키지 132개 항목은 원본/meta와 일치하며 씬은 FireNotification 하나다. 기존 외부 Starter Assets 의존 40개 및 T037/T038/T041/T042 미완료는 유지한다. `.specify/extensions.yml`이 없어 전후 훅은 없다.

## Phase 10: 편집 중 모든 공간 표시

- [X] T051 단일 씬에서 MenuSpace·FireNotificationSpace를 활성화해 저장하고 관련 스펙킷 문서에 편집/실행 구분을 명시한다.
- [X] T052 Play 시작 시 메뉴 활성·체험 비활성·세션 미시작 및 종료 후 양 공간 활성 복원을 확인하고 통합 안내·패키지를 갱신한다.

T052 결과: 편집 시 두 공간 활성, Play 시작 시 메뉴만 활성·체험 세션 없음, 종료 후 두 공간 활성 복원 확인. 콘솔 오류 0. 패키지 원본/meta 대조 일치. 전후 훅 없음. 기존 Quest/성능/Import 미완료는 유지한다.

## Phase 11: design.md 공통 UI 적용 (US5)

기준: 루트 design.md v0.1.1, spec.md US5/FR-024~029/SC-011~013, plan.md의 UI 적용 절. 기존 완료 표시는 이전 버전 증거이며 UI 변경 후 재검증을 대신하지 않는다.

- [X] T053 design.md 적용 범위·기존 중단 규칙과의 차이·제출 경계를 specs/001-fire-notification/spec.md·plan.md·research.md·data-model.md·contracts/experience-contract.md·quickstart.md에 반영한다. 체크리스트 마커와 AGENTS.md는 수정하지 않는다.
- [X] T054 [US5] Assets/_02LWS/UI/Fonts/에 Noto Sans CJK KR Regular 원본·TMP·fallback·라이선스를 준비하고 Assets/_02LWS/Documentation/ThirdPartyNotices.md에 출처·재배포 조건을 기록한다. 기존 폰트를 이름만 바꿔 대체하지 않는다.
- [X] T055 [US5] T054 후 Assets/_02LWS/UI/Theme/과 Prefabs/에 design.md 5~9절 토큰·둥근 패널·아이콘·버튼 상태 공통 자원을 구성하고 기존 Assets/_02LWS/Prefabs/UI/와 FireNotification.unity의 메뉴·선택·다음·복귀·자막에 연결한다. 필수 문구 대비 4.5:1 및 150% 레이아웃을 점검한다.
- [X] T056 [US5] T055 후 Assets/_02LWS/Scripts/Runtime/FireNotificationController.cs와 Scripts/Presentation/의 모든 진행·음성·경보·자막·NPC·화재 연출에 내부 일시정지 게이트를 연결한다. 같은 지점 재개, 시스템 중단 시 단계 재시작, 중단 중 내부 메뉴 유지 및 해제 후 한 번 재진입을 구현한다. 전역 timeScale은 변경하지 않는다.
- [X] T057 [US5] T056 후 Assets/_02LWS/UI/Prefabs/와 Scripts/Interaction/에 일시정지·설정·재정렬·확인창 및 SharedSystems 연결부를 구성한다. 음성/효과음·자막·100~150% 글자를 같은 실행 중 유지하고 모달 뒤의 선택·벨·텔레포트를 차단한다. PC Esc/마우스와 VR 화면 진입 버튼, 현재 입력 표시를 연결한다.
- [X] T058 [US5] T057 후 Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs 및 CompletionExperience.cs와 Assets/_02LWS/UI/Prefabs/의 결과 화면을 연결한다. 확인된 재시작은 새 세션, 체험 중 복귀는 확인 후 메뉴, 완료 복귀는 직접 메뉴로 처리하고 실제 수행 기록만 표시한다.
- [X] T059 [US5] T058 후 Editor Play Mode에서 US5 전체 수락 시나리오·9단계/오답별 내부 메뉴·시스템 중단 겹침·모달 입력 차단·설정 유지·150% 긴 문구·반복 새 press·기존 교육 순환을 확인하고 Assets/_02LWS/Documentation/Validation/SharedUI.md에 실제 결과와 미검증을 기록한다. 기존 자동 테스트가 있으면 실행한다.
- [ ] T060 [US5] T059 후 Quest 2에서 기본/150% 한국어·대비·실제 글리프/버튼 시야각·레이 선택·텔레포트 차단·내부 일시정지와 탈착/시스템 메뉴 복귀를 검증해 Assets/_02LWS/Documentation/Validation/SharedUI.md에 기기/빌드/입력과 결과를 기록한다. 기기 접근이 없으면 미완료로 유지한다.
- [X] T061 T059 후 Assets/_02LWS/Documentation/Integration.md·Validation/AcceptanceSummary.md·ExportImport.md를 갱신하고 Artifacts/FireNotification.unitypackage를 재Export하여 원본/meta·폰트/UI 의존성·공용 객체 중복을 대조한다. T060 및 기존 기기/Import 미완료는 명시한다.

의존성: T053 → T054 → T055 → T056 → T057 → T058 → T059 → T060. T061은 T059 이후 실행 가능하고 T060을 건너뛴 결과를 기기 합격으로 기록하지 않는다. 새 작업은 동일 씬/진행 코드를 공유하므로 병렬 표시하지 않는다.

요구 매핑: FR-024/025 → T054~T055,T059~T060; FR-026 → T056~T057,T059~T060; FR-027 → T057,T059~T060; FR-028 → T057~T059; FR-029 → T055,T057,T059~T060; SC-011~013 → T059~T060. 초기 검토 단위는 스타일 적용(T054~T055)이며 전체 범위 완료는 나머지 구현·검증을 필요로 한다.

Phase 11 Editor results: T054~T059 completed with 111 progression/lifecycle checks and 35 actual Input System mouse/layout checks, all passing. Exact local font and all 150% subtitle segments verified, 23 buttons bound, missing scripts 0, shared XR/EventSystem/listener each 1. Review fixed desktop-build mouse rejection and found no remaining critical/important issue. T060 stays unchecked: ADB device list empty. T037/T038/T041/T042 remain unchecked. Package update and integrity check are T061; an older APK does not represent this UI. Details: Assets/_02LWS/Documentation/Validation/SharedUI.md. No extension hooks registered (.specify/extensions.yml absent).

T061 최초 결과: Unity Editor Export 155개 파일, package/source/meta 바이트 불일치 0. 포함 씬 FireNotification 하나. Artifacts/SharedUIPackageAudit.json 및 SHA256SUMS.txt에 현재 패키지 해시 기록. 기존 외부 Starter Assets 의존 40개 유지, 새 UI 폰트/셰이더 내부화. Quest/성능/수신 프로젝트 검증 미완료는 유지.

종료 보완: UI 모달이 열린 채 종료할 때의 파괴된 컨트롤러 참조를 수정했다. UI 비활성화/재활성화/컨트롤러 선행 파괴 7개 검사 및 반복 입력·배치 35개 통과. 실제 설정창 열린 Play Mode 종료 후 콘솔 오류 0. 최종 패키지는 이 수정 이후 다시 Export·대조한다.

최종 T061: 종료 수정 포함 156개 파일 재Export, 원본/meta 불일치 0개. SHA-256 2b5e9a7d743281a7f812ca400ae701b115269334ab3d9446a97bb1ef3784edf5. T060 및 기존 실기기/성능/수신 프로젝트 미완료는 유지한다. 전후 확장 훅 없음.

## Phase 12: Meta 공식 공간 UI 지침 반영

- [X] T062 공식 근거와 프로젝트 조정값을 구분해 명세·계획·조사·계약·검증 절차를 갱신한다.
- [X] T063 ComfortFollowUI.cs 및 SharedUIController.cs에 작은 움직임을 무시하는 수평 추종, 누르는 동안 안정화, Origin 이동 후 즉시 재배치와 실제 벨 라벨 제외를 구현한다.
- [X] T064 Editor에서 작은/큰 이동·고개 회전·텔레포트·누르는 동안 안정화·재활성화를 검증하고 기존 교육/메뉴 회귀를 수행한다.
- [X] T065 변경 소스와 메타를 포함해 unitypackage를 다시 Export·대조한다.
- [ ] T066 실제 VR에서 가독성·추종 불편감·레이 조작성을 확인한다. 기기 결과 없이는 완료 표시하지 않는다.

Phase 12 결과: T062~T065 완료. Editor 추종/입력 13개 및 기존 회귀 111개 통과. 실제 VR 검증 T066/T060은 미완료. 패키지 161개 파일, 원본/meta 불일치 0. SHA-256 fd9317801dbebb1ebe4b10c6454285de01d9740d3a527c22c74caf98f130a49b. 새 APK 없음. 전후 훅 없음. 상세 제한은 Assets/_02LWS/Documentation/Validation/ComfortFollow.md.
