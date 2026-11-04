# US3 — 두 방법 연결과 상황 복원

2026-11-04, Unity 6000.3.26f1 Editor Play Mode. `.utmp/VerifyUS3.cs`, entry `VerifyUS3.Main`, returned success=true, checks=9, result=PASS.

Observed: AwaitNext reconstructs the finished bystander reaction, starts approved next narration and enables Next immediately. After 30 seconds, including narration completion, stage/execution remain unchanged. Hover does not continue. A fresh programmatic pointer down/click advances through RestoreSituation to BellGuidance; the repeated Continue command does not skip guidance. People return to idle and the playing clip is bell guidance, with no previous shout/next audio. Suspension/resume of next wait restarts that wait/narration. Suspension during RestoreSituation retains that stage; resume restores again and reaches BellGuidance, ignoring the old run completion.

This verifies controller, real coroutine and programmatic UI adapters in Editor. Quest controller held-trigger, transition races under device focus/presence and actual headset comfort remain T037 acceptance work.
