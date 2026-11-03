# ADR-017: Bound non-secure tunnel response correlation

Status: Owner decision on 2026-11-03; implementation and acceptance remain in progress.
Related task: [Task-017](../tasks/Task-017.md).

## Context

The non-secure connection response does not provide a per-attempt nonce. The
retained-response diagnostic at fork revision `777fd6d48418b6556cdfef5ff87188037c8ae11b`
demonstrated that old valid bytes from the expected peer, retargeted to a later
local endpoint, can be accepted as a new response. Endpoint reservation in
`c62a89ac03005e415bcba72c19d6242ebc683c4d` reduces accidental local endpoint
reuse within the linked library instance; it cannot prove resistance to
retargeted replies, replay, address spoofing, process restart or other instances.

## Decision

Simon decided that the original absolute cross-session response requirement
need not be strict and authorized a practical bounded operating model. Task-017
therefore requires matching the selected peer and active channel where the
protocol provides one, rejecting malformed responses, bounding waiting and
invalid-input handling, and avoiding local port reuse within the reservation
registry's documented lifetime and capacity. Exhaustion must fail explicitly.

This is a non-secure tunnel for a trusted, controlled network. It does not
authenticate the peer or establish replay resistance. The diagnostic remains
preserved as an acknowledged limitation, not a passing test. No timeout is
invented as a universal maximum packet lifetime. Physical gateway qualification
and all remaining required tests still apply to the actual tested revision.

## Boundaries

This decision does not enable KNX Secure, permit device writes, waive hardware
identity checks, or qualify a different revision from the one tested. Explicit
online testing is coordinated for a particular gateway and operation. A later
security or deployment requirement may demand stronger isolation or a different
protocol; this ADR does not establish safety against an adversarial network.
