import * as fs from 'fs';
import * as path from 'path';

export interface Skill {
  id: string;
  name: string;
  description: string;
  input: string;
  output: string;
  specMapping: string;
  instructions: string;
  filePath: string;
}

export class SkillLoader {
  constructor(private readonly skillsDir: string) {}

  loadAll(): Skill[] {
    if (!fs.existsSync(this.skillsDir)) {
      return [];
    }
    return fs
      .readdirSync(this.skillsDir)
      .filter(f => f.endsWith('.md'))
      .map(f => this.loadSkill(path.join(this.skillsDir, f)))
      .filter((s): s is Skill => s !== null)
      .sort((a, b) => a.name.localeCompare(b.name));
  }

  private loadSkill(filePath: string): Skill | null {
    const raw = fs.readFileSync(filePath, 'utf-8');
    const frontmatter = this.parseFrontmatter(raw);
    if (!frontmatter) { return null; }

    const { meta, body } = frontmatter;
    const id = path.basename(filePath, '.md');

    return {
      id,
      name: meta['name'] ?? id,
      description: meta['description'] ?? '',
      input: meta['input'] ?? '',
      output: meta['output'] ?? '',
      specMapping: meta['spec-mapping'] ?? '',
      instructions: body.trim(),
      filePath,
    };
  }

  private parseFrontmatter(raw: string): { meta: Record<string, string>; body: string } | null {
    const match = raw.match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n([\s\S]*)$/);
    if (!match) { return null; }

    const meta: Record<string, string> = {};
    for (const line of match[1].split('\n')) {
      const colon = line.indexOf(':');
      if (colon === -1) { continue; }
      const key = line.slice(0, colon).trim();
      const value = line.slice(colon + 1).trim();
      meta[key] = value;
    }

    return { meta, body: match[2] };
  }
}
