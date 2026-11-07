// SPDX-License-Identifier: Apache-2.0
// Simulated DOM/WebAuthn regression. No browser, keychain or real credential.
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import vm from "node:vm";
const source = await readFile(new URL("./src/operator_browser/browser.js", import.meta.url), "utf8");
const html = await readFile(new URL("./src/operator_browser/browser.html", import.meta.url), "utf8");
for (const [enrollment, guarded, outcome = "success"] of [[false, false], [true, false], [false, true], [false, true, "lost"], [false, true, "cleanup"]]) {
  test(`${guarded ? "guarded " + outcome : enrollment ? "enrollment" : "assertion"} preserves Safari POST origin and fresh click`, async () => {
    assert.match(html, /name="referrer" content="same-origin"/);
    const elements = new Map(["status", "approve", "ack", "plan", "digest", "effect"].map(id => [id, {
      disabled:true, checked:false, textContent:"", listeners:new Map(),
      addEventListener(event, callback) { this.listeners.set(event, callback); },
    }]));
    let cleared = false, activation = false;
    const paths = [], prompts = [];
    const prompt = kind => {
      assert.equal(activation, true, "WebAuthn must begin synchronously in operator click");
      assert.match(elements.get("plan").textContent, enrollment ? /Enroll the first operator/ : /Synthetic exact plan/);
      if (guarded) assert.match(elements.get("effect").textContent, /WRITE.*1\.1\.231/);
      prompts.push(kind);
      return Promise.resolve({id:"synthetic",rawId:new Uint8Array([1]).buffer,type:"public-key",
        response:enrollment ? {clientDataJSON:new Uint8Array([2]).buffer,attestationObject:new Uint8Array([3]).buffer} : {clientDataJSON:new Uint8Array([2]).buffer,authenticatorData:new Uint8Array([3]).buffer,signature:new Uint8Array([4]).buffer,userHandle:null},getClientExtensionResults:()=>({})});
    };
    const context = vm.createContext({
      location:{hash:"#"+"a".repeat(64)}, history:{replaceState(_state,_title,path){assert.equal(path,"/");cleared=true;}},
      document:{getElementById:id=>elements.get(id)}, window:{isSecureContext:true},
      navigator:{credentials:{create:()=>prompt("create"),get:()=>prompt("get")}},
      Uint8Array, atob:value=>Buffer.from(value,"base64").toString("binary"), btoa:value=>Buffer.from(value,"binary").toString("base64"),
      async fetch(path, options) {
        assert.equal(cleared,true,"remove fragment before any request");
        assert.equal(options.referrerPolicy,"same-origin","no-referrer can produce Origin:null in Safari");
        assert.equal(options.mode,"same-origin");
        assert.equal(options.headers["X-OpenKNX-Session"],"a".repeat(64));
        assert.ok(!path.includes("a".repeat(64)));
        paths.push(path);
        if (path === "/finish" && guarded && outcome === "lost") throw new Error("Synthetic lost reply");
        const value = path === "/ceremony" ? {physical_authorization:false,execution_mode:guarded?"guarded_write":"observe_only",status:enrollment?"enrollment_ready":"assertion_ready",plan_summary:"Synthetic exact plan",plan_digest:Array(32).fill(1),options:{publicKey:{challenge:"AQID",...(enrollment?{user:{id:"AQID"}}:{allowCredentials:[{id:"AQID"}]})}}} : guarded ? {status:"guarded_execution_report",execution_mode:"guarded_write",succeeded:outcome!=="cleanup",execution:{operation:"restored_verified",cleanup:outcome==="cleanup"?"uncertain":"device_and_gateway_confirmed"}} : {physical_authorization:false,status:enrollment?"enrollment_activated":"assertion_verified"};
        return {ok:true,json:async()=>value};
      },
    });
    await vm.runInContext(source,context);
    assert.deepEqual(prompts,[]);
    assert.equal(elements.get("ack").disabled,false);
    elements.get("ack").checked=true;
    elements.get("ack").listeners.get("change")();
    assert.equal(elements.get("approve").disabled,false);
    activation=true;
    const clicked=elements.get("approve").listeners.get("click")();
    activation=false;
    await clicked;
    assert.deepEqual(paths,["/ceremony","/finish"]);
    assert.deepEqual(prompts,[enrollment?"create":"get"]);
    assert.match(elements.get("status").textContent, guarded
      ? outcome === "lost" ? /write may have occurred/ : outcome === "cleanup" ? /cleanup is not confirmed/ : /Original value restored.*cleanup confirmed/
      : /No device authorization was issued/);
  });
}
