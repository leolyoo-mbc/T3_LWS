# Contract: Scene hierarchy organization

이 계약은 제작자가 하이라키에서 찾는 경로와 Editor 정리 도구의 입출력을 정의한다. 런타임 공개 API, 입력 액션, 콘텐츠 ID와 데이터 형식 변경은 없다.

## 1. 공용 구조

```text
SharedSystems                              # 기존 SharedUIController 유지
├─ Player/LwsXRStarterRig                    # 리그 전체 내부 보존
├─ Navigation/ContentNavigator              # SpaceNavigation에서 이름 변경
├─ Input/LwsEventSystem
├─ ExperienceLifecycle                      # SharedLifecycle
├─ HeadTracking                             # SharedHeadTracking
├─ Audio/Narration                          # SharedNarration
├─ Lighting/DirectionalLight                # SharedDirectionalLight
└─ UI
   ├─ SharedCanvas                          # 이름과 내부 부모 관계 고정
   │  ├─ ExperiencePanel                    # ContentPanel
   │  ├─ ContentSelectionPanel              # ContentMenuPanel
   │  ├─ ModalPanels                        # SharedUIModal
   │  │  ├─ PausePanel
   │  │  ├─ SettingsPanel
   │  │  └─ ConfirmationDialog               # ConfirmDialog
   │  └─ ResultPanel
   └─ SharedFloorButtons
      └─ FloorCanvas                        # 이름과 내부 좌표 계층 고정
```

새 분류용 부모는 active=true, localPosition=0, localRotation=identity, localScale=1이다. 기존 객체의 activeSelf는 바꾸지 않는다. UI는 Canvas 바깥에만 분류 그룹을 추가한다. 기존 CanvasGroup, LayoutGroup, Mask, Raycaster와 UI 자식 순서는 유지한다.

## 2. 체험별 이동표

기존 Contents와 네 공간 루트는 유지한다. 아래 상대 경로는 해당 공간 기준이다. 목록의 객체는 자손과 함께 이동한다.

| 공간 | 기존 대상 | 목적지 |
|---|---|---|
| MenuSpace | ContentMenuRuntime | Logic/ContentSelectionInput |
| MenuSpace | MenuStart | Anchors/EntryAnchor |
| FireNotificationSpace | FireNotificationRuntime | Logic/FireNotificationRuntime |
| FireNotificationSpace | Corridor, CorridorFinish | Environment/Corridor 아래 구조 및 역할별 분류 |
| FireNotificationSpace | Corridor/FireVisual | Effects/FireVisual, 내부 FireEffect는 통째로 보존 |
| FireNotificationSpace | FireAlarm | Interactables/FireAlarm |
| FireNotificationSpace | AlarmLabel | Interactables/AlarmLabelText |
| FireNotificationSpace | Bystander_Dummy_1, Bystander_Dummy_2 | Characters/Bystander01, Characters/Bystander02 |
| FireNotificationSpace | TeleportSurface, BellApproach | Navigation/TeleportSurface, Navigation/BellApproachAnchor |
| FireNotificationSpace | ContentStart | Anchors/EntryAnchor |
| FireNotificationSpace | FloorButtonAnchors/fire-actions | Anchors/FloorButtonAnchors/FireActionsAnchor |
| FireNotificationSpace | Environment probes | Environment/LightProbes |
| SmokePostureSpace | Floor, Ceiling, LeftWall, RightWall, SmokeRoomFinish | Environment/SmokeRoom 아래 역할별 분류 |
| SmokePostureSpace | AdjacentFireSpace | Environment/AdjacentFireRoom, 화재 효과는 별도로 분류 |
| SmokePostureSpace | AdjacentFireSpace/SmokeFireSource | Effects/FireSource, 내부 구조 유지 |
| SmokePostureSpace | FixedSmokeLayer | Effects/SmokeLayer, 내부 좌표 관계 유지 |
| SmokePostureSpace | SmokeEntry | Anchors/EntryAnchor |
| SmokePostureSpace | FloorButtonAnchors/smoke-ready | Anchors/FloorButtonAnchors/SmokeReadyAnchor |
| SmokePostureSpace | Environment probes | Environment/LightProbes |
| RouteSpace | A, B | ScenarioA, ScenarioB |

SmokePostureController와 SmokePostureValidationDisplay는 SmokePostureSpace 루트에, EvacuationRouteController는 RouteSpace 루트에 유지한다. 원래 컴포넌트가 없는 빈 Logic 그룹은 만들지 않는다. FireNotificationRuntime의 모든 컴포넌트도 함께 보존한다.

AdjacentFireRoom은 배경만 포함하고 SmokeFireSource는 Effects/FireSource로 이동한다. SmokeLayer와 FireSource의 내부 부모 및 로컬 좌표는 보존한다.

각 시나리오 안에서 다음 목적지를 사용한다.

| 기존 대상 | 목적지 |
|---|---|
| A의 복도 구조와 천장/조명 부품 | Environment/Corridor의 역할 그룹 |
| B의 West wing, North wing | Environment/WestCorridor, Environment/NorthCorridor |
| 각 복도의 반복 교실 부품 | 해당 복도/Classrooms/Classroom01부터 순번 배정 |
| Stairwell 계단 1, Stairwell 계단 2 | Environment/Stairwell01, Environment/Stairwell02 |
| Elevator | Environment/Elevator |
| a-w0~a-w6, b-w0~b-w6 | Navigation/Waypoint00~Waypoint06 |
| Blocked stair smoke | Effects/BlockedStairSmoke |
| B의 Corner 접두 부품 | Environment/Corner의 Structure 그룹 |
| FloorButtonAnchors | Anchors/FloorButtonAnchors |

이동 지점 내부 Destination은 DestinationAnchor로, Neutral teleport marker는 TeleportMarker로 이름만 변경한다. RouteWaypoint.id와 연결 배열, marker 및 anchor 참조는 유지한다.

## 3. 배경 세부 분류와 순서

분류는 기존 부모별 sibling 순서를 기준으로 하며 각 대상은 정확히 한 목적지에 포함한다.

- 복도 바닥, 천장 tile/seam, 벽, cornice는 Structure, luminaire/rim/diffuser와 실제 Light는 Lighting으로 분류한다. 복도의 연속 부품을 같은 역할 그룹에 묶되 교실 부품은 분리한다.
- Route 교실 블록은 `Classroom wall pier`에서 시작해 해당 반복 구성의 두 번째 `Skirting`까지의 연속 구간이다. 다음 교실 시작이나 계단, 엘리베이터, 이동 지점과 추가 마감 부품은 포함하지 않는다. 각 블록에서 Door/Class number와 Window, Skirting 구성 개수를 확인한다. A에는 6개, B의 West/North 복도에는 각각 2개 블록이 있어야 한다.
- 교실의 wall pier/panel, window sill/top wall, window pier는 Structure, door와 그 jamb/lintel/header/handle/glass surround/observation glass는 Doors, window glass/frame/mullion은 Windows, class number 및 skirting은 Details로 분류한다.
- 블록 뒤에 따로 추가된 `Door transom backing`은 같은 복도에서 가장 가까운 Recessed door leaf의 교실 Doors에 배정한다. 동률이면 원래 교실 순번이 작은 쪽을 사용한다. 기존 블록 안의 부품은 거리로 재분류하지 않는다.
- 화재 복도의 Closed door는 원래 순서로 Doors/Door01~06에 배정한다. Door jamb/header/handle은 가장 가까운 Door의 Details에 묶는다. 문짝과 기존 Collider는 그대로 유지한다. 동률이면 문 순번이 작은 쪽이다. 바닥, 벽, 천장, 마감은 Structure, 조명은 Lighting이다.
- 연기 방과 인접 화재 방은 Structure, Doors, Lighting으로 구분한다. 열린 문은 hinge와 자손을 통째로 Doors로 옮겨 내부 좌표를 유지한다.
- 계단은 landing/wall/ceiling을 Structure, step/nosing을 Steps, handrail을 Railings, portal을 Entrance, exit sign housing/sign과 stair ID를 Signage로 분류한다.
- 엘리베이터는 recess/floor/portal을 Structure, lift door를 Doors, call panel/indicator/floor 표시/label을 Details로 분류한다. 같은 이름의 Lift floor가 바닥 메시인지 층수 텍스트인지는 컴포넌트로 구분한다.
- 분류 안의 반복 이름에는 원래 순서대로 01, 02를 붙인다. 새 그룹 순서는 Structure, Doors, Windows, Steps, Railings, Entrance, Lighting, Signage, Details 중 존재하는 항목 순서다. 구역과 주요 기능 그룹 순서는 본 계약 트리 및 표 순서다.
- 최종 이름은 공백과 구분 기호를 제거한 PascalCase로 한다. 이름에 정보가 있는 Left/Right, West/North는 보존한다. 텍스트는 Text, 버튼은 Button, 위치 기준점은 Anchor로 끝낸다. 중복 번호는 접미어 앞에 둔다: Label01Text, Waypoint00Anchor처럼 쓴다. Waypoint 루트는 실제 기준점이 아닌 기능 묶음이므로 Waypoint00을 사용한다.
- 원래 의미가 모호한 이름은 컴포넌트와 실제 텍스트/연결로 용도를 확인해 변경 명세에 기록한다. 매칭되지 않는 항목은 DryRun 오류로 남겨야 하며 Misc나 Other 그룹으로 숨기지 않는다.

## 4. UI 및 위치 기준점 이름

UI의 한국어 표시 문자열은 변경하지 않는다. 아래 이름은 GameObject 표시 이름만 뜻한다.

| 기존 이름과 문맥 | 새 이름 |
|---|---|
| Content1 / SmokeContentButton / RouteContentButton | FireNotificationButton / SmokePostureButton / EvacuationRouteButton |
| ContentMenuPanel의 Title / Disclosure / VoiceCredit / Use / ContentConfigurationError | TitleText / DisclosureText / VoiceCreditText / UsageText / ConfigurationErrorText |
| ContentPanel의 Title / Status / Narration / VoiceDisclosure | TitleText / StatusText / SubtitleText / VoiceDisclosureText |
| 모든 설정 진입 버튼 | SettingsButton |
| PausePanel의 계속하기 / 처음부터 다시 / 시나리오 선택으로 | ResumeButton / RestartButton / ReturnToSelectionButton |
| 음성 줄이기 / 음성 키우기 | VoiceVolumeDownButton / VoiceVolumeUpButton |
| 효과음 줄이기 / 효과음 키우기 | EffectsVolumeDownButton / EffectsVolumeUpButton |
| 자막 켜기/끄기 / 글자 100%/150% | ToggleSubtitlesButton / ToggleTextScaleButton |
| 표시 위치 재정렬 / 뒤로 | RecenterUIButton / BackButton |
| SettingsValues / ConfirmDialog의 Question | SettingsValuesText / QuestionText |
| ConfirmDialog의 계속하기 / 실행 | CancelButton / ConfirmButton |
| ResultPanel의 Result / 다시 체험하기 / 선택 화면으로 | ResultText / RetryButton / ReturnToSelectionButton |
| ActionButton1 / ActionButton2 / Label | Action01Button / Action02Button / LabelText |

모든 나머지 Title 텍스트는 TitleText로 통일한다. UI 장식 이름은 의미를 유지해 PascalCase로 바꾼다. FloorViewport와 이동 패널의 이름은 현재 의미를 유지하고 내부 부모 및 sibling 순서를 바꾸지 않는다.

route-ready, route-a-left/lift/right, route-b-left/lift/right 기준점은 RouteReadyAnchor, Stairwell01ActionsAnchor, ElevatorActionsAnchor, Stairwell02ActionsAnchor로 각각 바꾼다. 서로 다른 시나리오 아래의 같은 이름은 허용한다. FireAlarm cap은 ButtonCap, poke volume은 PokeVolume, surface는 ButtonSurface, housing은 Housing으로 이름만 바꾼다. BellEmitter는 유지한다.

## 5. 보호와 편집 인터페이스

- LwsXRStarterRig 전체 내부, 캐릭터 Animator 아래, Starter Assets 및 외부 원본 내부, 자동 생성 TMP SubMesh는 이름과 부모를 고정한다. 캐릭터 인스턴스 루트와 리그 루트 바깥 분류는 허용한다.
- SharedCanvas, FloorCanvas 이름을 고정한다. Canvas 사이에 부모 관계를 새로 만들지 않는다. SharedFloorButtons는 ComfortFollowUI나 체험 루트의 자손이 되어서는 안 된다.
- 프리팹은 Unpack하거나 원본에 Apply하지 않는다. 씬의 프로젝트 소유 이름 override만 허용한다. 생성한 이름과 기존 프리팹 의미가 충돌하지 않아야 한다.
- SharedUIController와 체험 adapter/controller는 기존 오브젝트에서 유지한다. 어떤 기존 컴포넌트도 삭제하거나 새 부모로 옮기지 않는다.

Editor 메뉴는 `Tools/LWS/Hierarchy` 아래 Capture Baseline, Dry Run, Apply Organization, Verify Organization 네 동작만 제공한다. 모든 동작은 현재 씬이 FireNotification이며 Edit Mode이고 컴파일 중이 아닌지 확인한다. Capture와 Apply는 dirty 상태를 거부한다. Apply는 기준 씬/프리팹 해시 불일치, 중복 또는 누락 대상과 보호 위반도 거부한다. DryRun과 Verify는 읽기 전용이므로 현재 dirty 상태에서도 실행할 수 있지만, 적용 도구가 만든 변경 이외의 차이는 오류로 보고한다. 적용 전 dirty 상태에서 실행한 DryRun 결과로 Apply를 허용하지 않는다.

Capture는 기준 자료만 저장한다. DryRun은 변경 명세와 요약을 저장하고 씬을 수정하지 않는다. Apply는 검증된 변경 명세만 한 Undo 그룹으로 적용하고 자동 저장하지 않는다. 적용 후 Verify가 통과하면 사용자가 승인한 구현 흐름에서 씬을 저장하고 재개방 검사를 수행한다. 검증 실패는 적용분을 되돌린다. Verify는 읽기 전용이며 사람이 수정한 미승인 차이는 오류로 보고한다.

작업 자료는 baseline.json, organization-plan.json, comparison.json, hierarchy-before.txt, hierarchy-after.txt로 Artifacts의 해당 run에 저장한다. 대상이 이미 최종 구조라면 DryRun은 변경 0개를 반환한다. Apply 시점에 새 사용자 변경을 발견하면 덮어쓰지 않는다.

## 6. 적용 및 검사 범위

네 메뉴의 개수는 유지한다. Dry Run에서 StructureOnly, NamesOnly, Full 중 범위를 선택하고 organization-plan.json에 저장한다. 기본값은 Full이며 Apply와 Verify는 명세에 저장된 동일 범위를 사용한다. 범위를 바꾸면 baseline에서 변경 명세를 다시 생성한다. 다른 범위의 임시 적용 결과를 이어서 적용하지 않는다.

| 범위 | 적용과 검사 |
|---|---|
| StructureOnly | 기존 이름을 유지하고 승인된 그룹과 부모 구조만 적용한다. 구역/역할 분류, 빈 그룹 및 미분류를 검사하며 최종 영어 이름과 기존 이름 중복은 검사하지 않는다. |
| NamesOnly | 기존 부모와 sibling 순서를 유지하고 이름만 바꾼다. 그룹은 만들지 않는다. 현재 부모 아래에서 원래 순서대로 번호를 붙여 명명과 중복을 검사하며 최종 구조는 검사하지 않는다. |
| Full | 최종 구조와 이름을 함께 적용한다. 최종 부모별 원래 순서로 번호를 다시 배정하며 전체 요구사항을 검사한다. |

범위 밖의 미적용 기준은 NotEvaluated로 기록한다. 참조, 배치, 화면 문구, 활성 상태, UI 레이아웃과 보호 대상 보존은 모든 범위에서 필수다. NamesOnly의 번호는 임시 번호이며 Full에 그대로 복사하지 않는다.

StructureOnly와 NamesOnly는 검사 후 Undo하고 기존 ID/부모/이름/배치 및 참조가 baseline으로 복구됐는지 확인한다. 사용자 변경을 덮어쓰지 않으며 임시 적용 결과를 제품 씬에 저장하지 않는다. 저장은 Full 검사를 통과했을 때만 허용한다.

기존 컴포넌트의 ID와 개수는 모두 유지한다. 새 그룹 하나마다 Transform 하나만 추가하므로 전체 컴포넌트 수는 기존 수 + 해당 범위의 승인된 신규 그룹 수다. NamesOnly에서는 전체 컴포넌트 수도 기존과 같다.
