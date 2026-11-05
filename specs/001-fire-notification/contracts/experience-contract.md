# Experience Contract — 검토용 설계

외부 인터페이스는 VR 입력, 앱 중단 신호, 통합 프로젝트의 씬 진입이다. HTTP API는 없다. 모듈·필드는 AI 제안이며 교육 동작은 spec.md 기준이다.

## Scene boundary

- `Assets/_02LWS/Scenes/FireNotification.unity` 하나를 기능 진입점으로 등록한다. 직접 실행도 메뉴부터 시작하며 콘텐츠 1만 표시한다. 레이+새 trigger press로 같은 씬의 체험 공간으로 이동한다.
- 공용 XR Origin·AudioListener·EventSystem·XRUIInputModule은 각각 하나다. MenuSpace와 FireNotificationSpace 및 각 시작 앵커를 연결한다. 비선택 공간은 비활성화한다.
- ContentSceneNavigator.StartContent는 전환 중/체험 중 재요청을 거부하고 다음 프레임에 메뉴 비활성화 → 앵커 이동 → 체험 활성화 → FireNotificationController.BeginExperience를 호출한다. 메뉴 복귀 이벤트도 다음 프레임에 체험 비활성화 → 메뉴 앵커 이동 → 메뉴 활성화로 처리한다. BeginExperience는 활성 체험에 새 세션을 만들며 중복 시작은 거부한다. 씬 로드 API를 호출하지 않는다.
- ContentMenu.unity는 마이그레이션 원본으로만 보존하며 빌드·배포에서 제외한다.
- 콘텐츠 루트는 자신의 음성·진행·연출만 정리. 통합 프로젝트의 전체 audio/timeScale/XR 설정을 전역 변경하지 않음.

## Shared systems boundary

- SharedSystems는 menuRoot/contentRoot의 자손이 아니며, 공간 전환에서 비활성화하지 않는다.
- 기존 XR rig·입력/EventSystem·SpaceNavigation·기본 Directional Light·앱 중단 감지는 공용 인스턴스로 유지한다. controller.lifecycle은 SharedLifecycle의 ExperienceLifecycle을 참조한다.
- 기본 조명은 기존 회전·광량을 보존한다. 화재 Point Light와 체험 진행·음성·NPC·벨은 콘텐츠 루트에 유지하고 비선택 공간에서 실행하지 않는다.
- 공용 객체의 ID·활성 상태와 각각 하나인 개수를 메뉴/체험/복귀에서 검사한다. 중단 상태에서 체험을 시작하면 진행하지 않고 해제 시 해당 단계 처음부터 시작해야 한다.

## Input → command

| Input | Command | Preconditions | Observable result |
|---|---|---|---|
| 콘텐츠 1 trigger | StartContent | 메뉴, 공간 전환 미진행 | 체험 첫 안내 |
| 행동 UI trigger | ChooseAction(optionId) | AwaitAction, 피드백 중 아님 | 정답 결과 또는 설명 후 재선택 |
| 다음 trigger | Continue | AwaitNext | 상황 복원 후 벨 설명 |
| 벨 유효 push | PressBell | AwaitBellPress, armed, 가상 손 접촉·축 깊이 통과 | 경보·인물 반응 한 번 |
| 메뉴 복귀 trigger | ReturnToMenu | 모든 체험 단계 | 정리 후 메뉴, 재진입은 새 세션 |

hover/dwell, held trigger 재활성화, 벨 ray/grab, 단순 근접은 명령을 생성하지 않는다. 알 수 없는 optionId·현재 단계 외 명령은 상태 변경 없이 거부한다. 양손·반복 입력도 결과 실행은 한 번이다.

## Presentation boundary

- 단계 진입에 stage·executionId·로컬 cue·baseline 전달.
- 완료는 현재 executionId에 한 번만 소비. 취소는 오디오·경보·자막·애니메이션·지연 callback·입력을 정리.
- NPC 기본 행동과 알림 반응을 구분. 두 번째 방법 전에 이전 반응이 없어야 함.
- 필요한 cue·폰트·공간 참조 누락은 개발 검증 실패. 무음 성공·단계 자동 완료로 숨기지 않음.

## Lifecycle boundary

pause/focus/XR 착용 신호를 하나의 suspended 상태로 합성한다. 중단 중 입력·완료는 진행에 영향 없음. 모든 중단 요인이 해제되면 [단계 재진입 규칙](../data-model.md)을 따른다. 프로세스 재시작은 메뉴부터. 헤드셋 탈착·시스템 메뉴는 각각 Quest 2 검증 필수.

## Delivery boundary

소유 에셋·폰트·Input Actions·하위 의존성은 `_02LWS` 내부. 패키지·ProjectSettings는 별도 prerequisite이다. 전달 문서는 현재 Unity/패키지 버전, Android IL2CPP/ARM64·SDK 조건, OpenXR Quest support/Touch profile, 씬 등록, Input System, interaction layer 의미 매핑·충돌 설정, URP·폰트 조건을 기록한다. XRI 전역 레이어 설정을 임의 이동하지 않는다.

통합 대상 지정 후 기존 rig를 사용할 때 중복 rig/EventSystem을 제거하고 동일 입력·단계 계약을 재검증한다. 복사한 제3자 리소스 출처·라이선스를 보존한다.

## 편집 모드 공간 표시 — 2026-11-05

사용자 확정: 편집 모드에서는 모든 스페이스를 활성화해 배치를 확인한다. 구현은 MenuSpace·FireNotificationSpace를 활성화한 씬을 저장하는 방식이다. Play 시작 시 기존 ContentSceneNavigator.Start가 메뉴만 활성화하고 체험 공간을 끈다. startOnSceneEntry=false를 유지해 교육은 선택 후 시작한다. Play 종료 시 편집 상태의 두 공간 활성화로 복원되는지 확인한다. Play 중 전체 표시 기능은 추가하지 않는다.

## 공통 UI 계약 — FR-024~029

기준은 design.md v0.1.1과 plan.md의 UI 절이다. 공용 UI는 표시·설정·열기/닫기와 RestartRequested/ReturnToMenuRequested 요청만 제공하고 프로젝트 연결부가 교육 컨트롤러를 호출한다. 기존 UI.Select 및 새 press 규칙을 유지하고 PC Esc는 열기/한 단계 닫기, VR는 화면 메뉴 진입을 제공한다. EventSystem과 입력 모듈을 추가 중복 생성하지 않는다.

UISettings는 공용 수명으로 메뉴·체험·재시작에서 유지한다. 모달이 열리면 뒤쪽 UI·벨·이동 입력을 차단하며 추적은 계속한다. 내부 메뉴의 같은 지점 재개는 Lifecycle boundary의 시스템 중단 단계 재시작과 구별한다. 둘이 겹치면 시스템 중단이 우선하며 내부 메뉴까지 닫힌 뒤 한 번 재진입한다. 위 Input→command의 체험 중 ReturnToMenu 요청은 이제 확인창을 경유하며 완료 후 복귀는 직접 실행한다.

Subtitle은 동일 음성 구간을 표시하고 자막 끔은 출력만 숨긴다. 음성 음량 0이나 자막 끔이 교육 단계를 자동 완료시키지 않는다. 음성 완료 시점과 진행 조건은 유지한다. 저장 안내/없는 통합 메뉴는 표시하지 않는다.


## 공간 UI 추종 변경 — 2026-11-05

사용자 승인: 이동·텔레포트·고개 회전 후에도 UI를 읽고 조작할 수 있도록 Meta 공식 자료를 반영한다. 일반 메뉴를 머리에 1:1 고정하지 않고, 계속 찾아야 하는 안내/선택/메뉴 UI만 완만하게 따라오게 한다. 비상벨과 Alarm label은 공간 고정이다. 기존 “열 때 고정하고 수동 재정렬만” 규칙은 이 절로 대체한다.

공식 근거: [Meta HUD Placement](https://github.com/facebook/immersive-web-sdk/blob/main/docs/concepts/spatial-ui/hud.md)는 world-space 기본, 필요한 작은 UI의 dead zone/완만한 수평 추종, head pitch/roll 배제 및 헤드셋 보정을 설명한다(Web SDK API 자체를 Unity에 사용하지 않음). [Spatial UI](https://developers.meta.com/vr/design/hands-3d-best-practices/)와 [Ray casting best practices](https://developers.meta.com/vr/design/raycasting_bp/)는 2026-09-14 갱신: 사용자 지향·roll 고정·식별 가능한 조작 방식·충분한 버튼 간격·상태 피드백. 기존 버튼 스타일/레이 입력은 유지한다.

구현 초기값(AI 기술 판단, 공식 표준 아님): 수평 회전 30도, 머리 이동 .25m 초과 때 추종 시작, 지수 보간 계수 6/s. Origin 위치가 한 프레임에 .5m 초과 이동하면 즉시 새 위치에 재배치한다. 눌린 마우스/양손 XR trigger 동안 일반 추종은 정지하며 텔레포트 재배치는 우선한다. 패널 재활성화·수동 재정렬은 즉시 배치한다. 거리 1.5~1.8m 및 패널별 오프셋은 프로젝트 보정값이다. 작은 텔레포트도 확실하게 처리하도록 텔레포트 완료 신호 연결을 검증한다.

검증: 작은 흔들림 유지, 큰 이동/회전 후 수렴, pitch/roll 미전달, 누름 중 안정화, 텔레포트 및 공간 전환 즉시 배치, 꺼진 패널 재표시, 물리 벨 고정, 기존 메뉴/교육 흐름. Editor 결과와 실제 VR 가독성/불편감 검증은 구분한다.
