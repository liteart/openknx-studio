// SPDX-License-Identifier: Apache-2.0
"use strict";

const element = (id) => document.getElementById(id);
let csrf = null;
let enrolled = false;
let busy = false;
let prepared = null;
let refreshTimer = null;
const PREPARED_LIFETIME_MS = 100000;

function controls() {
  element("enroll").disabled = !csrf || busy || enrolled;
  element("reviewed").disabled = !csrf || busy || !enrolled;
  element("assert").disabled = !csrf || busy || !enrolled || !element("reviewed").checked;
  element("stop").disabled = !csrf || busy;
  element("enroll").textContent = prepared?.kind === "enroll"
    ? "Create temporary test passkey" : "Prepare temporary test passkey";
  element("assert").textContent = prepared?.kind === "assert"
    ? "Verify synthetic plan with passkey" : "Prepare plan verification";
}

function decode(value) {
  const padded = value.replace(/-/g, "+").replace(/_/g, "/") + "=".repeat((4 - value.length % 4) % 4);
  return Uint8Array.from(atob(padded), (character) => character.charCodeAt(0));
}

function encode(buffer) {
  let value = "";
  for (const byte of new Uint8Array(buffer)) value += String.fromCharCode(byte);
  return btoa(value).replace(/\+/g, "-").replace(/\//g, "_").replace(/=/g, "");
}

async function post(path, data = {}) {
  const response = await fetch(path, {
    method: "POST", mode: "same-origin", cache: "no-store", credentials: "same-origin",
    headers: { "Content-Type": "application/json", "X-OpenKNX-Development-CSRF": csrf },
    body: JSON.stringify(data),
  });
  if (!response.ok) throw new LocalRequestError(response.status);
  return response.json();
}

class LocalRequestError extends Error {
  constructor(status) { super(); this.status = status; }
}

function diagnostic(stage, error, gesture = null) {
  const safeNames = new Set(["NotAllowedError", "SecurityError", "NotSupportedError",
    "InvalidStateError", "ConstraintError", "AbortError", "UnknownError", "NotReadableError",
    "TypeError", "DataError", "EncodingError", "OperationError", "NetworkError", "SyntaxError"]);
  let code = "unavailable error name";
  if (error instanceof LocalRequestError && Number.isInteger(error.status)
      && error.status >= 400 && error.status <= 599) code = `HTTP ${error.status}`;
  else if (safeNames.has(error?.name)) code = error.name;
  const activation = gesture === null ? "" : `; user gesture ${gesture}`;
  return `Passkey test stopped during ${stage} (${code}${activation}). You can prepare a new attempt. No device authorization was issued.`;
}

function clearPrepared() {
  prepared = null;
  if (refreshTimer !== null) clearTimeout(refreshTimer);
  refreshTimer = null;
}

function decodeOptions(options, kind) {
  const publicKey = options.publicKey;
  publicKey.challenge = decode(publicKey.challenge);
  for (const field of ["excludeCredentials", "allowCredentials"]) {
    if (publicKey[field]) for (const credential of publicKey[field]) credential.id = decode(credential.id);
  }
  if (kind === "enroll") publicKey.user.id = decode(publicKey.user.id);
  return publicKey;
}

async function prepare(kind, retainedStatus = null) {
  busy = true; clearPrepared(); controls();
  if (!retainedStatus) element("status").textContent = "Preparing a short-lived challenge. No passkey prompt has started.";
  try {
    await post("/api/cancel");
    const publicKey = decodeOptions(await post(`/api/${kind}/start`), kind);
    prepared = { kind, publicKey, at: performance.now() };
    refreshTimer = setTimeout(() => {
      if (!busy && csrf) void prepare(enrolled ? "assert" : "enroll", retainedStatus);
    }, PREPARED_LIFETIME_MS);
    element("status").textContent = retainedStatus || (kind === "enroll"
      ? "Ready. Choose Create temporary test passkey to open your browser's passkey prompt."
      : "Ready. Review the exact plan, then choose Verify synthetic plan with passkey.");
  } catch (error) {
    const preparationStatus = diagnostic(`${kind === "enroll" ? "enrollment" : "assertion"} challenge preparation`, error);
    element("status").textContent = retainedStatus ? `${retainedStatus} ${preparationStatus}` : preparationStatus;
  } finally { busy = false; controls(); }
}

function serialize(credential) {
  const response = { clientDataJSON: encode(credential.response.clientDataJSON) };
  for (const field of ["attestationObject", "authenticatorData", "signature"]) {
    if (credential.response[field]) response[field] = encode(credential.response[field]);
  }
  if ("userHandle" in credential.response) {
    response.userHandle = credential.response.userHandle ? encode(credential.response.userHandle) : null;
  }
  return { id: credential.id, rawId: encode(credential.rawId), type: credential.type,
    response, extensions: credential.getClientExtensionResults() };
}

async function ceremony(kind) {
  if (busy || !csrf) return;
  if (!prepared || prepared.kind !== kind || performance.now() - prepared.at >= PREPARED_LIFETIME_MS) {
    await prepare(kind);
    return;
  }
  const publicKey = prepared.publicKey;
  clearPrepared();
  busy = true; controls();
  element("status").textContent = "Confirm using the passkey provider chosen in your browser.";
  let stage = `${kind === "enroll" ? "enrollment" : "assertion"} passkey prompt`;
  const gesture = navigator.userActivation ? (navigator.userActivation.isActive ? "active" : "inactive") : "unavailable";
  let retainedStatus = null;
  try {
    // The options are already fetched and decoded. The WebAuthn API is invoked
    // directly in the click handler's activation, before any asynchronous wait.
    const credential = kind === "enroll"
      ? await navigator.credentials.create({ publicKey })
      : await navigator.credentials.get({ publicKey });
    if (!credential) throw new Error("Passkey ceremony did not complete.");
    stage = `${kind === "enroll" ? "enrollment" : "assertion"} response encoding`;
    const response = serialize(credential);
    stage = `${kind === "enroll" ? "enrollment" : "assertion"} verification`;
    const result = await post(`/api/${kind}/finish`, response);
    if (result.physical_authorization !== false) throw new Error("Unexpected response.");
    if (kind === "enroll") {
      enrolled = true;
      retainedStatus = "Temporary test passkey enrolled. Review the plan below to test an assertion.";
    } else {
      retainedStatus = "Passkey assertion verified for this synthetic plan. No device authorization was issued.";
      element("reviewed").checked = false;
    }
  } catch (error) {
    // Only a fixed stage, allowlisted exception name or HTTP status and a boolean
    // activation observation are shown. Messages and credential data stay private.
    retainedStatus = diagnostic(stage, error, stage.endsWith("passkey prompt") ? gesture : null);
  } finally {
    busy = false;
    await prepare(enrolled ? "assert" : "enroll", retainedStatus);
  }
}

element("enroll").addEventListener("click", () => ceremony("enroll"));
element("assert").addEventListener("click", () => ceremony("assert"));
element("reviewed").addEventListener("change", controls);
element("stop").addEventListener("click", async () => {
  busy = true; clearPrepared(); controls();
  try { await post("/api/stop"); } catch (_) { /* A stopped server has no enrollment. */ }
  csrf = null;
  element("status").textContent = "Test server stopped. Remove the localhost test passkey from your provider if desired.";
  busy = false; controls();
});

(async () => {
  if (!window.isSecureContext || !window.PublicKeyCredential || !navigator.credentials) {
    element("status").textContent = "This browser context does not support passkeys. Open the localhost address in a supported desktop browser.";
    return;
  }
  try {
    const response = await fetch("/api/session", { mode: "same-origin", cache: "no-store", credentials: "same-origin" });
    if (!response.ok) throw new Error("Session unavailable.");
    const session = await response.json();
    if (session.physical_authorization !== false) throw new Error("Unexpected session.");
    csrf = session.csrf;
    element("plan").textContent = session.plan;
    await prepare("enroll");
  } catch (_) { element("status").textContent = "The local test session is unavailable. Restart the development runner."; }
})();
