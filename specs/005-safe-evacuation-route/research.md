# Research: 안전한 대피 경로 판단

2026-11-13 실제 소스와 명확화 답변을 기준으로 조사했다. 이동, 자산, 음원 경로는 읽기 전용 연구 에이전트로 교차 확인했다. Decision은 계획의 기술 선택이며 새 사용자 확정 요구가 아니다.

## 단일 씬과 공용 UI

**Decision**: ContentExperienceAdapter 상속 + 세 번째 ContentEntry + 기존 두 행동 버튼/최종 결과를 사용한다.

**Rationale**: ContentSceneNavigator는 root, anchor, adapter, 이동 정책과 메뉴 복귀를 제공한다. SharedContentUI.SetActions는 최대 2개를 지원하고 owner/generation 검사를 한다. SharedUIController는 ResultVisible과 안내 종료를 확인한다. A 완료에 최종 결과를 쓰면 다시하기/메뉴 의미가 달라지므로 안내 패널에서 다음 상황으로 넘긴다.

**Alternatives considered**: 새 Scene/rig/UI는 수명, 중복 문제를 늘린다. 기존 화재 controller에 분기를 넣거나 대상별 Canvas를 만드는 것은 독립 콘텐츠/공용화 원칙에 맞지 않는다.

## 고정 앵커와 도착

**Decision**: TeleportationAnchor + 인접 지점 그래프 + 실제 도착 위치 검증. 실제 중단과 이동만 잠그는 상태를 분리한다.

**Rationale**: 기존 TeleportSurface.prefab는 자유 바닥용 TeleportationArea다. 설치된 XRI 3.5.1의 TeleportationAnchor.teleportAnchorTransform 및 provider/orientation 설정을 로컬 소스에서 확인했다. MenuTeleportationProvider.QueueTeleportRequest는 모달/MovementAllowed를 확인하며 CancelPending()을 제공한다. disable만으로 예약 취소를 대신하지 않는다. SharedContentUI는 Blocked이면 버튼도 차단하므로 이동 전용 게이트가 필요하다.

**Alternatives considered**: 클릭을 도착으로 취급하면 이동 실패, 취소에도 원격 선택이 생긴다. 위험한 계단의 앵커를 숨기면 정답을 알려준다. 자유 바닥 이동은 승인된 이동 방식과 다르다.

## 학교 공간과 연기

**후속 합의 반영**: 기존 공간의 단순한 구성을 완성 기준으로 삼지 않고 문/창문 비례, 구조물의 깊이, 천장, 계단 연결을 보완한다(FR-017). 아래 재사용 결정은 자산의 재사용이며 이전 공간 구성의 반복을 뜻하지 않는다. Unity 전체 배치와 필요한 구조물의 Blender 제작은 도구 선택 제안이며 필수가 아니다. 특정 학교 재현 의무는 없다. 세 번째 콘텐츠의 실제 실행 외관을 사용자에게 확인받은 뒤 기존 두 콘텐츠의 후속 적용 범위를 결정한다(SC-008).

**Decision**: 기존 CorridorFinish, plaster/terrazzo CC0 재질, 연기 텍스처를 재사용하며 두 route prefab과 smoke-only presenter를 만든다. 계단 입구/엘리베이터는 로컬 geometry로 만든다.

**Rationale**: 기존 SmokeLayerPresenter.ConfigurationValid는 fireLight와 fireSystems를 요구한다. 막힌 계단 연기에 자세 인식, 불꽃을 함께 복제할 이유가 없다. 전용 학교 계단/엘리베이터 모델은 현재 목록에서 발견하지 못했다.

**Alternatives considered**: 외부 신규 모델 다운로드는 필수가 아니며 권리, 참조, 성능 검토를 늘린다. 기존 원본 복도 prefab 수정은 콘텐츠 1 회귀를 일으킬 수 있다.

## 음성과 증거

**Decision**: 기존 F1 생성 경로로 새 cue/WAV/manifest를 만들고 별도 청취 검수를 남긴다. 실행 검증에는 헌장 v1.3.0을 적용한다.

**Rationale**: VoiceGeneration.md 및 SmokePosture/VoiceManifest.json에 F1, ko, 16 steps, 0.95, 고정 모델 revision이 있다. 기존 승인 범위는 과거 음원이다. ThirdPartyNotices의 AI 음성 고지, 권리 조건을 보존한다. GuidanceCue.Validate는 로컬 음원, 텍스트, 자막 타이밍 일치를 요구한다.

**Alternatives considered**: 온라인 TTS는 오프라인 납품 조건과 다르다. 자막만 넣고 음성 검증까지 완료로 표시하지 않는다. 전체 PC 소리 녹음은 헌장에 위반된다.

## 검증 기반과 성능

**Decision**: Editor-only 도메인/통합 assertion runner를 추적 소스로 남기고 기존 SharedUIContractAudit를 사용한다. Editor/Android/Quest/import를 분리한다.

**Rationale**: 현재 추적된 feature 테스트 폴더는 없으며 이전 probe는 ignored .utmp에 있다. 이를 휴대 가능한 필수 기반으로 삼지 않는다. Performance.md의 Quest 성능은 미측정이며 72 Hz/13.9 ms는 AI 제안 기준이다.

**Alternatives considered**: 계획에서 새 테스트 패키지를 설치하거나 과거 통과 건수를 신규 콘텐츠 합격으로 재사용하지 않는다. 자동 검사로 청취, 기기 편안함을 승인하지 않는다.

## Resolved Unknowns

이동, 도착, 공용 UI, 음원 경로, 연기 경계, 결과 연결, 검증 진입점을 결정했다. 계획을 막는 기술 미결 항목은 없다. 신규 음원 청취 승인과 Quest/import 검증은 구현 이후 실제 수행 항목이다.
