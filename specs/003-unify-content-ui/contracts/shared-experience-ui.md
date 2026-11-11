# Contract: 공통 체험 UI와 콘텐츠 연결

**Status**: 구현 계획의 내부 계약. 외부 웹 API 없음. 기존 ContentExperienceAdapter의 생명주기·결과 계약을 유지한다.

## 1. 표시·행동 인터페이스

ContentExperienceAdapter에 다음 계약을 추가한다.

- `ExperienceUIState UIState { get; }`: 현재 내용. 첫 Begin 전에는 빈 상태, Stop 뒤에는 빈 상태.
- `void SelectUIAction(string id)`: 현재 UIState에 있는 가용 행동만 실행. UI와 별개로 현재 세션/차단 상태를 재검증한다.
- 기존 `StateChanged`: UI 모델이 달라질 때도 발행. 뷰가 매 프레임 상태를 탐색하지 않도록 한다.
- `void BindSubtitleOutput(IGuidanceSubtitleSink sink)` / `void UnbindSubtitleOutput()`: 어댑터가 자신의 GuidancePresenter에 전달한다. Bind는 Begin 전에, Unbind는 Stop 뒤 수행한다.

`IGuidanceSubtitleSink`는 `SetText(string text)`와 `Clear()`를 제공한다. 뷰가 발급하는 인스턴스가 해당 generation을 캡처하고 현재 소유권이 아니면 호출을 무시한다. 기본 TMP 참조를 여러 재생기에 직접 공유하지 않는다. GuidancePresenter는 voice/Elapsed/Play/Cancel/SetPaused의 기존 의미를 유지하며 subtitle 직접 참조 의존을 sink로 전환한다.

뷰의 `Bind(adapter, contentId)`는 기존 구독/버튼 동작을 정리하고 새 generation·sink를 만든 뒤 초기 상태를 표시한다. `Unbind()`는 소유권 폐기·구독 해제·화면 초기화를 수행한다. 프리팹의 버튼 UnityEvent에 콘텐츠별 컨트롤러를 영구 연결하지 않는다. 표시 데이터 상세는 [data-model.md](../data-model.md)를 따른다.

## 2. 콘텐츠 행동 매핑

| 콘텐츠/상태 | ID | 기존 실행 |
|---|---|---|
| 화재 AwaitAction, 피드백 비활성 | shout / wait | Send(ChooseAction, id), 기존 ActionOptions label 사용 |
| 화재 AwaitNext | continue | Send(Continue), 기존 ‘다음’ 문구 유지 |
| 자세 AwaitingReady | ready | Ready(), 추적/구성/차단 조건 재검증 |
| 체험 중 복귀 | return | RequestReturn(), 기존 이탈 확인 정책 |

그 외 화재 단계와 자세 Practice에서는 교육 행동 버튼을 추가하지 않는다. 결과의 다시하기/복귀는 기존 SharedUIController 경로를 유지한다. 두 콘텐츠의 세션 enum/명령은 뷰로 옮기지 않는다. 자세 오류 중에는 일반 Ready를 실행하지 못하지만 복귀는 가능하다.

## 3. 연결 순서와 실패

Navigator: 이전 어댑터 이벤트 구독 해제 → Stop → 자막 Unbind/뷰 Unbind → 루트 전환 → 새 뷰/자막 Bind 및 이벤트 구독 → Begin → 현재 UIState 표시. Begin 중 예외는 부분 연결까지 해제한 뒤 기존 메뉴 오류 경로로 복귀한다. OnDisable도 동일하게 정리한다.

재시작은 SharedUIController의 확인 실행과 결과 재시작이 공통 바인딩 갱신 헬퍼를 거쳐 소유권을 새로 발급한 뒤 기존 Restart를 호출하게 한다. 기존 도메인 Restart/Stop/Return 의미를 바꾸지 않는다. 단순 화면 갱신으로 음성을 재생하거나 Restart를 호출하지 않는다.

## 4. 입력과 모달

- FreshPressButton에 `ResetForNewContext()`를 추가한다. OnEnable도 이를 사용한다. acceptedPointer/released/armed/busy 상태를 초기화한다.
- 재무장 조건은 PC 마우스와 양쪽 선택 입력이 모두 해제된 상태. 오래 누름/옛 pointer click은 새 행동을 실행하지 못한다.
- 버튼 클릭 시 generation/revision/현재 목록/Available 및 어댑터 상태를 재확인한다. 버튼 리스너는 한 전달 경로만 둔다.
- 공유 CanvasGroup과 버튼은 SharedUIController.BlockWorld의 명시적 대상이다. 모달 중 뒤쪽 공유 버튼·비상벨·텔레포트 실행은 0건이어야 한다.
- 가용성 결정은 UIState와 모달 차단의 결합이다. FreshPressButton의 매 프레임 스타일 갱신이 콘텐츠의 비가용 상태를 다시 활성화하지 않도록 콘텐츠 가용성 입력을 보존한다.

## 5. 화면·설정

- 자막/필수 상태/행동 영역을 구분한다. 같은 대사 이중 표시 금지. 자막 끄기는 음성 자막만 숨기며 오류/준비/인식 상태는 남긴다.
- 기본/150% 모두 문구 전체를 제공한다. 버튼 높이와 패널을 확장하며 AutoSize·말줄임으로 필수 내용을 숨기지 않는다. 실제 한국어 TMP bounds와 화면 캡처를 함께 확인한다.
- 현재 공통 Theme/폰트/버튼 상태와 문서 값을 사용한다. 안내 패널은 명시적 ComfortFollowUI 참조로 등록하고 메뉴/결과는 기존 위치를 유지한다.
- 설정은 모든 콘텐츠 오디오에는 각 어댑터를 통해 적용하되 공통 자막 표시/글자 크기는 한 소유자만 적용한다. 볼륨 변경은 음성을 재시작하지 않는다.
- 물리 비상벨/라벨은 공통 패널에 포함하지 않는다. XR Origin/AudioListener/EventSystem/XRUIInputModule을 추가하지 않는다.

## 6. 대사 보존과 검증 출력

화재 원문·음성은 변경하지 않는다. 연기의 긴 안내만 다듬고 필수 안전 의미를 보존한다. 자막 구간은 화재처럼 문장·의미별로 표시한다. 음성 변경 없이 자막만 축약한 결과를 완료로 간주하지 않는다.

Editor 도구 `LWS.FireNotification.Editor.SharedExperienceUIValidation.RunAll`을 구현한다. 읽기 전용 검증으로 실제 씬 참조·단일 공유 UI·필수 데이터·카탈로그 원문과 timing·중복 콜백/옛 세대 거부를 검사하고 Console에 결과를 출력한다. 실패 시 예외로 batch 실행도 실패시킨다. 실행 모드는 quickstart에 정의한다. 검증을 위해 씬이나 콘텐츠 자산을 자동 재작성하지 않는다.
