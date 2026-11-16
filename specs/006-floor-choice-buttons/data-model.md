# Data Model: 공용 바닥 버튼

## FloorButtonAnchor (신규 MonoBehaviour)

| 필드 | 타입 | 의미/검증 |
|---|---|---|
| transform | Transform | 바닥 경계/방향/기울기. 머리/컨트롤러 자식 금지, scale=1 |
| id | string | 씬 내 고유 식별자, 빈 값/중복 금지 |
| raisedBottomHeight | float | 상승 후 판 하단, 시작값 0.60m, 0 이상 |
| floorClearance | float | 바닥 경계 여유, 시작값 0.02m, 0 이상 |
| singleWidth/twoTotalWidth/gap | float | 시작값 0.66m/1.35m/0.02m, 문구에 맞게 조정 |
| minimumHeight | float | 시작값 0.24m, preferred height보다 작게 자르지 않음 |
| riseSeconds/lowerSeconds | float | 시작값 0.35/0.25, 유한한 양수 |

모든 float는 유한해야 한다. 너비/높이는 양수이고 간격은 두 항목을 담을 수 있는 범위다. 실제 Canvas 단위 변환은 Presenter 한 곳이 담당한다. 숫자는 사용자 확정 외관이 아니다.

## ContentUIAction (기존)

`Id: string`, `Label: string`, `Invoke: Action`을 유지한다. 묶음 내 ID 유일, 공백 문구/null 콜백 금지. 행동 선택 최대 2개/진행 버튼 1개. 표시 측이 정답/문구를 바꾸지 않는다.

## FloorButtonRequest (런타임)

| 필드 | 의미 |
|---|---|
| owner | 현재 ContentExperienceAdapter |
| ticket | SharedContentUI가 발급하는 증가 요청 세대 |
| anchor | 유효한 위치 참조 |
| actions | 기존 행동 배열의 복사본 |
| consumed | 해당 묶음의 실행 수락 여부 |

SharedContentUI는 논리 요청 1개를 소유한다. Presenter는 표시 스냅샷 1개와 최신 대기 요청 최대 1개를 보관한다. 입력은 표시 ticket과 현재 요청 ticket이 일치할 때만 수락한다. 다른 owner/실행의 동일 ID는 같은 요청이 아니다.

## FloorButtonDisplay

`phase: Hidden | Rising | Ready | Lowering`, `progress: 0..1`, `paused: bool`, `outgoingSnapshot`, `pendingRequest`, `acceptedTicket`을 관리한다. 처리 중은 별도 대기 상태 대신 consumed/acceptedTicket으로 즉시 잠근다.

| 사건 | 상태/동작 |
|---|---|
| Hidden + 비어 있지 않은 요청 | 위치/문구 채움, Rising, 입력 불가 |
| Rising 완료 | Ready, InvalidatePress, 해제 후 새 누름 요구 |
| Ready + 유효 클릭 | 묶음 consumed/잠금, Lowering, 기존 콜백 즉시 1회 |
| Rising/Ready + 일반 취소/교체 | 이전 ticket 무효화, Lowering, 최신 요청 보관 |
| Lowering + 갱신 | 표시 스냅샷 유지, 최신 pending만 교체 |
| Lowering 완료 | Hidden, 유효한 pending이 있으면 Rising |
| pause/owner 차단/로딩 | phase/progress 보존, 이동/입력 정지 |
| 사용자 pause 해제 | phase/progress 복구, 새 입력 해제 요구 |
| Clear/owner 교체/restart/exit/구역 전환/비활성 | 즉시 Hidden, ticket 갱신, 표시/콜백/대기 입력 폐기 |

pause 중 같은 요청 갱신은 상승을 재시작하지 않는다. 시스템 복구가 기존 교육 Present를 다시 호출하면 최신 요청을 적용한다. 교육 복구 정책은 변경하지 않는다.

## 직렬화 위치 매핑

화재 어댑터는 선택/다음 앵커, 연기는 준비 앵커, 경로는 A 준비 앵커와 targetId -> anchor 배열을 갖는다. 화재 선택/다음은 같은 참조를 쓸 수 있다. 경로 next는 완료한 A 계단의 앵커를 사용한다.

경로 a-left/a-right/a-lift/b-left/b-right/b-lift의 누락/중복은 오류다. 저장 구성 검사에서 비활성 B 공간 참조는 허용하지만 현재 표시 요청은 활성 공간에 속해야 한다. 전환 전 이전 판/요청을 리셋한다.

영구 저장/새 결과 모델은 만들지 않는다. 기존 Session/run/execution은 교육 컨트롤러가 소유하며 Presenter가 변경하지 않는다.
