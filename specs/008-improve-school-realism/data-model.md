# Data Model: 학교 건축 자원과 검증 기록

**→ 성격.** 새 게임 상태나 저장 기능을 만들지 않는다. 아래 항목은 Editor 검사와 로컬 검증 기록의 최소 구조이며 모든 런타임 참조는 기존 시스템을 사용한다.

## ArchitectureAsset

| 필드 | 의미 |
| --- | --- |
| assetKey | solid-door-110, glazed-door-088, open-door-130, sliding-window-325, elevator-portal, stairwell-u |
| source/assetPath | Source~ 원본과 FBX/프리팹의 프로젝트 상대 경로 |
| nominalDimensionsMeters | 문짝 또는 창 유리의 기준 폭/높이/깊이, 프레임 바깥 치수와 구분 |
| pivot/front | 바닥 중앙 기준, 정면 -Z, 미터 단위 |
| materialRoles | Paint, Metal, Glass. 기존 바닥 재질은 별도 보존 |

**→ 관계.** 공통 모델1개는 여러 Placement에서 사용한다. 열린 문은 기존 hinge의 local pose와 분리된 자산 변형으로 적용한다. stairwell-u에는 상부 배경 출입문이 포함될 수 있으며 이 문은 기존 교실 문 목록과 구분한다.

**→ 검증.** 단위와 축, 프레임 fit, 법선과 UV2, 공유 재질을 확인한다. 원본 크기를 보존하기 위해 문틀과 손잡이를 비균일하게 변형하지 않는다.

## PlacementBaseline

| 필드 | 의미 |
| --- | --- |
| objectId/hierarchyPath | 변경 전 GlobalObjectId와 보조 계층 경로 |
| role/content/scenario | Door, Window, Skirting, Stairwell, Elevator / 세 콘텐츠 / A 또는 B |
| localPose/worldBounds | Transform과 Renderer bounds, 모델 원본 크기와 구분 |
| colliderState | 종류, enabled, layer, 크기와 pose, 참조 보존 여부 |
| protectedReferences | 앵커, waypoint, floorTargets, smoke와 동작 대상 연결 |
| materialState | 바닥/인물 material GUID와 주요 재질값, UV tiling |
| action | preserve, replace_visual, relocate_opening, remove_skirting |

**→ 관계.** 씬과 프리팹은 서로 다른 소유자다. 동일 외형이어도 원본 prefab entry와 scene entry를 각각 갖는다. 위치가 겹친다는 이유로 다른 역할의 객체를 같은 ID로 취급하지 않는다.

**→ 현재 규모.** 씬 baseline은 실내 문17개, 큰 창10개, 걸레받이29개, 엘리베이터 출입구2곳의 문짝4개와 프레임, 계단실4곳으로 시작한다. 엘리베이터 문짝은 실내 문17개에 포함하지 않고 별도 교체/검사 대상으로 등록한다. 구현 직전 사실 확인값을 기록하고 차이가 나면 원인을 기록한다. 새 상층 배경 문4개는 추가 자원으로 분류하며 기존 문이나 엘리베이터 문짝의 교체 누락을 숨기는 데 사용하지 않는다.

**→ 허용 변화.** 문/창문과 엘리베이터 문짝/프레임의 시각 교체, A Classroom04의 확인된 개구부 겹침 수정, B에서 실제 겹침이 확인된 출입구만의 이동, stairwell geometry와 조명, 지정 걸레받이 삭제를 허용한다. B에서 겹침이 없으면 위치를 유지한다. 보호 참조와 바닥 재질/UV 및 인물 모델은 불변이다. 문 relocation의 Collider는 새 개구부와 일치하도록 함께 옮기며 옛 개구부는 벽과 단순 충돌면으로 닫는다.

**→ 변경 허용 목록.** baseline.json은 변경 전 원본으로 유지한다. 개구부를 수정하기 전에 allowed-changes.json에 씬과 원본 프리팹의 해당 객체 ID, 변경 이유와 근거 화면, 허용할 부품/필드 및 변경 전후 위치를 등록한다. 문, 문틀, 유리, 손잡이, 충돌체와 인접 벽 중 등록된 범위만 검사 예외로 허용하고 앵커/경로 상태/바닥 재질/인물은 예외에 포함하지 않는다. B의 조건부 이동도 같은 규칙을 적용한다.

## Observation

| 필드 | 의미 |
| --- | --- |
| observationKey/targetId | 비교 시점과 관찰 대상 |
| position/rotation/fov | 동일 전후 비교용 시선 |
| headHeight/state/elapsedTime | 높은/낮은 시점, 실제 콘텐츠 단계, 필요시 단계 경과시간 |
| resolution/environment | 캡처 크기, Scene View/Play Mode/실제 기기 구분 |
| beforeFile/afterFile | 실제 이미지 상대 경로 |

**→ 검증.** 같은 관찰 키는 전후 조건이 같아야 한다. 편집 캡처와 실제 실행 캡처를 같은 종류로 표시하지 않는다. 연기 상태를 임의로 제거해 전후 비교를 만들지 않는다.

## ReviewDecision

| 필드 | 의미 |
| --- | --- |
| category | openings, route-structure, lighting, skirting-removal |
| state | pending, approved, changes_requested |
| evidenceFiles | 사용자가 본 실제 자료 |
| userResponse/recordedAt | 실제 응답과 한국 시간 기록 |

**→ 전이.** pending에서 사용자 응답으로 approved 또는 changes_requested가 된다. 수정 후 다시 pending으로 돌아간다. 승인된 범주라도 외형이 다시 바뀌면 pending으로 돌린다. 자동 검사는 approved를 설정하지 않는다.

## ValidationRun

**→ 필드.** runDirectory, baselinePath, actualEnvironment, checksAndResults, performanceSamples, videoMetadata, audioIsolationReference, playbackResult, limitations를 기록한다. 배포용 설정이나 앱 저장 데이터로 로드하지 않는다.

**→ 결과 의미.** 자동 검사 통과, 사용자 외관 승인, Quest 검증, 별도 수신 프로젝트 검사, 지정 최종 프로젝트 수용은 독립 상태다. 자료가 없으면 미검증이며 과거 작업의 완료를 복사하지 않는다.
