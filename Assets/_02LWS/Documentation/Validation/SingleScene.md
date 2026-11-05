# Single scene revision — 2026-11-05

## Implemented and checked

Unity 6000.3.26f1, current Editor, FireNotification.unity. MenuSpace at world origin and FireNotificationSpace at X=40m share one XR rig, EventSystem and AudioListener. Only content 1 is exposed. ContentMenu.unity is retained as an excluded migration source; no runtime scene switching remains in ContentSceneNavigator.

Play Mode authoring verification: **149 checks PASS, 0 failures**. Frames advanced. Checks covered exclusive root activation; tracked-camera position/heading with room-scale offset; duplicate start/return; all nine stages with synthetic suspend/resume, menu return, new session on re-entry, audio cancellation and rejection of a previous run's completion; leaving during wrong-answer feedback; and the natural authored-duration sequence from initial narration through shout, next wait, restore, bell guidance, bell command, alarm, completion and menu. Scene handle and shared rig/input/listener identities remained unchanged.

The first run reported 2 failures because the check waited three frames before suspending the one-frame RestoreSituation transition. The instrumentation was corrected to suspend during that stage and verify its immediate resumed state. Product behavior did not need a change. The fresh second run passed all 149 checks.

The full sequence used controller commands and synthetic lifecycle signals; it is not a physical headset/controller/teleport comfort test. Bell depth and XRI poke are unchanged and are not revalidated by a direct PressBell command.

Fresh Game View captures at the saved rig's normal 1.36m camera height were inspected: menu title/button/disclosures are visible; entering content displays the corridor, NPCs, fire, narration and return button, with no menu overlap. Inspection images and raw authoring checks are local in .utmp; no verification-only component was added to the product. Console errors: 0, compilationFailed: false. Saved-scene audit: missing scripts 0, missing persistent button targets 0, all six navigation references set.

## Remaining prerequisites and acceptance

The existing LwsXRStarterRig (introduced before this revision, Git commit 47a9c33) depends on the XRI 3.5.1 Starter Assets under Assets/Samples. Dependency scan found 40 outside-folder project asset paths. This revision preserves that rig and does not claim a self-contained export. Import the matching Starter Assets sample at the same path/GUIDs before opening the supplied scene, or separately internalize/remap the rig before submission; validate in the actual receiving project. The package contains only _02LWS assets, not those samples. Previous zero-external-dependency records predate the current rig and must not be applied here.

The revised package is regenerated after documentation updates. Quest 2 standalone/first-offline/lifecycle/comfort/performance, clean import of this revision and a new Android APK remain unverified. The previous APK is not the single-scene revision. T037/T038/T041/T042 remain open. AGENTS.md and requirements checklist markers were not edited.

## Raw Play Mode results

```text
Single scene Play Mode checks
PASS player loop advances
PASS starts in menu without session
PASS initial menu tracked head horizontal position
PASS initial menu tracked head heading
PASS initial menu same scene
PASS initial menu shared rig
PASS initial menu shared EventSystem
PASS initial menu shared listener
PASS start switches exclusive spaces
PASS first guidance starts
PASS duplicate start does not reset running session
PASS content entry with head offset tracked head horizontal position
PASS content entry with head offset tracked head heading
PASS content entry same scene
PASS content entry shared rig
PASS content entry shared EventSystem
PASS content entry shared listener
PASS suspend SituationGuidance
PASS resume same stage SituationGuidance
PASS return from SituationGuidance
PASS disposed session/run/snapshot SituationGuidance
PASS no residual audio SituationGuidance
PASS menu from SituationGuidance tracked head horizontal position
PASS menu from SituationGuidance tracked head heading
PASS menu from SituationGuidance same scene
PASS menu from SituationGuidance shared rig
PASS menu from SituationGuidance shared EventSystem
PASS menu from SituationGuidance shared listener
PASS new session after SituationGuidance
PASS late previous completion rejected SituationGuidance
PASS suspend AwaitAction
PASS resume same stage AwaitAction
PASS return from AwaitAction
PASS disposed session/run/snapshot AwaitAction
PASS no residual audio AwaitAction
PASS menu from AwaitAction tracked head horizontal position
PASS menu from AwaitAction tracked head heading
PASS menu from AwaitAction same scene
PASS menu from AwaitAction shared rig
PASS menu from AwaitAction shared EventSystem
PASS menu from AwaitAction shared listener
PASS new session after AwaitAction
PASS late previous completion rejected AwaitAction
PASS suspend ShoutResult
PASS resume same stage ShoutResult
PASS return from ShoutResult
PASS disposed session/run/snapshot ShoutResult
PASS no residual audio ShoutResult
PASS menu from ShoutResult tracked head horizontal position
PASS menu from ShoutResult tracked head heading
PASS menu from ShoutResult same scene
PASS menu from ShoutResult shared rig
PASS menu from ShoutResult shared EventSystem
PASS menu from ShoutResult shared listener
PASS new session after ShoutResult
PASS late previous completion rejected ShoutResult
PASS suspend AwaitNext
PASS resume same stage AwaitNext
PASS return from AwaitNext
PASS disposed session/run/snapshot AwaitNext
PASS no residual audio AwaitNext
PASS menu from AwaitNext tracked head horizontal position
PASS menu from AwaitNext tracked head heading
PASS menu from AwaitNext same scene
PASS menu from AwaitNext shared rig
PASS menu from AwaitNext shared EventSystem
PASS menu from AwaitNext shared listener
PASS new session after AwaitNext
PASS late previous completion rejected AwaitNext
PASS suspend RestoreSituation
PASS resume same stage RestoreSituation
PASS return from RestoreSituation
PASS disposed session/run/snapshot RestoreSituation
PASS no residual audio RestoreSituation
PASS menu from RestoreSituation tracked head horizontal position
PASS menu from RestoreSituation tracked head heading
PASS menu from RestoreSituation same scene
PASS menu from RestoreSituation shared rig
PASS menu from RestoreSituation shared EventSystem
PASS menu from RestoreSituation shared listener
PASS new session after RestoreSituation
PASS late previous completion rejected RestoreSituation
PASS suspend BellGuidance
PASS resume same stage BellGuidance
PASS return from BellGuidance
PASS disposed session/run/snapshot BellGuidance
PASS no residual audio BellGuidance
PASS menu from BellGuidance tracked head horizontal position
PASS menu from BellGuidance tracked head heading
PASS menu from BellGuidance same scene
PASS menu from BellGuidance shared rig
PASS menu from BellGuidance shared EventSystem
PASS menu from BellGuidance shared listener
PASS new session after BellGuidance
PASS late previous completion rejected BellGuidance
PASS suspend AwaitBellPress
PASS resume same stage AwaitBellPress
PASS return from AwaitBellPress
PASS disposed session/run/snapshot AwaitBellPress
PASS no residual audio AwaitBellPress
PASS menu from AwaitBellPress tracked head horizontal position
PASS menu from AwaitBellPress tracked head heading
PASS menu from AwaitBellPress same scene
PASS menu from AwaitBellPress shared rig
PASS menu from AwaitBellPress shared EventSystem
PASS menu from AwaitBellPress shared listener
PASS new session after AwaitBellPress
PASS late previous completion rejected AwaitBellPress
PASS suspend AlarmResult
PASS resume same stage AlarmResult
PASS return from AlarmResult
PASS disposed session/run/snapshot AlarmResult
PASS no residual audio AlarmResult
PASS menu from AlarmResult tracked head horizontal position
PASS menu from AlarmResult tracked head heading
PASS menu from AlarmResult same scene
PASS menu from AlarmResult shared rig
PASS menu from AlarmResult shared EventSystem
PASS menu from AlarmResult shared listener
PASS new session after AlarmResult
PASS late previous completion rejected AlarmResult
PASS suspend CompletionGuidance
PASS resume same stage CompletionGuidance
PASS return from CompletionGuidance
PASS disposed session/run/snapshot CompletionGuidance
PASS no residual audio CompletionGuidance
PASS menu from CompletionGuidance tracked head horizontal position
PASS menu from CompletionGuidance tracked head heading
PASS menu from CompletionGuidance same scene
PASS menu from CompletionGuidance shared rig
PASS menu from CompletionGuidance shared EventSystem
PASS menu from CompletionGuidance shared listener
PASS new session after CompletionGuidance
PASS late previous completion rejected CompletionGuidance
PASS wrong answer feedback active
PASS leave during feedback stops audio
PASS natural stage AwaitAction
PASS natural stage AwaitNext
PASS next wait does not auto advance
PASS natural stage AwaitBellPress
PASS NPC baseline restored after shout
PASS alarm silent before press
PASS natural stage CompletionGuidance
PASS alarm loop stopped at completion
PASS end of full loop same scene
PASS end of full loop shared rig
PASS end of full loop shared EventSystem
PASS end of full loop shared listener
PASS full loop returns to menu
DONE checks=149 failed=0 frame=10553
```
