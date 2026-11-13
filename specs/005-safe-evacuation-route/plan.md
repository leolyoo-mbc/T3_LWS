# Implementation Plan: 안전한 대피 경로 판단

**Branch**: 실제 Git `main` 유지 | **Date**: 2026-11-13 | **Spec**: [spec.md](spec.md)

**Input**: 현재 명세와 2026-11-13 명확화 답변. setup-plan의 BRANCH 출력 `005-safe-evacuation-route`는 기능 경로에서 파생된 식별자이며 실제 Git 브랜치가 아니다. 브랜치는 생성하지 않았다. 아래 배치 수치, 구현 구조는 AI 계획 제안이며 사용자 확정 요구와 구분한다.

## Summary

단일 씬의 세 번째 독립 콘텐츠로 학교 복도의 두 구역을 추가한다. 첫 상황에는 확인 방법, 조작법만 안내하고, 두 번째에는 다른 배치에서 판단하게 한다. 두 상황 모두 정답 방향을 미리 알려주지 않는다. 복도 이동은 고정 앵커, 계단/엘리베이터 앞 판단은 기존 공용 UI의 두 선택지를 사용한다. 직접 완료와 우회 완료 모두 인정하며 실제 수행만 기록한다.

순수 C# RouteSession이 판단, 결과를, EvacuationRouteController가 앵커 도착, 공용 화면/음성, 중단, 구역 전환을 담당한다. 새 Canvas, XR rig, EventSystem, 패키지는 만들지 않는다. 이 명령은 Phase 1 설계까지이며 작업 목록은 후속 speckit-tasks에서 생성한다.

## Technical Context

| 항목 | 확인 결과 / 계획 |
|---|---|
| Language/Version | C#, Unity 6000.3.26f1의 기존 컴파일 설정 |
| Primary Dependencies | XRI 3.5.1, Input System 1.20.0, OpenXR 1.17.1, Meta OpenXR 2.5.1, URP 17.3.0, uGUI 2.0.0; manifest 실측 버전 유지 |
| Storage | 씬, 프리팹, 로컬 WAV/자막. 판단 기록은 실행 메모리만 사용 |
| Testing | 순수 도메인 검사, Play Mode 통합 검사, 공용 UI 구조 검사, Android 빌드, 별도 Quest/import 확인 |
| Target Platform | Quest 2 Android VR, Windows Editor/XR Simulator는 개발 검증용 |
| Project Type | 기존 단일 씬 교육 앱의 독립 콘텐츠 추가 |
| Performance Goals | 선택 시 시각 반응 1초 이내를 계획 목표로 사용. 실제 refresh rate/CPU/GPU 시간을 측정. 72 Hz/13.9 ms는 참고값이며 사용자 확정 합격 기준이 아님 |
| Constraints | 납품 자산 Assets/_02LWS 내부, 공용 Canvas/안내 재생기 각 1개, 행동 최대 2개, 오프라인, AGENTS.md 수정 금지 |
| Scale/Scope | 한 사용자, 두 고정 구역, 각 계단 2개와 엘리베이터 1개. 실제 계단 하강/탈출, 119 실습 제외 |

## Constitution Check

| v1.3.0 게이트 | 연구 전 | 설계 후 |
|---|---|---|
| 실제 실행 MP4, 1920×1080 이상, 30fps 이상, 게임 소리 포함 | 검증 의무 수용: PASS | quickstart에 화면/소리 실제 재생 확인 포함: PASS |
| 다른 앱 소리, 마이크 제외, 녹음 방식/설정별 분리 검증 | 수용: PASS | 다른 앱 유음 + Unity 무음/유음 시험, 전체 PC 녹음 금지: PASS |
| 기준 미충족 시 대체 자료, 사유, 한계, 미검증 기록 | 수용: PASS | 대체 절차와 실제 조건 기록 명시: PASS |
| 공통 경로, Git 비추적, 대표 파일 직접 링크 | 수용: PASS | Artifacts/Validation/VisualEvidence 아래 보관과 ignore/추적 검사: PASS |
| 확정 요구, 미확정 제안 구분 | 명세 구분 유지: PASS | 수치, 배치, 음성 선택은 계획 기본값으로 표기: PASS |

설계 적합성 검사이며 실행 검증을 이미 통과했다는 뜻은 아니다. 위반을 요구하는 설계는 없고 문서만 작성하는 이번 단계에는 녹화가 필요하지 않다.

## Project Structure

### Documentation (this feature)

```text
specs/005-safe-evacuation-route/
  spec.md, plan.md, research.md, data-model.md, quickstart.md
  contracts/route-experience.md
  checklists/requirements.md
```

### Source Code (repository root, planned)

```text
Assets/_02LWS/
  Scripts/Domain/EvacuationRoute/RouteSession.cs, RouteTypes.cs
  Scripts/Runtime/EvacuationRouteController.cs
  Scripts/Interaction/RouteWaypoint.cs
  Scripts/Presentation/RouteGuidanceCatalog.cs, RouteSmokePresenter.cs
  Scripts/Validation/RouteDomainChecks.cs, RouteIntegrationChecks.cs
  Prefabs/Environment/RouteTeleportAnchor.prefab, SchoolRouteA.prefab, SchoolRouteB.prefab
  Audio/Voice/EvacuationRoute/  # WAV + VoiceManifest.json
  Data/EvacuationRoute/        # scenario/cue assets
  Scenes/FireNotification.unity
  Documentation/              # integration/authoring/voice provenance
```

**Structure Decision**: 순수 세션은 기존 noEngineReferences 도메인 어셈블리, 나머지는 기존 runtime 어셈블리에 둔다. 검증 코드는 UNITY_EDITOR 조건으로 Player에서 제외하고 결과 로그, 영상은 Assets에 넣지 않는다. tasks.md는 이번 단계에 생성하지 않는다.

## Implementation Design

### 판단, 기록

- [data-model.md](data-model.md)의 상태 전이와 [계약](contracts/route-experience.md)을 따른다. Available 계단의 대피 선택만 상황 완료다. 엘리베이터/SmokeBlocked 선택에는 이동 없이 피드백 후 재판단을 제공한다.
- 대상별 최초 선택은 덮어쓰지 않고 후속 선택을 추가한다. 미방문 대상을 정답/오답으로 기록하지 않는다. 막힌 계단에서 다른 경로 선택 → 확인 영역 이탈 → 다른 이용 가능한 계단 도착, 선택 완료 순서만 우회 수행으로 인정한다.
- ‘다른 경로’ 선택 후 같은 지점에서 UI가 즉시 다시 뜨지 않도록 확인 반경 이탈 전 재진입을 억제한다. 안전한 계단도 나중에 재방문 가능하다. 시선, 방문, 대기시간, 연기 수치로 이해나 성공을 판정하지 않는다.

### 이동, 도착, 중단

- 기존 자유 바닥 TeleportationArea 대신 XRI TeleportationAnchor를 사용한다. RouteWaypoint은 상황/지점/대상 ID, anchor, 양방향 인접 지점을 연결한다. 현재 지점의 인접 앵커만 같은 중립 표시로 활성화해 코너/장거리 건너뛰기를 막는다.
- 실제 locomotionEnded와 추적 유효한 머리의 바닥 투영 위치로 도착을 확인한다. 도착 반경 초기값 0.65 m, 지점 사이 최소 2 m로 두며 Rigidbody trigger에 의존하지 않는다. 현실의 짧은 걸음에 따른 확인 영역 진입/이탈도 같은 위치 검사를 적용한다.
- ContentExperienceAdapter에 `public virtual bool LocomotionAllowed => !Blocked`를 추가하고 navigator의 기존 MovementAllowed에 AND한다. 새 어댑터만 Explore/Decision에서 true를 반환한다. 기존 어댑터는 기본값으로 기존 동작을 유지한다. 일반 선택지를 표시한다는 이유로 Blocked를 켜지 않는다.
- 안내, 피드백, 전환, 결과는 이동을 잠그고 해당 단계 이외 선택을 비운다. 단계 잠금, pause, 추적 상실, Stop/Restart, 구역 교체 시 기존 CancelPending()으로 예약을 취소한다. 비동기 입력은 RunId/Execution/TargetId를 검증한다.
- 일반 pause는 음성 위치 보존, 시스템/추적 복구는 현재 안내 재제공이다. 정상 teleport의 추적 revision 변경은 오류/재시작 조건이 아니다. A/B 이동은 기존 XROrigin 높이 보존 이동과 공용 UI Recenter를 사용한다.
- B 공간 활성화와 시작 위치 이동, UI Recenter가 성공한 뒤 controller가 `TransitionDone(runId, execution)`을 호출한다. 세션은 현재 실행/단계와 차단되지 않은 Transition 상태를 확인해 B 안내로 전이한다. 중복/낡은 호출은 거부하고, 실패 시 B 안내 없이 Configuration 차단과 오류 안내, 메뉴 복귀를 제공한다. 중단 중 거부된 완료는 복구 후 실제 전환 상태를 재확인해 전달한다.

### 공간, 공용 UI, 음성

- RouteSpace 아래 A/B root를 둔다. Edit Mode에는 둘 다 보이고 Play에는 현재 구역만 활성화한다. 단일 씬의 기존 root, 공유 광원, 공용 렌더 설정은 바꾸지 않는다.
- 계획 초기 배치 A: 폭 3 m, 길이 24 m 직선 복도, 중앙 출발, 양 끝 계단, 중앙 부근 엘리베이터, 계단 둘 다 이용 가능. B: 폭 3 m, 각 팔 약 12 m인 꺾인 복도, 한 팔 중간 출발, 두 끝 계단, 코너 부근 엘리베이터. 가까운 끝 계단은 연기로 막고 코너 너머 다른 계단은 연기 없이 접근 가능하게 한다. 구역 회전도 달리해 출발, 계단 방향, 엘리베이터 위치 모두 달라지게 한다.
- 확인 지점은 입구에서 복도 쪽 최소 2 m를 초기값으로 하되 연기 입자 범위와 겹치면 더 물린다. 문 조작 없이 계단 상태가 보이게 만든다. 위 수치는 제작 초기값이며 현실의 안전 거리, 건축 기준이 아니다.
- 기존 CorridorFinish, CC0 실내 재질, 연기 텍스처를 재사용하고 원본을 수정하지 않는 두 배치를 만든다. 계단 입구/참, 엘리베이터 문, 교실 문/학교 표지는 로컬 geometry로 만든다. smoke-only presenter는 기존 자세 세션, 필수 화재 광원에 의존하지 않는다.
- 공용 Bind/SetStatus/SetActions/Clear와 안내 재생기를 사용한다. 첫 완료는 안내가 끝난 뒤 ‘다음 연습’ 1개를 제공하고, B 완료 안내 종료 후에만 ResultVisible/ResultText로 공용 결과를 띄운다. 결과는 상황별 실제 기록과 ‘실제 대피 완료는 아닙니다’를 포함한다.
- 음원은 기존 Supertonic 2 F1, ko, 속도 0.95, 16 steps 로컬 생성 방식을 계획 기본값으로 둔다. 준비, 확인 방법, B 도입, 엘리베이터 피드백, 막힌 계단 피드백, A 완료, 최종 완료 cue를 작성한다. 새 대본은 교육 근거와 대조하고 WAV 길이에 자막을 맞추며 신규 청취 검수는 별도로 받는다. 모델/Python은 납품하지 않는다.

### 검증, 통합 순서

1. 순수 세션: 직접/우회/오답 후 완료, 미방문, 중복, 낡은 입력, 중단, 재방문 억제를 검사한다(FR-007–011, SC-001–003).
2. 어댑터, 앵커: 실제 도착, 인접 이동, 예약 취소, 대상 무효화, 구역 전환, 추적 복구를 검사한다(FR-003–008/014).
3. 공간, 음성, UI: A/B 배치 차이, 연기 시인성, 연기 없는 우회, 기본/150%/자막 끄기, 대본/음원 일치를 확인한다(FR-002/012–016, SC-004–005/007).
4. 세 콘텐츠 회귀, 시각/소리 증거, Android 빌드, Quest, import 결과를 각각 기록한다(FR-001, SC-006). 구체적인 실행 절차는 [quickstart.md](quickstart.md)에 둔다.

## Review Focus

- 대상 변경 뒤 이전 선택 실행 거부: target/실행 번호 불일치 통합 검사.
- 중단 전 예약 이동이 복구 후 실행되지 않음: 예약→중단→복구 위치 불변 검사.
- 새 이동 게이트가 비상벨 이동을 막거나 자세 체험 이동을 켜지 않음: 세 콘텐츠 왕복 검사.
- 이용 가능한 계단에서 탐색을 선택했다가 재방문해도 완료 가능: 이탈 후 재선택 검사.
- 오답 후 완료가 최초 오답을 지우지 않음: 도메인 기록과 공용 결과 표시 대조.
