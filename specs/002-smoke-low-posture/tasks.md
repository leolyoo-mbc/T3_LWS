# Tasks: 연기 속에서 자세 낮추기

**Input**: `specs/002-smoke-low-posture/`의 spec.md, plan.md, research.md, data-model.md, contracts/, quickstart.md.

**Status**: 구현 진행 중. 완료 체크는 아래 검증 기록이 있는 항목만 반영한다. T014/T047 음성 청취·출처 점검과 T046 Editor 복구 보완은 2026-11-12 완료했다. 실기기·학습·최종 제출 검증은 별도로 남긴다. 현재 헌장 v1.1.0을 적용하며 후속 실행 변경은 아래 시각 검증 게이트를 따른다.

**확정/제안**: 실제 숙임, 준비 완료 순간 기준, 연속 2초 자동 인정, 연습 중 고정 연기층은 사용자 확정이다. 20% 하강·0.03 m 해제 여유·화면 문구·타입명은 AI 설계 제안이다. 튜닝으로 2초 조건이나 교육 범위를 바꾸지 않는다.

**Tests**: 명세의 시나리오를 Editor 전용 입력 주입과 Play Mode로 수동 검증한다. 입력 순서·기대/실제 결과·증거를 기록하며 plan.md와 quickstart.md Q1도 같은 방식을 따른다. 새 자동 테스트 코드/어셈블리는 만들지 않는다. 기존 관련 자동 테스트가 있으면 추가 실행한다.

## Format and paths

모든 작업은 `- [ ] T번호 [P 선택] [US번호 해당 시] 설명 + 경로`다. `[P]`는 명시한 선행 작업이 끝난 뒤 다른 파일에서 병렬 진행할 수 있다는 뜻이다. 병렬 실행 예시는 인력 배치 가능성을 나타내며 에이전트 실행 지시가 아니다.

경로는 저장소 루트 기준이다. 새 실행 자산·스크립트·제출 문서는 `Assets/_02LWS/`에 두고 Unity가 만든 `.meta`를 보존한다. 작업 안에 여러 파일이 나열되면 해당 연결을 함께 완결한다. 기존 `Assets/XR/Settings/OpenXRPackageSettings.asset` 변경과 AGENTS.md를 덮어쓰지 않는다.

## 후속 변경의 시각 검증 게이트

헌장 v1.1.0에 따라 이후 실행 결과에 영향을 주는 구현·수정(T046 및 후속 코드/자산 조정 포함)에 적용한다. 과거 완료 작업은 소급 녹화하지 않으며 문서만 수정하는 작업에는 새 녹화를 요구하지 않는다. 기존 기록은 일괄 이동하지 않는다.

실제 실행 MP4에 변경 기능의 진입·핵심 동작·결과를 포함한다. 녹화 불가 시 주요 단계의 대체 시각 자료와 사유·검증 한계를 기록한다. 실행 자체가 불가능하면 가능한 실제 Editor 화면과 미실행 사실을 남긴다. 대체 자료도 없으면 미확보 원인을 보고하고 게이트 충족으로 표시하지 않는다. 목업·텍스트 로그는 실제 실행 영상 또는 대체 시각 자료를 대신하지 못한다.

영상·스크린샷·동반 원본 기록은 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-작업명/`에 보관하고 기존 폴더를 덮어쓰지 않는다. 같은 폴더의 `verification.md`에 작업 식별자·일시·씬·환경/기기·입력 방식·확인 동작·파일명·실제 결과·미검증 항목과 대체 자료 사용 시 녹화 불가 사유·한계를 적는다. `git check-ignore`와 `git ls-files`로 제외 규칙 적용과 비추적 상태를 확인하고 강제 추가하지 않는다. 검증 원본을 Assets나 제출 패키지에 포함하지 않는다.

아래 `Assets/_02LWS/Documentation/Validation/` 경로는 제출용 결과 요약·원본 참조 문서다. 후속 변경의 검증 원본은 위 공통 경로에 둔다. 파일 존재·열람 가능 여부를 확인하고 완료 보고에 대표 파일 직접 링크와 결과·한계를 포함한다. Editor/PC/Quest 증거를 구분하며 자료 존재만으로 기능 합격을 선언하지 않는다.

## Phase 1: Setup

**목표**: 기존 상태와 검증 방법을 기록하고 필요한 폴더만 준비한다. 엔진·패키지를 재설치하거나 업그레이드하지 않는다.

- [X] T001 현재 `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `Packages/packages-lock.json`과 `Assets/_02LWS/Scenes/FireNotification.unity`를 확인하고 Git 변경·Console 상태·콘텐츠 1 기본 실행·Q1의 Editor 주입 검증 환경을 `Assets/_02LWS/Documentation/Validation/SmokePostureBaseline.md`에 기록한다. 기존 사용자 변경은 보존한다.
- [X] T002 [P] T001 후 `Assets/_02LWS/Scripts/Domain/SmokePosture/`, `Assets/_02LWS/Data/SmokePosture/`, `Assets/_02LWS/Audio/Voice/SmokePosture/`, `Assets/_02LWS/Prefabs/SmokePosture/`를 준비하고 기존 Domain/Runtime asmdef 경계를 유지한다. 빈 별도 프로젝트·새 XR Rig·새 패키지는 만들지 않는다.
- [X] T003 [P] T001 후 quickstart Q1~Q9와 FR-001~013/SC-001~006을 옮긴 결과 기록 틀을 `Assets/_02LWS/Documentation/Validation/SmokePostureAcceptance.md`에 만든다. Editor/Quest/Import/학습 평가를 분리하고 미실시를 PASS로 기록하지 않는다.

## Phase 2: Foundational — 모든 스토리의 선행 조건

**목표**: 공통 시스템이 활성 콘텐츠를 참조하도록 연결한다. 이 단계에서는 콘텐츠 1만 등록해도 기존 기능이 유지돼야 한다.

- [X] T004 `Assets/_02LWS/Scripts/Runtime/ContentExperienceAdapter.cs`에 시작·정지·재시작·일시정지·복귀 요청·설정 적용·차단·결과·상태/메뉴 이벤트의 직렬화 가능한 MonoBehaviour 계약을 구현한다. 계약 범위는 `specs/002-smoke-low-posture/contracts/content-integration.md`를 따른다. 메뉴의 활성 콘텐츠 null도 지원한다.
- [X] T005 T004 후 `Assets/_02LWS/Scripts/Runtime/FireNotificationAdapter.cs`에서 기존 FireNotificationController 메서드/이벤트·안내 음원/자막·결과를 연결한다. 기존 `Assets/_02LWS/Scripts/Runtime/FireNotificationController.cs`의 교육 순서·물리 벨 동작을 변경하지 않고 구독 해제와 중복 Stop을 처리한다.
- [X] T006 T005 후 `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`에 ContentEntry와 활성 항목 선택을 구현한다. 필드 제약은 id의 “중복 없는 문자열. fire-notification / smoke-low-posture”, root/anchor의 “씬 참조, 누락 불가. root와 XR Origin 공유 시스템 분리”, adapter의 “콘텐츠 실행·상태·설정 연결 참조”, locomotionAllowed의 “연기 체험 false. 공통 모달 정책과 AND 결합”을 그대로 적용한다. 등록 2개/활성 0개 또는 1개를 지원하되 두 번째 등록은 T017에서 연결하고, 무인자 StartContent()는 콘텐츠 1 호환으로 유지한다. 한 프레임 지연 전환·Stop·높이 보존 배치·중복/누락 ID 거부를 처리한다.
- [X] T007 T006 후 `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`의 콘텐츠 1 직접 참조를 활성 어댑터로 교체한다. pause/재시작/복귀/결과·설정 전달·음원 원래 볼륨 보존·활성 세션 없는 메뉴 설정·Canvas 명시 등록을 구현한다. 음량·자막·글자 150% 설정을 콘텐츠 교체에도 유지하고 조작/오류 안내는 자막 끄기로 숨기지 않는다.
- [X] T008 [P] T006 후 `Assets/_02LWS/Scripts/Interaction/ContentMenuInput.cs`에 명시 ID 선택을 연결하고 기존 무인자 버튼 호출 호환·놓았다 다시 누르기·전환 중 중복 선택 거부를 유지한다. T007과 다른 파일에서 병렬 진행할 수 있다.
- [X] T009 T007 후 `Assets/_02LWS/Scripts/Interaction/MenuTeleportationProvider.cs`와 `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`에 이동 정책을 연결한다. 콘텐츠 허용 AND 모달 닫힘 AND lifecycle 허용 AND 전환 중 아님으로 판정하고 pending 요청을 취소한다. 연기 체험의 회전/연속 이동 제공자도 차단하고 실제 머리·손 추적은 유지한다. pause 해제로 콘텐츠 이동 금지가 풀리지 않게 한다.
- [X] T010 T008/T009 후 `Assets/_02LWS/Scenes/FireNotification.unity`의 기존 콘텐츠 1 등록·메뉴 버튼·SharedSystems 참조를 새 연결부에 재연결한다. 공유 XR Origin/EventSystem은 각 1개만 유지하고 새 콘텐츠를 아직 완료된 항목으로 노출하지 않는다.
- [X] T011 `Assets/_02LWS/Scenes/FireNotification.unity`에서 콘텐츠 1 전체 순환·직접 벨·기존 이동·모달·설정·재시작·복귀를 실행하고 `Assets/_02LWS/Documentation/Validation/SmokePostureFoundation.md`에 결과를 기록한다. Console 오류 또는 기존 동작 회귀가 있으면 수정 후 재검증한다.

**Checkpoint**: T001~T011이 끝나야 스토리 연결을 시작한다. 콘텐츠 1 회귀가 남아 있으면 다음 스토리의 완료로 넘어가지 않는다.

## Phase 3: US1 — 연기와 낮은 자세의 관계 이해 (P1)

**목표**: 제자리에서 상부 연기 축적을 관찰하고 연기 흡입 감소라는 이유를 이해한다.

**독립 검증**: 자세 판정 구현 없이 원리 음성·자막과 초기 축적→고정 상부 연기를 확인한다. 30초 대기/카메라 높이 변경에도 층이 내려오거나 연기 전체가 사라지지 않는다. 이 중간 산출물을 자세 연습 완료로 표시하지 않는다.

- [X] T012 [US1] `specs/002-smoke-low-posture/contracts/experience-ui.md`를 기준으로 원리·준비·인식·한계 안내의 검토용 대본과 공식자료 적용 조건을 `Assets/_02LWS/Documentation/SmokePostureScript.md`에 정리한다. 흡입 감소 중심, ‘대피가 가능한 경우’, 자세만으로 안전하지 않음, 2초는 앱 조건이라는 구분을 유지하고 새 문구를 사용자 확정으로 표시하지 않는다.
- [X] T013 [P] [US1] `Assets/_02LWS/Scripts/Presentation/SmokeLayerPresenter.cs`와 `Assets/_02LWS/Prefabs/SmokePosture/SmokeLayer.prefab`에 InitialCollection / SteadyPractice / Frozen / Stopped를 구현한다. “Frozen은 직전 연출 상태를 보존하고 종료 시 입자를 정리한다”, “SteadyPractice의 층 경계는 공간에 고정하며 사용자 자세·대기 시간으로 바뀌지 않는다”를 적용한다. 기존 내부 FireSmoke 자산을 호환성 확인 후 재사용하며 전역 fog·성공 연기 제거·카메라 추종을 넣지 않는다. T012와 병렬 가능하다.
- [X] T014 [US1] T012 후 `Assets/_02LWS/Audio/Voice/SmokePosture/`에 대본과 일치하는 재배포 가능한 로컬 한국어 안내를 확보하고 `Assets/_02LWS/Data/SmokePosture/GuidanceCatalog.asset`에 큐를 구성한다. `Assets/_02LWS/Scripts/Presentation/SmokeGuidanceCatalog.cs`에서 기존 GuidanceCue/GuidancePresenter를 재사용하되 원리/연습/한계 전용 필수 ID를 검증한다. 기존 `Assets/_02LWS/Scripts/Presentation/GuidanceCatalog.cs`는 sealed이며 situation/next/bell/completion/feedback/shout를 필수로 요구하므로 새 큐를 그대로 넣거나 콘텐츠 1 검증을 약화하지 않는다. EducationCue의 “고유 key, 로컬 AudioClip, 시간 구간별 한국어 자막” 제약과 한국어 글리프 검증을 적용한다. 유료 구매·계정 사용·외부 전송이 필요하면 실행 권한을 먼저 확인하고, 확보하지 못한 음성을 완료로 처리하지 않는다.
- [X] T015 [US1] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`에 어댑터 수명·AwaitingReady→PrincipleGuidance→Practice 표시와 기존 GuidancePresenter 연결을 구현한다. 원리 안내 누락/잘못된 큐는 구성 오류로 알리고 메뉴 복귀를 제공하며 성공으로 건너뛰지 않는다. 이 단계의 Practice는 실제 판정 없이 대기하고, 준비 높이 캡처는 US2에서 연결한다.
- [X] T016 [US1] T013~T015 후 `Assets/_02LWS/Scenes/FireNotification.unity`에 SmokePostureSpace·고정 연기 영역·진입 anchor·한국어 안내/준비/중단 UI를 배치한다. 기존 Theme/Fonts/FreshPressButton/추적 UI 입력을 재사용하고 새 Canvas를 명시 등록한다. 이동 표면/앵커·별도 XR Rig/EventSystem을 만들지 않는다.
- [X] T017 [US1] `Assets/_02LWS/Scenes/FireNotification.unity`의 메뉴에 smoke-low-posture를 등록하고 새 root/controller/anchor/이동 불가 정책을 연결한다. 진입과 원리 관찰 후 메뉴 복귀를 확인하고 관찰만으로 자세 연습 완료나 Result를 표시하지 않는다.
- [X] T018 [US1] quickstart Q2/Q3의 원리 부분을 `Assets/_02LWS/Scenes/FireNotification.unity`에서 검증하고 `Assets/_02LWS/Documentation/Validation/SmokePostureObservation.md`에 30초 대기·반복 높이 변경·초기 축적·고정 경계·이동 차단·음성/자막·미구현 판정의 완료 방지 결과를 기록한다.
- [X] T019 [US1] `Assets/_02LWS/Documentation/Validation/SmokePostureObservation.md`에 서기/숙이기 관찰 위치에서 안내 배치·자막 끄기·음성 0·글자 150%·중단 버튼 검증 결과를 추가하고 필요한 `Assets/_02LWS/Scenes/FireNotification.unity` UI 배치만 조정한다. 환경/연기층을 UI 추종 대상으로 바꾸지 않고 Editor 결과와 실제 기기 읽기를 구분한다.

**Checkpoint / 관찰 MVP**: 원리 관찰을 독립 시연할 수 있다. 전체 기능의 완료 조건은 아니며 실제 자세 연습을 공개하려면 US2와 US3까지 필요하다.

## Phase 4: US2 — 실제로 몸을 낮추는 자세 연습 (P1)

**목표**: 유효 준비 순간의 높이를 기준으로 실제 머리 낮춤을 인식하고 연속 2초를 한 번만 인정한다.

**독립 검증**: 원리 연출 없이 Editor 전용 입력으로 시작 높이 3조건, 기립/낮춤/재기립, 1.99초/2초, 추적 상실·동시 중단을 재현할 수 있다. 실제 기기 인식 합격은 최종 Phase에서 별도 검증한다.

- [X] T020 [US2] `Assets/_02LWS/Scripts/Domain/SmokePosture/PostureTuning.cs`와 `Assets/_02LWS/Scripts/Domain/SmokePosture/HeadPoseSample.cs`에 순수 C# 입력/설정을 구현한다. dropRatio “0.20. 0보다 크고 1보다 작음”, releaseMarginMeters “0.03 m. 0 이상, baseline×dropRatio보다 작아야 함”, requiredHoldSeconds “2초. 명세 확정값이며 임의 조절 기능 없음”, heightInOrigin “XROrigin 기준 머리 y(m), 유한값”, positionTracked “장치 유효·isTracked·Position 비트 모두 충족”, referenceRevision “현재 추적 기준 공간 버전”, deltaSeconds “연속 유효 프레임의 경과 시간, 음수/NaN 금지”, lifecycleAllowed “앱/장치 세션이 판정 가능한 상태”를 그대로 적용한다.
- [X] T021 [US2] T020 후 `Assets/_02LWS/Scripts/Domain/SmokePosture/SmokePostureSession.cs`에 실행 모델을 구현한다. runId “새 진입/재시작마다 바뀌는 실행 번호, 영구 식별자 아님”, stage/stageExecution “아래 단계 및 콜백 무효화용 실행 번호”, baselineHeight “유효 준비 완료 때만 설정하는 양수 유한 m, 초기 null”, referenceRevision “기준 추적 공간 버전. 변경 시 미완료 기준 폐기”, holdSeconds “유효한 낮은 자세가 이어진 시간, 0~2”, lowRecognized “히스테리시스 판정 내부 상태, 초기 false”, postureCompleted “해당 실행의 완료 고정값. 초기 false, 한 번만 true”, blockers “UserPause / Lifecycle / Tracking / Configuration 차단 이유 집합”, closed “메뉴 이탈/비활성/교체 시 true. 이후 입력/콜백 무시”를 적용한다. 준비 이전 성공과 Practice 이전 시간 합산을 금지하고 지연 콜백은 runId/stageExecution 모두 확인한다.
- [X] T022 [P] [US2] T020 후 `Assets/_02LWS/Scripts/Interaction/HeadPoseSampleProvider.cs`에 실제 head 추적 유효성·Origin 기준 높이·trackingOriginUpdated/모드 변경·Origin 불연속 감지를 구현한다. “Origin scale이 1이 아니거나 참조 누락이면 Configuration 차단으로 안내한다”와 “Input adapter는 추적/세션 불가 시간이나 앱 정지 경과를 복구 첫 샘플에 포함하지 않는다”를 적용한다. UI 재정렬은 기준 변경과 구분하며 T021과 다른 파일에서 병렬 가능하다.
- [X] T023 [US2] T021/T022 후 `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`와 `Assets/_02LWS/Scenes/FireNotification.unity`에 준비 완료 순간의 유효 샘플 캡처·설정 데이터 연결·추적 불가 안내를 연결한다. `Assets/_02LWS/Data/SmokePosture/PostureTuning.asset`에는 제안 초기값을 두되 2초 변경용 사용자 설정을 노출하지 않는다. 자동 기준 캡처나 서 있음의 전신 검증을 주장하지 않는다.
- [X] T024 [US2] `Assets/_02LWS/Scripts/Domain/SmokePosture/SmokePostureSession.cs`에 entryHeight=baselineHeight×(1−dropRatio), 진입 ≤ entryHeight, 해제 > entryHeight+releaseMarginMeters와 연속 유효 2초 인정을 구현한다. 무효 설정은 판정 보류, 일어서기는 시간 초기화, 완료는 한 번 고정하고 이후 기립으로 취소하지 않는다.
- [X] T025 [US2] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`에 실제 샘플→세션 판정→1초 이내 인식 피드백→2초 시 즉시 일어서도 됨 안내를 연결한다. 실패 시간 제한·점수·반복 ‘더 깊이’ 압박·성공 연기 제거 없이 Practice의 무제한 대기를 유지한다. `Assets/_02LWS/Scripts/Presentation/SmokePostureValidationDisplay.cs`에 quickstart Q4의 경계 통과/피드백 표식·실행/프레임 번호·동일 시계 시각 표시를 구현한다. 원래 입력/표시 처리 프레임에 갱신하며 Editor/개발 빌드에서 명시적으로 켠 경우만 보인다. Quest에서 합성 입력/진행 변경은 제공하지 않고 측정값은 실행 종료 시 폐기한다.
- [X] T026 [US2] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`와 `Assets/_02LWS/Scripts/Domain/SmokePosture/SmokePostureSession.cs`에 모달/lifecycle/추적 상실/기준 변경을 완료보다 먼저 처리하는 경로를 구현한다. 완료 이전 lowRecognized=false/holdSeconds=0, 복수 차단 모두 해제 후 재개, 일반 pause의 음성/자막/입자 보존, 기준 변경의 재준비, 이미 인정된 완료 유지, 이탈/재시작의 콜백 무효화를 적용한다.
- [X] T027 [US2] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`에 UNITY_EDITOR 전용 높이·시간·차단·기준 변경 주입/상태 확인 경로를 제공한다. “시험용 샘플 공급자는 Editor/테스트 전용이며 플레이어가 가짜 높이로 완료할 경로를 제공하지 않는다”를 적용하고 신체정보 파일/분석 로그는 추가하지 않는다.
- [X] T028 [US2] quickstart Q1을 T027 경로로 수행하고 `Assets/_02LWS/Documentation/Validation/SmokePostureDetection.md`에 합성 높이 1.2/1.6/1.9 m별 기립/낮춤/재기립/추적 상실, 준비 전 성공 0건, 1.99초 초기화/2초 한 번 인정, 경계 포함/초과, 무효 수치·추적의 완료 0건을 기록한다. 기존 관련 자동 테스트가 있으면 추가 실행하고 수동 주입 결과와 구분한다.
- [X] T029 [US2] quickstart Q5를 `Assets/_02LWS/Scenes/FireNotification.unity`에서 수행하고 `Assets/_02LWS/Documentation/Validation/SmokePostureInterruption.md`에 1.9초 후 pause/추적 상실·기준 변경·동일 프레임 중단 우선·복구 새 2초·이미 완료 유지·입자/음성 정지·모달 뒤 입력 차단을 기록한다. 실제 OS/장치 중단 검증은 T039/T040〈Quest 중단 검증〉에서 구분한다.
- [X] T030 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`에 준비부터 관찰·Practice·완료 인정까지 실제 입력 경로를 최종 연결하고 `Assets/_02LWS/Documentation/Validation/SmokePostureDetection.md`에 추적/구성 오류·편하게 숙이기 안내·언제든 중단·무제한 대기를 확인한다. 한계 안내/결과는 US3 연결 전 완료된 전체 체험으로 표시하지 않는다.

**Checkpoint**: US2의 판정은 독립적으로 확인 가능하다. US1과 연결한 Editor 정상 경로를 확보한다. 실제 편안한 숙임·지연·Quest 판정은 T039~T041〈성능 검증〉이 끝나기 전 합격으로 표시하지 않는다.

## Phase 5: US3 — 교육의 한계 확인과 메뉴 복귀 (P2)

**목표**: 자세 연습과 실제 탈출을 구분하고 완료 안내·재시작·메뉴 복귀·재진입을 완결한다.

**독립 검증**: T027의 합성 입력으로 새 준비 선택→원리 안내 완료→Practice→유효한 낮은 자세 연속 2초를 진행해 한계 안내·결과를 확인한다. 단계 직접 진입 기능은 만들지 않는다. 실제 기기 자세 인식과 구분해 화면/복귀를 검증한다. 새 실행은 항상 새 준비부터 시작하고 네 단계 어디서든 메뉴 복귀가 가능하다.

- [X] T031 [US3] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`에 CompletionGuidance→Result를 연결하고 `Assets/_02LWS/Data/SmokePosture/GuidanceCatalog.asset`의 한계 음성/자막을 재생한다. 아래까지 퍼질 수 있음·낮은 자세만으로 안전하지 않음·대피 가능한 경우 낮은 자세로 신속 이동·제자리 연습 구분을 모두 제공한다. 원리/한계 안내 완료 전 건너뛰기를 금지한다.
- [X] T032 [US3] `Assets/_02LWS/Scenes/FireNotification.unity`와 `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`에 콘텐츠별 제목·실제 관찰/숙임 기록·다시 체험하기/콘텐츠 선택으로를 연결한다. 점수·합격 등급·탈출 성공·안전 확보·정확한 전신 검증을 표시하지 않는다.
- [X] T033 [US3] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`와 `Assets/_02LWS/Scripts/Domain/SmokePosture/SmokePostureSession.cs`의 종료/재시작/재진입에서 이전 큐·입자·이벤트를 취소하고 기준/유지 시간/완료/연출을 초기화한다. “세션/샘플/기준은 실행 종료 시 폐기한다”, “새 키/머리 위치 로그·파일·분석 전송은 추가하지 않는다”를 적용하고 공통 설정만 유지한다.
- [X] T034 [US3] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`와 `Assets/_02LWS/Scenes/FireNotification.unity`의 네 단계(시작 확인/원리/연습/완료 안내)에 메뉴 접근·기존 확인창 정책·불편/미인식 중단을 연결한다. 중단 실행은 완료로 기록하지 않고 종료 후 늦은 콜백을 무시한다.
- [X] T035 [US3] quickstart Q3/Q6에 따라 정상 순환 3회 및 네 단계 각각의 중단·재시작·재진입을 `Assets/_02LWS/Scenes/FireNotification.unity`에서 검증하고 `Assets/_02LWS/Documentation/Validation/SmokePostureLoop.md`에 이전 기준/완료/음성 잔존 0건·중단 후 의도치 않은 완료 0건·필수 교육 내용 100% 포함을 기록한다.
- [X] T036 [US3] `Assets/_02LWS/Documentation/Validation/SmokePostureLoop.md`에 T027의 Editor 합성 입력으로 새 실행에서 준비 선택→원리 안내 완료→Practice→유효한 낮은 자세 연속 2초를 진행한 뒤 한계 안내·결과·설정 보존·메뉴 복귀 검증을 추가한다. 단계 직접 진입이나 필수 안내 건너뛰기를 사용하지 않는다. 자세 연습 인정 직후 일어나도 완료가 취소되지 않으며 한계 안내는 제공되는지 확인한다.

**Checkpoint**: 전체 체험의 Editor 경로를 완료한다. 실제 기기·학습 효과·Export 검증은 아래 작업으로 남는다.

## Phase 6: Polish & cross-cutting validation

**목표**: 기존 콘텐츠 회귀, 실제 기기 인식/성능/교육 효과, 내부 자산과 제출 패키지를 검증한다. 필요한 기기/참여자/수신 환경이 없으면 해당 작업을 미완료로 남긴다.

- [X] T037 quickstart Q7의 콘텐츠 1 전체 순환을 연기 체험 전후 `Assets/_02LWS/Scenes/FireNotification.unity`에서 실행하고 `Assets/_02LWS/Documentation/Validation/SmokePostureRegression.md`에 직접 벨·기존 이동·설정·모달·재시작·복귀·콘텐츠별 결과·단일 활성 root/EventSystem을 기록한다. 공통 연결부 수정 후 회귀가 생기면 수정한 경로를 재검증한다.
- [X] T038 현재 Android/OpenXR 조건을 확인해 `Assets/_02LWS/Scenes/FireNotification.unity`를 포함한 개발 빌드를 `Artifacts/SmokePosture/SmokePosture-development.apk`에 생성한 뒤 APK를 `Assets/_02LWS/Artifacts~/SmokePosture/`에 복사해 보관한다. Unity는 Assets 내부로 직접 빌드할 수 없으므로 출력 임시 경로와 보관 경로를 구분한다. Unity/패키지/빌드 설정·소스 상태·빌드 결과를 `Assets/_02LWS/Documentation/Validation/SmokePostureBuild.md`에 기록한다. 기존 사용자 XR 설정 변경을 임의 덮어쓰지 않는다.
- [ ] T039 T038 후 Quest 2에서 quickstart Q4/Q7의 실제 준비·편한 숙임·재기립·새 실행을 검증하고 `Assets/_02LWS/Documentation/Validation/SmokePostureQuest.md`에 인식 성공/오인식·Q4의 경계 통과/피드백 표식 및 D+2G≤1초 판정(오차 상한이 불명확하면 미검증)·2초 완료·컨트롤러·양안 표현·읽기·150% 글자·중단 버튼을 기록한다. 불편한 깊이를 요구하지 말고 제안 튜닝 값은 `Assets/_02LWS/Data/SmokePosture/PostureTuning.asset`에서 조정한 뒤 관련 검증을 다시 수행한다.
- [ ] T040〈Quest 중단 검증〉 T038 후 Quest 2에서 quickstart Q5/Q7의 헤드셋 벗기/시스템 pause/실제 추적 상실·복구/기준 재설정·네 단계 중단 및 비행기 모드 최초 실행을 검증하고 `Assets/_02LWS/Documentation/Validation/SmokePostureQuest.md`에 기록한다. Editor 주입으로 실제 OS 동작 검증을 대신하지 않는다. T039와 같은 기기/기록 파일을 사용하므로 순차 수행한다.
- [ ] T041〈성능 검증〉 quickstart Q8의 Quest 프로파일링을 수행하고 `Assets/_02LWS/Documentation/Validation/SmokePosturePerformance.md`에 CPU/GPU 시간·양안 표시·입자 수/크기/중첩·설정을 기록한다. 필요 시 `Assets/_02LWS/Prefabs/SmokePosture/SmokeLayer.prefab`을 조정하고 연기층 불변·교육 표현을 재검증한다. 72 Hz/13.9 ms는 AI 제안 비교 기준이며 사용자 확정 합격 기준으로 쓰지 않는다.
- [ ] T042 [P] T039/T040〈Quest 중단 검증〉의 안전한 사용 경로 확인 후 실제 체험자 5명으로 quickstart Q8을 수행하고 `Assets/_02LWS/Documentation/Validation/SmokePostureLearning.md`에 익명 번호·설명 요지·결과를 기록한다. 4명 이상이 두 이유를 모두 설명해야 SC-005 충족이며 신체 수치/식별정보를 요구하지 않는다. 장치/진행자가 별도 확보된 경우에만 T041〈성능 검증〉과 병렬 수행한다.
- [ ] T043 `Assets/_02LWS/Documentation/ThirdPartyNotices.md`와 `Assets/_02LWS/Documentation/Integration.md`를 갱신하고 신규 음성/재질/텍스처/셰이더/폰트의 출처·재배포 조건·.meta·필수 로컬 파일을 점검한다. quickstart Q9에 따라 `Assets/_02LWS/Scenes/FireNotification.unity`/프리팹 참조의 외부 경로·신체정보 저장/전송·새 마이크 입력 0건을 확인해 `Assets/_02LWS/Documentation/Validation/SmokePostureDependencies.md`에 기록한다. 역사적 문서의 sample 의존성 설명과 현재 상태를 구분한다.
- [ ] T044 T037~T043의 수정 반영 후 `Assets/_02LWS`를 최신 소스로 Export하여 `Assets/_02LWS/Artifacts~/SmokePosture/SmokePosture.unitypackage`에 보관하고 별도 수신 프로젝트에서 필요한 UPM/XR 조건을 갖춰 Import·Console 오류 0건·두 콘텐츠 전체 진행을 확인한다. 결과/버전/패키지 해시를 `Assets/_02LWS/Documentation/Validation/SmokePostureImport.md`에 기록한다. Artifacts~의 이전 패키지는 재귀적으로 포함하지 않으며 지정 수신 프로젝트가 생기면 그 환경도 확인한다.
- [ ] T045 quickstart Q1~Q9 및 FR-001~013/SC-001~006의 증거를 `Assets/_02LWS/Documentation/Validation/SmokePostureAcceptance.md`에 집계한다. `specs/002-smoke-low-posture/tasks.md` 체크 상태를 실제 증거와 대조하고 자동화 미수행·기기 부재·교육 평가·Import의 남은 제한을 구분한다. 이전 콘텐츠의 과거 PASS를 새 체험 결과로 복사하지 않는다.

## Dependencies & execution order

```text
T001 → (T002 ∥ T003)
  → T004 → T005 → T006 → (T007 ∥ T008) → T009 → T010 → T011
  → US1: (T012 ∥ T013) → T014 → T015 → T016 → T017 → T018 → T019
  → US2: T020 → (T021 ∥ T022) → T023 → T024 → T025 → T026 → T027 → T028 → T029 → T030
  → US3: T031 → T032 → T033 → T034 → T035 → T036
  → T037 → T038 → T039 → T040〈Quest 중단 검증〉 → (T041〈성능 검증〉 ∥ T042) → T043 → T044 → T045
```

T009는 T007, T010은 T008/T009, T016은 T013/T014/T015, T023은 T021/T022를 모두 기다린다. 그래프는 안전한 기본 순서다. T042는 별도 기기/진행자가 없으면 T041〈성능 검증〉 이후 수행한다. 이 경우에도 작업을 생략하지 않는다.

US1은 Foundation 후 다른 스토리 없이 관찰만 검증할 수 있다. US2의 도메인/추적 파일(T020~T022)은 Foundation 후 독립 작업 가능하지만 UI/정상 흐름 연결(T023 이후)은 US1 씬에 의존하므로 순차 진행한다. US3는 T027의 합성 입력으로 준비/원리 안내/연속 2초를 진행해 화면·복귀를 검증하며 US2의 판정 경로에 의존한다. 완료 상태를 직접 주입하지 않는다. 공통 씬/Controller/세션 파일을 동시에 수정하지 않는다.

기기 검증과 학습 평가를 제외한 구현을 먼저 진행할 수 있다. 해당 환경이 없으면 T039~T042/T044를 미완료로 유지하고 부분 결과를 별도로 보고한다. 필수 환경 부재를 해결하기 위해 교육 기능을 바꾸거나 완료 증거를 만들어내지 않는다.

## Parallel examples per story

| 범위 | 병렬 가능한 작업 | 합류 조건 |
|---|---|---|
| Setup | T002 폴더 / T003 검증 기록 틀 | T001 완료 후, T004 전에 합류 |
| Foundation | T007 공통 UI / T008 메뉴 입력 | T006 완료 후, T010 전 합류 |
| US1 | T012 대본 문서 / T013 연기 코드·프리팹 | T014는 T012 후, 씬 연결 T016 전에 합류 |
| US2 | T021 세션 / T022 추적 제공자 | T020의 입력 형식 확정 후, T023 전에 합류 |
| US3 | 내부 병렬 없음 | 같은 Controller/씬/세션을 바꾸므로 T031~T036 순차 |
| 실제 검증 | T041〈성능 검증〉 프로파일링 / T042 학습 평가 | T039/T040〈Quest 중단 검증〉 후, 별도 장치/진행자 필요 |

US3는 병렬성을 만들기 위해 같은 파일의 작업을 인위적으로 분리하지 않는다. T036은 실제 기기 없이 합성 입력으로 정상 완료 경로를 재현해 한계 안내·결과·복귀를 검증한다.

## Implementation strategy

첫 구현 단위는 Foundation + US1의 관찰 MVP다. 의미 있는 최소 확인 범위는 상부 축적·낮은 자세의 이유·고정 연기층·중단이며, 자세 연습 완료를 주장하지 않는다.

다음으로 US2의 실제 머리 높이 입력·연속 2초·중단 우선순위를 연결하고 US3에서 한계 안내/재진입까지 완결한다. 사용자에게 전체 체험으로 제공할 범위는 US1+US2+US3다. 스토리별 checkpoint는 검증 경계이며 사용자 동의 없는 새 기획 확정이나 배포를 의미하지 않는다.

각 단계에서 실제 수정한 파일·확인한 결과·남은 제한을 기록한다. 유효한 구현 뒤 검증 결과에 따라 체크하며 문서 작성만으로 구현 작업을 완료하지 않는다. 후속 명령 권장은 먼저 `$speckit-analyze`로 명세·계획·작업의 일관성을 확인하는 것이다.

## Requirement coverage

| 요구사항 | 주요 작업 |
|---|---|
| FR-001 | T004~T011, T017, T034, T037 |
| FR-002 | T009, T013, T016, T018, T039 |
| FR-003/005 | T013, T018, T041〈성능 검증〉 |
| FR-004 | T012, T014~T016, T018/T019, T039 |
| FR-006 | T020~T023, T027/T028, T039 |
| FR-007 | T024/T025, T028/T029, T036, T039 |
| FR-008 | T025, T030, T018 |
| FR-009 | T007/T009, T026/T029, T034/T035, T040〈Quest 중단 검증〉 |
| FR-010 | T012/T014, T031/T032, T035/T036 |
| FR-011 | T033/T035, T039/T040〈Quest 중단 검증〉 |
| FR-012 | T012/T016/T025/T032, T043/T045 |
| FR-013 | T014, T027/T033, T040〈Quest 중단 검증〉, T043/T044 |
| SC-001 | T035 |
| SC-002 | T028/T029, T035/T036, T039/T040〈Quest 중단 검증〉 |
| SC-003 | T018/T019, T035, T041〈성능 검증〉 |
| SC-004 | T025, T039 |
| SC-005 | T042 |
| SC-006 | T034/T035, T040〈Quest 중단 검증〉 |

## Task totals

초기 목록의 역사적 집계 — 45 tasks: Setup 3 / Foundation 8 / US1 8 / US2 11 / US3 6 / Polish 9. `[P]` 6개(T002/T003/T008/T013/T022/T042), 파일 충돌 없이 조건부 병렬 가능한 묶음 5개. 현재 전체 집계는 문서 끝의 ID 대응표를 따른다. 새 자동 테스트 프레임워크/테스트 어셈블리는 추가하지 않았다.


## Implementation evidence — 2026-11-10

Foundation: existing shared UI 111/111 and physical bell 25/25. Smoke: Editor domain input probes 84/84, scene integration 63/63 (3 loops, four-stage exit), review/error/UI probes 17/17. Reports: Assets/_02LWS/Documentation/Validation/SmokePosture*.md. Prototype narration exists; T014 remains unchecked until listening and redistribution review. US1 uses the shared session/input types as prerequisites for scene wiring; their implementation was pulled forward to avoid a second temporary controller, while acceptance checks remain separated by story. All results here are Editor-only unless specifically stated.

## F1 음성 통일 — 2026-11-11

사용자 확정: 연기 자세 안내 6개는 콘텐츠 1과 같은 Supertonic 2 F1으로 통일한다. 기존 대본과 교육 흐름을 유지한다. 한국어, 속도 0.95, 16스텝으로 로컬 사전 생성한 WAV를 사용하며 실제 길이에 자막 종료 시각을 맞춘다. 모델·Python은 제출물에 포함하지 않는다. 2026-11-12 사용자 청취 검수 완료: 6개 모두 발음·억양·대본에 문제가 없다고 확인했다.

- [X] T040〈F1 음성 생성〉 안내 6개를 F1으로 생성·교체하고 기존 GUID와 대본을 보존, 자막 시간 및 생성 manifest 갱신.
- [X] T041〈F1 청취 검수〉 교체된 F1 안내 6개의 발음·억양을 사용자 청취 검수하고 결과 기록. T014는 이 검수 전까지 미완료 유지.

## Phase 7: Convergence

2026-11-11 Convergence 당시 기록: 콘텐츠 002의 spec/plan/tasks와 실행 코드·자산·검증 기록을 대조했고 당시 선행 확인 도구는 001을 반환했다. 현재 분석 도구는 002를 반환한다. 현재 헌장 v1.1.0을 적용하며 후속 실행 변경은 위 시각 검증 게이트를 따른다. 아래 작업은 기존 미완료 항목의 구체화이며 중복 실행을 요구하지 않는다. 기존 작업 47개 항목(고유 ID 45개)을 보존한다. Quest/참여자/수신 환경이 없으면 검증 항목을 완료 처리하지 않는다.

- [X] T046 [HIGH] 시스템 중단 복귀에서 현재 안내와 자세 확인을 다시 제공하도록 `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`의 lifecycle 복구 경로를 보완한다. 수정 전 LifecycleChanged는 차단/해제만 처리해 중단 전 안내 위치와 기준을 그대로 재사용했다. 일반 사용자 pause의 위치 보존과 구분하고, 미완료 유지 시간 초기화·이미 인정한 완료 유지·모달/다른 차단 모두 해제 후 복구·닫힌 실행 콜백 무효화를 보존한다. 추적 기준 공간이 유지되면 기존 시작 높이를 유지하고 기준 변경 시 미완료 기준을 폐기해 준비 완료 선택부터 다시 받는다. 준비/원리/연습/완료 안내 단계별 안내·자세 확인 재제공과 유지 시간 초기화·완료 유지·차단 해제 후 복구·종료 콜백 무효화를 Editor 입력으로 검증해 `Assets/_02LWS/Documentation/Validation/SmokePostureInterruption.md`에 기록한다. per spec: Edge Cases(시스템 중단 복귀), FR-009, T026 (contradicts)
- [X] T047 [HIGH] F1 안내 6개의 실제 청취 검수를 받아 발음·억양·대본 일치를 기록하고 필요한 구간만 재생성한다. `Assets/_02LWS/Audio/Voice/SmokePosture/VoiceManifest.json`의 검수 상태·해시·길이와 GuidanceCatalog 자막 시간을 동기화하고 현재 선택된 모델 라이선스·AI 음성 고지·로컬 파일을 점검한다. `Assets/_02LWS/Documentation/Validation/SmokePostureDependencies.md` 등 현재 음성을 Heami로 설명하는 기록은 역사/현재를 구분한다. 콘텐츠 1 음성은 보존하고 청취 전 최종 승인으로 표시하지 않는다. per FR-004/013, plan:F1 음성 통일, T014/T043 및 후미 T041〈F1 청취 검수〉 (partial)
- [ ] T048 [HIGH] T046 및 T051-A 완료 후 실제 Quest 2에서 준비·편한 숙임·재기립·다양한 시작 높이·1초 이내 피드백·2초 한 번 인정·양안/150% 글자/중단 조작·네 단계 시스템 중단/추적 복구/기준 변경·오프라인 실행·콘텐츠 1 회귀를 검증해 `Assets/_02LWS/Documentation/Validation/SmokePostureQuest.md`에 기록한다. 기존 T039 및 원래 Phase 6의 T040〈Quest 중단 검증〉 범위를 포함하며 Editor 합성 입력을 실제 기기 증거로 대체하지 않는다. per SC-002/004/006, FR-001/009/011/013, T039/T040〈Quest 중단 검증〉 (partial)
- [ ] T049 [MEDIUM] T048 완료 후 실제 Quest 프로파일링으로 CPU/GPU 시간·양안 표시·입자 설정을 `Assets/_02LWS/Documentation/Validation/SmokePosturePerformance.md`에 기록하고 필요한 최적화 후 고정 연기층·성공 후 유지 표현을 재검증한다. 72 Hz/13.9 ms를 사용자 확정 기준으로 취급하지 않는다. per plan:Performance Goals, FR-003/005, 원래 Phase 6의 T041〈성능 검증〉 (partial)
- [ ] T050 [MEDIUM] T048의 편안한 사용 경로 확인 후 체험자 5명의 두 교육 이유 설명을 익명으로 평가하여 `Assets/_02LWS/Documentation/Validation/SmokePostureLearning.md`에 기록한다. SC-005는 구현 기능이 아닌 사람 대상 검증이며 참여자 없이 통과를 주장하지 않는다. per T042, SC-005 (partial)
- [ ] T051 [MEDIUM] A/B 두 단계로 수행한다. T051-A는 T046 복구 수정 및 T047 음성 검수·필요한 재생성 반영 후 최신 Android 개발 빌드를 생성·보관하고 빌드/소스 식별·해시를 갱신한다. T051-B는 T048~T050 검증 및 T043 잔여 권리/의존성 점검 후 최신 `Assets/_02LWS`를 Export하고 별도 수신 프로젝트에서 두 콘텐츠 전체 진행·설정·중단·복귀·Console 및 참조를 검증해 `Assets/_02LWS/Documentation/Validation/SmokePostureBuild.md`와 `SmokePostureImport.md`에 기록한다. 예전 Heami 빌드·예비 Import 결과를 최신 F1 결과로 사용하지 않는다. T051은 A/B 모두 끝나야 완료 처리한다. 검증 중 실행 코드·자산을 수정하면 영향을 받는 빌드와 검증을 다시 수행하며 필요한 환경이 없으면 해당 단계는 미완료로 남긴다. per T038/T043/T044, plan:검증과 제출 (partial)
- [ ] T052 [MEDIUM] T046~T051의 결과를 `Assets/_02LWS/Documentation/Validation/SmokePostureAcceptance.md`에 집계하여 현재 F1 출처·Editor/Quest/학습/Import 증거·완료 수·미실시 제한을 대조한다. 이전 37/45 집계와 음성 권리 미확인 문구를 현재 검증 결과에 맞추고 실제 근거가 있는 작업만 완료 처리한다. 후속 실행 변경의 MP4 또는 대체 시각 자료·verification.md·Git 비추적 확인·대표 파일 링크·미검증 환경을 공통 게이트와 대조한다. per T045, plan:검증과 제출 (partial)
- [X] T053 [LOW] 기존 작업의 ID·의무·완료 근거를 보존하고 tasks.md 끝에 ID 대응표를 추가하여 원래 Phase 6의 T040〈Quest 중단 검증〉/T041〈성능 검증〉과 후미 F1 절의 T040〈F1 음성 생성〉/T041〈F1 청취 검수〉를 구분한다. 후미 음성 생성은 완료 근거를 유지하고 청취 의무는 고유 ID T047로 추적하며 전체 항목/고유 ID/완료 집계 기준을 명시한다. per tasks:Format and paths 및 완료 근거 추적, T045 (partial)

## ID 대응표 및 후속 실행 순서 — 2026-11-11 문서 정정

기존 ID와 항목을 보존한다. 중복 ID는 아래 구분명까지 사용해 식별한다. 앞의 번호 범위 T039~T041 및 T039~T042는 Phase 6의 기기 검증 범위다. F1 청취 의무는 고유 ID T047로 추적한다. T053은 이 대응표·참조 구분·집계 작성과 문서 검증으로 완료 처리하며 실제 앱 검증을 뜻하지 않는다.

| 기존 위치/ID | 구분 표기 | 후속 추적 |
|---|---|---|
| Phase 6 T040 | T040〈Quest 중단 검증〉 | T048 |
| Phase 6 T041 | T041〈성능 검증〉 | T049 |
| F1 절 T040 | T040〈F1 음성 생성〉 | 기존 생성 완료 근거 유지 |
| F1 절 T041 | T041〈F1 청취 검수〉 | T047 |

Phase 8 추가 전 역사적 집계(2026-11-12): 체크박스 55개 / 고유 ID 53개 / 체크 43개 / 미체크 12개. 2026-11-11 문서 정정에서는 T053만 완료로 변경해 39/16이었고, 이번에는 T014·F1 절 T041·T046·T047을 근거에 따라 완료 처리했다. 집계 단위는 체크박스이며 구현·실기기 검증 합격률이 아니다. Phase 7은 기존 미완료 의무의 구체화이므로 같은 검증을 중복 수행하지 않되 원래 항목도 같은 증거로 충족되는지 대조한다.

아래 순서는 Phase 7 후속 작업에 적용하며 앞의 Phase 1~6 역사적 그래프를 보완한다. T051-A/B는 T051 내부 단계이며 별도 체크박스나 새 작업 ID가 아니다.

```text
T046·T047 → T051-A(검증용 APK) → T048(Quest)
  → T049(성능)·T050(학습) → T043 잔여 점검
  → T051-B(Export·Import) → T052(최종 집계)
```

T049/T050은 별도 기기·진행자가 있을 때만 병렬 수행하고 그렇지 않으면 순차 수행한다. T051은 A/B 모두 완료해야 체크한다. 검증 중 코드·자산이 바뀌면 영향을 받는 빌드·검증을 다시 수행한다. 기기·참여자·수신 환경 부재는 해당 단계의 미완료로 남긴다.

## Implementation evidence — 2026-11-12

T046: 시스템 중단 복구 42/42 Editor 합성 입력 검사와 기존 도메인 회귀 84/84 통과. 네 단계 안내 재시작·일반 pause 위치 보존·미완료 유지 시간 초기화·완료 유지·모달/추적 차단 뒤 복구·기준 변경 재준비·종료/새 실행 분리를 확인했다. 실행 MP4와 검증 원본: `Artifacts/Validation/VisualEvidence/20261112-000952-smoke-system-recovery/`. 요약은 Assets/_02LWS/Documentation/Validation/SmokePostureInterruption.md. 실제 OS/Quest 복구 합격은 아니다.

T014/T047 및 F1 절 T041: 사용자 6개 음성 청취 승인(2026-11-12), 실제 WAV 해시/meta 6/6 일치, catalog 검증·자막 시간 일치, 현재 모델 출처/라이선스/AI 고지를 확인했다. 재생성 없이 승인 상태만 갱신했다. 기기·학습·최종 Export/Import와 T043 잔여 점검은 완료 처리하지 않는다.

T051-A: 최신 Android 개발 APK 빌드 성공(오류 0, 디버그 심볼 경고 1, 119.306초). `Artifacts/SmokePosture/SmokePosture-development-20261112.apk` 및 `Assets/_02LWS/Artifacts~/SmokePosture/SmokePosture-development-20261112.apk`의 SHA256 일치: 237DA822F24D81ED2C83DCEB89E6004FA2E2B3E16B996F479880BFA72BB61AF4. T051-B는 기기·학습 검증과 최종 의존성 점검 후 수행하므로 T051 전체는 미완료다.

## Phase 8: Shared UI — 사용자 승인 2026-11-12

이번 단계는 공용 UI 재사용 FR-014~018/SC-007~009를 구현한다. 기존 T039~T052의 실제 기기/학습/최종 제출 의무는 보존한다. 실행 순서는 T054→T055→T056→T057→T058→T059→T060이다. T055/T056 후 씬 전환 전에는 중간 연결이 완성됐다고 주장하지 않는다.

- [X] T054 사용자 승인 공용 UI 요구·설계·계약·검증 순서를 specs/002-smoke-low-posture/spec.md, plan.md, contracts/, quickstart.md에 반영하고 현재 구조의 실패 검사를 기록한다.
- [X] T055 Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs에 소유자 기반 화면/버튼 연결과 공용 재생·설정 처리를 구현하고 ContentExperienceAdapter.cs에 공용 view를 연결한다. 버튼 재사용·오래된 콜백·중단 차단을 검증한다.
- [X] T056 FireNotificationAdapter.cs와 SmokePostureController.cs가 공용 view에 내용/동작만 전달하도록 바꾸고 SharedUIController.cs/ContentSceneNavigator.cs의 콘텐츠 내부 UI 탐색과 콘텐츠 1 lifecycle 의존을 제거한다. 기존 교육/복구/효과음을 보존한다.
- [X] T057 Editor API로 Assets/_02LWS/Scenes/FireNotification.unity를 SharedCanvas 1개·공용 안내 재생기 1개·Contents 그룹 구조로 통합한다. 콘텐츠 내부 Canvas와 전용 UI 입력 연결을 제거하고 벨 표지를 3D 텍스트로 보존한다.
- [X] T058 Assets/_02LWS/Documentation/ContentAuthoring.md에 새 콘텐츠 표준 예시와 제작/검사 절차를 작성한다. Scripts/Runtime/SharedUIContractAudit.cs로 Canvas/재생기/전용 UI 중복을 검사하고 실제 임시 위반 검출을 확인한다.
- [X] T059 공용 UI 계층/오래된 버튼/두 콘텐츠 전체 진행/벨/자세/설정/중단 회귀를 Editor에서 검증하고 실행 MP4와 원본을 헌장 지정 경로에 보관한다. Assets/_02LWS/Documentation/Validation/SharedContentUI.md에 결과·원본 참조·기기 한계를 기록한다.
- [X] T060 관련 변경을 리뷰하고 컴파일·최신 Android 개발 APK·아카이브 해시를 확인해 SharedContentUI.md 및 SmokePostureBuild.md에 기록한다. 전체 체크 수와 기존 미완료 의무를 갱신하고 후속 구현 훅을 확인한다.
### Shared UI requirement coverage

| Requirement | Tasks |
|---|---|
| FR-014 | T055/T057/T059 |
| FR-015 | T055/T056/T059 |
| FR-016 | T055/T056/T059 |
| FR-017 | T056/T057/T059 |
| FR-018 | T058/T059 |
| SC-007 | T057/T058/T059 |
| SC-008 | T055/T056/T059 |
| SC-009 | T059/T060 |

최종 공용 UI Editor 결과: 통합47/47·자세 도메인84/84·콘텐츠1 단계 회귀111/111·연기 시스템 복구42/42 PASS. 저장/실행 씬의 Canvas1·안내 재생기1·누락 스크립트0·외부 프로젝트 자산 참조0을 확인했다. 기존 Quest·성능·학습·최종 Import 의무는 유지한다.
### Final shared UI status — 2026-11-12

T054~T060 완료. 현재 체크박스62개 / 고유ID60개 / 완료50개 / 미완료12개. 기존 실기기·학습·성능·최종 패키지/Import 의무는 유지했다. 새 UI 개발 APK `Artifacts/SmokePosture/SharedUI-development-20261112.apk` 및 보관본 `Assets/_02LWS/Artifacts~/SharedUI/SharedUI-development-20261112.apk`: 108592714 bytes, SHA256 E83CD20A3195E0BEE63B15EF05D32E44670396D8B5A227D2D86F8C698860FD92, 빌드 오류0/경고1. 검증 원본 `Artifacts/Validation/VisualEvidence/20261112-011720-shared-content-ui/verification.md`와 실행영상 `Artifacts/Validation/VisualEvidence/20261112-011720-shared-content-ui/execution.mp4`를 참조한다. 외부 확장 구현 전후 훅 파일은 없다.
## Phase 9: 체험 UI 시야 가림 수정 — 사용자 승인 2026-11-12

공용 Canvas/소유자 계약과 기존 교육 순서는 유지한다. design.md 6·8절의 시야 보존 규칙을 적용한다. 실행 순서 T061→T062→T063.

- [X] T061 공용 체험 패널의 고정 높이·중앙 배치를 제거하고 SharedContentUI.cs에서 실제 상태·자막·버튼 높이로 아래쪽 레이아웃을 계산한다. FireNotification.unity의 중복 제목/콘텐츠 전용 고정 문구는 숨긴다. 메뉴/설정/결과는 기존 배치를 유지한다.
- [X] T062 두 콘텐츠 진입·선택·직접 벨·자세 완료, 자막 끄기/빈 자막/150%/모달·숙임을 Play Mode에서 검증한다. 실제 실행 MP4와 시야/잘림 검사 결과를 헌장 경로에 보관한다.
- [X] T063 작성 규칙·명세·계약·검증 문서에 아래쪽 가변 높이 규칙과 검증 한계를 반영하고 컴파일/씬 연결/차이/확장 훅을 확인한다. 이전 APK가 이번 수정 전임을 명시한다.

T061~T063 완료: Editor 통합49/49·배치452/452, 실행1101표본·MP4 디코딩 PASS. 원본: Artifacts/Validation/VisualEvidence/20261112-023201-compact-content-ui/verification.md. 기존 APK는 이 수정 전이며 실기기/최종 제출 의무는 유지한다.
