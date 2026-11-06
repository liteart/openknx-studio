# ADR-019: Preserve ambiguous classic property failures

Status: Owner decision on 2026-11-06.
Related task: [Task-019](../tasks/Task-019.md).

## Context

The selected classic KNX property response can return the same zero-count,
empty-data form for an unavailable property, insufficient access rights or
another remote application problem. That response alone does not identify the
cause. Independent review of the operator-supplied KNX Application Layer
v02.01.01 §3.4.4.1 pp63–65 confirmed this limitation; the standard itself
remains private and is not reproduced here.

## Decision

Simon approved reporting unavailable and access-denied cases together when
the response cannot distinguish them. The generic service retains a structured
`RemoteFailure` rather than guessing a cause. A future user-facing presentation
may explain that the property is unavailable, access was denied, or another
device-side error occurred. Local validation errors, timeouts, malformed
responses and gateway rejection remain distinct.

This changes Task-019 DR-004 only for response forms that carry no distinct
cause; where a future verified service provides a distinction it must preserve
it. This decision does not claim that a failed property is definitely missing
or that access was definitely denied, nor does it authorize device writes.
