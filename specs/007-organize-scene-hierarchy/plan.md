# Implementation Plan: 씬 전체 하이라키와 영어 명명 정리

**Branch**: 실제 Git 브랜치 `main` | **Date**: 2026-11-17 | **Spec**: [spec.md](spec.md)

**Input**: `specs/007-organize-scene-hierarchy/spec.md`. Spec Kit의 기능 식별값 `007-organize-scene-hierarchy`는 Git 브랜치명이 아니다.

## Summary

현재 단일 씬의 오브젝트를 재사용해 공용 기능과 체험 공간을 나누고, 배경을 구역과 역할로 묶는다. 프로젝트 관리 이름을 영어 PascalCase로 통일한다. 실행 기능을 추가하지 않고 기존 참조, 배치, 조명, UI 레이아웃과 활성화 경계를 유지한다.

구체적인 이동 및 명명 규칙은 [하이라키 계약](contracts/hierarchy-contract.md), 비교 데이터는 [data-model.md](data-model.md)에 정의한다. 런타임 공개 API 변경은 없다.

## Technical Context

**Language/Version**: 기존 C#, Unity 6000.3.26f1. 추가 작업 도구와 검사는 Editor 전용.

**Primary Dependencies**: 기존 uGUI 2.0.0, TMP, XRI 3.5.1, Input System 1.20.0, URP 17.3.0, OpenXR 1.17.1. 설치 또는 업그레이드 없음. Meta SDK 209.0.0와 XR Operator는 개발 환경에 있으나 이번 변경 및 제출 콘텐츠의 필수 의존성으로 사용하지 않는다.

**Storage**: 기존 `.unity` 씬의 직렬화 데이터. 프리팹 변경은 씬 인스턴스의 이름 override만 사용하며 원본 프리팹과 GUID는 유지한다. 기준 자료와 변경 명세는 Git 제외 Artifacts에 보관한다.

**Testing**: Editor 전용 전후 비교, 기존 SharedUIContractAudit, FloorButtonChecks, RouteIntegrationChecks, RouteDomainChecks, Play Mode 경로 검증, 실제 하이라키와 실행 화면 증거.

**Target Platform**: 기존 Quest VR 콘텐츠, Windows Unity Editor에서 계획 및 일차 검증. Quest 실기 결과는 별도 표기.

**Project Type**: 단일 씬 VR 교육 앱의 편집 구조 정리.

**Performance Goals**: 추가 Update, 코루틴, 런타임 컴포넌트, 렌더러, Collider, Canvas, AudioSource와 외부 의존성 0개. 새 오브젝트는 분류용 Transform 그룹뿐이다. 이 범위는 runtime 동작 컴포넌트에 적용하며 Editor 전용 도구는 제외한다.

**Constraints**: 위치 오차 0.0001m 이하, 회전 오차 0.01도 이하, 크기와 RectTransform 수치 오차 0.0001 이하. 텍스트, 참조, 이벤트, 식별자, 활성 상태와 컴포넌트 설정은 동일. 보호 대상 내부는 이름과 부모도 동일.

**Scale/Scope**: `Assets/_02LWS/Scenes/FireNotification.unity` 전체, 공용 UI와 세 체험. 조사 당시 RouteSpace/A 직속 200개, B 직속 20개. 실제 실행 직전의 최신 씬을 비교 기준으로 다시 수집한다.

## Constitution Check

| Gate | Phase 0 | Phase 1 설계 후 |
|---|---|---|
| 사용자 확정 범위와 AGENTS 보존 | PASS, 영어 이름과 구역/역할 분류 확정 | PASS, AGENTS 수정 및 새 교육 기능 없음 |
| 실제 실행 영상과 게임 소리 | PASS, 구현 시 의무로 포함 | PASS, 1920×1080 이상, 30fps 이상 원본 실행 MP4 및 재생 확인 계획 |
| 외부 앱 소리와 마이크 제외 | PASS, Unity 전용 수음 | PASS, 동일 설정의 기존 분리 검증만 재사용 가능, 바뀌면 재검증 |
| 증거 보관과 Git 제외 | PASS | PASS, Artifacts/Validation/VisualEvidence 하위 보관 |
| 한계와 실기 구분 | PASS | PASS, Editor/합성 입력/Operator/실기 및 소리 직접 확인을 구분 |

현재는 문서 작성이므로 새 영상 의무는 적용되지 않는다. 구현 완료 시 확보하지 못한 영상, 소리 또는 실기 항목은 헌법의 대체 자료 및 미검증 보고 절차를 따른다. 계획 Gate PASS는 구현 검증 통과를 의미하지 않는다.

## Project Structure

### Documentation (this feature)

```text
specs/007-organize-scene-hierarchy/
├─ spec.md
├─ plan.md
├─ research.md
├─ data-model.md
├─ quickstart.md
├─ contracts/hierarchy-contract.md
└─ checklists/requirements.md
```

`tasks.md`는 다음 speckit-tasks 단계에서 생성한다.

### Source Code (repository root)

```text
Assets/_02LWS/
├─ Scenes/FireNotification.unity              # 그룹과 이름 변경
├─ Editor/SceneHierarchyOrganizer.cs          # Editor 전용 Capture/DryRun/Apply/Verify
├─ Scripts/Validation/                       # 기존 회귀 검사 재사용, 원칙적으로 수정 없음
├─ Prefabs/                                  # 원본 변경 없음
└─ Documentation/SceneHierarchy.md            # 최종 주요 편집 경로와 명명 예외
Artifacts/Validation/VisualEvidence/<run>/    # 비교 자료, 영상, 화면, 보고서, Git 제외
```

새 Editor 파일과 문서에는 해당 `.meta`를 함께 관리한다. 도구는 `LWS.FireNotification.Editor`에 둔다. 기존 `Editor` 폴더의 에디터 분리 방식을 따른다. 장면 전용 도구이며 다른 씬에 자동 적용하지 않는다.

**Structure Decision**: 한 번에 문자열을 치환하는 YAML 편집 대신 Unity Editor에서 기존 오브젝트의 부모와 이름만 변경한다. 기존 씬 오브젝트 ID와 프리팹 연결을 보존한다. 프리팹 내부에는 분류 그룹을 삽입하지 않으며 분류가 필요한 루트는 통째로 옮긴다. 프로젝트 소유 프리팹 내부의 표시 이름은 씬 override로만 바꾼다.

## Implementation Sequence

1. 최신 저장 씬, 소유 프리팹과 참조를 조사하고 전후 비교 기준을 수집한다. Play Mode 또는 저장되지 않은 사용자 변경이 있으면 적용하지 않는다. 기존 세 체험 경로와 검사의 실패 여부를 변경 전에 기록한다.
2. 계약 규칙으로 모든 대상을 `MoveAndRename`, `RenameOnly`, `Protected` 중 하나에 배정한 변경 명세를 생성한다. 누락, 중복, 모호한 분류가 있으면 Apply를 막는다. 매칭에는 기존 오브젝트 ID와 원래 sibling 순서를 사용한다.
3. 공용 기능, UI, 체험별 구역과 배경 분류 순서로 적용한다. 런타임 제어 컴포넌트는 기존 오브젝트에 유지한다. 한 Undo 그룹으로 묶고, 저장 전 비교 실패 시 그 적용분을 되돌린다.
4. 저장하고 같은 씬을 다시 열어 ID, 연결, 배치, 보호 대상과 이름 규칙을 재검사한다. 이미 정리된 씬에 다시 DryRun하면 변경 0개여야 한다.
5. quickstart의 기존 정적 검사, 전체 floor/route 회귀와 세 체험 주요 경로를 수행하고 시각 자료를 남긴다. 기존 환경 실패도 숨기지 않는다. 새 실패는 해결하고 기존 실패는 원인과 미검증 범위를 구분한다.
6. 최종 편집 경로와 보호 예외를 SceneHierarchy.md에 기록한다. 변경 파일 범위와 제출 의존성을 검토한다. 자동 커밋, push, APK나 unitypackage 생성은 하지 않는다.
