import * as vscode from 'vscode';

export interface MigrationResult {
  skillName: string;
  skillDescription: string;
  csharpCode: string;
  javaCode: string;
  tokensUsed: number;
  specContext: string;
}

export class MigrationPanel {
  private static current: MigrationPanel | undefined;
  private readonly panel: vscode.WebviewPanel;
  private disposables: vscode.Disposable[] = [];

  static createOrShow(context: vscode.ExtensionContext, title: string): MigrationPanel {
    const column = vscode.window.activeTextEditor
      ? vscode.window.activeTextEditor.viewColumn
      : undefined;

    if (MigrationPanel.current) {
      MigrationPanel.current.panel.reveal(column);
      return MigrationPanel.current;
    }

    const panel = vscode.window.createWebviewPanel(
      'migrationResult',
      `Migration: ${title}`,
      column || vscode.ViewColumn.Beside,
      {
        enableScripts: true,
        retainContextWhenHidden: true,
      }
    );

    MigrationPanel.current = new MigrationPanel(panel, context);
    return MigrationPanel.current;
  }

  private constructor(panel: vscode.WebviewPanel, _context: vscode.ExtensionContext) {
    this.panel = panel;
    this.panel.webview.html = this.getLoadingHtml();

    this.panel.onDidDispose(() => this.dispose(), null, this.disposables);

    this.panel.webview.onDidReceiveMessage(
      async (message: { command: string; text: string; filename: string }) => {
        if (message.command === 'copyCode') {
          await vscode.env.clipboard.writeText(message.text);
          vscode.window.showInformationMessage('Código Java copiado al portapapeles.');
        }
        if (message.command === 'saveFile') {
          const uri = await vscode.window.showSaveDialog({
            defaultUri: vscode.Uri.file(message.filename),
            filters: { 'Java Files': ['java'] },
          });
          if (uri) {
            await vscode.workspace.fs.writeFile(uri, Buffer.from(message.text, 'utf-8'));
            vscode.window.showInformationMessage(`Guardado: ${uri.fsPath}`);
          }
        }
      },
      null,
      this.disposables
    );
  }

  showResult(result: MigrationResult): void {
    this.panel.title = `Migration: ${result.skillName}`;
    this.panel.webview.html = this.getResultHtml(result);
  }

  showError(message: string): void {
    this.panel.webview.html = this.getErrorHtml(message);
  }

  dispose(): void {
    MigrationPanel.current = undefined;
    this.panel.dispose();
    this.disposables.forEach(d => d.dispose());
    this.disposables = [];
  }

  private getLoadingHtml(): string {
    return `<!DOCTYPE html>
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
    <h2>⚙️ InsuraTech Migration</h2>
    <p>Selecciona código C# en el editor y haz clic en una skill.</p>
  </div>
</body>
</html>`;
  }

  private getResultHtml(result: MigrationResult): string {
    const escapedCsharp = this.escapeHtml(result.csharpCode);
    const escapedJava   = this.escapeHtml(result.javaCode);
    const suggestedName = this.suggestFileName(result.javaCode);

    return `<!DOCTYPE html>
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
  <h1>☕ ${this.escapeHtml(result.skillName)}</h1>
  <div class="meta">${this.escapeHtml(result.skillDescription)} · ${result.tokensUsed} tokens usados</div>

  <div class="tabs">
    <button class="tab active" onclick="showTab('java')">Java (resultado)</button>
    <button class="tab" onclick="showTab('csharp')">C# (origen)</button>
    <button class="tab" onclick="showTab('spec')">Trazabilidad</button>
  </div>

  <div id="tab-java" class="tab-content active">
    <div class="toolbar">
      <button onclick="copyCode()">📋 Copiar</button>
      <button onclick="saveFile()">💾 Guardar como .java</button>
    </div>
    <pre><code class="language-java" id="java-code">${escapedJava}</code></pre>
  </div>

  <div id="tab-csharp" class="tab-content">
    <pre><code class="language-csharp">${escapedCsharp}</code></pre>
  </div>

  <div id="tab-spec" class="tab-content">
    <div class="spec-box">${this.escapeHtml(result.specContext || 'Sin contexto de spec disponible.')}</div>
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
      vscode.postMessage({ command: 'saveFile', text: code, filename: '${suggestedName}' });
    }
  </script>
</body>
</html>`;
  }

  private getErrorHtml(message: string): string {
    return `<!DOCTYPE html>
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
    <strong>❌ Error en la migración</strong>
    <pre>${this.escapeHtml(message)}</pre>
  </div>
</body>
</html>`;
  }

  private escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  private suggestFileName(javaCode: string): string {
    const match = javaCode.match(/(?:class|interface|enum|record)\s+(\w+)/);
    return match ? `${match[1]}.java` : 'Migrated.java';
  }
}
