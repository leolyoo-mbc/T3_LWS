# Local content and dependency audit — 2026-11-04

`.utmp/AuditFireContent.cs`, entry AuditFireContent.Main, returned success=true and PASS with six cues, two action options, five bound buttons, thirteen TMP text components, zero external project-asset dependencies and zero missing scripts across the two scenes. All button references resolve to exactly one selected handler and the local input asset; every text uses the local Korean font and has its characters available. The internalized font shader reports no compilation error.

Voice integrity command `.utmp/fire-notification-voice/verify.py` also passed: all six catalog/manifest texts match the approved content script, all hashes and decoded durations match, samples are finite/nonzero, and no clipping was detected. The catalog's clip paths match the manifest, including the user's unchanged ElevenLabs MP3 for shout. The situation uses the accepted sentence-by-sentence revision. Explicit approval versus no issue reported in the remaining cues is recorded in the manifest.

Guidance/subtitle data is separate from stage code; timing validation rejects absent clips, empty text, invalid intervals, duplicate IDs and missing Korean glyphs. The situation has sentence timing from synthesis. Other cues show the full corresponding approved text for their local clip duration; no sentence-level alignment claim is made for them.

Runtime source inspection found no UnityWebRequest, HttpClient, WebClient, DownloadHandler or Microphone usage. No ONNX weights/Python cache are in Assets/_02LWS. This is local-reference/no-fetch inspection; it does not replace T037's fresh-install offline test.

License/source records and copies are in ThirdPartyNotices.md and Documentation/Licenses: Noto OFL; local synthetic Supertonic narration; original geometry/alarm; Unity-dependent TMP shader/include with Companion License; user-confirmed ElevenLabs free-plan output with non-commercial/title-attribution conditions. Menu title, AI disclosure, provider credit and non-commercial note are present. Publication/submission title attribution must still be preserved when distributing.
