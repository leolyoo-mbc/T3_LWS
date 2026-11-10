# Interruption validation — 2026-11-10

Editor injected integration verified: 1.9s hold resets on modal pause; smoke and audio freeze; paused completion refused; continue cannot enable smoke locomotion; tracking loss resets hold; reference change invalidates baseline and requires explicit ready. Domain probes verify blocker wins over 2-second threshold, stale callbacks rejected and completed result preserved. Four stages individually confirmed exit and menu cleanup. Full loop repeated 3 times.

Evidence: SmokePostureLoop.md (63 checks, 0 failures), SmokePostureDetection.md (84 checks, 0 failures), SmokePostureReviewFixes.md (17 checks, 0 failures). Device OS interruption/recenter/actual tracking loss is not certified by synthetic inputs.