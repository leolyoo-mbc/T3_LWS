# Quickstart: 자막 표시 검증

**→ 전제:** SubtitlePresentationChecks, 자막 전용 씬 부모와 외곽선 머티리얼이 구현되어 있다. 아래 절차로 현재 변경을 검증한다. 실제 청취와 Quest 합격은 별도다. Unity 6000.3.26f1에서 Assets/_02LWS/Scenes/FireNotification.unity를 열고 기존 입력/음원을 사용한다.

## 1. 변경 전 기준과 연결

**→ 기준 저장:** 구현 전에 전체 cue.text/WAV SHA-256, 일반 안내를 비운 자막 단독 배치의 일반/150% 시야상 크기/아래쪽 각도/머티리얼 참조를 기록한다. 원본은 아래 증거 폴더를 쓴다.

**→ 성능 기준:** T002에서 구현 전에 같은 환경/기기/refresh rate/해상도/품질/장면/시작 상태/입력 구간을 기록한다. 5초 예열 후 30초 동안 CPU/GPU frame time을 각각 측정하고 중앙값/95백분위를 baseline.json에 보관한다. T024에서 같은 조건으로 다시 측정해 차이를 기록한다. GPU 측정 미지원 등은 미검증으로 남기고 Editor와 Quest 수치를 서로 비교 기준으로 사용하지 않는다.

```powershell
unity self-update --check --format json
unity status --project-path E:/projects/T3_LWS --format json
unity command --query "console status" --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli
unity command --query "script eval" --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli
```

**→ CLI:** 오래된 CLI면 최신 버전으로 갱신한 뒤 명령을 다시 확인한다. 발견한 명령의 help를 확인하고 매 unity command에 --caller plugin --skill unity-cli와 프로젝트 경로를 명시한다. 연결 불가면 진단을 기록하고 아래 Editor 메뉴를 사용한다. 검사 Running 종료/Passed/Failed와 Console 오류를 확인한다.

## 2. 구성 검사

**→ 실행:** Editor에서 Tools/LWS/Validate Shared UI, Tools/LWS/Validate Subtitle Configuration을 실행한다. 전체 카탈로그 Validate, 원문/WAV 해시 보존, 일반/150% 실제 TMP lineCount<=2/잘림 0, 전용 머티리얼 격리와 Canvas/추종기 수를 확인한다.

**→ 거리:** 안정된 추종 상태의 같은 문구에서 앞쪽 거리 1.00m±0.02m, 기존 대비 시야상 글자 크기/아래쪽 각도 차이 각각 5% 이내를 확인한다. 메뉴/결과/바닥 버튼/상태 행은 그대로여야 한다. 상태 행이 밀어내던 이전 가변 높이와 자막 단독 기준을 혼동하지 않는다.

## 3. Play Mode 통합

**→ 실행:** Play Mode에 진입해 Tools/LWS/Validate Subtitle Presentation을 실행한다. 자막 켜짐/꺼짐과 재생 중 토글, 무자막 간격, 경고 발생/해제, pause/resume, 취소/자연 종료, owner 교체, 다시하기/메뉴 복귀와 연속 cue 경계를 검사한다.

**→ 예상:** 경고 중에는 경고만, 자막 켜진 재생 중에는 자막만 표시된다. 무자막 간격에 일반 안내가 깜박이지 않는다. 종료 후 현재 행동 안내가 나온다. 자막 꺼짐에는 행동 안내/경고가 유지된다. 경고 해제 후 지난 구간을 재표시하지 않고 완료 콜백은 1회다.

**→ 회귀:** Tools/LWS/Validate Route Configuration, Tools/LWS/Validate Route Integration과 기존 FloorButtonChecks 구성/Play Mode 메뉴를 실행한다. 세 콘텐츠의 실제 진행/선택/메뉴/결과를 확인한다. 자동 입력은 실제 Quest 컨트롤러 검증을 대신하지 않는다.

**→ 보고된 중복:** 실제 화면에서 상태/자막 외의 중복 TMP나 안내 오브젝트도 조사한다. 완전 동일 문장의 중복을 재현하지 못하면 미재현으로 기록한다. 원인 후보 제거만으로 원래 보고 전체 해결을 단정하지 않는다.

## 4. 시각/소리/기기

**→ 가독성:** 밝은 벽/불/어두운 복도 × 일반/150%의 6개 조건에서 자막 전체 문구, 외곽선 잘림/번짐, 배경 제거와 다른 UI 보존을 확인한다. 읽기 실패를 자동 배경 복원으로 감추지 않는다. 1m보다 가까운 오브젝트 가림은 범위 밖이다.

**→ 청취:** 전체 음성을 실제로 듣고 각 구간의 첫 발음 시작/마지막 발음 종료 시각을 음원 기준으로 기록한다. 실제 표시 시작은 발화 시작 대비 ±0.2초, 종료는 마지막 발음 종료부터 0.3초 이내이며 발화 종료 전 자막을 지우지 않는다. cue/구간별 발화 시작/종료, 실제 표시 시작/종료와 오차를 subtitle-review.md에 남긴다. 무음/일시정지는 발화에서 제외하고, 경고/일시정지의 의도적 표시 억제는 별도 전환 검사로 확인한다. 파형/시간 이동 검사만으로 합격하지 않으며 누락/중복/다음 문장 전체 선행 표시 0건과 음원 해시 보존을 확인한다. 실제 청취 불가 구간은 미검증이다.

**→ Quest:** 세 콘텐츠에서 서기/숙이기/머리 회전/순간이동과 두 글자 크기의 양안 편안함/읽기를 확인한다. 기기 미실행은 미검증으로 기록한다. 필요할 때 기존 Android 빌드 절차를 사용하고 현재 문서 작업에서는 빌드하지 않는다.

## 5. 증거와 완료 보고

**→ 저장:** Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-subtitle-presentation/에 execution.mp4, 주요 장면 PNG, 검사 결과와 verification.md를 보관한다. 실제 화면 1920×1080 이상/30fps 이상과 게임 소리 포함 MP4가 기본이다. 낮은 해상도 확대나 프레임 복제로 충족 처리하지 않는다.

**→ 소리 분리:** Unity 오디오만 녹음하고 다른 앱/시스템 소리/마이크를 제외한다. 새 녹화 방식/입력 설정이면 다른 앱이 소리를 내는 동안 Unity 무음/유음 구간으로 분리를 확인한다. 같은 방식의 기존 증거를 재사용하면 기록을 링크한다. 저장한 MP4를 실제 재생해 소리와 화면 동기를 확인한다.

**→ 제한:** 캡처/화질/소리 조건을 못 맞추면 최선의 실제 영상과 원본 PNG/분리 녹음을 보관하고 이유를 적는다. 실행/청취를 못했으면 해당 항목을 미검증으로 기록한다. 원본은 Assets/제출 패키지에 넣지 않는다.

```powershell
git check-ignore -- Artifacts/Validation/VisualEvidence/<실제-폴더>/execution.mp4
git ls-files -- Artifacts/Validation/VisualEvidence/<실제-폴더>
git diff --check
```

**→ 보고:** verification.md에 환경/기기명/입력/검사 합계/해시/배치 수치/실제 해상도와 fps/소리 분리/재생 청취/실패와 미검증을 기록한다. 폴더만 제시하지 말고 실제 MP4 또는 대체 대표 파일을 링크한다. 구조 검사/Editor 화면/청취/Quest 결과를 구분한다.
