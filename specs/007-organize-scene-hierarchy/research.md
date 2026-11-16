# Phase 0 Research: 씬 하이라키 정리

**Date**: 2026-11-17

## 1. 실제 구조와 분류 방식

**Decision**: 기존 `SharedSystems`, `Contents` 루트를 유지하고 중간 분류 그룹을 추가한다. RouteSpace의 `A`, `B`는 `ScenarioA`, `ScenarioB`로 바꾼다. 세 체험 루트와 두 시나리오 루트의 제어 컴포넌트는 옮기지 않는다.

**Rationale**: 열린 FireNotification 씬은 Edit Mode, dirty=false였다. SharedSystems에 공용 기능 9개가 있고 Contents에 메뉴와 세 체험이 있다. A 아래 200개 직속 부품이 있어 구역 분류가 필요하다. ContentSceneNavigator는 참조한 루트를 켜고 끄므로 그 경계를 유지하면 체험 전환을 보존할 수 있다.

**Alternatives considered**: 별도 씬 분리는 체험 전환 구조를 바꾼다. 역할만 묶으면 교실이나 계단을 공간 단위로 편집하기 어렵다. 둘 다 채택하지 않는다.

## 2. 이름에 의존하는 실제 코드

**Decision**: `SharedCanvas`, `FloorCanvas`와 XR 내부 이름을 보존한다. 다른 프로젝트 관리 표시 이름은 바꾸되 콘텐츠와 이동 지점 ID는 바꾸지 않는다.

**Rationale**: SharedUIContractAudit.cs는 두 Canvas 이름을 직접 비교한다. RouteIntegrationChecks.cs는 `Teleport Interactor`를 이름으로 찾는다. `OpenMenuPanel` 참조는 없는 구형 메뉴를 확인하는 검사이므로 새 오브젝트를 만들지 않는다. RouteWaypoint의 id, scenarioId, targetId, adjacentIds는 표시 이름과 별개다.

**Alternatives considered**: 검사 코드를 바꾸면서 모든 이름을 일괄 변경하면 불필요하게 실행 계약까지 바뀐다. 현재 이름을 유지하는 두 예외로 해결한다.

## 3. 부모 관계와 좌표 의존성

**Decision**: XR origin 전체, 캐릭터 Animator 아래, FireAlarm 내부, FloorCanvas/viewport/movingPanel 좌표 계층과 Canvas 내부 UI 부모 관계는 유지한다. 분류용 Transform은 그 밖에 추가한다.

**Rationale**: SharedUIContractAudit 및 EvacuationRouteController는 Canvas/ComfortFollowUI 조상을 찾는다. FloorButtonPresenter는 viewport와 movingPanel의 로컬 회전/크기를 사용한다. BellPushInput과 ScenarioBaseline은 비상벨 cap의 localPosition을 사용한다. FireNotificationAdapter는 체험 아래 음원을 순회하며 FireNotificationController는 하위 경보 presenter를 순회한다. 캐릭터 애니메이션은 내부 경로와 뼈 연결에 의존한다.

**Alternatives considered**: 월드 위치만 보존하고 임의의 내부 부모를 바꾸는 방식은 로컬 축과 조상 검색을 보존하지 못하므로 제외한다.

## 4. 원본 프리팹과 수정 경계

**Decision**: 이번 작업은 씬과 씬의 이름 override만 변경한다. 원본 프리팹, XR Starter Assets와 공용 UI_Button 원본은 그대로 둔다. 이름 변경은 필요한 프로젝트 관리 인스턴스에만 적용한다.

**Rationale**: 열린 씬에서 XR rig, EventSystem, FloorButtonPanel, FireAlarm, LowpolyDummy와 여러 UI_Button 연결을 확인했다. 반면 RouteSpace/A와 B는 프리팹 인스턴스 루트가 아니므로 SchoolRouteA/B.prefab를 수정해도 현재 씬 정리를 대신하지 못한다. 연결되지 않은 프리팹과 과거 ContentMenu 씬까지 수정 범위를 확대하지 않는다.

**Alternatives considered**: 모든 프리팹 원본에 적용하면 기존 다른 사용처에 영향을 준다. Unpack은 원본 연결을 잃으므로 사용하지 않는다.

## 5. 배경 세부 분류

**Decision**: 기존 생성 순서의 반복 교실 부품 묶음을 교실 단위로 보존하고, 복도 구조 및 천장/조명은 Corridor 아래로 묶는다. 교실별 Doors, Windows, Structure, Details와 계단/엘리베이터의 역할 분류를 사용한다.

**Rationale**: 실제 좌표 확인에서 A의 Recessed door leaf/Class number는 x=139.4부터 4m 간격으로 여섯 번 반복되고 창은 대응하는 반복 묶음에 있다. B의 West wing/North wing은 각 두 교실 묶음을 가진다. 세계 좌표로 일괄 정렬하면 회전된 서쪽 복도에서 오분류할 수 있으므로 원래 sibling 블록을 기준으로 한다.

**Alternatives considered**: 가장 가까운 부품으로만 배정하면 반대편 창이나 코너 부품을 잘못 묶을 수 있다. 원래 블록과 역할 이름을 같이 검증한다.

## 6. 검증 방법과 제출 의존성

**Decision**: 새 도구는 Editor 전용이며 전후 비교와 DryRun/Apply/Verify만 제공한다. 기존 런타임 검사는 수정 없이 재사용한다. 패키지 설치와 API 변경은 없다.

**Rationale**: 기존 floor/route 검사는 실제 연결과 진행을 검증하지만 모든 오브젝트의 ID, 직렬화 참조, RectTransform 값과 보호 대상 보존을 대신하지 못한다. 별도 비교가 필요한 이유다. XR Operator는 사용할 수 있을 때 실제 입력 확인에만 사용하고 도구 실행이나 콘텐츠 작동의 필수 조건으로 만들지 않는다.

**Alternatives considered**: 이름 정규식만 확인하면 연결 손실과 레이아웃 변경을 놓친다. 런타임에 하이라키를 재구성하면 편집 결과가 저장되지 않고 실행 부담이 생긴다.

## Sources and remaining questions

로컬 근거: Scripts/Runtime/ContentSceneNavigator.cs, SharedUIContractAudit.cs, FloorButtonPresenter.cs, FireNotificationController.cs, FireNotificationAdapter.cs, Scripts/Interaction/BellPushInput.cs, RouteWaypoint.cs, Scripts/Presentation/ScenarioBaseline.cs, ConversationBystander.cs, Scripts/Validation/RouteIntegrationChecks.cs 및 열린 씬의 읽기 전용 조사.

별도 읽기 전용 연구 에이전트가 이름/조상/로컬 좌표 의존성과 기존 검증 진입점을 교차 확인했다. 새 외부 API를 도입하거나 외부 문서의 사실을 전제로 삼지 않았다. 미해결 NEEDS CLARIFICATION은 없다.
