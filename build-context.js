const fs = require('fs');
const path = require('path');

const filesToInclude = [
  'src/CafeBarrio.API/Controllers/ProductosController.cs',
  'src/CafeBarrio.API/Controllers/CategoriasController.cs',
  'src/CafeBarrio.API/Controllers/MetodosPagoController.cs',
  'src/CafeBarrio.API/Controllers/TransaccionesController.cs',
  'src/CafeBarrio.API/Controllers/OperadoresController.cs',
  'pos-pwa/src/api.ts',
  'pos-pwa/src/types.ts',
  'pos-pwa/package.json',
  'pos-pwa/src/components/LoginScreen.tsx',
  'pos-pwa/src/components/SalesModule.tsx'
];

let output = '';

for (const file of filesToInclude) {
  const fullPath = path.join('c:/Users/PC/Desktop/Database/CafeDeBarrio-POS', file);
  try {
    const content = fs.readFileSync(fullPath, 'utf8');
    output += `\n\n--- ARCHIVO: ${file} ---\n\n`;
    output += content;
  } catch (err) {
    output += `\n\n--- ARCHIVO: ${file} ---\n\n(No se pudo leer: ${err.message})\n`;
  }
}

const outDir = 'c:/Users/PC/Desktop/Database/First fronted';
if (!fs.existsSync(outDir)) {
  fs.mkdirSync(outDir, { recursive: true });
}

fs.writeFileSync(path.join(outDir, 'pos-pwa-context.txt'), output, 'utf8');
console.log('Contexto generado en', path.join(outDir, 'pos-pwa-context.txt'));
