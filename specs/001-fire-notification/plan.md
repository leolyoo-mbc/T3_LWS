# Implementation Plan: 콘텐츠 1 — 화재 사실 알리기

**Feature**: `001-fire-notification` | **Date**: 2026-11-04 | **Spec**: [spec.md](spec.md)

**Branch**: setup-plan 반환 식별자는 `001-fire-notification`, 실제 Git 브랜치는 `main`. 이번 작업에서 브랜치를 변경하지 않았다.

## Summary

한 층 복도에서 큰소리로 알리기 → 결과 확인 → 다음 선택 → 상황 복원 → 비상벨 설명 → 가상 손으로 밀어 누르기 → 경보 전파 → 완료·메뉴 복귀를 제공한다. Quest 2 단독·최초 오프라인 실행을 지원한다.

### 결정 상태

- **사용자 확정**: 명세의 FR-001~023, Unity VR와 `_02LWS` 제출 경계. 이번 대화에서 **단일 Unity 씬 안에 주제별 별도 공간을 배치하고 선택한 공간으로 이동**하도록 변경했다(2026-11-05). 이전 씬 분리 결정을 대체한다.
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

**Testing**: Editor Play Mode/Simulator에서 단계 전이·입력·취소·공간 전환을 수동 검증하고 Quest 2 단독 실행 및 별도 프로젝트 Import로 최종 확인한다. 새 자동 테스트 작성은 현재 범위에 포함하지 않는다. 기존 자동 테스트가 있으면 설치된 Unity Test Framework 1.6.0으로 추가 실행한다.

**Target Platform**: Quest 2 Android 단독 앱. 기존 IL2CPP/ARM64, OpenXR Quest 지원을 기준으로 설치 가능성과 입력을 실기기 검증한다.

**Project Type**: 오프라인 VR 앱, 메뉴·체험 공간을 포함한 단일 씬.

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
├── quickstart.md
└── tasks.md
```

### Source Code (repository root; 구현 구조)

```text
Assets/_02LWS/
├── Scenes/              # FireNotification.unity (메뉴 및 독립 체험 공간)
├── Scripts/
│   ├── LWS.FireNotification.Runtime.asmdef  # Runtime·Presentation·Interaction 공통
│   ├── Domain/          # 별도 Domain asmdef, Unity와 분리한 상태·명령 검증
│   ├── Runtime/         # 씬·세션·중단 조정
│   ├── Presentation/    # 음성·자막·주변 인물·상황 복원
│   └── Interaction/     # UI와 비상벨 poke 어댑터
├── Prefabs/             # XR rig, UI, 복도, 인물, 벨
├── Content/             # 안내·행동 선택 데이터
├── Audio/
├── Art/                 # 모델·애니메이션·재질·한글 폰트
├── Settings/            # 소유 Input Actions
└── Documentation/       # 통합 설정·리소스 출처·라이선스
```

**Structure Decision**: 단일 씬·별도 공간은 사용자 선택이다. 구현 방안: FireNotification.unity 안에 MenuSpace(원점)와 FireNotificationSpace(X축 40m)를 배치한다. 공용 XR rig·EventSystem·AudioListener 각 하나와 SpaceNavigation을 유지하고 공간 루트는 상호 배타적으로 활성화한다. 각 시작 앵커로 머리 추적 오프셋을 보정해 이동한다. 씬 실행은 메뉴부터, 콘텐츠 선택은 새 세션으로 첫 안내부터, 메뉴 복귀는 연출·입력·snapshot 취소 후 체험 공간을 끈다. 거리·이름은 기술 판단이다. 기존 ContentMenu.unity는 마이그레이션 원본으로 보존하지만 빌드·배포에서 제외한다. 기능 Build Scene List는 FireNotification 하나이며 무관한 SampleScene은 보존한다.

Scripts 루트의 Runtime assembly가 Domain assembly를 참조하고 Domain은 Runtime을 참조하지 않는다. Domain의 중첩 asmdef가 해당 하위 코드를 분리하며 나머지 Runtime·Presentation·Interaction은 공통 assembly에 포함한다.

공통 복도·화재 표현·인물 기본 행동과 반응 기능·필요한 텔레포테이션은 기반 단계에서 준비한다. US1과 US2는 이 기반을 공유하고 각 행동의 결과 표현을 별도로 연결해 독립 구현·검증한다.

기존 Starter Assets는 참고하되 폴더 밖 prefab/inputactions/material을 제품에서 그대로 참조하지 않는다. 필요한 소유 의존성을 내부로 복사·재연결하고 라이선스를 유지한다. XRI 전역 interaction layer 설정은 임의 이동하지 않고 통합 문서에 의미 기반 매핑을 기록한다. 필터는 레이어 번호 외에 poke interactor 유형도 검사한다.

## Shared systems revision — 2026-11-05

사용자 요구: 공용으로 사용할 수 있는 객체는 공용으로 사용한다. 구현 판단: 씬 루트 SharedSystems에 기존 XR rig(카메라·AudioListener·컨트롤러·입력·XRInteractionManager 포함), LwsEventSystem, SpaceNavigation과 단일 SharedDirectionalLight를 모은다. 기존 ExperienceLifecycle을 별도 SharedLifecycle 객체로 옮겨 앱 중단 신호를 메뉴에서도 감지하고 controller.lifecycle을 재연결한다. 월드 회전과 광량은 보존한다. SharedSystems는 공간 전환 대상에서 제외한다. 화재 Point Light·NPC·벨·안내 패널과 체험 음성/진행은 FireNotificationSpace에 유지한다. 폰트·입력 설정·재질 등 이미 참조로 공유되는 에셋은 복제하지 않는다. 범용 프레임워크·새 콘텐츠·새 공용 매니저 코드는 추가하지 않는다.

## Phase 0: Research

[research.md](research.md)에 기존 구성, 공식 근거, 대안, 선택 상태를 기록했다. 기존 씬 분리를 단일 씬·별도 공간 구성으로 대체했고 XRI poke와 커스텀 물리 버튼을 비교했다.

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

Phase 0·1 설계의 구현 전 사용자 결정 검토를 마쳤다. 단일 씬·별도 공간, 선택지·피드백, 표현 수준, 합성 음성 방식, 무료 범위와 안내 대본을 확정했다. 기술 세부사항은 사용자가 위임한 범위에서 AI 판단으로 기록하며 사용자 개별 선택으로 표현하지 않는다.

현재 추가로 꼭 필요한 사용자 결정은 없다. 리소스 선정·목소리·배치·누름 깊이·연출 길이는 확정 요구 안에서 선정·보정한다. 무료 자료를 확보하지 못하거나 확정 요구를 바꿔야 하는 상황은 별도 확인 대상이다. 통합 프로젝트 지정은 향후 외부 의존성이며 현재 계획 검토를 막지 않는다.

[tasks.md](tasks.md)를 생성했고 사용자 승인에 따라 분석에서 발견한 구조·검증·의존성 불일치를 문서에 반영했다. 초기 구현 이력은 tasks.md의 기존 체크박스에 보존한다. 2026-11-05 사용자 speckit-implement 요청으로 단일 씬 변경 T043~T047을 추가한다. 기기·성능·Import 미검증은 계속 미완료로 남긴다.

## 편집 모드 공간 표시 — 2026-11-05

사용자 확정: 편집 모드에서는 모든 스페이스를 활성화해 배치를 확인한다. 구현은 MenuSpace·FireNotificationSpace를 활성화한 씬을 저장하는 방식이다. Play 시작 시 기존 ContentSceneNavigator.Start가 메뉴만 활성화하고 체험 공간을 끈다. startOnSceneEntry=false를 유지해 교육은 선택 후 시작한다. Play 종료 시 편집 상태의 두 공간 활성화로 복원되는지 확인한다. Play 중 전체 표시 기능은 추가하지 않는다.

## design.md v0.1.1 적용 — 2026-11-05

사용자가 문서 적용과 선행 문서 갱신을 위임했다. 기존 기능에 대한 증분 변경으로 진행하며 완료 이력은 보존한다. FR-024~029/SC-011~013과 US5가 이번 범위다. 문서의 팀 검토 제안값을 프로젝트 초기값으로 사용하며 팀 전체 확정으로 바꾸지 않는다.

### UI와 제출 자원

기존 uGUI/TMP·XRI 구성을 유지하고 엔진/패키지는 올리지 않는다. 자원은 `Assets/_02LWS/UI/` 아래 Theme·Fonts·Icons·Prefabs에 준비한다. design.md의 외부 공통 경로 대신 이 경로를 쓰는 이유는 `_02LWS` 단독 Export 경계다. 공통 배포가 생기면 GUID를 보존하며 한 원본으로 통합한다. AGENTS.md와 원본 design.md는 수정하지 않는다.

Theme은 design.md 5~9절 토큰 전체를 중앙에서 관리한다. Noto Sans CJK KR Regular 원본·라이선스·TMP와 한국어 fallback을 확보하고 기존 NotoSansKR과 동일하다고 가정하지 않는다. 버튼/패널 둥근 배경은 9-slice, 아이콘은 별도 이미지, Regular를 사용한다. 제목 48/패널 제목 40/본문·버튼 32/보조 28u, 여백 32u, 버튼 최소 240×80u·간격 16u를 초기값으로 한다. 일반 메뉴 1000×720u·0.001m/u·거리 1.5m에서 시작하고 긴 문구/150%는 영역 확대·줄바꿈으로 처리한다. 시야각 글리프 0.6도/버튼 짧은 변 약 3도는 실기기 검증 목표이며 TMP 숫자만으로 합격 처리하지 않는다.

기존 선택·다음·복귀 Prefab과 씬 자막을 공통 스타일에 연결한다. 새 Subtitle/Dialogue/Pause/Settings/Confirm/Result 패널은 실제 필요한 화면만 만든다. 완료 기록은 실제 큰소리/벨 수행 정보만 표시한다. 별도 타이머·목표창·점수는 추가하지 않는다. 메뉴를 열 때 수평 시선 기준 위치를 고정하고 재정렬로만 이동한다.

### 진행·입력·중단

공용 UI 조정자는 SharedSystems에서 메뉴 스택과 설정을 소유한다. 공통 표시 코드는 씬 이름/교육 컨트롤러를 직접 참조하지 않고 프로젝트 연결부가 재시작·메뉴 복귀 요청을 처리한다. EventSystem과 XR UI 입력 모듈은 기존 한 벌을 재사용한다. PC는 XR 미실행 마우스 UI 모드를 별도 검증하고 Esc는 체험에서 메뉴 열기, 메뉴 안에서 한 단계 닫기로 동작한다. VR 메뉴는 화면 진입 버튼을 기본으로 하며 실제 앱 입력 바인딩이 있는 경우만 안내한다.

내부 메뉴 일시정지는 전역 timeScale이나 기존 Session.Suspend로 처리하지 않는다. 별도 userPaused 게이트가 명령/완료 커밋·UI 외 상호작용·텔레포트·연출 시간을 차단한다. 교육 음성/경보 AudioSource.Pause/UnPause와 자막의 같은 재생 위치를 보존한다. 모든 연출의 unscaledDeltaTime/코루틴/애니메이션/화재 효과도 게이트에 참여시키고 메뉴 애니메이션만 독립 시간으로 실행한다. 추적과 UI는 계속 작동한다. 메뉴 입력은 뒤의 콘텐츠 입력으로 전파되지 않는다.

시스템 lifecycle 중단은 기존 취소·단계 재진입을 유지한다. 내부 메뉴가 열린 동안 복귀하면 재진입은 계속하기까지 보류한다. 메뉴/시스템 중단의 모든 요인이 해제된 뒤에만 한 번 재진입한다. 확인 취소는 메뉴로 돌아오고, 재시작 확인은 기존 실행을 정리한 뒤 새 세션을 시작한다. 복귀 확인은 연결된 콘텐츠 메뉴로 이동한다. 완료 후 복귀는 확인 없이 이동한다. 모든 재활성 입력은 release 후 새 press를 요구한다.

설정 기본값은 음성/효과음 100%, 자막 켬, 글자 100%, 최대 150%다. 같은 실행 중에만 보존하며 영구 저장은 이번 범위에 추가하지 않는다. 교육 음성과 알림 효과의 음량을 나누고 실제 배경음이 없으면 설정도 숨긴다.

### 검증과 배포

T053~T061 순서로 기존 회귀와 새 UI를 검증한다. Editor 검증과 Quest 2 검증을 구분한다. 최신 소스로 Export·패키지 의존성/meta 대조 후 기존 Artifacts 검증 정보를 갱신한다. T037/T038/T041/T042와 새 실기기 항목은 기기/수신 프로젝트 증거가 없으면 미완료로 남긴다.


## 공간 UI 추종 변경 — 2026-11-05

사용자 승인: 이동·텔레포트·고개 회전 후에도 UI를 읽고 조작할 수 있도록 Meta 공식 자료를 반영한다. 일반 메뉴를 머리에 1:1 고정하지 않고, 계속 찾아야 하는 안내/선택/메뉴 UI만 완만하게 따라오게 한다. 비상벨과 Alarm label은 공간 고정이다. 기존 “열 때 고정하고 수동 재정렬만” 규칙은 이 절로 대체한다.

공식 근거: [Meta HUD Placement](https://github.com/facebook/immersive-web-sdk/blob/main/docs/concepts/spatial-ui/hud.md)는 world-space 기본, 필요한 작은 UI의 dead zone/완만한 수평 추종, head pitch/roll 배제 및 헤드셋 보정을 설명한다(Web SDK API 자체를 Unity에 사용하지 않음). [Spatial UI](https://developers.meta.com/vr/design/hands-3d-best-practices/)와 [Ray casting best practices](https://developers.meta.com/vr/design/raycasting_bp/)는 2026-09-14 갱신: 사용자 지향·roll 고정·식별 가능한 조작 방식·충분한 버튼 간격·상태 피드백. 기존 버튼 스타일/레이 입력은 유지한다.

구현 초기값(AI 기술 판단, 공식 표준 아님): 수평 회전 30도, 머리 이동 .25m 초과 때 추종 시작, 지수 보간 계수 6/s. Origin 위치가 한 프레임에 .5m 초과 이동하면 즉시 새 위치에 재배치한다. 눌린 마우스/양손 XR trigger 동안 일반 추종은 정지하며 텔레포트 재배치는 우선한다. 패널 재활성화·수동 재정렬은 즉시 배치한다. 거리 1.5~1.8m 및 패널별 오프셋은 프로젝트 보정값이다. 작은 텔레포트도 확실하게 처리하도록 텔레포트 완료 신호 연결을 검증한다.

검증: 작은 흔들림 유지, 큰 이동/회전 후 수렴, pitch/roll 미전달, 누름 중 안정화, 텔레포트 및 공간 전환 즉시 배치, 꺼진 패널 재표시, 물리 벨 고정, 기존 메뉴/교육 흐름. Editor 결과와 실제 VR 가독성/불편감 검증은 구분한다.


## 벽 부착형 비상벨 재제작 — 2026-11-05

사용자 확정: 공중에 떠 있는 임시 비상벨과 비현실적인 소리를 교체한다. 소화전함 없는 벽 부착형 발신기를 선택했고 Blender MCP로 직접 모델링하도록 요청했다. 기존 안내 대본·손으로 밀어 누르기·진행 순서는 유지한다. 예전 단순 기하/합성 경보음 선정은 이 변경으로 대체한다.

구현 판단: 국내 원형 발신기·표시등·경종의 외형을 참고해 별도 벽 부착판 위에 자체 모델링한다. 원형 발신기 약 144mm, 경종 약 120mm 외경을 참고하되 특정 제품 복제품·인증품으로 표시하지 않는다. 브랜드/KFI 마크와 제조사 사진은 배포 자산에 복사하지 않는다. 눌리는 중앙 부품을 분리하고 모든 런타임 모델·재질·음원은 Assets/_02LWS 안에 둔다. Blender 편집 원본과 재현 스크립트도 보존하고 Unity에서는 FBX를 사용한다.

실제 전기벨 녹음: Dave Welsh, School Bell - Fire Bell.wav, https://freesound.org/people/Dave%20Welsh/sounds/523317/ (2020-06-21), CC0 https://creativecommons.org/publicdomain/zero/1.0/ . 원저자는 솔레노이드 타격식 Edwards 벨 녹음이라고 설명한다. 국내 특정 모터식 경종과 동일한 녹음이라는 주장은 하지 않는다. 연속 타종 구간을 mono PCM WAV로 편집하고 반복 경계를 crossfade한다. 실제 제품 음압을 헤드셋 출력 목표로 사용하지 않는다.

형상 참고: 국내 제조/공급사 카탈로그 https://waf-e.dubudisk.com/ilshinfa.dubuplus.com/yollam%40naver.com/O18B62L/DubuDisk/www/2021%EC%B9%B4%EB%8B%A4%EB%A1%9C%EA%B7%B8.pdf (원형 발신기 144mm·경종 120mm·표시등 87mm 도면, 검색 색인 확인) 및 https://www.alllitelife.com/page.php?page=sub2_3_7 (발신기/경종/표시등 사진 및 경종 120x50mm 도면 직접 확인). 비율·부착판 배치·눌림 깊이와 음량은 프로젝트 초기 조정값이며 법적 설치 규정/모든 제품의 공통 규격으로 표현하지 않는다.

검증: 실제 벽면 부착, 정면 접근/텔레포트, 분리 누름부와 collider 정합, 표면 접촉·잘못된 입력 거부/유효 깊이 성공, 중단·메뉴·초기화, 전체 교육 순환을 Editor에서 확인한다. 실제 VR의 도달성·식별성·소리 자연스러움 및 Quest 성능은 기기 증거 없이는 미완료다.

## 공용 UI 구조 우선 적용 — 사용자 승인 2026-11-12

현재 콘텐츠 1과 연기 콘텐츠는 SharedSystems/SharedCanvas와 SharedNarration을 공유한다. 이 절은 아래의 과거 콘텐츠별 Canvas/GuidancePresenter/NextPanel/ReturnToMenuPanel 배치 설명보다 우선한다. 콘텐츠는 ContentExperienceAdapter.view에 제목·상태·선택/다음 행동을 연결하고 종료 때 연결을 정리한다. 화면/레이 입력/위치 추적/설정/모달/결과는 공용이며, 교육 진행·주변 사람 반응·물리 벨·공간 경보음은 콘텐츠에 남긴다. 벨 표지는 3D 텍스트로 보존한다.

새 콘텐츠 제작 기준: Assets/_02LWS/Documentation/ContentAuthoring.md. 구현 작업과 검증 추적: specs/002-smoke-low-posture/tasks.md Phase 8(T054~T060). Tools > LWS > Validate Shared UI로 공용화 규칙을 검사한다. AGENTS.md와 기존 기기/제출 미완료 작업은 수정하지 않는다.