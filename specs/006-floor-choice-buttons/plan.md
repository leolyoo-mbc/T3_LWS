# Implementation Plan: 바닥에서 올라오는 선택지와 진행 버튼

**Branch**: `main` 유지 | **Date**: 2026-11-16 | **Spec**: [spec.md](spec.md)

**Feature ID**: `006-floor-choice-buttons`. setup-plan의 BRANCH 반환값은 기능 식별자이며 실제 Git 브랜치는 `main`이다.

## Summary

세 체험의 선택지와 준비 완료/다음/다음 연습을 공용 바닥 버튼판으로 옮긴다. 기존 `SharedContentUI.SetActions`의 소유자 검증과 교육 콜백을 유지하고, 위치 참조를 추가해 공용 `FloorButtonPresenter`에 표시를 위임한다. 자막과 메뉴는 기존 Canvas에 남긴다.

사용자 확정 외형은 기둥/받침대/개폐구/특수 효과가 없는 판이다. 아래 크기/시간 수치는 **구현 시작값**이며 사용자 외관 승인이나 업계 표준이 아니다. 실제 배치 검토 후 조정한다.

## Technical Context

**Language/Version**: C#, Unity 6000.3.26f1, 기존 컴파일 설정 유지.

**Primary Dependencies**: uGUI 2.0.0/TMP, XRI 3.5.1, Input System 1.20.0, OpenXR 1.17.1, URP 17.3.0. 패키지 추가/업그레이드 없음.

**Storage**: `Assets/_02LWS` 내 직렬화된 씬/프리팹/앵커 설정. 신규 저장 데이터/네트워크 없음.

**Testing**: 기존 정적 검증 클래스 방식의 Editor 구성/Play Mode 검사, 실제 XR UI 레이 검사, 기존 경로 체험 회귀 검사, 실제 실행 영상과 Quest 실기 확인을 분리.

**Target Platform**: 기존 Quest/OpenXR VR 대상, Windows Editor 보조 검증.

**Project Type**: 단일 씬 Unity VR 교육 앱.

**Performance Goals**: 바닥 Canvas 1개, 동시 표시 묶음 최대 1개/버튼 최대 2개. 버튼을 재사용하고 프레임마다 생성/파괴하지 않는다. 기존 기기 주사율/성능 설정을 유지하며 새 FPS 합격 수치를 임의로 만들지 않는다. Quest에서 같은 구간의 변경 전후 CPU/GPU frame time을 비교한다.

**Constraints**: 교육 판단/문구/음성/준비 높이 저장 시점 유지. 다음 연습의 실제 구역 이동만 하강 완료 후 수행한다. 시선 조건 없음. 정지 중 이동/입력 정지. 구현 검증 영상은 실제 1920x1080 이상/30fps 이상, Unity 게임 소리만 포함, Git 비추적.

**Scale/Scope**: 3개 콘텐츠. 화재 공용 앵커 1개, 연기 준비 1개, 경로 준비 1개와 판단 대상 6개가 초기 구성이다. 화재 다음과 경로 다음 연습은 위치를 재사용하되 가림이 있으면 별도 앵커로 조정한다.

## Constitution Check

| Gate | Phase 0 진입 | Phase 1 설계 후 |
|---|---|---|
| I. 실제 실행 MP4/게임 소리/1920x1080/30fps 이상 | PASS: 구현 검증에 포함 | PASS: quickstart에 녹화/분리 확인/직접 재생 반영 |
| II. 불가 시 대체 자료와 미검증 명시 | PASS: 예외를 합격 처리하지 않음 | PASS: 단계별 실제 스크린샷/사유/한계 정의 |
| III. 공통 경로/Git 비추적/대표 파일 링크 | PASS: Artifacts 사용 | PASS: 제외/비추적 검사와 직접 파일 링크 정의 |
| 사용자 규칙: `_02LWS` 제출 범위/AGENTS 보호/확정과 제안 구분 | PASS | PASS: 신규 자산 위치/조정값 성격 명시 |
| 문서만 작성하는 현재 단계 | 새 실행 녹화 면제 | 실행 코드/씬 변경 없음, 실행 합격 선언 없음 |

헌법 위반 없음. Canvas 1개 검사는 헌법이 아닌 기존 구현 제약이다. 표시 역할을 분리하면서 그 검사를 개정하되 콘텐츠별 UI 복제 금지와 공용 소유는 유지한다.

## Project Structure

### Documentation (this feature)

```text
specs/006-floor-choice-buttons/
  spec.md
  plan.md
  research.md
  data-model.md
  quickstart.md
  contracts/floor-buttons.md
  checklists/requirements.md
```

`tasks.md`는 이번 명령에서 생성하지 않는다.

### Source Code (planned changes, repository root)

```text
Assets/_02LWS/
  Scenes/FireNotification.unity                 # 공용 판/앵커/참조 연결
  Prefabs/UI/FloorButtonPanel.prefab            # 신규 World Space 판
  Scripts/Runtime/FloorButtonPresenter.cs       # 신규 표시 상태/이동/입력 잠금
  Scripts/Runtime/FloorButtonAnchor.cs          # 신규 위치/조정값/gizmo
  Scripts/Runtime/SharedContentUI.cs            # 요청 위임, 자막에서 Actions 행 제거
  Scripts/Runtime/SharedUIController.cs         # 바닥 정지/설정/마우스 raycaster 연동
  Scripts/Runtime/SharedUIContractAudit.cs      # 두 공용 Canvas 역할/중복 검사
  Scripts/Runtime/FireNotificationAdapter.cs    # 화재 위치 전달
  Scripts/Runtime/SmokePostureController.cs     # 준비 위치 전달
  Scripts/Runtime/EvacuationRouteController.cs  # 준비/판단 대상별 위치 전달
  Scripts/Validation/FloorButtonChecks.cs       # 신규 구성/실행 검사
  Scripts/Validation/RouteIntegrationChecks.cs  # Ready 대기/새 버튼 위치 검사
  Editor/FloorButtonAnchorEditor.cs             # 신규 등장 전/후 미리보기
```

신규 자산에는 Unity `.meta`를 포함한다. Domain/Scenario/교육 데이터/음성은 변경하지 않는다. `FreshPressButton`은 재사용하며 상태 진입 시 `InvalidatePress`를 호출한다. 이 컴포넌트를 수정하게 되면 메뉴 버튼도 회귀 검사한다.

## Phase 0: Research

근거/대안은 [research.md](research.md)에 기록했다. 기술 조사 미해결 항목은 없다. 외형 수치는 편집 가능한 시작값으로 정하며 최종 외관 승인은 실제 배치 검토에 남긴다.

## Phase 1: Design

### 공용 소유와 Hierarchy

```text
SharedSystems
  SharedCanvas                         # 기존 머리 추종, 자막/상태/메뉴/결과
    ContentPanel                       # 기존 Actions 행 제거
  SharedFloorButtons                    # ComfortFollowUI 밖
    FloorCanvas                         # World Space, 공용 1개
      FloorViewport                     # RectMask2D, 바닥 아래 표시 차단
        MovingPanel
          ActionButton1
          ActionButton2
  LwsEventSystem                        # 기존 1개 재사용
각 콘텐츠 공간
  FloorButtonAnchors                    # 위치/설정만, Canvas/입력 없음
```

감사는 Canvas 2개뿐 아니라 지정된 두 참조/공용 소유/다른 Canvas 부재/narrator 1개/EventSystem 1개를 검사한다. 기존 통합 검사와 저장 씬 검사에 남은 단일 Canvas 가정도 갱신한다.

바닥 Canvas에 `TrackedDeviceGraphicRaycaster`와 기존 XR 카메라를 연결한다. 기존 좌우 레이/트리거와 이벤트 모듈을 재사용한다. Editor 마우스용 `GraphicRaycaster`는 기존처럼 XR 비활성일 때만 사용한다. 바닥판의 3D 가림 검사를 켜고 프로젝트 레이어에 맞춰 벽/바닥 차단 마스크를 지정한다. 모달이 열리면 바닥 렌더링/raycast를 숨기되 애니메이션 진행률은 보존한다.

### 위치 연결

| 기존 단계 | 버튼 | 초기 위치 |
|---|---|---|
| 화재 AwaitAction | shout/wait | 사건 앞 공용 앵커 |
| 화재 AwaitNext | next | 같은 앵커, 가림 검토 후 분리 가능 |
| 연기 AwaitingReady | ready | 시작 지점의 연기 관찰 방향 |
| 경로 AwaitingReady | ready | A 시작 a-w3 앞 |
| 경로 Decision | use/another | a-left/a-right/a-lift/b-left/b-right/b-lift 각각의 확인 지점 앞 |
| 경로 A ScenarioCompletion 안내 완료 | next | 실제 완료한 a-left 또는 a-right 위치 재사용 |

현재 B 시작 b-w1에는 새 준비 완료를 추가하지 않는다. B 완료는 기존처럼 결과로 간다. 위치는 콘텐츠/현재 대상에서 정하며 사용자 고개 방향으로 생성하지 않는다.

### 요청, 상태, 실행 시점

`SharedContentUI`가 owner/action generation을 유지하고 위치가 있는 요청을 Presenter에 전달한다. `SetActions(owner)`는 빈 요청/취소로 유지한다. 비어 있지 않은 요청에 위치가 없으면 구성 오류로 보고하며 기존 화면 버튼으로 대체하지 않는다. 같은 요청 반복은 상승을 재시작하지 않는다.

상태는 Hidden -> Rising -> Ready -> Lowering -> Hidden이다. 처리 중은 별도 대기 상태 대신 클릭 수락 시 원자적 잠금으로 나타낸다. pause는 진행률을 보관하는 별도 플래그다. `unscaledDeltaTime`을 쓰되 owner 차단/메뉴/로딩/비활성에서는 이동 delta를 0으로 하며 전역 timescale에 의존하지 않는다.

유효한 선택 시 전체 묶음의 입력을 먼저 잠그고 하강을 시작한 뒤 기존 콜백을 즉시 한 번 실행한다. 연기의 준비 기준 높이가 클릭 시점에 저장되도록 하강 완료까지 콜백을 미루지 않는다. 다음 연습 콜백도 요청을 즉시 수락하되 경로 컨트롤러가 실제 Transition을 하강 완료까지 보류한다. 현재 A 공간/위치를 유지한 채 `SharedContentUI.IsFloorExitComplete(owner)`가 현재 수락 요청의 정상 하강 완료를 확인한 뒤 B 전환을 한 번 수행한다. owner/run/execution을 묶어 대기하고 pause/차단 중에는 전환하지 않는다. 재시작/종료/owner 교체는 대기 이동을 취소하며 ResetDisplay를 하강 완료 신호로 취급하지 않는다. 콜백이 동기적으로 요청을 지우거나 바꿔도 내려가는 문구/위치는 별도 스냅샷으로 유지한다. 최신 다음 요청 하나만 대기시키고 하강 후 상승시킨다.

오답 피드백 이후 재요청에는 새 ticket으로 다시 상승한다. 유효한 클릭이 기존 컨트롤러의 최신 조건 검사에서 거부돼 같은 단계가 남으면 하강 후 현재 요청을 재확인해 다시 제공한다. 콜백 결과를 추측하지 않는다. Clear/owner 교체/restart/exit는 연출을 중단해 잔류 판/대기 요청/입력을 즉시 폐기한다. 정상적인 다음 연습 전환은 하강을 마친 후에만 잔류 상태를 리셋하고 구역을 바꾼다.

Rising -> Ready와 pause 진입/해제에 `InvalidatePress`를 호출한다. 상승/정지 중 누른 채 유지한 트리거는 실행되지 않고 해제 후 새로 눌러야 한다. 이전 listener와 좌우 동시 선택은 ticket 단위로 막는다.

### 외형/Editor 조정

기존 불투명 테마/글꼴/선택 피드백을 사용한다. RectMask2D의 하단을 바닥 경계에 맞추고 안에서 판만 수직 이동한다. 특수 셰이더/Animator/Tween 패키지/물리 장치는 추가하지 않는다. TMP preferred size와 글자 배율 1/1.5에 맞춰 높이를 계산한다.

시작값: 상승 0.35초/하강 0.25초, 바닥 여유 0.02m, 상승 후 판 하단 0.60m, 두 항목 총 너비 1.35m/간격 0.02m, 한 항목 너비 0.66m, 최소 높이 0.24m. 긴 문구/확대 글자에는 높이를 늘린다. 기울기는 기본 0도이고 편집 가능하지만 기울인 판의 바닥 경계/가림도 검사한다. 위치/방향은 수동 앵커이며 자동 정면 재배치/바닥 탐색을 넣지 않는다.

Inspector에 숨김/완전 상승/해제 미리보기를 제공한다. 실제 프리팹을 Editor 전용 비저장 객체로 표시하고 교육 콜백/음성/입력은 실행하지 않는다. 선택 변경/Play Mode 진입/재로드에 정리한다. 구성 검사는 비저장 미리보기를 제외하고 저장된 공용 인스턴스만 검사한다.

배치 검사는 spec의 배치 검사 기준 표를 따른다. 경로의 관찰 지점은 A 준비 a-w3, 판단 a-left/a-w0, a-right/a-w6, a-lift/a-w4, b-left/b-w0, b-right/b-w6, b-lift/b-w2이며 다음 연습은 A 양쪽 계단 완료 위치에서 각각 검사한다. 화재/연기는 해당 ContentEntry.anchor의 시작 위치를 사용한다. 좌표/방향/자세별 머리 높이를 먼저 기록하고 배율 1/1.5로 같은 조건을 반복한다. 지원하지 않는 자세는 근거와 제외 사유를 남긴다.

### 구현 순서/요구사항 추적

| 순서 | 변경/검증 | 요구사항 |
|---|---|---|
| 1 | 공용 프리팹/앵커/표시 상태, 구성 누락/Rising 입력 실패 검사부터 작성 | FR-001, FR-002, FR-005, FR-007 |
| 2 | 요청 위임/콜백/하강 스냅샷/정지/리셋. 동기 재진입/유지 입력/이전 listener 검사 | FR-006, FR-007, FR-008, FR-009, FR-013 |
| 3 | 전 체험/전 대상 앵커 연결, 기존 Actions 행 제거, 실제 레이/교육 회귀 | FR-001, FR-003, FR-004, FR-006, FR-011, FR-013 |
| 4 | 미리보기/배율/자세/배치 전수 확인, 실제 실행 자료와 제작자 검토 | FR-010, FR-012, SC-001~007 |

상세 데이터는 [data-model.md](data-model.md), 인터페이스는 [contracts/floor-buttons.md](contracts/floor-buttons.md), 실행 절차는 [quickstart.md](quickstart.md)를 따른다. 이번 계획 작성에서 구현/실행 검증을 완료한 것으로 표시하지 않는다.

## Complexity Tracking

헌법 위반 없음. 공용 Canvas 2개는 머리 추종 UI와 공간 고정 판을 분리하는 최소 변경이다. 콘텐츠별 Canvas/별도 EventSystem/새 입력 체계는 만들지 않는다.

## 메뉴 입력 호환성 유지 보수, 2026-11-17

사용자 승인된 후속 복구로 C4/FR-009/SC-005의 기존 왼손 메뉴 동작을 두 컨트롤러 형식에서 보존한다. 단일 기존 액션의 바인딩을 왼손 XR MenuButton usage로 변경하고 실제 연결 장치의 controls를 검사하는 Editor 전용 MenuInputChecks를 추가한다. 공용 UI 상태 전환과 바닥판/교육 API는 유지한다. 새 패키지와 제출 SDK 의존성은 추가하지 않는다. 실제 XR Operator 입력, 기존 바닥판 메뉴 회귀 검사와 실행 영상은 별도 자료에 남긴다.
