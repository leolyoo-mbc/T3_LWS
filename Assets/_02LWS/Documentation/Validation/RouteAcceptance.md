# 안전한 대피 경로 판단 검증 집계

2026-11-13. 세 번째 콘텐츠 구현과 자동 수행 가능한 Editor/Android/수신 검증을 마쳤다. 신규 음원 청취, 외관의 사용자 승인, 최종 영상의 세밀한 소리 동기화 청취와 실제 Quest 검증은 남아 있다. 전체 작업/제출 합격으로 표시하지 않는다.

## 환경별 상태

| 범위 | 실제 결과 | 한계 |
|---|---|---|
| 순수 도메인 | 69개 통과 | 실제 공간/기기 입력과 별개 |
| 원본 Editor 경로 | 164개 통과, 결과 추적 상실 추가 재현 검사 실패 후 수정 통과 | 합성 추적/버튼/앵커 입력 |
| 기존 두 콘텐츠 전체 회귀 | 49개 통과, 패널 활성 4,460개 프레임의 글자 잘림 0개 | 실제 Quest 조작 아님 |
| 수신 Editor 경로 | 최종 코드의 추가 검사 포함 166개 통과 | 별도 테스트 프로젝트 |
| 수신 기존 콘텐츠 | 화재 안내/선택지/이동, 자세 전체 완료/이동 금지 통과 | 수신 화재 전체 절차는 미반복 |
| 성능 | 공간 5개, 10초 워밍업 뒤 30초씩 3회, CPU/GPU 기록 | Editor/XR Simulator, Quest 성능 아님 |
| Android | IL2CPP ARM64 개발 APK 성공, 오류 0, 진단 심볼 경고 1 | 설치/실기기 실행 미수행 |
| 패키지 | _02LWS 원본/메타 비교, 별도 신규 프로젝트 가져오기/컴파일/Play 통과 | 최종 지정 통합 프로젝트는 별도 |
| 신규 음원 | F1/ko/16 steps/0.95, 7개 WAV 길이/hash 일치 | 사용자 청취 승인 대기 |
| 외관 | A/B 구조와 실제 실행 자료 확보 | 사용자 승인 대기 |
| Quest 2 | ADB 연결 기기 0개 | 조작/가독성/성능/편안함 미검증 |

## 명세별 결과

| 요구 | 실제 확인 | 상태 |
|---|---|---|
| FR-001 | 메뉴 등록 3개, 진입/복귀, 기존 콘텐츠 회귀 | Editor 통과 |
| FR-002 | 직선 A/꺾인 B, 교실 문/창/계단/엘리베이터/표지 | 실행 자료 확인 |
| FR-003 | A 확인 방법 안내, 다른 B 배치, 막힌 계단 1개/다른 계단 1개 | Editor 통과 |
| FR-004 | 고정 앵커 14개, 실제 도착 확인, 연기 내부 목적지 없음 | Editor 통과 |
| FR-005 | 대상명과 동일 계단/엘리베이터 선택 문구, 최대 2개 행동 | Editor 통과 |
| FR-006 | 동일 중립 앵커/버튼 표현, 연기는 공간 판단 단서 | 코드/실행 확인 |
| FR-007 | 엘리베이터/연기 진입 이동 거부, 실제 음성/자막 뒤 재선택 | Editor 통과 |
| FR-008 | 다른 경로 선택 뒤 직접 인접 이동, 정답 자동 이동 없음 | Editor 통과 |
| FR-009 | 막힌 계단/엘리베이터 미방문 직접 완료 | Editor 통과 |
| FR-010 | 다른 경로 선택/이탈/다른 계단 선택의 실제 우회만 기록 | Editor 통과 |
| FR-011 | 최초 오답 보존, 직접/우회/피드백 후 완료 구분 | Editor 통과 |
| FR-012 | 공용 Canvas/재생기/추종기 각 1개, 콘텐츠 내부 UI 0개 | 구성 통과 |
| FR-013 | 하단 패널, 기본/150%, 자막 끄기, 결과 내용 높이 확장 | Editor 통과, Quest 미검증 |
| FR-014 | 중단/예약 취소/추적 상실/재시작/낡은 선택 거부 | Editor 통과 |
| FR-015 | 계단 상태 확인, 연기 진입 회피, 낮은 자세의 안전 보장 금지 | 대본/결과 대조 |
| FR-016 | 실제 탈출 완료와 구분, 모든 경로 막힘은 별도 교육 | 대본/결과 대조 |
| FR-017 | 문/창 비례와 깊이, 천장, 계단참/난간, 엘리베이터 깊이 | 구현/화면 확인, 외관 승인 대기 |
| SC-001 | A/B 직접/우회/오답 후 완료 | 통과 |
| SC-002 | 시험한 위험 선택에서 진입 이동 0건, 피드백/재선택 제공 | Editor 통과 |
| SC-003 | 방문 강제/감점/우회 허위 표시 0건, 첫 오답 보존 | Editor 통과 |
| SC-004 | 출발/계단 방향/엘리베이터 배치 차이, 연기 없는 우회 | 구성/실행 확인 |
| SC-005 | 기본/150%/자막 끄기 필수 조작과 문구 보존 | Editor 통과, Quest 미검증 |
| SC-006 | 세 콘텐츠 전환, 중단/재시작/이전 입력 무효화 | Editor 통과 |
| SC-007 | 7개 대본/결과와 공식 행동요령의 의미 대조 | 검토 완료, 청취 승인 별도 |
| SC-008 | 시작/복도/계단/엘리베이터 실제 A/B 자료 | 사용자 검토 대기 |

## 직접 확인 파일

[대표 실행 MP4](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/execution.mp4)

[A 시작](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/01-a-start.png), [B 막힌 계단](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/05-b-blocked-stair.png), [B 복도](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/06-b-corridor.png), [B 엘리베이터](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/09-b-elevator.png), [150% 결과](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/08-result.png).

[결과 중 추적 상실/복구 실행 MP4](E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/20261113-214458-safe-evacuation-route-final/result-tracking-recovery/execution.mp4). 이 짧은 검사는 도메인 입력으로 결과 상태를 준비한 뒤 실제 UI를 렌더링했으며 물리 이동 검사가 아니다. 결과/추적 경고 구간은 원래 무음이다.

영상은 원본 1920x1080/30fps 카메라 렌더와 Unity 내부 출력만 기록했다. 외부 앱/마이크는 제외하며 동일 입력 분리 시험을 재사용했다. 저장 대표 MP4를 실제 VideoPlayer로 재생하고 디코딩한 화면과 비무음 소리 출력 신호를 확인했다. 발음/억양, 세밀한 화면/소리 동기화의 청취 승인은 아직 없다. 상세 조건과 원본 로그는 같은 폴더 verification.md, RouteVoiceReview.md, RouteVisualReview.md, RoutePerformance.md, RouteAndroidBuild.md, RouteExportImport.md를 따른다.

## 변경 판단과 남은 작업

기존 두 콘텐츠의 환경/원본 프리팹/재질은 수정하지 않았다. 공용 이동 허용 조건을 유지하면서 콘텐츠별 이동 게이트를 추가했다. 추적/lifecycle 차단은 XRI 예약 처리보다 먼저 실행한다. 공용 UI는 콘텐츠 갱신 뒤 높이를 계산하고, 결과 문구 높이를 확장하며, 차단 중 결과에서는 복구 안내/메뉴를 보존한다. 콘텐츠별 공용 UI 분기는 추가하지 않았다.

A/B 계단 확인 지점은 UI와 계단 표지의 겹침을 없애기 위해 입구에서 3m인 제작값으로 조정했다. 도착 반경 0.65m, 지점 간 최소 2m와 함께 실제 화재 안전 기준이 아니다. 초기 공통 세션 구현에 B 전이를 함께 작성한 뒤 B 도메인 검사를 보강한 순서 차이가 있다. 초기 세션 부재의 실패는 확인했지만 B 전용 검사 각각의 구현 전 적색 단계 기록은 확보하지 않았다. 기능의 도메인/실제 세 경로/수신 검증 결과와 이 과정의 한계를 구분한다.

T028 신규 음원 청취, T032 Quest 2, T034 최종 영상 세밀한 소리 동기화 청취, T036 외관 사용자 승인 상태를 미완료로 유지한다. 개발 빌드/패키지와 기능 검사 통과가 이 승인을 대신하지 않는다. 확장 훅 파일 `.specify/extensions.yml`은 없으며 전후 훅은 없다. AGENTS.md는 수정하지 않았다.

## 메뉴 버튼 겹침 수정

세 번째 콘텐츠 버튼과 설정 버튼의 실제 사각형이 겹친 실패를 확인했다. 메뉴 패널 높이를 1140에서 1320으로 늘리고 버튼/하단 안내 간격을 재배치했다. 콘텐츠 버튼 3개와 설정 버튼의 실제 GraphicRaycaster 적중, 기본/150% 글자 크기, 설정 열기를 포함한 Editor Play 검사 28개가 통과했다. 증거는 `Artifacts/Validation/VisualEvidence/20261113-231242-menu-button-spacing`의 PNG, execution.mp4, checks.txt에 있다. 이 메뉴와 설정 구간은 원래 무음이며 Unity 내부 출력만 녹화했다. Quest 실기 확인은 미완료다.
