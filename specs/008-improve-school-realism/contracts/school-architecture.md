# Contract: 학교 건축 자원과 기존 체험 연결

**→ 적용 범위.** 외부 서비스나 런타임 공개 API는 추가하지 않는다. 이 계약은 Blender 반입 자원, 기존 씬 연결과 Editor 검증의 경계다.

## 1. 모델과 프리팹

| Asset key | 프리팹 이름 | 적용 |
| --- | --- | --- |
| solid-door-110 | SchoolDoorSolid | 콘텐츠1의 닫힌 문6개, 문짝1.10×2.20×0.06m |
| glazed-door-088 | SchoolDoorGlazed | 콘텐츠3의 교실 문10개, 문짝0.88×2.20×0.08m |
| open-door-130 | SchoolDoorOpen | 콘텐츠2의 열린 문1개, 문짝1.30×2.26×0.06m |
| sliding-window-325 | SchoolWindowSliding | 콘텐츠3의 큰 창문10개, 유리 영역3.25×1.72m |
| elevator-portal | SchoolElevatorPortal | A/B 엘리베이터 출입구2곳의 닫힌 문짝4개와 프레임, 기존 개구부 크기 기준 |
| stairwell-u | SchoolStairwellVisual | 콘텐츠3의 계단실4곳, A/B 루트 회전에 맞춰 배치 |

**→ 저장 위치.** FBX는 `Assets/_02LWS/Art/Models/SchoolArchitecture/SchoolArchitecture.fbx`, 프리팹은 `Prefabs/Environment/SchoolArchitecture`, 재질은 `Art/Materials/Environment/SchoolArchitecture`다. FBX는 선택한 모델만 포함하고 Blender의 기존 카메라/조명/기본 큐브를 포함하지 않는다.

**→ 형상 규약.** 미터, 정면 -Z, 바닥 기준 pivot, 정상 방향의 법선, 정적 모델용 UV2를 사용한다. 문틀 fit 치수는 개구부별 실측값이다. 재사용 인스턴스가 기준 크기와 다르면 크기 변형을 제작하고 손잡이까지 비균일 스케일하지 않는다. 유리/paint/metal 슬롯은 명시적으로 매핑한다.

**→ 열린 문.** 기존 OpenDoorHinge 위치와 회전은 보존한다. 잎 중심은 기존 hinge 기준 local(0.66,1.13,0), 메시 크기는1.30×2.26×0.06m다. 새 모델 pivot과 기존 Collider의 좌표 변환을 확인하고 실제 열린 각도를 유지한다.

**→ Collider.** 새 장식 mesh에 자동 Collider를 만들지 않는다. 기존 충돌 기능은 유지하며 필요한 벽과 개구부 충돌면만 형상과 맞춘다. 교실 문의 작은 유리와 손잡이에 새로운 입력 기능을 추가하지 않는다.

## 2. 씬과 원본 프리팹의 연결

**→ 현재 씬 기준 루트.** `Contents/FireNotificationSpace/Environment/Corridor`, `Contents/SmokePostureSpace/Environment/SmokeRoom`, `Contents/RouteSpace/ScenarioA|ScenarioB/Environment`의 기존 구조를 사용한다. 각 공간, Characters, Effects, Navigation, Anchors와 공용 UI/XR 루트를 교체하지 않는다.

**→ 원본 프리팹.** Corridor, CorridorFinish, SmokeRoomFinish, SchoolRouteA/B의 시각 대응 부품을 별도 수정한다. 기존 root/GUID와 비시각 구성은 유지한다. 씬의 새 계층을 과거 원본 프리팹에 강제 복사하지 않고 적용 대상 및 차이를 Documentation/SchoolArchitecture.md에 남긴다.

**→ 참조 불변.** RouteWaypoint id/scenarioId/targetId/adjacentIds, anchor/marker, controller spaces/waypoints/floorTargets/smoke, 기존 내비게이션, 비상벨과 자세 판정 설정을 보존한다. 바닥 버튼 기준점을 모델 중앙에 맞추려고 이동하지 않는다.

**→ 구조 변화 예외.** A Classroom04의 확인된 겹침을 수정하고 B는 실제 겹침이 확인된 출입구만 이동하며 없으면 배치를 유지한다. 씬과 원본 프리팹의 변경 객체/부품, 근거 화면과 전후 위치를 outputDirectory의 allowed-changes.json에 수정 전에 등록한다. ValidateAll은 변경 전 baseline을 유지한 채 이 목록에 등록된 필드만 비교 예외로 허용한다. 앵커와 경로 상태, 바닥 재질, 인물의 예외 등록은 거부한다. 엘리베이터 위치/선택 동작과 엘리베이터/계단 판단 앵커는 유지한다. 네 계단실의 새 상층은 시각 배경이며 새 이동 목적지가 아니다.

**→ 엘리베이터 시각 교체.** 문짝의 두께와 가운데 맞물림, 프레임 깊이와 금속 재질을 개선한다. 기존 Elevator 루트와 호출 패널/층 표시/판단 연결을 보존하고 닫힌 상태를 유지한다. 출입구2곳/문짝4개를 실내 문17개와 별도 검사한다. 새 문 열기나 탑승 기능은 추가하지 않는다.

**→ 보호 자원.** 인물 Renderer의 mesh/material/animator 연결, 기존 바닥 material GUID/주요값/UV tiling, 콘텐츠2의 가구 없음 상태를 보존한다. 걸레받이 삭제가 이 보호 자원의 변경 허용으로 이어지지 않는다.

## 3. Editor 검증 진입점

**→ 새 클래스.** `Assets/_02LWS/Editor/SchoolArchitectureChecks.cs`에 `LWS.FireNotification.Editor.SchoolArchitectureChecks`를 둔다. 런타임 assembly에서 참조하지 않는다.

| 메서드 | 입출력과 부작용 |
| --- | --- |
| `public static string CaptureBaseline(string outputDirectory)` | 열린 대상 씬을 읽고 baseline.json 경로를 반환한다. 제품 자산과 씬을 변경하지 않고 outputDirectory에만 기록한다. |
| `public static string ValidateAll(string baselinePath, string outputDirectory)` | baseline과 현재 씬 및 대상 프리팹을 읽고 구조/참조/유지 조건의 checks.json 경로를 반환한다. failureCount와 항목별 pass/fail을 포함한다. |

**→ 입력 조건.** 출력 경로는 프로젝트의 `Artifacts/Validation/VisualEvidence` 하위 절대 경로다. 대상 씬이 다르거나 baseline이 없으면 명확한 오류로 중단한다. allowed-changes.json이 없으면 개구부 이동 예외는 없는 것으로 처리하며 존재하지만 잘못된 내용이면 오류로 중단한다. 기본값으로 현재 씬을 저장하거나 다른 씬을 자동으로 열지 않는다. 검사에서 누락된 프리팹을 임의 생성하지 않는다.

**→ 자동 검사 경계.** 보호 참조/pose/재질, 걸레받이 잔여, 모델의 범위와 누락, prefab/scene 대응, 누락 스크립트와 새 외부 참조를 검사한다. 자동 검사만으로 UV 미관, 유리 품질, 계단실 자연스러움 또는 사용자 승인을 판정하지 않는다. 계단 상부 경로와 틈은 실제 화면 확인을 별도로 요구한다.

## 4. 증거와 제출

**→ 실행 원본.** 헌법 경로의 baseline.json, checks.json, 비교 이미지, execution.mp4, performance.json, approval.md와 verification.md를 사용한다. 미승인 범주를 자동 approved로 처리하지 않는다.

**→ 패키지 경계.** 제품 의존성이 _02LWS 안에 있어야 한다. 외부 UPM/XR/URP 설정은 기존 통합 조건으로 명시하고 ProjectSettings를 일괄 덮어쓰지 않는다. Source~, Artifacts, 보관용 ContentMenu.unity와 작업용 Python/모델 가중치는 패키지에 포함하지 않는다.
