# Implementation Plan: 체험별 실내 환경과 화재·연기 비주얼 개선

**Branch**: 실제 Git `main` 유지 | **Date**: 2026-11-13 | **Spec**: [spec.md](spec.md)

**Input**: `specs/004-improve-visual-environments/spec.md`. setup-plan의 BRANCH=004-improve-visual-environments는 기능 식별값이며 실제 Git 브랜치 생성·전환 결과가 아니다.

## Summary

기존 단일 씬의 두 공간을 유지하며 실내 표면·마감·정적 조명을 보완한다. 연기 체험에는 문 너머 화재와 지속적인 문 상단 연기 유입을 연결하고 천장 연기층의 빈틈을 줄인다. 사용자 승인 기준은 Unity 실제 실행 화면이다. 구조·조작·대사·자세 기준은 유지한다. 아래 좌표·재질·조명 수치는 구현 시작안이며 최종 미관 승인을 대신하지 않는다.

## Technical Context

**Language/Version**: 기존 C#, Unity 6000.3.26f1.

**Primary Dependencies**: URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Input System 1.20.0, uGUI 2.0.0, Pipeline 0.10.0-exp.1. 패키지 추가·업그레이드 없음.

**Storage**: 씬·프리팹·재질·텍스처·조명 데이터는 `Assets/_02LWS`, 출처는 기존 VisualAssetSources.json/ThirdPartyNotices.md, 검증 원본은 Git 비추적 Artifacts 경로. 새 사용자 저장 정보 없음.

**Testing**: 기존 Editor 동작 검사와 실제 Play Mode 회귀, 고정 관찰 조건의 전후 비교, 성능 표본, 패키지 의존성 점검. 상태 연결 변경에는 중단·재개·초기화 검사를 수행한다. 가역적인 재질 조정만을 위한 자동 테스트는 추가하지 않는다.

**Target Platform**: 기존 Quest 2 Android 단독 앱. 이번 승인은 Editor Game View로 수행하며 실제 VR 검증은 별도 후속 항목.

**Project Type**: 단일 씬의 독립 VR 교육 공간 2개.

**Performance Goals**: 같은 품질·해상도·관찰 조건으로 전후 프레임 시간·입자 수·그리기 통계 기록. 동일 장면에서 새로 재현되는 멈춤·입력 지연 0건. FPS 수치를 새 합격 기준으로 만들지 않으며 Editor 값은 Quest 성능이 아니다.

**Constraints**: 공유 XR rig·UI·Directional Light·기존 사용자 수정 보존. 실제 교육 흐름을 캡처 목적으로 바꾸지 않는다. 새 외부 자산 CC0, 오프라인 실행 및 패키지 내부 참조 유지.

**Scale/Scope**: 복도 약 5×12 m, 연기 방 약 7×7 m. 현재 방 높이와 진입 anchor 보존. 재질 2종, 간단한 건축 마감, 연기 체험용 화재 1곳. 메뉴·가구·새 교육 행동 제외.

## Constitution Check

| Gate | 설계 전 | 설계 후 이행 방법 |
|---|---|---|
| 실제 실행 MP4 | PASS | 두 체험 진입·둘러보기·핵심 동작·결과를 녹화한다. 문서 작성만인 이번 단계는 녹화 불요 |
| 녹화 불가 대체·한계 | PASS | 단계별 실행 이미지와 녹화 불가 사유·확인 불가능한 동작을 기록한다 |
| 공통 경로·Git 제외 | PASS | `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-visual-environments/`에 영상·전후 이미지·verification.md, 디코딩·ignore·비추적 확인 |
| 사실과 승인 구분 | PASS | 사용자 미관 승인과 기기 미검증을 각각 기록한다. 미승인 시 외관 완료 불가 |
| 작업·제출 경계 | PASS | 생성 자원·베이크 데이터는 _02LWS 안에 포함하고 종속성 점검. 공용 URP의 최소 설정은 수신 프로젝트 조건으로 문서화 |

정당화되지 않은 위반 없음. AGENTS.md·헌법 수정, 브랜치 생성, 구현은 이번 산출물에 포함하지 않는다.

## Project Structure

### Documentation (this feature)

```text
specs/004-improve-visual-environments/
  spec.md
  plan.md
  research.md
  data-model.md
  contracts/environment-presentation.md
  quickstart.md
  checklists/requirements.md
```

`tasks.md`는 생성된 실행 목록이며 이번 공용 설정 단순화와 함께 갱신한다.

### Source Code (repository root)

```text
Assets/_02LWS/
  Scenes/FireNotification.unity
  Art/Textures/Environment/              # CC0 2K 원본 지도와 사용 지도
  Art/Materials/Environment/             # 콘텐츠별 재질, 공유 원본 지도
  Prefabs/Environment/                   # 기존 FireEffect + 새 건축 마감
  Lighting/                             # 씬 lighting settings, bake 데이터
  Scripts/Presentation/SmokeLayerPresenter.cs
  Scripts/Runtime/SmokePostureController.cs           # 기존 호출 보존
  Documentation/VisualAssetSources.json
  Documentation/ThirdPartyNotices.md
```

**Structure Decision**: 기존 씬을 Editor API로 수정하고 기존 GUID를 유지한다. 새 콘텐츠 자원은 _02LWS 내부에 둔다. 기존 공용 URP 자산의 추가 조명 지원만 최소 조정하며 OpenXR 설정·폰트·기존 사용자 변경은 보존한다. Blender는 필요한 UV·메시 마감 작업에만 사용하며 새 외부 모델러나 패키지를 전제로 하지 않는다.

## Phase 0 — Research conclusions

[research.md](research.md)에 근거·대안을 기록했다. 로컬 씬 및 실행 설정을 읽기 전용으로 확인했고, 명세의 기능 의미를 막는 기술 미확정 사항은 없다. 다운로드·렌더링·베이크·시각 승인은 구현 검증 대상이다.

## Phase 1 — Design

### 1. 기준 확보와 환경 형상

- 첫 수정 전 씬·anchor·재질·입자 설정과 화면을 보관한다. 현재 Editor는 Edit Mode, 씬 dirty=false다. 구현 시작 시 다시 확인하고 저장되지 않은 사용자 변경을 덮어쓰지 않는다.
- 복도 로컬 바닥 중심 (0,-0.1,3), 범위 x±2.5/z=-3..9. 같은 경계에 천장 y=3.2, 뒤 마감 z=-3을 더한다. 6개 기존 문·비상벨·TeleportSurface·BellApproach와 anchor를 이동하지 않는다. 문틀 두께 약 0.06 m, 걸레받이 높이 0.10 m는 첫 시안 기본값이다.
- 연기 방은 x±3.5, z=-1.5..5.5, 천장 y=3.2를 보존한다. 현재 문은 없다. 정면 BackWall(z=5.4)의 중심에 폭 1.4 m·높이 2.3 m 개구부를 만들도록 벽을 분할하고 문틀과 열린 문짝을 배치한다. 기존 문 위치 이동은 없으며 기존 방 외곽 크기·SmokeEntry는 유지한다.
- 문 너머에 폭 3 m·깊이 3 m의 짧은 폐쇄 공간을 추가하고 끝·측면·천장을 닫는다. 불은 그 안에 배치해 문에서 일부만 보이게 한다. 플레이어 뒤의 열린 면 z=-1.5도 마감한다. 화재 구역은 이동 가능한 새 체험 경로가 아니며 문 조작을 추가하지 않는다.
- 두 공간 모두 단순 불투명 메시로 마감하고 불필요한 콜라이더는 제외한다. 새 벽 콜라이더가 기존 접근 영역을 침범하지 않도록 확인한다. 창문·가구·건물 용도 표지는 추가하지 않는다.

### 2. CC0 표면과 조명

- 첫 시안은 ambientCG PaintedPlaster017(벽·천장), Terrazzo001(바닥) 2K-JPG. 문·문틀은 기존 형상에 별도 불투명 도장 재질을 사용한다. 원본 지도는 공유하되 콘텐츠별 material은 분리한다.
- URP/Lit, BaseColor는 sRGB, normal은 NormalMap, roughness를 smoothness로 변환한다. 높이 변위·parallax·실시간 반사·새 후처리 없음. UV 반복은 실측 크기에 맞추고 벽·문 주변 근접에서 찌그러짐을 확인한다. Android 초기 텍스처 상한 1024/ASTC 6×6, 원본 2K 보존. 최종 가독성과 압축 흔적은 실행 시 확인한다.
- 고정 천장등은 baked lighting, 정적 건축 메시와 light probes를 사용한다. 발광 면만으로 주변이 밝아진다고 가정하지 않는다. 베이크 중 공간 root는 활성 상태로 두되 플레이 시작 시 기존 배타적 활성화를 유지한다. baked 데이터는 _02LWS/Lighting 아래 저장한다.
- 현재 Low가 사용하는 기존 공용 Performance URP Config의 additional lights를 PerPixel, per-object limit=1, additional shadows=off로 최소 조정한다. 메뉴·두 체험은 같은 설정을 사용하며 HDR·render scale·XR 옵션·renderer·프로젝트 품질 선택은 유지한다. 다른 품질에서 추가 조명을 이미 지원하면 그대로 두며 품질 전환 기능은 추가하지 않는다.
- 콘텐츠별 URP/renderer 복사본과 진입·종료 시 설정 교체·복원 컴포넌트는 만들지 않는다. 천장등·화재 불빛은 각 공간 root에 두고 기존 공간 활성화 규칙을 따른다. 공용 설정 변경의 메뉴·두 체험 외관·성능 영향을 함께 비교한다.
- 화재 light는 그림자 없는 Point Light 1개, 짧은 범위로 문 주변만 비춘다. 기존 공유 Directional Light와 FireEffect 원본은 바꾸지 않는다. 문 주변은 실시간 빛을 받는 Lit로 유지한다.

### 3. 화재·연기 상태 연결

- 기존 FireEffect를 새 화재 공간의 인스턴스로 사용한다. 원본 prefab은 수정하지 않는다. 새 연출의 재생·중단은 SmokeLayerPresenter가 단독 소유한다. FireVisualPresenter를 복제 연결하여 서로 다른 상태 소유자가 경쟁하게 하지 않는다.
- `rising` 참조·이름은 직렬화 호환을 위해 유지하되 문 상단 유입 emitter로 재배치한다. +z 문 너머에서 -z 방 안으로 들어오도록 속도·shape를 설정하고 천장층과 이어지게 한다.
- 현재 Steady()는 rising을 지운다. 이 동작을 제거하고 Collect()/Steady() 모두 유입과 상층 순환·화재·불빛을 유지한다. 대기/Stop에서는 모두 중단·초기화한다. 자세 단계 전환이 연출을 재시작하지 않아야 한다.
- SmokeLayerPresenter에 fireSystems[], fireLight, 깜빡임 범위·주기 설정을 추가한다. 기존 공개 메서드와 controller 참조는 보존한다. paused 상태에서는 입자와 불빛 시간도 멈추고 재개 시 이어간다. Stop/OnDisable은 잔여 입자를 지우고 불빛을 끈다. 새 Collect는 고정 seed·시간 0으로 초기화한다.
- UpperCirculatingSmoke 현재 constant size=0.8, rate=10/s, lifetime=9s, max=96, box=5×0.2×5. 크기 0.8/1.2/1.6의 세 시안을 같은 emitter·색·개수·seed·단계 시간으로 먼저 비교한다. 가장 작은 크기 중 사용자 요구를 만족하는 시안을 채택한다. 크기 증가가 벽·아래쪽을 침범하면 spawn 영역을 해당 반경만큼 안쪽으로 줄이고 비교 조건 변경을 기록한다.
- 어떤 시안도 만족하지 않으면 개수 증가 전에 위치 분포·텍스처 빈 부분을 조정하고 전후를 다시 비교한다. 특정 방식의 비용 우위를 약속하지 않는다. 기존 알파 블렌딩 particle shader 유지, volumetric fog·VFX Graph·새 full-screen 효과는 도입하지 않는다.

### 4. 검증·패키징·승인

- 계약의 상태표와 quickstart 절차로 두 체험의 정상 경로, 모든 중단·재진입, UI 기본/150%를 확인한다. 새로운 유입은 연습·완료에서도 유지돼야 한다.
- 연기 비교는 기립/낮은 시점 × yaw 0/90/180/270, 동일 FOV·해상도·seed·단계별 경과시간으로 8쌍을 기록한다. 비교용 자유 카메라·입력 주입은 Editor 검증에만 사용하고 runtime anchor·교육 판정을 변경하지 않는다.
- 성능은 같은 머신·해상도·quality·경로에서 워밍업 10초 뒤 30초씩 전후 3회 기록하고 median/p95 CPU·GPU frame time(지원 시), 입자 수·draw/batch를 남긴다. 측정 불가 항목은 unavailable로 기록한다. 촬영 부하는 동일하게 맞춘다.
- 영상은 실제 Play Mode Game View를 캡처한다. FFmpeg/녹화 도구 존재를 구현 시 확인하며 신규 패키지 설치를 전제로 하지 않는다. 영상 실패 시 단계별 대체 화면과 한계를 보존한다.
- 패키지 의존성 검사에서 UPM 외 필수 프로젝트 자원이 _02LWS 밖을 가리키지 않게 한다. baked 데이터·지도 포함 여부를 확인한다. 외부 공용 URP 자산·ProjectSettings는 패키지에 포함되지 않으므로 공용 추가 조명 조건을 Integration.md에 별도로 적는다. export/import의 실제 수행 여부와 미검증 수신 환경을 구분한다.
- 완료는 기존 동작 회귀 0건, 출처 기록·실행 증거·비교·두 외관 사용자 승인이다. 실제 Quest 검증과 전체 제출 승인은 별도다. 거절 시 승인받은 구조·교육 흐름을 유지하고 해당 외관만 재조정한다.

## Complexity Tracking

위반 없음. 코드 변경은 기존 연기 presenter와 해당 구성 검사에 한정한다. 렌더 설정을 전환하는 새 컴포넌트는 만들지 않는다. 새 교육 상태·전역 매니저·외부 API는 만들지 않는다.
