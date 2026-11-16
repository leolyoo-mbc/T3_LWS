# Data Model: 편집 구조와 전후 비교

## SceneBaseline

- scenePath, sceneAssetGuid, capturedAt, savedSceneHash, prefabSourceHashes.
- 모든 기존 GameObject와 Component의 GlobalObjectId, 타입, 원래 이름, 부모 ID, sibling 순서, activeSelf, enabled, tag, layer, static flags.
- Transform world position/rotation/lossyScale과 RectTransform의 anchorMin/Max, pivot, sizeDelta, anchoredPosition3D, offsetMin/Max, localRotation/localScale 및 world corners.
- SerializedObject 기준 모든 직렬화 property의 경로, 타입과 값. 오브젝트 참조는 세션 InstanceID 대신 GlobalObjectId 또는 asset GUID/local file ID로 정규화한다. UnityEvent 대상, 메서드와 인자도 포함한다.
- Renderer/Light/Collider/Canvas/AudioSource, 라이트맵 인덱스와 scale-offset, material/mesh, 프리팹 원본 연결과 overrides도 비교 대상으로 포함한다.
- 데이터는 Artifacts/Validation/VisualEvidence/<run>/baseline.json에 보관한다. 기준 수집은 저장된 현재 씬에서 수행하며 기존 사용자 변경을 자동 저장하지 않는다.

## OrganizationEntry

- objectId, originalPath, originalSiblingIndex, disposition, targetParentId 또는 새 그룹의 targetPath, targetName, reason. 변경 명세 전체에 scope를 기록하며 StructureOnly, NamesOnly, Full 중 하나를 사용한다.
- disposition은 MoveAndRename, RenameOnly, Protected 중 하나. 최상위 유지 그룹도 Protected로 기록하되 자손의 보호 여부는 개별 기록한다.
- 새 그룹은 별도 GroupEntry로 관리하며 fullPath, parentPath와 생성 순서를 가진다. 한 객체가 둘 이상의 목적지에 배정되거나 누락되면 오류다.
- NamesOnly는 현재 부모, Full은 최종 부모 아래에서 targetName이 고유해야 한다. 번호는 각각의 부모별 원래 sibling 순서로 배정하며 기존 식별자나 월드 좌표 순서로 다시 정렬하지 않는다. NamesOnly 임시 번호를 Full에 재사용하지 않는다. StructureOnly는 기존 이름을 보존하고 새 그룹 이름만 목적지 계약을 따른다.
- 구현 직전 생성하는 organization-plan.json은 모든 기존 대상을 명시적으로 포함한다. 모호한 대상은 임의의 Misc로 보내지 않고 DryRun 오류로 보고한다.

## ProtectionRule

- 보호 범위, 고정 이름, 부모 변경 허용 여부, 변경 사유를 기록한다.
- XR origin 전체 자손, 캐릭터 Animator 아래 자손, 외부/Starter Assets 내부 및 자동 생성 TMP 보조 요소는 이름과 내부 부모 모두 고정이다.
- FireAlarm 내부는 부모/로컬 좌표 고정, 프로젝트 관리 이름만 RenameOnly 허용. FloorButtonPanel과 공용 UI도 내부 부모/순서/RectTransform 고정, 이름 override만 허용한다.
- `SharedCanvas`, `FloorCanvas`의 이름 고정. 컴포넌트와 참조 연결은 전체 대상에 걸쳐 고정이다.

## ComparisonResult

- scope, missingObjects, unexpectedObjects, changedReferences, changedProperties, transformDifferences, protectionViolations, namingViolations, emptyGroups, unclassifiedObjects와 결과 PASS/FAIL. 해당 scope의 미적용 기준은 PASS가 아니라 NotEvaluated로 기록한다.
- 허용되는 변경은 명세에 기록한 이름, 부모, 해당 이동에 필요한 Transform 로컬 좌표와 분류 그룹 추가뿐이다. m_Children 및 sibling 변화도 승인된 그룹 이동과 일치해야 한다.
- world position 0.0001m, world rotation 0.01도, world scale 0.0001을 초과하면 실패한다. 보호 대상의 로컬 값과 UI RectTransform 속성 및 world corners는 0.0001 이하만 허용한다.
- 기존 컴포넌트의 ID와 개수는 모두 유지한다. 해당 scope에서 새로 만든 그룹 하나마다 Transform 하나만 추가한다. 전체 오브젝트 수와 전체 컴포넌트 수는 각각 기존 수 + 해당 scope의 승인된 신규 그룹 수여야 한다. NamesOnly의 신규 그룹 수는 0이다.
- 모든 다른 직렬화 값은 동일해야 한다. 경로 문자열을 담은 애니메이션은 보호 대상 내부 경로가 동일한지 확인한다.

## Validation scope

| scope | 허용 변경 | 범위별 검사 |
|---|---|---|
| StructureOnly | 승인된 부모 이동, 필요한 Transform 로컬 좌표 및 분류 그룹 추가. 기존 이름은 유지 | 구역/역할 분류, 빈 분류 그룹과 미분류 여부. 최종 영어 이름 및 기존 이름 중복 기준은 NotEvaluated |
| NamesOnly | 기존 부모 아래 이름만 변경. 그룹 생성과 부모/sibling 변경 금지 | 영어 명명과 현재 부모 아래 중복. 최종 구조 기준은 NotEvaluated |
| Full | 계약의 최종 구조와 이름 | 모든 요구사항, 최종 부모별 이름/번호, 구조와 저장/재개방 검사 |

참조, 컴포넌트 ID/수, 월드 배치, 한국어 문구, 활성 상태, 보호 대상과 UI 레이아웃 보존은 세 scope 모두 필수다. 기존 이름 또는 부모가 미적용 범위라는 이유로 이 보존 검사를 생략하지 않는다. 임시 scope는 적용/비교 후 Undo하고 baseline 복구를 비교한다. 제품 씬 저장은 Full 검증을 통과한 경우에만 허용한다.

## Workflow state

Captured → Planned → AppliedUnsaved → VerifiedUnsaved → Saved → ReopenedVerified.

StructureOnly와 NamesOnly는 VerifiedUnsaved 후 Undo로 baseline 복구를 확인하고 Planned로 돌아간다. Saved와 ReopenedVerified는 Full에서만 허용한다. 기준과 무관한 사용자 변경으로 씬 해시 또는 실제 대상이 변하면 Captured로 돌아가 새로 조사한다. 계획 적용 중 실패하거나 저장 전 비교가 실패하면 Undo로 해당 적용을 되돌린다. 실패 상태를 저장하거나 자동 재시도해 부분 변경을 남기지 않는다. 완료된 씬의 DryRun은 NoChanges를 반환한다. 런타임 데이터 및 저장 형식 변경은 없다.
