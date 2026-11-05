# Data Model — 검토용 설계

확정 교육 흐름을 표현하는 AI 설계 제안이다. 네트워크·영구 저장은 없다.

## Entities

| Entity | Fields | Relationships / validation |
|---|---|---|
| ExperienceSession | sessionId, stage, executionId, suspended, transitionInFlight | 체험 공간 진입마다 새 세션. 단계 진입/재진입 시 executionId 갱신. 중단 중 명령 거부 |
| ActionOption | id, label, isShout, feedbackCueId | ‘큰소리로 알리기’와 ‘다른 사람이 알릴 때까지 기다리기’ 두 항목만 제공. 오답에 확정 피드백의 음성·자막 cue 필수 |
| GuidanceCue | id, audioClip, subtitleSegments | 로컬 음성 필수, 한글 글리프 포함. segment 시작/끝 시간이 유효하고 음성과 같은 의미 전달 |
| ScenarioBaseline | npcTransforms, idleStates, alarmCapRestPose, fireVisualState, playerAnchor | 최초와 같은 화재 상황 복원 기준. 각 결과의 반응 시작/종료 상태도 정의 |
| BellState | armed, normalizedDepth, pressed, contactReleased | 표면 접촉 후 축 방향 깊이 통과만 press. ray/grab 금지. 복귀 때 접촉 해제 후 재무장 |
| PresentationRun | executionId, stage, completionStatus | 음성·애니메이션·지연 callback 수명. 취소된 실행 완료는 무시 |
| SuspensionSnapshot | stage, stageEntryContext | 같은 프로세스 메모리에만 보존. 메뉴 복귀·앱 재실행에는 사용하지 않음 |

## Shared object lifetime

SharedSystems는 메뉴/체험 공간과 형제인 항상 활성 루트다. XR Origin·AudioListener·EventSystem·XRUIInputModule·SpaceNavigation·SharedDirectionalLight·ExperienceLifecycle은 하나씩 유지한다. 활성 세션이 없는 메뉴에서도 lifecycle은 중단 요인을 감지하지만, controller는 활성 체험 중에만 구독한다. 공간 복귀 후 새 세션을 시작할 때 현재 Blocked 상태를 확인한다. 콘텐츠 전용 상태/연출은 공간 종료 시 기존 규칙대로 정리한다.

## Stage transitions

메뉴는 같은 씬의 별도 공간이다. 공용 XR 플레이어가 선택 공간의 시작 앵커로 이동한다. 체험은 FR-019의 9단계와 대응한다.

| Stage | 진입 상태 | 허용 완료/입력 | Next |
|---|---|---|---|
| SituationGuidance | 초기 복도·상황 음성/자막, 선택 차단 | 현재 안내 완료 | AwaitAction |
| AwaitAction | 행동 선택지 표시 | 정답 선택 | ShoutResult |
| AwaitAction | 오답 선택 시 내부 feedback 모드, 입력 잠금 | 피드백 완료 | 같은 선택지 대기 |
| ShoutResult | 반응 시작 상태부터 “불이야!”·인물 반응 | 현재 결과 완료 | AwaitNext |
| AwaitNext | 큰소리 결과 종료 모습, 다음 버튼 | 다음 trigger | RestoreSituation |
| RestoreSituation | 이전 연출 취소·기본 복도 복원 | 복원 완료 | BellGuidance |
| BellGuidance | 초기 인물 행동, 벨 미활성, 역할 음성/자막 | 현재 안내 완료 | AwaitBellPress |
| AwaitBellPress | 경보 없음, 벨 초기 위치·재무장 | 가상 손 유효 push | AlarmResult |
| AlarmResult | 경보·인물 반응 시작부터 | 로컬 결과 구간 완료 | CompletionGuidance |
| CompletionGuidance | 완료 안내와 메뉴 버튼 | 메뉴 trigger | 같은 씬의 메뉴 공간 |

AlarmResult는 경보 loop 자체 종료를 기다리지 않고 설정된 결과 구간 완료로 종료한다. 완료 안내에서 loop를 정지해 안내를 들을 수 있게 하는 안이다. 길이는 연출 리소스 검토 때 정한다.

## Universal rules

1. 모든 단계에서 메뉴 복귀 허용. 새 입력 차단 → 실행 취소 → 음성/경보/반응 정리 → snapshot 폐기 → 체험 공간 비활성화 → 메뉴 앵커 이동 → 메뉴 공간 활성화. 씬 로드 없음. 중복 요청은 합친다.
2. 중단은 stage를 바꾸지 않는다. 현재 실행을 취소하고 입력·진행·음성·연출을 멈춘다. 모든 중단 요인 해제 시 같은 단계 시작 상태를 새 executionId로 한 번 재구성한다.
3. 대기 단계에서는 자동 성공 없음. AwaitAction의 오답 피드백은 내부 동작이므로 복귀하면 같은 선택지를 다시 기다린다.
4. 결과 단계는 해당 결과만 처음부터 재생. AwaitNext 복귀는 결과 종료 모습+다음 대기. CompletionGuidance 복귀는 완료 안내 처음부터. 이전 완료 단계 전체를 다시 실행하지 않는다.
5. RestoreSituation 중단은 복원 작업을 취소하고 복귀 시 복원부터 다시 수행. 반복 복원 결과는 같아야 한다.
6. 중복 pause/focus는 snapshot을 덮어쓰거나 재시작을 반복하지 않는다. 활성 세션 없는 시작 이벤트를 체험 복귀로 처리하지 않는다.
7. stage와 executionId가 맞는 완료만 한 번 소비한다. 메뉴/중단과 완료가 동시에 발생하면 메뉴/중단을 우선한다.
8. UI trigger는 release 후 새 press만 인정한다. 벨도 손을 벨 밖으로 뺀 뒤 새 접촉·push를 요구한다. 중단 전에 잡고 있던 입력으로 자동 완료하지 않는다.

## 편집 모드 공간 표시 — 2026-11-05

사용자 확정: 편집 모드에서는 모든 스페이스를 활성화해 배치를 확인한다. 구현은 MenuSpace·FireNotificationSpace를 활성화한 씬을 저장하는 방식이다. Play 시작 시 기존 ContentSceneNavigator.Start가 메뉴만 활성화하고 체험 공간을 끈다. startOnSceneEntry=false를 유지해 교육은 선택 후 시작한다. Play 종료 시 편집 상태의 두 공간 활성화로 복원되는지 확인한다. Play 중 전체 표시 기능은 추가하지 않는다.
