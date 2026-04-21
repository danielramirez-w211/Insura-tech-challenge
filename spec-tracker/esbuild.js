const esbuild = require('esbuild');

esbuild.build({
  entryPoints: ['src/extension.ts'],
  bundle: true,
  outfile: 'dist/extension.js',
  external: ['vscode'],       // vscode lo provee el host, no se bundlea
  format: 'cjs',
  platform: 'node',
  sourcemap: true,
}).catch(() => process.exit(1));