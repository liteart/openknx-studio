# ETS SDK alternatives investigation

## Result and scope

Task-005 research, 2026-10-21. The operator has licensed ETS6 but no ETS6 SDK
package or KNX membership. The coordinator and predefined explorer independently
reviewed the primary sources below. No working, freely provisioned drop-in
replacement for the reference DCA host contracts was established. This is a
bounded search result, not proof that none exists. No actual SDK terms were
available; no legal prohibition is inferred from missing membership.

Distinguish three questions: obtaining the original SDK contracts, implementing
KNX transport, and providing selected DALI commissioning functions. Solving one
does not establish either of the others. No vendor code was loaded or executed,
no device was contacted, and no third party was contacted.

## Candidates and disposition

| Candidate | Evidence and capability | Disposition for this project |
|---|---|---|
| Official ETS6 SDK | [KNX acquisition instructions](https://support.knx.org/hc/en-us/articles/7588377788562-How-to-get-the-ETS6-SDK) specify active membership and MyKNX Member Resources. [Requirements](https://support.knx.org/hc/en-us/articles/7588671243410-Requirements) describe a NuGet package, not an unrestricted public feed. | Original contract route remains unavailable through the operator's account. An exact lawful source and terms could reopen it. No membership purchase or contact is authorized by this report. |
| Older ETS5 SDK | [KNX ETS5 acquisition instructions](https://support.knx.org/hc/en-us/articles/7588297224082-How-to-get-the-ETS5-SDK) also require membership. | Not an established membership-free route; a framework token in the MDT DLL cannot select an SDK version. |
| Public KNX DCA demos and documentation | [KNX demo page](https://support.knx.org/hc/en-us/articles/7588568212626-Documentation-Demos) lists DCA and ETS sample ZIPs and SDK CHM documentation. | Useful next evidence for API shape if downloaded and inspected privately. Not proof of SDK binaries, redistribution rights or standalone compatibility. |
| `Knx.Falcon.Sdk` 6.4.8671 | The [publisher's NuGet listing](https://www.nuget.org/packages/Knx.Falcon.Sdk/6.4.8671) describes bus and telegram APIs. | Not an ETS DCA host-contract replacement. Do not add a Falcon driver: all generic KNX protocol remains in the knust fork. No Falcon package installed or license conclusion made. |
| XKNX / Calimero | Maintainer repositories describe a [Python KNX library](https://github.com/XKNX/xknx) and [KNX network tools](https://github.com/calimero-project/calimero-tools). | Protocol examples, not evidence of original MDT DCA hosting. No language or protocol ownership change. |
| Community ETS MCP bridge | [Maintainer README](https://github.com/knx-ai/knx-ets-mcp) says its AddIn runs inside licensed ETS. | Does not meet independent runtime requirements. No installation or license/signature bypass path is adopted. |
| Minimal independently implemented compatibility surface | Public docs plus actual CLR reference metadata could identify a small required surface. | An investigation candidate only. Exact type/assembly identity, permitted sources/use, strong names, concrete ETS services and activation remain unknown. No invented shim or full SDK reconstruction is authorized. Stop if unchanged vendor binaries require unavailable identity/services. |
| Manufacturer-supplied SDK-independent functions | See the separate PRO64 candidate below. | Potential partial feature alternative on a different product; not a substitute DLL and not approved scope replacement. |

KNX's [App validation rules](https://support.knx.org/hc/en-us/articles/360001508820-App-validation-rules)
exclude SDK assemblies from app containers and prohibit direct bus access for ETS
apps. These ETS submission rules are contextual evidence, not a legal ruling on
this independent host or proof of this particular package's compliance. Do not
expect an arbitrary `.etsapp` to supply SDK binaries.

## Concrete manufacturer alternative: PRO64 web interface

MDT's [DALI Control PRO64 product page](https://www.mdt-group.com/products/product-detail/actuators/dimming-actuators/dali-control-pro64-gateway.html)
identifies **SCN-DA641P.04S** and advertises DALI commissioning and ballast
replacement through its integrated web interface without ETS. This is a different
product from the selected **SCN-DA641.04 / SCN-DA642.04** reference. The latter's
[product page](https://www.mdt.de/produkte/produktdetail/aktorik/dimmaktoren/dali-control-64-gateway.html)
lists the ETS DCA, not a standalone commissioning tool.

The [PRO64 manual, V1.1, 04/2024](https://www.mdt.ch/fileadmin/user_upload/user_upload/download/SCN-DA641P-04S_MDT_TM_V11_EN.pdf)
qualifies its cover for application V2.1, DCA V2.1.0.0 and firmware V[2]1.0.
Section 2.4 (printed page 13) distinguishes device controls, ETS/DCA and web UI,
warns against simultaneous use, and describes a required ETS parameter/group
assignment download. Its documented web functions therefore do not establish a
complete ETS-free KNX download path, automatic state synchronization or DCA parity.
A new device/profile, firmware evidence, non-secure eligibility and per-function
state/safety tests would be required. No HTTP automation API was established.

This is a real, documented partial alternative worth investigating if the operator
chooses a different reference strategy. It is not a recommendation to purchase
hardware, not a current support claim, and not qualification of the original DCA.

## Smallest next investigation

1. Keep SDK-free, read-only package/CLR metadata inspection distinct from Windows
   DCA execution. Task-006 already specifies that inspection; it needs bounded
   archive checks and verified Rust prerequisites, not SDK objects at runtime.
   Its existing readiness dependencies remain unchanged by this report.
2. Obtain and privately inspect the official demo project/package references when
   accessible. The web tool's attachment fetch failed; a direct bounded public-page
   fetch first failed sandbox DNS, then returned HTTP 403 on authorized retry.
   No demo bytes or API definitions were inspected. Do not report download PASS.
3. Compare actual MDT assembly references and type/member signatures against public
   documentation. Record concrete dependencies, public-key identities, missing
   members and permitted reference provenance before selecting any host approach.
4. If a small independent compatibility surface appears viable, define a separate
   bounded task after the metadata and rights evidence. Require unchanged binaries,
   no identity/signature bypass, honest unsupported services, clean Windows UI and
   real state round-trip tests. A diagram or demo project is not that proof.
5. If it is not viable, present an explicit scope decision: retain blocked original
   DCA hosting, or investigate a native product-specific adapter / different-device
   web workflow. Before replacing the original-DCA-first requirement, obtain the
   operator's explicit choice and update the ADR, goals, R-011/R-012/R-013,
   contracts, scope baseline and affected acceptance tests/gates together.

## Task disposition and checks

Task-005 now explicitly evaluates alternative provisioning and feature routes;
its runtime acceptance criteria remain unmet and its status remains **Blocked**.
Task-003's Windows DCA build and Task-028's original-DCA gate remain closed. No
accepted source pin, scope hash, gate or support matrix was changed to bypass them.
Public source inspection is PASS; demo inspection and compatibility proof are
NOT RUN; a working SDK alternative is INCONCLUSIVE. Source commits, independent
review and offline documentation checks are recorded in Task-005 after execution.
