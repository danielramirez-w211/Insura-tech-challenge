import * as vscode from 'vscode';
import { Skill, SkillLoader } from './SkillLoader';

const ICON_MAP: Record<string, string> = {
  'analyze-csharp':        'search',
  'migrate-models':        'symbol-class',
  'migrate-controllers':   'symbol-interface',
  'migrate-services':      'symbol-method',
  'migrate-repositories':  'database',
  'migrate-config':        'settings-gear',
  'migrate-tests':         'beaker',
  'validate-migration':    'pass-filled',
};

export class SkillTreeItem extends vscode.TreeItem {
  constructor(readonly skill: Skill) {
    super(skill.name, vscode.TreeItemCollapsibleState.None);
    this.tooltip     = `${skill.description}\n\nEntrada: ${skill.input}\nSalida: ${skill.output}`;
    this.description = skill.description;
    this.contextValue = 'skill';
    this.iconPath     = new vscode.ThemeIcon(ICON_MAP[skill.id] ?? 'symbol-misc');
    this.command      = {
      command: 'migration.runSkill',
      title: 'Ejecutar skill',
      arguments: [skill.id],
    };
  }
}

export class SkillTreeProvider implements vscode.TreeDataProvider<SkillTreeItem> {
  private readonly _onDidChangeTreeData = new vscode.EventEmitter<SkillTreeItem | undefined>();
  readonly onDidChangeTreeData = this._onDidChangeTreeData.event;

  constructor(private readonly loader: SkillLoader) {}

  refresh(): void {
    this._onDidChangeTreeData.fire(undefined);
  }

  getTreeItem(element: SkillTreeItem): vscode.TreeItem {
    return element;
  }

  getChildren(): SkillTreeItem[] {
    const skills = this.loader.loadAll();
    if (skills.length === 0) {
      const empty = new vscode.TreeItem('No se encontraron skills');
      empty.description = 'Verifica la carpeta skills/';
      return [];
    }
    return skills.map(s => new SkillTreeItem(s));
  }
}
