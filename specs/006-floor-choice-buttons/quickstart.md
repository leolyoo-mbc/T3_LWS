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

```powershell
unity command editor.playmode start --project-path E:/projects/T3_LWS --format json
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'LWS.FireNotification.Validation.FloorButtonChecks.RunPlayMode();' --format json
```

신규 검사 클래스는 Running/Passed/Failed/EvidenceDirectory를 노출한다. 시작 호출 반환만으로 통과 처리하지 않고 `Running==false && Failed==0`과 최종 로그를 확인한다. 전환 중 연결 실패는 Play Mode 완료 후 재시도한다. 기존 RouteIntegrationChecks.RunPlayMode도 Ready 대기/새 버튼 경로로 갱신 후 실행한다.

| 검사 | 기대 결과 | 대응 |
|---|---|---|
| Q1 전 대상 | 화재 shout/wait/next, 연기 ready, 경로 ready/6개 대상 use+another/A 완료 next의 상승/대기/하강. 이전 화면 버튼 중복 0 | SC-001 |
| Q2 배치 | 전 앵커, 의도된 관찰 위치, 배율 1/1.5, 기존 지원 자세. 사건 가림/관통/잘림/겹침/통로 침범 0. 이동 후 위치 고정 | SC-002 |
| Q3 행동/입력 | 두 선택 결과/오답 재선택/준비 높이/next/nextpractice 보존. Rising 유지/연타/좌우 동시/Lowering/이전 listener 오실행 0. 상승 때와 클릭 때 자세를 달리해 클릭 순간 높이 저장. 자동 진행 0 | SC-003 |
| Q4 방향 | 반대 방향에서 진입해 정상 등장, 자유롭게 돌아봐 선택, 시선 조건 차단 0 | SC-004 |
| Q5 수명 | Rising/Ready/Lowering 각각 Menu pause/resume, Esc, restart/exit/다른 체험/B 전환. 진행률/음성 위치 보존, 이전 판/입력 잔류 0 | SC-005 |
| Q6 제외 | 자막/상태/선택 화면/일시정지/설정/확인/결과 연출 오적용 0. 글자 설정/재정렬/텔레포트 후 앵커 고정 | SC-006 |
| Q7 증거 | 실제 등장 전/상승/대기/선택/하강 식별, 하강 중 추가 실행 0, 제작자 검토/수정 요청 기록 | SC-007 |

추가 경계: 콜백의 동기 빈 요청+새 요청, 기존 컨트롤러의 선택 거부/같은 단계 유지, tracking/focus 복구, 비활성 B 앵커 구성, 프리뷰 저장/재로드. 최신 요청만 남고 이전 ticket은 무효여야 한다.

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
