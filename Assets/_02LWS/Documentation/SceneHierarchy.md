# FireNotification 씬 편집 안내

대상 씬: `Assets/_02LWS/Scenes/FireNotification.unity`.

이 문서의 경로는 Unity Hierarchy 내부 경로다. 파일과 폴더 경로가 아니다. 기존 오브젝트와 컴포넌트를 재사용했으며 한국어 표시 문구, 체험 ID와 이동 지점 ID는 그대로다.

## 시작 위치

| 수정할 대상 | 펼칠 위치 |
|---|---|
| 플레이어와 컨트롤러 | `SharedSystems/Player/LwsXRStarterRig` |
| 체험 등록과 전환 | `SharedSystems/Navigation/ContentNavigator` |
| EventSystem | `SharedSystems/Input/LwsEventSystem` |
| 포커스/추적 중단 상태 | `SharedSystems/ExperienceLifecycle` |
| 머리 높이 추적 | `SharedSystems/HeadTracking` |
| 공용 안내 음성 | `SharedSystems/Audio/Narration` |
| 공용 방향광 | `SharedSystems/Lighting/DirectionalLight` |
| 화면 UI와 바닥 버튼 | `SharedSystems/UI` |
| 체험 선택 공간 | `Contents/MenuSpace` |
| 화재 사실 알리기 | `Contents/FireNotificationSpace` |
| 연기 속 자세 낮추기 | `Contents/SmokePostureSpace` |
| 대피 경로 판단 | `Contents/RouteSpace` |

`SharedSystems`의 SharedUIController, SmokePostureSpace의 SmokePostureController와 SmokePostureValidationDisplay, RouteSpace의 EvacuationRouteController는 기존 루트에 남아 있다. 컴포넌트가 없는 Logic 그룹은 추가하지 않았다.

## 공용 UI

화면 UI 기준 경로는 `SharedSystems/UI/SharedCanvas`다.

| 화면 | 하위 경로 |
|---|---|
| 체험 중 안내 | `ExperiencePanel` |
| 체험 선택 | `ContentSelectionPanel` |
| 일시정지 | `ModalPanels/PausePanel` |
| 설정 | `ModalPanels/SettingsPanel` |
| 재시작/나가기 확인 | `ModalPanels/ConfirmationDialog` |
| 체험 결과 | `ResultPanel` |

체험 선택 버튼은 ContentSelectionPanel의 FireNotificationButton, SmokePostureButton, EvacuationRouteButton이다. 결과 버튼은 ResultPanel의 RetryButton, ReturnToSelectionButton이다. 각 버튼 아래 LabelText가 표시 문구를 담당한다.

바닥 버튼은 별도 경로 `SharedSystems/UI/SharedFloorButtons/FloorCanvas/FloorViewport/MovingPanel`의 Action01Button과 Action02Button이다. 바닥 버튼을 SharedCanvas 아래로 옮기지 않는다. FloorCanvas의 좌표 계층도 유지한다.

비활성 패널은 실행 상태에 따라 켜지는 기존 규칙을 따른다. 정리 작업에서 패널을 모두 활성화하지 않았다. 패널 확인용 새 미리보기 도구는 포함하지 않는다.

## 화재 사실 알리기

아래 경로의 기준은 `Contents/FireNotificationSpace`다.

| 대상 | 하위 경로 |
|---|---|
| 체험 진행 코드 | `Logic/FireNotificationRuntime` |
| 비상벨 | `Interactables/FireAlarm` |
| 비상벨 안내 글자 | `Interactables/AlarmLabelText` |
| 첫 번째 주변 인물 | `Characters/Bystander01` |
| 두 번째 주변 인물 | `Characters/Bystander02` |
| 불 효과 | `Effects/FireVisual/FireEffect` |
| 복도 배경 | `Environment/Corridor` |
| 여섯 문 | `Environment/Corridor/Doors/Door01`부터 `Door06` |
| 문 주변 마감 | 각 Door 그룹의 `Details` |
| 천장 조명 | `Environment/Corridor/Lighting` |
| 이동 가능한 바닥 | `Navigation/TeleportSurface` |
| 비상벨 접근 지점 | `Navigation/BellApproachAnchor` |
| 체험 시작 위치 | `Anchors/EntryAnchor` |
| 선택지/다음 버튼 위치 | `Anchors/FloorButtonAnchors/FireActionsAnchor` |

기존 CorridorFinish는 Corridor의 Structure로 재사용했다. FireAlarm의 ButtonCap, PokeVolume, ButtonSurface, Housing과 BellEmitter는 기존 내부 부모와 좌표를 유지한다. 비상벨 조작을 수정할 때 이름만 보고 내부 부모를 바꾸지 않는다.

## 연기 속 자세 낮추기

아래 경로의 기준은 `Contents/SmokePostureSpace`다.

| 대상 | 하위 경로 |
|---|---|
| 연기 효과 | `Effects/SmokeLayer` |
| 인접 방 화재 효과 | `Effects/FireSource` |
| 체험 방 배경 | `Environment/SmokeRoom` |
| 열린 문과 문틀 | `Environment/SmokeRoom/Doors` |
| 인접 화재 방 배경 | `Environment/AdjacentFireRoom/Structure` |
| 조명 | `Environment/SmokeRoom/Lighting` |
| 시작 위치 | `Anchors/EntryAnchor` |
| 준비 완료 버튼 위치 | `Anchors/FloorButtonAnchors/SmokeReadyAnchor` |

기존 SmokeRoomFinish는 SmokeRoom의 Structure로 재사용했다. 열린 문의 OpenDoorHinge와 자손은 통째로 옮겼으며 hinge 내부 좌표는 유지한다.

## 대피 경로 판단

기준은 `Contents/RouteSpace`다. 기존 A와 B는 ScenarioA와 ScenarioB다. 시나리오 ID, 지점 ID와 연결은 바꾸지 않았다.

| 대상 | 하위 경로 |
|---|---|
| A 복도 | `ScenarioA/Environment/Corridor` |
| A 여섯 교실 | `ScenarioA/Environment/Corridor/Classrooms/Classroom01`부터 `Classroom06` |
| B 서쪽 복도 | `ScenarioB/Environment/WestCorridor` |
| B 북쪽 복도 | `ScenarioB/Environment/NorthCorridor` |
| B 각 복도의 두 교실 | 각 복도의 `Classrooms/Classroom01`, `Classroom02` |
| B 코너 | `ScenarioB/Environment/Corner/Structure` |
| 막힌 계단의 연기 | `ScenarioB/Effects/BlockedStairSmoke` |

각 시나리오에는 같은 상대 경로가 있다.

| 대상 | 시나리오 아래 경로 |
|---|---|
| 첫 번째 계단 | `Environment/Stairwell01` |
| 두 번째 계단 | `Environment/Stairwell02` |
| 엘리베이터 | `Environment/Elevator` |
| 일곱 이동 지점 | `Navigation/Waypoint00`부터 `Waypoint06` |
| 바닥 버튼 위치 묶음 | `Anchors/FloorButtonAnchors` |

계단 내부는 Structure, Steps, Railings, Entrance, Signage로 나눈다. 엘리베이터는 Structure, Doors, Details로 나눈다. 교실은 Structure, Doors, Windows, Details로 나눈다. 각 구역에서 실제 부품이 있는 그룹만 둔다.

Waypoint의 DestinationAnchor는 실제 도착 위치, TeleportMarker는 시각 표시다. RouteWaypoint의 id, scenarioId, targetId, adjacentIds, anchor, marker 연결은 유지한다.

ScenarioA의 RouteReadyAnchor와 각 시나리오의 Stairwell01ActionsAnchor, ElevatorActionsAnchor, Stairwell02ActionsAnchor는 `Anchors/FloorButtonAnchors` 아래에 있다.

## 이름과 보호 예외

프로젝트 관리 이름은 공백 없는 영어 PascalCase다. 버튼은 Button, 글자는 Text, 위치 기준점은 Anchor로 끝난다. 반복 부품은 같은 부모 아래에서 원래 순서대로 01, 02처럼 구분한다. 화면의 한국어 문구를 영어로 번역한 것이 아니다.

SharedCanvas와 FloorCanvas 이름은 기존 검사에서 사용하므로 유지한다. XR 리그 내부, EventSystem 내부, 캐릭터 Animator 아래 뼈와 메시, 외부 프리팹 내부, 자동 생성 TMP SubMesh 이름은 예외다. 전체 이름 치환이나 프리팹 Unpack을 하지 않는다.

## 정리 도구

Editor 메뉴 `Tools/LWS/Hierarchy`에 네 동작이 있다.

1. Capture Baseline: 저장된 편집 씬의 기준을 수집한다. 기존 기준 파일이 있으면 새 고유 run을 만들어 이전 증거를 보존한다.
2. Dry Run: Full, StructureOnly, NamesOnly 범위의 변경 명세를 만든다. 씬을 바꾸지 않는다. 이미 적용된 최종 씬에서는 NoChanges를 확인한다.
3. Apply Organization: 명세를 한 Undo 그룹으로 적용하고 비교한다. 자동 저장하지 않는다. 오류가 나면 적용분을 되돌린다.
4. Verify Organization: 현재 씬을 기준과 명세에 대조한다. 저장 후에는 씬을 다시 열고 TMP 자동 생성이 끝난 다음 검사한다.

StructureOnly와 NamesOnly는 임시 확인용이다. 검사 후 Undo하고 다음 Dry Run에서 범위를 선택한다. 부분 적용을 제품 씬에 저장하지 않는다. Full 검증이 통과한 결과만 저장한다.

Play Mode, 컴파일 중, 다른 씬, 저장하지 않은 사용자 변경, 오래된 씬/프리팹 기준에서는 적용을 거부한다. 새 사용자 변경을 정리 결과로 덮어쓰지 않는다.

기준 JSON, 변경 명세, 비교 결과와 영상은 `Artifacts/Validation/VisualEvidence/<run>/`에 보관한다. Assets와 제출 패키지에 포함하지 않는다. 도구는 Editor 전용이며 새 런타임 패키지나 Meta SDK/Operator 의존성을 추가하지 않는다.

검증은 저장 객체의 ID, 컴포넌트, 참조, 배치, 문구와 보호 대상 보존을 확인한다. TMP 임시 메시에는 저장 ID가 없어 이름에 기반한 의미로 비교한다. 같은 이름의 다른 임시 메시 개체까지 정확히 구분하는 검사는 아니며, 상세 한계는 작업별 verification.md를 따른다. Editor/합성 입력 결과는 Quest 실기 검증과 구분한다.
