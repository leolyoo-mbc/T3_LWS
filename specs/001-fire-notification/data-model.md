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

## 내부 메뉴 상태 — design.md 적용

UISettings는 voiceVolume/effectsVolume(0~1, 기본 1), subtitlesEnabled(기본 true), textScale(1~1.5, 기본 1)을 같은 실행 동안 공유한다. 영구 진도나 저장 기록은 만들지 않는다.

UI 상태는 Closed/Pause/Settings/ConfirmRestart/ConfirmExit다. 메뉴/설정/확인창이 열려 있는 동안 userPaused=true로 교육 명령과 완료 커밋을 보류한다. 취소는 이전 메뉴로 돌아가고 계속하기는 메뉴 스택을 닫는다. 시스템 중단 suspended는 기존 단계 snapshot과 독립한다.

내부 메뉴만 열렸다면 stage·executionId·재생 위치·물체 상태를 보존한다. 시스템 중단이 발생하면 기존 실행은 취소하고 pendingStageRestart를 남긴다. 시스템 중단 요인 해제와 userPaused=false가 모두 충족된 뒤 같은 단계에 새 executionId로 한 번 진입한다. 메뉴 복귀/재시작은 보류 완료와 snapshot을 폐기한다. 내부 메뉴를 닫을 때 held trigger/벨 접촉은 새 입력으로 인정하지 않는다.


## 공간 UI 추종 변경 — 2026-11-05

사용자 승인: 이동·텔레포트·고개 회전 후에도 UI를 읽고 조작할 수 있도록 Meta 공식 자료를 반영한다. 일반 메뉴를 머리에 1:1 고정하지 않고, 계속 찾아야 하는 안내/선택/메뉴 UI만 완만하게 따라오게 한다. 비상벨과 Alarm label은 공간 고정이다. 기존 “열 때 고정하고 수동 재정렬만” 규칙은 이 절로 대체한다.

공식 근거: [Meta HUD Placement](https://github.com/facebook/immersive-web-sdk/blob/main/docs/concepts/spatial-ui/hud.md)는 world-space 기본, 필요한 작은 UI의 dead zone/완만한 수평 추종, head pitch/roll 배제 및 헤드셋 보정을 설명한다(Web SDK API 자체를 Unity에 사용하지 않음). [Spatial UI](https://developers.meta.com/vr/design/hands-3d-best-practices/)와 [Ray casting best practices](https://developers.meta.com/vr/design/raycasting_bp/)는 2026-09-14 갱신: 사용자 지향·roll 고정·식별 가능한 조작 방식·충분한 버튼 간격·상태 피드백. 기존 버튼 스타일/레이 입력은 유지한다.

구현 초기값(AI 기술 판단, 공식 표준 아님): 수평 회전 30도, 머리 이동 .25m 초과 때 추종 시작, 지수 보간 계수 6/s. Origin 위치가 한 프레임에 .5m 초과 이동하면 즉시 새 위치에 재배치한다. 눌린 마우스/양손 XR trigger 동안 일반 추종은 정지하며 텔레포트 재배치는 우선한다. 패널 재활성화·수동 재정렬은 즉시 배치한다. 거리 1.5~1.8m 및 패널별 오프셋은 프로젝트 보정값이다. 작은 텔레포트도 확실하게 처리하도록 텔레포트 완료 신호 연결을 검증한다.

검증: 작은 흔들림 유지, 큰 이동/회전 후 수렴, pitch/roll 미전달, 누름 중 안정화, 텔레포트 및 공간 전환 즉시 배치, 꺼진 패널 재표시, 물리 벨 고정, 기존 메뉴/교육 흐름. Editor 결과와 실제 VR 가독성/불편감 검증은 구분한다.


## 벽 부착형 비상벨 재제작 — 2026-11-05

사용자 확정: 공중에 떠 있는 임시 비상벨과 비현실적인 소리를 교체한다. 소화전함 없는 벽 부착형 발신기를 선택했고 Blender MCP로 직접 모델링하도록 요청했다. 기존 안내 대본·손으로 밀어 누르기·진행 순서는 유지한다. 예전 단순 기하/합성 경보음 선정은 이 변경으로 대체한다.

구현 판단: 국내 원형 발신기·표시등·경종의 외형을 참고해 별도 벽 부착판 위에 자체 모델링한다. 원형 발신기 약 144mm, 경종 약 120mm 외경을 참고하되 특정 제품 복제품·인증품으로 표시하지 않는다. 브랜드/KFI 마크와 제조사 사진은 배포 자산에 복사하지 않는다. 눌리는 중앙 부품을 분리하고 모든 런타임 모델·재질·음원은 Assets/_02LWS 안에 둔다. Blender 편집 원본과 재현 스크립트도 보존하고 Unity에서는 FBX를 사용한다.

실제 전기벨 녹음: Dave Welsh, School Bell - Fire Bell.wav, https://freesound.org/people/Dave%20Welsh/sounds/523317/ (2020-06-21), CC0 https://creativecommons.org/publicdomain/zero/1.0/ . 원저자는 솔레노이드 타격식 Edwards 벨 녹음이라고 설명한다. 국내 특정 모터식 경종과 동일한 녹음이라는 주장은 하지 않는다. 연속 타종 구간을 mono PCM WAV로 편집하고 반복 경계를 crossfade한다. 실제 제품 음압을 헤드셋 출력 목표로 사용하지 않는다.

형상 참고: 국내 제조/공급사 카탈로그 https://waf-e.dubudisk.com/ilshinfa.dubuplus.com/yollam%40naver.com/O18B62L/DubuDisk/www/2021%EC%B9%B4%EB%8B%A4%EB%A1%9C%EA%B7%B8.pdf (원형 발신기 144mm·경종 120mm·표시등 87mm 도면, 검색 색인 확인) 및 https://www.alllitelife.com/page.php?page=sub2_3_7 (발신기/경종/표시등 사진 및 경종 120x50mm 도면 직접 확인). 비율·부착판 배치·눌림 깊이와 음량은 프로젝트 초기 조정값이며 법적 설치 규정/모든 제품의 공통 규격으로 표현하지 않는다.

검증: 실제 벽면 부착, 정면 접근/텔레포트, 분리 누름부와 collider 정합, 표면 접촉·잘못된 입력 거부/유효 깊이 성공, 중단·메뉴·초기화, 전체 교육 순환을 Editor에서 확인한다. 실제 VR의 도달성·식별성·소리 자연스러움 및 Quest 성능은 기기 증거 없이는 미완료다.
