import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';

// Representa un spec individual en el árbol
export class SpecItem extends vscode.TreeItem {

  constructor(
    public readonly label: string,
    public readonly status: string,
    public readonly filePath: string,
  ) {
    super(label, vscode.TreeItemCollapsibleState.None);

    this.description = status;
    this.tooltip = filePath;
    this.iconPath = new vscode.ThemeIcon(getIconForStatus(status));

    // Al hacer click abre el archivo
    this.command = {
      command: 'vscode.open',
      title: 'Abrir spec',
      arguments: [vscode.Uri.file(filePath)],
    };
  }
}

// El provider que VSCode consulta para construir el árbol
export class SpecTreeProvider implements vscode.TreeDataProvider<SpecItem> {

  private _onDidChangeTreeData = new vscode.EventEmitter<void>();
  readonly onDidChangeTreeData = this._onDidChangeTreeData.event;

  private filterStatus: string | undefined = undefined;

  constructor(private readonly workspaceRoot: string) {}

  // VSCode llama esto para obtener el elemento visual de cada nodo
  getTreeItem(element: SpecItem): vscode.TreeItem {
    return element;
  }

  // VSCode llama esto para obtener los hijos de un nodo (null = raíz)
  getChildren(): SpecItem[] {
    const specs = this.loadSpecs();
    if (this.filterStatus) {
      return specs.filter(s => s.status === this.filterStatus);
    }
    return specs;
  }

  refresh(): void {
    this._onDidChangeTreeData.fire();
  }

  setFilter(status: string | undefined): void {
    this.filterStatus = status;
    this.refresh();
  }

  private loadSpecs(): SpecItem[] {
    const cfg = vscode.workspace.getConfiguration('specTracker');
    const specsFolder = cfg.get<string>('specsFolder') ?? 'asd/.github/specs';
    const specsDir = path.join(this.workspaceRoot, specsFolder);

    if (!fs.existsSync(specsDir)) { return []; }

    return fs.readdirSync(specsDir)
      .filter(f => f.endsWith('.spec.md'))
      .map(f => {
        const filePath = path.join(specsDir, f);
        const status   = parseStatus(fs.readFileSync(filePath, 'utf-8'));
        const label    = f.replace('.spec.md', '');
        return new SpecItem(label, status, filePath);
      })
      .sort((a, b) => statusOrder(a.status) - statusOrder(b.status));
  }
}

function parseStatus(content: string): string {
  const match = content.match(/^status:\s*(.+)$/m);
  return match ? match[1].trim().toUpperCase() : 'UNKNOWN';
}

function getIconForStatus(status: string): string {
  switch (status) {
    case 'IMPLEMENTED':  return 'pass-filled';
    case 'IN_PROGRESS':  return 'sync~spin';
    case 'APPROVED':     return 'circle-outline';
    case 'DRAFT':        return 'edit';
    default:             return 'question';
  }
}

function statusOrder(status: string): number {
  const order: Record<string, number> = {
    IN_PROGRESS: 0, APPROVED: 1, DRAFT: 2, IMPLEMENTED: 3, UNKNOWN: 4,
  };
  return order[status] ?? 4;
}
