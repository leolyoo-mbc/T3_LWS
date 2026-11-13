# Tasks: 안전한 대피 경로 판단

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md), [화면·통합 계약](contracts/route-experience.md), [검증 가이드](quickstart.md).

**Prerequisites**: 위 설계 문서를 읽고 사용자 확정 요구와 계획 초기값을 구분한다. Unity 6000.3.26f1 및 기존 패키지·단일 씬·공용 UI를 사용한다. 실제 Git 브랜치는 main이며 이 목록이 브랜치 생성·커밋·푸시를 지시하지 않는다.

**Tests**: 명세의 인수 시나리오와 계획에 명시된 도메인/통합 assertion runner 및 실제 실행 검증을 포함한다. 신규 테스트 패키지·임의의 추가 테스트 체계는 도입하지 않는다. 검증 구현은 UNITY_EDITOR로 Player에서 제외한다.

**Format**: `- [ ] T번호 [P 선택] [US번호 해당 시] 작업과 정확한 경로`. [P]는 명시한 선행 조건 완료 후 다른 파일 작업과 병행 가능하다는 뜻이다. 같은 controller·scene·catalog·검증 파일은 동시에 수정하지 않는다.

**Path Conventions**: 문서 안의 경로는 프로젝트 루트 기준이다. Unity 자산 생성 시 .meta를 함께 보존하고 기존 사용자 변경을 덮어쓰지 않는다. 새 Canvas·안내 재생기·XR rig·EventSystem·자유 바닥 이동 표면을 추가하지 않는다. 녹화/로그는 Assets에 넣지 않는다.

## Phase 1: Setup

**Purpose**: 기존 프로젝트와 사용자 변경을 보존하고 작업 경계를 준비한다.

- [ ] T001 `Assets/_02LWS/Scenes/FireNotification.unity`, `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`의 실제 상태와 열린 씬 dirty 여부·Git 변경을 읽기 확인하고 기존 두 콘텐츠의 기준 상태를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-safe-evacuation-route-baseline/verification.md`에 기록한다. 기존 `Assets/_02LWS/UI/Fonts/DisasterKorean.asset` 변경을 사용자 작업과 구분하며 자동 되돌림/일괄 저장하지 않는다.
- [ ] T002 `Assets/_02LWS/Scripts/Domain/EvacuationRoute/`, `Assets/_02LWS/Scripts/Validation/`, `Assets/_02LWS/Data/EvacuationRoute/`, `Assets/_02LWS/Audio/Voice/EvacuationRoute/`의 필요한 폴더와 .meta를 준비하고 기존 domain/runtime 어셈블리 경계를 사용한다. `Assets/_02LWS/Scripts/Domain/LWS.FireNotification.Domain.asmdef`의 noEngineReferences를 유지하며 새 패키지는 설치하지 않는다.

## Phase 2: Foundational

**Purpose**: 모든 이야기에서 사용할 모델 계약·고정 이동·공용 화면 연결을 준비한다. 이 단계 완료 전 사용자 시나리오 통합을 시작하지 않는다.

- [ ] T003 `Assets/_02LWS/Scripts/Domain/EvacuationRoute/RouteTypes.cs`에 data-model의 Scenario/Target/Waypoint/Decision/Result와 stage/choice/blocker 타입을 정의한다. 제약은 “Stair는 Available/SmokeBlocked, Elevator는 Unavailable”, “최초 선택 보존, 후속 기록 추가”, “Available Stair Use만 완료. 미방문은 미수행”, “새 실행은 새 RunId, 단계/대상 변경 시 Execution 증가”를 그대로 적용한다. ID는 “상황을 포함한 ordinal 고유 문자열”이며 모델에 Transform·AudioClip·영구 저장을 넣지 않는다.
- [ ] T004 [P] `Assets/_02LWS/Scripts/Runtime/ContentExperienceAdapter.cs`에 `public virtual bool LocomotionAllowed => !Blocked`를 추가하고 `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`의 기존 MovementAllowed에 AND한다. 기존 두 어댑터는 기본값을 사용하며 loading/lifecycle/콘텐츠 이동 허용 조건을 제거하지 않는다.
- [ ] T005 [P] `Assets/_02LWS/Scripts/Interaction/RouteWaypoint.cs`와 `Assets/_02LWS/Prefabs/Environment/RouteTeleportAnchor.prefab`에 ID·상황·선택 대상(optional)·인접 ID·고정 목적지 Transform을 연결한다. 연결은 “동일 상황 안의 양방향 연결. 시작부터 모든 확인 지점 왕복 가능”을 적용한다. XRI TeleportationAnchor/기존 MenuTeleportationProvider와 중립 표시를 사용하고 기존 TeleportSurface의 자유 이동 설정을 복제하지 않는다.
- [ ] T006 [P] `Assets/_02LWS/Scripts/Presentation/RouteGuidanceCatalog.cs`에 Get/Validate와 로컬 GuidanceCue 검증을 구현하고, `Assets/_02LWS/Documentation/RouteNarration.md`에 준비·확인 방법·B 도입·두 오답 피드백·A 완료·최종 완료 대본을 작성한다. 방향/정답 사전 지시, 연기 없는 외관의 안전 보장, 낮은 자세로 막힌 계단 통과, 실제 탈출 완료 표현을 금지하며 명세의 공식 근거와 대조한다. 작성한 대본은 청취 승인 완료로 표시하지 않는다.

**Checkpoint**: 모델 계약, 일반 차단과 이동만 차단하는 게이트, 고정 앵커, 공용 안내 연결 기반이 준비된다.

## Phase 3: US1 — 안내를 받으며 대피 경로 선택하기 (P1, MVP)

**Goal**: 첫 학교 구역에서 확인 방법·조작법만 안내받고 계단/엘리베이터를 직접 판단한다.

**Independent Test**: A만 실행해 계단 선택 완료, 엘리베이터 이용 차단·이유 안내·재선택, 안전한 계단에서 탐색 후 재방문을 확인한다. 방향/정답 사전 안내와 위험 이동은 0건이다.

- [ ] T007 [US1] `Assets/_02LWS/Scripts/Validation/RouteDomainChecks.cs`에 A 시작·준비/안내·실제 도착 후 선택·계단 완료·엘리베이터 피드백·다른 경로·재방문·중복/낡은 입력 거부 assertion과 Run 메뉴를 먼저 작성한다. 아직 구현하지 않은 동작은 실제 실패를 확인하고 임시 성공 반환으로 우회하지 않는다.
- [ ] T008 [US1] `Assets/_02LWS/Scripts/Domain/EvacuationRoute/RouteSession.cs`에 계약의 Begin/Ready/Arrive/LeaveCheckpoint/Choose/GuidanceDone/SetBlockers/Close와 A 상태 전이를 구현한다. “입력·비동기 완료는 현재 RunId/Execution/TargetId와 일치해야 한다”를 적용하고 다른 경로 선택 후 영역 이탈 전 재선택을 억제한다. T007을 실행해 전부 통과시킨다.
- [ ] T009 [P] [US1] `Assets/_02LWS/Prefabs/Environment/SchoolRouteA.prefab`에 계획 초기값인 폭 3 m·길이 24 m 직선 학교 복도·중앙 출발·양 끝 이용 가능한 계단·중앙 부근 엘리베이터·교실 문·유도등을 만든다. 기존 CorridorFinish/로컬 재질을 참조하되 원본은 수정하지 않는다. 확인 위치는 입구 복도 쪽 최소 2 m를 시작값으로 두고 상태가 보이게 배치한다. `Assets/_02LWS/Scripts/Presentation/RouteScenarioAsset.cs`에 순수 RouteScenario로 변환하는 ScriptableObject 래퍼를 추가하고 `Assets/_02LWS/Data/EvacuationRoute/ScenarioA.asset`에 ID·상태·연결을 저장한다. 씬 Transform은 RouteWaypoint에 남겨 프리팹/데이터 에셋의 씬 객체 참조를 피한다. 수치는 실제 안전 기준이 아니다.
- [ ] T010 [P] [US1] `Assets/_02LWS/Audio/Voice/EvacuationRoute/`에 A에서 필요한 준비·확인 방법·엘리베이터 피드백·A 완료 WAV를 기존 Supertonic 2 F1/ko/속도 0.95/16 steps 방식으로 생성하고 `VoiceManifest.json`에 모델 revision·대본·길이·hash·청취 미검수 상태를 기록한다. `Assets/_02LWS/Data/EvacuationRoute/RouteGuidance.asset`의 자막 구간을 실제 길이에 맞추며 Python/모델은 자산 폴더에 넣지 않는다.
- [ ] T011 [US1] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`에 A용 adapter·공용 Bind/SetStatus/SetActions·준비/안내/피드백/상황 완료를 연결한다. 계단/엘리베이터 선택 문구는 명세 그대로 최대 2개만 제공한다. 실제 locomotionEnded와 추적 유효한 바닥 투영 위치로 확인 지점 도착을 검증하고, 기본 반경 0.65 m·지점 간 최소 2 m·인접 앵커만 활성화한다. 클릭만으로 도착을 인정하지 않고 대상 변경/이탈 시 이전 선택을 폐기한다.
- [ ] T012 [US1] `Assets/_02LWS/Scenes/FireNotification.unity`에 RouteSpace/A와 `safe-evacuation-route` ContentEntry(root·A 시작 anchor·adapter·locomotionAllowed=true)를 등록하고 기존 테마의 메뉴 버튼을 추가한다. 공용 view/재생기/추적/lifecycle를 연결하고 새 Canvas는 만들지 않는다. MVP는 A 완료에서 멈춰 메뉴로 복귀 가능하게 하며, B가 없는 상태에서 동작하지 않는 다음 버튼을 노출하지 않는다. B 연결은 T020에서 수행한다.
- [ ] T013 [US1] `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`에 ValidateConfiguration 메뉴와 A용 구성/Play 검사 기반을 구현한다. ID 중복·없는 연결·누락 UI/root/anchor/cue·확인 영역 겹침은 구성 오류로 안내하고 메뉴 복귀를 제공한다. B의 필수 검사는 두 상황 통합 단계 T021에서 추가한다. 기존 SharedUIContractAudit를 함께 실행해 공용 구성 위반 0개를 확인한다.
- [ ] T014 [US1] `specs/005-safe-evacuation-route/quickstart.md`의 A·엘리베이터·재방문 사례를 실제 실행하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-safe-evacuation-route-us1/verification.md`와 실행 MP4/대체 자료에 결과를 남긴다. 명세 US1의 5개 인수 시나리오, 실제 도착 전 선택 거부·정답 미노출을 확인한다. 아래 공통 증거 기준을 적용하고 음원 미승인은 별도 남긴다.

**Checkpoint**: A는 독립적으로 체험·재시도·메뉴 복귀가 가능하다. 이는 최종 두 상황 콘텐츠 완료가 아니다.

## Phase 4: US2 — 다른 구역에서 경로 상태를 보고 선택하기 (P1)

**Goal**: B에서 막힌 계단을 판단해 우회하거나, 이용 가능한 계단을 바로 찾아 완료한다.

**Independent Test**: B만 검증 진입해 직접 완료·우회 완료·잘못된 선택 후 완료를 각각 확인하고, 최종 A→B 진행도 확인한다. 미방문 감점·우회 허위 기록·연기 속 이동은 0건이다.

- [ ] T015 [US2] `Assets/_02LWS/Scripts/Validation/RouteDomainChecks.cs`에 B의 직접 완료·막힌 계단 오답/재선택·다른 경로 후 미완료·실제 우회·미방문·도중 엘리베이터 방문·오답 보존·A→B 결과 보존 assertion을 추가한다. 실제 구현 전 해당 실패를 확인한다.
- [ ] T016 [US2] `Assets/_02LWS/Scripts/Domain/EvacuationRoute/RouteSession.cs`에 NextScenario/Transition과 B 판단·우회 기록을 구현한다. “고정 A/B. A 계단 2개 Available, B Available/SmokeBlocked 각 1개, 각 엘리베이터 1개”를 검증하고, “막힌 계단 FindAnotherRoute → 영역 이탈 → 다른 Available Stair 도착 → Use 완료의 순서만 DetourPerformed=true”를 적용한다. 직접 완료는 Completed=true/DetourPerformed=false이며 필수 방문·우회 감점을 추가하지 않는다. T015를 통과시킨다.
- [ ] T017 [P] [US2] `Assets/_02LWS/Scripts/Presentation/RouteSmokePresenter.cs`에 Play/SetPaused/Stop을 구현해 기존 연기 텍스처 기반 smoke-only 효과를 시작·일시정지·재개·완전 정리한다. 자세 판정·fireLight/fireSystems를 필수 의존성으로 가져오지 않는다.
- [ ] T018 [US2] `Assets/_02LWS/Prefabs/Environment/SchoolRouteB.prefab`와 `Assets/_02LWS/Data/EvacuationRoute/ScenarioB.asset`에 폭 3 m·팔 약 12 m의 꺾인 학교 복도·다른 출발/계단 방향/엘리베이터 배치를 구성한다. 가까운 끝 계단은 연기로 막고 코너 너머 다른 계단까지는 연기 없이 접근 가능하게 한다. 확인 영역·앵커가 연기와 겹치면 위치를 물리고 연기 내부 목적지는 두지 않는다. 양쪽 확인 지점은 같은 중립 표시를 쓴다.
- [ ] T019 [P] [US2] `Assets/_02LWS/Audio/Voice/EvacuationRoute/`에 B 도입·막힌 계단 피드백·최종 완료 WAV를 생성하고 `VoiceManifest.json`, `Assets/_02LWS/Data/EvacuationRoute/RouteGuidance.asset`에 실제 길이·자막·hash를 연결한다. B 안내에서 정답 방향을 알려주지 않고 두 경로 모두 막힘은 별도 교육 범위로 구분한다. 기존 음원 승인을 신규 cue 승인으로 취급하지 않는다.
- [ ] T020 [US2] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`와 `Assets/_02LWS/Scenes/FireNotification.unity`에 A 완료 안내 후 다음 연습→예약 취소→A 비활성화→B 시작 위치 이동/높이 보존→UI Recenter→B 도입을 연결한다. Edit에서는 두 구역을 보이고 Play에서는 현재 구역만 활성화한다. 다른 경로 선택은 정답 자동 이동 없이 복도 탐색을 재개하며, 위험 선택은 진입을 막고 음성/자막 피드백 후 재선택한다.
- [ ] T021 [US2] `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`에 B 구성 검사·RunPlayMode 메뉴·명세 US2의 7개 인수 시나리오를 추가하고 실제 실행한다. 최소 하나의 이용 가능한 계단, 연기 없는 우회 연결, 세 배치 항목 차이, 정답 미노출, 직접/우회/오답 후 완료를 확인해 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-safe-evacuation-route-us2/verification.md`와 MP4/대체 자료로 남긴다.

**Checkpoint**: 두 상황을 순서대로 실행할 수 있으며 B 직접 완료와 우회 완료가 모두 성립한다.

## Phase 5: US3 — 실제 수행 결과 확인 및 체험 제어 (P2)

**Goal**: 실제 완료·우회·첫 판단을 보여주고 설정·중단·재시작·기존 콘텐츠 교체를 보존한다.

**Independent Test**: 직접/우회/오답 후 완료 결과를 각각 대조하고 중단·설정 변경·재시작·세 콘텐츠 왕복을 확인한다. 하지 않은 행동 표시·낡은 선택 실행은 0건이다.

- [ ] T022 [US3] `Assets/_02LWS/Scripts/Validation/RouteDomainChecks.cs`에 상황별 결과 분리·최초 선택 보존·미방문 미수행·중단 중 진행 거부·재시작 초기화 assertion을 추가한다. 제약 “Blockers는 UserPause/Lifecycle/Tracking/Configuration이다. 하나라도 있으면 진행·선택·이동을 거부한다”와 “중단은 기록 보존, 재시작/종료는 초기화한다”를 고정하며 세션 누락 동작은 `Assets/_02LWS/Scripts/Domain/EvacuationRoute/RouteSession.cs`에서 보완해 통과시킨다.
- [ ] T023 [US3] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`의 ResultVisible/ResultText를 공용 최종 결과에 연결한다. A는 결과 모달을 띄우지 않고 B 완료 안내 종료 후 두 상황의 ‘대피 경로 선택 완료’, 실제 우회만 ‘우회 수행’, 첫 선택/피드백 후 완료를 표시한다. 우회 미수행 감점·미방문 정답·실제 탈출 완료 표현을 금지하고 기존 다시하기/메뉴 버튼을 재사용한다.
- [ ] T024 [US3] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`의 pause/lifecycle/추적/Stop/Restart/메뉴 복귀 수명을 완성한다. 일반 pause는 음성 위치 보존, 시스템 복구는 현재 안내 재제공이며 잠금 때 MenuTeleportationProvider.CancelPending을 호출한다. 정상 teleport revision은 오류로 취급하지 않는다. 구독·재생 콜백·대상·root·연기·이전 실행을 정리하고 공유 시스템 자체를 비활성화하지 않는다.
- [ ] T025 [P] [US3] `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`에 기본/150%·자막 끄기·음량 0·대상 변경·누른 채 전환·빠른 중복 선택·이전 음성 콜백·예약 직후 중단/복구·재시작 검사와 결과 비교를 추가한다. 검증 환경의 추적 대체는 Editor 전용이며 Player에서 실제 추적 상실을 우회하지 않게 한다.
- [ ] T026 [US3] `specs/005-safe-evacuation-route/quickstart.md`의 중단/설정 및 메뉴→화재 전체→자세 전체→경로 전체→화재 왕복을 실제 실행하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-safe-evacuation-route-us3/verification.md`에 명세 US3의 6개 인수 시나리오와 SC-005/006 결과를 기록한다. 기존 비상벨 이동/조작·자세 연속 2초/이동 금지와 새 이동 게이트 회귀를 확인해 MP4/대체 자료로 남긴다.

**Checkpoint**: 세 사용자 시나리오가 실제 결과·설정·중단·복귀까지 연결된다. 기기/음원/제출 검증 상태는 아래 작업과 별개다.

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 신규 자산 권리·청취·성능·빌드·실기기·제출과 실행 증거를 검증하고 한계를 보고한다.

- [ ] T027 `Assets/_02LWS/Documentation/ContentAuthoring.md`, `Assets/_02LWS/Documentation/Integration.md`, `Assets/_02LWS/Documentation/ThirdPartyNotices.md`에 새 콘텐츠 등록·고정 앵커/이동 게이트·새 음성/재사용 자산 출처와 AI 음성 고지를 반영한다. `Assets/_02LWS/Documentation/RouteNarration.md`의 대본을 SC-007 및 공식 근거와 전수 대조하고 새 권리/교육 의미 누락을 수정한다. AGENTS.md는 수정하지 않는다.
- [ ] T028 `Assets/_02LWS/Audio/Voice/EvacuationRoute/VoiceManifest.json`의 모든 새 cue를 실제 재생해 발음·억양·대본/자막·길이를 대조하고 사용자 청취 검수 결과를 `Assets/_02LWS/Documentation/Validation/RouteVoiceReview.md`에 기록한다. 검수 요청 시 재생 가능한 실제 파일을 제공한다. 사용자 검수를 확보하지 못했다면 승인 대기로 남기고 이 작업을 체크하지 않는다.
- [ ] T029 `Assets/_02LWS/Scenes/FireNotification.unity` 및 route 프리팹/데이터/음원의 참조를 검사하고 `Assets/_02LWS/Documentation/Validation/RouteAssetAudit.md`에 비-UPM 필수 참조의 _02LWS 내부 포함·누락 스크립트/분홍 재질 0개·GUID 보존·공용 Canvas/안내 재생기/UI 추적기 각 1개·콘텐츠 내부 UI 0개 결과를 남긴다. 생성 도구·모델·Python·검증 원본을 납품 자산에 섞지 않는다.
- [ ] T030 `specs/005-safe-evacuation-route/quickstart.md`의 메뉴/기존 두 콘텐츠/A/B 성능을 동일 머신/품질에서 워밍업 10초 후 30초씩 3회 수집하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-safe-evacuation-route-performance/verification.md`에 CPU/GPU median/p95·입자/batch·녹화 여부·선택 시 시각 반응 1초 계획 목표를 기록한다. 미지원 값은 미측정, 72 Hz/13.9 ms는 참고값으로 표시하며 임의 합격 기준으로 쓰지 않는다.
- [ ] T031 기존 Android 설정으로 개발 빌드를 생성해 `Artifacts/EvacuationRoute/EvacuationRoute-development.apk`와 빌드 로그를 보관하고 `Assets/_02LWS/Documentation/Validation/RouteAndroidBuild.md`에 실제 성공/실패·씬·설정·누락 의존성을 기록한다. APK 성공을 Quest 실행 합격으로 처리하지 않는다.
- [ ] T032 실제 Quest 2에서 앵커 조작·돌아서기·연기 관찰·우회·150% 읽기·중단 복구·성능/편안함을 확인하고 `Assets/_02LWS/Documentation/Validation/RouteQuestAcceptance.md`에 입력/기기/refresh rate·실제 결과·한계를 기록한다. 기기가 없거나 실행하지 못하면 미검증으로 남기고 작업을 체크하지 않는다.
- [ ] T033 `Assets/_02LWS/Documentation/Validation/ExportImport.md`의 기존 절차로 새 _02LWS 패키지를 `Artifacts/EvacuationRoute/`에 내보내고 별도 수신 테스트 프로젝트에서 참조·컴파일·공용 UI·세 콘텐츠 Play를 검증해 `Assets/_02LWS/Documentation/Validation/RouteExportImport.md`에 기록한다. 필수 외부 패키지/프로젝트 설정은 문서로 전달하고 대상 프로젝트 설정을 일괄 덮어쓰지 않는다. 수신 환경이 없으면 미검증으로 남기고 체크하지 않는다.
- [ ] T034 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-safe-evacuation-route-final/execution.mp4`와 `verification.md`에 정상·오답·직접 완료·실제 우회·결과·회귀를 확인할 대표 실행 증거를 보관한다. 아래 공통 증거 기준으로 원본 해상도/fps·화면/소리 실제 재생·입력 분리·Git 비추적을 확인하고, 대체 사용 시 사유와 미검증 범위를 함께 남긴다.
- [ ] T035 `specs/005-safe-evacuation-route/quickstart.md`와 `Assets/_02LWS/Documentation/Validation/RouteAcceptance.md`에 FR-001–016/SC-001–007별 실제 검증 결과·증거 직접 파일 링크·Editor/Android/Quest/import/청취 승인 상태·남은 작업을 집계하고 `git diff --check`를 실행한다. 다른 미완료 작업을 체크하거나 전체 구현/제출 합격으로 표시하지 않는다.

## Dependencies & Execution Order

```text
T001 → T002 → T003 → (T004 ∥ T005 ∥ T006)
                     ↓
                US1 T007–T014
                     ↓
                US2 T015–T021
                     ↓
                US3 T022–T026
                     ↓
             최종 검증 T027–T035
```

US1은 기반 완료 뒤 독립적으로 시연 가능한 MVP다. US2는 US1의 세션/어댑터/앵커를 확장하므로 구현은 US1 뒤에 진행하되 검증 진입점은 B만 시작할 수 있게 만든다. US3은 두 상황의 실제 결과를 사용하므로 US2 뒤에 통합한다. 이야기 전체를 동시에 개발하지 않는다.

T011은 T008/T009/T010/T004/T005에, T012/T013은 T011에, T014는 T012/T013에 의존한다. T018은 T017/T005에, T020은 T016/T018/T019에, T021은 T020에 의존한다. T025는 US2 완료 후 검사 작성만 병행 가능하며 실행/판정은 T023/T024 이후 T026에서 한다. T030/T031은 안정된 콘텐츠와 T026/T029 이후, T032는 T031 이후, T033은 T029 이후에 진행한다. 같은 Editor를 사용하는 빌드/Play/import는 직렬 실행한다. T034는 대표 실행 상태가 확정된 뒤 수행하며, T035는 나머지 결과 또는 명시적 미검증 상태를 모아 집계한다.

### Parallel Examples

| 범위 | 병행 가능한 작업 | 합류 조건 |
|---|---|---|
| 기반 | T004 공용 게이트 ∥ T005 앵커 ∥ T006 catalog/대본 | T003 완료; 각기 다른 파일 |
| US1 | T007→T008 도메인 ∥ T009 A 공간 ∥ T010 A 음원 | T006 대본 완료 후 음원; T011 전에 모두 합류 |
| US2 | T015→T016 도메인 ∥ T017 연기 ∥ T019 B 음원 | US1 완료; T018은 연기 준비 후, T020 전에 합류 |
| US3 | T022→T023/T024 결과·수명 ∥ T025 통합 검사 작성 | US2 완료; T026에서 직렬 실행 검증 |

병행 표시는 작업 충돌 분석이며 실제 서브에이전트 실행 지시가 아니다. .unity/프리팹 등 Editor 자산 변경은 단일 작성자가 직렬로 반영한다.

## Implementation Strategy

**MVP**: T001–T014로 첫 안내 연습·엘리베이터 피드백·계단 선택 완료·메뉴 복귀까지 실제 실행한다. MVP만으로 두 상황·우회·최종 결과까지 완료했다고 표시하지 않는다.

**Incremental delivery**: US1의 독립 실행 증거 확보 → US2 직접/우회/오답 후 완료 → US3 결과·중단·회귀 → 최종 청취/빌드/기기/import 검증으로 진행한다. 사람/기기/수신 환경이 필요한 작업은 구체적인 파일과 절차를 준비하고, 확보되지 않은 부분은 미완료로 남긴다.

## 공통 증거·완료 기준

T014/T021/T026/T034의 동작 변경은 헌장 v1.3.0에 따라 실제 실행 MP4를 원본 1920×1080 이상·30fps 이상·게임 소리 포함으로 남긴다. 다른 앱/마이크는 제외하고 PC 전체 소리 녹음은 사용하지 않는다. 새 녹음 방식/입력 설정은 다른 앱이 소리를 내는 동안 Unity 무음/유음 구간으로 입력 분리를 확인한다. 기존 동일 방식 검증을 재사용하면 기록을 직접 가리킨다.

저장 MP4를 실제 재생해 화면·문구·게임 소리·동기화를 확인한다. 오디오 트랙 존재·파일 존재만으로 검증 완료하지 않는다. 녹화/소리/화질 기준 불가 시 가능한 최선의 영상·단계별 원본 스크린샷·분리 게임 소리 등 대체 자료와 불가 사유·실제 조건·미검증 범위를 남긴다. 대체도 없으면 기준 충족/시각 검증 완료를 주장하지 않는다.

영상·스크린샷·검증 원본은 Artifacts/Validation/VisualEvidence 아래 Git 비추적으로 보관하고 git check-ignore/git ls-files로 확인한다. 추적 문서에는 결과 요약과 원본 참조만 남긴다. 대표 파일을 직접 링크한다. Editor 합성 입력·실제 Quest·수신 환경 결과와 새 음원 승인 상태를 구분한다.

## Requirement Coverage

| 요구 | 구현 / 검증 작업 |
|---|---|
| FR-001 | T012, T024, T026, T033 |
| FR-002–003 | T006, T009, T018–T021 |
| FR-004–006 | T005, T011, T013, T018, T021 |
| FR-007–009 | T007–T008, T011, T014–T016, T020–T021 |
| FR-010–011 | T015–T016, T022–T023, T026 |
| FR-012–013 | T011–T013, T023, T025–T026, T029 |
| FR-014 | T004, T020, T022, T024–T026 |
| FR-015–016 | T006, T019, T023, T027–T028 |
| SC-001–003 | T014–T016, T021–T023, T026 |
| SC-004 | T009, T018, T021 |
| SC-005–006 | T025–T026, T029, T032–T033 |
| SC-007 | T006, T019, T023, T027–T028 |

미완료 체크박스는 실제 구현·검증 미완료를 뜻한다. 문서 생성 시점에는 35개 전부 미완료다.
