# Data Model: 안전한 대피 경로 판단

## 모델과 관계

| 모델 | 필드 / 관계 | 규칙 |
|---|---|---|
| RouteScenario | Id, IsGuided, StartWaypointId, Targets, Waypoints | 고정 A/B. A 계단 2개 Available, B Available/SmokeBlocked 각 1개, 각 엘리베이터 1개 |
| RouteTarget | Id, Kind(Stair/Elevator), State(Available/SmokeBlocked/Unavailable), CheckpointId | Stair는 Available/SmokeBlocked, Elevator는 Unavailable |
| RouteWaypoint | Id, ScenarioId, TargetId(optional), AdjacentIds | 동일 상황 안의 양방향 연결. 시작부터 모든 확인 지점 왕복 가능 |
| RouteDecision | ScenarioId, TargetId, Choice(Use/FindAnotherRoute), Sequence, FeedbackRequired | 최초 선택 보존, 후속 기록 추가 |
| RouteScenarioResult | ScenarioId, Completed, ChosenStairId, Decisions, DetourPerformed | Available Stair Use만 완료. 미방문은 미수행 |
| RouteSession | RunId, Execution, Stage, ScenarioIndex, CurrentTargetId, Blockers, Results, DetourOriginId | 새 실행은 새 RunId, 단계/대상 변경 시 Execution 증가 |

순수 모델은 UnityEngine에 의존하지 않는다. Transform·AudioClip은 컴포넌트/catalog에 둔다. ID는 상황을 포함한 ordinal 고유 문자열(예: a-stair-east)로 두고 저장 파일/네트워크 스키마는 만들지 않는다.

## 상태 전이

| 현재 | 입력 / 이벤트 | 다음·효과 |
|---|---|---|
| Closed | Begin | AwaitingReady, 초기화 |
| AwaitingReady | Ready | Intro(A) |
| Intro | GuidanceDone | Explore |
| Explore | 실제 확인 지점 도착 | Decision, 대상의 두 선택지 |
| Decision | 확인 영역 이탈/중간 지점 이동 | Explore, 대상 무효화 |
| Decision | Use + Available Stair | ScenarioCompletion, 완료 기록 |
| Decision | Use + 막힌 계단/엘리베이터 | Feedback, 오답 기록·입력/이동 잠금 |
| Feedback | GuidanceDone | 대상 영역 안이면 Decision, 밖이면 Explore |
| Decision | FindAnotherRoute | Explore, 해당 영역을 떠나기 전 재진입 억제 |
| ScenarioCompletion(A) | 안내 종료 후 Next | Transition, 예약 취소·A 끄기·B 시작 이동 |
| Transition | 전환 성공 | Intro(B) |
| ScenarioCompletion(B) | 안내 종료 | Result |
| 모든 상태 | Stop / Restart | Closed / 새 AwaitingReady(A) |

입력·비동기 완료는 현재 RunId/Execution/TargetId와 일치해야 한다. 반복 호출은 첫 전이 뒤 거부한다. 피드백으로 최초 기록을 지우지 않는다.

## 우회·차단·도착

- 막힌 계단 FindAnotherRoute → 영역 이탈 → 다른 Available Stair 도착 → Use 완료의 순서만 DetourPerformed=true다. 중간에 엘리베이터 방문/오답이 있어도 이 순서를 완료하면 우회를 인정하고 최초 오답은 보존한다. 직접 완료는 Completed=true, DetourPerformed=false다.
- Blockers는 UserPause/Lifecycle/Tracking/Configuration이다. 하나라도 있으면 진행·선택·이동을 거부한다. 설정 오류 안내와 메뉴 복귀는 가능해야 한다.
- LocomotionAllowed는 차단 없음 AND Explore/Decision이다. 안내·피드백·전환·결과 중 이동하지 않는다. Ready/Next는 해당 단계에서만, 대상 선택은 Decision에서만 표시한다.
- 유효한 추적·활성 상황·바닥 투영 위치로 도착/이탈을 확인한다. 텔레포트는 locomotionEnded 후 확인한다. 기본 반경 0.65 m, 지점 간 최소 2 m는 제작값이며 안전 기준이 아니다. 실제 걸음으로 진입한 경우도 같은 검사를 적용한다.
- 추적 공급자의 정상 teleport revision 변경은 오류가 아니다. 시선·방문만으로 관찰/이해 성공을 주장하지 않는다. 중단은 기록 보존, 재시작/종료는 초기화한다.

## 구성 검증

중복 ID·없는 인접 ID·누락 target/root/anchor/UI·누락 cue·확인 영역 겹침을 시작 전에 거부한다. B는 연기 영역을 통과하지 않고 Available 계단까지 도달할 수 있어야 한다. A/B 출발·계단 방향·엘리베이터 배치 차이를 검사한다. 실패 시 Configuration 차단과 메뉴 복귀를 제공하고 완료를 부여하지 않는다.
