# Tasks: 자막 가독성과 표시 흐름 개선

**Input**: `specs/009-refine-subtitle-presentation/`의 spec.md, plan.md, research.md, data-model.md, contracts/subtitle-presentation.md, quickstart.md

**Prerequisites**: 위 설계 문서와 프로젝트 헌법 1.3.0. Unity 6000.3.26f1, 기존 세 콘텐츠와 음원.

**Tests**: 명세 FR-012/성공 기준과 계획의 구성/통합 검사를 구현한다. 검사 항목을 먼저 추가해 기존 동작에서의 실패를 확인하고 해당 구현 후 재검사한다. 구조 검사/실제 음성 청취/Quest 합격은 구분한다.

**Organization**: US1=P1 거리와 스타일, US2=P1 중복 방지와 경고 우선, US3=P2 자막 분할과 음성 동기.

## Format and Paths

**→ 형식:** 모든 작업은 체크박스, T번호, 선택적 [P], 스토리 작업의 [US번호], 대상 경로를 포함한다. [P]는 해당 선행 조건이 끝난 뒤 서로 다른 파일을 독립적으로 처리할 수 있다는 뜻이다. 같은 Unity 씬이나 Editor 세션을 동시에 수정하지 않는다.

**→ 경로:** 아래 파일 경로는 프로젝트 루트 기준이다. 새 Unity 자산/스크립트의 .meta도 함께 관리한다. 검증 원본은 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/`에 보관하며 아래의 verification.md/baseline.json/subtitle-review.md/execution.mp4는 실제 생성한 같은 폴더에 둔다. 문서의 날짜 형식은 폴더 명명 규칙이며 기존 자료를 덮어쓰지 않는다.

## Phase 1: Setup

- [ ] T001 `Assets/_02LWS/Scenes/FireNotification.unity`와 `specs/009-refine-subtitle-presentation/quickstart.md`를 기준으로 Unity 연결/Console/작업 트리를 확인하고, 현재 원문 전체와 WAV SHA-256, 공통 머티리얼 참조를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/baseline.json`에 기록한다. 음성 재합성이나 원본 변경 없이 기준을 확보한다.
- [ ] T002 같은 씬에서 일반 상태 행을 비운 동일 자막의 100%/150% 기준 화면과 앞쪽 거리/시야상 크기/아래쪽 각도를 기록하고, 메뉴/결과/일반 안내/바닥 버튼의 기존 위치를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/verification.md`에 남긴다. 구현 전에 같은 환경/기기/refresh rate/해상도/품질/장면/시작 상태/입력 구간을 기록하고 5초 예열 후 30초 CPU/GPU frame time의 중앙값/95백분위를 같은 폴더 baseline.json에 저장해 T024의 비교 기준으로 삼는다. 측정 불가 항목은 미검증으로 남긴다. 위아래 완전 동일 문장 중복을 실제 화면에서 조사하고 재현/미재현을 구분한다.

## Phase 2: Foundational

- [ ] T003 `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`에 기존 RouteIntegrationChecks 패턴의 ValidateConfiguration/RunPlayMode, Running/Passed/Failed와 계약의 두 Editor 메뉴 진입점을 추가한다. 증거 경로를 받을 수 있게 하고 검사용 임시 객체와 owner/설정 상태를 종료/실패 시 정리한다. 아직 구현되지 않은 기능을 성공으로 보고하지 않는다.
- [ ] T004 `Assets/_02LWS/Scripts/Runtime/SharedUIContractAudit.cs`에 자막 전용 참조/머티리얼과 raycastTarget 검사 확장 지점을 마련한다. 기존 Canvas 2개/ComfortFollowUI 1개/GuidancePresenter 1개/EventSystem 1개/바닥 버튼 계약을 유지하고 현재 씬의 기준 검사 결과를 기록한다.

**→ Checkpoint:** T001~T004 이후 스토리 구현을 시작한다. 기존 실패는 새 자막 실패와 구분한다.

## Phase 3: User Story 1 - 배경판 없이 가까운 자막 읽기 (P1, MVP)

**Goal**: 자막만 1m로 옮겨 보이는 크기를 유지하고 배경 없이 흰 글자/검정 외곽선으로 표시한다.

**Independent Test**: 같은 문구/안정된 추종 상태에서 1.00m ± 0.02m, 시야상 크기와 아래쪽 각도 차이 각각 5% 이내, 배경 0개/다른 UI 변경 0건을 확인한다. 6개 배경/크기 조합의 최종 시각 합격은 T025/T026에서 확인한다.

- [ ] T005 [US1] `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`에 T002 기준 대비 거리/시야각/100%와150%/부모 숨김 상속/다른 UI 보존 검사를 먼저 추가하고 현재 1.8m/배경 표시 상태에서 실패를 확인한다. 전용 머티리얼 격리와 TMP 패딩/잘림 검사도 포함한다.
- [ ] T006 [P] [US1] `Assets/_02LWS/Art/Materials/SubtitleOutline.mat`를 기존 LwsKorean 머티리얼에서 복제한다. 기존 font/atlas/셰이더 참조를 유지하고 흰 FaceColor/검정 OutlineColor/OUTLINE_ON/OutlineWidth=0.15/OutlineSoftness=0/Underlay 비활성을 설정한다. 공통 머티리얼과 ZTest를 바꾸지 않는다.
- [ ] T007 [US1] 연결된 Editor에서 `Assets/_02LWS/Scenes/FireNotification.unity`의 ContentPanel 아래 SubtitleRoot를 추가하고 기존 SubtitleText만 이동한다. 기존 TMP/GuidancePresenter 참조를 보존하며 root에는 별도 Canvas/Image/CanvasGroup/레이캐스터를 추가하지 않는다. 자막 raycastTarget=false와 전용 머티리얼 참조를 저장한다.
- [ ] T008 [US1] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`에서 r=5/9, 부모 로컬 추종 기준 머리점 H에 대한 root.localPosition=(1-r)H와 root.scale=r를 적용한다. 원래 폭 1352u/상단 여백 24u/상단 기준을 유지하며 기준값에서 계산해 누적 이동을 막는다. 자막을 상태 패널 높이/배경 표시 계산에서 제외하고 일반 상태 배경/레이아웃은 보존한다.
- [ ] T009 [US1] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`의 ApplySettings에서 `Assets/_02LWS/Scripts/Runtime/SharedUIController.cs`의 기존 ThemedText.Apply 이후 자막 전용 프리셋을 유지시킨다. 100%/150%와 패널 재활성화/메뉴 복귀 후에도 프리셋을 보존하고 문구/크기/가시성 변경 시만 측정한다.
- [ ] T010 [US1] `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`의 US1 검사와 공용 UI 계약을 실행한다. 일반 안내/메뉴/결과/바닥 버튼 위치 보존, 재정렬/숙이기/순간이동의 기존 추종 동작을 확인하고 결과를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/verification.md`에 기록한다.

**→ MVP:** Setup/Foundation과 T005~T010으로 거리/스타일 변경을 독립 검증할 수 있다. 전체 기능 완료에는 US2/US3와 최종 증거가 필요하다.

## Phase 4: User Story 2 - 중복 안내 없이 한 번에 하나의 일반 안내 읽기 (P1)

**Goal**: 필수 경고 > 자막 켜진 안내 재생 > 일반 행동 안내 순서로 표시하고 종료/경고 해제 시 현재 상태를 복원한다.

**Independent Test**: 짧은 기존 cue로도 음성/무자막 간격/자막 설정 토글/경고/owner 전환을 검증할 수 있다. 경고와 일반 안내/자막 동시 표시, 오래된 안내 복원, 무자막 간격의 일반 안내 깜박임은 각각 0건이어야 한다.

- [ ] T011 [US2] `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`에 Instruction/Warning, 자막 켜짐/꺼짐, 무자막 간격, 경고 발생/해제, 재생 중 설정 토글과 owner 교체에 대한 표시 표 검사를 추가하고 기존 동시 표시 실패를 확인한다. 텍스트 자체가 같은 경우도 검증한다.
- [ ] T012 [US2] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`에 ContentStatusKind(Instruction/Warning), SetStatus(owner,text,kind=Instruction), 사용자 SubtitlesEnabled를 구현한다. LateUpdate에서 Warning > SubtitlesEnabled && IsPresenting > Instruction 순으로 렌더러를 결정하며 text를 지워 숨기지 않는다. 비어 있지 않은 Warning만 우선하고 기존 호출 호환성을 보존한다.
- [ ] T013 [P] [US2] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`의 추적/구성 오류에 Warning을 명시한다. 일반 준비/연습/완료 안내는 Instruction으로 유지하고 기존 음성/교육 단계/중단 정책은 바꾸지 않는다.
- [ ] T014 [P] [US2] `Assets/_02LWS/Scripts/Runtime/EvacuationRouteController.cs`의 추적/구성/전환 실패에 Warning을 명시한다. ApplySettings의 subtitle.enabled 직접 쓰기를 제거해 공용 뷰로 일원화하고 기존 음성/이동/완료 흐름을 보존한다.
- [ ] T015 [US2] `Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs`의 Bind/Clear/설정/경고 해제를 최신 owner와 현재 음성 상태 기준으로 처리해 오래된 자막/행동 안내 복원을 막는다. `Assets/_02LWS/Scripts/Runtime/FireNotificationAdapter.cs`와 `Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs`의 기존 시스템 중단/메뉴 진입 오류 경로를 확인하고 그 표시가 숨겨지지 않도록 필요한 공용 연결만 조정한다.
- [ ] T016 [US2] `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`의 US2 검사를 세 콘텐츠에 실행한다. 경고 해제 후 현재 구간/행동 안내, 꺼진 자막 미복원, 일시정지/재개/취소/다시하기/메뉴 복귀/연속 cue에서 오래된 문구 0건을 확인하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/verification.md`에 기록한다.

## Phase 5: User Story 3 - 말하는 부분만 순서대로 읽기 (P2)

**Goal**: 기존 음성/원문을 보존하고 현재 발화에 해당하는 문장/의미 단위를 일반/150%에서 최대 두 줄로 순차 표시한다.

**Independent Test**: 전체 cue의 원문/해시/구간 제약/두 줄 검사와 구간 전후 표시를 확인하고, 전체 음성을 실제로 들으며 발화와 자막 교체를 비교한다. 합성 시간 이동만으로 청취 합격을 선언하지 않는다.

- [ ] T017 [US3] `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`에 전체 카탈로그 순회 검사와 재생 경계 검사를 추가한다. 제약은 “0 <= start < end <= audio.length+0.03”, “앞 end <= 다음 start”, “단일 공백으로 합친 원문 일치”다. 기존 `Assets/_02LWS/Scripts/Presentation/GuidanceCue.cs`의 Validate를 재사용하고 일반/150% 실제 TMP lineCount<=2/잘림 0, 원문/WAV 해시 보존과 긴 기존 구간의 실패를 확인한다.
- [ ] T018 [US3] `Assets/_02LWS/Scripts/Presentation/GuidancePresenter.cs`의 구간 선택/Elapsed를 실제 AudioSource.timeSamples/clip.frequency 기준으로 바꾼다. Play 시작 프레임/자연 종료 위치 초기화/Pause/UnPause/Cancel/연속 Play를 처리하고 IsPresenting은 무자막 간격/pause에 true, 종료/취소에 false로 유지한다. 현재 run의 완료 콜백 1회와 기존 중단 동작을 검증한다.
- [ ] T019 [P] [US3] `Assets/_02LWS/Content/GuidanceCatalog.asset`의 전체 cue를 150% 기준으로 검토하고 긴 구간만 문장/의미 단위로 나눈다. `Assets/_02LWS/Content/VoiceManifest.json`의 기존 생성 경계를 재사용하며 추가 경계는 기존 WAV 파형과 청취로 정한다. 원문/음원/manifest를 보존하고 T017의 세 제약을 만족시킨다.
- [ ] T020 [P] [US3] `Assets/_02LWS/Data/SmokePosture/GuidanceCatalog.asset`의 6개 cue를 문장/의미 단위로 분할한다. 기존 단어 사이 공백에서 나누고 기존 WAV 파형/실제 청취로 시각을 작성한다. 원문/음원/manifest 보존, T017의 세 제약과 일반/150% 최대 두 줄을 만족시킨다.
- [ ] T021 [P] [US3] `Assets/_02LWS/Data/EvacuationRoute/RouteGuidance.asset`의 7개 cue를 문장/의미 단위로 분할한다. 기존 단어 사이 공백에서 나누고 기존 WAV 파형/실제 청취로 시각을 작성한다. 원문/음원/manifest 보존, T017의 세 제약과 일반/150% 최대 두 줄을 만족시킨다.
- [ ] T022 [US3] `Assets/_02LWS/Scripts/Validation/SubtitlePresentationChecks.cs`로 수정한 전체 카탈로그의 구간 직전/직후/무자막 간격/클립 종료/중단을 검사한다. 일반/150% 줄 수/잘림, 원문/해시, 완료 콜백 1회와 US2의 표시 우선순위 회귀를 확인한다.
- [ ] T023 [US3] 전체 음성을 실제 재생해 듣고 구간별 첫 발음 시작/마지막 발음 종료와 실제 자막 표시 시작/종료, 각 오차를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/subtitle-review.md`에 기록한다. 정상 표시 시작은 발화 시작 대비 ±0.2초, 표시 종료는 마지막 발음 종료부터 0.3초 이내이고 발화 종료 전 자막이 사라지지 않아야 한다. 무음/일시정지는 발화에서 제외하고 경고/일시정지의 의도적 숨김은 별도 전환 검사로 확인한다. 길이 비례 시각이나 무음 검출만으로 합격하지 않으며 누락/중복/다음 문장 전체 선행 표시 0건을 확인한다. 청취 불가 항목은 미완료로 남긴다.

## Phase 6: Polish and Cross-Cutting Validation

- [ ] T024 `Assets/_02LWS/Scripts/Runtime/SharedUIContractAudit.cs`, `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`, `Assets/_02LWS/Scripts/Validation/FloorButtonChecks.cs`의 구성/Play Mode 회귀와 세 콘텐츠의 실제 진행을 확인한다. 자막 배치 전제가 달라진 검사만 새 계약에 맞추고 기존 실패와 새 실패를 구분한다. T002의 baseline.json에 기록된 같은 환경/기기/refresh rate/해상도/품질/장면/시작 상태/입력 구간으로 5초 예열 후 30초를 재측정하고 CPU/GPU frame time의 중앙값/95백분위 차이와 매 프레임 측정/머티리얼 할당 여부를 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/verification.md`에 기록한다. 기준값 부재/측정 불가는 미검증으로 남기고 다른 환경의 수치로 대체하지 않는다.
- [ ] T025 `specs/009-refine-subtitle-presentation/quickstart.md`에 따라 밝은 벽/불/어두운 복도 × 일반/150%의 6개 조건을 실제 실행으로 검증하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/execution.mp4`와 필요한 PNG를 저장한다. 실제 화면 1920×1080 이상/30fps 이상과 Unity 소리 분리, 저장 영상의 실제 재생/청취를 확인한다. 외곽선 가독성 실패 시 실패로 기록하고 배경/강제 전면 표시를 임의로 도입하지 않는다.
- [ ] T026 실제 Quest에서 세 콘텐츠의 서기/숙이기/머리 회전/순간이동과 일반/150% 자막의 읽기/양안 편안함/컨트롤러 진행을 확인하고 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/verification.md`에 기기/결과를 기록한다. 기기를 실행하지 못했으면 미검증으로 남기며 이 작업을 완료 표시하지 않는다.
- [ ] T027 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/verification.md`에 전체 검사 합계/배치 수치/해시 보존/실제 청취/Quest/성능/실패와 미검증, 영상 해상도/fps/소리 분리/재생 확인을 정리한다. 캡처 조건 미충족 시 사유와 대체 자료를 포함하고 git check-ignore/git ls-files로 원본 제외를 확인한다. `specs/009-refine-subtitle-presentation/tasks.md`에는 실제 완료한 작업만 체크하고 git diff --check 후 대표 증거 파일을 직접 링크해 보고한다. 자동 커밋/푸시/패키지 배포는 하지 않는다.

## Dependencies and Execution Order

**→ 전체 순서:** T001 → T002 → T003 → T004 → US1 → US2 → US3 → T024~T027. 같은 공용 뷰/검사 파일을 수정하는 작업은 순서대로 수행한다.

**→ US1:** T005와 T006은 Foundation 이후 병렬 작성 가능하다. T007은 T006 이후, T008은 T007 이후, T009는 T008 이후, T010은 T005~T009 이후다.

**→ US2:** T011 → T012 → (T013, T014) → T015 → T016. T013/T014는 서로 다른 콘텐츠 파일이므로 API 확정 뒤 병렬 가능하다. US1의 파일 변경과 충돌하므로 기본 실행은 US1 뒤다. 동작 테스트 자체는 짧은 기존 cue로 독립 수행 가능하다.

**→ US3:** T017 → T018 → (T019, T020, T021) → T022 → T023. 세 카탈로그의 구간 작성은 별도 텍스트/청취 검토로 병렬 가능하지만 씬/Editor 조작은 직렬화한다. 통합 저장/검사는 한 세션에서 수행한다. US1의 최종 표시 폭과 US2의 표시 규칙을 기준으로 완료 판정한다.

**→ 최종 검증:** T024는 세 스토리의 구현 이후, T025는 회귀 실패 정리 이후, T026은 기기 사용 가능 시 수행한다. T027은 확보한 결과를 정리할 수 있지만 누락된 청취/기기 검증을 완료로 바꾸는 근거가 아니다.

## Parallel Examples

**→ US1 예시:** T005의 거리/스타일 검사 작성과 T006의 자막 전용 머티리얼 준비는 서로 다른 파일을 사용한다.

**→ US2 예시:** T012 완료 후 T013의 연기 콘텐츠 경고 분류와 T014의 경로 콘텐츠 경고 분류를 병렬로 처리할 수 있다.

**→ US3 예시:** T018 완료 후 T019/T020/T021의 카탈로그별 구간 초안/청취 검토를 분리할 수 있다. 하나의 Editor에 동시에 자산 변경을 보내지 않는다.

## Implementation Strategy

**→ MVP 먼저:** Setup/Foundation과 US1을 완료해 1m/외곽선/배경 제거를 검증한다. MVP는 중간 검토 지점이며 사용자 요청 전체의 완료가 아니다.

**→ 누적 전달:** US2의 경고/중복 방지를 추가하고 짧은 cue로 검증한 뒤, US3에서 전체 자막 데이터를 분할한다. 마지막에 전체 회귀와 실제 증거를 확보한다.

**→ 범위 유지:** 새 Canvas/음성 재합성/공통 디자인 변경/추가 그림자/장애물 회피/강제 전면 표시를 추가하지 않는다. 1m보다 가까운 오브젝트 가림은 여전히 가능하다.

**→ 검증 정직성:** 기존 성공 기록을 이번 변경의 합격으로 재사용하지 않는다. 보고된 완전 동일 문장 중복의 미재현, 청취 불가, 기기 미실행은 각각 구분하고 관련 작업을 임의로 완료 표시하지 않는다.
