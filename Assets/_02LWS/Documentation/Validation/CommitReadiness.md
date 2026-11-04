# 커밋 시점의 현재 상태 — 2026-11-04

현재 캐릭터는 Umplix의 CC0 Lowpoly Dummy 두 명이며, 사용자가 가져온 Mixamo Talking_01, Talking_02, Standing Idle을 사용한다. 뼈대 수동 보정 없이 Unity의 자동 Humanoid 설정으로 연결했다. 서로 대화하다가 "불이야!" 음성이 끝나면 불 쪽으로 돌아선다. 불은 로컬 Kenney 텍스처를 사용하는 FireEffect 프리팹이다.

커밋 직전 Editor Play Mode에서 대화 재생, 알림 이후 회전 시점, 실제 불 방향, 일시정지, 결과 상태 복원, 다시하기, 취소된 알림의 뒤늦은 반응 방지를 다시 검사해 통과했다. 실제 음성 길이를 사용하는 메뉴 → 선택 → 큰소리 결과 → 다음 → 비상벨 → 완료 → 메뉴의 14개 검사도 다시 통과했다. 두 씬의 외부 프로젝트 에셋 참조는 0개이며 음성 카탈로그 검증과 컴파일 오류 검사도 통과했다.

최종 확인에서 교체된 Kenney 불의 ScenarioBaseline.fireEffect 참조가 비어 있는 것을 발견해 해당 필드를 연결했다. 불의 파티클 값과 외형은 변경하지 않았다. 이후 실제 ExperienceLifecycle.DebugSuspend 경로로 세 파티클의 재생 → 일시정지 → 복원 검사를 추가 실행해 통과했다.

이전 Quaternius 및 ithappy 관련 검증 문서는 변경 과정의 기록이다. ExportContents.txt와 AndroidBuild.md, ExportImport.md의 산출물 검증은 당시 버전의 결과이며 현재 더미/불 조합을 빌드하거나 다시 내보낸 결과가 아니다. 최신 APK와 unitypackage는 추후 다시 생성해야 한다. Quest 2 실행·입력·성능·최초 오프라인 검증은 여전히 남아 있다.

기존 작업 트리에 남은 환경 설정 변경은 기능과 분리해 커밋한다. uGUI 직접 의존성 및 Post Processing 패키지 추가, TMP 기본 스프라이트 리소스, URP 직렬화/깊이 텍스처 설정, OpenXR 설정, XR 시뮬레이션 기본 환경, 플랫폼별 컴파일 심볼, Unity 서비스 활성화와 빈 Preloaded Assets 목록을 현재 상태대로 보존한다. 빈 Preloaded Assets 목록에는 이전 XR 공통 설정 및 시뮬레이션 설정이 빠져 있다. 이 설정 변경 전체가 필수이거나 Quest 2에서 검증됐다는 의미는 아니다.

미사용 Civilian 재질 8개, 교체 전 shout.wav와 Character Preview 레이어는 참조 검사 후 제거했다. 남아 있는 일부 미사용 Kenney 텍스처와 원시 재질은 같은 리소스 묶음의 여분 파일이다. AGENTS.md와 기존 샘플 씬은 변경하지 않았다.
