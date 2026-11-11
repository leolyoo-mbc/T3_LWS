# Implementation Plan: 콘텐츠 공통 UI 통일

**Branch**: 현재 Git `main` (기능 식별자 `003-unify-content-ui`) | **Date**: 2026-11-11 | **Spec**: [spec.md](spec.md)

**Input**: `specs/003-unify-content-ui/spec.md`

## Summary

기존 World Space uGUI와 Theme/버튼을 재사용해 콘텐츠 루트 밖에 공통 체험 UI 한 벌을 둔다. 현재 콘텐츠의 어댑터가 안내·필수 상태·행동 데이터를 공급한다. 기존 메뉴·모달·결과와 콘텐츠 진행·음성 재생·자세 판정은 유지한다. 화재 안내 원문과 음성은 보존하고 자세 연습의 긴 안내만 간결화하고 문장별 자막을 제공한다.

아래 자료형과 연결 방식은 구현을 위한 설계 결정이며 사용자 확정 요구나 팀 합의로 소급 기록하지 않는다.

## Technical Context

**Language/Version**: 기존 C#, Unity 6000.3.26f1의 컴파일 환경 유지.

**Primary Dependencies**: uGUI 2.0.0/TMP, Input System 1.20.0, XRI 3.5.1, XR Core Utils 2.6.0, OpenXR 1.17.1, URP 17.3.0. 패키지 변경 없음.

**Storage**: 기존 ScriptableObject 카탈로그·Theme·선택지 자산, 씬과 Prefab. DB/진행 저장 없음.

**Testing**: 프로젝트 소유 자동 테스트는 조사에서 발견하지 못했다. Editor 검증 도구, 실제 씬 Play Mode 시나리오, Quest 2 검증을 사용한다. 새 테스트 패키지 설치는 요구하지 않는다.

**Target Platform**: Quest 2/OpenXR, Windows Editor의 PC 테스트.

**Project Type**: Unity VR 교육 앱의 UI 공통화 및 제한된 대사 정리.

**Performance Goals**: 전환 시만 바인딩/행동 목록 갱신. 매 프레임 객체 검색·버튼 생성·리스너 누적 금지. 상태가 같으면 표시 모델 재할당과 레이아웃 재계산을 하지 않는다. 새 FPS 보장값 대신 Quest 2에서 변경 전후 UI 할당·레이아웃 부하를 비교한다.

**Constraints**: design.md v0.1.1 적용. AGENTS.md 수정 금지. 제출용 코드/자원/검증 도구는 Assets/_02LWS 내부. 화재 문구·음성 보존, 교육 순서·자세 판정·이동 제한 유지. 사용자 작업 중인 XR 설정과 폰트 변경을 덮어쓰지 않는다.

**Scale/Scope**: 기존 두 콘텐츠, 공통 체험 UI 한 벌과 기존 메뉴·모달·결과. 새 콘텐츠/팀 공용 패키지/UI Toolkit 전환 없음.

## Constitution Check

헌법은 미작성 템플릿이므로 자리표시자를 원칙으로 간주하지 않는다. 다음은 실제 프로젝트 지침과 사용자 요구의 게이트다.

| Gate | 연구 전 | 설계 후 |
|---|---|---|
| 확정 요구·설계 결정·미확정 제안 구분 | 통과 | 통과 — 아래 검토 절차의 지위 명시 |
| 교육 의미·흐름 보존 | 통과 | 통과 — 도메인 세션 유지 |
| design.md 및 기존 자원 우선 | 통과 | 통과 — Theme/Prefab/uGUI 재사용 |
| Assets/_02LWS 제출 경계 | 통과 | 통과 — 검증 도구 포함 |
| 화재 원문 보존·PC/기기 결과 구분 | 통과 | 통과 — 기준 캡처와 환경별 검증 |

## Project Structure

### Documentation (this feature)

```text
specs/003-unify-content-ui/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── contracts/shared-experience-ui.md
├── quickstart.md
└── checklists/requirements.md
```

`tasks.md`는 후속 speckit-tasks에서 생성한다.

### Source Code (repository root)

```text
Assets/_02LWS/
├── Scripts/Runtime/       # 어댑터·Navigator·SharedUIController
├── Scripts/Presentation/  # 공유 모델/뷰·자막 출력 계약
├── Scripts/Interaction/   # 버튼 전달·새 누름 초기화
├── UI/Prefabs/            # SharedExperienceUI.prefab, 기존 버튼/모달
├── Data/SmokePosture/      # 긴 안내 정리·분할 자막
├── Content/               # 화재 카탈로그·선택지 보존
├── Scenes/                # 실제 씬 연결 교체
├── Editor/                # 공통 UI 검증 도구
└── Documentation/Validation/ # 화면·대사 대조와 검증 기록
```

**Structure Decision**: 기존 Runtime/Presentation/Interaction 분리와 어셈블리를 유지한다. namespace는 현재 LWS.FireNotification 하위를 사용한다. 팀 공용 namespace/패키지 전환은 범위 밖이다.

## Phase 0 — Research

[research.md](research.md)에 기존 코드·카탈로그·씬·명세 조사와 대안 비교를 정리했다. 환경·UI 프레임워크·소유권·입력·검증 방법의 미해결 기술 항목은 없다. 신규 외부 API/패키지를 도입하지 않는다.

## Phase 1 — Design

### 화면·데이터·행동

- 기존 화재 UI에서 자원을 추출·조합한 SharedExperienceUI.prefab을 공통 앱 계층에 한 인스턴스 둔다. 전체 씬을 재생성하지 않는다.
- SharedExperienceUIView는 안내 자막·필수 상태·행동 영역을 표시한다. ContentExperienceAdapter.UIState와 SelectUIAction(id)를 통해 연결하고 구체적 세션 단계는 해석하지 않는다.
- 기존 ActionOptions를 승인된 화재 선택지의 단일 원본으로 유지한다. 다음/준비/복귀 문구와 상태 안내는 콘텐츠별 ContentUIConfig 자산에 둔다. GuidanceCatalog는 기존 음성·대사 원본을 유지한다.
- 화재 어댑터는 기존 Send/RequestReturnToMenu, 자세 어댑터는 Ready/RequestReturn에 연결한다. 기존 ActionChoiceInput/NextInput/콘텐츠별 복귀 UI 연결은 새 공통 전달로 교체하고 남은 참조 확인 후 정리한다. 비상벨 입력은 유지한다.

### 연결·음성·중단

- Navigator 순서: 이전 어댑터 구독 해제 → 이전 Stop → UI 소유권 해제/초기화 → 루트/위치 전환 → 새 소유권 부여/구독 → Begin. 최초 안내 전 연결하며 실패/비활성화 경로도 정리한다.
- GuidancePresenter와 AudioSource는 콘텐츠에 남기고 자막 출력만 IGuidanceSubtitleSink로 공유한다. 소유권 세대가 일치할 때만 쓰기/지우기를 허용한다. 이전 Cancel이 새 자막을 지울 수 없어야 한다. 기존 run/execution 완료 보호를 유지한다.
- 상태 모델 갱신이 재생 중 자막을 덮어쓰지 않는다. 공통 자막 켜기/끄기는 SharedUIController가 한 번 적용하고, 각 어댑터는 자기 음량만 적용한다. 비활성 콘텐츠가 공통 자막 설정을 바꾸지 않는다.
- SharedUIController.BlockWorld에 공유 CanvasGroup/버튼을 명시적으로 등록한다. 모달이 열리면 콘텐츠 입력과 공유 입력을 함께 차단하며 닫기는 기존 재생 지점에서 재개한다.
- 소유자/행동/가용성 변경 때 FreshPressButton.ResetForNewContext를 호출한다. 이전 포인터/처리 상태를 버리고 마우스·양쪽 트리거를 모두 뗀 뒤 새 누름만 허용한다. 선택 시 세대·revision·현재 가용성을 다시 확인한다.
- 자세의 status/readyPanel/guidance.subtitle 직접 참조 검증을 공통 계약 검증으로 교체한다. 상태 변경 시만 Publish하며 추적 오류·구성 오류·자동 인정·practiceArmed 및 기존 전이 정책을 유지한다.

### 디자인·안내

- Theme/폰트/둥근 Sprite/버튼 상태를 재사용하고 design.md 5~10절을 대조한다. 월드 스케일 0.001m/u, 본문/버튼 32u, 패널 여백 32u, 버튼 간격 16u 이상, 최소 240×80u를 적용한다. 메뉴는 기존 1000×720u/1.5m 기준을 유지한다.
- 공통 안내·선택은 기존 정면 아래쪽 역할 배치를 기준으로 자막·필수 상태·행동 영역을 한 패널 안에서 구분한다. 같은 대사는 중복 표시하지 않는다. 버튼이 없으면 영역을 접는다. TMP preferred size로 확대/두 줄 버튼 높이를 확보하고 기존 화재 UI 폭을 기준으로 한다. 문서 대비 변화는 근거를 기록한다.
- ComfortFollowUI를 명시적 참조로 한 번 등록한다. 기존 안내 Canvas 이름별 등록을 교체하되 메뉴/결과 추종과 안전 표지 공간 고정은 유지한다.
- 화재 cue ID/text/segment 문구/오디오 참조를 기준 기록과 비교해 보존한다. 연기는 긴 안내만 다듬고 문장·의미별 segment로 나눈다. 임의 글자 수 상한은 도입하지 않는다.
- 연기 문구가 바뀌면 해당 음성과 timing도 함께 갱신한다. 기존 로컬 음성 제작 경로가 있으면 재사용한다. 마련 전에는 불일치 상태를 완성본으로 연결하지 않는다. 새 안전 요령이나 교육 근거는 추가하지 않는다.

### 순서·검토

기준 화면/원문 확보 → 공통 표시/입력 계약 → 화재 연결 → 자세 연결/안내 정리 → 중복 UI 참조 정리 → 회귀 검증 순서다. 이는 기술적 순서이며 ‘화재 화면 승인 전 다른 콘텐츠 적용 금지’라는 미확정 제안을 승인된 조건으로 추가하지 않는다. 문서에 없는 새 디자인이 필요하면 해당 제안만 확인한다.

## Validation and Completion

[data-model.md](data-model.md), [연결 계약](contracts/shared-experience-ui.md), [quickstart.md](quickstart.md)에 모델·소유권·실행 검증을 정의한다. Editor 도구로 이전 콜백 거부·설정 격리·원문 불변과 실제 씬 구성을 검사하고 Play Mode/Quest에서 입력과 표시를 확인한다. 도메인 판정 알고리즘을 다시 설계하지 않는다.

이번 단계는 설계 문서만 작성한다. 구현·기기 성공으로 표시하지 않으며 음성/기기 미준비는 검증 기록에 남긴다. design.md와 기존 콘텐츠 명세는 수정하지 않는다.

## Complexity Tracking

게이트 위반 없음. 별도 UI 프레임워크·글로벌 이벤트 버스·새 패키지·새 도메인 상태 머신은 도입하지 않는다.
