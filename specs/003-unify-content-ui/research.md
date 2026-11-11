# Research: 콘텐츠 공통 UI 통일

**Date**: 2026-11-11. 저장소 조사와 별도 연구 에이전트의 읽기 전용 검토를 통합했다. 아래 선택은 구현 설계다.

## 기존 프레임워크

- **Decision**: Unity 6000.3.26f1, 기존 uGUI/TMP/XRI와 Theme/Prefab 유지.
- **Rationale**: ProjectVersion/manifest/runtime asmdef에 환경이 명시되어 있고 UITheme/ThemedText/ThemedButton과 UI_Button.prefab이 존재한다. 기본 Theme는 문서 색상·크기를 사용한다.
- **Alternatives considered**: UI Toolkit/팀 공용 패키지 전환은 입력·이식 범위를 늘린다. 콘텐츠별 복제는 반복 제작 문제를 남긴다.

## 공유 UI의 소유권

- **Decision**: 콘텐츠 밖에 체험 UI 한 벌, Navigator가 활성 어댑터와 연결한다. 세션은 콘텐츠별 유지.
- **Rationale**: ContentSceneNavigator.Navigate는 콘텐츠 루트를 비활성화하고 Begin/Stop한다. SharedUIController는 이미 메뉴/모달/결과를 공유한다. 뷰에 두 세션 단계 해석을 넣으면 공통 UI가 콘텐츠 로직까지 소유한다.
- **Alternatives considered**: 콘텐츠마다 동일 Prefab 인스턴스를 배치하면 소유권이 분산된다. 모든 진행을 공통 컨트롤러로 합치는 것은 회귀 범위가 크다.

## 음성·자막·필수 상태

- **Decision**: GuidancePresenter/AudioSource는 콘텐츠에 남기고 세대 검사를 하는 공통 자막 출력 계약을 연결한다. 상태 슬롯은 자막과 분리한다.
- **Rationale**: GuidancePresenter.Cancel은 자막을 지우므로 공유 TMP를 직접 양쪽에 주면 오래된 취소가 새 자막을 지울 수 있다. FireNotificationAdapter.ApplySettings는 콘텐츠 자식 오디오를 찾는다. 자세 status는 자막 끄기와 별개인 필수 안내다.
- **Alternatives considered**: 직접 TMP 공유는 쓰기 소유권이 없다. 음성까지 한 재생기로 옮기면 기존 완료·취소·음량 경로가 크게 달라진다.

## 행동·입력

- **Decision**: 버튼 ID를 활성 어댑터로 전달하고 세대/revision을 검사한다. 문맥 변경 때 새 누름을 요구한다.
- **Rationale**: ActionChoiceInput/NextInput/ReturnToMenuInput은 화재 컨트롤러에 직접 연결된다. ActionOptions.Validate의 승인 선택지는 보존해야 한다. FreshPressButton은 OnEnable에서 초기화하므로 내용만 바꾸면 이전 입력이 남는다. BlockWorld의 콘텐츠 자식 검색은 외부 공유 버튼을 찾지 못한다.
- **Alternatives considered**: 여러 콘텐츠의 UnityEvent 콜백 누적은 중복·오동작 위험, 전역 이벤트 버스는 불필요한 복잡성이다.

## 안내의 실제 차이

- **Decision**: 화재 원문·음성 유지. 연기의 긴 안내만 간결화하고 문장·의미별 자막으로 나눈다.
- **Rationale**: 화재 Content/GuidanceCatalog.asset는 문장별 segment가 있다. 연기 Data/SmokePosture/GuidanceCatalog.asset는 cue 전체가 하나의 segment다. ready 약 11초, completion 약 18초 분량을 한 번에 표시한다. GuidanceCue.Validate는 segment 공백 결합과 text의 일치 및 audio 길이 내 timing을 요구한다. 총 길이뿐 아니라 한 번에 보이는 정보량을 줄여야 한다.
- **Alternatives considered**: 자막만 축약하면 음성과 불일치한다. 임의 글자 수 제한이나 화재 대사까지 재작성하는 것은 사용자 결정과 맞지 않는다.

## 검증과 기존 문서

- **Decision**: Editor 검증 도구와 실제 씬/기기 검증, 변경 근거 기록에 더해 헌법 v1.1.0의 MP4 기록·대체 시각 자료·공통 경로 보관·결과물 경로 보고를 적용한다.
- **Rationale**: 프로젝트 소유 자동 테스트는 발견하지 못했다. 기존 자세 quickstart는 Editor 주입과 기기 검증을 구분한다. 최초 조사 이후 2026-11-11 채택된 헌법 v1.1.0은 사용자가 나중에 확인할 수 있는 실행 화면 기록을 요구한다. design.md의 콘텐츠 구현 상태는 오래되어 최신 자세 명세를 취소하지 않으며 기존 승인 추종 정책은 유지한다.
- **Alternatives considered**: PackageCache 테스트를 프로젝트 검증으로 간주하거나 PC 성공을 실기기 성공으로 대체하지 않는다. 텍스트 로그만으로 녹화를 대체하지 않으며 캡처 도구는 구현 시 실제 환경에서 확인한다. 특정 도구의 가용성을 검증했다고 주장하지 않는다.

## 조사 근거

저장 경로 교정: 헌법 1.0.0 최초 채택 뒤 1.1.0에서 비추적 의무와 경로를 명시했다. 기존 `.gitignore`의 `/[Aa]rtifacts/` 규칙을 사용해 프로젝트 루트 `Artifacts/Validation/VisualEvidence/`에 로컬 결과물을 둔다. Assets 안에 생성하면 Git 및 Unity 자산/제출물에 섞일 수 있어 검증 원본 경로로 사용하지 않는다.

프로젝트 루트 기준: design.md, AGENTS.md, specs/001-fire-notification/spec.md, specs/002-smoke-low-posture/contracts/experience-ui.md 및 quickstart.md, ProjectSettings/ProjectVersion.txt, Packages/manifest.json, Assets/_02LWS/Scripts 아래 Runtime/Presentation/Interaction의 위 클래스와 두 카탈로그.

남은 기술 미확정 항목은 없다. 신규 외부 API/패키지가 없어 웹 API 조사는 수행하지 않았다. 실제 음성/기기 마련 여부는 구현 검증 결과에 기록한다.
