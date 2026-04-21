import * as vscode from 'vscode';
import { SpecTreeProvider } from './SpecTreeProvider';

export function activate(context: vscode.ExtensionContext): void {
  const workspaceRoot = vscode.workspace.workspaceFolders?.[0].uri.fsPath ?? '';
  const provider = new SpecTreeProvider(workspaceRoot);

  vscode.window.registerTreeDataProvider('specList', provider);

  context.subscriptions.push(
    vscode.commands.registerCommand('specTracker.refresh', () => {
      provider.refresh();
    }),

    vscode.commands.registerCommand('specTracker.filterByStatus', async () => {
      const selected = await vscode.window.showQuickPick(
        ['Todos', 'DRAFT', 'APPROVED', 'IN_PROGRESS', 'IMPLEMENTED'],
        { placeHolder: 'Filtrar specs por estado' }
      );
      if (selected) {
        provider.setFilter(selected === 'Todos' ? undefined : selected);
      }
    })
  );
}

export function deactivate(): void {}
