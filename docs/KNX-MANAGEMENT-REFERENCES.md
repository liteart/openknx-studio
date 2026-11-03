# Selected KNX management simulation references

Task: [Task-016](../tasks/Task-016.md). The operator supplied KNX Standard
v3.0.0 privately on 2026-10-22. This register identifies the actual technical
inputs used for the selected read-only simulator. It does not certify rights,
publish the documents, qualify a device or authorize hardware access.

## Input identities

The package readme `readme v3.0.0.txt` has SHA-256
`ba5507c44c16def183852108979bc0f9fdfbceea145f8f7b8acd5b5021756ad4`.
The following PDFs are in its `03 Volume 3 System Specifications` directory.
Internal document versions were checked against their title/update pages.

| File | Internal version | SHA-256 |
|---|---|---|
| `03_03_07 Application Layer v02.01.01 AS.pdf` | 02.01.01 | `34047bffc9d1ebc13778da6bf3b0b3f15e71625fa64940f7856d535b50c3fa5d` |
| `03_03_04 Transport Layer v01.02.03 AS.pdf` | 01.02.03 | `4ec1e952d0d9bc75a624467f781e17d01e764e90768a814af7b5084523354d44` |
| `03_06_03 EMI_IMI v01.04.02 AS.pdf` | 01.04.02 | `6a80ead0a3e72efc43749141021b4d9e4d9da9b4d27dcead487bd55b7388c0b7` |
| `03_08_02 Core v01.06.02 AS.pdf` | 01.06.02 | `7c61a3b08e9ecc681e998727661071f8b0799c93d2ea4623f0d03dc95b4ac929` |
| `03_08_04 Tunnelling v01.07.01 AS.pdf` | 01.07.01 | `36d336d2ac8008ac81210b8bdf32ba6788b39c28c62220bfe54ab4a50d3601fd` |

## Selected evidence

PDF pages match printed pages. Relevant wire tables were rendered with native
PDFKit and visually inspected; text extraction alone was not used to determine
the bit boundaries. Private extracts/renderings remain under the operator's
`private/evidence/task016-specs` directory. None enters either repository.

| Selected behavior | Primary locator |
|---|---|
| Property read/response selectors, count/index and error response | Application Layer 3.4.1 p48; 3.4.4.1 p63, figures 46/47 |
| Classic connected memory read/response fields | Application Layer 3.5.3 p109, figures 74/75 |
| Selected transport control values and separate counters | Transport Layer 2 p6, figure 3; 3.7 p13; 3.9 p15; 5.3 pp19-20, actions A1/A2/A3/A7/A8 |
| Mandatory TPCI at cEMI length zero | EMI/IMI 4.1.5.3.2 pp76-77; accepted [Task-149](../tasks/Task-149.md) |
| Basic tunnel connect/disconnect envelopes | Core 7.8.1/7.8.2 pp38-39, figure 36; 7.8.5/7.8.6 p41 |
| UDP tunnel duplicate receipt, CRI/CRD, request/ACK envelopes | Tunnelling 2.6.1 p9; 5.4.3 p26; 5.4.4/5.4.5 p30; 5.4.6/5.4.7 p31, figures 9/10 |

No permitted real-device capture was consumed. The implemented specimen scope
and original vectors live only in the fork's `SIMULATOR.md` and
`tests/support/management_peer.rs`. Property protocol-error injection uses a
zero-element response without value bytes. Memory error injection is explicitly
unsupported by this selected model. No product property meaning, physical
memory map, writable value or real target identity is inferred.

## Owned fixture identities

These SHA-256 values cover each exact APDU specimen before TPCI combination,
not a capture or a claimed manufacturer configuration. Fixture contents and
their assertions remain in the fork, not editor protocol code.

| Fork specimen | SHA-256 |
|---|---|
| `PROPERTY_READ` | `8a7b5a1c49fbd66287bd34c4edcbd9522cbb9302f89598c82ce5894e043e4167` |
| `PROPERTY_RESPONSE` | `763f07408c1ccb1495fed5dda66afabe27c353ee3adb451a04cf7e4df79842ee` |
| `MEMORY_READ` | `cff61f25cae491ae4777d4bee61abe6d00ab3b4dc3fc410a9677824acb8594a3` |
| `MEMORY_RESPONSE` | `fe181537fda148b0ad8325521ee84a7cad374e1db126ee7b57d5a54ff889212e` |

This is a selected deterministic simulation reference register. Full transport
conformance, real timeout/retry behavior, production property/memory services,
Secure, manufacturer compatibility and hardware programming remain separate.
