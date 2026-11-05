# Phase 0 Research — 화재 사실 알리기

2026-11-04. Decision은 조사상 권장 결론이며 사용자 확정을 의미하지 않는다. 초기 씬 분리는 2026-11-05 사용자 요청으로 단일 씬·별도 공간 구성으로 대체되었다. 오답은 최초 후보 작성 후 계획 검토에서 A안을 확정했다.

## 1. 기존 기술 기반

- **Decision**: Unity 6000.3.26f1, URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1, Input System 1.20.0 유지.
- **Rationale**: ProjectVersion, manifest, lock에서 확인. `_02LWS`는 비어 있음. Android OpenXR·Quest support·Touch profile, IL2CPP·ARM64 활성. SDK 최소/타깃 34는 현재 설정일 뿐 보유 기기 설치 성공을 보장하지 않는다.
- **Alternatives considered**: SDK 교체·업그레이드는 현재 요구를 위해 필요하다는 근거가 없다.

## 2. 씬 경계 — 사용자 선택

- **Decision (2026-11-05 사용자 변경)**: 단일 Unity 씬에 주제별 독립 공간 배치. 선택하면 해당 공간으로 이동. 이번 범위는 메뉴와 콘텐츠 1이다.
- **Rationale**: 주제별 독립성을 유지하면서 씬 교체를 없앤다. 공용 rig·입력은 유지하고 공간 루트 전환 및 새 세션 시작/종료로 초기화한다. 거리만으로는 소리·입력이 분리되지 않아 비선택 공간은 비활성화한다.
- **Alternatives considered**: 기존 씬 교체와 모든 주제의 동일 장소 공유는 새 요구와 불일치. 별도 공간을 한 씬에 배치한다.

## 3. 가상 손의 비상벨 밀기 — AI 제안

- **Decision**: controller 기반 XRPokeInteractor + XRSimpleInteractable + XRPokeFilter. 표면 접촉 후 축 방향 깊이 통과만 유효 push. 가상 손과 버튼 캡 이동을 시각화한다. ray/grab 및 단순 접촉을 배제한다.
- **Rationale**: 설치된 XRI 공식 문서는 controller poke, hover/select 평면, 축·깊이·진입각을 지원한다. 샘플 Push Button의 XRPokeFollowAffordance는 deprecated이므로 무조건 제품 의존성으로 삼지 않는다.
- **Alternatives considered**: Trigger enter는 근접 성공 위험. Rigidbody joint 버튼은 떨림·관통·복귀 보정 부담. 샘플 그대로 사용은 외부 에셋 의존성 증가.
- **검토/검증**: 높이·깊이·손 위치 보정, 빠른 밀기, 양손, 벨 안에 손이 남은 채 복귀. 구체적 수치는 실기기 보정 대상이다.
- **Source**: 설치 XRI `Documentation~/poke-interactables-guide-3d-objects.md`, `xr-poke-filter.md`, `xr-poke-interactor.md`; [Unity 공식 3D poke 문서](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.5/manual/poke-interactables-guide-3d-objects.html).

## 4. UI·음성·오프라인 — AI 제안

- **Decision**: world-space Canvas + TrackedDeviceGraphicRaycaster + 단일 XRUIInputModule. UI Press를 trigger에 연결하고 hover/dwell 선택은 사용하지 않는다. 음성·자막 cue와 한글 폰트를 로컬 빌드에 포함한다.
- **Rationale**: 공식 UI 지침에 부합하며 최초 실행 필수 다운로드를 피한다.
- **Alternatives considered**: 온라인 TTS·remote 콘텐츠는 오프라인 제약에 맞지 않는다. 점수·진도 저장은 요구 없음.
- **Source**: 설치 XRI `Documentation~/ui-setup.md`; [공식 UI 설정](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.5/manual/ui-setup.html).

## 5. 앱 중단과 이벤트 취소 — AI 제안

- **Decision**: pause/focus와 OpenXR session focus/user presence를 합성한다. 하나라도 중단이면 진행을 차단하고 모두 해제될 때 현재 단계를 한 번 재진입한다. executionId로 취소된 callback을 무효화한다.
- **Rationale**: Android pause/focus는 동일 신호가 아니다. OpenXRUtility.IsUserPresent는 sensing 미지원 시 true일 수 있으므로 Quest 2의 탈착·시스템 메뉴를 각각 실측해야 한다. 앱 시작 이벤트는 활성 세션이 없으면 재진행하지 않는다.
- **Alternatives considered**: 씬 전체 재로드는 이전 완료 단계까지 잃고 음성 pause만으로는 단계 시작을 재현하지 못한다. timeScale만으로 모든 연출을 정리할 수 없다.
- **Source**: [Unity 6000.3 OnApplicationPause](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html); 설치 OpenXR `Runtime/OpenXRUtility.cs`. 합성 정책·실행 번호는 AI 설계 추론이다.

## 6. 패키지 제출 — AI 제안

- **Decision**: 소유 자산을 `_02LWS` 안으로 복사·재연결하고 패키지·ProjectSettings 조건은 별도 안내. 깨끗한 프로젝트 Import 검증.
- **Rationale**: 현재 rig/input/material/XR 설정은 폴더 밖에 있다. Include dependencies만 켜면 제출 폴더 밖 자료까지 묶일 수 있다. XRI 전역 interaction layer 설정은 임의 이동하지 말고 대상 프로젝트에서 의미 기준으로 매핑한다.
- **Alternatives considered**: 전체 SampleScene 배포는 불필요한 의존성 증가. ProjectSettings 자동 덮어쓰기는 통합 프로젝트에 부적절.
- **조건**: 통합 프로젝트는 미지정. 현재 버전을 기준으로 선행 검증 후 대상 지정 시 재검증. 복사한 리소스 라이선스 유지.

## 7. 첫 선택지와 오답 피드백 — 사용자 확정

공식 자료는 큰소리 또는 비상벨로 주변에 알리는 행동을 뒷받침한다. 아래 문구는 공식 문구 인용이 아니라 AI가 작성하여 사용자 검토를 받은 학습 문구다.

| 구분 | 선택지 | 피드백 |
|---|---|---|
| 정답, 명세 확정 | 큰소리로 알리기 | “불이야!” 음성과 주변 사람 반응 |
| 오답, 사용자 확정 | 다른 사람이 알릴 때까지 기다리기 | 기다리기만 하면 주변 사람들이 화재를 알아차리지 못할 수 있어요. 큰소리로 화재 사실을 알려 보세요. |

계획 검토에서 사용자가 정답+A의 두 항목 구성을 승인했다. 사진 찍기 후보는 채택하지 않았다. 119 신고·비상벨·대피 같은 다른 유효 행동을 포괄적으로 오답 처리하지 않는다. 두 방법을 순차 비교하는 체험을 모든 실제 화재의 강제 순서로 설명하지 않는다.

**Primary sources**:
- [행정안전부 국민안전24 경기장 안전 행동요령](https://www.safekorea.go.kr/safekorea-kor/acts/nacts/action-guide.do?actsHeaderTitle=%EA%B2%BD%EA%B8%B0%EC%9E%A5%EC%95%88%EC%A0%84&category=stadiumSafety&menuSn=4): 비상벨 또는 큰소리로 주변에 알리기. 장소별 다른 대응은 본 콘텐츠로 확장하지 않는다.
- [서울소방 안전교육·훈련 포털](https://firesafety.eseoul.go.kr/multi/train/trainMethod.do): 발견·전파 훈련과 발신기 작동 근거. 시설 대응조직의 전체 절차를 일반 학습자 요구로 확장하지 않는다.

## 공용 객체 — 2026-11-05 변경

- 사용자 확정: 공용 사용 가능한 객체를 공용으로 유지한다.
- 구현 판단: Directional Light는 공간 위치로 영향 범위가 제한되지 않으므로 기본 조명 한 개를 SharedSystems에 둔다. 화재 Point Light는 지역 효과여서 체험 공간에 남긴다. XR rig·EventSystem·공간 전환은 기존 인스턴스를 재사용하고, 앱 중단 감지는 공용 객체로 옮긴다.
- 제외: 주제별 진행·효과·음성은 수명과 기준 상태가 달라 공용으로 합치지 않는다. 공용 에셋 참조는 유지한다.

## Conclusion

필수 기술 선택은 기존 패키지와 사용자 선택으로 정리했다. 후속 계획 검토에서 안내 대본을 확정했다. 구체적 리소스·배치는 선정/보정 대상으로 남긴다. 실기기·성능·Import 검증을 완료했다는 뜻은 아니다.

## 계획 검토 — 자료 준비 현황

- **사용자 확인**: 콘텐츠용 에셋과 음성 자료는 아직 없다.
- **사용자 확정 — 화면 표현**: 단순하면서 실제 복도와 비상벨을 알아볼 수 있는 수준. 교육 행동과 조작 대상의 식별을 우선한다.
- **사용자 확정 — 음성**: 안내·피드백과 플레이어의 “불이야!”는 합성 음성으로 준비한다. 사전 생성한 파일을 로컬 빌드에 포함해 최초 실행부터 오프라인으로 재생한다.
- **사용자 확정 — 비용**: 에셋과 합성 음성 모두 무료 범위로 준비한다. 특정 서비스의 무료 이용·배포 가능 여부는 자료 선택 시 확인한다.
- **미확정**: 구체적인 에셋과 합성 음성 도구·목소리. 무료 조건과 생성 음성의 앱 배포·패키지 포함 조건을 확인해 선택하고 필요한 출처·라이선스를 보존한다. 적합한 무료 자료가 없으면 유료 전환을 임의로 진행하지 않는다.
- **사용자 확정 — 안내**: 차분한 안내자가 상황과 행동의 의미를 짚어 주는 방향 및 [수정 대본](content-script.md) 전체를 승인했다. 최초 튜토리얼 중심 대본은 수정 대본으로 대체했다.
- **기술 판단**: 자료를 선택할 때 로컬 배포 가능 여부, Quest 2 실행 비용, 재배포·unitypackage 포함 조건을 확인한다. 특정 자료가 정해지기 전에는 이용 조건을 충족했다고 간주하지 않는다.

## 편집 모드 공간 표시 — 2026-11-05

사용자 확정: 편집 모드에서는 모든 스페이스를 활성화해 배치를 확인한다. 구현은 MenuSpace·FireNotificationSpace를 활성화한 씬을 저장하는 방식이다. Play 시작 시 기존 ContentSceneNavigator.Start가 메뉴만 활성화하고 체험 공간을 끈다. startOnSceneEntry=false를 유지해 교육은 선택 후 시작한다. Play 종료 시 편집 상태의 두 공간 활성화로 복원되는지 확인한다. Play 중 전체 표시 기능은 추가하지 않는다.

## UI 적용 결정 — 2026-11-05

Decision: design.md v0.1.1을 현재 프로젝트 UI의 기준으로 사용한다. 공통 자원은 전달되지 않았으므로 기존 uGUI/TMP·XRI를 유지하며 `_02LWS/UI`에 내부화한다. 색상·크기 제안은 이 프로젝트 초기값이며 팀 전체 합의로 격상하지 않는다.

Rationale: 기존 UI Prefab과 단일 씬을 재사용하고 단독 Export 경계를 지킨다. 기존 폰트 파일은 지정된 CJK KR Regular와 동일한지 확인되지 않았으므로 새 원본/TMP/라이선스 확인이 필요하다. 패키지 업그레이드는 필요하지 않다.

Alternatives: 문서의 외부 SharedUI 경로는 단독 Export 시 누락 위험이 있어 현재 채택하지 않는다. 전역 timeScale만으로 멈추면 unscaledDeltaTime 자막/연출이 계속될 수 있고 기존 lifecycle 중단 API는 실행을 취소하므로 내부 메뉴의 같은 지점 재개에 그대로 사용할 수 없다. 별도 게이트를 사용한다.

근거 범위: 로컬 design.md, 기존 GuidancePresenter의 unscaledDeltaTime 및 controller의 Suspend 시 취소/Resume 시 Enter 흐름을 확인했다. 새 UI 동작과 기기 가독성은 아직 검증하지 않았다. 원본 폰트 출처와 설치된 XRI 버전의 세부 API는 구현 시 공식 자료로 확인한다.


## 공간 UI 추종 변경 — 2026-11-05

사용자 승인: 이동·텔레포트·고개 회전 후에도 UI를 읽고 조작할 수 있도록 Meta 공식 자료를 반영한다. 일반 메뉴를 머리에 1:1 고정하지 않고, 계속 찾아야 하는 안내/선택/메뉴 UI만 완만하게 따라오게 한다. 비상벨과 Alarm label은 공간 고정이다. 기존 “열 때 고정하고 수동 재정렬만” 규칙은 이 절로 대체한다.

공식 근거: [Meta HUD Placement](https://github.com/facebook/immersive-web-sdk/blob/main/docs/concepts/spatial-ui/hud.md)는 world-space 기본, 필요한 작은 UI의 dead zone/완만한 수평 추종, head pitch/roll 배제 및 헤드셋 보정을 설명한다(Web SDK API 자체를 Unity에 사용하지 않음). [Spatial UI](https://developers.meta.com/vr/design/hands-3d-best-practices/)와 [Ray casting best practices](https://developers.meta.com/vr/design/raycasting_bp/)는 2026-09-14 갱신: 사용자 지향·roll 고정·식별 가능한 조작 방식·충분한 버튼 간격·상태 피드백. 기존 버튼 스타일/레이 입력은 유지한다.

구현 초기값(AI 기술 판단, 공식 표준 아님): 수평 회전 30도, 머리 이동 .25m 초과 때 추종 시작, 지수 보간 계수 6/s. Origin 위치가 한 프레임에 .5m 초과 이동하면 즉시 새 위치에 재배치한다. 눌린 마우스/양손 XR trigger 동안 일반 추종은 정지하며 텔레포트 재배치는 우선한다. 패널 재활성화·수동 재정렬은 즉시 배치한다. 거리 1.5~1.8m 및 패널별 오프셋은 프로젝트 보정값이다. 작은 텔레포트도 확실하게 처리하도록 텔레포트 완료 신호 연결을 검증한다.

검증: 작은 흔들림 유지, 큰 이동/회전 후 수렴, pitch/roll 미전달, 누름 중 안정화, 텔레포트 및 공간 전환 즉시 배치, 꺼진 패널 재표시, 물리 벨 고정, 기존 메뉴/교육 흐름. Editor 결과와 실제 VR 가독성/불편감 검증은 구분한다.


## 벽 부착형 비상벨 재제작 — 2026-11-05

사용자 확정: 공중에 떠 있는 임시 비상벨과 비현실적인 소리를 교체한다. 소화전함 없는 벽 부착형 발신기를 선택했고 Blender MCP로 직접 모델링하도록 요청했다. 기존 안내 대본·손으로 밀어 누르기·진행 순서는 유지한다. 예전 단순 기하/합성 경보음 선정은 이 변경으로 대체한다.

구현 판단: 국내 원형 발신기·표시등·경종의 외형을 참고해 별도 벽 부착판 위에 자체 모델링한다. 원형 발신기 약 144mm, 경종 약 120mm 외경을 참고하되 특정 제품 복제품·인증품으로 표시하지 않는다. 브랜드/KFI 마크와 제조사 사진은 배포 자산에 복사하지 않는다. 눌리는 중앙 부품을 분리하고 모든 런타임 모델·재질·음원은 Assets/_02LWS 안에 둔다. Blender 편집 원본과 재현 스크립트도 보존하고 Unity에서는 FBX를 사용한다.

실제 전기벨 녹음: Dave Welsh, School Bell - Fire Bell.wav, https://freesound.org/people/Dave%20Welsh/sounds/523317/ (2020-06-21), CC0 https://creativecommons.org/publicdomain/zero/1.0/ . 원저자는 솔레노이드 타격식 Edwards 벨 녹음이라고 설명한다. 국내 특정 모터식 경종과 동일한 녹음이라는 주장은 하지 않는다. 연속 타종 구간을 mono PCM WAV로 편집하고 반복 경계를 crossfade한다. 실제 제품 음압을 헤드셋 출력 목표로 사용하지 않는다.

형상 참고: 국내 제조/공급사 카탈로그 https://waf-e.dubudisk.com/ilshinfa.dubuplus.com/yollam%40naver.com/O18B62L/DubuDisk/www/2021%EC%B9%B4%EB%8B%A4%EB%A1%9C%EA%B7%B8.pdf (원형 발신기 144mm·경종 120mm·표시등 87mm 도면, 검색 색인 확인) 및 https://www.alllitelife.com/page.php?page=sub2_3_7 (발신기/경종/표시등 사진 및 경종 120x50mm 도면 직접 확인). 비율·부착판 배치·눌림 깊이와 음량은 프로젝트 초기 조정값이며 법적 설치 규정/모든 제품의 공통 규격으로 표현하지 않는다.

검증: 실제 벽면 부착, 정면 접근/텔레포트, 분리 누름부와 collider 정합, 표면 접촉·잘못된 입력 거부/유효 깊이 성공, 중단·메뉴·초기화, 전체 교육 순환을 Editor에서 확인한다. 실제 VR의 도달성·식별성·소리 자연스러움 및 Quest 성능은 기기 증거 없이는 미완료다.
