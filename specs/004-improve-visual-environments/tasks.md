# Tasks: 체험별 실내 환경과 화재·연기 비주얼 개선

**Input**: `specs/004-improve-visual-environments/`의 spec.md, plan.md, research.md, data-model.md, contracts/environment-presentation.md, quickstart.md.

**Prerequisites**: Unity 6000.3.26f1, 기존 패키지·단일 씬 유지. 첫 작업에서 실제 Editor·Git 상태를 다시 확인한다. 이 파일 생성은 구현 실행이 아니다.

**Tests**: 명세가 요구한 실행·회귀·시각·성능 검증을 작업에 포함한다. 새 자동 테스트 프로젝트나 재질 설정을 그대로 복제하는 테스트는 추가하지 않는다.

## Format and paths

`- [ ] Tnnn [P?] [USn?] 설명과 파일 경로`. [P]는 명시된 선행 작업이 끝난 뒤 파일 충돌 없이 병렬 가능한 작업이다. 모든 경로는 저장소 루트 기준이다.

`E`는 T001에서 실제 생성할 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-visual-environments/`의 이번 실행 폴더다. 이후 `${E}/파일명`은 그 실제 경로를 뜻한다. 기존 폴더를 덮어쓰지 않는다. 전후 자료는 같은 E 아래 before/after로 구분한다. 자료·원본 archive는 Git 비추적, 제품 자원은 Assets/_02LWS 내부다.

## Phase 1: Setup

**Goal**: 기존 사용자 변경과 수정 전 기준을 보존한다. T001~T004를 모두 끝내기 전 구현하지 않는다.

- [ ] T001 현재 Git 변경·Unity 버전·열린 씬·dirty 상태·quality·활성 pipeline·녹화 도구를 확인하고 E를 생성하여 `${E}/verification.md`에 기록한다. `Assets/XR/Settings/OpenXRPackageSettings.asset`, `Assets/_02LWS/UI/Fonts/DisasterKorean.asset`의 기존 사용자 변경과 미저장 씬 변경을 덮어쓰지 않는다.
- [ ] T002 `Assets/_02LWS/Scenes/FireNotification.unity`의 두 root·anchor·문·비상벨·teleport 위치와 연기 모듈·재질 참조·GUID 기준을 `${E}/before/scene-baseline.json`에 읽기 전용으로 저장하고 원본 씬 복사본을 `${E}/before/FireNotification.unity`에 보관한다.
- [ ] T003 quickstart.md의 동일 관찰 조건으로 두 체험의 실제 실행 기준 화면·진입/핵심 동작을 `${E}/before/execution.mp4`와 `${E}/before/comparison.json`에 보관한다. 영상 불가 시 단계별 PNG·이유·한계를 기록하고, 기립/낮은 자세×4방향 연기 기준 8장을 확보한다. 화면 조건 변경이 필요하면 수정 전 기준부터 다시 확보한다.
- [ ] T004 메뉴와 두 체험의 동일 해상도·품질·관찰 조건에서 워밍업 10초 후 30초씩 3회 성능 기준을 수집하여 `${E}/before/performance.json`에 CPU/GPU 시간(지원 시)·입자·draw/batch·측정 조건을 기록한다. 미측정은 0 대신 unavailable로 표시한다.

## Phase 2: Foundational

**Goal**: CC0 지도·재질과 기존 공용 추가 조명 설정을 준비한다. T005/T006/T007 완료 후 story 씬 적용을 시작한다.

- [ ] T005 [P] research.md의 ambientCG PaintedPlaster017·Terrazzo001 2K-JPG를 공식 경로에서 받아 CC0 근거·SHA256을 `${E}/source-assets.json`에 저장하고 원본 archive는 E, 실제 사용할 Color/Normal/Roughness 지도는 `Assets/_02LWS/Art/Textures/Environment/`에 import한다. 실패 시 다른 라이선스 자산으로 조용히 대체하지 않는다.
- [ ] T006 [P] 기존 공용 `Assets/Settings/Project Configuration/Performance URP Config.asset`의 추가 조명만 PerPixel·per-object limit1·추가 그림자off로 조정한다. HDR/renderScale/XR/renderer/품질 선택과 기존 사용자 변경을 보존하고 `${E}/shared-render-settings.md`에 전후값을 기록한다. 콘텐츠별 복사본·전환 컴포넌트는 만들지 않으며 다른 품질의 기존 조명 지원은 유지한다.
- [ ] T007 T005 후 `Assets/_02LWS/Art/Materials/Environment/`에 CorridorWall/CorridorFloor/CorridorCeiling/SmokeWall/SmokeFloor/SmokeCeiling/DoorPaint/Trim 재질을 만든다. “콘텐츠별 재질은 별도 인스턴스, 동일 texture 원본은 공유 가능하다”를 지킨다. URP/Lit·sRGB Color·NormalMap·roughness→smoothness, 실측 UV·Android 상한1024/ASTC6×6·원본2K를 적용하고 변환을 `${E}/source-assets.json`에 기록한다.

**Checkpoint**: 컴파일 오류 0건, 공용 추가 조명 지원 확인, CC0 지도와 콘텐츠별 재질 준비. 아직 외관 승인 아님.

## Phase 3: US1 — 완성된 실내 복도에서 화재 사실 알리기 (P1, MVP)

**Goal**: 기존 복도 구조·동선을 유지하면서 사방이 마감된 실내를 제공한다.

**Independent Test**: 알리기 체험만 실행해 기존 도착 위치별 6방향과 전체 정상 경로를 확인한다. 빈 배경/미완성 면·입력 회귀 0건.

- [ ] T008 [P] [US1] `Assets/_02LWS/Prefabs/Environment/CorridorFinish.prefab`에 복도용 천장·뒤벽·기존 6개 문용 문틀·걸레받이·천장등 형상을 만든다. 첫 기준 x±2.5/z=-3..9, 천장 y3.2/뒤벽 z-3, 문틀 두께0.06m/걸레받이 높이0.10m이며 기존 문·비상벨 접근을 침범하지 않게 한다.
- [ ] T009 [US1] T008 후 `Assets/_02LWS/Scenes/FireNotification.unity`의 FireNotificationSpace에 마감과 T007 재질을 적용한다. 기존 6개 문·anchor·TeleportSurface·BellApproach의 기준 transform과 체험 root 위치를 유지하고 표면 UV·노멀·근접 뒷면을 보완한다.
- [ ] T010 [US1] 복도 정적 건축과 천장등·light probes를 설정해 `Assets/_02LWS/Lighting/EnvironmentLightingSettings.lighting` 및 그 하위 bake 자원을 생성하고 `Assets/_02LWS/Scenes/FireNotification.unity`에 연결한다. 두 공간을 authoring 상태로 활성화하되 runtime 배타적 활성화를 유지하고 공유 Directional Light는 바꾸지 않는다.
- [ ] T011 [US1] 알리기 체험 진입·모든 기존 이동 도착 위치별 6방향에서 천장/뒤벽 누락·틈·뒷면·외부 노출 0건, 벽/문 재질·조명·접합부를 확인하여 `${E}/us1-surfaces.md`와 실행 화면을 남긴다. 실패는 T008~T010 해당 부분을 수정하고 영향 범위만 재확인한다.
- [ ] T012 [US1] 알리기 선택→외치기→다음→상황 복원→비상벨 직접 누름→완료/메뉴, 일시정지/재개·중단/재진입을 각각1회 이상 실행하고 UI 기본/150%·접근·입력 회귀0건을 `${E}/us1-regression.md`에 기록한다.
- [ ] T013 [US1] 완성된 복도 실제 Game View의 진입·둘러보기·핵심 행동·결과를 `${E}/us1-execution.mp4`에 기록하고 사용자 검토용 대표 화면을 저장한다. 녹화 불가 시 단계별 실제 PNG와 사유·한계를 같은 E에 남긴다. 승인 요청은 US4에서 기록하되 MVP만 전달할 경우 이 증거로 먼저 받는다.

**Checkpoint**: US1만으로 복도 개선 시안을 보여줄 수 있다. MVP 완료도 사용자 외관 승인 없이 확정하지 않는다.

## Phase 4: US2 — 문 너머 화재와 빽빽한 연기 관찰 (P1)

**Goal**: 문 너머 화재·불빛과 문 상단 유입·상층 연기를 단일 lifecycle로 연결한다.

**Independent Test**: US3의 최종 표면 마감 전이라도 최소 문/연결 공간에서 화재4요소·연기 비교·중단/재진입을 확인한다.

- [ ] T014 [P] [US2] `Assets/_02LWS/Scripts/Presentation/SmokeLayerPresenter.cs`에 fireSystems/fireLight·불빛 기본강도/변동폭/주기/시간을 추가한다. rising/layer·기존 메서드를 보존하고 Collect 초기화→Steady 유입유지→Frozen 입자/불빛 시간동결→복귀·Stop/OnDisable clear/off를 구현한다. “정지 중 pause 호출은 시작시키지 않는다”, “반복 Steady는 재시작하지 않음”을 준수하고 독립 FireVisualPresenter와 이중 제어하지 않는다.
- [ ] T015 [P] [US2] `Assets/_02LWS/Prefabs/Environment/SmokeFireSource.prefab`을 기존 FireEffect의 독립 변형으로 만들고 그림자 없는 Point Light 1개와 효과 참조를 구성한다. 원본 FireEffect.prefab과 알리기 인스턴스는 보존하고 자동 재생·독립 FireVisualPresenter의 상태 경쟁을 제거한다.
- [ ] T016 [US2] `Assets/_02LWS/Scenes/FireNotification.unity`의 연기 방 외곽7×7m·천장y3.2·SmokeEntry를 유지하며 BackWall z5.4 중앙에 폭1.4m/높이2.3m 문 개구부와 최소 문틀·열린 문짝을 만들고 문 너머 폭3m/깊이3m 폐쇄 공간을 구성한다. 현재 문이 없다는 기준을 지키고 새 이동·문 조작을 추가하지 않는다.
- [ ] T017 [US2] T014~T016 후 `Assets/_02LWS/Scenes/FireNotification.unity`에서 문 너머 화재를 연결하고 기존 InitialRisingSmoke를 문 위쪽으로 재배치해 +z→-z 유입과 천장 확산을 잇는다. UpperCirculatingSmoke의 방 안 층 높이를 유지하고 fireSystems/light를 SmokeLayerPresenter에 할당한다.
- [ ] T018 [US2] `Assets/_02LWS/Scripts/Runtime/SmokePostureController.cs`의 시작 구성검사에 새 필수 화재/조명 참조를 포함한다. “mandatory 참조 누락 시 기존 구성 오류 흐름으로 진입을 차단하고 메뉴 복귀를 허용한다”를 지키며 stage 호출·대사·개인 자세 기준·연속2초 판정은 바꾸지 않는다.
- [ ] T019 [US2] 현재 기준과 0.8/1.2/1.6 크기 시안을 같은 seed7100+i·rate10/s·lifetime9s·max96·색·shape 조건으로 비교하고 `${E}/us2-smoke-candidates.json`에 값·화면을 남긴다. Practice+3초에서 높이1.65/1.15m×yaw0/90/180/270의8장씩 확보하고 Collect+2/+5초도 기록한다. 이는 관찰용 값이며 판정 기준이 아니다. 벽/아래 침범 시 분포 보정과 비교 조건 변경을 명시한다.
- [ ] T020 [US2] T019에서 큰 빈틈·층 범위·UI 가독성을 비교해 사용자 검토용 시안을 `Assets/_02LWS/Scenes/FireNotification.unity`에 적용한다. 부족하면 개수 증가 전 위치 분포/텍스처 여백을 재검토하고 `${E}/us2-smoke-candidates.json`에 근거를 남긴다. 승인 전 수치를 최종값이라 부르거나 특정 방식이 저비용이라고 단정하지 않는다.
- [ ] T021 [US2] 실제 실행에서 대기off·Collect시작·Steady유입지속·일시정지/다중blocker·추적복구·Stop/재진입·문 너머 불빛 반응을 검사하여 `${E}/us2-lifecycle.md`에 계약 상태표 결과를 기록한다. 참조 누락 경로는 임시 참조를 복구하고 확인하며 잘못된 완료·입자 잔류0건이어야 한다.
- [ ] T022 [US2] 원리→연습→낮은 자세2초→완료/결과→메뉴와 중단/재진입을 실제 실행해 `${E}/us2-execution.mp4` 또는 이유를 적은 단계별 PNG를 남긴다. 낮은 위치 안전지대·층 하강·불이 방 안에서 발생하는 인상·필수 안내 차단이 없는지 기록한다.

## Phase 5: US3 — 상자 같은 공간 인상 개선 (P2)

**Goal**: 연기 방의 기존 형태를 유지하면서 표면·건축 마감·빛으로 실제 실내 인상을 만든다.

**Independent Test**: T016의 최소 문/연결 공간을 기반으로 연기off와 연기on에서 사방·상하·비스듬한 문 관찰을 수행한다. US2의 최종 밀도·자세 완료 여부와 별개로 평가할 수 있다.

- [ ] T023 [P] [US3] `Assets/_02LWS/Prefabs/Environment/SmokeRoomFinish.prefab`에 기존 방용 걸레받이·문 접합부·천장등·뒤쪽 z-1.5 마감을 만든다. 문 너머 접속 기준은 T016을 사용하고 창문·가구·새 건물용도 표지는 넣지 않는다.
- [ ] T024 [US3] T016/T023 후 `Assets/_02LWS/Scenes/FireNotification.unity`의 연기 방·연결 공간에 마감과 SmokeWall/SmokeFloor/SmokeCeiling·DoorPaint/Trim을 적용한다. 외곽·anchor를 보존하며 측면 문틀 두께·벽틈·재질 UV·콜라이더 침범을 확인한다.
- [ ] T025 [US3] 연기 방과 문 너머 구역의 정적 천장등·probes·메시를 설정하고 두 공간을 함께 재베이크하여 `Assets/_02LWS/Lighting/EnvironmentLightingSettings.lighting`과 bake 자원을 갱신한다. 알리기 공간의 기존 승인 외관·동작을 훼손하지 않고 문 주변은 실시간 화재빛을 받는 Lit로 유지한다.
- [ ] T026 [US3] 연기off/on에서 사방·상하·비스듬한 문을 비교하고 `${E}/us3-room-comparison.md`와 대표 실행 PNG를 저장한다. 문 너머 바닥·벽·끝마감, 뒤벽과 천장, 접합부, 상자 같은 인상 개선 여부를 사용자에게 판단 가능한 화면으로 제시한다.
- [ ] T027 [US3] 마감 후 화재4요소·연기 빈틈/벽 침범·UI기본/150%·기존 낮은 자세 흐름을 재확인하고 `${E}/us3-integration.md`에 결과를 기록한다. US2 비교 이후 베이크/재질/형상이 바뀌었으면 영향받는8방향 비교와 대표 실행 자료를 최종 상태로 갱신한다.

## Phase 6: US4 — 실제 화면으로 개선 결과 판단 (P2)

**Goal**: 검증 조건·사용자 승인·미검증 한계를 추적할 수 있는 실행 증거를 전달한다.

**Independent Test**: 확보한 자료만 열어 전후 차이·입력 회귀·성능 측정 여부·승인과 VR 미검증을 각각 확인할 수 있어야 한다.

- [ ] T028 [P] [US4] 확보된 동작 자료를 확인하며 `${E}/evidence-index.md`에 전후 화면·시안별 값·카메라조건·seed·단계시각·회귀 결과·영상 링크를 묶는다. “없음과 미측정을 0이나 성공으로 기록하지 않는다”를 지키고 final 자료 누락은 pending으로 남긴다. T002~T004 후 시작 가능하며 최종화는 T027 이후다.
- [ ] T029 [US4] 최종 씬에서 T004와 동일한10초워밍업/30초×3회 표본을 수집하여 `${E}/after/performance.json`과 `${E}/performance-comparison.md`에 전후 median/p95·측정불가·촬영부하를 기록한다. 재현되는 새 멈춤/입력지연은 미해결로 남기고 조정 시 영향받는 외관 비교도 갱신한다.
- [ ] T030 [US4] 두 체험 진입·둘러보기·핵심 행동·결과를 최종 실제 Game View로 `${E}/execution.mp4`에 기록한다. 영상 불가 시 최종 단계별 PNG와 이유·동작 확인 한계를 `${E}/verification.md`에 기록하고, 영상 존재만으로 회귀통과를 선언하지 않는다.
- [ ] T031 [US4] `${E}/execution.mp4` 또는 대체 이미지의 재생/열람과 실제 프레임 진행을 확인하고 git check-ignore/git ls-files로 E 전체의 제외·비추적을 검사해 `${E}/verification.md`에 기록한다. 원본을 Assets나 패키지에 섞지 않고 T028 인덱스를 최종 갱신한다.
- [ ] T032 [US4] 복도 실행 화면·연기 방 전후·빽빽함 시안을 사용자에게 보여주고 `${E}/approval.md`에 콘텐츠별 pending/approved/rejected와 답변·대표증거를 기록한다. “approved는 사용자 응답이 있을 때만 기록하며 재수정 시 해당 외관 승인은 pending으로 돌린다”를 지킨다. 답변 대기 중에는 독립된 출처·의존성 작업을 계속한다.
- [ ] T033 [US4] 거절된 외관이 있으면 해당 콘텐츠의 재질/마감/연기만 재조정하여 `Assets/_02LWS/Scenes/FireNotification.unity`와 관련 자원을 갱신하고 영향받는 회귀·성능·시각자료를 재확인한 뒤 `${E}/approval.md`를 갱신한다. 수정 불필요 시 두 콘텐츠 approved 근거를 확인해 완료한다. VR입체감·편안함·성능은 별도 후속 미검증으로 남긴다.

## Phase 7: Polish & Cross-Cutting Concerns

**Goal**: 출처·제출 경계·공용 설정 영향·완료 근거를 정리한다.

- [ ] T034 `${E}/source-assets.json`을 근거로 `Assets/_02LWS/Documentation/VisualAssetSources.json`의 environment 항목과 `Assets/_02LWS/Documentation/ThirdPartyNotices.md`를 갱신한다. assetId/sourceUrl/downloadUrl/license=CC0-1.0/licenseUrl/retrievedAt/sourceFile/SHA256/importedFiles/transformations를 기록하며 “기존 current/historical 항목은 보존한다”. 다운로드 지도100%의 개별 근거·사용파일을 대조한다. T005/T007 후 US작업과 독립 실행 가능하다.
- [ ] T035 `Assets/_02LWS/Documentation/Integration.md`에 공용 URP의 추가 조명 조건·bake·기존 연출 스크립트 변경 전달 조건을 추가하고 _02LWS 밖 필수 프로젝트 자원 참조를 검사하여 `${E}/package-dependencies.json`에 기록한다. UPM과 ProjectSettings는 별도 전제, archive/검증원본/보관용 ContentMenu.unity는 제외한다. 실제 export/import 수행 여부와 수신 환경 미검증을 구분하며 의존성검사만으로 import합격을 표시하지 않는다.
- [ ] T036 최종 `Assets/_02LWS/Scenes/FireNotification.unity`에서 메뉴→알리기→메뉴→연기→메뉴→알리기·Play종료를 수행하여 외관 잔류·실행 중 공용 렌더 설정 교체·참조누락·새 Console오류0건을 `${E}/final-regression.md`에 기록한다. 최초 메뉴부터 같은 공용 설정을 쓰는지와 메뉴·두 체험의 외관·성능 영향을 확인하고 기준anchor·대사·2초판정·공유rig/UI·기존 사용자 변경도 보존한다.
- [ ] T037 `${E}/verification.md`에 SC-001~007별 실제 결과·증거·사용자승인·미검증기기/수신프로젝트를 집계하고 `specs/004-improve-visual-environments/tasks.md`의 실제 완료 항목만 체크한다. 구현 검증이 통과한 최종 MP4/대체 대표파일을 직접 링크해 보고하고, 미승인/미해결을 남긴 채 전체완료·VR검증완료·제출승인이라 선언하지 않는다.

## Dependencies & Execution Order

```text
T001 → T002 → T003 → T004
  → (T005 ∥ T006) → T007
  → US1 T008..T013 → MVP 검토 가능
  → US2 (T014 ∥ T015), T016 → T017 → T018 → T019 → T020 → T021 → T022
  → US3 T023 → T024 → T025 → T026 → T027
  → US4 T028 → T029 → T030 → T031 → T032 → T033
  → T034 → T035 → T036 → T037
```

그래프는 안전한 기본 순차 실행이다. 개별 작업에 명시한 조기 병렬 실행은 아래와 같다.

- Foundation은 모든 story의 선행 조건이다. US1/US2는 이후 서로의 완성을 요구하지 않는다. **하나의 열린 Editor와 같은 FireNotification.unity를 수정하는 작업은 항상 직렬화**한다. prefab/code 준비만 겹칠 수 있다.
- T008과 T014/T015은 foundation 후 다른 파일이므로 병렬 준비 가능하다. T016은 US2용 최소 문/화재 공간이며 US3는 T016 후 시작 가능하다. US3가 US2 완성을 기다릴 필요는 없지만 최종 시각·성능 자료는 두 작업을 통합한 상태여야 한다.
- T023는 T016 후 다른 prefab을 작성하므로 T018의 controller 작업과 병렬 가능하다. T025의 scene/bake 변경은 US2 scene 수정·촬영과 겹치지 않는다.
- T028은 before 자료 이후 순수 기록만 먼저 준비 가능하다. T034 출처 정리는 T005/T007 이후 앞당길 수 있다. 최종 T035~T037는 실제 최종 자원·승인 상태를 반영해야 한다.
- T013/T022 등 중간 증거는 US4까지 미루지 않는다. 최종 변경으로 영향받은 자료만 갱신한다.

## Parallel Examples per Story

| Story | 선행 조건과 안전한 예시 |
|---|---|
| US1 | Foundation 후 T008 복도 prefab과 T014 연기 presenter를 병렬 작성. T009/씬 적용은 단일 Editor에서 순차 |
| US2 | Foundation 후 T014 코드와 T015 독립 화재 prefab 병렬 준비. T017 통합 전 둘 다 완료 |
| US3 | T016 후 T023 실내마감 prefab과 T018 구성검사 코드 병렬 준비. T024/T025은 scene lock |
| US4 | T003/T004 후 T028 인덱스 초안과 T034 출처 문서 정리 가능. T029~T033의 실제 실행/캡처/승인은 직렬화 |

[P]가 없는 작업도 선행 조건과 파일/Editor 충돌을 명시적으로 확인한 위 예시에서만 앞당긴다. 병렬 도구 호출이 아닌 독립 파일 준비를 뜻하며 동일 열린 씬 동시 편집은 허용하지 않는다.

## Implementation Strategy

**MVP first**: Setup+Foundation+US1(T001~T013)로 복도 시안을 완성하고 실제 실행 자료를 보여준다. MVP만 전달하면 T032의 해당 콘텐츠 승인 기록과 T031 증거 검증, T034~T037 중 해당 범위를 수행한다. MVP는 전체 두 콘텐츠 완료가 아니다.

**Incremental delivery**: US1 → US2 최소 문/화재·연기 → US3 방 마감 → US4 최종 비교/승인 → 최종 출처/의존성/회귀 집계. 사용자 승인 대기 동안 독립 문서·의존성 작업은 계속하고, 승인 없는 외관은 pending으로 남긴다.

**Scope guard**: 모든 좌표·재질·크기 시안은 plan.md의 구현 시작안이다. 사용자 확정 구조·교육 흐름을 바꾸지 않는다. 새 패키지·자동테스트 프레임워크·전역관리자·가구·새 문 조작·VR 합격 수치·콘텐츠별 렌더 설정 전환을 추가하지 않는다. 실제 Quest 검증은 별도 후속이며 이번 외관 작업의 필수 완료 조건이 아니다.

## Requirement Coverage

| 요구 | 작업 |
|---|---|
| FR-001 / 콘텐츠별 범위 | T002, T008~T027, T036 |
| FR-002~004 / 복도·마감·조명 | T007~T013 |
| FR-005 / 연기방 인상 | T016, T023~T027, T032~T033 |
| FR-006~007 / 문너머 화재·유입 | T014~T018, T021~T022 |
| FR-008~010 / 빽빽함·비교·한계 | T019~T022, T027, T032~T033 |
| FR-011 / 기존 행동·중단 | T012, T018, T021~T022, T027, T036 |
| FR-012~013 / CC0·전달 | T005, T007, T034~T035 |
| FR-014 / 시각 증거 | T003, T013, T022, T026, T028, T030~T031, T037 |
| FR-015 / 성능 | T004, T029, T033 |
| FR-016 / 화면승인·기기분리 | T032~T033, T037 |
| FR-017 / 공용 렌더 설정·단순화 | T006, T036 |

**Completion rule**: 작업 ID 37개는 모두 실제 결과 확인 후 체크한다. 검사/승인 작업은 파일이 존재한다는 이유만으로 완료하지 않는다. 확장 훅 파일이 없다면 전후 훅은 실행 대상 없음으로 기록한다.
