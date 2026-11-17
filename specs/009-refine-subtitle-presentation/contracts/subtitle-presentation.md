# UI Contract: 공용 자막과 안내

## 공용 인터페이스

**→ SetStatus:** SharedContentUI.SetStatus(ContentExperienceAdapter owner, string text, ContentStatusKind kind=Instruction). 현재 owner의 최신 상태만 수락한다. 필수 경고는 Warning을 명시하고 문구로 추측하지 않는다.

**→ ApplySettings:** SharedContentUI.ApplySettings(float voice,bool subtitles)의 호출 계약을 유지하고 사용자 SubtitlesEnabled를 보관한다. 외부 호출자는 guidance.subtitle.enabled를 직접 변경하지 않는다. 자막 꺼짐에서 경고가 해제되어도 자막을 켜지 않는다.

**→ GuidancePresenter:** 기존 Play/Cancel/SetPaused/IsPresenting/Elapsed를 유지한다. text 생산과 표시 억제를 구분한다. Elapsed는 실제 재생 위치를 반영하며 pause에서 고정된다. IsPresenting은 pause/무자막 간격에 true이고 종료/취소에 false다. 경고 표시 자체는 Play/Cancel을 호출하지 않는다.

**→ 표시 결정:** 공용 뷰 LateUpdate에서 Warning > 자막 켜진 재생 > 일반 안내 순으로 결정한다. 경고 해제 후 현재 상태로 복원하고 지난 구간을 재표시하지 않는다. 콘텐츠는 교육 단계와 경고 의미를 소유한다.

## 씬과 스타일 호환

**→ 보존 계약:** SharedCanvas/FloorCanvas 이름과 WorldSpace 모드, Canvas 총 2개, ComfortFollowUI 1개, GuidancePresenter 1개, EventSystem 1개와 기존 바닥 버튼 참조를 보존한다.

**→ 자막 부모:** SubtitleRoot는 ContentPanel 자식이고 기존 CanvasGroup/활성 상태를 상속한다. 부모 높이가 바뀌어도 상단 기준을 유지한다. SubtitleText 컴포넌트 참조는 그대로이고 입력 대상이 아니다.

**→ 스타일/설정:** ThemedText의 100%/150%를 유지하고 설정 뒤 전용 프리셋을 적용한다. 다른 텍스트의 공통 머티리얼은 수정하지 않는다. 자막은 상태 패널의 높이/배경 계산에서 제외한다.

**→ 경고 위치:** 기존 상태 행 위치/배경을 유지한다. 메뉴 진입 실패는 기존 메뉴 오류 경로를 보존한다. 1m 배치는 자막만의 변경이다.

## 검증 인터페이스

**→ 신규 검사:** LWS.FireNotification.Validation.SubtitlePresentationChecks.ValidateConfiguration()/RunPlayMode()와 Running/Passed/Failed를 제공한다. Editor 메뉴는 Tools/LWS/Validate Subtitle Configuration, Tools/LWS/Validate Subtitle Presentation으로 정한다. 후자는 Play Mode가 필요하다.

**→ 판정:** 기존 GuidanceCue.Validate 전체 적용, 원문/음원 해시 보존, TMP 두 줄/잘림, 시간 경계와 표시 전환을 검사한다. 구조 검사 합격은 실제 음성 청취/Quest 합격을 뜻하지 않는다.

**→ 동기 합격:** 정상 표시 중 첫 발음 시작 대비 실제 표시 시작 ±0.2초, 마지막 발음 종료 대비 실제 표시 종료 0~+0.3초를 만족해야 한다. 발화 시각은 실제 청취로 확인하고 표시 시각과 함께 기록한다. 경고/일시정지에 따른 의도적 숨김은 동기 실패 대신 해당 표시 전환 계약으로 검사한다.
