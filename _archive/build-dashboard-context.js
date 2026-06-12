const fs = require('fs');
const path = require('path');

const filesToInclude = [
  'src/CafeBarrio.API/Controllers/OperadoresController.cs',
  'src/CafeBarrio.API/Controllers/TransaccionesController.cs',
  'src/CafeBarrio.API/Controllers/ProductosController.cs',
  'src/CafeBarrio.API/Controllers/ReportesController.cs',
  'dashboard/src/api/CafeBarrioApiAdapter.ts',
  'dashboard/src/types/index.ts',
  'dashboard/package.json',
  'dashboard/src/index.css',
  'dashboard/src/pages/DashboardPage.tsx',
  'dashboard/src/components/OperadorModal.tsx'
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

fs.writeFileSync(path.join(outDir, 'dashboard-context.txt'), output, 'utf8');
console.log('Contexto generado en', path.join(outDir, 'dashboard-context.txt'));
