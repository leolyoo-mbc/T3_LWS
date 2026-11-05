# Meta 공간 UI 추종 검증 — 2026-11-05

구현: ComfortFollowUI, SharedUIController. 소유 자원은 Assets/_02LWS 안에 유지한다. XR rig 높이/카메라 오프셋은 변경하지 않았다. 물리 벨/Alarm label은 공간 고정이다. runtime에 명시적 Canvas 이름 목록의 8개 패널에 컴포넌트를 연결하므로 이름 변경 시 연결 목록도 갱신해야 한다.

근거: https://github.com/facebook/immersive-web-sdk/blob/main/docs/concepts/spatial-ui/hud.md (2026-11-05 조회; Web SDK API가 아닌 설계 원칙 적용), https://developers.meta.com/vr/design/hands-3d-best-practices/ , https://developers.meta.com/vr/design/raycasting_bp/ (두 문서 2026-09-14 갱신). 고정 head HUD 회피, 필요한 UI의 dead zone/수평 추종, 상태 식별과 기존 버튼 피드백 유지.

기술 초기값: 30도/.25m dead zone, 지수 보간 6/s, 거리 1.5~1.8m. 공식 표준값/기기 합격값이 아니다. Origin .5m 초과 불연속 감지와 TeleportationProvider.locomotionEnded를 함께 사용해 짧은 텔레포트도 즉시 재배치한다. 패널 재활성화/수동 재정렬도 즉시 배치한다. 누름 중 일반 추종은 정지한다.

Editor Unity 6000.3.26f1 결과: ComfortFollow-Editor.txt 8/8, ComfortFollow-Input.txt 5/5, ComfortFollow-Regression.txt 111/111 통과. 실제 .1m 텔레포트 요청/적용/즉시 배치 확인. 콘솔 현재 오류 0, 컴파일 실패 없음.

검증 한계: 포커스 없는 Editor는 지속 mouse 입력을 해제하여 초기 누름 유지 테스트가 실패했다. 최종 누름 고정 검사는 가상 Mouse에 상태 이벤트를 전달한 직후 InputSystem.Update 후 LateUpdate를 직접 호출해 분기에서 위치 불변을 확인했다. 실제 VR 장시간 누름/레이/추종 편안함을 대체하지 않는다. 테스트 장치는 제거하고 입력 backgroundBehavior를 복원했다. Play 시작 전/종료 후 편집 모드 유지.

실기기 T066/T060, 기존 T037/T038/T041/T042는 미완료다. 새 APK 생성 없음. 패키지 원본/meta 대조는 Artifacts/SharedUIPackageAudit.json에서 확인한다. 전후 훅 설정 .specify/extensions.yml 없음.
