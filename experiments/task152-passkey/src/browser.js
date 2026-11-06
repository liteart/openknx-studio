// SPDX-License-Identifier: Apache-2.0
"use strict";

const element = (id) => document.getElementById(id);
let csrf = null;
let enrolled = false;
let busy = false;

function controls() {
  element("enroll").disabled = !csrf || busy || enrolled;
  element("reviewed").disabled = !csrf || busy || !enrolled;
  element("assert").disabled = !csrf || busy || !enrolled || !element("reviewed").checked;
  element("stop").disabled = !csrf || busy;
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
  if (!response.ok) throw new Error("Local test request was rejected.");
  return response.json();
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
  busy = true; controls();
  element("status").textContent = "Confirm using the passkey provider chosen in your browser.";
  try {
    const options = await post(`/api/${kind}/start`);
    const publicKey = options.publicKey;
    publicKey.challenge = decode(publicKey.challenge);
    for (const field of ["excludeCredentials", "allowCredentials"]) {
      if (publicKey[field]) for (const credential of publicKey[field]) credential.id = decode(credential.id);
    }
    if (kind === "enroll") publicKey.user.id = decode(publicKey.user.id);
    const credential = kind === "enroll"
      ? await navigator.credentials.create({ publicKey })
      : await navigator.credentials.get({ publicKey });
    if (!credential) throw new Error("Passkey ceremony did not complete.");
    const result = await post(`/api/${kind}/finish`, serialize(credential));
    if (result.physical_authorization !== false) throw new Error("Unexpected response.");
    if (kind === "enroll") {
      enrolled = true;
      element("status").textContent = "Temporary test passkey enrolled. Review the plan below to test an assertion.";
    } else {
      element("status").textContent = "Passkey assertion verified for this synthetic plan. No device authorization was issued.";
      element("reviewed").checked = false;
    }
  } catch (_) {
    // Neither authenticator data nor browser error details are logged or shown.
    try { await post("/api/cancel"); } catch (_) { /* The temporary server may have stopped. */ }
    element("status").textContent = "The passkey test was cancelled or rejected. You can retry while the server is running.";
  } finally { busy = false; controls(); }
}

element("enroll").addEventListener("click", () => ceremony("enroll"));
element("assert").addEventListener("click", () => ceremony("assert"));
element("reviewed").addEventListener("change", controls);
element("stop").addEventListener("click", async () => {
  busy = true; controls();
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
    element("status").textContent = "Ready. Choose Create temporary test passkey when you want to start.";
    controls();
  } catch (_) { element("status").textContent = "The local test session is unavailable. Restart the development runner."; }
})();
