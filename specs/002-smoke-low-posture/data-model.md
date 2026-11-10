# Data Model: 연기 속에서 자세 낮추기

**Status**: 구현용 AI 설계 제안. 실행별 메모리 상태이며 신체정보를 저장·전송하지 않는다.

## ContentEntry — 콘텐츠 연결 정보

| 필드 | 형식/규칙 |
|---|---|
| id | 중복 없는 문자열. fire-notification / smoke-low-posture |
| root / anchor | 씬 참조, 누락 불가. root와 XR Origin 공유 시스템 분리 |
| adapter | 콘텐츠 실행·상태·설정 연결 참조 |
| locomotionAllowed | 연기 체험 false. 공통 모달 정책과 AND 결합 |

등록은 2개, 활성 항목은 0개(메뉴) 또는 1개다. 메뉴 진입 시 활성 세션은 닫는다. 번호 002는 콘텐츠 후보 번호가 아니다.

## SmokePostureSession — 실행 상태

| 필드 | 형식/규칙 |
|---|---|
| runId | 새 진입/재시작마다 바뀌는 실행 번호, 영구 식별자 아님 |
| stage / stageExecution | 아래 단계 및 콜백 무효화용 실행 번호 |
| baselineHeight | 유효 준비 완료 때만 설정하는 양수 유한 m, 초기 null |
| referenceRevision | 기준 추적 공간 버전. 변경 시 미완료 기준 폐기 |
| holdSeconds | 유효한 낮은 자세가 이어진 시간, 0~2 |
| lowRecognized | 히스테리시스 판정 내부 상태, 초기 false |
| postureCompleted | 해당 실행의 완료 고정값. 초기 false, 한 번만 true |
| blockers | UserPause / Lifecycle / Tracking / Configuration 차단 이유 집합 |
| closed | 메뉴 이탈/비활성/교체 시 true. 이후 입력/콜백 무시 |

실행은 매번 새로 생성한다. UI나 연출이 임의로 완료 값을 쓰지 않고 세션이 유효 입력을 검증한다. 지연 콜백은 runId와 stageExecution이 모두 일치할 때만 유효하다.

## HeadPoseSample — 실제 입력 어댑터의 샘플

| 필드 | 형식/규칙 |
|---|---|
| heightInOrigin | XROrigin 기준 머리 y(m), 유한값 |
| positionTracked | 장치 유효·isTracked·Position 비트 모두 충족 |
| referenceRevision | 현재 추적 기준 공간 버전 |
| deltaSeconds | 연속 유효 프레임의 경과 시간, 음수/NaN 금지 |
| lifecycleAllowed | 앱/장치 세션이 판정 가능한 상태 |

Input adapter는 추적/세션 불가 시간이나 앱 정지 경과를 복구 첫 샘플에 포함하지 않는다. 시험용 샘플 공급자는 Editor/테스트 전용이며 플레이어가 가짜 높이로 완료할 경로를 제공하지 않는다. Origin scale이 1이 아니거나 참조 누락이면 Configuration 차단으로 안내한다.

## PostureTuning — 앱 판정 제안값

| 필드 | 초기 제안/검증 |
|---|---|
| dropRatio | 0.20. 0보다 크고 1보다 작음 |
| releaseMarginMeters | 0.03 m. 0 이상, baseline×dropRatio보다 작아야 함 |
| requiredHoldSeconds | 2초. 명세 확정값이며 임의 조절 기능 없음 |

entryHeight = baselineHeight×(1−dropRatio). 비인식 상태에서 현재 높이가 entryHeight 이하이면 인식한다. 인식 중 entryHeight+releaseMarginMeters보다 높아지면 해제하고 시간을 초기화한다. 유효한 인식 상태가 2초 이어지면 완료를 한 번 고정한다. 시작 높이·설정 관계가 유효하지 않으면 판정 보류이며 사용자에게 더 숙이라고 요구하지 않는다. 20%/0.03 m는 조정 가능한 AI 기술 제안이며 공식 안전 기준이 아니다.

## 단계 전이

| 현재 상태 | 입력/조건 | 다음 상태/효과 |
|---|---|---|
| AwaitingReady | 유효 추적 중 새 준비 완료 선택 | 기준 저장 → PrincipleGuidance |
| AwaitingReady | 준비 미선택/추적 불가 | 대기, 기준/완료 없음 |
| PrincipleGuidance | 해당 실행의 원리 안내 완료 | Practice, 유지 시간 0 |
| Practice | 낮은 자세 인식 | 같은 단계, 유지 시간 계산/피드백 |
| Practice | 인정 전 일어서기 | 같은 단계, 시간 0/재안내 |
| Practice | 유효 연속 2초 및 차단 없음 | 완료 고정 → CompletionGuidance, 일어서도 됨 안내 |
| CompletionGuidance | 해당 한계 안내 완료 | Result |
| Result | 다시 체험하기 | 기존 실행 폐기 → 새 AwaitingReady |
| 모든 상태 | 메뉴 복귀/비활성/콘텐츠 교체 | Closed, 연출 취소/기준 폐기 |

CompletionGuidance와 Result는 명세의 완료 안내 단계다. 원리 안내와 한계 안내는 필수이며 완료 콜백 전에 다음 단계로 건너뛰지 않는다. Practice 진입 전에 숙인 시간은 계산하지 않는다.

## 중단 우선순위와 복구

1. 이탈/재시작/비활성은 기존 콜백을 무효화한다.
2. 일시정지·lifecycle·추적 불가·기준 변경을 완료 확정보다 먼저 반영한다. 둘 이상이면 모두 해제돼야 재개한다.
3. 완료 이전 차단은 lowRecognized=false, holdSeconds=0이다. 같은 프레임의 2초 충족도 완료하지 않는다.
4. 일반 pause는 음성/자막/입자를 같은 지점에 보존한다. 시스템 중단은 현재 단계 안내를 재제공할 수 있다. 기준 공간이 같으면 기준을 유지하고 새 연속 2초부터 확인한다.
5. 추적 원점/모드 변경 또는 Origin 불연속 이동 시 미완료 기준을 폐기하고 AwaitingReady로 되돌린다. 이전 안내 실행 번호를 무효화한다.
6. 이미 완료된 실행의 일어서기·pause·추적 상실은 완료를 취소하지 않는다. 새 실행/재진입에서만 초기화한다.

## SmokePresentationState / EducationCue

연기 상태는 InitialCollection / SteadyPractice / Frozen / Stopped를 구분한다. Frozen은 직전 연출 상태를 보존하고 종료 시 입자를 정리한다. SteadyPractice의 층 경계는 공간에 고정하며 사용자 자세·대기 시간으로 바뀌지 않는다. 원리 관찰 뒤에는 완료 안내 동안에도 같은 연기를 유지하고 메뉴 이탈 시 정리한다.

EducationCue는 고유 key, 로컬 AudioClip, 시간 구간별 한국어 자막을 가진다. 음성·자막 누락/불일치는 콘텐츠 구성 오류다. 필수 안내 누락 시 성공으로 건너뛰지 않고 메뉴 복귀와 문제 안내를 제공한다. 대본은 contracts/experience-ui.md와 함께 검토한다.

## 데이터 수명

세션/샘플/기준은 실행 종료 시 폐기한다. 새 키/머리 위치 로그·파일·분석 전송은 추가하지 않는다. 검증 보고서에는 시험 조건과 결과만 기록하고 실제 체험자의 신체 수치/식별정보는 요구하지 않는다. 계정·네트워크·서버·DB·마이그레이션은 없다.
