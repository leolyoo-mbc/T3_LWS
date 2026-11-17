# Implementation Plan: 학교 건축 외형과 공간 구조의 사실감 개선

**Branch**: 실제 Git `main` 유지 | **Date**: 2026-11-17 | **Spec**: [spec.md](spec.md)

**Input**: `specs/008-improve-school-realism/spec.md`. setup-plan이 반환한 BRANCH=008-improve-school-realism은 기능 식별값이다. 실제 Git 브랜치를 생성하거나 전환하지 않았다.

## Summary

Blender에서 문과 창문 공통 모델을 제작하고 현재 단일 씬과 제출에 포함되는 기존 환경 프리팹에 각각 적용한다. 경로 체험의 엘리베이터 뒤 교실 문을 분리하고 네 계단실에 되돌아가는 상부 계단과 계단참을 연결한다. 걸레받이는 제거하고 건축 변경 후 조명을 다시 굽는다. 인물, 바닥 재질, 빈방 구성과 기존 교육 및 이동 동작은 보존한다.

**→ 결정 구분.** 범위와 Blender 사용은 사용자 확정이다. 아래 모델 형태, 유리 표현, 계단 형상과 치수는 구현용 첫 시안이다. 실제 화면에서 항목별 승인을 받기 전 최종 외형으로 취급하지 않는다. 외부 풍경은 제작하지 않는다.

## Technical Context

**Language/Version**: 기존 C#, Unity 6000.3.26f1. 실행 중 Blender 5.2.3 LTS, Metric/scale_length=1, FBX exporter 사용 가능을 읽기 전용으로 확인했다.

**Primary Dependencies**: URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Input System 1.20.0, uGUI 2.0.0, Pipeline 0.10.0-exp.1. 새 패키지 설치나 업그레이드 없음.

**Storage**: 제품 자원은 `Assets/_02LWS` 내부. Blender 원본과 재현 스크립트는 `Art/Models/SchoolArchitecture/Source~`, 전달 자원은 FBX와 URP 재질, 프리팹. 검증 스냅샷은 Git 비추적 Artifacts에 보관하며 런타임 저장 데이터는 추가하지 않는다.

**Testing**: 구조와 보존 조건을 검증하는 Editor 검사, 기존 RouteDomainChecks/RouteIntegrationChecks/FloorButtonChecks, 세 콘텐츠 실제 Play Mode 회귀, 고정 시점 전후 비교, 자원 의존성 검사. 단순 색상값이나 메시 생성 코드를 그대로 반복하는 단위 테스트는 추가하지 않는다.

**Target Platform**: 기존 Quest 2 Android VR, 현재 Editor의 실제 실행 화면으로 외관 검토. 실제 Quest와 지정 제출 프로젝트의 수용 검증은 별도 상태로 보고한다.

**Project Type**: 단일 씬의 독립 VR 교육 콘텐츠 3개와 선택 메뉴. 콘텐츠 3에는 A/B 상황이 있다.

**Performance Goals**: 동일 해상도, 품질, 경로와 촬영 부하로 전후 비교한다. 10초 준비 후 30초씩 3회 측정해 median/p95 프레임 시간, batches, triangles를 기록한다. 새로 재현되는 멈춤과 입력 지연 0건. 72Hz/13.9ms는 기존 참고값이며 새 합격 기준으로 승격하지 않는다.

**Constraints**: 현재 씬의 환경은 unpacked 상태다. 기존 환경 프리팹만 수정해 씬에 반영됐다고 간주하지 않는다. XR, UI, 공용 렌더 설정, 앵커와 교육 로직은 보존한다. 런타임 공개 API, 네트워크, 새 문 열기나 층간 이동 기능은 추가하지 않는다.

**Scale/Scope**: 관찰 시점 실내 문짝 17개(콘텐츠 1: 6, 콘텐츠 2: 1, 콘텐츠 3: 10), 큰 창문 10개, 걸레받이 29개, 엘리베이터 출입구 2곳의 문짝 4개와 프레임, 계단실 4곳. 엘리베이터 문짝 4개는 실내 문 17개와 별도 개선/검사 대상으로 등록한다. 구현 직전 다시 목록을 고정하며 원본 프리팹의 대상 수는 별도 집계한다. 교실 문의 작은 유리 10개는 해당 문의 하위 부품으로 다룬다.

## Constitution Check

| Gate | 설계 전 | 설계 후 이행 조건 |
| --- | --- | --- |
| 실제 실행 영상 | PASS | 변경한 세 체험의 진입, 관찰, 핵심 동작, 결과와 복귀를 실제 Play Mode에서 기록 |
| 소리와 화질 | PASS | 1920×1080 이상, 30fps 이상, Unity 게임 소리만 입력, 실제 재생과 청취 및 동기화 확인 |
| 대체 증거 | PASS | 제약별 사유, 실제 조건, 대체 자료와 미확인 동작을 기록하며 완전 검증으로 표시하지 않음 |
| 공통 경로와 제외 | PASS | `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism/`에 원본과 verification.md, Git 비추적 확인 |
| 사용자 승인 | PASS | 문/창문, 구조, 조명, 걸레받이 제거를 한 항목씩 보여주고 승인 상태를 별도 기록 |
| 작업과 제출 경계 | PASS | 자원과 bake 데이터는 _02LWS 내부, Source~와 검증 원본은 패키지 제외, 수신 환경 의존성 명시 |

**→ 재평가.** Phase 1 설계 후에도 위반 없음. PASS는 검증 의무를 설계에 반영했다는 뜻이며 구현이나 실제 실행 검증을 완료했다는 뜻이 아니다. 이번 문서 작업에는 새 실행 녹화를 요구하지 않는다.

## Project Structure

### Documentation (this feature)

```text
specs/008-improve-school-realism/
  spec.md
  plan.md
  research.md
  data-model.md
  contracts/school-architecture.md
  quickstart.md
  checklists/requirements.md
```

**→ 후속 단계.** tasks.md는 `$speckit-tasks`에서 생성한다. 이번 호출에서는 구현하지 않는다.

### Source Code (repository root)

```text
Assets/_02LWS/
  Scenes/FireNotification.unity
  Art/Models/SchoolArchitecture/
    SchoolArchitecture.fbx
    Source~/SchoolArchitecture.blend
    Source~/BuildSchoolArchitecture.py
  Art/Materials/Environment/SchoolArchitecture/
  Prefabs/Environment/SchoolArchitecture/
  Prefabs/Environment/                     # 기존 환경 프리팹의 동일 역할 수정
  Lighting/EnvironmentLightingSettings.lighting
  Lighting/BakedEnvironment/
  Editor/SchoolArchitectureChecks.cs       # 신규, Editor 전용 보존/구조 검사
  Documentation/SchoolArchitecture.md
  Documentation/VisualAssetSources.json
  Documentation/Integration.md
```

**Structure Decision**: 현재 씬의 007 계층을 유지한다. 기존 환경 프리팹 GUID와 비시각 구성도 보존하며 양쪽에 새 공통 모델을 적용한다. 씬 루트를 옛 프리팹으로 교체하거나 씬 전체를 프리팹에 덮어쓰지 않는다. 기존 원본 프리팹의 다른 계층과 공용 조명 차이는 Integration에 명시한다.

## Phase 0: Research

[research.md](research.md)에 실제 소스와 실행 중 Editor/Blender의 조사 결과, 선택과 대안을 기록했다. 기능을 막는 기술 미확정 사항은 없다. 조명 얼룩 원인은 구현 중 통제 비교로 진단하며 특정 원인을 이미 확인한 것으로 취급하지 않는다.

## Phase 1: Design

### 1. 변경 전 기준과 참조 보호

**→ 기준 확보.** 첫 수정 전 저장 상태와 사용자 변경을 확인한다. 문/창문, 충돌체, 29개 걸레받이, 계단, 조명, 주요 직렬화 참조와 고정 시점 자료를 baseline.json에 저장한다. 실제 숫자가 달라지면 새 상태를 기준으로 목록을 재확인하며 숫자를 맞추려고 오브젝트를 삭제하지 않는다.

**→ 보호 대상.** 공간 루트, RouteWaypoint의 id/scenarioId/targetId/adjacentIds/anchor/marker, floorTargets, smoke 연결, EntryAnchor, 비상벨과 OpenDoorHinge의 pose, 공용 UI와 XR은 유지한다. 시각 부품에 Collider가 있으면 충돌 기능을 시각 메시와 분리해 유지하고 장식 프레임에 새 조작을 붙이지 않는다.

**→ 변경 예외 기록.** 변경 전 baseline.json은 그대로 보관한다. A Classroom04와 실제 겹침이 확인된 B의 출입구는 변경 대상/부품/필드, 근거 화면, 전후 위치를 같은 실행 폴더의 allowed-changes.json에 수정 전에 기록한다. 검사는 이 범위만 예외로 허용하고 앵커/경로 상태/바닥 재질/인물은 계속 보호한다. B에 겹침이 없으면 이동 예외를 만들지 않는다.

### 2. Blender 공통 모델과 유리

**→ 제작.** 기존 Cube/Camera/Light를 보존하고 LWS_SchoolArchitecture 컬렉션에 새 모델만 만든다. 기존 색 계열을 첫 시안으로 유지하고 금속 프레임, 문짝 두께, 둥근 손잡이와 부착부, 창짝 겹침과 레일을 형상으로 구분한다. 닫힌 문, 작은 관찰 유리 문, 열린 문, 미닫이 창의 역할을 분리한다. 계단 입구에 없던 문을 추가해 기존 관찰을 가리지 않는다.

**→ 크기와 축.** 기존 1.10×2.20×0.06m 닫힌 문, 0.88×2.20×0.08m 교실 문, 1.30×2.26×0.06m 열린 문, 3.25×1.72m 창 유리 영역을 기준으로 맞춤 크기 모델을 제작한다. 열린 문의 회전된 월드 바운드를 원본 폭으로 사용하지 않는다. 프레임 바깥 크기는 각 실제 개구부를 측정해 맞춘다. 손잡이를 포함한 완성 모델을 비균일하게 늘리지 않는다.

**→ 반입.** 선택한 컬렉션의 mesh만 FBX로 내보내고 단위와 축을 importer에서 고정한다. Unity 1unit=1m, 바닥 기준 pivot, 정면 local -Z, 부모 scale=1인 규약을 검증한다. 모델의 Read/Write, 카메라/조명/애니메이션 import와 자동 MeshCollider는 끈다. 고정 메시의 lightmap UV를 생성/검사하고 재질은 명시적 URP 매핑을 사용한다. 네이티브 mesh 복제는 기본으로 추가하지 않는다.

**→ 유리 첫 시안.** 외부 풍경이 없는 조건에 맞춰 유리 전용 URP Lit 불투명 서리유리 표현을 사용한다. 색칠한 청록색 판의 인상은 프레임, 겹친 창짝, 반사와 미세 표면으로 개선한다. 화면 복사, 굴절, 실시간 반사, 새 외부 배경과 다중 투명 면은 도입하지 않는다. 이 재질 외관도 사용자 승인 전에는 시안이다.

**→ 엘리베이터 모델.** 같은 Blender 원본에 elevator-portal을 추가하고 SchoolElevatorPortal.prefab으로 적용한다. 두 문짝의 두께, 중앙 맞물림과 프레임의 깊이, 금속 반사를 구현하며 기존 개구부 크기에 맞춘다. A/B 출입구2곳/문짝4개를 적용한다. 기존 Elevator 루트와 위치, 호출 패널/층 표시, 판단 앵커와 선택 동작은 보존하고 닫힌 상태를 유지한다.

### 3. 씬 적용, 엘리베이터, 계단

**→ 적용.** 기존 Door/Windows 그룹 안에 새 공통 프리팹을 배치한다. 기능 참조와 충돌체가 있는 기존 문 GameObject는 보존하고 시각 부품만 교체한다. 원래 스케일이 있는 객체 아래에 새 미터 모델을 직접 넣어 이중 스케일하지 않는다. 열린 문은 기존 hinge를 유지한다. 낡은 시각 부품과 새 부품이 중복 렌더되지 않아야 한다.

**→ 출입구 배치.** 엘리베이터와 판단 앵커는 유지하고 충돌하는 교실 문을 같은 벽의 여유 구간으로 이동한다. A의 Classroom04 문 중심은 Corridor 기준 x=13.4m에서 x=11.4m로 옮기는 후속 화면 승인 배치을 사용한다. 문, 문틀, 손잡이와 관찰 유리 및 충돌체를 함께 옮기고 옛 개구부는 벽으로 닫으며 새 개구부를 만든다. 두 출입구 프레임 사이 여유를 확인한다. B는 실제 겹침이 확인된 출입구만 같은 허용 목록 규칙으로 수정하고 겹침이 없다면 문 위치를 유지한다.

**→ 계단 시안.** 각 기존 계단실의 로컬 4.0×4.8m 범위에 첫 10단(단높이 0.16m, 단깊이 0.27m, 폭 1.25m), 높이 1.60m의 중간 계단참, 반대 방향의 10단, 높이 3.20m의 상부 계단참을 구성한다. 두 계단은 나란히 두고 중간 계단참으로 연결한다. 위쪽 계단의 공중 부유 인상을 없애도록 연속 경사 하부판과 계단참 받침을 연결하며, 이 보완안은 후속 실제 화면 검토에서 사용자 유지 승인을 받았다. 기존 계단 위 3.05m 천장은 제거/분할하고 계단실 상부를 6.40m까지 닫아 상부 계단과 계단참을 가리지 않는다. 상부 계단참에는 닫힌 상층 출입문으로 다음 층의 의미를 표현한다. 이 치수는 제작값이며 법규 적합성이나 시설 사용 교육을 주장하지 않는다.

**→ 계단 보호.** 하부 입구, 유도표지, 판단 지점과 연기 발생 위치는 유지한다. 상층은 시각 배경이며 텔레포트 앵커/면을 추가하지 않는다. 새 상부 메시가 현재 복도 천장이나 다른 공간을 관통하지 않는지 측면에서도 확인한다. 기존 하부 통행 차단과 경로 판정은 유지한다.

### 4. 걸레받이 제거와 조명 진단

**→ 제거.** ID와 경로, 실제 벽 하부 위치로 고정한 걸레받이만 제거한다. Skirting/RearSkirting/EndSkirting과 원본 프리팹의 대응 부품을 대상으로 하며 trim/wall 등의 넓은 이름 검색으로 삭제하지 않는다. 천장 cornice, 문틀, 창틀, 난간은 보존한다. 벽과 바닥 접합은 보완하되 바닥 재질, UV 반복과 인물은 변경하지 않는다.

**→ 조명.** 문제 등기구의 겹친 면과 천장 관통, 법선과 UV2, 발광면, baked 광원 위치, realtime 보조광 중첩을 같은 시점에서 한 요인씩 비교한다. 형상/UV 문제를 먼저 고치고 광원 위치와 강도를 필요한 만큼 조정한다. 지지 구조의 그림자는 남기되 검은 얼룩과 좌우의 과한 흰 덩어리를 줄인다. 기존 12texels/m, atlas1024, padding2, 32/64/32 samples는 기준값이며 원인 확인 없이 전부 올리지 않는다.

**→ 최종 bake.** 구조 변경 후 기존 LightingSettings와 BakedEnvironment 위치로 정적 조명과 probe를 다시 생성한다. 공용 URP, HDR off, 추가광 per-object limit1과 그림자 off는 유지한다. 새 runtime 조명 제어나 콘텐츠별 렌더 설정 전환은 추가하지 않는다. bake 뒤 실제 세 콘텐츠를 각각 활성화해 누락된 조명과 잔상을 확인한다.

### 5. 검증과 전달

**→ 구조 검사.** 신규 SchoolArchitectureChecks는 baseline 생성과 사후 검증만 담당한다. 보호 참조/pose/바닥 material과 UV, 인물, 중복 시각 부품, 남은 걸레받이, 누락 스크립트, _02LWS 외부 새 필수 자원 참조를 확인한다. 수치 검사만으로 자연스러운 외형을 승인하지 않는다. 계약과 기록은 [data-model.md](data-model.md), [contracts/school-architecture.md](contracts/school-architecture.md)를 따른다.

**→ 실제 실행.** [quickstart.md](quickstart.md)의 세 콘텐츠, 경로 직접 선택/우회, 중단/계속/복귀를 확인한다. 같은 입력과 촬영 방식으로 프레임 전후 표본을 비교한다. 새 도구를 만들기 전에 기존 검사 진입점과 녹화 스크립트의 현재 실행 가능 여부를 확인한다.

**→ 사용자 검토.** 문/창문, 구조, 조명, 걸레받이 제거의 네 범주를 한 번에 하나씩 보여준다. 문제가 여러 개면 범주 안에서도 한 장씩 검토한다. 미승인은 pending, 수정 요청은 changes_requested로 남기며 승인된 범주를 수정하면 다시 pending으로 바꾼다.

**→ 패키지.** 새 FBX/재질/프리팹과 bake 참조의 자급 여부를 검사하고 기존 _02LWS 내보내기 절차로 개발용 패키지의 내용 일치를 확인한다. Source~와 검증 원본은 제외한다. 별도 수신 프로젝트로 새 자원 참조와 조명을 확인할 수 있으면 기록하되 지정 최종 프로젝트와 실제 Quest 수용 검증을 대신하지 않는다. 최신 소스와 옛 패키지가 같다고 표시하지 않는다.
