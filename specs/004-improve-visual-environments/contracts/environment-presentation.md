# Contract: 콘텐츠 환경·연기 표시와 전달

## 플레이어 경계

기존 단일 씬 메뉴와 두 체험 id·anchor·UI·조작을 유지한다. 새 문은 비상구 조작이나 진입 목표가 아닌 화재 원인 표시다. 문 너머 접근·진화·실제 이동을 요구하지 않는다. 낮은 자세의 기존 개인 기준과 연속 2초 인식은 변경하지 않는다.

## Presenter 경계

| 호출 | 계약 |
|---|---|
| Collect() | 이전 효과 clear, seed/경과시간 초기화, 문 유입·상층·화재·불빛 시작 |
| Steady() | 유입·상층·화재 유지. 이미 실행 중이면 연출 시간을 되돌리지 않음 |
| SetPaused(true) | 정지 상태는 그대로, 실행 중이면 입자·불빛 시간 동결 |
| SetPaused(false) | 동결 이전 phase와 시간으로 복귀 |
| Stop()/OnDisable | 모든 효과 clear·불빛 off, 이후 Update로 재활성화 금지 |

SmokePostureController만 기존 stage/blocker 경로로 호출한다. FireNotification의 FireVisualPresenter 동작을 변경하지 않는다. 단계 완료·자세 판정에 새로운 콜백을 추가하지 않는다.

## 렌더 설정 경계

메뉴·두 체험은 기존 공용 URP 설정을 사용한다. 콘텐츠별 설정 복사본·전환 컴포넌트는 추가하지 않으며 진입·복귀·Play 종료로 렌더 설정을 교체하지 않는다. 공용 추가 조명 지원만 최소 조정하고 HDR·renderScale·XR·renderer·품질 선택은 유지한다. 공유 rig·Directional Light는 보존하고 메뉴·두 체험에서 영향을 검증한다. host에는 URP 17.3 기준 공용 추가 조명 조건을 별도로 안내한다.

## 제출 경계

포함: _02LWS 내부 scene, 생성 mesh/prefab, material/texture, lighting data/settings/probes, 변경된 script·meta, provenance/notices. 제외: 원본 압축 archive, 영상·검증 원본, Library와 ProjectSettings, 보관용 ContentMenu.unity. UPM 및 XR 프로젝트 설정은 기존 Integration.md의 별도 전제다.

기존 export 정책을 유지하되 새 필수 참조 누락을 검사한다. GUID·shader 참조를 보존하고 recipient에서 기존 공용 URP의 추가 조명 지원과 baked texture/data 연결을 확인한다. 외부 공용 URP 자산과 프로젝트 설정을 _02LWS 패키지 내부 자원이라고 표시하지 않는다. 실제 import를 하지 않았다면 의존성 검사만으로 import 합격이라 표시하지 않는다.

## 승인 경계

Unity 실제 실행 화면의 공간·연기 비교를 사용자가 승인해야 한다. VR 입체감·편안함·성능은 후속 검증으로 구분하며, 이번 승인으로 전체 앱·제출 승인을 선언하지 않는다.
