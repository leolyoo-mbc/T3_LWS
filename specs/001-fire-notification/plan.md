# Implementation Plan: 콘텐츠 1 — 화재 사실 알리기

**Feature**: `001-fire-notification` | **Date**: 2026-11-04 | **Spec**: [spec.md](spec.md)

**Branch**: setup-plan 반환 식별자는 `001-fire-notification`, 실제 Git 브랜치는 `main`. 이번 작업에서 브랜치를 변경하지 않았다.

## Summary

한 층 복도에서 큰소리로 알리기 → 결과 확인 → 다음 선택 → 상황 복원 → 비상벨 설명 → 가상 손으로 밀어 누르기 → 경보 전파 → 완료·메뉴 복귀를 제공한다. Quest 2 단독·최초 오프라인 실행을 지원한다.

### 결정 상태

- **사용자 확정**: 명세의 FR-001~021, Unity VR와 `_02LWS` 제출 경계. 이번 대화에서 **메뉴 씬·체험 씬 분리**를 선택했다.
- **추가 사용자 확정**: 첫 선택지는 ‘큰소리로 알리기’와 ‘다른 사람이 알릴 때까지 기다리기’ 두 항목. 후자는 research.md의 확정 피드백을 음성·자막으로 제공한다.
- **사용자 확인 — 자료 현황**: 이번 콘텐츠에 사용할 에셋과 음성 자료는 아직 확보하지 않았다. 자료의 제작·조달 방식은 미확정이다.
- **추가 사용자 확정 — 화면 표현**: 단순하면서 실제 복도와 비상벨을 알아볼 수 있는 수준으로 구성한다. 사실적인 모델·조명 재현보다 교육 행동과 조작 대상의 식별을 우선한다. 구체적인 에셋은 별도로 정한다.
- **추가 사용자 확정 — 음성**: 안내·오답 피드백과 플레이어의 “불이야!”는 합성 음성으로 준비한다. 미리 생성한 로컬 음성 파일을 앱에 포함하고 체험 중 온라인 합성을 요구하지 않는다. 특정 서비스·유료 요금제·목소리는 아직 선택하지 않았다.
- **추가 사용자 확정 — 비용**: 에셋과 합성 음성은 무료 범위로 준비한다. 자료 선택 시 앱 배포와 제출용 unitypackage에 포함할 수 있는 이용 조건을 확인하고 필요한 출처·라이선스를 함께 제공한다. 적합한 무료 자료가 없으면 임의로 결제하거나 유료 이용으로 전환하지 않는다.
- **추가 사용자 확정 — 안내 방향·대본**: 차분한 안전교육 안내자가 상황을 함께 살피고 행동의 의미와 이유를 짚어 주는 음성·자막으로 구성한다. content-script.md의 4개 단계 수정 대본 전체를 별도 검토에서 승인받았다.
- **AI 제안**: 아래 모듈, 파일명, 상태 표현, 테스트 구성, 성능 목표. 사용자 확정 사항으로 취급하지 않는다.
- **후속 선정·보정**: 구체적인 무료 리소스와 합성 음성 도구·목소리, 배치와 누름 깊이. 확정된 표현 수준·비용·입력·교육 흐름 안에서 AI가 이용 조건과 기술 적합성을 확인해 선정하고 실기기로 보정한다. 현재 특정 자료의 사용 조건이나 기기 성능이 검증된 것은 아니다.

## Technical Context

**Language/Version**: C#, Unity 6000.3.26f1 지원 컴파일러. 별도 언어 버전 강제 없음.

**Primary Dependencies**: 기존 URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1, XR Management 4.6.1, Input System 1.20.0, uGUI/TMP. 추가 XR SDK 없음.

**Storage**: 로컬 에셋·음성·자막, 메모리 세션 상태. 서버·로그인·영구 진도 없음.

**Testing**: Unity Test Framework 1.6.0. EditMode 단계 전이, PlayMode 입력·취소·씬 전환, Quest 2 단독 실행, 깨끗한 별도 프로젝트 Import 검증.

**Target Platform**: Quest 2 Android 단독 앱. 기존 IL2CPP/ARM64, OpenXR Quest 지원을 기준으로 설치 가능성과 입력을 실기기 검증한다.

**Project Type**: 오프라인 VR 앱, 메뉴·체험 두 씬.

**Performance Goals**: AI 제안으로 72 Hz 설정 시 프레임 예산 13.9 ms를 기준으로 CPU/GPU 프로파일링. 사용자 확정 합격 기준 또는 측정 결과가 아니다.

**Constraints**: 소유 코드·에셋은 `Assets/_02LWS`. 필수 자료 다운로드·마이크·핸드 트래킹 요구 없음. `.unitypackage`와 별도로 패키지/XR/Android 설정 안내 필요. 통합 대상은 추후 지정되므로 현재 버전을 기준으로 조건을 기록하고 지정 후 호환성을 재검증한다.

**Scale/Scope**: 콘텐츠 1, 한 층 복도, 두 알림 행동. 점수·시간 평가 없음. 중단 복귀는 해당 단계 재시작, 메뉴 재진입은 전체 재시작.

## Constitution Check

헌법은 미작성 템플릿이므로 예시 원칙을 규정으로 사용하지 않으며 임의 수정하지 않는다. 다음은 실제 AGENTS.md·명세 제약 점검이다.

| Gate | Phase 0 전 | Phase 1 후 |
|---|---|---|
| AGENTS.md 수정 금지 | 준수 | 수정 없음 |
| 확정·검토·AI 제안 구분 | 준수 | 문서에 명시, 다음 단계는 검토 전 보류 |
| `_02LWS` 제출 | 준수 | 소유 의존성 내부화·Import 검증 포함 |
| 기본 이동 텔레포테이션 | 준수 | 필요한 이동만 제공, 강제 연속 이동 없음 |
| 콘텐츠 1 범위 | 준수 | 후보 2~8·평가·안내 캐릭터 추가 없음 |
| Quest 2·오프라인·입력·중단 | 준수 | 계약·검증 가이드 반영 |

기술 조사상 필수 기술 선택의 미해결 항목은 없다. 문구·리소스·배치 보정은 검토 대상으로 유지한다. 이 점검은 실기기 합격 또는 미작성 헌법의 준수 인증이 아니다.

## Project Structure

### Documentation (this feature)

```text
specs/001-fire-notification/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── content-script.md
├── contracts/experience-contract.md
└── quickstart.md
```

### Source Code (repository root; AI 제안, 아직 생성하지 않음)

```text
Assets/_02LWS/
├── Scenes/              # ContentMenu.unity, FireNotification.unity
├── Scripts/
│   ├── Domain/          # Unity와 분리한 상태·명령 검증
│   ├── Runtime/         # 씬·세션·중단 조정
│   ├── Presentation/    # 음성·자막·주변 인물·상황 복원
│   └── Interaction/     # UI와 비상벨 poke 어댑터
├── Prefabs/             # XR rig, UI, 복도, 인물, 벨
├── Content/             # 안내·행동 선택 데이터
├── Audio/
├── Art/                 # 모델·애니메이션·재질·한글 폰트
├── Settings/            # 소유 Input Actions
├── Tests/EditMode/
├── Tests/PlayMode/
└── Documentation/       # 통합 설정·리소스 출처·라이선스
```

**Structure Decision**: 씬 분리는 사용자 선택이다. 씬마다 XR rig·EventSystem·AudioListener 하나씩 배치하고 Single 로드로 교체하는 구성을 AI가 제안한다. 전역 영속 매니저는 도입하지 않는다. 체험 씬 직접 실행도 첫 안내부터 초기화한다. 메뉴 이동 전 체험을 정리한다.

기존 Starter Assets는 참고하되 폴더 밖 prefab/inputactions/material을 제품에서 그대로 참조하지 않는다. 필요한 소유 의존성을 내부로 복사·재연결하고 라이선스를 유지한다. XRI 전역 interaction layer 설정은 임의 이동하지 않고 통합 문서에 의미 기반 매핑을 기록한다. 필터는 레이어 번호 외에 poke interactor 유형도 검사한다.

## Phase 0: Research

[research.md](research.md)에 기존 구성, 공식 근거, 대안, 선택 상태를 기록했다. 한 씬 방식은 사용자 선택으로 제외했고 XRI poke와 커스텀 물리 버튼을 비교했다.

## Phase 1: Design

- 단일 조정자가 [data-model.md](data-model.md)의 9단계 전이를 관리하고 UI·벨·연출은 명령과 완료 이벤트를 보낸다.
- 단계 진입/재진입마다 executionId를 바꾼다. 지난 실행의 완료는 무시하고 메뉴·중단 시 음성·경보·연출·지연 작업을 취소한다.
- 중단 시 단계 ID와 시작 context를 기억한다. 복귀 시 해당 단계 시작 상태를 재구성한다. 전역 timeScale만으로 중단하지 않는다.
- UI는 world-space ray·trigger, 벨은 poke 축·깊이 통과로 분리한다. 벨 ray·grab·단순 근접으로 성공하지 않는다.
- 음성과 자막을 같은 로컬 cue로 관리한다. NPC의 기본 행동·알림 반응을 분리하며 대피 시작은 추가하지 않는다.
- [계약](contracts/experience-contract.md), [검증 가이드](quickstart.md)에 입력·씬·중단·제출 경계를 기록한다.
- [안내 대본](content-script.md)의 선택지·피드백·수정 안내 대본은 사용자 검토를 거쳐 확정되었다. 텍스트와 음성 참조를 진행 로직에서 분리해 추후 문구·음성 교체를 쉽게 한다.

## Complexity Tracking

정당화할 확정 제약 위반 없음. 네트워크·DB·범용 콘텐츠 프레임워크는 불필요하다.

## Handoff Status

Phase 0·1 설계의 구현 전 사용자 결정 검토를 마쳤다. 씬 분리, 선택지·피드백, 표현 수준, 합성 음성 방식, 무료 범위와 안내 대본을 확정했다. 기술 세부사항은 사용자가 위임한 범위에서 AI 판단으로 기록하며 사용자 개별 선택으로 표현하지 않는다.

현재 추가로 꼭 필요한 사용자 결정은 없다. 리소스 선정·목소리·배치·누름 깊이·연출 길이는 확정 요구 안에서 선정·보정한다. 무료 자료를 확보하지 못하거나 확정 요구를 바꿔야 하는 상황은 별도 확인 대상이다. 통합 프로젝트 지정은 향후 외부 의존성이며 현재 계획 검토를 막지 않는다.

tasks.md, 코드, 씬, 음성 파일, Android 빌드는 생성하지 않았다. 이 검토 완료를 다음 단계 실행 승인으로 간주하지 않으며 여기서 멈춘다.
