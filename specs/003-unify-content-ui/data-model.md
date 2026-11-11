# Data Model: 콘텐츠 공통 UI

**Date**: 2026-11-11. 구현 설계이며 기존 교육 도메인 상태를 대체하지 않는다.

## ContentUIConfig — 콘텐츠별 편집 데이터

ScriptableObject로 콘텐츠마다 한 자산을 둔다. contentId, 버튼 label/역할(primary 또는 secondary), 상태 메시지 key/text를 보관한다. cue와 화재 선택지 원문을 복제하지 않는다. 화재 행동은 기존 ActionOptions에서 id/label을 읽고, 음성·자막은 각 GuidanceCatalog에서 읽는다.

검증: contentId는 Navigator 등록 ID와 같아야 한다. 노출할 버튼 label과 상태 text는 비어 있지 않아야 한다. 한 화면의 행동 ID는 중복되지 않아야 한다. 요청한 key/행동이 없으면 누락 상태로 처리한다. 표시 문구에 컨트롤러 메서드명이나 씬 이름을 넣지 않는다.

## ExperienceUIState — 콘텐츠가 제공하는 런타임 상태

| Field | Type | Meaning |
|---|---|---|
| Revision | long | 같은 체험 안에서 행동 문맥이 바뀔 때 증가 |
| StatusText | string | 필수 조작·자세 인식·추적/구성 오류 안내. 자막 설정과 무관 |
| StatusKind | enum | None/Info/Warning/Error. 색 외에 문구로 의미 전달 |
| Actions | UIAction[] | 표시 순서의 행동 목록. 없으면 빈 배열 |

UIAction: Id(string), Label(string), Role(enum Primary/Secondary), Available(bool). 행동 실행 delegate는 데이터 안에 저장하지 않는다. 세션 완료·Blocked·ResultText는 기존 어댑터 계약을 유지한다. 차단 상태에서는 일반 행동을 실행하지 않되 구성 오류의 메뉴 복귀 경로는 유지한다. 런타임 UIState가 편집 자산을 수정하지 않는다.

Revision은 매 프레임 증가시키지 않는다. 행동 목록·대상 단계·가용성이 달라질 때만 증가한다. 상태 문구만 바뀌면 화면만 갱신한다. 재시작은 새 소유권 세대를 부여한다.

## UI 소유권

공유 뷰가 contentId + generation(long)을 소유권으로 관리한다. Navigator 바인딩/해제/재시작 시 generation을 증가시키고 초기화한다. 각 버튼 클릭은 바인딩 generation과 state revision을 캡처하고 실행 직전에 현재 값과 비교한다. 이전 세대, 이전 화면, 현재 목록에 없는 ID, 비가용 행동은 거부한다.

UI 데이터에는 교육 판정·2초 타이머·자세 높이·세션 전이 규칙을 복사하지 않는다.

## Subtitle output

자막은 ExperienceUIState와 분리된 시간 기반 출력이다. GuidancePresenter의 cue와 segments는 기존 구조를 유지한다. 자막 연결은 발급받은 generation에 한해 쓰기/지우기가 가능한 IGuidanceSubtitleSink를 사용한다. 비활성 콘텐츠의 Cancel/OnDisable/완료 콜백은 현재 화면에 쓰지 못한다.

GuidanceCue의 기존 검증: 필수 cue ID/음성/text, 유효 segment 시간, audio 길이 이내, segment text 공백 결합과 cue.text 일치. 화재 cue ID/text/segment 문구/audio 참조는 보존한다. 연기 문구를 수정하면 text/segments/audio/timing을 함께 갱신한다.

## 생명주기

1. 메뉴: 소유자 없음, 체험 패널 내용과 행동 비움.
2. 진입: 이전 Stop/해제 후 새 소유권·자막 연결 → 어댑터 구독 → Begin.
3. 단계 변경: 모델 갱신, 행동 문맥 변경이면 revision 증가/입력 재무장 해제. 재생 중 자막은 일반 모델로 덮지 않음.
4. 일시정지: 소유권과 모델 유지, 공유 UI 숨김/차단, 기존 음성/진행 정지. 재개는 새 선택 요구, 재생 처음부터 시작 금지.
5. 재시작: 이전 Stop → 공유 연결 해제 → 새 소유권 연결 → 기존 Restart. 이벤트가 새 세대로만 전달됨.
6. 종료/실패/비활성화: 구독 해제, Stop, 소유권 폐기, 버튼/자막/상태 정리. 오류는 Navigator 메뉴 오류 또는 공유 오류+복귀 중 실제 진입 상태에 맞는 기존 경로로 표시.

## 디자인/대사 기준 기록

구현 전 캡처에 콘텐츠·단계·글자 설정·환경을 붙인다. 화재 카탈로그는 cue ID/text/segments 문구/audio GUID의 의미 데이터 스냅샷으로 비교하고 .meta도 보존한다. 디자인 대조에는 design.md 절/적용값/변경 이유를 기록한다. 연기 대조에는 원문/수정문/보존한 교육 의미/음성 및 timing 갱신 여부를 기록한다. 런타임 진행 저장 기능은 아니다.
