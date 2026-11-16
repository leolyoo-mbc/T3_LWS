# Quickstart: 하이라키 정리 검증

이 문서는 구현 후 실행할 절차다. 현재 계획 작성만으로 아래 검사를 수행했다고 간주하지 않는다.

## 준비와 기준 확보

1. Unity 6000.3.26f1로 프로젝트와 `Assets/_02LWS/Scenes/FireNotification.unity`를 연다. 사용자 수정과 씬 저장 상태를 확인한다. 작업 중인 변경을 자동 저장하거나 버리지 않는다.
2. 현재 Git 변경과 씬 배치를 기록한다. 실제 검사 자료는 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-organize-scene-hierarchy/`에 둔다.
3. `Tools/LWS/Hierarchy/Capture Baseline`과 `Dry Run`을 실행한다. [계약](contracts/hierarchy-contract.md)의 목적지와 [비교 모델](data-model.md)에 따라 모든 대상이 한 번씩 분류되어야 한다.
4. 변경 전 정적 검사와 아래 주요 경로를 확인한다. 이전 작업의 성공 기록으로 현재 기준 검사를 대체하지 않는다. 기존 실패가 있으면 정확한 실패와 환경을 기록한다.

CLI 연결 확인:

```powershell
unity status --project-path E:/projects/T3_LWS --format json
```

## 적용과 저장 후 비교

1. 구현 단계에서 `Apply Organization`을 실행한다. 오브젝트 재생성, 원본 프리팹 변경, 사용자 변경 손실이 없어야 한다.
2. `Verify Organization`으로 미승인 변경 0개, 보호 위반 0개, 누락 참조 0개, 이름 위반 0개, 빈 분류 그룹 0개를 확인한다. 실패하면 적용분을 되돌리고 저장하지 않는다.
3. 씬을 저장하고 다시 연 뒤 동일 검사를 반복한다. DryRun을 다시 실행하면 변경 0개여야 한다.
4. 계층과 속성의 비교 자료를 보관한다. 새 오브젝트는 승인된 그룹과 해당 Transform만 허용한다. Canvas 수 2, floor presenter 1, EventSystem 1, narrator 1과 follower 1을 유지한다.

정적 검사 명령은 씬이 열린 Edit Mode에서 실행한다.

```powershell
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'LWS.FireNotification.Runtime.SharedUIContractAudit.CheckScene(); LWS.FireNotification.Validation.FloorButtonChecks.ValidateConfiguration(); LWS.FireNotification.Validation.RouteIntegrationChecks.ValidateConfiguration(); LWS.FireNotification.Validation.RouteDomainChecks.Run(); return "configuration checks completed";' --format json
```

## 실행 경로와 회귀 검사

각 기존 전체 검사 실행 전에는 Play Mode를 새로 시작한다. 검사가 사용하는 합성 추적/장치와 실제 입력을 구분해 기록한다.

```powershell
unity command editor.playmode start --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli --format json
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'LWS.FireNotification.Validation.FloorButtonChecks.RunPlayMode(); return "started";' --format json
unity command script eval --project-path E:/projects/T3_LWS --caller plugin --skill unity-cli 'return "running="+LWS.FireNotification.Validation.FloorButtonChecks.Running+" passed="+LWS.FireNotification.Validation.FloorButtonChecks.Passed+" failed="+LWS.FireNotification.Validation.FloorButtonChecks.Failed;' --format json
```

Running=false 이후 Failed=0이 기대 결과다. Running=true 상태나 명령 호출 성공만으로 통과 처리하지 않는다. 로그가 생성된 실제 경로를 확인한다. 완료 후 Play를 중지하고 새로 시작해 RouteIntegrationChecks.RunPlayMode()도 같은 방식으로 실행하며 Running/Passed/Failed를 확인한다. 테스트 후 합성 장치와 추적 override를 해제하거나 Play를 중지해 초기화한다.

환경 의존 실패가 있으면 전후 비교와 원인을 남긴다. 실패하는 검사를 삭제하거나 제품 입력을 바꿔 통과시키지 않는다. 이번 변경의 신규 실패는 완료 전에 해결한다.

| 경로 | 확인할 결과 |
|---|---|
| 체험 선택 화면 | 세 체험 버튼, 설정, 안내 문구와 선택 진입 유지 |
| 화재 사실 알리기 | 행동 선택, 음성, 주변 인물 반응, 상황 복원, 다음, 비상벨 직접 조작, 결과와 복귀 |
| 연기 속 대피 | 준비 완료, 높이 기준과 자세 진행, 안내와 연기, 결과와 복귀 |
| 대피 경로 판단 | 준비 완료, A 탐색과 선택/피드백, 다음 연습, B 우회/선택, 결과와 복귀 |
| 공용 화면 | 일시정지/계속, 설정 변경, 뒤로, 재시작/나가기 확인, 결과의 다시하기와 복귀 |
| 바닥 버튼 | 상승/준비/하강, 레이 선택과 벽 차폐, 입력 중복 방지, 두 글자 배율에서 배치 유지 |

실제 연결된 컨트롤러가 있으면 MenuInputChecks.ValidateLiveBinding()과 실제 왼손 메뉴 입력으로 세 체험 중지/재개도 확인한다. 컨트롤러 미연결은 통과가 아니라 미검증이다. Operator가 없다는 이유로 새 의존성을 추가하지 않는다.

## 편집 가독성과 시각 자료

- 전역 이름 검색 없이 spec의 SC-001 대상과 공용 UI를 그룹을 따라 찾아본다. 최종 경로를 `Assets/_02LWS/Documentation/SceneHierarchy.md`에 기록한다.
- 하이라키 전후 실제 Editor 화면과 같은 관찰 위치의 배경/UI를 캡처한다. 재배치로 인한 조명, 가림, 레이아웃 차이가 없는지 확인한다. 기준점은 이동하지 않는다.
- 주요 실행 구간을 원본 1920×1080 이상, 30fps 이상 MP4로 녹화하고 Unity 게임 소리만 담는다. 다른 앱 소리와 마이크를 포함하지 않는다. 동일 수음 설정의 기존 분리 검증을 재사용하면 정확한 기록을 링크한다. 방식이나 설정이 바뀌면 외부 앱 소리가 나는 상태에서 Unity 무음/유음 구간으로 분리를 재검증한다.
- 저장한 영상의 실제 메타데이터와 화면을 확인하고 소리가 있는 구간은 직접 재생해 청취와 동기화를 확인한다. 청취하지 못했다면 해당 항목은 미검증으로 적는다. 헌법 예외 사용 시 최선의 영상, 원본 스크린샷과 부족한 부분의 대체 자료 및 이유를 남긴다.
- verification.md에 Editor/합성 입력/실제 컨트롤러/Quest 실기를 구분한다. 자료 존재만으로 실행 전체나 실기 성공을 주장하지 않는다.
- `git check-ignore <evidence-file>`와 `git ls-files -- <evidence-file>`로 자료의 Git 제외 및 비추적 상태를 확인한다. 영상이나 기준 JSON을 Assets로 옮기지 않는다.

## 최종 변경 범위 확인

Packages manifest/lock, ProjectSettings, 입력 액션, 런타임 기능 코드, 프리팹 원본과 ContentMenu.unity가 변경되지 않았는지 확인한다. 의존성 검사에서 새 Meta SDK/Operator 참조가 없어야 한다. 기존 오브젝트의 GUID와 Component ID를 보존한다. 문서, 씬, Editor 전용 도구와 필요한 meta만 변경한다. APK나 unitypackage 생성, 커밋과 push는 이 검증 절차에 포함하지 않는다.
