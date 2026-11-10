# Receiving-project preliminary validation — 2026-11-10

Created a separate Unity 6000.3.26f1 project under .utmp/SmokePostureImport. Initial normal UPM resolution/import startup was cancelled. Retried with the same locally installed package versions embedded in that disposable project, then imported the development unitypackage. Imported official TMP Essential Resources as required by Integration.md, in a separate step before validation.

Final validation process exited with code 0. Result:

```json
{
    "project": "E:/projects/T3_LWS/.utmp/SmokePostureImport/Assets",
    "roots": 4,
    "contents": 2,
    "missingScripts": 0,
    "compileAndReferences": true,
    "domainCompletion": true
}
```

Both content catalogs validated; registered roots/anchors/adapters and required smoke presenters/pose origin were present. No missing MonoBehaviour scripts. Pure domain ready→practice→2-second completion passed. This is a compile/reference/domain check, not headset input, receiver Play Mode whole-flow or designated final-project acceptance.

Development package SHA256: 37B99F639B37C6802488F04683C58EA59E4CDBC7A1B317F74D6F664661AF2C53. This test package contains development narration whose final listening/redistribution check is pending. No final submission package is certified. T044 remains incomplete until its full runtime/receiving-environment requirements and predecessor asset acceptance are completed. Detailed local logs: .utmp/smoke-import-offline.log, smoke-tmp-only.log, smoke-import-final-validation.log.