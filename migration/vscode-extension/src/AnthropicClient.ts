import { spawn } from 'child_process';

export interface MigrateOptions {
  skillInstructions: string;
  csharpCode: string;
  specContext: string;
  model: string;
}

export interface MigrateResult {
  javaCode: string;
  tokensUsed: number;
}

export class AnthropicClient {
  async migrate(options: MigrateOptions): Promise<MigrateResult> {
    const { skillInstructions, csharpCode, specContext } = options;

    const prompt = [
      skillInstructions,
      '',
      '## Código C# a migrar',
      '```csharp',
      csharpCode,
      '```',
      '',
      specContext ? `## Contexto del spec\n${specContext}` : '',
    ].join('\n');

    const javaCode = await this.runClaude(prompt);
    return { javaCode, tokensUsed: 0 };
  }

  private runClaude(prompt: string): Promise<string> {
    return new Promise((resolve, reject) => {
      const isWin = process.platform === 'win32';
      const proc = spawn(
        isWin ? 'claude.cmd' : 'claude',
        ['--print', '--output-format', 'text'],
        { shell: isWin }
      );

      let stdout = '';
      let stderr = '';

      proc.stdout.on('data', (chunk: Buffer) => { stdout += chunk.toString(); });
      proc.stderr.on('data', (chunk: Buffer) => { stderr += chunk.toString(); });

      proc.on('error', (err) => {
        reject(new Error(
          `No se pudo ejecutar Claude Code CLI: ${err.message}\n` +
          'Asegúrate de que "claude" está en el PATH del sistema.'
        ));
      });

      proc.on('close', (code) => {
        if (code !== 0) {
          reject(new Error(`Claude CLI salió con código ${code}:\n${stderr}`));
        } else {
          resolve(stdout.trim());
        }
      });

      proc.stdin.write(prompt);
      proc.stdin.end();

      setTimeout(() => {
        proc.kill();
        reject(new Error('Timeout: Claude CLI tardó más de 2 minutos.'));
      }, 120_000);
    });
  }
}
