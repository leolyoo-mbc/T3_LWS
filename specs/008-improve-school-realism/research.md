# Research: 학교 건축 외형과 공간 구조의 사실감 개선

**Date**: 2026-11-17 | **Spec**: [spec.md](spec.md)

**→ 조사 범위.** 로컬 소스, 현재 Unity 씬과 Blender를 읽기 전용으로 확인했다. 조사 중 제품 씬과 모델은 수정하지 않았다. 아래 결정은 구현 설계이며 최종 외관 승인을 뜻하지 않는다.

## 1. 현재 씬을 직접 갱신하고 원본 프리팹은 별도 동기화

**Decision**: 현재 씬의 계층과 기능 객체를 유지하며 시각 자식만 바꾼다. Corridor/CorridorFinish/SmokeRoomFinish/SchoolRouteA/SchoolRouteB 원본 프리팹의 같은 역할 외형도 별도 수정한다.

**Rationale**: 실시간 PrefabUtility 조회에서 Corridor, SmokeRoom, ScenarioA/B, Stairwell과 Elevator는 source path가 없는 unpacked 객체였다. 원본 프리팹만 수정해도 씬은 바뀌지 않는다. 반면 RouteAcceptance와 RouteAssetAudit은 원본 프리팹도 전달 자원으로 관리한다. Integration은 Corridor 원본과 씬 구성이 다를 수 있음을 기록한다.

**Alternatives considered**: 프리팹만 수정은 실행 씬 누락, 씬 루트 전체 교체는 007 하이라키와 직렬화 연결 손실 위험, 원본 프리팹을 임의로 폐기하는 안은 기존 전달 정책과 충돌한다.

**Sources**: `Assets/_02LWS/Documentation/Integration.md`, `Documentation/Validation/RouteAcceptance.md`, `RouteAssetAudit.md`, `specs/007-organize-scene-hierarchy/contracts/hierarchy-contract.md`, 현재 Editor PrefabUtility 조회.

## 2. 공통 FBX 모델과 단순 충돌체 분리

**Decision**: Blender의 별도 컬렉션에서 제작한 모델을 FBX로 가져오고 공통 프리팹과 명시적인 URP 재질로 사용한다. 기존 충돌과 기능 참조는 별도 보존한다.

**Rationale**: Unity6000.3.26f1/URP17.3.0와 Blender5.2.3 LTS의 Metric scale1, FBX exporter 존재를 확인했다. 기존 WallAlarm은 Source~ 원본과 FBX/URP 자원 전달의 선례다. RouteController는 문 메시 이름보다 spaces/waypoints/floorTargets/smoke 연결에 의존한다. 기존 닫힌 문과 열린 문, 경로 문과 프레임에는 Collider가 있어 시각 교체와 함께 삭제하면 안 된다.

**Alternatives considered**: .blend 직접 import는 수신 환경의 Blender 의존성, 전체 네이티브 mesh 재생성은 불필요한 중복, 장식 손잡이마다 MeshCollider를 만드는 방식은 기존 동작에 필요 없다.

**→ 승인 후 범위 보완.** 사용자가 엘리베이터 문도 다듬는 제안을 승인했다. 실내 문17개와 별도로 엘리베이터 출입구2곳의 문짝4개와 프레임을 공통 모델 제작/적용/검사에 포함한다. 패널과 판단 동작은 유지한다. A/B 모두 실제 중복 여부를 확인하되 A의 확인된 겹침을 수정하고 B는 겹침이 있는 출입구만 이동하며 근거와 변경 허용 목록을 남긴다.

**Primary documentation**: Unity 모델 importer는 단위 변환, 축 변환과 Read/Write를 별도로 제어한다. 이 계획은 1unit=1m 검증과 런타임 Read/Write 비활성화를 사용한다. [Unity 6.3 Model Import Settings](https://docs.unity3d.com/6000.3/Documentation/Manual/FBXImporter-Model.html).

**Documentation limitation**: Blender 공식 온라인 매뉴얼 FBX 페이지는 도구에서 402로 열리지 않았다. 온라인 매뉴얼을 읽었다고 주장하지 않으며 현재 실행 인스턴스와 로컬 선례를 근거로 삼았다. 실제 export/import 축 검증은 구현 시 수행한다.

## 3. 외부 풍경 없는 창문

**Decision**: 미닫이 창틀, 겹친 창짝과 레일을 모델링하고 첫 시안은 불투명 서리유리 전용 재질로 제시한다. 외부 배경, 굴절과 다중 투명 레이어는 추가하지 않는다.

**Rationale**: 사용자는 창문 자체만 개선하고 외부 풍경은 제외한다고 확정했다. 현재 단일 청록색 면과 굵은 틀의 인상을 줄이려면 형상과 표면 구분을 우선해야 한다. 이 선택은 외부 건물 제작을 유리 표현의 전제로 만들지 않는다.

**Alternatives considered**: 완전 투명 유리는 빈 배경 노출과 추가 외부 제작을 유발할 수 있다. 현재 단색 면을 그대로 두는 방식은 사실감 개선 목적을 충족하기 어렵다. 첫 시안의 색과 불투명 유리 외관은 사용자 미승인이다.

## 4. 계단실 상부 연결과 엘리베이터 출입구 분리

**Decision**: 기존 footprint 안에 U형 두 계단과 중간/상부 계단참을 구성하고 계단실 높이를 확보한다. 엘리베이터의 판단 위치를 유지하고 중복되는 교실 문을 같은 벽의 빈 구간으로 이동한다.

**Rationale**: 현재 계단실은 4.0×4.8m이며 10단의 마지막 높이가1.6m, 뒤 벽이3.0m, 천장 중심이3.05m다. 직선 계단을 연장하면 뒤 벽을 침범한다. A 교실 문 중심 world x151.4m는 엘리베이터 x151.66/152.34m 구간과 중복되며 측면 화면에서 문 유리가 뒤로 드러났다. B는 실제 교차 여부를 독립 확인해야 한다.

**Alternatives considered**: 계단 뒤 벽만 제거하면 빈 배경과 끝없는 구멍으로 보인다. 건물 전체 다음 층 제작이나 실제 층간 이동은 범위를 벗어난다. 엘리베이터와 waypoint 이동은 기존 접근성 회귀 위험이 더 크다.

**Sources**: 현재 씬의 Renderer bounds/Transform 조회와 실제 Scene View, `Scripts/Runtime/EvacuationRouteController.cs`, `RouteWaypoint.cs`.

## 5. 걸레받이29개를 좁은 목록으로 제거

**Decision**: 위치와 GlobalObjectId를 함께 기록한 걸레받이 목록을 제거한다. 원본 프리팹은 별도 목록으로 처리한다.

**Rationale**: 현재 씬에 Skirting/RearSkirting/EndSkirting 계열29개가 있다. SceneHierarchyOrganizer는 skirting과 cornice를 서로 다른 역할로 분류한다. trim/wall 검색으로 삭제하면 문틀과 천장 마감을 훼손할 수 있다.

**Alternatives considered**: 색만 바꾸거나 얇게 남기는 안은 사용자가 거절했다. 검색 이름이 비슷한 모든 객체를 삭제하는 방식은 안전하지 않다.

## 6. 조명 원인을 분리한 후 최종 구조에서 bake

**Decision**: 등기구의 면/법선/UV, 발광면, 광원 위치와 중첩을 고정 시점으로 분리 진단한다. 건축 변경 후 조명을 다시 굽되 공용 렌더 설정과 기존 방식은 유지한다.

**Rationale**: 저장 LightingSettings는12texels/m, atlas1024, padding2, AO off, direct32/indirect64/environment32 samples다. 현재 씬의 Light는 Realtime15개/Baked7개이며 세 콘텐츠 전체 합계다. 이 값만으로 얼룩 원인을 확정할 수 없다. [Unity 6.3 lightmapping quality troubleshooting](https://docs.unity3d.com/6000.3/Documentation/Manual/troubleshooting-lightmapping-quality.html)은 UV seams와 map 품질 등 서로 다른 원인을 구분한다. 단위 비교가 필요한 이유이지 특정 값 변경의 정답을 뜻하지 않는다.

**Alternatives considered**: 해상도와 샘플을 무조건 올리는 방식은 원인 미해결과 메모리 증가 위험, HDR나 새 실시간 그림자 도입은 메뉴와 다른 콘텐츠까지 영향, 콘텐츠별 렌더 전환은 기존 공용 설정 결정과 충돌한다.

**Sources**: `Lighting/EnvironmentLightingSettings.lighting`, `Lighting/BakedEnvironment`, `Assets/Settings/Project Configuration/Performance URP Config.asset`, `specs/004-improve-visual-environments/plan.md`.

## 7. 실행과 제출 검증 재사용의 한계

**Decision**: 현재 제공되는 검사를 우선 재사용하고 구조 보존 검사만 새로 추가한다. 실제 실행 녹화는 Unity 내부 AudioRenderer/MediaEncoder 방식을 우선 사용하되 현재 호출 타입과 입력 설정을 확인한다.

**Rationale**: RouteDomainChecks.Run, RouteIntegrationChecks.ValidateConfiguration/RunPlayMode, FloorButtonChecks.ValidateConfiguration/RunPlayMode가 현재 소스에 있다. `.utmp/record-route.cs`는1920×1080/30fps 카메라 렌더와 게임 오디오 캡처 예시지만 과거 실행 스크립트이므로 그대로 실행하지 않는다. `Artifacts/Validation/VisualEvidence/20261113-150016-audio-isolation/verification.md`의 기존 소리 분리 결과는 같은 방식과 설정일 때만 재사용한다. 청취와 화면/소리 동기화는 새 파일에서 확인한다.

**Alternatives considered**: 스크린샷만으로 실행 검증 완료, 오디오 트랙만으로 소리 검증 완료, 옛 패키지나 import 성공을 새 모델 전달 성공으로 재사용하는 방식은 배제한다.

**Performance**: 기존 조건은10초 준비+30초씩3회이며 새 하드웨어 합격선을 만들지 않는다. Editor 표본으로 Quest 성능을 주장하지 않는다.

**Resolved unknowns**: 제작 도구, 모델 전달 방식, 씬/프리팹 갱신 정책, 참조와 충돌 보존, 외부 배경 경계, 계단 연결 방식, 증거 및 제출 검증 방식이 정해졌다. 남은 실제 조명 원인과 미관 평가는 구현 검증 대상이다.
