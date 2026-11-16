# Quickstart: 바닥 버튼 구현 후 검증

현재는 계획 문서다. 아래 신규 FloorButtonChecks 클래스/메서드는 구현할 인터페이스이며 지금 존재하거나 통과한 명령이 아니다.

## 준비/구성

Unity 6000.3.26f1로 프로젝트와 `Assets/_02LWS/Scenes/FireNotification.unity`를 연다. 기존 패키지/입력/세 체험을 유지하고 공용 FloorCanvas/앵커 연결을 구현한 뒤 검사한다.

```powershell
unity recompile --project-path E:/projects/T3_LWS --format json
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'LWS.FireNotification.Runtime.SharedUIContractAudit.CheckScene(); LWS.FireNotification.Validation.FloorButtonChecks.ValidateConfiguration();' --format json
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'LWS.FireNotification.Validation.RouteDomainChecks.Run();' --format json
```

예상: 컴파일 오류 0, 지정 Canvas 2개/내레이터 1개/EventSystem 1개/콘텐츠별 UI 복제 0개/앵커 오류 0, 기존 Domain PASS.

## Editor 미리보기

전 앵커 Inspector의 숨김/상승/해제에서 한 버튼/두 버튼, 배율 1/1.5를 확인하고 위치/회전/크기를 조정한다. 선택 변경/Play Mode/재로드에 임시 객체가 없어야 한다. 저장/재열기 후 Canvas 2개를 재검사한다. 실제 수치와 제작자의 검토/수정 요청은 verification.md에 기록한다.

## Play Mode

### 배치 검사표

검사 전 아래 위치의 실제 월드 좌표/방향과 자세별 머리 높이를 verification.md에 고정 기록한다. 각 조합을 기본 100%/확대 150%로 반복하고 가려지면 안 되는 대상과 버튼 문구 전체가 함께 보이는지 확인한다. 기존에 지원하지 않는 자세는 근거와 제외 사유를 적으며 합격으로 계산하지 않는다.

| 버튼 | 관찰 지점 | 자세 | 가리면 안 되는 대상 |
|---|---|---|---|
| 화재 선택지/다음 | fire-notification ContentEntry.anchor의 시작 위치 | 기존 지원 서기/앉기 | 화재 상황/비상벨 |
| 연기 ready | smoke-low-posture ContentEntry.anchor의 시작 위치 | 기존 안내대로 서기 | 관찰할 연기 |
| 경로 ready | a-w3 | 기존 지원 서기/앉기 | 주변 대피 경로 |
| a-left/a-right/a-lift 선택지 | 각각 a-w0/a-w6/a-w4 | 기존 지원 서기/앉기 | 해당 출입구/판단에 필요한 연기 |
| b-left/b-right/b-lift 선택지 | 각각 b-w0/b-w6/b-w2 | 기존 지원 서기/앉기 | 해당 출입구/판단에 필요한 연기 |
| 다음 연습 | a-left 완료의 a-w0, a-right 완료의 a-w6를 각각 검사 | 기존 지원 서기/앉기 | 완료한 계단 출입구 |

화재 다음은 버튼 표시 시점의 시작 관찰 위치에서 검사하며 이후 비상벨 접근/조작 조건을 바꾸지 않는다. 실제 기기와 Editor 합성 머리 높이는 서로 구별한다.

```powershell
unity command editor.playmode start --project-path E:/projects/T3_LWS --format json
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'LWS.FireNotification.Validation.FloorButtonChecks.RunPlayMode();' --format json
```

신규 검사 클래스는 Running/Passed/Failed/EvidenceDirectory를 노출한다. 시작 호출 반환만으로 통과 처리하지 않고 `Running==false && Failed==0`과 최종 로그를 확인한다. 전환 중 연결 실패는 Play Mode 완료 후 재시도한다. 기존 RouteIntegrationChecks.RunPlayMode도 Ready 대기/새 버튼 경로로 갱신 후 실행한다.

| 검사 | 기대 결과 | 대응 |
|---|---|---|
| Q1 전 대상 | 화재 shout/wait/next, 연기 ready, 경로 ready/6개 대상 use+another/A 완료 next의 상승/대기/하강. 이전 화면 버튼 중복 0 | SC-001 |
| Q2 배치 | 위 표의 전 관찰 지점/기록한 머리 높이/지원 자세와 배율 1/1.5. 사건 가림/관통/잘림/겹침/통로 침범 0. 이동 후 위치 고정 | SC-002 |
| Q3 행동/입력 | 두 선택 결과/오답 재선택/준비 높이/next 보존. nextpractice는 현재 A에서 하강 완료 전 이동 0/완료 후 이동 1회. Rising 유지/연타/좌우 동시/Lowering/이전 listener 오실행 0. 클릭 순간 높이 저장과 유효 입력 없는 자동 진행 0 | SC-003 |
| Q4 방향 | 반대 방향에서 진입해 정상 등장, 자유롭게 돌아봐 선택, 시선 조건 차단 0 | SC-004 |
| Q5 수명 | Rising/Ready/Lowering 각각 Menu pause/resume, Esc, restart/exit/다른 체험/B 전환. 진행률/음성 위치 보존, 이전 판/입력 잔류 0 | SC-005 |
| Q6 제외 | 자막/상태/선택 화면/일시정지/설정/확인/결과 연출 오적용 0. 글자 설정/재정렬/텔레포트 후 앵커 고정 | SC-006 |
| Q7 증거 | 실제 등장 전/상승/대기/선택/하강 식별, 하강 중 추가 실행 0, 제작자 검토/수정 요청 기록 | SC-007 |

추가 경계: 콜백의 동기 빈 요청+새 요청, 기존 컨트롤러의 선택 거부/같은 단계 유지, tracking/focus 복구, 비활성 B 앵커 구성, 프리뷰 저장/재로드. 최신 요청만 남고 이전 ticket은 무효여야 한다.

다음 연습 하강 중 Menu pause에서는 판과 이동이 함께 기다리고 재개 후 하강을 마쳐 한 번만 이동해야 한다. 재시작/종료 중에는 대기 이동을 취소한다. ResetDisplay로 Hidden이 됐다는 이유만으로 B로 이동하면 실패다. 정상 전환 후 B 소개가 시작되는 기존 순서는 유지한다.

## 실제 레이/Quest

한 버튼/두 버튼 유형을 양손 레이와 트리거로 실행한다. 실제 TrackedDeviceGraphicRaycaster 적중과 이벤트 모듈을 통한 콜백 1회를 확인한다. 벽 뒤 클릭은 막혀야 한다. pointer 메서드 직접 호출을 실제 레이 결과로 대신하지 않는다.

Quest는 전 배치의 가독성/거리/사건 가림/기존 지원 자세/입력 유지/메뉴 복귀를 확인한다. 연기 체험의 서기 준비 조건을 보존하고 미지원 자세까지 합격 범위를 확대하지 않는다. 같은 기기/설정/구간의 변경 전후 CPU/GPU frame time을 기록한다. 기기가 없으면 Quest는 미검증이다.

## 녹화/완료

새 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/`에 execution.mp4, 단계별 스크린샷, verification.md를 보관한다. 실제 1920x1080 이상/30fps 이상으로 캡처하고 낮은 해상도 확대/프레임 복제를 합격 처리하지 않는다. 세 체험의 대표 상승/선택/하강과 오답/진행/일시정지를 식별 가능하게 담는다.

Unity 게임 소리만 포함하고 다른 앱/마이크를 제외한다. 녹화 방식/입력 설정이 처음이거나 변경되면 다른 앱이 소리를 내는 동안 Unity 무음/유음 구간을 녹음해 분리를 확인한다. 같은 설정의 기존 검증 재사용은 정확한 기록을 가리킨다. 저장 영상을 직접 재생해 소리/화면 동기를 확인한다. 트랙 존재/파형만으로 소리 합격 처리하지 않는다.

불가 시 가능한 최선의 영상, 상승 전/중/후/선택/하강 실제 스크린샷, 필요한 분리 실행 녹음을 남기고 사유/실제 조건/미검증을 적는다. 실행 불가면 실제 Editor 자료와 실행 미검증을 명시한다. 자료도 없으면 시각 검증 완료를 선언하지 않는다.

verification.md에 환경/기기/입력, 대상별 결과, 실제 수치/제작자 검토, 해상도/FPS/소리 분리/직접 재생 결과, 미검증을 기록한다. 아래 timestamp는 실제 이름으로 치환한다.

```powershell
git check-ignore -- Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/execution.mp4
git ls-files -- Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-floor-choice-buttons/
unity command editor.playmode stop --project-path E:/projects/T3_LWS --format json
git diff --check
```

예상: ignore 경로가 출력되고 ls-files는 비어 있음, 저장 씬 감사 오류 0, 공백 오류 0. 자료를 강제 추가하거나 제출 자산에 넣지 않는다. 완료 보고는 대표 MP4/대체 파일을 직접 링크하고 Editor/Quest/소리 미검증을 구분한다. 사용자 검토 전 외관을 승인 완료로 표시하지 않는다.

## 구현 후 바로 확인하기

`FireNotification.unity`에서 Play를 누르고 체험을 선택한다. 선택지/준비 완료/다음/다음 연습은 바닥판, 자막/상태/일시정지/설정/결과는 기존 SharedCanvas를 사용한다.

편집 위치:

- 공용 모양: `SharedSystems/SharedFloorButtons/FloorCanvas/FloorViewport/MovingPanel`과 `Assets/_02LWS/Prefabs/UI/FloorButtonPanel.prefab`.
- 개별 위치: 각 콘텐츠 공간의 `FloorButtonAnchors`. 경로는 A/B 공간 아래에 각각 있다. 앵커를 선택해 Inspector의 숨김 위치/완전 상승/해제 미리보기로 확인한다. 선택 해제/Play 진입/스크립트 재로드에 미리보기가 제거된다.
- 준비 완료 앵커는 `readyAnchor`, 화재는 `choiceAnchor/nextAnchor`, 경로 판단은 `floorTargets`의 targetId별 참조다. 새 경로 대상은 구성 검사를 먼저 확장해야 한다.
- 위치/방향은 수동 편집한다. 고개나 컨트롤러 방향에 자동으로 맞추지 않는다. 기본 하단0.60m/상승0.35초/하강0.25초는 수정 가능한 초기값이다.

`Tools/LWS/Validate Floor Buttons`는 저장 구성 검사다. Play Mode의 `Tools/LWS/Run Floor Button Play Mode Checks`는 합성 입력 회귀 검사이며 종료 시 선택 화면으로 돌아간다. `FloorButtonChecks.Running/Passed/Failed/EvidenceDirectory`로 완료를 확인한다. 실제 헤드셋 검사를 대신하지 않는다.

`ContentExperienceAdapter.ActionExecution`은 같은 문구를 다시 제공해도 교육 실행이 바뀌면 이전 listener를 무효화하는 경계다. 기존 Session 실행 번호를 노출하며 Domain 판단을 바꾸지 않는다. 정상 수락 판의 하강 완료와 `ResetDisplay`를 구별해야 다음 연습이 조기에 이동하지 않는다.

2026-11-16 Editor 검증 자료: `Artifacts/Validation/VisualEvidence/20261116-194559-floor-choice-buttons/`. 실제 영상, 46개 배치 원본, 검사 로그, 상세 한계는 해당 폴더의 `verification.md`를 따른다. Quest/제작자 외관 승인/직접 청취는 별도다.

## Convergence 보완 검사 T035~T037

`FloorButtonChecks.RunPlayMode()`는 기존 검사에 반대 방향 진입, non-owner/owner 교체, 실제 Input System Esc, 상태별 재시작/종료, 하강 중 다음 연습 취소, 기울기 20개 조합을 포함한다. 검사 각도는 pitch/roll +/-15도와 복합15도, 버튼 1개/2개, 글자 100%/150%의 검사 입력이며 사용자 확정 외형이나 지원 각도 제한이 아니다. 저장 앵커의 기본 0도는 유지한다.

`EvidenceDirectory`는 실행을 시작할 때 실제 고유 결과 경로로 갱신된다. 같은 요청 경로가 이미 있으면 `-001` 같은 접미사를 붙이므로, 호출자가 지정한 원래 경로 대신 실행 후 반환 필드를 읽는다. Floor/Route 검사 모두 기존 결과를 덮어쓰지 않는다. 결과는 계속 Artifacts/Validation/VisualEvidence 아래에 저장한다.

기록 보존의 독립 검사는 `ValidationEvidenceChecks.Run(새 검사 경로)`로 실행한다. 실행 자료가 없는 미리보기 비교는 통과시키지 않는다. Play Mode 검사 종료 후 Edit Mode에서 `FloorButtonChecks.ValidateTiltPreviews(실제 EvidenceDirectory)`를 실행하면 20개 실제 Inspector 미리보기와 실행 위치를 1mm 이내 기준으로 비교한다.

실행 결과와 한계는 `Artifacts/Validation/VisualEvidence/20261116-205922-floor-coverage/verification.md`를 따른다. T035/T036은 검증 보완이며, T037에서 재현된 기울기 문제는 Canvas/mask의 바닥 경계를 유지하고 판만 수직 이동하도록 수정했다. Quest, 제작자 외관 승인, 직접 청취 동기는 별도다. 메뉴 입력 바인딩 수정은 이번 범위에 포함하지 않는다.

## Touch Plus 메뉴 입력 복구, 2026-11-17

C4/FR-009/SC-005의 기존 중지/재개 동작을 복구했다. `LwsInputActions.inputactions`의 `Left/Menu`는 `<XRController>{LeftHand}/{MenuButton}`을 사용한다. Oculus Touch와 Touch Plus의 왼손 메뉴 usage를 공유하며 오른손 시스템 버튼은 연결하지 않는다. 새 액션/버튼 기능/SDK 의존성을 추가하지 않는다.

실제 XR 세션에서 다음 검사를 실행한다. 연결된 왼손 MenuButton과 액션의 실제 controls가 일치해야 PASS이며, XR 장치가 없으면 미검증으로 실패한다. 모형 검사로 실제 컨트롤러 호환성을 대체하지 않는다.

```powershell
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'return LWS.FireNotification.Validation.MenuInputChecks.ValidateLiveBinding();' --format json
```

실제 Simulator Touch Plus/Oculus Touch 메뉴 입력과 합성 입력 회귀 결과, 녹화 조건과 한계는 `Artifacts/Validation/VisualEvidence/20261117-005243-touch-plus-menu/verification.md`에 기록한다. Quest 실기기와 직접 청취는 별도다.
