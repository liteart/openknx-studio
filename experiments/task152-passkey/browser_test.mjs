// SPDX-License-Identifier: Apache-2.0
// Simulated DOM/credential API regression tests; never invoke a real provider.
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import vm from "node:vm";

const source = await readFile(new URL("./src/browser.js", import.meta.url), "utf8");

function credential(kind) {
  return {
    id: "synthetic-id", rawId: new Uint8Array([1, 2, 3]).buffer, type: "public-key",
    response: kind === "enroll"
      ? { clientDataJSON: new Uint8Array([1]).buffer, attestationObject: new Uint8Array([2]).buffer }
      : { clientDataJSON: new Uint8Array([1]).buffer, authenticatorData: new Uint8Array([2]).buffer,
        signature: new Uint8Array([3]).buffer, userHandle: null },
    getClientExtensionResults: () => ({}),
  };
}

function harness({ createError = null, getError = null, rejectPath = null, rejectionChecks = null, malformedOptions = false } = {}) {
  const elements = new Map();
  for (const id of ["enroll", "assert", "reviewed", "stop", "status", "plan"]) {
    elements.set(id, { disabled: true, checked: false, textContent: "", listeners: new Map(),
      addEventListener(name, fn) { this.listeners.set(name, fn); } });
  }
  const calls = [];
  const prompts = [];
  const timers = new Map();
  let nextTimer = 1;
  let currentTime = 0;
  const activation = { isActive: false };
  const context = vm.createContext({
    document: { getElementById: (id) => elements.get(id) },
    window: { isSecureContext: true, PublicKeyCredential: function () {} },
    navigator: { userActivation: activation, credentials: {
      create(options) {
        prompts.push("create");
        assert.equal(activation.isActive, true, "create must run synchronously during the click");
        assert.ok(options.publicKey.challenge instanceof Uint8Array);
        assert.ok(options.publicKey.user.id instanceof Uint8Array);
        return createError ? Promise.reject(createError) : Promise.resolve(credential("enroll"));
      },
      get(options) {
        prompts.push("get");
        assert.equal(activation.isActive, true, "get must run synchronously during the click");
        assert.ok(options.publicKey.allowCredentials[0].id instanceof Uint8Array);
        return getError ? Promise.reject(getError) : Promise.resolve(credential("assert"));
      },
    } },
    performance: { now: () => currentTime },
    Uint8Array,
    atob: (value) => Buffer.from(value, "base64").toString("binary"),
    btoa: (value) => Buffer.from(value, "binary").toString("base64"),
    setTimeout(fn, milliseconds) { const id = nextTimer++; timers.set(id, { fn, milliseconds }); return id; },
    clearTimeout(id) { timers.delete(id); },
    async fetch(path, options = {}) {
      calls.push(path);
      if (options.method === "POST") {
        assert.equal(options.mode, "same-origin");
        assert.equal(options.referrerPolicy, "same-origin", "POST must preserve an exact Origin instead of null");
      }
      if (path === rejectPath) return { ok: false, status: 403, json: async () => ({ checks: rejectionChecks }) };
      const response = path === "/api/session"
        ? { csrf: "synthetic-token", plan: "synthetic exact plan", physical_authorization: false }
        : path.endsWith("/start")
          ? { publicKey: malformedOptions ? {} : { challenge: "AQIDBA", user: { id: "AQID" },
            allowCredentials: [{ id: "AQID" }], authenticatorSelection: { userVerification: "required" } } }
          : path === "/api/enroll/finish"
            ? { enrolled: true, physical_authorization: false }
            : path === "/api/assert/finish"
              ? { verified: true, physical_authorization: false }
              : {};
      if (options.body) JSON.parse(options.body);
      return { ok: true, status: 200, json: async () => response };
    },
  });
  vm.runInContext(source, context, { filename: "browser.js" });
  return {
    elements, calls, prompts, timers,
    setTime(value) { currentTime = value; },
    async settle() { for (let i = 0; i < 8; i++) await new Promise((resolve) => setImmediate(resolve)); },
    async click(id) {
      assert.equal(elements.get(id).disabled, false, `${id} must be enabled`);
      activation.isActive = true;
      const result = elements.get(id).listeners.get("click")();
      activation.isActive = false;
      await result;
      await this.settle();
    },
    status() { return elements.get("status").textContent; },
  };
}

test("prepared create and get execute directly during the user gesture, never on load", async () => {
  const page = harness(); await page.settle();
  assert.deepEqual(page.prompts, []);
  assert.ok(page.calls.includes("/api/enroll/start"));
  assert.equal(page.elements.get("enroll").textContent, "Create temporary test passkey");
  assert.equal(page.elements.get("reviewed").disabled, true);
  await page.click("enroll");
  assert.deepEqual(page.prompts, ["create"]);
  assert.ok(page.calls.includes("/api/assert/start"));
  assert.equal(page.elements.get("reviewed").disabled, false);
  page.elements.get("reviewed").checked = true;
  page.elements.get("reviewed").listeners.get("change")();
  await page.click("assert");
  assert.deepEqual(page.prompts, ["create", "get"]);
  assert.match(page.status(), /assertion verified/);
  assert.match(page.status(), /No device authorization/);
});

test("prompt failures expose only stage, allowlisted exception name and activation", async () => {
  const error = new Error("private browser detail or credential value must not escape");
  error.name = "NotAllowedError";
  const page = harness({ createError: error }); await page.settle();
  await page.click("enroll");
  assert.match(page.status(), /enrollment passkey prompt \(NotAllowedError; user gesture active\)/);
  assert.doesNotMatch(page.status(), /private browser|credential value/);
  assert.equal(page.elements.get("reviewed").disabled, true);
  assert.equal(page.elements.get("enroll").disabled, false);
  assert.deepEqual(page.prompts, ["create"]);
});

test("unrecognized error names and messages are not displayed", async () => {
  const error = new Error("secret-message"); error.name = "<untrusted-secret-name>";
  const page = harness({ createError: error }); await page.settle();
  await page.click("enroll");
  assert.match(page.status(), /unavailable error name/);
  assert.doesNotMatch(page.status(), /secret|<untrusted/);
});

test("verification HTTP rejection identifies the failing stage and retains disabled review", async () => {
  const page = harness({ rejectPath: "/api/enroll/finish" }); await page.settle();
  await page.click("enroll");
  assert.match(page.status(), /enrollment verification \(HTTP 403\)/);
  assert.equal(page.elements.get("reviewed").disabled, true);
});

test("request diagnostics expose only allowlisted booleans and never header values", async () => {
  const page = harness({ rejectPath: "/api/cancel", rejectionChecks: {
    origin_present: true, origin_matches: false, origin_is_null: true, csrf_matches: true,
    csrf_present: "secret-token", authorization: "secret-authorization", error: "secret-message",
  } });
  await page.settle();
  assert.match(page.status(), /origin present true; origin matches false; origin null true; CSRF matches true/);
  assert.doesNotMatch(page.status(), /secret|CSRF present/);
  assert.deepEqual(page.prompts, []);
});

test("challenge preparation HTTP and decoding failures never invoke a provider", async () => {
  const rejected = harness({ rejectPath: "/api/enroll/start" }); await rejected.settle();
  assert.match(rejected.status(), /enrollment challenge preparation \(HTTP 403\)/);
  assert.deepEqual(rejected.prompts, []);
  assert.equal(rejected.elements.get("enroll").textContent, "Prepare temporary test passkey");
  const malformed = harness({ malformedOptions: true }); await malformed.settle();
  assert.match(malformed.status(), /enrollment challenge preparation \(TypeError\)/);
  assert.deepEqual(malformed.prompts, []);
});

test("prepared challenges refresh before server expiry without opening a provider", async () => {
  const page = harness(); await page.settle();
  assert.equal(page.calls.filter((path) => path === "/api/enroll/start").length, 1);
  const timer = [...page.timers.values()][0];
  assert.equal(timer.milliseconds, 100000);
  page.setTime(100000); timer.fn(); await page.settle();
  assert.equal(page.calls.filter((path) => path === "/api/enroll/start").length, 2);
  assert.deepEqual(page.prompts, []);
});

test("a stale prepared click refreshes options and requires a fresh user click", async () => {
  const page = harness(); await page.settle();
  page.setTime(100001);
  await page.click("enroll");
  assert.deepEqual(page.prompts, []);
  assert.equal(page.elements.get("enroll").textContent, "Create temporary test passkey");
  await page.click("enroll");
  assert.deepEqual(page.prompts, ["create"]);
});

test("stopping clears the refresh timer and cannot trigger a passkey prompt", async () => {
  const page = harness(); await page.settle();
  await page.click("stop");
  assert.equal(page.timers.size, 0);
  assert.equal(page.elements.get("enroll").disabled, true);
  assert.deepEqual(page.prompts, []);
});
