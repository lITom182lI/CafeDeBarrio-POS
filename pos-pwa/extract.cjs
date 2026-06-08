const fs = require('fs');
const path = require('path');

const content = fs.readFileSync('c:/Users/PC/Desktop/Database/First fronted/Interfaz pwa.txt', 'utf-8');

const lines = content.split('\n');
let currentFile = null;
let currentContent = [];

for (let i = 0; i < lines.length; i++) {
  const line = lines[i];
  
  // Detect file header
  const matchFile = line.match(/\/?[\*\/]\s*ARCHIVO:\s*(src\/[a-zA-Z0-9_\.\/]+)/i);
  if (matchFile) {
    if (currentFile) {
      // pop trailing 'code' and 'Tsx' / 'CSS' lines if they exist
      while (currentContent.length > 0 && ['code', 'Tsx', 'CSS', 'code\r', 'Tsx\r', 'CSS\r'].includes(currentContent[currentContent.length - 1].trim())) {
        currentContent.pop();
      }
      
      const fullPath = path.join(__dirname, currentFile);
      fs.mkdirSync(path.dirname(fullPath), { recursive: true });
      fs.writeFileSync(fullPath, currentContent.join('\n'));
      console.log('Wrote', fullPath);
    }
    
    currentFile = matchFile[1];
    currentContent = [];
    
    // Check if the previous lines were the ────── separator, if so, we don't want them
    // Wait, the match is just the ARCHIVO line. The content we want starts after the next separator.
    // Let's just collect everything after the separator.
    
    // Skip the next separator line if it exists
    if (i + 1 < lines.length && lines[i + 1].includes('────')) {
      i++;
    }
    continue;
  }
  
  if (currentFile) {
    currentContent.push(line);
  }
}

if (currentFile) {
  while (currentContent.length > 0 && ['code', 'Tsx', 'CSS', 'code\r', 'Tsx\r', 'CSS\r'].includes(currentContent[currentContent.length - 1].trim())) {
    currentContent.pop();
  }
  const fullPath = path.join(__dirname, currentFile);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  fs.writeFileSync(fullPath, currentContent.join('\n'));
  console.log('Wrote', fullPath);
}
