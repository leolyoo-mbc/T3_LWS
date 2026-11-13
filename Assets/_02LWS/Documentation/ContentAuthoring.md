# 새 콘텐츠 제작 기준

새 콘텐츠는 기존 공용 화면에 내용과 동작을 연결한다. 교육 진행과 공간 반응만 콘텐츠 안에 만든다.

## 구조와 소유권

```text
SharedSystems
  SharedCanvas         메뉴·안내·버튼·모달·결과 / Canvas 1개
  SharedNarration      음성·자막 재생기 1개
  SharedHeadTracking   공용 XR 추적 입력
  SharedLifecycle      앱·헤드셋 중단 신호
Contents
  MenuSpace
  FireNotificationSpace
  SmokePostureSpace
```

콘텐츠의 어댑터는 ContentExperienceAdapter를 상속하고 view에 씬의 SharedContentUI를 연결한다. Navigator의 contents에 고유 ID·root·anchor·adapter·이동 허용 여부를 등록한다. 콘텐츠 선택 항목은 공용 ContentMenuPanel의 기존 선택 버튼과 같은 프리팹/테마를 사용하고 ContentMenuInput.contentId에 동일 ID를 연결한다. 메뉴 자동 생성 기능을 가정하지 않는다. 공간의 표지판은 3D 텍스트로 만든다.

콘텐츠 안에 Canvas, GuidancePresenter, UI용 AudioSource, FreshPressButton, 전용 Next/Return/Action 입력 컴포넌트를 추가하지 않는다. 공용 화면의 형태를 바꿔야 한다면 공용 UI를 수정하고 두 기존 콘텐츠를 함께 검증한다. 벨·등장인물 등 공간 효과음과 실제 상호작용은 콘텐츠에 둔다.

## 연결 예시

아래는 어댑터 메서드 안에서 사용하는 실제 API 예시다. 준비의 의미와 다음 단계 결정은 콘텐츠가 구현한다.

```csharp
// Begin: 해당 콘텐츠의 표시 이름과 준비 동작을 연결한다.
view.Bind(this, "체험 제목");
view.SetStatus(this, "안내문");
view.SetActions(this, new ContentUIAction("ready", "준비 완료", Ready));

// 선택지: id는 화면 안에서 중복 없이, 문구는 짧게 제공한다.
view.SetActions(this,
    new ContentUIAction("first", "첫 번째 행동", ChooseFirst),
    new ContentUIAction("second", "두 번째 행동", ChooseSecond));

// 조작이 필요 없는 단계는 버튼을 숨긴다.
view.SetActions(this);

// Stop/종료: 해당 소유자의 연결과 재생을 정리한다.
view.Clear(this);
```

SharedContentUI는 버튼을 재사용하고 새 바인딩마다 이전 이벤트를 무효화한다. 비활성 콘텐츠·다른 소유자·이전 바인딩·중단 상태의 호출은 실행하지 않는다. 버튼은 놓았다 다시 누르는 FreshPressButton 정책을 따른다. 표시 글자/상태와 준비/선택/다음 행동만 전달하며 공용 UI에 콘텐츠별 if/switch를 추가하지 않는다.

음성은 활성 콘텐츠에서 view.guidance를 사용한다. 재생 완료 콜백은 콘텐츠의 실행 번호·단계 번호·중단/종료 상태를 확인하고 진행해야 한다. Stop/재시작은 자신의 진행 콜백도 폐기한다. 일반 pause는 음성 위치를 보존하고 시스템 중단 복귀는 현재 안내를 다시 제공한다. 기준 구현은 FireNotificationAdapter와 SmokePostureController다.

음량·자막·150% 글자 설정은 SharedUIController가 공용 화면에 적용한다. 콘텐츠의 ApplySettings는 자기 공간 효과음만 처리한다. 공용 Canvas/추적 공급자/공용 재생기의 Transform을 콘텐츠가 이동하거나 비활성화하지 않는다.

## 완료 검사

Unity 메뉴 `Tools > LWS > Validate Shared UI` 또는 `SharedUIContractAudit.CheckScene()`을 실행한다. 같은 검사는 콘텐츠 진입 전에도 수행된다. Canvas 1개, 안내 재생기 1개, UI 추적기 1개, 콘텐츠 내부 Canvas/안내 재생기/전용 UI 입력 0개여야 한다.

준비/선택/다음과 메뉴 복귀, 콘텐츠 교체 후 이전 버튼의 무효화, pause·시스템 중단·추적 상실, 음량 0·자막 끄기·글자 150%, 두 콘텐츠의 기존 진행을 확인한다. 변경한 UI의 실제 실행 MP4와 verification.md는 헌장 지정 `Artifacts/Validation/VisualEvidence/` 아래에 Git 비추적으로 보관한다. 이 문서나 텍스트 로그만으로 실행 검증을 대신하지 않는다. 실제 Quest 확인과 Editor 합성 입력 결과는 구분한다.

AGENTS.md는 사용자만 수정한다. 새 콘텐츠 제작 시 이 문서와 공용화 검사를 함께 적용한다.

공용 결과 화면은 콘텐츠가 결과를 제공하고 현재 안내 재생까지 끝난 뒤 표시한다. 완료 안내 중 자막을 숨기지 않는다. 버튼 누름은 모달/추적/시스템 차단의 시작과 해제 때 무효화되어 복귀 후 새 누름이 필요하다. 기존 선택지 의미와 문구를 UI 정리 과정에서 줄이거나 바꾸지 않는다.

현재 공용 버튼 줄은 두 기존 콘텐츠에 맞춰 최대 2개 행동을 표시한다. 더 많은 선택지가 필요하면 공용 레이아웃과 150% 읽기 검사를 먼저 확장한다. 콘텐츠별 임시 패널로 우회하지 않는다.

## 체험 중 시야 보존

세 번째 콘텐츠 `safe-evacuation-route`는 EvacuationRouteController와 ScenarioA/B, 고정 RouteWaypoint 그래프를 사용한다. 인접 TeleportationAnchor만 켜고 실제 도착 위치를 확인한다. 일반 선택 단계는 Blocked로 막지 않는다. LocomotionAllowed를 Explore/Decision에서만 허용하며 navigator의 기존 이동 조건과 AND한다. 잠금, 전환, 종료는 예약 이동도 취소한다. 경로 데이터와 실제 앵커 ID/인접 연결을 함께 갱신하고 Validate Route Configuration/Domain/Integration을 실행한다.

공용 Canvas 하나라는 규칙이 안내를 큰 판 하나에 항상 표시하라는 뜻은 아니다. SharedContentUI가 하단에 실제 상태·자막·동작만 배치한다. 비어 있는 행/배경을 남기지 않고 150% 글자는 높이를 늘려 수용한다. 제목과 음성 출처는 메뉴에서 확인한다. 콘텐츠별 고정 설명·제목·큰 배경·별도 Canvas를 추가하지 않는다. 메뉴/설정/확인/결과는 공용 정면 패널을 재사용한다. 새 문구를 넣으면 실제 환경·손·대상과의 겹침, 긴 자막과 선택지, 150%, 자막 끄기, 숙인 상태를 다시 확인한다. 하단 배치만으로 모든 장면의 시야 보존을 보장하지 않는다.
