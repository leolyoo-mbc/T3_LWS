# Data Model: 공용 자막 표시

## 저장 자산

| Entity | 필드/관계 | 검증 |
|---|---|---|
| GuidanceCue | 기존 id/audio/text/segments | WAV/전체 text 유지 |
| SubtitleSegment | 기존 start/end/text | 0 <= start < end <= audio.length+0.03, 앞 end <= 다음 start, 단일 공백으로 합친 원문 일치 |
| SubtitleOutline | 기존 font/atlas/셰이더, 흰 본문/검정 외곽선 | SubtitleText와 StatusText만 공유, Underlay 비활성 |
| SubtitleRoot | ContentPanel 아래 SubtitleText와 StatusText의 공용 부모 | 기존 이름/필드/GUID 유지, 별도 Canvas/Image 없음, 기준 머리점에서 5/9 비율 이동/축소 |

## 실행 상태

**→ 상태 안내:** owner, text, ContentStatusKind(Instruction/Warning)를 공용 뷰가 보관한다. 기존 SetStatus 호출은 Instruction 기본값이며 오류 분기에 Warning을 명시한다.

**→ 사용자 설정:** SubtitlesEnabled와 voice volume을 보관한다. TMP.enabled는 설정 자체가 아닌 최종 표시 결과다.

**→ 음성 안내:** GuidancePresenter의 현재 cue/run, IsPresenting, paused, 실제 재생 위치/현재 segment text. 무자막 간격과 pause에는 IsPresenting=true, Cancel/자연 종료에 false다. 완료 콜백은 현재 run에서 한 번 실행한다.

## 표시 전환

| 우선순위 | 조건 | 상태 행 | 음성 자막 |
|---|---|---|---|
| 0 | owner 없음/부모 패널 숨김 | 기존 부모 숨김 | 숨김 |
| 1 | 비어 있지 않은 Warning | 경고만, 1m 무배경 스타일 | 숨김 |
| 2 | SubtitlesEnabled && IsPresenting | 숨김/배경 숨김 | 현재 text가 있을 때만, 배경 없음 |
| 3 | 그 외 | 현재 일반 안내가 있으면 1m 무배경 스타일로 표시 | 숨김 |

**→ 경고 해제:** 위 표를 현재 상태로 재계산한다. 지난 자막을 저장하거나 몰아서 보여주지 않는다. 오디오가 멈춰 있으면 그 위치를 따르고 진행했으면 현재 구간을 따른다.

**→ 소유권:** 다른 owner의 갱신을 거부한다. Bind owner 교체/Clear에서 상태 종류/이전 text/레이아웃 캐시를 초기화한다. 기존 실행 ticket/완료 정책을 보존한다.

**→ 레이아웃:** 음성 자막 구간을 같은 최대 폭의 일반/150%에서 검사한다. 두 줄을 넘으면 데이터의 의미 단위를 추가 분할하고 글자 크기를 줄이지 않는다. 원문에 없는 공백을 만들도록 단어 중간을 자르지 않는다. 일반 행동 안내와 필수 경고는 전체 문구를 자연스럽게 줄바꿈하고 두 줄 제한을 적용하지 않는다.

**→ 동기 검증 기록:** 런타임 자산 형식은 유지한다. 검증 기록에는 cue/구간, 실제 청취로 확인한 첫/마지막 발음 시각, 실제 표시 시작/종료와 오차를 남긴다. 표시 시작 오차 ±0.2초, 종료 오차 0~+0.3초를 검사하며 발화 종료 전에는 숨기지 않는다. 무음/일시정지는 발화에서 제외하고 경고의 의도적 숨김은 별도 전환 검사로 확인한다.
