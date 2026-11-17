# Research: 자막 가독성과 표시 흐름 개선

**→ 조사 범위:** 현재 저장소와 사용자 결정, 헌법을 조사했다. 실제 보고 장면 재현, 전체 음성 청취와 Quest 실행은 하지 않았다.

## 1. 거리와 공용 패널

**Decision:** ContentPanel 아래 자막 전용 RectTransform 하나를 두고 기존 TMP를 이동한다. Canvas와 추종기는 추가하지 않는다.

**Rationale:** SharedContentUI.RefreshLayout은 상태/자막을 위아래로 배치하고 내용이 있으면 공통 배경을 켠다. ComfortFollowUI는 SharedCanvas 전체를 offset=(0,-0.35,1.8)로 배치한다. 자막만 머리 기준 5/9로 축소/이동하면 기존 메뉴/상태 거리와 CanvasGroup 숨김을 보존할 수 있다.

**Alternatives considered:** Canvas 전체 이동은 다른 UI를 바꾼다. 새 Canvas/추종기는 SharedUIContractAudit의 Canvas 2개/추종기 1개 계약을 깨뜨린다. 배경 크기 계산기는 배경 제거 후 필요 없다.

**Evidence:** Assets/_02LWS/Scripts/Runtime/SharedContentUI.cs, Scripts/Runtime/SharedUIContractAudit.cs, Scripts/Presentation/ComfortFollowUI.cs, Scenes/FireNotification.unity (뒤 세 경로도 Assets/_02LWS 기준).

## 2. 기존 TMP 외곽선

**Decision:** 자막 전용 머티리얼 프리셋으로 기존 SDF 외곽선을 사용한다.

**Rationale:** LwsKoreanSDF.shader에 Outline/Underlay가 있고 현재 씬은 DisasterKorean 폰트를 사용하고 기존 폰트 머티리얼 OutlineWidth는 0이다. atlas padding은 5다. ThemedText.Apply가 폰트/스타일을 재설정하므로 설정 적용/재활성화 뒤 전용 프리셋 유지와 패딩 검사가 필요하다.

**Alternatives considered:** 공통 font material 수정은 메뉴/환경 글자까지 바꾼다. 별도 Shadow/Outline 컴포넌트와 새 셰이더는 기존 TMP 기능과 중복된다. 추가 그림자는 기본 범위가 아니다.

**Evidence:** Assets/_02LWS/Art/Shaders/LwsKoreanSDF.shader, UI/Fonts/DisasterKorean.asset, Scripts/Presentation/ThemedText.cs, Scripts/Runtime/SharedUIController.cs.

## 3. 중복과 경고

**Decision:** 콘텐츠가 Instruction/Warning을 명시하고 공용 뷰가 표시를 결정한다. 사용자 설정과 실제 renderer.enabled를 구분한다.

**Rationale:** 현재 상태와 자막이 동시에 표시되는 구조는 확인했다. 연기/경로는 일반 문구와 오류를 같은 SetStatus 경로로 보낸다. 완전 동일 문장 중복은 실제 화면 추가 조사가 필요하다. IsPresenting을 기준으로 일반 안내를 숨기면 문장 사이에도 깜박이지 않는다.

**Alternatives considered:** 문구 비교는 의미상 중복과 경고를 구분하지 못한다. Blocked 전체를 경고로 취급하면 사용자 일시정지와 혼동한다. text를 지워 숨기면 경고 해제 후 복원과 충돌한다.

## 4. 음원 보존과 구간 분할

**Decision:** 기존 segments만 분할하고 WAV/전체 대사는 유지한다. 기존 생성 시각이 없으면 파형 후보와 실제 청취로 시각을 작성한다.

**Rationale:** 콘텐츠 1은 문장 구간이 있고 연기 6개/경로 7개 cue는 대부분 전체 설명이 한 구간이다. Content/VoiceManifest.json의 situation.generation.segments에는 생성 경계가 있지만 나머지 manifest는 전체 길이/대사/해시만 있다. GuidanceCue.Validate는 비중첩/시간 범위와 단일 공백으로 합친 원문 일치를 요구하므로 기존 단어 사이 공백에서 나눈다.

**Alternatives considered:** 재합성은 승인 범위를 넘는다. 글자 수 비례 시간 분배는 쉼과 속도를 반영하지 못한다. 무음 검출은 후보일 뿐 청취 승인을 대신하지 않는다. 새 런타임 자동 분할기는 필요 없다.

**Evidence:** Assets/_02LWS/Scripts/Presentation/GuidanceCue.cs, Content/GuidanceCatalog.asset, Data/SmokePosture/GuidanceCatalog.asset, Data/EvacuationRoute/RouteGuidance.asset, Content/VoiceManifest.json, Audio/Voice/SmokePosture/VoiceManifest.json, Audio/Voice/EvacuationRoute/VoiceManifest.json.

## 5. 재생과 검증

**Decision:** 실제 오디오 재생 위치를 구간 선택 기준으로 쓰고 전체 재생/일시정지/자연 종료/취소를 검사한다. 구조 검사, 청취, Quest 합격을 구분한다.

**Rationale:** 현재 GuidancePresenter.Elapsed는 unscaledDeltaTime 누적이다. 실제 음성 위치와 같은 시간 기준으로 비교하며 기존 완료 콜백/중단 규칙을 보존해야 한다. RouteIntegrationChecks의 구성/Play Mode/Running/Passed/Failed 검사 패턴을 재사용할 수 있다.

**Alternatives considered:** 합성 시간 이동이나 과거 음성 승인만으로 새 분할 시점과 기기 가독성을 승인할 수 없다.

**→ 분석 후 보완:** SC-004의 프로젝트 검증 기본값은 정상 표시 시작 오차 ±0.2초, 마지막 발음 종료 후 표시 종료 0~+0.3초다. 실제 청취로 발화 시각을 정하고 실제 표시 시각/오차를 기록한다. 성능 비교는 T002에서 구현 전 같은 조건으로 5초 예열/30초 측정한 CPU/GPU 중앙값/95백분위를 확보하고 T024에서 재측정한다. 두 항목은 공식 외부 권고가 아니라 이번 프로젝트의 검증 절차를 구체화한 결정이다.

**→ Research gate:** 구현 접근의 미해결 질문은 없다. 청취/Quest/보고된 중복 재현은 구현 후 검증 의존성으로 남는다.
