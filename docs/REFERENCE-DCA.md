# Reference MDT DALI DCA: Static Input Evidence

## Scope of this inspection

On 2026-10-19 the supplied archive was read directly from the conversation attachment in the analysis environment. Inspection used ZIP metadata, the package manifest, the DLL configuration text and simple byte-token checks. No manufacturer assembly was executed; no complete CLR metadata/reflection/decompilation analysis, Windows host experiment or hardware test was performed.

This small inventory is input to Task-006/008, not their full implementation or a passed PoC.

## Artifact identity

| Field | Observed value |
|---|---|
| Original file | `SCN-DA64x-04_MDT_DCA_v4_0_0_0.etsapp` |
| Bytes | 1,730,085 |
| SHA-256 | `cd5bbe33e8b13ba92d7575d33b3ca8f895589f5ec5d87706f847f997cddc3ec9` |
| Archive entries | 112 total |
| Regular files | 108 |
| Directories | 4 |
| Sum of expanded file bytes | 3,393,880 |
| Manifest | `AddInManifest.xml` |
| Manifest AppId | `M0083-A0158` |
| Manifest display name | `SCN-DA641(2).04` |
| Manifest AddInAssembly spelling | `Mdt-DaliDCAPlus.dll` |
| Actual archive DLL spelling | `MDT-DaliDcaPlus.dll` |
| Actual primary DLL bytes | 452,608 |
| Declared Is64BitCompatible | `true` |
| Declared IsSingleInstanceAddIn | `false` |

The manifest and archive differ in assembly filename case. A loader must resolve this deliberately and reject ambiguous case-colliding entries; it must not rewrite the original archive. The bitness flag is a declaration, not proof of successful x64 execution. The filename advertises version 4.0.0.0; full CLR/file-version verification remains a task.

## Declared application program IDs

```text
M-0083_A-0154-10-0000
M-0083_A-0155-10-0000
M-0083_A-0154-11-0000
M-0083_A-0155-11-0000
M-0083_A-0154-40-0000
M-0083_A-0155-40-0000
```

These are the six IDs declared by the package manifest. They do not prove that the user's physical gateway uses one particular ID, mask or firmware. Obtain the matching KNXPROD and actual device identity before selecting a test configuration.

## Configuration and token observations

The included `MDT-DaliDcaPlus.dll.config` declares a supported runtime with `sku=".NETFramework,Version=v4.6.1"`. The same framework token is present in the primary DLL bytes. Tokens for `Knx.Ets.Sdk`, `PresentationFramework`, `WindowsBase` and `IDeviceConfigurationAddIn` are also present.

The configuration file contains binding redirects for `Knx.Ets.Xml.ObjectModel`, `Knx.Falcon`, `Knx.Bus.Common`, `Knx.Ets.DynamicData` and `Knx.Ets.Converter.ConverterEngine`. A binding redirect is not proof that the DCA executes or even requires that assembly on the tested path. In particular, the simple primary-DLL token scan did not find the literal `Knx.Falcon` string; this is neither proof of absence nor permission to assume an independent driver works.

The archive includes English/German localization XML, help/resources, and `Templates/MDT_HCL.xml`. These files demonstrate resource presence only. They do not prove standalone HCL commissioning or equivalence to a web UI.

## What remains unknown

The separate [Task-140 static candidate map](DCA-CONTRACT-MAP.md) now records the
primary assembly identity, selected type references and concrete SDK version
differences. It does not establish which SDK contracts are executed. Remaining
unknowns include whether proprietary concrete/runtime services or activation are indispensable; legal provisioning/redistribution; correct WPF initialization/lifecycle; the real DCA state format; whether bus requests can be mediated through Rust/knust; and complete DALI workflow behavior.

The only provided package is the DALI DCA. It must not be described as the separate firmware-update DCA. No original vendor files or extracted binaries are included in this documentation package.
