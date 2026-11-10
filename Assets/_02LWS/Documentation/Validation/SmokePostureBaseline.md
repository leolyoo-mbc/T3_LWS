# Smoke posture baseline — 2026-11-10

Unity 6000.3.26f1 / existing manifest and asmdefs retained. Initial active scene FireNotification, clean, 3 roots. Console errors 0, pre-existing warning 1. Git: user-modified Assets/XR/Settings/OpenXRPackageSettings.asset; specs/002-smoke-low-posture untracked. User XR settings must be preserved.

Connected Editor verified by MCP (CLI could not reach localhost; update check DNS unavailable). Play frames advanced 1→1503→3567. Existing menu starts content; situation reached AwaitAction, shout selection reached AwaitNext. Full bell and modal regression is a later foundation checkpoint.

Q1 uses Editor sample injection/manual assertions, no new test framework. Actual Quest and clean-project Import remain separate checks. Ignore file already covers Unity Library/Temp/Obj/Build/Logs, bin, IDE files and exported binaries. Do not ignore Packages/ or Unity assets.