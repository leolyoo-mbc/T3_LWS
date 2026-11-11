# Research: 연기 속에서 자세 낮추기

**Date**: 2026-11-10. 현재 프로젝트 파일 조사와 공식자료 확인을 바탕으로 한 설계 결정 기록이다. 결정은 구현 방법의 제안이며 신규 교육 범위를 사용자 확정으로 취급하지 않는다.

## R1. 기존 단일 씬과 공통 UI 연결

**Decision**: 기존 씬에 SmokePostureSpace를 추가하고 콘텐츠별 어댑터를 둔다. 콘텐츠 1의 도메인 단계 모델을 확장해 다른 체험 단계를 섞지 않는다.

**Rationale**: ContentSceneNavigator.cs는 contentRoot/contentAnchor/controller 하나만 보유한다. SharedUIController.cs의 Controller 속성은 navigator.controller를 직접 반환하며 음원 수집·일시정지·재시작·메뉴 복귀·결과 문구가 콘텐츠 1에 묶여 있다. MenuTeleportationProvider.cs 역시 해당 controller.lifecycle을 참조한다. 이 세 연결부를 함께 수정해야 두 콘텐츠가 공통 설정과 중단 기능을 사용할 수 있다.

**Alternatives considered**: 별도 씬은 XR Rig·EventSystem·공통 설정 중복과 씬 로딩 처리를 추가한다. 콘텐츠 1 복제는 수정 사항을 두 벌 유지하게 한다. 범용 시나리오 프레임워크는 현재 두 콘텐츠에 불필요하다.

**Evidence**: Assets/_02LWS/Scripts/Runtime/ContentSceneNavigator.cs, SharedUIController.cs, FireNotificationController.cs, Scripts/Interaction/ContentMenuInput.cs, MenuTeleportationProvider.cs. 현재 씬의 MenuSpace/FireNotificationSpace/SharedSystems도 파일에서 확인했다.

## R2. 개인 기준과 실제 추적 높이

**Decision**: 준비 완료 순간 XROrigin.CameraInOriginSpaceHeight를 저장한다. Origin scale=1, 동일 기준 공간에서 현재 높이와 비교한다. 실제 head 장치의 유효성·isTracked·Position tracking 비트를 별도로 확인한다. 추적 불가 샘플로 기준·유지 시간·완료를 만들지 않는다.

**Rationale**: 설치 CoreUtils의 XROrigin.cs에서 CameraInOriginSpaceHeight는 origin.InverseTransformPoint(camera.position).y다. 월드 y와 달리 공간 배치 이동의 영향을 줄인다. 다만 추적 원점/모드 변경은 별도이며 ExperienceLifecycle의 세션 활성·포커스·착용 상태만으로 위치 추적 유효성을 증명할 수 없다.

**Alternatives considered**: 절대 월드 높이는 작은 사용자와 공간 이동에 취약하다. 카메라 pitch는 고개만 돌려도 변한다. 전신 추적은 현재 기기·입력 범위를 늘린다.

**Evidence**: Library/PackageCache/com.unity.xr.core-utils@*/Runtime/XROrigin.cs의 CameraInOriginSpaceHeight와 trackingOriginUpdated 처리, com.unity.inputsystem@*/InputSystem/Runtime/Plugins/XR/TrackedPoseDriver.cs의 trackingStateInput/Position 검사. 캐시 해시는 환경마다 달라지므로 패키지 ID와 심볼로 재확인한다.

**Limit**: 헤드셋만으로 사용자가 준비 순간 실제로 서 있는지, 전신이 올바른 자세인지 증명할 수 없다. 준비 안내를 제공하고 판정 한계를 명시한다.

## R3. 낮은 자세·연속 시간·복구

**Decision**: 초기 앱 설정 제안은 낮춤 거리=시작 높이×0.20, 해제 여유=0.03 m다. 진입은 currentHeight ≤ baselineHeight−drop, 유지 중 해제는 currentHeight > baselineHeight−drop+0.03이다. 해제 경계는 baseline보다 낮아야 한다. 설정은 데이터로 분리해 기기에서 조정한다. 연속 2초 조건은 사용자 확정이며 자동 완료는 한 실행에서 한 번이다.

**Rationale**: 개인별 상대 높이와 진입/해제 경계 분리로 작은 흔들림을 줄인다. 완료 이전 일어서기·정지·추적 상실은 유지 시간을 초기화한다. 시간·샘플 입력을 분리하면 1.99초/2초와 같은 경계 및 중단 우선순위를 재현할 수 있다.

**Alternatives considered**: 한 절대 높이는 사용자 차이를 반영하지 않는다. 누적 시간은 여러 번 잠깐 숙여도 통과하게 한다. 숙인 채 버튼 누르기는 사용자가 확정한 자동 인정을 바꾼다.

**Recovery**: XRInputSubsystem.trackingOriginUpdated, 추적 모드 변경, Origin 불연속 이동에서 미완료 기준을 폐기한다. 카메라 높이 급변만으로 모든 recenter를 구별한다고 주장하지 않는다. 같은 기준 공간의 일시적 추적 복구는 안내 후 유지 시간을 새로 잰다. 이미 인정한 완료는 같은 실행의 일어서기/중단 복귀로 취소하지 않는다.

**Limit**: 20%와 0.03 m는 공식 교육 기준이나 검증 결과가 아니다. 편안한 숙임에서 미인식/흔들림이 나오면 반복 압박 대신 중단 경로를 제공하고 튜닝 검증 실패로 기록한다. 안전 높이를 더 낮추라는 교육으로 바꾸지 않는다.

## R4. 연기 표현과 Quest 비용

**Decision**: 기존 내부 FireSmoke 재질/텍스처를 호환성 확인 후 재사용하는 URP ParticleSystem을 첫 구현으로 삼는다. 초기 상승/상부 축적과 연습용 고정 상부 영역을 구분한다. 연습 중 경계 위치·밀도 목표는 자세와 시간에 따라 악화되지 않고 입자만 순환한다. 무거운 유체 시뮬레이션이나 화면 전체 fog는 추가하지 않는다.

**Rationale**: 명세는 물리 예측이 아니라 위쪽 연기와 낮아진 관찰 위치의 공간 관계를 요구한다. 기존 프로젝트가 URP이고 내부 FireSmoke.mat를 보유한다. 입자 수·크기·중첩은 Quest 프로파일링으로 조정하고 머리 양쪽 눈에서 표현을 확인한다. 성능 합격은 현재 주장하지 않는다.

**Alternatives considered**: 전역 fog는 높이 차이 교육을 약화한다. 카메라 연동 알파 감소는 숙이면 위험이 사라진다는 보상이 된다. 유체/볼륨 추가 패키지는 의존성과 비용을 늘린다.

## R5. UI·중단·설정 재사용

**Decision**: 기존 uGUI/TMP·Theme·FreshPressButton·GuidancePresenter를 재사용한다. 새 Canvas/음원/자막은 어댑터에 명시 등록한다. 모달 중단과 콘텐츠 이동 금지를 별개 조건으로 합성한다. 기존 StartContent() 무인자 호출은 콘텐츠 1을 시작하는 호환 경로로 유지한다.

**Rationale**: SharedUIController.ConfigureFollowing은 Canvas 이름 허용 목록이고 음원은 초기 콘텐츠 root에서 한 번 수집된다. 새 root만 추가해서는 동작하지 않는다. ComfortFollowUI는 0.25 m 이상 위치 변화에 반응하므로 숙인 상태의 읽기와 버튼 도달성을 실제 기기에서 검증해야 한다. 연기/환경에는 이 컴포넌트를 적용하지 않는다.

**Alternatives considered**: UI Toolkit 전환, 새로운 설정 UI 복제, 연습 중 텔레포트 허용은 필요 범위를 늘리거나 승인된 제자리 체험과 맞지 않는다.

## R6. 교육 근거·대본·로컬 음성

**Decision**: 핵심 설명은 연기 흡입을 줄이기 위해 머리와 몸을 낮추는 이유다. 실제 대피 문구에는 ‘대피가 가능한 경우’라는 조건을 유지한다. 낮은 자세가 모든 연기 통로를 지나갈 수 있거나 바닥 공기가 안전하다는 뜻으로 확대하지 않는다.

**Rationale / verified sources**:

- [행정안전부·소방청, 다중이용시설 화재 국민행동요령](https://www.mois.go.kr/frt/bbs/type002/commonSelectBoardArticle.do?bbsId=BBSMSTR_000000000530&nttId=117081), 2025-04-14, 2026-11-10 본문 대체텍스트 확인. 대피 가능한 경우 낮은 자세로 이동하며, 불가능한 경우 연기 유입 차단·구조 요청으로 구분한다. 시설 맥락의 안내이며 모든 상황의 자동 대피 명령으로 일반화하지 않는다.
- [조지아주 재난관리기관 House Fires](https://gema.georgia.gov/house-fires), 2026-11-10 확인. 열·연기 상승과 천장 쪽 유해물질 축적, 낮게 이동하는 행동의 보충 근거다. 국내 표준이나 앱의 판정 수치 검증이 아니다.
- [안전한TV 자기집 화재 안내](https://www.safetv.go.kr/base/video/view?idx=2951&menuLevel=&menuNo=&playlistManagementIdx=8), 2026-03-18, 조사 에이전트가 페이지 설명 확인. 자기집 화재의 대피 가능/불가능 구분 참고이며 영상 전체 시청 검증은 하지 않았다.

**Alternatives considered**: 기존 docs/research의 학교 PDF 조사 기록을 이번에 다시 확인한 최신 근거로 취급하지 않는다. 부산 PDF는 크기 제한, 전남 PDF는 캐시 오류로 재조회가 실패했다. 국내 정량 연기층 하강 모델을 확보한 것으로 주장하지 않는다.

**Local assets**: 한국어 음성·동일 의미 자막은 로컬 에셋으로 공급한다. 대본은 contracts/experience-ui.md의 검토용 초안이다. 새 음성 파일은 아직 없고 생성/구매/업로드를 실행하지 않았다. 향후 확보한 음원·텍스처·셰이더의 권리와 배포 조건을 ThirdPartyNotices에 기록한다. 실습자의 마이크 입력은 사용하지 않는다.

**Limit**: 위 자료는 고정 연기층·20%·0.03 m·2초를 안전 기준으로 뒷받침하지 않는다. 자료 대조와 대본 제안은 전문가 검수·교육 효과 검증의 완료를 의미하지 않는다.

## 조사 완료 범위

메뉴 확장, 추적 입력/기준 공간, 판정 방법, 연기 표현, 공통 UI, 교육 대본 근거의 설계 선택을 기록했다. 구현을 막는 기술 선택 미정 항목은 없다. 실제 인식 튜닝·Quest 성능·학습 효과·수신 프로젝트 Import는 quickstart의 실행 검증 항목으로 남긴다.

## F1 음성 통일 — 2026-11-11

사용자 확정: 연기 자세 안내 6개는 콘텐츠 1과 같은 Supertonic 2 F1으로 통일한다. 기존 대본과 교육 흐름을 유지한다. 한국어, 속도 0.95, 16스텝으로 로컬 사전 생성한 WAV를 사용하며 실제 길이에 자막 종료 시각을 맞춘다. 모델·Python은 제출물에 포함하지 않는다. 최종 발음·억양 청취 검수는 미완료다.
