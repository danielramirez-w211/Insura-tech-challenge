"use strict";var $=Object.create;var b=Object.defineProperty;var P=Object.getOwnPropertyDescriptor;var I=Object.getOwnPropertyNames;var F=Object.getPrototypeOf,L=Object.prototype.hasOwnProperty;var H=(s,e)=>{for(var t in e)b(s,t,{get:e[t],enumerable:!0})},E=(s,e,t,r)=>{if(e&&typeof e=="object"||typeof e=="function")for(let o of I(e))!L.call(s,o)&&o!==t&&b(s,o,{get:()=>e[o],enumerable:!(r=P(e,o))||r.enumerable});return s};var u=(s,e,t)=>(t=s!=null?$(F(s)):{},E(e||!s||!s.__esModule?b(t,"default",{value:s,enumerable:!0}):t,s)),R=s=>E(b({},"__esModule",{value:!0}),s);var A={};H(A,{activate:()=>N,deactivate:()=>U});module.exports=R(A);var i=u(require("vscode")),M=u(require("path"));var v=u(require("fs")),x=u(require("path")),w=class{constructor(e){this.skillsDir=e}loadAll(){return v.existsSync(this.skillsDir)?v.readdirSync(this.skillsDir).filter(e=>e.endsWith(".md")).map(e=>this.loadSkill(x.join(this.skillsDir,e))).filter(e=>e!==null).sort((e,t)=>e.name.localeCompare(t.name)):[]}loadSkill(e){let t=v.readFileSync(e,"utf-8"),r=this.parseFrontmatter(t);if(!r)return null;let{meta:o,body:n}=r,c=x.basename(e,".md");return{id:c,name:o.name??c,description:o.description??"",input:o.input??"",output:o.output??"",specMapping:o["spec-mapping"]??"",instructions:n.trim(),filePath:e}}parseFrontmatter(e){let t=e.match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n([\s\S]*)$/);if(!t)return null;let r={};for(let o of t[1].split(`
`)){let n=o.indexOf(":");if(n===-1)continue;let c=o.slice(0,n).trim(),l=o.slice(n+1).trim();r[c]=l}return{meta:r,body:t[2]}}};var p=u(require("vscode")),z={"analyze-csharp":"search","migrate-models":"symbol-class","migrate-controllers":"symbol-interface","migrate-services":"symbol-method","migrate-repositories":"database","migrate-config":"settings-gear","migrate-tests":"beaker","validate-migration":"pass-filled"},S=class extends p.TreeItem{constructor(t){super(t.name,p.TreeItemCollapsibleState.None);this.skill=t;this.tooltip=`${t.description}

Entrada: ${t.input}
Salida: ${t.output}`,this.description=t.description,this.contextValue="skill",this.iconPath=new p.ThemeIcon(z[t.id]??"symbol-misc"),this.command={command:"migration.runSkill",title:"Ejecutar skill",arguments:[t.id]}}},k=class{constructor(e){this.loader=e}_onDidChangeTreeData=new p.EventEmitter;onDidChangeTreeData=this._onDidChangeTreeData.event;refresh(){this._onDidChangeTreeData.fire(void 0)}getTreeItem(e){return e}getChildren(){let e=this.loader.loadAll();if(e.length===0){let t=new p.TreeItem("No se encontraron skills");return t.description="Verifica la carpeta skills/",[]}return e.map(t=>new S(t))}};var d=u(require("vscode")),f=class s{static current;panel;disposables=[];static createOrShow(e,t){let r=d.window.activeTextEditor?d.window.activeTextEditor.viewColumn:void 0;if(s.current)return s.current.panel.reveal(r),s.current;let o=d.window.createWebviewPanel("migrationResult",`Migration: ${t}`,r||d.ViewColumn.Beside,{enableScripts:!0,retainContextWhenHidden:!0});return s.current=new s(o,e),s.current}constructor(e,t){this.panel=e,this.panel.webview.html=this.getLoadingHtml(),this.panel.onDidDispose(()=>this.dispose(),null,this.disposables),this.panel.webview.onDidReceiveMessage(async r=>{if(r.command==="copyCode"&&(await d.env.clipboard.writeText(r.text),d.window.showInformationMessage("C\xF3digo Java copiado al portapapeles.")),r.command==="saveFile"){let o=await d.window.showSaveDialog({defaultUri:d.Uri.file(r.filename),filters:{"Java Files":["java"]}});o&&(await d.workspace.fs.writeFile(o,Buffer.from(r.text,"utf-8")),d.window.showInformationMessage(`Guardado: ${o.fsPath}`))}},null,this.disposables)}showResult(e){this.panel.title=`Migration: ${e.skillName}`,this.panel.webview.html=this.getResultHtml(e)}showError(e){this.panel.webview.html=this.getErrorHtml(e)}dispose(){s.current=void 0,this.panel.dispose(),this.disposables.forEach(e=>e.dispose()),this.disposables=[]}getLoadingHtml(){return`<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <style>
    body { font-family: var(--vscode-font-family); color: var(--vscode-foreground);
           background: var(--vscode-editor-background); display: flex;
           align-items: center; justify-content: center; height: 100vh; margin: 0; }
    .spinner { text-align: center; }
    .spinner h2 { color: var(--vscode-textLink-foreground); }
  </style>
</head>
<body>
  <div class="spinner">
    <h2>\u2699\uFE0F InsuraTech Migration</h2>
    <p>Selecciona c\xF3digo C# en el editor y haz clic en una skill.</p>
  </div>
</body>
</html>`}getResultHtml(e){let t=this.escapeHtml(e.csharpCode),r=this.escapeHtml(e.javaCode),o=this.suggestFileName(e.javaCode);return`<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <meta http-equiv="Content-Security-Policy" content="default-src 'none'; script-src 'unsafe-inline' https://cdnjs.cloudflare.com; style-src 'unsafe-inline' https://cdnjs.cloudflare.com;">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github-dark.min.css">
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: var(--vscode-font-family); font-size: 13px;
           color: var(--vscode-foreground); background: var(--vscode-editor-background); padding: 16px; }
    h1 { font-size: 18px; margin-bottom: 4px; color: var(--vscode-textLink-foreground); }
    .meta { font-size: 12px; color: var(--vscode-descriptionForeground); margin-bottom: 16px; }
    .tabs { display: flex; gap: 0; border-bottom: 1px solid var(--vscode-panel-border); margin-bottom: 16px; }
    .tab { padding: 6px 16px; cursor: pointer; border: none; background: none;
           color: var(--vscode-descriptionForeground); font-size: 13px; border-bottom: 2px solid transparent; }
    .tab.active { color: var(--vscode-textLink-foreground); border-bottom-color: var(--vscode-textLink-foreground); font-weight: 600; }
    .tab-content { display: none; }
    .tab-content.active { display: block; }
    .toolbar { display: flex; gap: 8px; margin-bottom: 8px; }
    button { padding: 4px 12px; cursor: pointer; border-radius: 4px; font-size: 12px;
             background: var(--vscode-button-background); color: var(--vscode-button-foreground);
             border: none; }
    button:hover { background: var(--vscode-button-hoverBackground); }
    pre { border-radius: 6px; overflow: auto; max-height: 70vh; font-size: 12px; }
    code { font-family: var(--vscode-editor-font-family, monospace); }
    .spec-box { background: var(--vscode-textBlockQuote-background);
                border-left: 3px solid var(--vscode-textLink-foreground);
                padding: 12px; border-radius: 4px; white-space: pre-wrap; font-size: 12px; }
    .tokens { font-size: 11px; color: var(--vscode-descriptionForeground); margin-top: 8px; }
  </style>
</head>
<body>
  <h1>\u2615 ${this.escapeHtml(e.skillName)}</h1>
  <div class="meta">${this.escapeHtml(e.skillDescription)} \xB7 ${e.tokensUsed} tokens usados</div>

  <div class="tabs">
    <button class="tab active" onclick="showTab('java')">Java (resultado)</button>
    <button class="tab" onclick="showTab('csharp')">C# (origen)</button>
    <button class="tab" onclick="showTab('spec')">Trazabilidad</button>
  </div>

  <div id="tab-java" class="tab-content active">
    <div class="toolbar">
      <button onclick="copyCode()">\u{1F4CB} Copiar</button>
      <button onclick="saveFile()">\u{1F4BE} Guardar como .java</button>
    </div>
    <pre><code class="language-java" id="java-code">${r}</code></pre>
  </div>

  <div id="tab-csharp" class="tab-content">
    <pre><code class="language-csharp">${t}</code></pre>
  </div>

  <div id="tab-spec" class="tab-content">
    <div class="spec-box">${this.escapeHtml(e.specContext||"Sin contexto de spec disponible.")}</div>
  </div>

  <script>
    const vscode = acquireVsCodeApi();
    hljs.highlightAll();

    function showTab(tab) {
      document.querySelectorAll('.tab, .tab-content').forEach(el => el.classList.remove('active'));
      document.querySelector('[onclick="showTab(\\'' + tab + '\\')"]').classList.add('active');
      document.getElementById('tab-' + tab).classList.add('active');
    }

    function copyCode() {
      const code = document.getElementById('java-code').textContent;
      vscode.postMessage({ command: 'copyCode', text: code });
    }

    function saveFile() {
      const code = document.getElementById('java-code').textContent;
      vscode.postMessage({ command: 'saveFile', text: code, filename: '${o}' });
    }
  </script>
</body>
</html>`}getErrorHtml(e){return`<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <style>
    body { font-family: var(--vscode-font-family); color: var(--vscode-foreground);
           background: var(--vscode-editor-background); padding: 24px; }
    .error { color: var(--vscode-errorForeground); background: var(--vscode-inputValidation-errorBackground);
             border: 1px solid var(--vscode-inputValidation-errorBorder); padding: 16px; border-radius: 6px; }
    pre { white-space: pre-wrap; margin-top: 8px; font-size: 12px; }
  </style>
</head>
<body>
  <div class="error">
    <strong>\u274C Error en la migraci\xF3n</strong>
    <pre>${this.escapeHtml(e)}</pre>
  </div>
</body>
</html>`}escapeHtml(e){return e.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;").replace(/'/g,"&#39;")}suggestFileName(e){let t=e.match(/(?:class|interface|enum|record)\s+(\w+)/);return t?`${t[1]}.java`:"Migrated.java"}};var D=require("child_process"),y=class{async migrate(e){let{skillInstructions:t,csharpCode:r,specContext:o}=e,n=[t,"","## C\xF3digo C# a migrar","```csharp",r,"```","",o?`## Contexto del spec
${o}`:""].join(`
`);return{javaCode:await this.runClaude(n),tokensUsed:0}}runClaude(e){return new Promise((t,r)=>{let o=process.platform==="win32",n=(0,D.spawn)(o?"claude.cmd":"claude",["--print","--output-format","text"],{shell:o}),c="",l="";n.stdout.on("data",a=>{c+=a.toString()}),n.stderr.on("data",a=>{l+=a.toString()}),n.on("error",a=>{r(new Error(`No se pudo ejecutar Claude Code CLI: ${a.message}
Aseg\xFArate de que "claude" est\xE1 en el PATH del sistema.`))}),n.on("close",a=>{a!==0?r(new Error(`Claude CLI sali\xF3 con c\xF3digo ${a}:
${l}`)):t(c.trim())}),n.stdin.write(e),n.stdin.end(),setTimeout(()=>{n.kill(),r(new Error("Timeout: Claude CLI tard\xF3 m\xE1s de 2 minutos."))},12e4)})}};function N(s){let e=M.join(s.extensionPath,"skills"),t=new w(e),r=new k(t);i.window.registerTreeDataProvider("migrationSkills",r),s.subscriptions.push(i.commands.registerCommand("migration.refreshSkills",()=>{r.refresh()}),i.commands.registerCommand("migration.runSkill",async o=>{let c=i.workspace.getConfiguration("insuratech.migration").get("model")??"claude-sonnet-4-6",l=i.window.activeTextEditor;if(!l){i.window.showWarningMessage("Abre un archivo .cs y selecciona el c\xF3digo a migrar.");return}let a=l.selection,C=l.document.getText(a.isEmpty?void 0:a);if(!C.trim()){i.window.showWarningMessage("Selecciona el c\xF3digo C# que deseas migrar.");return}let m=t.loadAll().find(g=>g.id===o);if(!m){i.window.showErrorMessage(`Skill "${o}" no encontrada.`);return}let T=await B(s),j=f.createOrShow(s,m.name);await i.window.withProgress({location:i.ProgressLocation.Notification,title:`InsuraTech Migration: ejecutando "${m.name}"`,cancellable:!1},async()=>{try{let h=await new y().migrate({skillInstructions:m.instructions,csharpCode:C,specContext:T,model:c});j.showResult({skillName:m.name,skillDescription:m.description,csharpCode:C,javaCode:h.javaCode,tokensUsed:h.tokensUsed,specContext:T})}catch(g){let h=g instanceof Error?g.message:String(g);j.showError(h),i.window.showErrorMessage(`Migration error: ${h}`)}})}),i.commands.registerCommand("migration.showPanel",()=>{f.createOrShow(s,"InsuraTech Migration")}))}function U(){}async function B(s){let e=i.workspace.workspaceFolders;if(!e||e.length===0)return"";let r=i.workspace.getConfiguration("insuratech.migration").get("specsFolder")??"asd/.github/specs",o=i.Uri.joinPath(e[0].uri,r);try{let c=(await i.workspace.fs.readDirectory(o)).filter(([a])=>a.endsWith(".spec.md")).map(([a])=>a),l=[`Specs disponibles en el proyecto (${c.length} total):`,""];for(let a of c.slice(0,10))l.push(`- ${a.replace(".spec.md","")}`);return c.length>10&&l.push(`... y ${c.length-10} m\xE1s`),l.join(`
`)}catch{return""}}0&&(module.exports={activate,deactivate});
//# sourceMappingURL=extension.js.map
