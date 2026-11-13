# 경로 콘텐츠 내보내기와 수신 검증

2026-11-13, Unity 6000.3.26f1. `_02LWS` 자산만 선택하고 Include dependencies 없이 내보냈다. 보관용 `Scenes/ContentMenu.unity`는 제외했다. 모델 가중치/Python과 Artifacts 실행 원본은 포함하지 않는다.

패키지: `Artifacts/EvacuationRoute/SafeEvacuationRoute.unitypackage`. 전체 자산과 .meta를 원본과 비교한 결과는 `Artifacts/EvacuationRoute/PackageAudit.json`이다. 최종 문서 갱신 후 같은 방식으로 패키지를 다시 내보내 비교한다.

메뉴 버튼 간격 수정과 관련 문서를 포함해 패키지를 다시 내보냈다. 아래 수신 프로젝트의 166개 검사는 메뉴 간격 수정 전 결과이며, 수정 후 메뉴는 원본 Editor에서 별도 28개 검사로 확인했다.

수신 프로젝트는 `.utmp/EvacuationRouteImport`에 새로 만들었다. 이전 테스트 프로젝트에서 핀 고정된 Packages, ProjectSettings와 XR/TMP 사전 설정만 복사했으며 기존 `_02LWS` 콘텐츠와 옛 Editor 검증 스크립트는 복사하지 않았다. 신규 패키지로 제품 자산을 가져왔다. 수신 프로젝트 안에서 URP renderer/pipeline을 새로 구성했고 원본 프로젝트 설정은 덮어쓰지 않았다.

컴파일 성공, 콘텐츠 등록 3개, 누락 스크립트 0개, 씬의 `_02LWS` 밖 비-UPM 필수 참조 0개, 도메인 69개와 공용 UI/경로 구성 검사 통과. 수신 Editor Play에서 경로 통합 166개가 모두 통과했다. 화재 콘텐츠의 안내 후 공용 선택지/이동 허용, 자세 콘텐츠의 전체 완료/이동 금지도 확인했다. 수신 화재 콘텐츠 전체 절차를 반복 완료했다는 뜻은 아니며, 기존 두 콘텐츠의 전체 회귀는 원본 Editor의 49개 검사로 확인했다.

결과: `Artifacts/EvacuationRoute/ReceiverAudit.txt`, `ReceiverPlaySummary.txt`, `ReceiverEditor.log`, `Artifacts/Validation/VisualEvidence/20261113-220921-safe-evacuation-route-receiver/integration.txt`. 같은 폴더의 실제 렌더 PNG에서 재질, 연기와 UI를 확인했다. 입력/추적은 Editor 합성 입력이다.

Unity/UPM/XR/URP/Input System 사전 설정은 여전히 필요하다. 제품 자산은 패키지로 전달하고 받는 프로젝트의 설정을 일괄 덮어쓰지 않는다. 이 결과는 별도 테스트 프로젝트의 수신 검증이며, 아직 지정되지 않은 최종 통합 프로젝트나 실제 Quest 실행의 합격이 아니다.
