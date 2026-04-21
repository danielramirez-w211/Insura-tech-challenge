"use strict";
var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
  mod
));
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

// src/extension.ts
var extension_exports = {};
__export(extension_exports, {
  activate: () => activate,
  deactivate: () => deactivate
});
module.exports = __toCommonJS(extension_exports);
var vscode2 = __toESM(require("vscode"));

// src/SpecTreeProvider.ts
var vscode = __toESM(require("vscode"));
var fs = __toESM(require("fs"));
var path = __toESM(require("path"));
var SpecItem = class extends vscode.TreeItem {
  constructor(label, status, filePath) {
    super(label, vscode.TreeItemCollapsibleState.None);
    this.label = label;
    this.status = status;
    this.filePath = filePath;
    this.description = status;
    this.tooltip = filePath;
    this.iconPath = new vscode.ThemeIcon(getIconForStatus(status));
    this.command = {
      command: "vscode.open",
      title: "Abrir spec",
      arguments: [vscode.Uri.file(filePath)]
    };
  }
};
var SpecTreeProvider = class {
  constructor(workspaceRoot) {
    this.workspaceRoot = workspaceRoot;
  }
  _onDidChangeTreeData = new vscode.EventEmitter();
  onDidChangeTreeData = this._onDidChangeTreeData.event;
  filterStatus = void 0;
  // VSCode llama esto para obtener el elemento visual de cada nodo
  getTreeItem(element) {
    return element;
  }
  // VSCode llama esto para obtener los hijos de un nodo (null = raíz)
  getChildren() {
    const specs = this.loadSpecs();
    if (this.filterStatus) {
      return specs.filter((s) => s.status === this.filterStatus);
    }
    return specs;
  }
  refresh() {
    this._onDidChangeTreeData.fire();
  }
  setFilter(status) {
    this.filterStatus = status;
    this.refresh();
  }
  loadSpecs() {
    const cfg = vscode.workspace.getConfiguration("specTracker");
    const specsFolder = cfg.get("specsFolder") ?? "asd/.github/specs";
    const specsDir = path.join(this.workspaceRoot, specsFolder);
    if (!fs.existsSync(specsDir)) {
      return [];
    }
    return fs.readdirSync(specsDir).filter((f) => f.endsWith(".spec.md")).map((f) => {
      const filePath = path.join(specsDir, f);
      const status = parseStatus(fs.readFileSync(filePath, "utf-8"));
      const label = f.replace(".spec.md", "");
      return new SpecItem(label, status, filePath);
    }).sort((a, b) => statusOrder(a.status) - statusOrder(b.status));
  }
};
function parseStatus(content) {
  const match = content.match(/^status:\s*(.+)$/m);
  return match ? match[1].trim().toUpperCase() : "UNKNOWN";
}
function getIconForStatus(status) {
  switch (status) {
    case "IMPLEMENTED":
      return "pass-filled";
    case "IN_PROGRESS":
      return "sync~spin";
    case "APPROVED":
      return "circle-outline";
    case "DRAFT":
      return "edit";
    default:
      return "question";
  }
}
function statusOrder(status) {
  const order = {
    IN_PROGRESS: 0,
    APPROVED: 1,
    DRAFT: 2,
    IMPLEMENTED: 3,
    UNKNOWN: 4
  };
  return order[status] ?? 4;
}

// src/extension.ts
function activate(context) {
  const workspaceRoot = vscode2.workspace.workspaceFolders?.[0].uri.fsPath ?? "";
  const provider = new SpecTreeProvider(workspaceRoot);
  vscode2.window.registerTreeDataProvider("specList", provider);
  context.subscriptions.push(
    vscode2.commands.registerCommand("specTracker.refresh", () => {
      provider.refresh();
    }),
    vscode2.commands.registerCommand("specTracker.filterByStatus", async () => {
      const selected = await vscode2.window.showQuickPick(
        ["Todos", "DRAFT", "APPROVED", "IN_PROGRESS", "IMPLEMENTED"],
        { placeHolder: "Filtrar specs por estado" }
      );
      if (selected) {
        provider.setFilter(selected === "Todos" ? void 0 : selected);
      }
    })
  );
}
function deactivate() {
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  activate,
  deactivate
});
//# sourceMappingURL=extension.js.map
