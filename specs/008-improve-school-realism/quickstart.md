# Quickstart: 학교 건축 개선 검증

**→ 목적.** 구현 후 세 콘텐츠의 외관과 기존 동작을 확인한다. 이 문서는 검증 안내이며 이번 계획 단계에서 아래 구현이나 검사를 실행했다는 뜻이 아니다.

## 1. 전제와 기준 확보

**→ 환경.** Unity6000.3.26f1, 기존 manifest와 XR 설정, FireNotification 씬을 사용한다. Blender5.2.3은 제작에만 필요하다. 새 검증 진입점은 [계약](contracts/school-architecture.md)의 구현이 완료된 뒤 사용할 수 있다.

**→ 최초 연결.** Unity skill의 최신 CLI 확인과 명령 탐색을 따른다. 상태 조회 실패를 Editor 미실행으로 단정하지 않는다. 현재 세션에서는 CLI만 unreachable이고 Unity MCP는 ready였으므로, 동일 문제가 재현되면 제공된 Unity 전용 연결에서 프로젝트 경로를 확인한 후 같은 읽기/실행 명령을 사용한다.

```powershell
unity self-update --check --format json
unity status --project-path E:/projects/T3_LWS --format json
unity command --query 'script eval' --project-path E:/projects/T3_LWS
```

**→ 변경 전 자료.** 새 `YYYYMMDD-HHmmss-school-realism` 실행 폴더를 만들고 기존 코드/앵커/재질 보존 baseline과 실제 전후 비교 시점을 확보한다. 예시 경로의 시각은 실제 실행 시각으로 바꾼다. 사용자 미저장 변경을 자동 저장하거나 폐기하지 않는다.

```csharp
LWS.FireNotification.Editor.SchoolArchitectureChecks.CaptureBaseline(
    "E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism");
```

**→ 예상 결과.** 반환 baseline.json에 실내 문17개, 큰 창10개, 걸레받이29개, 엘리베이터 출입구2곳의 문짝4개와 프레임, 계단실4곳과 보존 참조가 있다. 엘리베이터 문짝은 별도 교체/검사 대상이다. 차이가 나면 먼저 현재 사실을 확인한다. 구조 모델은 이 자료 이후에 수정한다.

**→ 개구부 변경 전.** A의 확인된 겹침과 B에서 실제 겹침이 확인된 출입구만 allowed-changes.json에 객체/부품/필드, 근거 화면과 전후 위치를 등록한다. baseline은 덮어쓰지 않는다. B에 겹침이 없으면 위치를 유지한다. 앵커/경로 상태/바닥 재질/인물은 예외로 등록할 수 없다.

## 2. 반입과 구조 확인

**→ 반입 검사.** 모델별 실제 미터 치수, 바닥 pivot, 정면 -Z, 음수/이중 scale, 법선, UV2, 재질 매핑을 확인한다. Blender 설치가 없는 수신 환경에서도 FBX/재질 참조가 해결돼야 한다. 자동 collider/light/camera import가 없어야 한다.

```csharp
LWS.FireNotification.Editor.SchoolArchitectureChecks.ValidateAll(
    "E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism/baseline.json",
    "E:/projects/T3_LWS/Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism");
```

**→ 예상 결과.** checks.json failureCount=0. 바닥 material/UV와 인물 참조 유지, 지정 걸레받이 잔여0, 기존 기능 참조 누락0, 새 제품 외부 필수 자원 참조0을 확인한다. 새 상층 배경 문은 기존 교실 문 replacement 목록과 따로 집계한다.

**→ 수동 전수 관찰.** 목록의 모든 실내 문/큰 창문과 엘리베이터 문짝4개/프레임을 정면과 양쪽 사선, 낮은 시점에서 확인한다. 엘리베이터 문짝의 두께/중앙 맞물림/금속 표면과 프레임 깊이, A/B 엘리베이터 뒤 교실 문 노출0, 네 계단실의 중간/상부 연결 막힘0, 제거한 걸레받이 자리의 틈0을 확인한다. 연기 없는 화면을 만들기 위해 제품 효과 설정을 저장 변경하지 않는다.

## 3. 실제 Play Mode 회귀

**→ 연결 검사.** `LWS.FireNotification.Validation.RouteDomainChecks.Run()`과 `RouteIntegrationChecks.ValidateConfiguration()`, `FloorButtonChecks.ValidateConfiguration()`을 실행한다. 오류0을 확인한 뒤 Play Mode에 진입한다. 호출 전 namespace와 현재 메서드 존재를 소스에서 재확인한다.

**→ 콘텐츠1.** 메뉴 진입, 큰소리로 알리기, 반응 확인, 상황 복원, 비상벨 접근과 직접 누르기, 완료, 메뉴 복귀를 확인한다. 닫힌 문의 충돌과 비상벨 가림/도달성이 유지돼야 한다.

**→ 콘텐츠2.** 준비, 원리 안내, 자세 낮추기, 기존 연속 유지 판정, 결과, 메뉴 복귀를 확인한다. 열린 문 상태와 화재/연기 연결은 유지하고 새로운 가구가 없어야 한다.

**→ 콘텐츠3.** `RouteIntegrationChecks.EvidenceDirectory`를 실행 폴더로 지정하고 `RunPlayMode()`를 실행한다. Running=false까지 관찰하고 Failed=0과 실제 로그를 확인한다. 직접 안전계단 완료, 막힌 계단에서 우회, 엘리베이터 거부와 재선택, A/B 전환을 확인한다.

**→ 중단과 버튼.** 세 콘텐츠에서 내부 메뉴 중단/계속/메뉴 복귀를 확인한다. `FloorButtonChecks.RunPlayMode()`도 별도 실행해 구조 변경 뒤 실제 ray/선택/이동 회귀가 없는지 확인한다. 합성 입력 검사를 물리 컨트롤러 검사로 표시하지 않는다.

## 4. 조명, 성능과 증거

**→ 조명 비교.** 콘텐츠1/2 천장등의 같은 시점에서 전후 화면을 비교한다. 검은 얼룩과 좌우 과한 흰 덩어리 해소, 문/벨/계단 식별을 확인하고 메뉴와 A/B에도 새 문제가 없는지 확인한다. 최종 geometry와 bake를 저장한 뒤 다시 연 씬에서도 확인한다.

**→ 성능 비교.** 같은 해상도/품질과 렌더 경로에서10초 준비 후30초씩3회, median/p95 frame time과 batches/triangles를 기록한다. 실시간 캡처 프레임 고정 검사는 성능 측정과 분리한다. 새로 재현되는 멈춤이나 입력 지연은 미해결로 남기며 Editor 값으로 Quest 성능을 합격 처리하지 않는다.

**→ 녹화.** 실제 실행을1920×1080 이상,30fps 이상으로 기록하고 Unity 내부 출력만 담는다. `.utmp/record-route.cs`는 참고 예시로만 사용한다. 같은 AudioRenderer 입력 설정이면 `Artifacts/Validation/VisualEvidence/20261113-150016-audio-isolation/verification.md`를 재사용 근거로 연결하고, 방식/입력 설정이 달라지면 외부 앱 소리 재생 중 Unity 무음/유음 구간을 새로 검증한다. 다른 앱을 끈 것만으로 분리 확인을 대신하지 않는다.

**→ 재생 확인.** 저장 MP4를 열어 실제 해상도/프레임률, 고유한 동작 프레임, 안내와 버튼 식별, 게임 소리의 청취와 화면 동기화를 확인한다. AudioRenderer 캡처용 frame 고정은 끝나면 복원한다. 녹화나 소리/화질 기준을 충족하지 못하면 가능한 영상과 원본 스크린샷 등 대체 자료, 원인과 미검증 항목을 남긴다.

## 5. 사용자 검토와 전달 감사

**→ 한 항목씩.** 문/창문, 경로 구조, 조명, 걸레받이 제거의 네 범주를 하나씩 보여준다. [data-model.md](data-model.md)의 approval 상태를 실제 응답으로 갱신한다. 미승인이나 수정 요청 범주가 있으면 전체 외관 완료로 표시하지 않는다.

**→ 제외 확인.** 실행 폴더가 Git 비추적이고 제품 Assets에 원본 검증 미디어가 없는지 확인한다. 구체적인 실제 실행 경로를 인자로 사용한다.

```powershell
git check-ignore Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism/execution.mp4
git ls-files -- Artifacts/Validation/VisualEvidence/YYYYMMDD-HHmmss-school-realism
```

**→ 예상 결과.** 첫 명령은 제외 규칙이 적용된 파일 경로, 두 번째는 출력 없음이다. 영상과 verification.md를 실제 열 수 있어야 한다.

**→ 개발 패키지.** `_02LWS`, Include dependencies off, 보관용 ContentMenu 제외의 기존 절차로 내보낸다. 새 FBX/재질/프리팹과 bake 데이터의 source/meta 대응 및 의존성을 검사한다. Source~와 검증 원본 포함0을 확인한다. 가능한 별도 수신 환경에서 새 자료를 import해 컴파일과 조명을 확인하고 실행하지 못하면 미검증으로 기록한다.

**→ 완료 보고.** 대표 MP4 또는 대체 자료 파일 링크, 자동 검사, 범주별 사용자 승인, 실제 기기/수신 환경의 확인 범위를 구분한다. 과거 APK와 패키지를 최신 변경 결과라고 부르지 않는다.
