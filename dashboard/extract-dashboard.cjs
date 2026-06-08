const fs = require('fs');
const path = require('path');

const filePath = 'C:/Users/PC/Desktop/Database/First fronted/Interfaz CafeBarrio.txt';
const content = fs.readFileSync(filePath, 'utf8');

const lines = content.split('\n');
let currentFile = null;
let currentContent = [];

const fileMarkerRegex = /^\/src\//;

for (let i = 0; i < lines.length; i++) {
  const line = lines[i];
  const trimLine = line.trim();

  // Detect file header like /src/App.tsx
  if (fileMarkerRegex.test(line)) {
    if (currentFile) {
      // pop trailing 'code', 'Tsx', 'CSS' lines if they exist
      while (currentContent.length > 0 && ['code', 'Tsx', 'CSS'].includes(currentContent[currentContent.length - 1].trim())) {
        currentContent.pop();
      }
      const fullPath = path.join(__dirname, currentFile);
      fs.mkdirSync(path.dirname(fullPath), { recursive: true });
      fs.writeFileSync(fullPath, currentContent.join('\n'));
      console.log('Wrote', fullPath);
    }
    
    currentFile = line.trim(); // e.g. /src/App.tsx
    // Remove leading slash to make it relative to __dirname (which is the dashboard dir)
    if (currentFile.startsWith('/')) {
        currentFile = currentFile.substring(1); // src/App.tsx
    }
    currentContent = [];
    
    // Skip 'code', 'Tsx' lines right after the file name
    while (i + 1 < lines.length && ['code', 'Tsx', 'CSS'].includes(lines[i + 1].trim())) {
        i++;
    }
    continue;
  }
  
  if (currentFile) {
    currentContent.push(line);
  }
}

if (currentFile) {
  while (currentContent.length > 0 && ['code', 'Tsx', 'CSS'].includes(currentContent[currentContent.length - 1].trim())) {
    currentContent.pop();
  }
  const fullPath = path.join(__dirname, currentFile);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  fs.writeFileSync(fullPath, currentContent.join('\n'));
  console.log('Wrote', fullPath);
}
