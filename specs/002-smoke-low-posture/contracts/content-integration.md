# Contract: 콘텐츠와 공통 시스템

**Status**: 구현 시 사용할 AI 인터페이스 설계 제안. 공개 웹 API가 아니라 메뉴/공통 UI와 콘텐츠 사이의 실행 계약이다. 아직 구현된 타입/API가 아니다.

## 책임과 연결

Navigator는 콘텐츠 등록·활성 항목·공간 전환을 소유한다. 콘텐츠 어댑터는 자기 세션·안내·결과를 소유한다. SharedUIController는 공통 모달·설정·입력 차단을 소유한다. XR Origin과 EventSystem은 공유하며 콘텐츠 root 비활성화에 따라 꺼지지 않는다.

| 제안 동작 | 계약 |
|---|---|
| StartContent(id) | 유효한 등록 ID만 시작. 로딩/이미 실행 중 중복 요청 거부. 기존 무인자 StartContent()는 콘텐츠 1 시작 호환 유지 |
| Begin | 새 runId로 시작, 중복 호출로 실행을 두 번 만들지 않음 |
| SetUserPaused(bool) | 자기 음성/자막/연출/진행 차단 적용. 연기 미완료 유지 시간 초기화 |
| Restart | 현재 실행·콜백 취소 후 새 시작 기준 확인. 공통 설정 보존 |
| RequestReturn | 기존 확인창 정책에 따라 이탈 확인 요청. 모달 중 판정 차단 |
| ReturnToMenu / Stop | 자기 실행·안내·연기·입력을 정리. 여러 번 호출해도 안전 |
| ApplySettings | 음성/효과음/자막/글자 크기 적용. 새 콘텐츠 활성화에도 현재 설정 즉시 전달 |
| StateChanged | 현재 실행의 상태만 통지. 비활성 어댑터 구독 해제 |
| MenuRequested | 활성 콘텐츠의 요청만 수락 |
| ResultView | 제목·실제 수행/관찰 기록·재시작/복귀 가능 여부. 콘텐츠별 문자열, 점수/탈출 성공 추가 금지 |
| Blocked / LocomotionAllowed | 활성 콘텐츠 차단 상태와 콘텐츠 이동 정책. 메뉴의 null 활성 항목도 안전하게 처리 |

실제 C# 형태는 직렬화 가능한 MonoBehaviour 기반 추상 어댑터로 시작한다. FireNotificationAdapter는 기존 FireNotificationController 메서드/이벤트를 전달한다. SmokePostureController는 독립 세션을 연결한다. 공통 코드에서 FireNotificationController의 구체 Stage를 새 콘텐츠 결과 판정에 사용하지 않는다.

## 전환 순서

1. 선택 입력을 한 번만 수락하고 Loading=true. 현재 UI 이벤트 처리 종료까지 한 프레임 기다린다.
2. 이전 콘텐츠 Stop·구독 해제, 관련 루트 비활성, 예약 이동 취소.
3. 공유 XR Origin을 진입 anchor에 기존 높이 보존 방식으로 배치. 아직 자세 기준은 없다.
4. 새 활성 항목 등록·정책/설정 적용·root 활성·Begin. 입력을 새로 놓았다 누르는 조건 유지.
5. 메뉴 복귀는 Stop 후 콘텐츠 root 비활성, 메뉴 활성, 기존 이동 정책 복원, 활성 항목 비움.

등록 참조 누락·중복 ID는 시작을 차단하고 메뉴에서 문제를 표시한다. 누락된 콘텐츠를 완료로 표시하지 않는다. 전환 중 두 root가 동시에 입력을 받거나 두 EventSystem이 활성화되면 검증 실패다.

## 이동/모달 합성

텔레포트 요청 허용 = 현재 공간의 이동 허용 AND 모달 미표시 AND lifecycle 허용 AND 전환 중 아님. 연기 공간은 이동 허용=false다. 모달을 닫으며 저장된 enabled 값을 무조건 복원하지 않는다. pending 요청은 진입/모달/이탈 때 취소한다. 회전/연속 이동 제공자도 존재 여부를 조사해 연기 체험 중 비활성화한다. 실제 머리·손 추적은 차단하지 않는다.

모달은 뒤쪽 콘텐츠 입력을 막는다. 일반 pause는 같은 음성 위치와 자막을 유지한다. 추적 상실은 공통 lifecycle 신호와 별개 차단 이유이며 연기 입력부에서 전달한다. 완료 확정 전에 모든 차단 사유를 다시 확인한다.

## 공통 설정과 자원

각 콘텐츠가 음성·효과음·자막·테마 텍스트 대상을 등록한다. 최초 콘텐츠 root에서 수집한 AudioSource 목록만 재사용하지 않는다. 음량은 원래 음원 볼륨×설정 배율로 계산해 반복 적용 시 계속 줄어들지 않게 한다. 자막 끄기는 기존 설정이며 필수 조작/오류 문구를 숨기는 기능이 아니다. 글자 150%에서 잘림·버튼 중첩을 확인한다.

메뉴 상태에서 설정을 열어도 콘텐츠 1 세션이 있다고 가정하지 않는다. 두 콘텐츠의 재시작·교체로 공통 설정을 초기화하지 않는다. 새 Canvas는 기존 이름 허용 목록에 명시 추가하거나 컴포넌트 참조로 등록한다.

## 회귀 불변 조건

콘텐츠 1의 큰소리 알림 선택·주변 반응·상황 복원·직접 벨 누르기·경보·완료 순서는 유지한다. 물리 벨을 UI 트리거 선택으로 바꾸지 않는다. 메뉴/음량/자막/글자/일시정지/재시작/복귀/콘텐츠 1의 기존 이동을 전체 순환에서 확인한다. 연기 체험 진입 전후 모두 통과해야 한다.

## Approved shared UI contract — 2026-11-12

ContentExperienceAdapter.view references SharedContentUI. Bind(owner,title) selects one content; SetStatus(owner,text) updates operational text; SetActions(owner,ContentUIAction[]) supplies stable action IDs, labels and callbacks; Clear(owner) invalidates bindings and presentation on exit. Shared view rejects stale owner/generation and blocked input. Content owns stage progression, cue selection and completion callbacks; common GuidancePresenter owns playback/subtitles. SharedUIController owns common settings/modal/result/menu visibility. No UI class references FireNotificationController or SmokePostureController. All UI panels share one Canvas; gameplay text labels are 3D text. This replaces earlier per-content Canvas registration/input adapters.

Result display waits for both ResultVisible and GuidancePresenter.IsPresenting=false so completion narration retains its captions. Common button presses are invalidated on blocker transitions, including tracking/lifecycle changes, not just when panels are rebound. Existing authored action labels are preserved.

Current shared action row capacity is two choices, matching both existing contents; more choices require extending the common layout and 150% checks first. Buttons are pooled and reuse the same presenter contract. This is a current layout limit, not a restriction on future educational content.
