# Data Model: 환경 시안과 연출 상태

새 DB·사용자 영구 저장은 없다. 기존 scene/prefab 직렬화와 문서 기록만 확장한다.

## EnvironmentVariant

| 필드 | 의미·규칙 |
|---|---|
| contentId | 기존 fire-notification / smoke-low-posture 중 하나 |
| root / entryAnchor | 기존 root·anchor 참조. root 하나만 실행 활성 |
| surfaces / trim / ceilingLights | 해당 콘텐츠 소유 메시·재질·정적 조명 |
| lightingData | _02LWS 내부 bake 데이터. 기존 공용 URP 설정은 통합 조건으로 별도 안내 |
| approval | pending / approved / rejected, 사용자 답변과 증거 위치 기록 |

씬 공간 식별자는 기존 id를 그대로 사용한다. approved는 사용자 응답이 있을 때만 기록하며 재수정 시 해당 외관 승인은 pending으로 돌린다. 콘텐츠별 재질은 별도 인스턴스, 동일 texture 원본은 공유 가능하다.

## SmokeLayerPresenter 확장

기존 rising/layer 필드, Current, Collect/Steady/SetPaused/Stop 공개 경계 보존. fireSystems[]·fireLight·빛 기본 강도/변동폭/주기와 내부 경과시간 추가. mandatory 참조 누락 시 기존 구성 오류 흐름으로 진입을 차단하고 메뉴 복귀를 허용한다. controller의 기존 구성 검증에 새 참조 검사를 포함한다.

| Phase | 유입·상층·화재 | 불빛 | 전이 |
|---|---|---|---|
| Stopped | 정지·clear | off, 시간 초기화 | Collect → InitialCollection |
| InitialCollection | 고정 seed로 새 실행 | 활성·시간 진행 | Steady → SteadyPractice |
| SteadyPractice | 유입 포함 계속 실행 | 계속 진행 | 반복 Steady는 재시작하지 않음 |
| Frozen | 전부 Pause | 강도·시간 고정 | SetPaused(false) → beforePause |

Stop/OnDisable은 어느 상태에서도 Stopped로 간다. 정지 중 pause 호출은 시작시키지 않는다. 복구 중 다른 blocker가 있으면 controller가 재개를 허용할 때까지 Frozen을 유지한다. 시스템 복구로 Collect가 다시 호출되면 기존 규칙대로 새 원리 연출을 시작한다. 새 게임 도메인 상태는 만들지 않는다.

## 공용 렌더 설정

기존 프로젝트 URP 설정을 메뉴·두 체험이 공용으로 사용한다. 추가 조명 지원만 최소 조정하고 콘텐츠별 설정 참조·소유권·이전 값 저장·교체·복원 상태는 만들지 않는다. 천장등·화재 불빛은 공간 root와 기존 연출 lifecycle을 따른다. 변경값과 수신 프로젝트 조건은 Integration.md에 기록한다.

## AssetProvenance

기존 VisualAssetSources.json에 environment 항목 추가: assetId, sourceUrl, downloadUrl, license=CC0-1.0, licenseUrl, retrievedAt, sourceFile, SHA256, importedFiles[], transformations[]. 기존 current/historical 항목은 보존한다. sourceFile 원본 아카이브는 Artifacts, 실제 사용 map·출처 설명은 _02LWS 안에 포함한다. notices에도 같은 출처를 기록한다.

## EvidenceRecord

검증 폴더의 verification.md: 환경/버전/quality/해상도/FOV/카메라 위치·높이/yaw/seed/단계·경과시간/파일 목록/회귀 결과/성능 조건·표본/승인 답변/VR 미검증/녹화 실패 이유. 없음과 미측정을 0이나 성공으로 기록하지 않는다. 원본은 Git 비추적이며 패키지에 포함하지 않는다.
