// SPDX-License-Identifier: Apache-2.0
"use strict";
(async () => {
  const secret = location.hash.slice(1);
  history.replaceState(null, "", "/");
  const status = document.getElementById("status");
  const button = document.getElementById("approve");
  const ack = document.getElementById("ack");
  const post = async (path, body) => {
    const response = await fetch(path, {method:"POST", mode:"same-origin", cache:"no-store", credentials:"omit", referrerPolicy:"same-origin", headers:{"Content-Type":"application/json", "X-OpenKNX-Session":secret}, body:JSON.stringify(body)});
    if (!response.ok) throw new Error("Operator session rejected or expired.");
    return response.json();
  };
  const decode = value => Uint8Array.from(atob(value.replace(/-/g,"+").replace(/_/g,"/")), c => c.charCodeAt(0));
  const encode = value => btoa(String.fromCharCode(...new Uint8Array(value))).replace(/\+/g,"-").replace(/\//g,"_").replace(/=+$/g,"");
  try {
    if (!/^[0-9a-f]{64}$/.test(secret) || !window.isSecureContext || !navigator.credentials) throw new Error("Private operator bootstrap or WebAuthn support unavailable.");
    const ready = await post("/ceremony", {});
    const enroll = ready.status === "enrollment_ready";
    if (ready.physical_authorization !== false || (!enroll && ready.status !== "assertion_ready")) throw new Error("Unexpected operator response.");
    document.getElementById("plan").textContent = enroll ? "Enroll the first operator passkey in the protected store. No device authorization is issued." : ready.plan_summary;
    document.getElementById("digest").textContent = enroll ? "" : "Plan SHA256: " + ready.plan_digest.map(byte => byte.toString(16).padStart(2,"0")).join("");
    const publicKey = ready.options.publicKey;
    publicKey.challenge = decode(publicKey.challenge);
    if (publicKey.user) publicKey.user.id = decode(publicKey.user.id);
    for (const field of ["allowCredentials", "excludeCredentials"]) if (publicKey[field]) for (const credential of publicKey[field]) credential.id = decode(credential.id);
    ack.disabled = false;
    ack.addEventListener("change", () => { button.disabled = !ack.checked; });
    status.textContent = "Review the details, then explicitly confirm with your passkey.";
    button.addEventListener("click", async () => {
      button.disabled = true; ack.disabled = true;
      try {
        const credential = enroll ? await navigator.credentials.create({publicKey}) : await navigator.credentials.get({publicKey});
        if (!credential) throw new Error("Passkey ceremony did not complete.");
        const response = {clientDataJSON:encode(credential.response.clientDataJSON)};
        for (const field of ["attestationObject", "authenticatorData", "signature"]) if (credential.response[field]) response[field] = encode(credential.response[field]);
        if ("userHandle" in credential.response) response.userHandle = credential.response.userHandle ? encode(credential.response.userHandle) : null;
        const serialized = {id:credential.id, rawId:encode(credential.rawId), type:credential.type, response, extensions:credential.getClientExtensionResults()};
        const result = await post("/finish", {credential:serialized});
        if (result.physical_authorization !== false) throw new Error("Unexpected authorization response.");
        status.textContent = result.status + ". No device authorization was issued.";
      } catch (_) { status.textContent = "Ceremony failed or session expired. No device authorization was issued. Enrollment outcome may require protected operator inspection."; }
    }, {once:true});
  } catch (_) { status.textContent = "Private operator session unavailable or expired. No device authorization was issued."; }
})();
