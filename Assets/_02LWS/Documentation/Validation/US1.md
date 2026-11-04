# US1 — 큰소리로 화재 알리기

2026-11-04, Unity 6000.3.26f1 Editor Play Mode, FireNotification scene. Authoring command `.utmp/VerifyUS1.cs`, entry `VerifyUS1.Main`, returned success=true, checks=15, result=PASS. The check executes real scene components/coroutines and programmatic pointer events; it does not claim physical controller tracking validation.

| Scenario | Observed |
|---|---|
| Direct first guidance | Local audio playing with Korean subtitle; choice panel hidden |
| 30 seconds no input | AwaitAction retained same executionId |
| Pointer hover | No action chosen |
| Wrong answer | Local feedback playing, two choices hidden/locked; after feedback, same choices reappear |
| Answer during feedback | Rejected; no shout result |
| Correct answer | User-supplied local shout starts once; choices hidden; bystander reaction; then AwaitNext |
| Repeated pointer click / command | No duplicate shout or execution restart |
| Suspend explanation | Audio/subtitle stop, snapshot captured; resume same explanation with new execution |
| Suspend choice wait | Same wait, new execution, no automatic answer |
| Suspend shout result | Audio stops; resume same result from start, old completion ignored |

During verification, a missing serialized prefab-instance UnityEvent override prevented the wrong-answer button from dispatching. Recorded the instance property modifications and saved the scene. The original failing pointer-event scenario was rerun and all 15 checks passed.

Real Editor window focus loss also triggered expected suspension; the deterministic sequence uses the nonserialized Editor-only lifecycle isolation hook. No production lifecycle input is bypassed in player builds. Physical Quest rays/held-trigger behavior, standalone rendering/audio and headset removal remain T037 acceptance items. No Quest success claim is made here.
