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
  private static readonly API_URL = 'https://api.anthropic.com/v1/messages';
  private static readonly API_VERSION = '2023-06-01';

  constructor(private readonly apiKey: string) {
    if (!apiKey || apiKey.trim() === '') {
      throw new Error(
        'API Key de Anthropic no configurada. ' +
        'Ve a Configuración → insuratech.migration.anthropicApiKey'
      );
    }
  }

  async migrate(options: MigrateOptions): Promise<MigrateResult> {
    const { skillInstructions, csharpCode, specContext, model } = options;

    const userMessage = [
      '## Código C# a migrar',
      '```csharp',
      csharpCode,
      '```',
      '',
      specContext ? `## Contexto del spec\n${specContext}` : '',
    ].filter(Boolean).join('\n');

    const response = await fetch(AnthropicClient.API_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-api-key': this.apiKey,
        'anthropic-version': AnthropicClient.API_VERSION,
      },
      body: JSON.stringify({
        model,
        max_tokens: 4096,
        system: skillInstructions,
        messages: [{ role: 'user', content: userMessage }],
      }),
    });

    if (!response.ok) {
      const err = await response.text();
      throw new Error(`Error de la API Anthropic (${response.status}): ${err}`);
    }

    const data = await response.json() as AnthropicResponse;
    return {
      javaCode: data.content[0]?.text ?? '',
      tokensUsed: (data.usage?.input_tokens ?? 0) + (data.usage?.output_tokens ?? 0),
    };
  }
}

interface AnthropicResponse {
  content: Array<{ type: string; text: string }>;
  usage?: { input_tokens: number; output_tokens: number };
}
