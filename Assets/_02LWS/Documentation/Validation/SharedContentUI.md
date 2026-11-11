## Latest: 체험 UI 시야 가림 수정 — 2026-11-12

공용 Canvas1개와 안내 재생기1개를 유지하고 체험 안내를 아래쪽 가변 높이로 바꿨다. 고정1.325m 높이 배경·중복 제목·콘텐츠2 전용 고정 설명을 제거했다. 빈 상태/자막 행은 공간을 사용하지 않고 내용/동작이 모두 없으면 배경을 숨긴다. 글자150%는 높이를 늘려 수용한다. 메뉴/설정/결과 배치는 유지한다.

현재 수정본 Editor 통합49/49, 자막/배치452/452 PASS. 실제 실행 영상의1101개 활성 안내 표본에서 상단 고정·글자 영역 초과0건. 저장 씬 Canvas1/안내 재생기1/누락 스크립트0/외부 프로젝트 자산 참조0, 컴파일 오류0. 실제 MP4를 Unity VideoPlayer로 디코딩했다.

원본: Artifacts/Validation/VisualEvidence/20261112-023201-compact-content-ui/verification.md, execution.mp4, 03-fire-choices.png, 03b-fire-choices150.png, 09-synthetic-crouch.png. 검증은 Editor 합성 입력이다. Quest 가독성/편안함·기기 성능·학습·최종 Import는 미검증. 아래 기존 APK와284검사 기록은 이 수정 이전 이력이며 최신 시야 수정이 APK에 포함됐다는 뜻이 아니다.
# 공용 UI 통합 — 2026-11-12

## 변경 결과

SharedSystems/SharedCanvas 하나에 메뉴·안내/자막/상태·선택/준비/다음·설정/중단/결과를 일반 패널로 모았다. SharedNarration 하나에서 음성·자막을 재생한다. Contents 아래 메뉴/두 콘텐츠를 그룹화했고 콘텐츠 내부 Canvas는 0개다. 비상벨 표지는 같은 공간에 붙는 3D 텍스트다. 공용 머리 추적 공급자를 SharedSystems로 옮겼으며 개인 기준/2초 판정은 연기 세션 소유다.

SharedContentUI는 소유자와 버튼 세대를 확인해 내용·동작을 연결하며 콘텐츠 구체 타입을 참조하지 않는다. 콘텐츠 종료/교체·화면 변경·중단 경계에서 이전 누름/콜백을 거부한다. 공용 결과 화면은 완료 안내 재생이 끝난 뒤 표시해 자막을 보존한다. 원래 선택지 문구와 교육 순서·물리 벨·공간 경보음·F1 음원은 유지했다.

신규 콘텐츠는 ContentAuthoring.md와 같은 API를 사용한다. Tools > LWS > Validate Shared UI 검사와 진입 전 검사는 Canvas/안내 재생기/UI 추적기 중복과 콘텐츠 내부 UI 입력을 검출한다. 현재 버튼 줄은 최대 2개 선택지를 지원하며 더 많은 선택지는 공용 레이아웃을 확장하고 150% 검사를 추가해야 한다. 별도 콘텐츠 UI로 우회하지 않는다. AGENTS.md는 수정하지 않았다.

## 검증 상태

최종 저장 씬/Play Mode: Canvas 1개·GuidancePresenter 1개·콘텐츠 내부 Canvas 0개. 실제 임시 Canvas 추가가 검사와 진입 차단에 검출됐다. 새 콘텐츠 UI 연결·오래된 이벤트·모달·전체 두 콘텐츠 진행·실제 XRPokeInteractor 벨 입력·자세 판정·재시작/복귀의 통합 47/47 PASS. 기존 자세 도메인 검사 84/84 PASS. 실제 공용 TrackedDeviceGraphicRaycaster에 가상 XR 레이를 넣어 Content1 버튼 hit PASS.

150% 글자에서 기존 긴 선택지의 필요 높이 147.01 <= 164, 안내 6개의 필요 높이 최대 278.02 <= 자막 영역 300을 확인했다. 화면 크기와 자막/버튼/고지 간격을 교정하고 실제 실행 프레임을 확인했다. 원본은 헌장 지정 경로에 별도 보관한다. 실제 Quest 양안/읽기/편안함 판정은 아니다.

## 발견 및 수정

- 공용화 전 Canvas=10, 안내 재생기=2, 콘텐츠 내부 Canvas=6으로 단일 공용화 조건 실패를 확인했다.
- 추적 상실 전에 시작한 준비 누름이 복귀 후 실행되는 결함을 재현했다. CanSelect와 차단 경계의 InvalidatePress로 교정했고 같은 재현 검사는 PASS다.
- 완료 음성 재생 중 결과 화면이 자막을 숨기는 결함을 재현했다. IsPresenting 기반으로 결과 표시를 지연해 교정했고 재현 검사 PASS다.
- 최초 입력 검사는 XR 활성 상태에 마우스 PointerEventData를 사용해 실패했다. 실제 XR용 TrackedDeviceEventData로 교정한 전체 진행은 통과했다. 초기 회귀 검사의 음량 1.0 고정 가정과 종료한 콘텐츠의 guidance 직접 참조도 새 소유권 계약에 맞춰 보정했다. 초기 실패 로그는 원본 폴더에 보존한다.

## 한계

Editor의 합성 포즈/시간/중단 및 가상 포인터·poke를 사용했다. 실제 Quest OS 신호·컨트롤러·오프라인·성능·양안·학습 효과·최종 receiver Import는 기존 미완료 의무로 남는다. 추가 회귀와 최신 개발 APK 결과는 아래에 기록했다.
## 추가 회귀 완료

콘텐츠 1의 기존 단계별 설정/일시정지/시스템 복구/재시작/복귀111/111 PASS, 연기 시스템 복구42/42 PASS. 앞의 통합47와 도메인84를 합쳐284개 Editor 검사 모두 통과했다. 저장/실행 환경을 구분하고 실제 Quest 결과로 표시하지 않는다.

최종 원본: `Artifacts/Validation/VisualEvidence/20261112-011720-shared-content-ui/execution.mp4` 및 같은 폴더의 verification.md. MP4 110.1초·1280×720·10fps·1101프레임, 실제 VideoPlayer 디코딩 확인. 저장/실행 씬 모두 구조 위반0, 외부 프로젝트 자산 참조0, 누락 스크립트0이다. UPM/ProjectSettings는 별도 수신 환경 전제다. 개발 빌드 결과는 아래에 기록했다.

## Final shared UI development build — 2026-11-12

T060 complete: Android Development/IL2CPP/ARM64 build Succeeded,0 errors,1 diagnostics-symbol warning,293.495 seconds. Actual APK 108592714 bytes; SHA256 E83CD20A3195E0BEE63B15EF05D32E44670396D8B5A227D2D86F8C698860FD92. Main scene Assets/_02LWS/Scenes/FireNotification.unity. No package/engine upgrade or intentional XR setting change.

Output: Artifacts/SmokePosture/SharedUI-development-20261112.apk
Identical archive: Assets/_02LWS/Artifacts~/SharedUI/SharedUI-development-20261112.apk

The warning requests SymbolTable/Full debug symbols for detailed crash traces. Current APK includes the one-Canvas shared UI and replaces earlier APKs only for this refactor's development testing. Source/archive hashes match. ADB returned no attached devices; install/run, actual Quest readability/performance and final receiver acceptance remain pending. No final unitypackage re-export/receiver certification is claimed.

Build-generated XR preloaded entries were restored to the initially empty list. Dynamic Korean glyph cache and automatic LiberationSans material serialization were removed from the change set only after byte-identical backups and hashes were retained under Artifacts/Validation/VisualEvidence/20261112-011720-shared-content-ui/generated-font-backup/. The initial automatic approval rejection was resolved by making cleanup reversible and verifying the backups; no user font edits were intentionally discarded. Raw evidence/media/backups remain Git ignored and outside Assets.