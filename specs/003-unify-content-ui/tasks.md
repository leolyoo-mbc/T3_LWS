# Tasks: 콘텐츠 공통 UI 통일

**Input**: `specs/003-unify-content-ui/`의 spec.md, plan.md, research.md, data-model.md, contracts/shared-experience-ui.md, quickstart.md
**Governance**: `.specify/memory/constitution.md` v1.1.0, `AGENTS.md`, `design.md` v0.1.1
**Status**: 작업 목록 생성만 완료. 모든 구현·검증 작업은 미실행이다.

**Organization**: US1(P1) 시각 일관성 → US2(P1) 콘텐츠 데이터/동작 연결 → US3(P2) 전환·중단 안정성. 기존 세션 로직과 화재 대사는 유지한다.
**Validation**: 계획에 정의한 Editor 검증 도구와 실제 실행 시나리오를 구현한다. 별도 Test Framework 설치나 새 단위 테스트 모음은 요구하지 않는다. 실행 기능을 수정한 각 완료 단위마다 헌법의 시각 자료를 남긴다.

## Format and Paths

작업 형식은 `- [ ] T번호 [P?] [US번호?] 설명 및 파일 경로`다. [P]는 선행 조건 완료 후 서로 다른 파일에서 병행 가능한 작업이다. 이 표시는 자동 에이전트 위임 지시가 아니다.

모든 경로는 프로젝트 루트 기준이다. 아래 작업의 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-작업명/`는 실제 생성 시각과 작업 식별자로 치환한다. 같은 이름이 있으면 접미사를 추가한다. 영상·스크린샷·동반 검증 기록·기준 스냅샷은 이 로컬 경로에만 보관하며 Git에 강제 추가하지 않는다. 제출용 코드/자원은 `Assets/_02LWS` 내부다.

## Phase 1: Setup

**Purpose**: 기존 작업과 화면/대사 기준을 보호하고 캡처 환경을 준비한다.

- [ ] T001 `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `.gitignore`와 Git 상태를 확인해 Unity 6000.3.26f1/기존 패키지 및 사용자 변경을 보존할 범위를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-baseline/verification.md`에 기록하고 자료 폴더가 `/[Aa]rtifacts/`로 제외되는지 확인한다.
- [ ] T002 `Assets/_02LWS/Scenes/FireNotification.unity`의 기존 화재 안내·선택·다음·메뉴·결과를 기본/150%로 실행 캡처하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-baseline/`에 단계 이름으로 저장하며 문서 규칙별 비교표를 작성한다.
- [ ] T003 `Assets/_02LWS/Content/GuidanceCatalog.asset`, `Assets/_02LWS/Content/ActionOptions.asset`와 화재 음성 참조의 cue ID/text/segments 문구/audio GUID 및 관련 .meta 기준을 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-baseline/fire-guidance-baseline.json`에 보존한다.
- [ ] T004 사용 가능한 MP4 캡처/재생 수단과 로컬 음성 제작 경로를 확인하고 `Assets/_02LWS/Audio/Voice/SmokePosture/VoiceManifest.json`을 조사하여 결과를 baseline의 `verification.md`에 기록한다. 도구·음성·기기 미준비를 구분하고 녹화 불가 시 스크린샷 대안과 한계를 명시한다.

## Phase 2: Foundational

**Purpose**: 두 콘텐츠가 공통으로 사용하는 데이터·자막·입력 계약. 이 단계 완료 전 실제 씬의 UI 연결을 교체하지 않는다.

- [ ] T005 [P] `Assets/_02LWS/Scripts/Presentation/ExperienceUIState.cs`에 Revision(long), StatusText(string), StatusKind(None/Info/Warning/Error), Actions(UIAction[]) 및 Id/Label/Role(Primary/Secondary)/Available 모델을 정의한다. “없으면 빈 배열”, “행동 실행 delegate는 데이터 안에 저장하지 않는다”, “Revision은 매 프레임 증가시키지 않는다”, “행동 목록·대상 단계·가용성이 달라질 때만 증가한다”, “상태 문구만 바뀌면 화면만 갱신한다”를 준수하고 교육 판정·타이머를 복제하지 않는다.
- [ ] T006 [P] `Assets/_02LWS/Scripts/Presentation/ContentUIConfig.cs`에 contentId, 버튼 label/역할, 상태 key/text와 검증을 구현한다. “contentId는 Navigator 등록 ID와 같아야 한다”, “노출할 버튼 label과 상태 text는 비어 있지 않아야 한다”, “한 화면의 행동 ID는 중복되지 않아야 한다”, “요청한 key/행동이 없으면 누락 상태로 처리한다”, “표시 문구에 컨트롤러 메서드명이나 씬 이름을 넣지 않는다”를 지키며 화재 선택지/음성 대사 원본을 복제하지 않는다.
- [ ] T007 `Assets/_02LWS/Scripts/Presentation/IGuidanceSubtitleSink.cs`의 SetText/Clear와 `Assets/_02LWS/Scripts/Presentation/GuidancePresenter.cs`의 sink 연결을 구현한다. voice/Elapsed/Play/Cancel/SetPaused, segment timing과 run 취소 의미를 유지하고, 실제 공유 연결 전 기존 씬이 동작하도록 기존 출력 호환 경로를 보존한다.
- [ ] T008 `Assets/_02LWS/Scripts/Interaction/FreshPressButton.cs`에 ResetForNewContext를 추가해 acceptedPointer/released/armed/busy를 초기화하고 PC 마우스·양쪽 선택 입력이 모두 해제된 뒤 새 선택만 허용한다. 콘텐츠 가용성과 모달 차단을 스타일 갱신이 덮어쓰지 않도록 분리한다.
- [ ] T009 `Assets/_02LWS/Scripts/Runtime/ContentExperienceAdapter.cs`에 UIState/SelectUIAction/BindSubtitleOutput/UnbindSubtitleOutput 계약을 추가한다. 파생 `FireNotificationAdapter.cs`와 `SmokePostureController.cs`가 같은 컴파일 단계에서 호환되게 빈 기본 상태 또는 최소 계약을 제공하고 기존 Begin/Stop/Restart/결과 계약을 유지한다.

**Checkpoint**: 기존 씬 컴파일 및 원래 콘텐츠 동작을 유지하며 공유 뷰를 만들 수 있다.

## Phase 3: US1 — 문서에 맞는 일관된 화면 (P1)

**Goal**: 기존 자원을 활용한 공통 안내·선택 화면과 문서 대조 자료를 제공한다.
**Independent Test**: 실제 콘텐츠 전환에 의존하지 않는 Editor 임시 프리뷰에서 기존 두 콘텐츠의 표시 데이터를 넣어 기본/150% 한국어·0/1/여러 버튼·상태·자막 표시를 확인한다. 임시 프리뷰는 원본 씬에 저장하지 않는다. 두 콘텐츠 실제 연결의 최종 수용 검증은 US2 이후 수행한다.

- [ ] T010 [US1] `Assets/_02LWS/Scripts/Presentation/UITheme.cs`와 기존 `Assets/_02LWS/UI/Prefabs/UI_Button.prefab`의 실제 참조 자원을 조사해 design.md의 폰트·색·모서리·위계·여백·버튼 상태를 적용한다. 기존 자원을 재사용하고 필요한 변경 및 문서 근거를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-us1/design-comparison.md`에 기록한다.
- [ ] T011 [US1] `Assets/_02LWS/Scripts/Presentation/SharedExperienceUIView.cs`에 자막/필수 상태/행동 슬롯, UIState 렌더링, Bind/Unbind와 contentId+generation(long) 소유권을 구현한다. “이전 세대, 이전 화면, 현재 목록에 없는 ID, 비가용 행동은 거부한다”를 적용하고 sink가 발급 generation과 일치할 때만 자막을 쓰거나 지우게 하며 일반 상태 갱신은 재생 자막을 덮지 않게 한다.
- [ ] T012 [US1] `Assets/_02LWS/UI/Prefabs/SharedExperienceUI.prefab`을 기존 화재 UI와 버튼으로 구성한다. 자막·상태·행동 영역을 구분하고 선택지 없으면 접는다. World Space/Tracked Device Graphic Raycaster, 스케일 0.001m/u, 본문·버튼 32u, 여백 32u, 버튼 간격 16u 이상·최소 240×80u, 기존 정면 아래 역할 배치를 적용하고 새 EventSystem/XR Origin은 만들지 않는다.
- [ ] T013 [US1] `Assets/_02LWS/Editor/SharedExperienceUIValidation.cs`에 `LWS.FireNotification.Editor.SharedExperienceUIValidation.RunAll`과 `LWS/Validation/Shared Experience UI` 메뉴를 구현한다. 임시 프리뷰에서 실제 카탈로그/선택지를 읽어 표시 검사를 수행하고 실패는 예외로 보고하며 원본 씬/자산을 재작성하지 않는다.
- [ ] T014 [US1] `Assets/_02LWS/UI/Prefabs/SharedExperienceUI.prefab`과 `Assets/_02LWS/Scripts/Presentation/SharedExperienceUIView.cs`에서 TMP preferred size로 긴 한국어·두 줄 버튼·150% 확대의 높이를 확보한다. 필수 내용 AutoSize/말줄임·중복 대사 표시 없이 빈 영역을 접고, 단순 Overflow만으로 가독성 검증을 통과시키지 않는다.
- [ ] T015 [US1] `specs/003-unify-content-ui/quickstart.md` V1 기준으로 프리뷰를 실행해 baseline과 문서 대비 변경을 검토하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-us1/`에 MP4 또는 불가 사유를 포함한 단계별 스크린샷과 verification.md를 저장한다. 기본/150% 잘림·겹침·누락 0건을 확인한다.

**Checkpoint/MVP**: 공통 화면을 직접 검토할 수 있는 시각 MVP. 아직 두 콘텐츠 전환까지 완료한 것으로 보고하지 않는다. 문서에 없는 새 디자인은 별도 제안으로 다룬다.

## Phase 4: US2 — 콘텐츠별 내용·행동 연결 (P1)

**Goal**: 실제 두 콘텐츠가 화면 한 벌을 사용하며 기존 교육 순서와 화재 대사를 유지한다.
**Independent Test**: 두 콘텐츠를 각각 처음부터 결과/복귀까지 실행한다. 한 콘텐츠의 문구 변경이 다른 콘텐츠에 영향을 주지 않고 공통 버튼 표현 변경은 양쪽에 반영되는지 확인한 뒤 임시 변경을 되돌린다.

- [ ] T016 [P] [US2] `Assets/_02LWS/UI/Data/FireNotificationUI.asset`에 기존 다음/복귀/필요 상태 문구를 옮기고 `Assets/_02LWS/Content/ActionOptions.asset`의 shout/wait ID와 승인 label을 단일 원본으로 연결한다. 화재 카탈로그·음성 문구는 변경하지 않는다.
- [ ] T017 [P] [US2] `Assets/_02LWS/UI/Data/SmokePostureUI.asset`에 ready/return 및 준비·인식·추적·구성 오류 상태 문구를 분리한다. 일반 가이드 자막과 필수 상태를 구분하고 config의 contentId·누락 key·중복 행동 검증을 적용한다.
- [ ] T018 [US2] `Assets/_02LWS/Scripts/Runtime/FireNotificationAdapter.cs`에 UIState/행동 매핑/자막 연결을 구현한다. AwaitAction의 shout/wait만 Send(ChooseAction,id), AwaitNext의 continue만 Send(Continue), return은 기존 복귀 요청으로 연결하고 기존 피드백·Blocked/Result 정책과 선택지 검증을 유지한다.
- [ ] T019 [US2] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`의 status/readyPanel 직접 쓰기를 UIState/config로 교체하고 ready/return을 Ready/RequestReturn에 연결한다. 직접 UI 참조 검증은 새 계약 검증으로 바꾸고 변경 시만 Publish하며 practiceArmed·추적·기준 재확인·2초 자동 인정·한계 안내·음성 완료 보호를 보존한다.
- [ ] T020 [US2] `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`에 이전 구독 해제→Stop→자막/뷰 Unbind→루트 전환→새 뷰/자막 Bind·구독→Begin→현재 UIState 반영 순서를 구현한다. 첫 안내 전에 연결하고 소유권과 기존 메뉴 이벤트 구독을 중복하지 않는다.
- [ ] T021 [US2] `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`에 공유 뷰/CanvasGroup/버튼과 ComfortFollowUI를 명시적으로 등록한다. 공통 자막/글자 설정은 한 번만 적용하고 어댑터의 비활성 콘텐츠 음량 적용이 공통 자막을 바꾸지 못하게 하며 기존 모달·결과와 물리 라벨은 유지한다.
- [ ] T022 [P] [US2] `Assets/_02LWS/Data/SmokePosture/GuidanceCatalog.asset`의 긴 안내를 기존 화재 안내 수준으로 다듬을 원문/수정문·필수 의미 대조를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-us2/smoke-guidance-comparison.md`에 작성한다. 임의 글자 수 상한이나 새 교육 내용을 추가하지 않고 준비 공간·편한 범위·중단 가능성·낮은 자세의 한계·대피 가능 조건을 보존한다.
- [ ] T023 [US2] T022 대조에 따라 `Assets/_02LWS/Data/SmokePosture/GuidanceCatalog.asset`, `Assets/_02LWS/Audio/Voice/SmokePosture/`의 변경 cue 음성과 VoiceManifest.json을 함께 갱신하고 문장/의미별 segment를 설정한다. “segment text 공백 결합과 cue.text 일치” 및 audio 길이 내 유효 timing을 검증한다. 음성 미준비 시 자막만 축약한 상태를 완료로 연결하지 않는다.
- [ ] T024 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`에 콘텐츠 루트 밖 공유 프리팹 한 인스턴스와 두 config를 연결하고 `Assets/_02LWS/Scenes/ContentMenu.unity` 진입을 확인한다. `ActionChoiceInput.cs`, `NextInput.cs`, `ReturnToMenuInput.cs`의 기존 참조와 GuidanceCanvas/SmokeGuidanceCanvas/ready 패널을 새 경로로 이관한 뒤 중복 사용을 제거한다. 필요한 .meta를 보존하고 실제 참조가 남은 프리팹/스크립트는 먼저 삭제하지 않는다.
- [ ] T025 [US2] `Assets/_02LWS/Editor/SharedExperienceUIValidation.cs`에 실제 씬 단일 UI·config·필수 참조·카탈로그/timing 검사와 T003 baseline 원문/오디오 비교를 추가하고 quickstart V1/V2를 실행한다. 공통 스타일 공유·내용 격리·기존 두 체험 전 과정 및 화재 문구 변경 0건을 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-us2/verification.md`와 실행 MP4/대체 자료로 남긴다.

**Checkpoint**: 두 콘텐츠가 한 공통 UI로 각각 실행되며 대사와 행동 차이는 데이터/어댑터로 처리된다. US3의 예외·중단 검증은 별도다.

## Phase 5: US3 — 전환·중단 후 올바른 화면 (P2)

**Goal**: 이전 콘텐츠의 자막·콜백·입력이 새 체험에 전달되지 않으며 모달/오류에서도 안전하게 복귀한다.
**Independent Test**: 메뉴→화재→메뉴→자세→메뉴→화재 및 각 체험의 일시정지/취소/재개/재시작을 실행하고 오래된 자막·선택·뒤쪽 입력 실행 0건을 확인한다.

- [ ] T026 [US3] `Assets/_02LWS/Editor/SharedExperienceUIValidation.cs`에 새 Bind 뒤 이전 sink.SetText/Clear와 옛 generation/revision 선택을 주입하는 검증을 추가한다. 원본 자산을 수정하지 않는 임시 인스턴스에서 현재 화면/진행 불변과 중복 콜백 거부를 검사하고 실패를 보고한다.
- [ ] T027 [US3] `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`의 Begin 예외·부분 연결·메뉴 복귀·OnDisable 정리를 검증하고 누락된 구독/소유권 해제를 보완한다. `SharedExperienceUIView.cs`의 해제 후 버튼/상태/자막 초기화 및 비활성 콘텐츠 Cancel 거부도 확인한다.
- [ ] T028 [US3] `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`의 확인 후 재시작과 결과 재시작을 공통 바인딩 갱신 경로로 연결한다. 이전 Stop/해제 후 새 generation/sink를 발급하고 기존 Restart를 호출하며 단순 표시 갱신으로 음성을 다시 시작하지 않는다.
- [ ] T029 [US3] `Assets/_02LWS/Scripts/Presentation/SharedExperienceUIView.cs`와 `Assets/_02LWS/Scripts/Interaction/FreshPressButton.cs`의 소유자·행동 목록·가용성 변경/모달 재개 때 입력 초기화를 통합한다. 마우스/트리거를 누른 채 전환한 옛 pointer click 거부, 모두 해제 후 새 선택 1회 실행, Available=false의 스타일 재활성화 방지를 검증한다.
- [ ] T030 [US3] `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`의 BlockWorld와 공유 CanvasGroup/버튼 차단을 검증·보완한다. 모달 취소/계속하기에서 기존 음성·자막·환경·진행을 유지하고 뒤쪽 공유 버튼·비상벨·pending 텔레포트 실행을 차단한다.
- [ ] T031 [US3] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs` 및 `FireNotificationAdapter.cs`의 연결 누락·추적/구성 오류·자막 끄기 분기를 실제 임시 오류 주입으로 확인한다. 필수 상태와 복귀를 유지하고 오류를 완료 처리하지 않으며 비활성 어댑터 설정 적용의 자막 간섭을 제거한다.
- [ ] T032 [US3] `Assets/_02LWS/Scenes/FireNotification.unity`에서 quickstart V3/V4의 왕복 전환·재진입·중단 겹침·오래 누름·취소·재개·재시작을 실행하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-us3/verification.md`에 조건별 기대/실제와 중복·잔류·뒤쪽 실행 0건 여부를 기록한다.
- [ ] T033 [US3] `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-us3/execution.mp4`에 전환/모달/재시작의 실행 화면을 기록하고 재생 확인한다. 녹화 불가 시 해당 폴더에 단계별 PNG와 이유·한계를 남기며 로그만으로 대체하지 않는다. 잘못된 진행이나 자료 미확보는 성공으로 기록하지 않는다.

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 전체 명세·헌법 준수와 실제 기기 검증 및 결과물 전달.

- [ ] T034 `Assets/_02LWS/Editor/SharedExperienceUIValidation.cs`의 RunAll과 quickstart V1~V4를 최종 실행하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-final/verification.md`에 검사 출력·실패/재검증 결과·FR-001~014와 SC-001~008 대응을 정리한다. 컴파일 오류·원문 변경·중복 UI 참조가 없음을 확인한다.
- [ ] T035 `Assets/_02LWS/Scenes/FireNotification.unity`의 안내·선택·메뉴·결과 전체를 기본/150%로 확인하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-final/design-comparison.md`에 문서 적합성·한국어 누락/잘림/겹침·인물/경로 가림·중복 대사 여부를 기록한다. 필요한 수정 후 영향을 받은 검증만 재실행한다.
- [ ] T036 Quest 2에서 quickstart V5의 레이+트리거·고개 회전/텔레포트 후 읽기·서기/숙이기 중 메뉴·두 체험 완료/복귀 및 UI 할당/레이아웃 부하를 검증하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-final/verification.md`에 PC와 분리해 기록한다. 기기/빌드 환경이 없으면 미검증으로 남기고 해당 작업을 성공 체크하지 않는다.
- [ ] T037 `Assets/_02LWS/Scenes/FireNotification.unity`와 `ContentMenu.unity` 및 `Assets/_02LWS/Scripts/LWS.FireNotification.Runtime.asmdef` 참조를 검사해 XR Origin/AudioListener/EventSystem/XRUIInputModule 단일 소유·제출 경계·필수 .meta 보존을 확인한다. 결과는 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-final/verification.md`에 남기며 내보내기/제출은 실행하지 않는다.
- [ ] T038 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-final/`에 최종 변경본의 진입·안내/선택·행동·결과/복귀 MP4를 저장해 직접 재생한다. 별도 완료 단위의 영상도 실제 반영 버전인지 확인하고 녹화 불가 시 단계별 스크린샷·사유·한계를 남긴다. 문서만 수정한 단위에는 새 영상을 요구하지 않는다.
- [ ] T039 `Artifacts/Validation/VisualEvidence/`의 실제 결과물에 git check-ignore를 실행하고 git ls-files 결과가 비어 있는지 검사한다. 기존 `.gitignore`의 Artifacts 제외를 유지하고 영상·스크린샷·동반 기록을 Assets/제출물/추적 목록에 섞지 않으며 강제 추가하지 않는다.
- [ ] T040 `specs/003-unify-content-ui/tasks.md`의 체크 상태를 실제 결과에 맞춰 갱신하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-final/verification.md`에 최종 성공/실패/미검증을 정리한다. 완료 응답에 대표 MP4 또는 PNG의 실제 파일 경로와 확인한 환경·남은 한계를 보고하며 폴더 링크만 제시하거나 미확보 자료를 존재한다고 보고하지 않는다.

## Dependencies & Execution Order

```text
Setup T001~T004
  → Foundation T005~T009
  → US1 T010~T015 (시각 MVP)
  → US2 T016~T025 (실제 콘텐츠 공통화)
  → US3 T026~T033 (전환/중단 회귀)
  → Final T034~T040
```

- T005/T006은 병행 가능하며 T007/T008은 서로 파일이 겹치지 않아 순차 진행 중 병행할 수 있다. T009는 T005/T007 후 진행한다.
- US1: T010/T011 후 T012, T012/T013 후 T014/T015. Editor 검증과 뷰가 같은 계약을 사용하므로 계약 확정 전에 병행하지 않는다.
- US2: T016→T018, T017→T019. 두 분기는 서로 다른 자산/어댑터를 편집하므로 병행 가능하다. T020/T021은 두 어댑터 계약 후, T022→T023은 대사/오디오 분기로 병행 가능하다. T024는 T018~T023 완료 후, T025는 T024 후다.
- US3는 두 콘텐츠 실제 연결을 선행 조건으로 한다. T026 검증 경로 준비 후 T027~T031 통합을 순차 검증하고 T032/T033으로 결과를 남긴다. Navigator/SharedUIController를 여러 작업에서 동시에 수정하지 않는다.
- T034~T040은 검증 결과와 같은 폴더 기록을 공유하므로 병행 표시하지 않는다. T038/T039 이전에 T040 전체 완료를 선언하지 않는다.
- US1 단독 프리뷰는 시각 결과를 독립 검토할 수 있고, US2/US3는 각각의 독립 검증 경로가 있으나 공통 뷰 및 실제 연결을 선행 구현으로 요구한다. 기능 간 의존성을 숨기기 위해 가짜 병렬화를 도입하지 않는다.

## Parallel Execution Examples

- **US1**: 뷰 계약 확정 후 T013의 Editor 프리뷰 검증 도구와 T014의 프리팹/레이아웃 작업은 다른 파일 영역에서 가능하다. 뷰 파일까지 수정할 때는 순차 진행한다. 통합 캡처 T015는 양쪽 완료 후다.
- **US2**: T016/T018 화재 데이터·어댑터와 T017/T019 자세 데이터·어댑터를 병행할 수 있다. T022/T023의 대사·음성 정리도 별도 분기다. 씬 저장 T024는 단일 담당 순차 작업이다.
- **US3**: T026 작성 후 T027의 Navigator 정리와 T029의 뷰/입력 전환 검증을 병행할 수 있다. T030은 SharedUIController를 공유하는 T028 이후다. 실제 씬 실행/증거 기록 T032/T033은 한 번에 한 실행으로 진행한다.

## Requirements Coverage

| Requirement / outcome | Tasks |
|---|---|
| FR-001/002, SC-001: 공통 화면·콘텐츠 데이터/행동 분리 | T005~T012, T016~T025 |
| FR-003/004, SC-002: 문서 준수·변경 근거 | T002, T010~T015, T035 |
| FR-005/013, SC-003/007: 교육 흐름·화재 원문·간결한 연기 안내 | T003, T018/019, T022/023, T025, T034 |
| FR-006, SC-005: 이전 소유권·단계 선택 차단 | T008, T011, T020, T026~T029, T032 |
| FR-007/008/011: 모달·음성·설정·오류 복귀 | T007, T019~T021, T027~T032 |
| FR-009/010, SC-004/006: 한국어 확대·추종·기기 조작 | T012~T015, T021, T035/036 |
| FR-012/014, SC-008 및 헌법: 자료·비추적·대표 파일 보고 | T001~T004, T015, T025, T032/033, T034~T040 |

## Implementation Strategy

1. 기존 화면/원문과 사용자 변경을 보호한 후 공통 계약과 뷰를 만든다. US1 프리뷰가 최소 검토 가능한 MVP다.
2. US2에서 화재를 먼저 연결하고 자세를 연결한다. 화재 대사를 기준으로 보존하고 다른 콘텐츠의 긴 안내만 다듬는다.
3. US3에서 전환·오류·중단을 통합 검증한다. 새 교육 단계나 UI 체계는 추가하지 않는다.
4. 각 실행 기능 변경 완료 단위에서 MP4 또는 대체 자료를 남기고 로컬 경로를 보고한다. 문서에 없는 새 디자인만 사용자 확인 대상으로 제시하며 미확정 단계적 승인 절차를 의무로 만들지 않는다.
5. 실제 실패·기기 미검증·음성 미준비·증거 미확보는 미완료 상태를 유지한다. 이 작업 목록 생성은 구현 실행·커밋·배포 요청이 아니다.
