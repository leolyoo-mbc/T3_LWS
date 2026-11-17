# Implementation Plan: 자막 가독성과 표시 흐름 개선

**Branch**: `main` | **Date**: 2026-11-18 | **Spec**: [spec.md](spec.md)

**Input**: `specs/009-refine-subtitle-presentation/spec.md`

**→ 경로 해석:** 실제 Git 브랜치는 main이다. setup-plan의 BRANCH=009-refine-subtitle-presentation은 feature 식별자이며 브랜치를 생성하거나 전환하지 않았다.

## Summary

**→ 확정 결과:** 자막만 앞쪽 1m에 배치하고 보이는 크기를 유지한다. 배경을 제거하고 흰 글자와 검정 외곽선을 쓴다. 필수 경고 > 음성 자막 > 일반 행동 안내 순으로 표시하며 긴 자막은 기존 음성에 맞춰 분할한다.

**→ 구현 결정:** 기존 SharedCanvas/ContentPanel/TMP/GuidancePresenter를 재사용한다. ContentPanel 아래 자막 전용 RectTransform 하나를 추가한다. 새 Canvas/추종기/배경 크기 계산기/음원 재생성은 필요 없다.

## Technical Context

| 항목 | 현재 환경 및 결정 |
|---|---|
| Language/Version | C#, Unity 6000.3.26f1 |
| Primary Dependencies | uGUI/TMP 2.0.0, URP 17.3.0, XRI 3.5.1, Input System 1.20.0 |
| Storage | 기존 GuidanceCue/SubtitleSegment 직렬화 자산, 기존 WAV, 자막 전용 머티리얼 |
| Testing | 전용 SubtitlePresentationChecks 구성/Play Mode 검사, 기존 공용 UI/경로/바닥 버튼 회귀, 실제 음성 청취와 Quest |
| Target Platform | 기존 Meta Quest Android VR, Editor/PC 보조 검증 |
| Project Type | 단일 씬의 교육용 VR 앱 |
| Performance Goals | 기존 refresh rate 유지. T002에서 구현 전 5초 예열 후 30초 CPU/GPU frame time을 측정하고 T024에서 동일 조건의 중앙값/95백분위를 비교한다. 매 프레임 텍스트 측정/머티리얼 생성/장애물 검색 없음 |
| Constraints | Assets/_02LWS 안에서 구현, AGENTS.md 수정 금지, Canvas 2개/추종기 1개/안내 재생기 1개 계약 유지 |
| Scale/Scope | 구현된 3개 콘텐츠의 전체 cue. 연기 6개/경로 7개 우선 분할, 콘텐츠 1도 전체 검사 |

## Constitution Check

| Gate | Phase 0 전 | Phase 1 후 |
|---|---|---|
| 사용자 확정과 구현 결정 구분 | PASS | PASS: 아래 구현 설계와 명세 구분 |
| 실제 실행 MP4, 1920×1080 이상/30fps 이상/게임 소리 | PASS: 구현 시 의무 수용 | PASS: quickstart에 캡처/실제 재생 확인 포함 |
| Unity 오디오와 외부 소리/마이크 분리 | PASS | PASS: 분리 검증과 기록 포함 |
| 캡처 불가 시 대체 자료와 한계 보고 | PASS | PASS: 원본 이미지/분리 녹음/미검증 명시 |
| Artifacts/Validation/VisualEvidence 보관, Git 비추적 | PASS | PASS: 경로/검사 명령/대표 파일 링크 포함 |
| 문서 작업의 범위 보존 | PASS | PASS: 이번 실행은 계획 문서만 작성 |

**→ 판정:** 설계 게이트 위반과 미해결 질문은 없다. PASS는 설계 준수 판정이며 구현/청취/Quest 합격이 아니다. 문서 작업에는 새 실행 영상 의무가 없다.

## Project Structure

### Documentation (this feature)

```text
specs/009-refine-subtitle-presentation/
  spec.md
  plan.md
  research.md
  data-model.md
  contracts/subtitle-presentation.md
  quickstart.md
  checklists/requirements.md
```

**→ 후속 산출물:** tasks.md는 speckit-tasks에서 생성한다.

### Source Code (repository root)

```text
Assets/_02LWS/
  Scenes/FireNotification.unity
  Scripts/Runtime/
    SharedContentUI.cs
    SharedUIController.cs
    FireNotificationAdapter.cs
    SmokePostureController.cs
    EvacuationRouteController.cs
    SharedUIContractAudit.cs
  Scripts/Presentation/GuidancePresenter.cs
  Scripts/Validation/SubtitlePresentationChecks.cs   (추가)
  Art/Materials/SubtitleOutline.mat                 (추가)
  Content/GuidanceCatalog.asset
  Data/SmokePosture/GuidanceCatalog.asset
  Data/EvacuationRoute/RouteGuidance.asset
```

**→ Structure Decision:** 콘텐츠는 교육 단계와 경고 의미를 제공하고 공용 뷰가 표시를 결정한다. 폰트 원본/공통 머티리얼/셰이더는 보존한다. 씬/머티리얼은 연결된 Unity Editor에서 기존 참조를 확인한 뒤 수정한다.

## Implementation Design

### 자막 배치와 스타일

**→ 씬:** ContentPanel 아래 SubtitleRoot를 추가하고 기존 SubtitleText만 옮긴다. 기존 GuidancePresenter.subtitle 참조는 유지한다. root에는 Image/Canvas/CanvasGroup/레이캐스터를 추가하지 않는다. 기존 부모의 활성 상태/CanvasGroup을 상속하므로 메뉴/결과/전환 때 함께 숨겨진다. 자막은 raycastTarget=false다.

**→ 거리 계산:** 비율 r=1/1.8=5/9를 쓴다. 원래 자막 행의 최대 폭 1352u/상단 여백 24u와 부모의 상단 기준을 유지하고 root scale=r로 둔다. 추종 기준 머리점을 부모 로컬 좌표 H로 바꾸어 root.localPosition=(1-r)H로 정한다. 원래 자막 좌표 P가 H+r(P-H)가 되므로 시야상 크기와 높이 각도가 유지된다. 현재 offset=(0,-0.35,1.8), 부모 상단=-180u, scale=.001에서는 H=(0,530,-1800), root 위치=(0,235.5556,-800)u다. 기준값에서 계산하고 반복 누적하지 않는다. 실제 머리 위치에 매 프레임 강제로 붙이지 않고 기존 ComfortFollowUI의 추종 기준을 따른다.

**→ 비교 기준:** 변경 전 일반 상태 행을 비운 자막 단독 배치를 같은 문구/100%/150%에서 기록한다. 상태 행 때문에 자막이 밀리는 기존 가변 높이는 새 기준에 포함하지 않는다. 자막 영역 폭은 유지하고 문구에 따른 높이만 측정한다. 일반 안내/메뉴/결과의 거리와 배치는 보존한다.

**→ 외곽선:** 기존 LwsKorean 폰트 머티리얼을 SubtitleOutline.mat로 복제하고 같은 셰이더/atlas를 참조한다. OUTLINE_ON, 흰 FaceColor, 검정 OutlineColor, OutlineWidth=0.15, OutlineSoftness=0, Underlay 비활성으로 시작한다. 공통 머티리얼/ZTest는 수정하지 않는다. TMP 패딩을 갱신하고 150%에서 외곽선 잘림을 검사한다.

**→ 설정과 레이아웃:** ThemedText.Apply 뒤 SharedContentUI.ApplySettings에서 전용 프리셋을 적용하고 재활성화 후에도 유지시킨다. 상태 행 배경/레이아웃은 기존대로 유지하되 자막은 상태 패널의 높이/배경 표시 계산에서 제외한다. 문구/글자 크기/가시성 변경 시만 레이아웃을 측정한다.

### 표시 우선순위와 공용 인터페이스

**→ 상태 종류:** ContentStatusKind { Instruction, Warning }을 추가하고 SetStatus(owner,text,kind=Instruction)로 확장한다. 연기/경로의 추적/구성/전환 실패 분기에 Warning을 명시한다. 일반 문구나 Blocked 전체를 경고로 추측하지 않는다. 기존 콘텐츠 1의 시스템 중단과 메뉴 진입 실패 경로는 보존한다.

**→ 단일 결정 지점:** SharedContentUI가 SubtitlesEnabled 사용자 설정과 최신 status kind를 보관하고 LateUpdate에서 렌더러 enabled를 최종 결정한다. Warning이면 경고만, 자막 켜진 IsPresenting이면 자막만, 그 외에는 현재 행동 안내를 표시한다. IsPresenting은 pause/무자막 간격에 유지해 안내 깜박임을 막는다. 경고 해제 시 현재 데이터로 재계산하며 지난 자막을 쌓아두지 않는다.

**→ 호환성:** ApplySettings(float voice,bool subtitles)의 호출 형태는 유지한다. RouteController 등에서 subtitle.enabled에 직접 쓰는 경로를 없애고 공용 뷰에 일원화한다. GuidancePresenter는 표시 여부와 관계없이 현재 구간 text를 갱신한다. owner 교체/Clear에서 상태 종류와 캐시를 초기화한다. 교육 단계/선택 버튼/기존 음성 중단 규칙은 변경하지 않는다.

### 자막 분할과 재생

**→ 구간 작성:** GuidanceCue/SubtitleSegment 형식과 기존 Validate를 유지한다. 단어 사이 기존 공백에서 문장/의미 단위를 나누고 전체 text/WAV/manifest 해시는 보존한다. 콘텐츠 1의 기존 시각은 재사용하고 긴 문장은 추가 분할한다. 연기/경로는 파형과 실제 청취로 경계를 작성한다. 길이 비례 배분이나 무음 검출만으로 최종 승인하지 않는다.

**→ 재생 기준:** 실제 AudioSource 위치(timeSamples/frequency)로 구간을 선택한다. Play 직후 시작 프레임, 자연 종료 시 재생 위치 초기화, Pause/UnPause/Cancel을 처리하고 기존 run 취소와 완료 콜백 1회 실행을 보존한다. IsPresenting은 무자막 간격/일시정지에도 true이며 종료/취소에 false다. UI 숨김만으로 오디오를 취소하거나 되감지 않는다.

**→ 동기 판정:** 전체 구간의 첫/마지막 발음을 실제로 듣고 음원 내 시각을 기록한다. 실제 자막 표시 시작은 발화 시작 대비 ±0.2초, 종료는 마지막 발음 종료부터 0.3초 이내다. 발화 종료 전 자막을 지우지 않으며 무음/일시정지 구간은 발화에서 제외한다. 현재 AudioSource 위치 기준의 실제 표시 시각과 기준 발화/오차를 함께 기록한다. 일시정지/경고로 의도적으로 숨긴 시간은 정상 표시 동기 측정과 구분하고 표시 전환 계약으로 검증한다.

**→ 성능 기준 확보:** T002에서 같은 실행 환경/기기/refresh rate/해상도/품질/장면/시작 상태/입력 구간을 기록하고 5초 예열 뒤 30초 CPU/GPU frame time의 중앙값/95백분위를 저장한다. T024는 이 조건을 재현해 차이를 보고한다. GPU 측정 미지원이나 기준값 부재는 0으로 대체하지 않고 미검증으로 남긴다. 이번 보완은 비교 절차를 정하며 새 성능 합격 임계값을 도입하지 않는다.

## Validation and Delivery

**→ 검사:** 전체 cue의 원문/해시/타이밍/100%와150% 실제 TMP 두 줄, 1.00m±0.02m와 시야상 차이 5% 이내, 경고/자막 설정/owner 전환/자연 종료/완료 1회를 검증한다. SharedUIContractAudit, RouteIntegrationChecks, FloorButtonChecks를 회귀 실행한다. 기존 검사 중 자막 배치 전제가 달라지는 부분만 새 계약에 맞추고 기존 실패는 별도 기록한다.

**→ 실제 증거:** 전체 음성 청취, 밝은 벽/불/어두운 복도와 두 글자 크기, 서기/숙이기/순간이동을 quickstart에 따라 확인한다. MP4/검증 기록과 Quest 가독성/양안 편안함 결과를 구분한다. 기기나 청취 도구가 없으면 그 항목을 미검증으로 남긴다.

**→ 한계:** 1m 안쪽 장애물 가림은 남는다. 외곽선만으로 읽기 어려운 장면은 실패로 기록하며 승인 없이 배경/강제 전면 표시를 도입하지 않는다. 이번 계획 작성은 구현/빌드/패키지 재생성/커밋/푸시를 포함하지 않는다.
