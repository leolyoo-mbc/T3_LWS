# Tasks: 학교 건축 외형과 공간 구조의 사실감 개선

**Input**: `specs/008-improve-school-realism/`의 spec.md, plan.md, research.md, data-model.md, contracts/school-architecture.md, quickstart.md.

**Prerequisites**: 현재 기능 명세와 설계 문서, `.specify/memory/constitution.md` 1.3.0. 새 패키지 설치나 프로젝트 초기화는 하지 않는다.

**Organization**: 사용자 이야기별 구현과 독립 검증. 모든 작업은 미완료로 생성한다. 작업 완료와 사용자 외관 승인, 실제 Quest 검증은 서로 다르다.

## Format: `[ID] [P?] [Story] Description`

**→ 표기.** `[P]`는 파일과 실행 자원이 겹치지 않아 동시에 수행할 수 있는 작업이다. `[US1]`부터 `[US4]`는 명세의 사용자 이야기다. 같은 씬, 같은 Blender 파일, 같은 FBX 또는 bake를 수정하는 작업은 순차 수행한다.

## Path Conventions

**→ 제품 경로.** 모든 제품 경로는 저장소 루트 기준 `Assets/_02LWS/` 내부다. 씬은 `Assets/_02LWS/Scenes/FireNotification.unity`다. 실제 파일 작업에는 `E:/projects/T3_LWS/`를 붙인 절대 경로를 사용한다.

**→ 검증 경로.** T001에서 한국 시간의 실제 실행 시각으로 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism/`를 생성하고 그 절대 경로를 `$E`로 고정한다. 이 문서의 `$E/파일명`은 그 실행 폴더 안의 정확한 출력 경로다. 기존 폴더가 있으면 접미사를 추가하고 덮어쓰지 않는다. `.utmp/school-realism-evidence-path.txt`에 실제 경로를 기록한다.

**→ 범위.** 확정 작업은 문/창문 개선, 콘텐츠3의 중복 출입구와 계단 막힘 수정, 조명 개선, 전체 걸레받이 제거다. 인물, 바닥 재질/무늬 비율, 콘텐츠2의 빈방 구성은 유지하고 외부 풍경은 제작하지 않는다. 색, 유리와 계단 형상은 첫 시안이며 결과 화면 승인을 대신하지 않는다.

## Phase 1: Setup

**Purpose**: 실행 환경과 증거 저장 조건을 확인한다. 제품 씬과 모델은 아직 수정하지 않는다.

- [ ] T001 현재 Git/저장 상태, Unity6000.3.26f1/manifest, 대상 씬과 Blender5.2.3의 연결 및 기존 작업을 확인하고 `$E/preflight.json`과 `.utmp/school-realism-evidence-path.txt`를 기록한다. CLI 실패를 Editor 미실행으로 단정하지 않으며 기존 사용자 변경을 저장/폐기하거나 AGENTS.md를 수정하지 않는다.

- [ ] T002 현재 `.utmp/record-route.cs`와 검사 클래스의 진입점을 확인하고 재사용 가능한 녹화 도구를 `.utmp/record-school-realism.cs`에 준비한다. 출력은 `$E`로 제한하고1920×1080 이상/30fps 이상, Unity 내부 게임 소리만 사용, frame/camera/AudioRenderer 복원을 확인한다. 동일 입력 설정의 기존 분리 근거를 `$E/audio-isolation.md`에 연결하거나 새 무음/유음 분리 확인을 수행한다. 불가한 조건은 대체 자료 방식과 한계로 기록한다.

## Phase 2: Foundational

**Purpose**: 모든 시각 변경 이전의 기준과 보존 검사를 만든다. 이 단계 전에는 사용자 이야기의 씬/모델 수정을 시작하지 않는다.

- [ ] T003 `Assets/_02LWS/Editor/SchoolArchitectureChecks.cs`에 계약의 `LWS.FireNotification.Editor.SchoolArchitectureChecks.CaptureBaseline(string outputDirectory)`를 구현한다. `objectId/hierarchyPath`, localPose/worldBounds, colliderState, protectedReferences, materialState와 action을 읽어 baseline.json 경로를 반환하며 씬/제품 자산을 수정하지 않는다. role은 `Door, Window, Skirting, Stairwell, Elevator`, action은 `preserve, replace_visual, relocate_opening, remove_skirting`으로 제한하고 출력 경로와 대상 씬 불일치를 명확히 거부한다.

- [ ] T004 T003을 실행해 `$E/baseline.json`과 `$E/observations.json`, `$E/performance-before.json`을 확보한다. 실내 문17개/큰 창10개/걸레받이29개/엘리베이터 출입구2곳의 문짝4개와 프레임/계단실4를 현재 사실과 대조하고 원본 prefab 목록은 별도 집계한다. 앵커/waypoint/비상벨/hinge/인물/바닥 material GUID와 값/UV를 보존 기준으로 저장한다. 전후 비교의 `position/rotation/fov`, `headHeight/state/elapsedTime`, `resolution/environment`를 고정하고 기존 Scene View 자료와 실제 Play 자료를 구분한다. 성능은 동일 조건10초 준비 후30초씩3회 측정한다.

- [ ] T005 `Assets/_02LWS/Editor/SchoolArchitectureChecks.cs`에 계약의 `ValidateAll(string baselinePath, string outputDirectory)`를 구현해 checks.json 경로와 failureCount/항목별 결과를 출력한다. 누락 스크립트/새 외부 의존성, 보존 참조/Collider/인물/바닥 UV, 시각 교체 목록과 지정 걸레받이를 검사한다. `$E/checks-before.json`에 기존 구조 문제를 기록하고 의도한 수정 예외와 보호 불변을 분리한다. 자동 검사로 외관 승인 상태를 설정하지 않는다. outputDirectory의 allowed-changes.json이 없으면 개구부 이동 예외를 허용하지 않고, 등록된 객체/부품/필드만 비교 예외로 처리한다. 잘못된 허용 목록과 앵커/경로 상태/바닥 재질/인물 예외 등록은 오류로 거부하고 baseline 원본은 수정하지 않는다.

**Checkpoint**: 기준 자료가 실제 저장되고 두 Editor 진입점이 컴파일된다. 현재 알려진 시각 문제의 검사 실패는 기록하며 이를 숨기기 위해 baseline이나 조건을 완화하지 않는다.

## Phase 3: US1 - 실제 학교 같은 문과 창문 관찰 (P1, MVP)

**Goal**: 실내 문17개, 큰 창10개와 엘리베이터 출입구2곳의 문짝4개 및 프레임을 실제 학교에 맞는 시각 모델로 교체한다. 기존 열린/닫힌 상태와 외부 풍경 제외 조건을 유지한다.

**Independent Test**: 각 모델 유형의 정면/사선과 모든 배치의 접합부 및 낮은 시점을 확인한다. 기존 문/창 목록의100%를 다루며 틈/관통/중복 렌더0건, 참조와 충돌 기능 보존을 확인한다. 엘리베이터 중복 배치와 계단 구조는 US2가 해결한다.

- [ ] T006 [US1] Blender의 기존 오브젝트를 보존하고 `Assets/_02LWS/Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend`의 LWS_SchoolArchitecture 컬렉션에 `solid-door-110`을 제작한다. 문짝1.10×2.20×0.06m, 문틀 깊이와 둥근 손잡이/부착부를 분리하고 실제 개구부에 맞춘다. 재현 가능한 제작 코드는 같은 Source~의 `BuildSchoolArchitecture.py`에 남긴다.

- [ ] T007 [US1] 같은 `Assets/_02LWS/Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend`와 `BuildSchoolArchitecture.py`에 `glazed-door-088`을 추가한다. 문짝0.88×2.20×0.08m, 작은 관찰 유리와 고정부를 구분하고 기존 교실 문 용도 및 개폐 상태를 유지한다. T006 자원을 덮어쓰거나 비균일 확대해 손잡이를 찌그러뜨리지 않는다.

- [ ] T008 [US1] 같은 `Assets/_02LWS/Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend`와 `BuildSchoolArchitecture.py`에 `open-door-130`을 추가한다. 문짝1.30×2.26×0.06m를 사용하고 기존 hinge 기준 중심(0.66,1.13,0)과 열린 각도에 맞춘다. 회전된 월드 바운드를 원본 폭으로 사용하지 않는다.

- [ ] T009 [US1] 같은 `Assets/_02LWS/Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend`와 `BuildSchoolArchitecture.py`에 `sliding-window-325`를 추가한다. 기존3.25×1.72m 유리 영역을 기준으로 창틀/겹친 창짝/레일/창대를 구분하고 외부 풍경이나 새 창문은 추가하지 않는다.

- [ ] T010 [US1] `Assets/_02LWS/Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend`와 `BuildSchoolArchitecture.py`에 `elevator-portal`을 제작한다. 기존 A/B 출입구 크기를 기준으로 문짝 두께와 가운데 맞물림, 프레임 깊이, 금속 표면을 개선한다. 출입구2곳/문짝4개를 실내 문17개와 별도 대상으로 관리하고 기존 Elevator 위치, 호출 패널/층 표시, 닫힌 상태와 판단 동작은 유지한다.

- [ ] T011 [US1] 선택 mesh만 `Assets/_02LWS/Art/Models/SchoolArchitecture/SchoolArchitecture.fbx`로 내보내고 `Art/Materials/Environment/SchoolArchitecture/`의 Paint/Metal/Glass 재질 및 `Prefabs/Environment/SchoolArchitecture/SchoolDoorSolid.prefab`, `SchoolDoorGlazed.prefab`, `SchoolDoorOpen.prefab`, `SchoolWindowSliding.prefab`, `SchoolElevatorPortal.prefab`을 만든다. “바닥 중앙 기준, 정면 -Z, 미터 단위”와1unit=1m, scale1, 법선/UV2를 검증하고 camera/light/animation/ReadWrite/자동 MeshCollider import를 끈다. 유리는 첫 시안의 URP Lit 불투명 서리유리로 명시 매핑한다.

- [ ] T012 [US1] `Assets/_02LWS/Scenes/FireNotification.unity`의 기존 Door/Windows 역할 그룹에 새 모델을 적용한다. 실내 문6/1/10개와 큰 창10개, 엘리베이터 문짝4개와 프레임2곳의 시각 부품을 교체한다. 원래 기능 객체/Collider/hinge와 Elevator 루트/위치/호출 패널/층 표시/판단 연결을 보존한다. 이중 scale과 낡은 부품 중복 렌더를 없애고 XR/UI/Effects/Navigation/Anchors를 이동하지 않는다.

- [ ] T013 [US1] `Assets/_02LWS/Prefabs/Environment/Corridor.prefab`, `CorridorFinish.prefab`, `SmokeRoomFinish.prefab`, `SchoolRouteA.prefab`, `SchoolRouteB.prefab`의 대응 실내 문/창문과 엘리베이터 문짝/프레임 시각 부품을 별도 갱신한다. 기존 prefab GUID/root/비시각 구성은 유지하고 현재 씬을 옛 prefab으로 교체하거나 새 계층을 강제로 적용하지 않는다.

- [ ] T014 [US1] US1 독립 검증을 실행하고 `$E/openings-review.json`과 `$E/us1-openings.mp4` 또는 헌법상 대체 자료를 저장한다. 실내 문17개/큰 창10개와 별도로 엘리베이터 문짝4개 및 프레임2곳의 적용/접합을 전수 확인하고 각 유형의 실제 실행 정면/사선/낮은 시점 자료를 남긴다. 엘리베이터와 겹친 개구부는 US2 이관으로 명시하고 전체 구조 합격으로 표시하지 않는다.

**Checkpoint**: 문과 창문 모델 자체를 실제 화면으로 검토할 수 있다. 첫 시안의 색과 유리 표현은 사용자 미승인이다.

## Phase 4: US2 - 모순 없는 대피 경로 공간 관찰 (P1)

**Goal**: 엘리베이터 뒤 교실 문 노출과 계단 상부 막힘을 해결한다.

**Independent Test**: A/B 엘리베이터2곳의 정면과 양쪽 사선, 네 계단실의 입구/확인 지점/측면에서 중복 출입구0건과 상부 막힘0건을 확인한다. 판단 앵커와 기존 경로 상태는 유지한다.

- [ ] T015 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`와 `Prefabs/Environment/SchoolRouteA.prefab`에서 A Classroom04 개구부를 같은 벽의 여유 구간으로 이동한다. Corridor 기준 중심x13.4m→15.8m를 첫 시안으로 문/문틀/유리/손잡이/Collider를 함께 맞추고 옛 개구부는 벽과 충돌면으로 닫는다. 엘리베이터와 판단 앵커를 유지하고 인접 출입구 프레임이 겹치지 않도록 확인한다. 수정 전에 씬과 프리팹의 변경 객체 ID/부품/필드, 근거 화면과 전후 위치를 `$E/allowed-changes.json`에 등록하며 baseline은 그대로 보관한다.

- [ ] T016 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`의 B 엘리베이터와 `Prefabs/Environment/SchoolRouteB.prefab`을 독립 확인한다. 중복되는 출입구가 있으면 같은 벽의 여유 구간으로 개구부 부품과 충돌면을 함께 수정하고 없으면 위치를 유지한다. 실제 결과를 `$E/elevator-layout.json`에 A/B 각각 기록한다. B도 실제 겹침이 확인된 출입구만 수정 전에 객체/부품/필드, 근거 화면과 전후 위치를 `$E/allowed-changes.json`에 등록한다. 겹침이 없으면 이동 예외를 만들지 않는다.

- [ ] T017 [US2] `Assets/_02LWS/Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend`와 `BuildSchoolArchitecture.py`에 `stairwell-u`를 제작한다. 기존4.0×4.8m 내에 폭1.25m/단높이0.16m/단깊이0.27m의10단 두 줄, 중간1.60m와 상부3.20m 계단참, 난간, 닫힌 상층 배경 문을 연결한다. 상부를6.40m 높이에서 마감하고 입구에 새 문을 추가하지 않는다. 제작 치수를 법규 기준으로 표현하지 않는다.

- [ ] T018 [US2] 기존 mesh 이름과 재질 연결을 유지하며 `Assets/_02LWS/Art/Models/SchoolArchitecture/SchoolArchitecture.fbx`에 계단 모델을 추가 export하고 `Prefabs/Environment/SchoolArchitecture/SchoolStairwellVisual.prefab`을 만든다. T011 문/창 자원의 참조가 reimport로 끊기지 않는지 확인하고 추가 상층 배경 문은 기존 문17개 교체 목록과 구분한다.

- [ ] T019 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`와 `Prefabs/Environment/SchoolRouteA.prefab`, `SchoolRouteB.prefab`의 네 계단실에 새 시각 구조를 적용한다. 기존3.05m 천장과 뒤 벽의 막힘을 제거/분할하고 상부를 닫되 하부 입구/유도표지/연기 위치/이동 차단/판단 앵커는 유지한다. 상층에 텔레포트 면이나 새 이동 목적지를 만들지 않는다.

- [ ] T020 [US2] `$E/route-structure.json`과 `$E/us2-route-structure.mp4` 또는 헌법상 대체 자료에 US2 독립 검증을 기록한다. 두 엘리베이터와 네 계단실을 전수 관찰하고 새 메시의 복도 천장/다른 공간 관통0건을 확인한다. RouteDomainChecks.Run과 RouteIntegrationChecks.ValidateConfiguration 및 실제 직접 안전계단 선택/막힌 계단 우회에서 기존 의미와 동작을 확인한다.

**Checkpoint**: 모델이 다음 층으로 이어지는 계단실로 보이며 기존 경로 판단을 수행할 수 있다. 실제 등반 기능을 추가한 것으로 설명하지 않는다.

## Phase 5: US3 - 얼룩진 조명과 걸레받이 없는 실내 (P1)

**Goal**: 걸레받이를 전부 없애고 천장등 주변의 흰 빛 덩어리와 검은 얼룩을 개선한다.

**Independent Test**: 세 콘텐츠와 A/B의 벽 하부를 전수 확인하고 조명 전후를 같은 시점에서 비교한다. 지정 걸레받이/제거 틈0건, 주요 대상 식별 유지, 바닥 재질과 인물 보존을 확인한다.

- [ ] T021 [US3] `$E/baseline.json`의 지정 목록으로 `Assets/_02LWS/Scenes/FireNotification.unity`의 걸레받이29개와 `Prefabs/Environment/Corridor.prefab`, `CorridorFinish.prefab`, `SmokeRoomFinish.prefab`, `SchoolRouteA.prefab`, `SchoolRouteB.prefab`의 대응 부품을 제거한다. 이름 trim/wall 부분 일치로 삭제하지 않으며 문틀/창틀/난간/천장 cornice는 보존한다.

- [ ] T022 [US3] `Assets/_02LWS/Scenes/FireNotification.unity`와 T021의 대상 환경 프리팹에서 제거 후 드러난 벽/바닥 접합부를 보완한다. 다른 하부 띠를 추가하지 않고 바닥 재질/값/UV 반복, 인물과 콘텐츠2의 가구 없음 상태를 baseline과 비교해 `$E/skirting-removal.json`에 기록한다.

- [ ] T023 [US3] 현재 최종 구조의 천장등을 고정 시점에서 관찰하고 `$E/lighting-diagnosis.json`에 겹친 면/천장 관통/법선/UV2/발광면/baked광 위치/realtime광 중첩을 한 요인씩 비교한 결과를 남긴다. 대상은 `Assets/_02LWS/Scenes/FireNotification.unity`, `Lighting/EnvironmentLightingSettings.lighting`, `Art/Materials/Environment/`이며 원인을 확인하기 전에 bake 품질값이나 공용 렌더 설정을 일괄 변경하지 않는다.

- [ ] T024 [US3] T023에서 재현한 원인에 맞춰 `Assets/_02LWS/Scenes/FireNotification.unity`, 대응 환경 프리팹과 필요한 `Art/Materials/Environment/`의 등기구/발광/광원 위치와 강도/UV를 최소 수정한다. HDR off, 공용 URP, 추가광 per-object limit1과 그림자 off를 보존하고 새 런타임 조명 제어나 콘텐츠별 설정 전환은 추가하지 않는다. 수정 원인과 값을 `$E/lighting-diagnosis.json`에 연결한다.

- [ ] T025 [US3] US1/US2/걸레받이 구조가 확정된 후 `Assets/_02LWS/Lighting/EnvironmentLightingSettings.lighting`와 `Lighting/BakedEnvironment/`에 최종 정적 조명과 probe를 다시 생성하고 `Scenes/FireNotification.unity` 참조를 저장한다. 미완성 geometry의 bake를 최종 자료로 섞지 않으며 정적 변경의 최종 상태와 씬 재개방 결과를 확인한다.

- [ ] T026 [US3] `$E/us3-lighting-skirting.mp4` 또는 헌법상 대체 자료와 `$E/lighting-skirting-review.json`에 US3 독립 검증을 기록한다. 콘텐츠1/2의 같은 천장등 시점과 세 콘텐츠/A/B의 주요 대상 식별, 새 얼룩/잔여 걸레받이/접합부 틈0건을 확인한다. 바닥 재질과 인물 보존 검사를 함께 실행하고 실제 외관 승인은 T031에 남긴다.

**Checkpoint**: 네 범주의 구현 결과가 실제 검토 가능한 상태다. 조명값 변경과 파일 존재만으로 미관 개선을 합격 처리하지 않는다.

## Phase 6: US4 - 실제 화면으로 한 항목씩 결과 검토 (P2)

**Goal**: 최종 실행 회귀와 실제 화면 검토를 분리해 기록하고 사용자가 항목별로 판단할 수 있게 한다.

**Independent Test**: 세 콘텐츠 핵심 진행3개, 경로 직접 선택/우회, 중단/계속/복귀가 통과하고 네 외관 범주의 실제 자료와 실제 사용자 응답이 각각 있다.

- [ ] T027 [US4] `Assets/_02LWS/Scenes/FireNotification.unity`의 최종 Play Mode에서 콘텐츠1의 알리기/상황복원/비상벨, 콘텐츠2의 준비/안내/기존 자세 판정, 콘텐츠3의 A/B 진행을 실제 입력으로 수행하고 각각 결과/메뉴복귀를 확인한다. T002 녹화를 함께 사용해 `$E/execution-fire.mp4`, `$E/execution-smoke.mp4`, `$E/execution-route.mp4`와 `$E/journeys.json`에 실제 결과를 저장하고 캡처 목적으로 교육 문구나 판정 기준을 바꾸지 않는다.

- [ ] T028 [US4] 현재 `Assets/_02LWS/Scripts/Validation/RouteIntegrationChecks.cs`, `FloorButtonChecks.cs` 진입점을 사용해 경로의 직접 안전계단/연기 계단 우회/엘리베이터 재선택과 바닥 버튼 회귀를 실행하고 `$E/integration.txt`에 실제 최종 결과를 모은다. 세 콘텐츠의 내부 메뉴 중단/계속/메뉴복귀도 확인하고 필요한 실행 영상에 포함한다. 합성 입력과 물리 Quest 입력을 구분한다.

- [ ] T029 [US4] T004와 같은 품질/해상도/경로/촬영 부하에서10초 준비 후30초씩3회 측정해 `$E/performance-after.json`과 `$E/performance-comparison.md`를 만든다. median/p95 frame time과 batches/triangles, 새 멈춤/입력 지연을 비교하며 캡처용 고정 프레임 검사를 성능 표본으로 쓰지 않는다.72Hz/13.9ms를 확정 합격선이나 Quest 측정값으로 표현하지 않는다.

- [ ] T030 [US4] `$E/execution-fire.mp4`, `execution-smoke.mp4`, `execution-route.mp4`를 실제 열어1920×1080 이상/30fps 이상, 식별 가능한 동작과 글자, 게임 소리의 청취와 화면 동기화를 확인하고 `$E/media-verification.json`과 `$E/audio-isolation.md`에 결과를 기록한다. 프레임 복제/확대나 오디오 트랙 존재만으로 통과시키지 않는다. 제약이 있으면 최선의 영상, 원본 이미지/소리 등 대체 자료와 미검증 항목을 남긴다.

- [ ] T031 [US4] 문/창문, 경로 구조, 조명, 걸레받이 제거를 실제 전후 자료로 한 항목씩 사용자에게 보여주고 `$E/approval.md`에 `category = openings, route-structure, lighting, skirting-removal`, `state = pending, approved, changes_requested` 및 실제 응답/시각/자료를 기록한다. 자동 검사는 approved를 설정하지 않는다. 수정하면 해당 범주를 다시 pending으로 돌리고 영향받은 구조/실행/미디어 검사를 갱신한다. 답변 대기 중 이 작업을 완료로 체크하지 않는다.

- [ ] T032 [US4] 최종 `Assets/_02LWS/Editor/SchoolArchitectureChecks.cs`를 실행해 `$E/checks.json`과 `$E/acceptance-summary.md`에 SC-001~007의 근거를 집계한다. 실내 문17개/큰 창10개와 엘리베이터 문짝4개/프레임2곳 적용, 엘리베이터2/계단실4, 걸레받이0, 인물/바닥/빈방 보존, 실행 회귀와 네 외관 승인 상태를 연결한다. 사용자 미승인 범주와 Quest/수신 환경 미검증을 전체 완료로 숨기지 않는다.

**Checkpoint**: 실제 결과와 검증 한계를 항목별로 설명할 수 있다. 사용자 응답이 없으면 T031은 열린 상태로 두고 독립 문서/감사는 진행할 수 있으나 전체 외관 완료는 선언하지 않는다.

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: 원본 자원, 전달 패키지, 한계와 완료 보고를 일치시킨다.

- [ ] T033 [P] 최종 모델과 실제 사용 자원의 원본/제작 절차/참고 출처/재배포 조건을 `Assets/_02LWS/Documentation/SchoolArchitecture.md`와 `Documentation/VisualAssetSources.json`에 기록한다. 포함 자원과 참고 이미지를 구분하고 필요한 경우 `Documentation/ThirdPartyNotices.md`를 갱신한다. `.blend`/재현 스크립트는 Source~ 보관이며 런타임 Blender 의존성이 없음을 실제 반입 방식과 맞춘다.

- [ ] T034 [P] `Assets/_02LWS/Documentation/Integration.md`에 현재 unpacked 씬과 별도 원본 프리팹의 적용 정책, FBX/URP/bake 전달 조건, 외부 풍경 제외와 기존 공용 렌더 설정 보존을 기록한다. 구현과 다르게 남은 옛 설명을 필요한 범위에서 수정하고 ProjectSettings 일괄 덮어쓰기를 요구하지 않는다.

- [ ] T035 최종 `_02LWS`를 Include dependencies off와 보관용 `Scenes/ContentMenu.unity` 제외의 기존 절차로 `Artifacts/SchoolRealism/SchoolRealism-development.unitypackage`에 내보내고 `Artifacts/SchoolRealism/PackageAudit.json`에 source/meta 내용 일치와 새 FBX/재질/프리팹/bake 의존성을 검사한다. Source~/Artifacts/검증 원본 포함0을 확인하고 옛 패키지를 최신 결과로 표시하지 않는다.

- [ ] T036 준비 가능한 별도 수신 프로젝트에서 T035 패키지의 새 자원 참조/컴파일/씬 조명을 확인하고 `$E/receiver-status.md`에 실제 환경과 결과를 남긴다. 환경 준비나 실행이 불가능하면 구체적 사유와 미검증 범위를 기록한다. 지정 최종 프로젝트 수용과 실제 Quest의 입체감/편안함/성능은 독립 미검증으로 구분하며 새 APK 제작이나 실기 합격을 이 작업의 완료 조건으로 추가하지 않는다.

- [ ] T037 `specs/008-improve-school-realism/quickstart.md` 순서와 실제 결과를 대조하고 `$E/verification.md`, `$E/acceptance-summary.md`를 최종 갱신한다. `git check-ignore`/`git ls-files`로 `$E`의 제외와 비추적을 확인하고 실제 열리는 대표 MP4 또는 대체 파일 링크, 검사/승인/미검증 상태를 보고한다. 미실행 작업은 tasks.md에서 열린 채 남기고 요구 품질 체크리스트를 구현 완료 표시로 바꾸지 않는다.

## Dependencies & Execution Order

### Phase Dependencies

```text
Setup T001 → T002
  → Foundational T003 → T004 → T005
  → US1 T006 → T007 → T008 → T009 → T010 → T011 → T012 → T013 → T014
  → US2 T015 → T016 → T017 → T018 → T019 → T020
  → US3 T021 → T022 → T023 → T024 → T025 → T026
  → US4 T027 → T028 → T029 → T030 → T031 → T032
  → Polish T033 + T034 → T035 → T036 → T037
```

**→ US1.** T005 후 시작한다. T014의 전수 검사는 이미 알려진 엘리베이터 개구부 충돌을 별도 US2 실패로 표시한다. 문/창 자체 검토 가능성과 전체 구조 합격은 구분한다.

**→ US2.** 최종 모델 적용은 US1을 사용한다. 계단 원본 제작 T017은 T009 이후 먼저 진행할 수 있지만 같은 Blender/FBX 파일은 동시에 쓰지 않는다. T018은 기존 문/창 FBX의 이름과 참조를 보존해야 한다.

**→ US3.** 제거 T021과 진단 T023는 baseline 후 독립 착수가 가능하지만 같은 씬 변경과 캡처는 직렬화한다. 최종 bake T025는 T013/T019/T022/T024 모두 완료한 뒤 수행한다.

**→ US4.** 최종 실행/성능/미디어는 T026 뒤 수행한다. T031은 사용자 응답에만 의존하는 승인 작업이다. 미승인 중에도 T033~036는 외형을 바꾸지 않는 한 진행 가능하며 T032/T037의 상태 집계는 그때까지 pending을 유지한다.

**→ Polish.** T033와 T034은 T026 이후 서로 다른 문서에서 병렬 실행 가능하다. T035는 두 문서 반영과 최종 제품 저장 후 실행한다. T036는 새 패키지에 의존한다. T037은 실제 검사/승인/전달 결과를 모두 반영한다.

### Parallel Examples per Story

| 이야기 | 병렬 가능 범위 | 직렬화할 자원 |
| --- | --- | --- |
| US1 | 모델/씬 작업 중 T033의 독립 출처 초안 준비 가능. 최종 갱신은 제품 확정 뒤 수행 | T006~009의 .blend/제작 스크립트, T011 FBX와 T012 씬 |
| US2 | 기존 경로 소스 읽기와 실제 Editor를 사용하지 않는 T033 문서 정리 가능 | T015~019의 씬/.blend/FBX/SchoolRouteA/B |
| US3 | T034 통합 문서 초안은 조명 진단과 다른 파일에서 진행 가능 | T021~025의 씬/환경 프리팹/bake |
| US4 | 사용자 응답 대기 중 T033/T034과 저장된 미디어의 오프라인 검증 정리 가능 | 실제 Play Mode 검사/촬영/성능 측정은 같은 Editor에서 순차 실행 |

**→ 제한.** 위 예시는 병렬 가능성을 설명하며 새 에이전트 사용을 자동 승인하는 지시는 아니다. 동일 실행 Editor나 Blender 인스턴스를 여러 작업이 동시에 제어하지 않는다.

## Requirement Coverage

| 요구와 완료 기준 | 담당 작업 |
| --- | --- |
| FR-001~003, SC-001 | T004, T006~014, T015, T032 |
| FR-004~005, SC-002 | T015~020, T032 |
| FR-006, SC-004 조명 | T023~026, T030~032 |
| FR-007, SC-003 | T021~022, T026, T032 |
| FR-008, SC-006 | T003~005, T012, T022, T032 |
| FR-009, SC-005 | T020, T027~028, T032 |
| FR-010, SC-004 개별 승인 | T014/T020/T026 자료, T031~032 |
| FR-011, SC-007 실행 증거 | T001~002, T027~030, T037 |
| FR-012, SC-007 전달과 한계 | T033~037 |

## Implementation Strategy

### MVP First

**→ 최소 시연.** T001~014으로 문과 창문 모델 개선을 독립 시연한다. 알려진 경로 구조 문제는 남아 있음을 표시한다. 이 단계는 전체 외관 승인이나 제출 완료가 아니다.

### Incremental Delivery

**→ 순서.** 문/창문 시연 후 엘리베이터와 계단을 연결하고, 걸레받이 제거 및 조명 최종 bake를 수행한다. 완성 geometry에서 세 체험과 중단 흐름을 재검증하고 네 외관 범주를 한 장씩 검토한다. 수정한 범주는 필요한 검사와 승인만 갱신한다.

**→ 완료 규칙.** 37개 작업은 실제 수행 결과에 따라 체크한다. 파일 존재/자동 검사 통과/이전 승인만으로 사용자 외관 승인이나 기기 수용을 완료 처리하지 않는다. 구현 후 소리/화질/녹화에 제약이 생기면 헌법 예외에 따라 증거와 한계를 남기며 자료 미확보를 통과로 표시하지 않는다.
