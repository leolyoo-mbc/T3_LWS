# Research: 바닥 상승형 버튼

**Date**: 2026-11-16. 현재 소스/설치 버전을 확인했다. 아래는 구현 설계이며 외관 승인/실행 검증 결과가 아니다.

## R1. 공용 Canvas 분리

**Decision**: SharedSystems에 바닥 World Space Canvas 1개를 추가하고 기존 Canvas/내레이터/EventSystem을 유지한다.

**Rationale**: `SharedContentUI.cs`는 Actions를 자막 행으로 배치하며 기존 Canvas는 ComfortFollowUI로 움직인다. 독립된 바닥 Canvas가 매 프레임 역변환 보정보다 단순하다. XRI 공식 문서는 World Space/Tracked Device Graphic Raycaster와 단일 활성 EventSystem을 안내한다. [Unity XRI UI 설정](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.5/manual/ui-setup.html)

**Alternatives considered**: 기존 Canvas 안 역변환 보정, 콘텐츠별 Canvas, 3D 콜라이더 입력. 공용 소유/기존 입력 재사용보다 복잡하다.

**Consequence**: `SharedUIContractAudit`의 Canvas==1을 두 지정 공용 역할 검사로 변경한다. 콘텐츠별 Canvas 금지는 유지한다. Unity는 복수 Canvas와 World Space 위치/방향을 지원한다. [Unity Canvas 설명](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/class-Canvas.html)

## R2. 콜백 즉시 실행과 재진입

**Decision**: 입력 잠금/하강 시작 후 기존 콜백을 즉시 실행한다. 내려가는 판은 스냅샷으로 유지하고 다음 요청은 최신 하나만 보관한다.

**Rationale**: `SmokePostureController.Ready()`는 호출 순간 높이를 읽는다. `EvacuationRouteController.Present()`는 빈 SetActions 뒤 새 요청을 동기적으로 보낼 수 있다. 화재는 Send 명령 큐로 진행하므로 양쪽 실행 경계를 모두 지원해야 한다.

**Alternatives considered**: 하강 후 콜백, 콘텐츠마다 연출 복제. 높이 저장 시점 변화와 중복 때문에 제외한다.

## R3. 수동 앵커

**Decision**: 콘텐츠/현재 대상에 연결된 FloorButtonAnchor 참조를 전달한다.

**Rationale**: `Assets/_02LWS/Data/EvacuationRoute/ScenarioA.asset`, `ScenarioB.asset`의 판단 대상은 a-left/a-right/a-lift/b-left/b-right/b-lift 총 6개다. 준비는 A a-w3, B 시작은 b-w1이다. `RouteSession.cs`에서 A 완료 뒤만 다음 연습을 제공하고 B 완료는 Result로 간다. 고개 방향과 무관한 고정 위치가 필요하다.

**Alternatives considered**: 콘텐츠당 단일 지점, 플레이어 정면 생성. 사건별 관찰 위치와 공간 고정 요구를 충족하지 못한다.

## R4. 단순 판 이동

**Decision**: RectTransform 이동/RectMask2D/기존 테마와 TMP를 사용한다. 상승 0.35초/하강 0.25초는 조정 가능한 시작값이다.

**Rationale**: 판만 움직이는 확정 C안에 맞는다. 바닥 아래는 mask로 가려 바닥 두께에 의존하지 않는다. 실제 문구/배율/기울기의 mask와 가독성은 실행 검증한다.

**Alternatives considered**: 홀로그램/클리핑 셰이더, 바닥 개폐구, Tween 패키지. 범위를 넘고 의존성을 늘린다.

## R5. 입력과 정지

**Decision**: FreshPressButton을 재사용하고 Ready/정지 경계에 InvalidatePress를 호출한다. Owner.Blocked/메뉴/로딩은 정지, Clear/재시작/구역 전환은 하드 리셋으로 구분한다.

**Rationale**: `FreshPressButton.Update`는 CanSelect가 거짓이어도 armed가 될 수 있어 Ready 경계에서 무효화가 필요하다. `SharedUIController.BlockWorld`는 기존 contentGroup만 차단하므로 바닥 그룹 연동이 필요하다. 기존 정지는 timescale을 사용하지 않는다.

**Alternatives considered**: 새 InputAction, 시간 간격만으로 중복 방지, timescale=0. 기존 조작/수명 경계를 보존하지 못한다.

## R6. 검증

**Decision**: 기존 `Scripts/Validation/RouteIntegrationChecks.cs` 스타일의 검사와 실제 raycast/영상/Quest 검토를 분리한다.

**Rationale**: 기존 Click 도우미의 pointer 직접 호출은 레이가 실제 판을 맞히는 증거가 아니다. 실제 양손 UI 레이 경로와 헌법 v1.3.0의 게임 소리 분리/직접 재생/비추적 규칙을 추가한다.

**Alternatives considered**: 텍스트 검사만, 목업을 실행 증거로 사용, Editor 합격으로 Quest 합격 처리. 요구사항에 맞지 않는다.

## Resolution

미해결 기술 조사 항목 없음. 실제 위치/크기 조정과 사용자 배치 검토는 구현 후 검증 절차다. 새 교육 행동은 추가하지 않는다.
