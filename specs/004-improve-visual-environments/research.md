# Research: 체험별 환경·연기 개선

**Date**: 2026-11-13. 읽기 전용 조사이며 실제 자산 다운로드·성능 검증 결과가 아니다.

## 1. 현재 환경과 형상

**Decision**: 기존 단일 씬을 Editor API로 수정하고 anchors·방 외곽은 유지한다. 연기 방 정면에 새 문과 인접 화재 구역을 만들고 열린 뒤쪽을 마감한다.

**Rationale**: 실제 열린 FireNotification.unity는 dirty=false, Edit Mode이며 공간 root는 Contents 아래 중첩되어 있다. 복도 root x=40, 바닥 5×12 m, 천장·뒤쪽 벽 없음. 연기 방 root x=80, 7×7 m, 천장 y=3.2와 Back/Left/RightWall만 존재한다. 문은 아직 없다. SmokeEntry=(80,0,0), forward=+z. 문을 정면 z=5.4에 추가하면 anchor를 이동하지 않고 화재 원인을 볼 수 있다. 명세의 ‘문 위치 보존’은 새 문이 이미 존재한다는 뜻이 아니다.

**Alternatives considered**: 전체 건물 모델 교체는 기존 동선 보존에 불리하다. HDRI는 배경일 뿐 실제 천장·벽 마감을 대신하지 못한다. 가구·휴게 공간안은 사용자가 채택하지 않았다.

## 2. CC0 표면

**Decision**: 첫 시안은 PaintedPlaster017(벽/천장)과 Terrazzo001(바닥)의 2K-JPG를 사용하고 최종 사용자 승인과 구분한다. 문·마감은 기존 형상과 자체 단색 도장 재질을 사용한다.

**Rationale**: [PaintedPlaster017](https://ambientcg.com/view?id=PaintedPlaster017)은 흰색 도장 미장 재질, [Terrazzo001](https://ambientcg.com/view?id=Terrazzo001)은 실내 바닥 재질로 2K 배포를 확인했다. [ambientCG 공식 라이선스](https://docs.ambientcg.com/license/)는 다운로드 자산에 CC0를 적용하고 원본 프로젝트 포함·수정·재배포를 허용한다. 개별 다운로드 URL·해시·license 근거·변환 기록은 구현 시 확보한다. 기록 의무는 프로젝트 요구이며 CC0 attribution 의무라고 설명하지 않는다.

**Alternatives considered**: [Poly Haven ceiling_interior](https://polyhaven.com/a/ceiling_interior)는 CC0이나 낡고 벗겨진 표면이 첫 중성 시안에 부적합하다. 테이블·선반·유료 팩은 제외한다. 다운로드 실패 시 묵시적으로 다른 자산으로 바꾸지 않고 원인을 보고하고 같은 공식 자산의 다른 해상도 경로를 확인한다.

## 3. 조명 경로

**Decision**: 정적 천장등은 bake, 문 근처 흔들리는 화재빛은 그림자 없는 point 1개. URP/Lit 유지. 기존 공용 URP 설정을 사용하고 추가 조명 지원만 최소 조정한다. 사용자 요청에 따라 콘텐츠별 설정 복사본·실행 중 전환·새 전환 컴포넌트를 제외한다.

**Rationale**: live Quality=Low, currentRenderPipeline=Assets/Settings/Project Configuration/Performance URP Config.asset. 원본 additional lights=0(Disabled), additional shadows=0, HDR=0, renderScale=1. SmokeRoomWall은 URP/Lit이며 base/normal 지도는 비어 있다. Light intensity만 바꿔도 현재 파이프라인에서 point light가 표면에 비친다고 가정할 수 없다.

공용 Performance URP Config의 추가 조명은 PerPixel·per-object limit1·추가 그림자off로 조정하고 HDR·renderScale·XR·renderer·품질 선택은 보존한다. 메뉴·두 체험의 외관·성능 영향을 함께 검증하며 수신 프로젝트에는 설정 조건을 별도 안내한다. 이번 문서 수정은 실제 설정 변경이 아니다. [Android Unity 조명 가이드](https://developer.android.com/games/optimize/lighting-for-mobile-games-with-unity)는 정적 조명 baking과 동적 물체용 probes를 설명한다.

**Alternatives considered**: 콘텐츠별 URP 복사·실행 중 전환은 상태 관리가 늘어나 사용자 요청으로 폐기했다. 기존 공용 설정 최소 조정은 메뉴·두 체험에 영향을 줄 수 있으므로 함께 검증한다. Baked Lit는 실시간 화재빛 대상에 맞지 않는다([Unity shader 선택](https://docs.unity.com/en-us/engine/6000.3/manual/materials-and-shaders/built-in/shaders-in-universalrp/choose)). emissive 표면만으로 방을 비춘다고 간주하지 않는다. 다수 실시간 천장등·실시간 GI는 제외한다.

## 4. 연기와 화재 lifecycle

**Decision**: SmokeLayerPresenter가 문 유입·천장층·화재·불빛 상태를 소유한다. rising 필드와 기존 메서드는 유지하고 Steady의 유입 제거 동작을 바꾼다.

**Rationale**: 현재 Collect는 rising/layer를 재생하지만 Steady는 rising을 StopEmittingAndClear한다. 그대로 두면 연습 중 문 유입이 사라진다. SmokePostureController는 대기 Stop, 원리 Collect, 연습/완료/결과 Steady, blockers SetPaused를 이미 호출한다. 이 호출 경계를 유지하면 새 교육 상태가 필요 없다. 기존 FireVisualPresenter는 별도 사용자 일시정지 플래그를 가져 이중 소유로 연결하지 않는다.

**Alternatives considered**: 모든 효과 autoPlay는 시작·중단·재진입과 충돌한다. 공유 FireEffect 원본 변경은 알리기 체험을 바꿀 수 있으므로 독립 인스턴스의 참조를 사용한다.

## 5. 빽빽함과 비용

**Decision**: 현재 0.8에서 1.2/1.6 크기 시안을 우선 비교하고 다른 조건을 통제한다. 최종값은 사용자 비교 승인 및 측정에 따라 기록한다.

**Rationale**: 실제 UpperCirculatingSmoke는 rate=10/s, lifetime=9s, max=96, box=5×0.2×5, size mode Constant/value=0.8이다. 조회의 constantMin=0은 실제 랜덤 최솟값을 뜻하지 않는다. 사용자는 크기 증가로 빽빽해졌다고 관찰했으나 원복했다. 색만 바꾸면 공간적 빈틈이 줄어든다는 보장은 없다. [Unity 모바일 최적화](https://unity.com/how-to/mobile-game-optimization-tips-part-2)는 투명 중첩과 입자 면적·개수 비용을 설명한다. 구현 전후를 동일 조건에서 측정해야 한다.

**Alternatives considered**: 개수·수명을 무조건 늘리거나 volumetric fog를 추가하지 않는다. spawn 영역 축소만 하면 전체 덮는 범위가 줄어드는 tradeoff가 있다. 특정 방법을 더 싸다고 단정하지 않는다.

## 6. 검증과 미확정 범위

**Decision**: Editor 실제 실행 전후 자료·MP4·회귀·성능 표본·출처 검사를 제공하고 사용자 외관 승인을 받는다. Quest는 후속 상태로 남긴다.

**Rationale**: 명세 Clarifications의 사용자 선택과 헌법 1.1.0을 따른다. 현재 FFmpeg 설치는 확인되지 않았으므로 구현 시 확인하고 녹화 불가 시 대체 화면 절차를 따른다. 패키지 수신 검증은 수행 사실과 대상 버전을 기록해야 한다.

**Alternatives considered**: 정지 mockup이나 Scene View만으로 실제 실행·입력 검증을 대체하지 않는다. 사용자 승인이나 기기 검증을 AI의 시각 판단으로 갈음하지 않는다.

기술 선택 조사 완료. 남은 작업은 명세 불명확성 대신 실제 구현·시각 승인·환경별 검증이다.
