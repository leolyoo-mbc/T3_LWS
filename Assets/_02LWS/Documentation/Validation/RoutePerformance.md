# 경로 콘텐츠 Editor 성능

2026-11-13, NVIDIA GeForce RTX 2060, Intel i5-12400F, Low 품질, XR Simulator 표시 refresh 72Hz, 카메라 pixelRect 1680x1760. 실제 Quest 결과가 아니다. 녹화 없이 각 공간에서 10초 워밍업 후 30초씩 3회 측정했다.

CPU는 Main Thread inclusive marker로 대기와 Editor 비용을 포함한다. GPU는 FrameTimingManager다. 아래는 3회의 median 범위와 p95 최댓값이며, 원본 15행은 E:\projects\T3_LWS\Artifacts\Validation\VisualEvidence\20261113-214112-safe-evacuation-route-performance\samples.csv에 있다.

| 공간 | CPU median ms | CPU p95 max ms | GPU median ms | GPU p95 max ms | batches | 입자 max |
|---|---:|---:|---:|---:|---:|---:|
| menu | 13.8693–13.8810 | 14.7666 | 1.1581–1.2060 | 2.0040 | 105 | 0 |
| fire | 13.8803–13.8912 | 14.8504 | 1.1776–1.2815 | 2.1053 | 162 | 56 |
| smoke | 13.8884–13.8972 | 14.9856 | 1.6814–1.7428 | 2.4479 | 125 | 169 |
| route-a | 13.8702–13.8799 | 14.8242 | 1.1612–1.3517 | 1.9162 | 384 | 0 |
| route-b | 13.8797–13.8932 | 14.9553 | 0.7498–1.3187 | 1.9476 | 262 | 74 |

Editor 합성 버튼 입력에서 피드백 상태 변경과 화면/음성 재생 명령 전달까지 같은 콜백 내 0.315ms를 측정했다. 실제 헤드셋 입력부터 화면까지의 지연은 미측정이다. 1초는 계획 목표, 72Hz/13.9ms는 참고값이며 사용자 확정 합격 기준으로 삼지 않는다. Quest CPU/GPU 성능과 편안함은 미검증이다.
