import * as vscode from 'vscode';
import * as path from 'path';
import { SkillLoader } from './SkillLoader';
import { SkillTreeProvider } from './SkillTreeProvider';
import { MigrationPanel } from './MigrationPanel';
import { AnthropicClient } from './AnthropicClient';

export function activate(context: vscode.ExtensionContext): void {
  const skillsDir = path.join(context.extensionPath, 'skills');
  const loader    = new SkillLoader(skillsDir);
  const provider  = new SkillTreeProvider(loader);

  vscode.window.registerTreeDataProvider('migrationSkills', provider);

  context.subscriptions.push(
    vscode.commands.registerCommand('migration.refreshSkills', () => {
      provider.refresh();
    }),

    vscode.commands.registerCommand('migration.runSkill', async (skillId: string) => {
      const cfg     = vscode.workspace.getConfiguration('insuratech.migration');
      const apiKey  = cfg.get<string>('anthropicApiKey') ?? '';
      const model   = cfg.get<string>('model') ?? 'claude-sonnet-4-6';

      if (!apiKey) {
        const action = await vscode.window.showErrorMessage(
          'API Key de Anthropic no configurada.',
          'Abrir Configuración'
        );
        if (action === 'Abrir Configuración') {
          vscode.commands.executeCommand(
            'workbench.action.openSettings',
            'insuratech.migration.anthropicApiKey'
          );
        }
        return;
      }

      const editor = vscode.window.activeTextEditor;
      if (!editor) {
        vscode.window.showWarningMessage('Abre un archivo .cs y selecciona el código a migrar.');
        return;
      }

      const selection = editor.selection;
      const csharpCode = editor.document.getText(
        selection.isEmpty ? undefined : selection
      );

      if (!csharpCode.trim()) {
        vscode.window.showWarningMessage('Selecciona el código C# que deseas migrar.');
        return;
      }

      const skills = loader.loadAll();
      const skill  = skills.find(s => s.id === skillId);
      if (!skill) {
        vscode.window.showErrorMessage(`Skill "${skillId}" no encontrada.`);
        return;
      }

      const specContext = await loadSpecContext(context);

      const panel = MigrationPanel.createOrShow(context, skill.name);

      await vscode.window.withProgress(
        {
          location: vscode.ProgressLocation.Notification,
          title: `InsuraTech Migration: ejecutando "${skill.name}"`,
          cancellable: false,
        },
        async () => {
          try {
            const client = new AnthropicClient(apiKey);
            const result = await client.migrate({
              skillInstructions: skill.instructions,
              csharpCode,
              specContext,
              model,
            });
            panel.showResult({
              skillName: skill.name,
              skillDescription: skill.description,
              csharpCode,
              javaCode: result.javaCode,
              tokensUsed: result.tokensUsed,
              specContext,
            });
          } catch (err) {
            const msg = err instanceof Error ? err.message : String(err);
            panel.showError(msg);
            vscode.window.showErrorMessage(`Migration error: ${msg}`);
          }
        }
      );
    }),

    vscode.commands.registerCommand('migration.showPanel', () => {
      MigrationPanel.createOrShow(context, 'InsuraTech Migration');
    })
  );
}

export function deactivate(): void {}

async function loadSpecContext(context: vscode.ExtensionContext): Promise<string> {
  const workspaceFolders = vscode.workspace.workspaceFolders;
  if (!workspaceFolders || workspaceFolders.length === 0) { return ''; }

  const cfg       = vscode.workspace.getConfiguration('insuratech.migration');
  const specsPath = cfg.get<string>('specsFolder') ?? 'asd/.github/specs';
  const specsDir  = vscode.Uri.joinPath(workspaceFolders[0].uri, specsPath);

  try {
    const files = await vscode.workspace.fs.readDirectory(specsDir);
    const specFiles = files
      .filter(([name]) => name.endsWith('.spec.md'))
      .map(([name]) => name);

    const lines = [`Specs disponibles en el proyecto (${specFiles.length} total):`, ''];
    for (const file of specFiles.slice(0, 10)) {
      lines.push(`- ${file.replace('.spec.md', '')}`);
    }
    if (specFiles.length > 10) {
      lines.push(`... y ${specFiles.length - 10} más`);
    }
    return lines.join('\n');
  } catch {
    return '';
  }
}
