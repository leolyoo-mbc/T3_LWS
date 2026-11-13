# Contract: 경로 판단 체험과 공용 시스템

## 사용자 화면과 행동

| 단계 | 표시 / 입력 | 결과 |
|---|---|---|
| 준비 | 확인 방법, 조작법 + 준비 완료 | 방향/정답 미리 지시하지 않음 |
| 탐색 | 중립 인접 앵커, 필수 조작 안내 | 실제 도착에 따라 대상 갱신 |
| 계단 앞 | ‘이 계단으로 대피한다 / 다른 경로를 찾는다’ | Available이면 완료, 막혔다면 이동 없이 피드백 |
| 엘리베이터 앞 | ‘이용한다 / 다른 경로를 찾는다’ | 탑승 없음, 이유 안내 후 재선택 |
| 다른 경로 | 대상 선택지 해제, 이동 재개 | 직접 돌아서 다른 계단 탐색 |
| A 완료 | 안내 종료 후 다음 연습 | 기록 보존, B 시작 위치로 이동 |
| B 완료 | 최종 안내 후 공용 결과 | 상황별 완료, 실제 우회, 첫 판단/피드백 후 완료 구분 |

대상명을 명시하고 이전 대상 선택은 무효화한다. 문구, 색, 표시 유무로 정답을 먼저 드러내지 않는다. 자막 끄기에서도 대상/필수 조작/선택은 남는다. 연기 없는 외관만으로 현실 안전을 보장하지 않는다.

## 공용 API

SharedContentUI.Bind(owner, heading), SetStatus(owner, text), SetActions(owner, 최대 2개 ContentUIAction), Clear(owner)를 사용한다. ContentExperienceAdapter의 기존 수명, 설정, 이벤트, 결과 계약을 유지한다. A 완료는 ResultVisible=false, B 완료 안내 후에만 true다.

ContentEntry id=`safe-evacuation-route`, root=RouteSpace, anchor=A 시작점, adapter=EvacuationRouteController, locomotionAllowed=true로 등록한다. 메뉴는 기존 테마 버튼을 명시적으로 추가한다. 공용 화면에 콘텐츠 타입별 분기를 추가하지 않는다.

## 최소 공용 변경

ContentExperienceAdapter에 `public virtual bool LocomotionAllowed => !Blocked`를 추가한다. navigator.MovementAllowed의 기존 loading/lifecycle/content 조건에 이를 AND한다. 기존 두 어댑터는 기본 구현으로 기존 동작을 유지한다. Blocked는 실제 중단/추적/설정 오류용이며 일반 선택지 노출에 사용하지 않는다.

MenuTeleportationProvider API는 바꾸지 않는다. controller는 이동 잠금, Stop/Restart, 구역 전환 때 기존 CancelPending()을 호출한다. 공유 UI는 새 콘텐츠를 직접 참조하지 않는다.

## 신규 내부 경계

- `RouteSession.Begin(long runId, RouteScenario[] scenarios)`은 새 실행을 생성한다. Stage/Execution/Blocked/CurrentTargetId/Results는 읽기 전용으로 노출한다.
- `Ready(long runId, long execution)`, `Arrive(long runId, long execution, string waypointId)`, `LeaveCheckpoint(long runId, long execution, string targetId)`, `Choose(long runId, long execution, string targetId, RouteChoice choice)`, `GuidanceDone(long runId, long execution)`, `NextScenario(long runId, long execution)`는 수락 bool을 반환한다.
- `bool TransitionDone(long runId, long execution)`은 현재 RunId/Execution이 일치하고 차단되지 않은 Transition 상태에서만 수락한다. controller가 B 공간 활성화, 시작 위치 이동, UI Recenter를 성공적으로 마친 뒤 호출하며, 수락 시 Execution을 증가시키고 Intro(B)로 전이한다. 다른 상태, 중복 호출, 재시작 전 실행의 완료 호출은 false로 거부한다. 전환 실패 시 호출하지 않고 Configuration 차단과 오류 안내, 메뉴 복귀를 제공한다. 중단 중 완료 호출은 거부하며, 복구 후 controller가 현재 실행의 실제 전환 상태를 재확인해 유효한 완료만 전달한다.
- `SetBlockers(RouteBlocker blockers)`, `Close()`는 세션 제어용이다. 세션은 판단/전이, controller는 위치/표시/소리의 부수효과를 소유한다.
- RouteWaypoint은 ID, 대상, 인접 지점, anchor만 연결한다. 도착 검출/구독 해제는 controller 단일 소유다. 클릭만으로 도착을 보고하지 않는다.
- RouteGuidanceCatalog는 `GuidanceCue Get(string id)`와 `void Validate()`를, RouteSmokePresenter는 `Play()`, `SetPaused(bool)`, `Stop()`을 제공한다.

## 수명, 실패

낡은 RunId/Execution/TargetId 입력, 콜백은 거부한다. 일반 pause는 음성 위치 보존, 시스템 복구는 현재 안내 재제공이며 새 선택/이동 입력을 요구한다. 구성 오류를 성공으로 표시하지 않고 메뉴 복귀를 제공한다.

공용 Canvas/안내 재생기/UI 추적기는 각각 1개다. 콘텐츠 내부 Canvas/안내 재생기/전용 UI 입력은 0개다. 150% 문구는 기존 공용 줄바꿈, 높이 확장 규칙 안에서 처리한다.
