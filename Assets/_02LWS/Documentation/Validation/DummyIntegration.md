# Lowpoly Dummy replacement — 2026-11-04

User selected Umplix's Lowpoly Dummy Character as the CC0 replacement. Source and license evidence: ../Licenses/UmplixDummy-Source.txt.

The author's FBX was imported directly, without Blender conversion. Standard Human/CreateFromThisModel automatic import produces Avatar.isValid=true and Avatar.isHuman=true. No skeleton, mesh, bind-pose or keyframe edits were made. Bundled source animations are not imported; the two scene instances use the user's Talking_01 / Talking_02 and Standing Idle Mixamo clips through the existing Conversation_1 / Conversation_2 controllers.

Two Bystander_Dummy instances replace the absent ithappy people at their previous conversation positions. The existing approved flow remains: face each other and talk, then stop talking and turn toward the current fire when the shout clip finishes; reset returns them to conversation. Scene FireVisual and its particle hierarchy were preserved. The only visual adaptation is a local URP material and model scale for scene placement.

Editor Play Mode validation passed for two animated conversations, reaction timing after the shout, facing the actual fire, freeze of pose/rotation, result restoration, reset and cancellation of a pending shout reaction. Rendered conversation and fire-facing states were inspected. Scene dependency scan has zero ithappy references. User visual acceptance and Quest 2 device validation remain pending. This record supersedes the empty-character status in ConversationIntegration.md and the incompatible Quaternius original attempt; those files are retained as history.

Current source and scene are updated; previous APK/unitypackage exports were not rebuilt for this model replacement and remain historical.
