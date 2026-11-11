# Implementation Plan: 연기 속에서 자세 낮추기

**Feature**: `002-smoke-low-posture` | **Date**: 2026-11-10 | **Spec**: [spec.md](spec.md)

**Git branch**: `main`. setup-plan이 반환한 `002-smoke-low-posture`는 기능 식별자이며 실제 Git 브랜치를 생성하지 않았다.

**Status**: Phase 1 설계 초안. 사용자 확정 방향의 구현 방법을 제시한 AI 기술 제안이다. 구현·실기기 합격이나 다음 단계의 실행 승인을 뜻하지 않는다.

## Summary

기존 단일 씬에 독립적인 SmokePostureSpace를 추가한다. 서서 준비 완료 → 연기가 위로 모이는 원리 관찰 → 실제 몸 낮추기 → 연속 2초 자동 인정 → 한계 안내·결과·메뉴 순서다. 연습 중 연기층 높이는 고정하고 움직임은 유지한다. 실제 탈출·소화기·수건 조작·점수·제한시간은 추가하지 않는다.

기존 ContentSceneNavigator와 SharedUIController가 콘텐츠 1의 구체 타입을 참조하므로 작은 콘텐츠 어댑터를 도입한다. 콘텐츠 1의 교육 단계는 유지하고 새 콘텐츠는 독립적인 세션·자세 판정을 둔다. 머리 높이는 자세의 대리 측정이며 전신 자세 검증이 아니다.

## Technical Context

| 항목 | 관찰한 구성과 설계 |
|---|---|
| Language/Version | C#, Unity 6000.3.26f1. 기존 컴파일러 설정 유지 |
| Primary Dependencies | manifest 기준 URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1, CoreUtils 2.6.0, Input System 1.20.0, uGUI 2.0.0/TMP. 새 패키지/업그레이드 없음 |
| Storage | 실행별 메모리만 사용. 머리 위치·시작 높이·유지 시간의 파일 저장/전송 없음. 기존 음량/자막/글자 설정은 앱 실행 동안 공유 |
| Testing | Editor 전용 입력 주입으로 판정/세션 수동 검증, 씬 연결 Play Mode 검증, Quest 2 단독·오프라인·성능·Import 검증 |
| Target Platform | 기존 Quest 2 Android 단독 앱. IL2CPP/ARM64/SDK 34는 ProjectSettings 관찰값이며 설치 성공 증거가 아님 |
| Project Type | 오프라인 1인 VR 교육 앱, 단일 씬의 메뉴/독립 공간 |
| Performance Goals | 자세 피드백 1초 이내. 2초 유지 완료는 별도 조건. 기존 계획의 72 Hz/13.9 ms는 AI 제안 프로파일링 기준이며 사용자 확정 합격 수치가 아님 |
| Constraints | 실행 코드·자산·테스트·제출 문서는 Assets/_02LWS. 외부 다운로드·마이크·핸드트래킹 불필요. 연습 중 가상 이동 차단, 언제든 중단 가능 |
| Scale/Scope | 콘텐츠 1 유지 + 제자리 자세 체험 1개. 활성 콘텐츠 1개, XR Origin/EventSystem 각 1개 |

조사 항목은 [research.md](research.md)에서 설계 결론으로 처리했다. 초기 튜닝 값은 AI 제안이며 기기 검증에서 조정한다. 교육 안전 기준으로 표시하지 않는다.

## Constitution Check

현재 헌장 v1.1.0을 적용한다. AGENTS.md, 명세, 기존 UI 기준과 제출 조건도 함께 따른다. 아래 시각 검증 게이트는 이후 실행 결과에 영향을 주는 구현·수정에 적용하며, 과거 완료 작업의 소급 녹화와 문서만 수정하는 작업의 새 녹화는 요구하지 않는다.

| 게이트 | Phase 0 전 | Phase 1 후 |
|---|---|---|
| 사용자 확정/AI 제안 구분 | PASS | 확정 체험 유지, 기술 초기값·대본은 제안으로 표시 |
| Assets/_02LWS와 Export 경계 | PASS | 내부 자산, 라이선스·의존성 재검사 계획 |
| 콘텐츠 1 보존 | PASS | 기존 단계 유지, 연결부 변경 후 전체 회귀 |
| 제자리/실제 숙임/고정 연기층 | PASS | 이동 차단, 실제 머리 추적, 연습 중 연기층 하강 금지 |
| 중단/추적 상실/개인정보 | PASS | 미완료 유지 시간 초기화, 기준 공간 검증, 실행 종료 시 폐기 |
| 교육 오해 방지 | PASS | 낮은 자세≠안전/탈출, 2초≠안전 시간, 대피 가능한 경우라는 대본 조건 |
| 실행 시각 자료 | 채택 전 과거 작업은 소급 적용 제외 | 변경 기능의 진입·핵심 동작·결과를 실제 실행 MP4로 기록. 불가능하면 대체 시각 자료와 사유·검증 한계 기록. 실행/자료 확보도 불가능하면 미충족으로 보고 |
| 검증 원본 보관 | 채택 전 과거 작업은 소급 적용 제외 | Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-작업명/에 자료와 verification.md 보관. 기존 폴더 덮어쓰기 금지 |
| Git 비추적 | 채택 전 과거 작업은 소급 적용 제외 | git check-ignore와 git ls-files로 제외 규칙 적용·비추적 확인. Assets/제출 패키지에 검증 원본 포함 금지 |
| 결과 보고 | 채택 전 과거 작업은 소급 적용 제외 | 대표 파일 직접 링크·실제 확인 결과·미검증 환경 명시. 자료 존재만으로 기능 합격 판정 금지 |

PASS는 설계 지침 충족이다. 런타임·실기기·교육 효과·Import 합격은 아직 검증하지 않았다. 정당화할 규칙 위반은 없다.

## Project Structure

### Documentation (this feature)

```text
specs/002-smoke-low-posture/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── contracts/
│   ├── content-integration.md
│   └── experience-ui.md
├── quickstart.md
└── checklists/requirements.md
```

tasks.md는 후속 $speckit-tasks 산출물이며 이번에 생성하지 않는다.

### Source Code (repository root)

아래는 구현 시 적용할 배치 제안이며 이번에 생성하지 않는다.

```text
Assets/_02LWS/
├── Scenes/FireNotification.unity        # SmokePostureSpace 추가
├── Scripts/
│   ├── Domain/SmokePosture/             # 기존 순수 C# asmdef: 세션·판정
│   ├── Runtime/
│   │   ├── ContentSceneNavigator.cs     # 콘텐츠 등록/선택
│   │   ├── SharedUIController.cs        # 활성 콘텐츠 연결
│   │   ├── ContentExperienceAdapter.cs
│   │   ├── FireNotificationAdapter.cs
│   │   └── SmokePostureController.cs
│   ├── Interaction/                    # 메뉴/실제 머리 추적 입력
│   └── Presentation/                   # 연기 연출, 기존 안내 표시 재사용
├── Data/SmokePosture/                   # 튜닝·안내 큐, 생성 전 경로 정합
├── Art/                                # 내부 연기 재질/텍스처
├── Audio/                              # 로컬 한국어 안내·권리 기록
├── UI/                                 # 기존 Theme/Fonts 재사용
└── Documentation/                      # 통합/검증 기록
```

**Structure Decision**: 별도 씬·XR Rig·범용 시나리오 엔진을 만들지 않는다. 기존 asmdef를 유지하며 새 자동 테스트 어셈블리는 만들지 않는다. 새 도메인 namespace는 기존 어셈블리 안의 LWS.FireNotification.Domain.SmokePosture로 한정한다.

## Phase 0 — Research decisions

상세 근거·대안은 [research.md](research.md). 신규 콘텐츠 UI 제작은 [ContentAuthoring.md](../../Assets/_02LWS/Documentation/ContentAuthoring.md)를 따르고 공용화 검사를 통과해야 한다.

1. 기존 메뉴·공통 UI·이동 제공자의 콘텐츠 1 직접 참조를 작은 어댑터로 감싼다. 상태·결과·설정·복귀는 활성 콘텐츠로 전달한다.
2. XROrigin 기준 머리 높이를 사용하고 별도로 위치 추적 유효성을 검사한다. 추적 기준 공간 변경 시 미완료 기준을 폐기한다. PC 가짜 높이는 시험 전용이다.
3. 최초 판정 제안은 시작 높이의 20% 이상 하강, 해제 경계는 진입 경계보다 0.03 m 높은 위치다. 흔들림 완화용 앱 값이며 안전 기준이 아니다. 2초는 사용자 확정값이다.
4. URP 입자 연출로 초기 상승·축적을 보여주고 연습에서는 고정된 상부 영역 안에서 움직인다. 전역 fog·성공 시 연기 제거·시간에 따른 층 하강을 사용하지 않는다.
5. 한국어 음성·자막은 로컬 에셋으로 준비한다. 외부 생성/구매는 이번에 실행하지 않는다. 권리·대본 일치·필수 파일 준비를 구현 작업에 포함한다.

## Phase 1 — Architecture and behavior

### 공통 시스템 연결

ContentEntry는 안정적인 id, 공간 root, 진입 anchor, 어댑터를 가진다. 기존 버튼은 fire-notification, 새 버튼은 smoke-low-posture를 선택한다. 후보 목록 번호와 ID를 분리한다. UI 이벤트가 끝난 다음 프레임에 이전 콘텐츠를 종료하고 공간을 바꾼다. XR Origin 재배치는 메뉴↔공간 진입에만 사용하고 연습 중 카메라를 이동하지 않는다.

SharedUIController의 navigator.controller, 음원 분류, 고정 결과 문구, 자식 UI 탐색을 활성 어댑터로 연결한다. 메뉴에서 활성 세션 없이 설정을 여는 경우도 지원한다. 음량·자막·글자 설정은 두 콘텐츠와 재진입에 유지한다. 기존 자막 끄기는 유지하되 준비·추적 문제·인식·중단 같은 조작 정보는 계속 표시한다. 기본 자막 켬 상태에서 동일 의미의 음성/한국어 자막 제공을 검증한다.

MenuTeleportationProvider의 콘텐츠 1 lifecycle 직접 참조도 제거한다. 연기 공간에는 이동 표면/앵커를 두지 않고 pending 요청을 취소한다. 이동 허용은 콘텐츠 정책 AND 모달 닫힘 AND lifecycle 활성으로 계산한다. 일시정지 해제가 연기 공간의 이동을 되살리지 않게 하고 메뉴/콘텐츠 1 복귀 시 기존 이동 정책을 복원한다.

### 상태와 판정

진입은 AwaitingReady다. 유효한 추적 샘플에서 사용자가 준비 완료를 선택한 순간의 높이를 저장한다. 준비 이전/추적 불가 상태에서는 기준·성공을 만들지 않는다. 헤드셋만으로 서 있는지를 검증했다고 표시하지 않는다.

PrincipleGuidance에서 원리를 설명하고 Practice에서만 유지 시간을 계산한다. 관찰 중 미리 숙인 시간은 합산하지 않는다. 인식 피드백은 1초 이내, 유효한 낮은 자세 연속 2초면 한 번만 완료를 고정하고 즉시 일어서도 된다고 안내한다. CompletionGuidance에서 한계를 설명한 뒤 Result를 표시한다. 이 두 상태는 명세의 완료 안내 단계에 속한다. 개발 빌드에는 quickstart Q4의 경계 통과/피드백 표시 시점 확인 도구를 제공한다. 실제 추적 입력을 관찰할 뿐 합성 입력·진행 변경을 제공하지 않으며 명시적으로 켠 경우에만 표시한다. 측정값은 실행 종료 시 폐기한다.

일시정지·추적 상실·이탈·재시작은 같은 프레임 완료보다 우선한다. 미완료 유지 시간은 0으로 초기화하고 정지 시간을 더하지 않는다. 일반 사용자 일시정지는 음성·자막·입자·교육 진행을 멈추고 같은 지점에서 재개한다. 시스템 중단 복귀에서는 현재 단계 안내와 자세 확인을 반드시 다시 제공한다. 모달이나 다른 차단이 남아 있으면 모두 해제된 뒤 복구한다. 판정 재개 후 유효한 낮은 자세 연속 2초를 새로 확인하되 이미 인정한 자세 완료는 같은 실행에서 취소하지 않는다.

추적 원점/모드 변경·XR Origin 불연속 이동 시 미완료 기준을 폐기하고 AwaitingReady에서 새 준비 선택을 받는다. 단순 UI 재정렬은 XR 기준 변경과 구분한다. 기준이 바뀌지 않은 단순 추적 복구는 안내 후 새 연속 2초부터 확인한다. 세부 필드·전이는 [data-model.md](data-model.md)를 따른다.

### 표현과 교육 문구

연습 중 연기층 경계는 공간에 고정하고 카메라 자식으로 붙이지 않는다. 상부 입자는 계속 움직이고 성공해도 유지된다. 초기 배치는 서 있는 위치에서도 상부 축적을 관찰할 수 있게 하되 더 깊은 숙임을 유도하도록 층을 낮추지 않는다. 공기의 안전이나 독성은 알파/높이로 계산하지 않는다.

기존 ComfortFollowUI는 머리의 수직 이동에도 반응한다. 새 안내는 숙인 상태에서도 읽히게 배치하되 환경/연기층은 따라 움직이지 않는다. 서기·숙이기·150% 글자에서 읽기와 중단 버튼 도달성을 실제 기기로 검증한다. 새 Canvas는 기존 이름 허용 목록에 자동 등록된다고 가정하지 않는다.

화면과 대본 초안은 [experience-ui.md](contracts/experience-ui.md). 실제 대피 안내에는 ‘대피가 가능한 경우’라는 조건을 유지한다. 경로 판단 기능을 추가하는 의미가 아니다. 조사한 국내 공식자료는 자세 판정 수치·고정 연기층·2초를 검증하지 않는다.

### 검증과 제출

[quickstart.md](quickstart.md)의 순서로 판정·씬·기기·Export를 확인한다. Editor 전용 높이·시간·차단 입력으로 연속 시간·상태·중단 우선순위를 수동 검증하고 입력/기대/실제 결과를 기록한다. 새 자동 테스트 코드는 만들지 않으며 기존 관련 자동 테스트가 있으면 추가 실행한다. 콘텐츠 1 전체 순환과 모달 설정을 다시 확인한다.

Export 전 최신 자산의 내부 참조·라이선스를 재검사한다. Documentation/Integration.md 최상단 최신 수정 기록이 아래의 역사적 sample 의존성 설명을 대체한다. UPM/ProjectSettings는 unitypackage만으로 설치되지 않는다. 수신 프로젝트 Import와 Quest 실행 증거가 없으면 해당 검증은 미완료다.

후속 실행 변경의 시각 검증 원본과 `verification.md`는 헌장 지정 경로에 보관한다. verification.md에는 작업 식별자·검증 일시·씬·환경/기기·입력 방식·확인 동작·파일명·결과·미검증 항목과, 대체 자료 사용 시 녹화 불가 사유·한계를 기록한다. 파일 존재·열람 가능 여부 및 Git 제외·비추적 상태를 확인하고 완료 보고에 대표 파일을 직접 연결한다. Assets/_02LWS의 제출 문서에는 결과 요약과 원본 참조를 남긴다.

최신 검증 순서는 T046 복구 수정·T047 음성 검수/반영 → T051-A 검증용 APK → T048 Quest 검증 → T049 성능·T050 학습 평가 → T043 잔여 권리/의존성 점검 → T051-B Export·Import → T052 집계다. T049/T050은 별도 기기·진행자가 있을 때만 병렬 수행한다. 검증 중 실행 코드·자산을 수정하면 영향을 받는 빌드와 검증을 다시 수행한다. T051은 A/B 모두 끝나야 완료이며, 필요한 환경이 없으면 해당 단계는 미완료로 남긴다.

## Requirement coverage

| 요구사항 | 설계/검증 |
|---|---|
| FR-001/011 | 콘텐츠 선택·초기화, Q1/Q6/Q7 |
| FR-002 | 이동 정책·진입 외 카메라 이동 없음, Q2/Q7 |
| FR-003/005 | 초기 축적→고정 경계 입자, Q2 |
| FR-004/010 | 원리/한계 대본·자막, Q3/Q8 |
| FR-006/007 | 개인 기준·상대 판정·연속 2초, Q1/Q4 |
| FR-008/009 | 무제한 대기·중단 우선·유지 시간 초기화, Q2/Q5 |
| FR-012/013 | 제외 기능·로컬 자산·비저장, Q8/Q9 |
| SC-001~006 | quickstart 요구사항 매핑·검증 기록 |

## Complexity Tracking

정당화할 게이트 위반 없음. 연결부는 콘텐츠 직접 참조를 풀기 위한 최소 범위다. 범용 프레임워크·백엔드·영구 저장·패키지 추가는 계획하지 않는다.

## F1 음성 통일 — 2026-11-11

사용자 확정: 연기 자세 안내 6개는 콘텐츠 1과 같은 Supertonic 2 F1으로 통일한다. 기존 대본과 교육 흐름을 유지한다. 한국어, 속도 0.95, 16스텝으로 로컬 사전 생성한 WAV를 사용하며 실제 길이에 자막 종료 시각을 맞춘다. 모델·Python은 제출물에 포함하지 않는다. 2026-11-12 사용자 청취 검수 완료: 6개 모두 발음·억양·대본에 문제가 없다고 확인했다.

## 공용 UI 통합 실행 계획 — 2026-11-12

Goal: 콘텐츠별 UI를 SharedSystems/SharedCanvas 하나로 모으고, 동일 연결 계약을 사용하는 새 콘텐츠 제작 규칙을 적용한다. Spec FR-014~018/SC-007~009 및 현 사용자 승인을 구현한다. 기존 설계의 콘텐츠별 Canvas/별도 안내 재생기/Canvas 이름 탐색은 이 절로 대체한다. Unity/uGUI/XRI/폰트/음원 버전과 교육 진행은 유지한다. 현재 열려 있는 Editor와 기존 미커밋 작업을 보존하고 이 작업에서 커밋/브랜치/패키지 설치를 수행하지 않는다.

Architecture: SharedContentUI가 제목·상태·자막·버튼과 GuidancePresenter/AudioSource를 소유한다. ContentExperienceAdapter를 소유자 키로 Bind/SetStatus/SetActions/Clear를 제공하며, 버튼은 id/label/Action을 받는다. UI는 콘텐츠 구체 타입과 교육 단계를 모른다. 활성 소유자/바인딩 세대/차단 상태를 검사해 교체/중단/닫힌 화면의 콜백을 거부한다. 진행 판단은 각 콘텐츠 세션에 남긴다.

1. 현재 Canvas/재생기/콘텐츠 내부 UI 검사로 실패 기준을 먼저 기록한다.
2. SharedContentUI와 ContentUIAction, 공용화 검사 SharedUIContractAudit를 구현한다. 기존 버튼 prefab/테마와 FreshPressButton의 놓았다 다시 누르기 정책을 사용한다. 버튼은 필요 수만큼 풀로 재사용한다.
3. FireNotificationAdapter가 상태 변경에 따라 선택/다음 동작을 연결하고 SmokePostureController가 준비 동작/상태를 연결한다. 두 콘텐츠는 동일 공용 재생기를 사용한다. 공용 음성 설정은 SharedUIController가 1회 적용하고 효과음만 콘텐츠에 적용한다.
4. Editor API로 SharedCanvas를 만들고 메뉴/안내/모달/결과/메뉴 버튼을 일반 패널로 통합한다. 기존 콘텐츠 UI와 전용 Next/Return/Action 입력 컴포넌트를 제거한다. 벨 표지는 같은 위치의 3D 텍스트로 보존한다. Contents 아래 메뉴/두 공간을 그룹화하고 추적 공급자는 SharedSystems로 이동한다.
5. ContentAuthoring.md에 최소 연결 예시·소유권·검사 실행·시각 검증 기준을 기록하고 현재 구현과 대조한다. 공용화 검사는 진입 시 및 Editor 메뉴에서 실행 가능하게 한다.
6. 저장/Play 계층 검사, 임시 Canvas 위반 검출, 버튼의 오래된 연결·다중 클릭·pause/시스템/추적 중 차단, 두 콘텐츠 순환과 기존 벨/자세 판정 회귀, 150% 읽기·음량/자막 설정, 실제 실행 MP4를 확인한다. 도메인 84검사와 시스템 복구 42검사를 수정된 공용 UI에 맞춰 수행한다.
7. 변경 범위를 별도 리뷰하고 컴파일/개발 APK/아카이브 해시를 확인한다. 실제 Quest·학습·최종 receiver 검증은 기존 미완료 항목으로 유지한다.

Review focus: 공유 재생기를 이전 콘텐츠 종료가 취소하는 오류, 한 버튼을 누른 채 화면 교체 시 다음 화면 오작동, 모달 뒤 클릭/이동, 시스템 중단 복구 안내의 소유자 혼동, Canvas 통합 후 벨 표지/150% UI 표시 손실.
## 체험 UI 시야 보존 수정 — 2026-11-12

사용자 승인: 공용 Canvas를 유지하며 앞을 가리는 체험 안내를 수정한다. design.md의 시야 보존·하단 대화 원칙을 적용한다. 구현 수치는 조정 가능한 배치값이며 기기 검증 합격값이 아니다.

SharedContentUI의 체험 배경을 고정 1300×1060u/1.25배에서 폭1400u/1배·내용 높이로 바꾼다. 상단 pivot을 고정하고 SharedCanvas 중심보다180u 아래에 배치한다. 기존 Canvas의 머리 대비 -0.35m/앞1.8m를 유지하므로 체험 패널 상단은 머리보다0.53m 아래(약16.4도)다. 비어 있는 상태/자막 행은 공간을 차지하지 않으며 아무 내용·버튼도 없으면 배경을 숨긴다. 중복 제목과 콘텐츠2 전용 고정 설명은 체험 패널에서 숨기고 메뉴의 이름/음성 출처는 유지한다. 글자 크기를 축소하지 않고 150%에 맞춰 높이를 늘린다. 메뉴·모달·결과는 정면 배치를 유지한다.

검증: 수정 전 상단+662.5u의 실패 기록 → Editor API 씬 수정 → 두 콘텐츠 실제 진행 영상 및 상태/자막/버튼150% 잘림·상단 고정·빈 배경·모달 차단 검사. Quest 가독성·목 숙임 편안함·수신 프로젝트 검증은 기존 미완료로 남긴다. 이전 Android APK는 이 수정이 포함되지 않은 이력 빌드다.
