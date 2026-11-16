# Contract: 바닥 버튼과 교육 흐름

## C1. 요청 인터페이스 (계획)

```csharp
// SharedContentUI
void SetActions(ContentExperienceAdapter owner, params ContentUIAction[] actions);
void SetActions(ContentExperienceAdapter owner, FloorButtonAnchor anchor, params ContentUIAction[] actions);
void Clear(ContentExperienceAdapter owner);
bool IsFloorExitComplete(ContentExperienceAdapter owner);

// FloorButtonPresenter
void Present(ContentExperienceAdapter owner, long ticket, FloorButtonAnchor anchor,
             ContentUIAction[] actions, Action<long, string> select);
void SetBlocked(bool blocked);
void ResetDisplay();
```

첫 오버로드는 빈 요청/취소에 쓴다. 비어 있지 않은 요청에 위치가 없으면 구성 오류다. non-owner 요청/Clear는 무시한다. 최대 2항목, 고유 ID/문구/콜백/앵커를 검사한다.

SharedContentUI가 owner/ticket/최신 행동의 실행 관문이다. Presenter의 select(ticket,id)는 이 관문으로 요청하며 교육 결과를 직접 만들지 않는다. 같은 요청 갱신은 상승을 재시작하지 않는다. owner/anchor/행동/실행 문맥 변경은 새 ticket을 발급한다. Clear/restart는 같은 ID/콜백이라도 이전 ticket을 폐기한다.

## C2. 클릭과 재진입

수락 조건: owner 활성, !owner.Blocked, !메뉴, !로딩, 현재 ticket 일치, Ready, 미처리, 상승/정지 이후 해제된 새 입력. 하나라도 거짓이면 콜백 0회다.

전체 묶음을 먼저 잠그고 하강을 기록한 뒤 콜백을 즉시 정확히 1회 실행한다. 하강 완료까지 미루지 않는다. 동기 빈 요청/새 요청이 발생해도 내려가는 스냅샷은 유지한다. ResetDisplay는 그 스냅샷도 즉시 버린다.

다음 연습은 콜백의 요청 수락과 실제 공간 이동을 분리한다. 경로 컨트롤러는 현재 owner/run/execution에 연결된 대기 전환을 유지하고 `IsFloorExitComplete(owner)`가 참인 뒤에만 A를 끄고 B로 이동한다. 이 메서드는 현재 owner의 마지막 수락 판이 정상 하강을 마쳤을 때만 참이며 단순 Hidden/빈 요청/ResetDisplay를 완료로 취급하지 않는다. 정지/차단 중 전환은 보류하고 재시작/종료/owner 교체 시 대기 전환과 완료 표식을 폐기한다. 준비 완료와 다른 버튼 콜백의 실행 시점은 바꾸지 않는다.

좌우 동시/연타/이전 UnityEvent는 첫 수락 이후 0회다. 기존 컨트롤러에서 실행을 거부해 같은 단계가 남으면 하강 후 현재 요청을 재확인해 새 ticket으로 제공한다. 상승/하강 종료로 교육 콜백을 실행하지 않는다.

## C3. 표시/배치

공간 고정, 고개/컨트롤러 추종/자동 정면 배치 없음. 반대 방향을 보고 있어도 등장한다. 한 항목 가운데/두 항목 좌우, 기존 순서/문구/동등한 스타일 유지, 정답 암시 없음.

바닥 아래는 mask로 차단한다. 배율 1/1.5와 기존 지원 자세에서 문구 전체를 담고 벽/바닥/판단 대상/통로/텔레포트 지점과 겹치지 않게 조정한다. 벽에 가려진 판은 클릭되지 않아야 한다. mask/occlusion이 배치 오류 수정의 대안은 아니다.

## C4. 생명주기/설정

사용자 pause는 phase/progress를 보존하고 입력/이동을 멈추며 모달 아래 판을 숨긴다. 재개 시 해제 후 새 입력이 필요하다. 음성 재생 지점은 기존 컨트롤러가 유지한다. 시스템 차단 복구의 교육 정책은 그대로 둔다.

restart/exit/owner 변경은 즉시 ResetDisplay하고 대기 이동을 취소한다. 정상 B 전환은 현재 판의 하강 완료를 확인한 뒤 ResetDisplay하고 새 구역 요청을 받는다. 이전 listener는 새 실행을 조작할 수 없다. 테마/글자 배율은 공용 설정을 적용한다. 재정렬/텔레포트로 바닥 앵커를 머리 앞에 옮기지 않는다. 제외 UI와 왼쪽 Menu/PC Esc는 기존 동작 유지.

## C5. 제작 인터페이스/감사

앵커 Inspector에서 위치/방향/기울기/크기/시간과 숨김/상승 후/해제를 확인한다. 비저장 미리보기는 교육/음성/입력 없이 동작하고 선택 해제/Play Mode/재로드에 제거한다. 씬 저장에 runtime 상태나 임시 Canvas가 섞이면 실패다.

지정 공용 Canvas 2개/내레이터 1개/EventSystem 1개/Presenter 1개, 콘텐츠별 Canvas/입력 0개, 전 앵커 참조를 감사한다. 이전 Actions 행에 중복 실행 버튼이 남지 않아야 한다. 비활성 템플릿은 실행 대상에서 제외한다.
