# Quickstart: 환경·화재·연기 개선 검증

**현재 상태**: 구현 후 사용할 검증 가이드. 아래 설계 대상은 아직 구현·실행 검증한 결과가 아니다. 상태 계약은 [environment-presentation.md](contracts/environment-presentation.md), 기록 필드는 [data-model.md](data-model.md)를 따른다.

## 1. 준비와 기준 확보

Unity 6000.3.26f1로 E:/projects/T3_LWS를 열고 기존 FireNotification.unity를 사용한다. 새 Editor·패키지 설치 없이 기존 XR Simulator/입력으로 진행한다. 수정 전 scene dirty 상태·사용자 변경을 확인하고 기준 화면·anchor·particle 설정을 저장한다. 프로젝트가 열려 있으므로 별도 batch Editor를 동시에 시작하지 않는다.

```powershell
unity status --project-path 'E:/projects/T3_LWS' --format json
unity command scene list_open --caller plugin --skill unity-cli --project-path 'E:/projects/T3_LWS' --format json
unity command console status --caller plugin --skill unity-cli --project-path 'E:/projects/T3_LWS' --format json
unity command editor.playmode start --caller plugin --skill unity-cli --project-path 'E:/projects/T3_LWS' --format json
```

현재 CLI discovery·명령 옵션은 구현 시 확인한다. sandbox에서 unreachable이면 이를 Editor 종료로 단정하지 않고 허용된 로컬 권한으로 다시 조회한다. 오류/timeout을 숨긴 채 반복 실행하지 않는다. Console 컴파일 오류 0건, 메뉴에서 콘텐츠를 정상 선택할 수 있어야 한다.

변경 전후 1920×1080, vertical FOV=60도, 동일 quality로 비교한다. 실제 Game View가 다른 설정이면 기준부터 같은 값으로 재기록한다. 임의 검증 카메라는 Editor 전용으로 쓰고 제품 rig·anchor·교육 흐름을 변경하지 않는다.

## 2. 복도와 실내 — SC-001/004

화재 알리기 진입·BellApproach·기존 텔레포트 도착 위치마다 6방향을 본다. 천장·뒤쪽·문 옆·걸레받이 접합부에서 빈 배경·뒷면·틈·다른 체험 노출 0건. 벽·문 표면 반복과 노멀 왜곡, 과한 반짝임, pink/missing material 0건. 비상벨 접근과 UI 가독성을 확인한다.

연기 방은 원래 외곽·진입 anchor·바닥·천장 높이를 대조한다. 추가 정면 문, 열린 문짝·문틀·문 너머 바닥·벽·끝 마감을 비스듬히 본다. 사용자의 뒤 열린 면도 마감되어야 한다. 연기가 덮기 전에도 실내 인상이 개선됐는지 사용자에게 실제 화면으로 확인한다.

## 3. 화재·유입·빽빽함 — SC-002/003

준비 완료 → 원리 안내에서 문 너머 불꽃 일부·흔들리는 벽빛·문 상단 유입·천장 확산 4요소를 관찰한다. Practice/Completion/Result에서도 문 유입·화재가 남고 단계 전환 때 점프하지 않아야 한다. 대기에서는 연기·불꽃·불빛이 꺼져 있어야 한다.

비교 seed는 모든 시안에 같은 값을 사용한다(초기값 7100+i). 각 시안의 Collect 이후 2초·5초, Practice 진입 이후 3초를 기록한다. 자동 안내 길이 차이로 해당 단계에 도달하지 않았다면 단계 기준 시간을 우선하고 실제 시간을 남긴다. 8방향 비교 세트는 Practice+3초에 기립 높이 1.65 m/낮은 높이 1.15 m, yaw=0/90/180/270으로 촬영한다. 이 높이는 관찰용이며 제품 자세 판정 기준이 아니다. 비주얼 카메라 위치만으로 세션 성공을 발생시키지 않는다.

0.8/1.2/1.6 크기에서 나머지 rate/lifetime/max/색/shape를 고정해 먼저 비교한다. 큰 입자가 벽이나 아래쪽으로 침범하면 spawn 영역 조정과 변경 조건을 기록하고 다시 비교한다. 사용자에게 전후를 보여주고 빽빽함 승인/거절을 기록한다. 색 변화만으로 빈틈 개선 합격을 선언하지 않는다.

## 4. 상태·입력 회귀 — SC-005

화재: 정상 선택 → 외치기 → 다음 → 상황 복원 → 비상벨 직접 누름 → 결과 → 메뉴. 자세: 준비 완료 → 원리 → 자세 낮춤 연속 2초 → 완료 안내 → 결과 → 메뉴. 대사·자막·새 누름·기존 자세 판정 유지. UI 기본/150%에서 필수 글자 잘림과 조작 차단 0건.

두 체험 각각 전체 완료 1회, 일시정지·재개 1회, 중단·재진입 1회 이상. 추가로 원리/연습 단계에서 연기·화재·불빛이 함께 멈추고 재개되는지 확인한다. 추적 상실·복구 시 잘못된 완료와 효과 중복 0건. 메뉴 → 화재 → 메뉴 → 자세 → 메뉴 → 화재를 순회해 비활성 root 효과가 남지 않고 최초 Play의 메뉴부터 진입·복귀·Play 종료까지 공용 렌더 설정이 교체되지 않는지 확인한다. 비활성 공간의 천장등·화재 불빛은 다른 공간에 남지 않아야 한다. Stop/재진입은 이전 입자와 불빛 시간을 초기화해야 한다.

## 5. 성능·전달 — SC-006/007

메뉴와 두 체험을 대상으로 같은 머신·해상도·품질·관찰 조건에서 변경 전후 워밍업 10초 후 30초 표본을 3회씩 수집한다. CPU/GPU frame time의 median/p95, 입자 수·draw/batch, 녹화 사용 여부를 기록한다. GPU 시간 등 지원하지 않는 값은 unavailable로 표시한다. Editor 수치를 Quest 값으로 표기하지 않는다. 차이가 크면 같은 외관의 더 작은 크기/단순 재질 대안을 비교하고 사용자 미관 요구를 조용히 낮추지 않는다.

VisualAssetSources.json의 environment 출처·다운로드 hash·CC0·가공 내역과 실제 파일을 대조한다. UPM을 제외한 필수 참조가 _02LWS 안에 있는지 검사한다. lighting data/lightmap/probes·shader/texture가 포함되는지 확인하고 import 수행 여부를 별도로 기록한다. 기존 공용 URP 추가 조명 지원은 패키지 포함 자원이 아닌 수신 프로젝트 설정 조건으로 안내하고 확인한다.

## 6. 실행 증거와 승인

MP4 도구가 있으면 실제 Game View를 녹화한다. 도구가 없거나 캡처 불가 시 임의 설치보다 사유와 단계별 대체 시각 자료를 남긴다. MP4는 디코딩 가능 여부까지 확인한다. 프레임이 진행하고 입력·연출이 움직이는지 확인하며 정지된 첫 프레임을 실행 증거로 삼지 않는다.

보관 경로는 `Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-visual-environments/`이고 아래 명령의 example 경로는 실제 생성 폴더로 바꾼다.

```powershell
unity command capture game_view --caller plugin --skill unity-cli --project-path 'E:/projects/T3_LWS' --output-path 'E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/example/01-game.png' --format json
unity command editor.playmode stop --caller plugin --skill unity-cli --project-path 'E:/projects/T3_LWS' --format json
git -C 'E:/projects/T3_LWS' check-ignore 'Artifacts/Validation/VisualEvidence/example/verification.md'
git -C 'E:/projects/T3_LWS' ls-files -- 'Artifacts/Validation/VisualEvidence/'
```

check-ignore는 제외 규칙이 적용되어야 하고 ls-files에는 검증 원본이 없어야 한다. MP4 또는 대체 자료와 verification.md를 실제 생성 경로에 저장하고 사용자에게 대표 파일을 직접 링크한다. 두 콘텐츠 외관 승인을 각각 기록한다. VR 입체감·편안함·성능과 최종 수신 프로젝트/제출 승인은 이번 Editor 시각 승인과 구분한다.
