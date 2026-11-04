# US4 — 메뉴 진입·완료·언제든 복귀

2026-11-04, Unity 6000.3.26f1 Editor Play Mode. `.utmp/VerifyUS4.cs` was executed in two separate passes.

| Pass | Actual result |
|---|---|
| EndToEnd | success=true, checks=14, PASS |
| ReturnMatrix | success=true, checks=49, PASS |

EndToEnd used the actual local audio durations and scene coroutines. A programmatic menu pointer down/click started first guidance; correct choice played the supplied shout and bystander reaction; Next restored the initial idle situation and began bell narration; a real XRPokeInteractor moved by the harness crossed the surface/depth threshold; alarm result completed and stopped its loop; completion guidance and return button were available together; pointer activation returned to ContentMenu. There is only one selectable content entry. Menu and experience each had exactly one XR Origin, AudioListener and EventSystem; menu had no remaining voice/alarm AudioSources.

ReturnMatrix started a new scene/session for each of the nine stages, issued duplicate return requests and re-entered. Each session ID was new and began at SituationGuidance; return closed the old session, discarded snapshot and stopped voice/alarm. Rig/listener/EventSystem counts remained one after every return. Wrong-answer feedback also retained menu access and returned cleanly. Duplicate content start requests caused only one load.

These checks use programmatic pointer events, synthetic lifecycle and poke motion. Actual Quest controller UI ray hit geometry, held-trigger across panel/scene transitions, headset signals, install/first offline launch and standalone performance remain device acceptance. Do not equate these Editor results with SC-006–008.
