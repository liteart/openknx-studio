# Test Evidence: Case identifier

## Identity

Task ID; test case ID; expected acceptance criterion; code/fixture versions; actual result PASS / FAIL / INCONCLUSIVE / NOT RUN.

## Reproduction data

Exact source commits, feature flags, command/arguments, OS/architecture/runtime, fixture hashes and generator seed. Record actual wall-clock timestamps only in evidence, not deterministic compiler inputs.

## Hardware authorization when applicable

Isolated bench, gateway, physical target identity, product/application/mask/firmware, approved operation/ranges/artifacts, before-state, recovery procedure and human operator grant. No authorization means no write test.

## Expected and actual result

Describe the observable difference. Include raw output/evidence locations and hashes. Never create expected golden output using only the function under test without an independent oracle.

## Cleanup and residual state

Recorded restoration/verification, retained unknown state, interrupted operations, and required next human action. A process exit or Git restore is not device rollback.

## Limitations and review

Untested cases, simulation assumptions, missing environments/inputs, reviewer and final disposition. State whether this evidence qualifies one exact device/version or only an offline code path.
