# 경로 콘텐츠 자산 검사

2026-11-13, Unity 6000.3.26f1, 실제 FireNotification.unity와 신규 route 자산 검사.

씬의 모든 재귀 의존성 중 Assets/_02LWS 밖 비-UPM 필수 자산 참조 0개, 누락 스크립트 0개, 누락/오류 셰이더 재질 0개. 공용 Canvas/안내 재생기/추종기 각 1개, 콘텐츠 내부 UI 0개는 SharedUIContractAudit로 확인했다.

RouteSpace, SchoolRouteA/B, RouteTeleportAnchor, ScenarioA/B, RouteGuidance와 7개 WAV/manifest는 _02LWS 내부에 있다. .meta를 함께 보존한다. 기본 Unity 도형 메시와 UPM 의존성은 Unity/패키지에서 제공된다. 모델 가중치, Python, 임시 생성 코드와 검증 원본은 ignored .utmp 또는 Artifacts에만 두며 납품하지 않는다.

기존 두 콘텐츠의 원본 프리팹과 환경 재질은 수정하지 않고 복도 마감 재질을 참조한다. 신규 Route 재질과 길이별 바닥 재질은 로컬 복제 자산이다. 출처는 ThirdPartyNotices.md와 VisualAssetSources.json을 유지한다.
