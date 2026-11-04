# Experience Contract — 검토용 설계

외부 인터페이스는 VR 입력, 앱 중단 신호, 통합 프로젝트의 씬 진입이다. HTTP API는 없다. 모듈·필드는 AI 제안이며 교육 동작은 spec.md 기준이다.

## Scene boundary

- `Assets/_02LWS/Scenes/ContentMenu.unity`: 콘텐츠 1만 표시. 레이+새 trigger press로 체험 씬 진입.
- `Assets/_02LWS/Scenes/FireNotification.unity`: 직접 로드해도 첫 안내부터. 이전 씬 snapshot 이어받지 않음.
- 두 씬을 Build Profile Scene List에 등록. 씬마다 XR Origin·AudioListener·EventSystem·XRUIInputModule 하나씩. Single 로드로 이전 씬 제거.
- 콘텐츠 루트는 자신의 음성·진행·연출만 정리. 통합 프로젝트의 전체 audio/timeScale/XR 설정을 전역 변경하지 않음.

## Input → command

| Input | Command | Preconditions | Observable result |
|---|---|---|---|
| 콘텐츠 1 trigger | StartContent | 메뉴, 로드 미진행 | 체험 첫 안내 |
| 행동 UI trigger | ChooseAction(optionId) | AwaitAction, 피드백 중 아님 | 정답 결과 또는 설명 후 재선택 |
| 다음 trigger | Continue | AwaitNext | 상황 복원 후 벨 설명 |
| 벨 유효 push | PressBell | AwaitBellPress, armed, 가상 손 접촉·축 깊이 통과 | 경보·인물 반응 한 번 |
| 메뉴 복귀 trigger | ReturnToMenu | 모든 체험 단계 | 정리 후 메뉴, 재진입은 새 세션 |

hover/dwell, held trigger 재활성화, 벨 ray/grab, 단순 근접은 명령을 생성하지 않는다. 알 수 없는 optionId·현재 단계 외 명령은 상태 변경 없이 거부한다. 양손·반복 입력도 결과 실행은 한 번이다.

## Presentation boundary

- 단계 진입에 stage·executionId·로컬 cue·baseline 전달.
- 완료는 현재 executionId에 한 번만 소비. 취소는 오디오·경보·자막·애니메이션·지연 callback·입력을 정리.
- NPC 기본 행동과 알림 반응을 구분. 두 번째 방법 전에 이전 반응이 없어야 함.
- 필요한 cue·폰트·씬 참조 누락은 개발 검증 실패. 무음 성공·단계 자동 완료로 숨기지 않음.

## Lifecycle boundary

pause/focus/XR 착용 신호를 하나의 suspended 상태로 합성한다. 중단 중 입력·완료는 진행에 영향 없음. 모든 중단 요인이 해제되면 [단계 재진입 규칙](../data-model.md)을 따른다. 프로세스 재시작은 메뉴부터. 헤드셋 탈착·시스템 메뉴는 각각 Quest 2 검증 필수.

## Delivery boundary

소유 에셋·폰트·Input Actions·하위 의존성은 `_02LWS` 내부. 패키지·ProjectSettings는 별도 prerequisite이다. 전달 문서는 현재 Unity/패키지 버전, Android IL2CPP/ARM64·SDK 조건, OpenXR Quest support/Touch profile, 씬 등록, Input System, interaction layer 의미 매핑·충돌 설정, URP·폰트 조건을 기록한다. XRI 전역 레이어 설정을 임의 이동하지 않는다.

통합 대상 지정 후 기존 rig를 사용할 때 중복 rig/EventSystem을 제거하고 동일 입력·단계 계약을 재검증한다. 복사한 제3자 리소스 출처·라이선스를 보존한다.
