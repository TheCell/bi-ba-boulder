import { NodeIO } from '@gltf-transform/core';
import { createHash, randomUUID } from 'crypto';
import path from 'path';
import fs from 'fs/promises';
import { EXTTextureWebP, KHRMaterialsSpecular } from '@gltf-transform/extensions';
import { CopyrightIndex } from './ICopyrightIndex';
import type { CopyrightEntry } from './ICopyrightEntry';

main().catch((error) => {
  console.error(error);
  process.exit(1);
});

async function main() {
  const folder = process.argv[2];

  if (!folder) {
    console.error('Usage: npx tsx add-copyright.ts <folder>');
    process.exit(1);
  }

  const files = await fs.readdir(folder);

  const glbFiles = files.filter((f) => f.toLowerCase().endsWith('.glb'));

  if (glbFiles.length === 0) {
    console.log('No GLB files found.');
    return;
  }

  const indexPath = path.join(folder, 'copyright-index.json');

  const index = await loadIndex(indexPath);

  for (const file of glbFiles) {
    const entry = await processFile(path.join(folder, file));
    index.assets.push(entry);
  }

  await saveIndex(indexPath, index);
}

async function processFile(filePath: string): Promise<CopyrightEntry> {
  const io = new NodeIO();
  io.registerExtensions([EXTTextureWebP, KHRMaterialsSpecular]);
  const document = await io.read(filePath);
  const copyrightId = randomUUID();

  // Access the underlying glTF JSON
  const root = document.getRoot();
  const json = root.getExtras();

  root.setExtras({
    ...(json ?? {}),
    copyrightId,
    owner: 'Simon Hischier',
    createdAt: new Date().toISOString(),
    watermarkVersion: 1
  });

  const outputPath = path.join(path.dirname(filePath), `${copyrightId}.glb`);

  await io.write(outputPath, document);

  await addAssetCopyright(outputPath, `© Simon Hischier - ${copyrightId}`);
  const sha256 = await calculateSha256(outputPath);

  console.log(`${path.basename(filePath)} -> ${path.basename(outputPath)}`);
  console.log(`  Copyright ID: ${copyrightId}`);
  return {
    copyrightId,
    sourceFile: path.basename(filePath),
    generatedFile: path.basename(outputPath),
    sha256,
    createdAt: new Date().toISOString()
  };
}

async function addAssetCopyright(filePath: string, copyright: string) {
  const buffer = await fs.readFile(filePath);

  // Read GLB JSON chunk
  const jsonLength = buffer.readUInt32LE(12);
  const jsonStart = 20;

  const jsonBuffer = buffer.subarray(jsonStart, jsonStart + jsonLength);

  const json = JSON.parse(jsonBuffer.toString('utf8').trim());

  json.asset ??= {};
  json.asset.copyright = copyright;

  let newJson = Buffer.from(JSON.stringify(json), 'utf8');

  // GLB chunks must be aligned to 4 bytes
  const padding = (4 - (newJson.length % 4)) % 4;

  if (padding > 0) {
    newJson = Buffer.concat([
      newJson,
      Buffer.alloc(padding, 0x20) // spaces
    ]);
  }

  const oldJsonChunkEnd = jsonStart + jsonLength;
  const rest = buffer.subarray(oldJsonChunkEnd);
  const newFileLength = 12 + 8 + newJson.length + rest.length;

  const header = Buffer.from(buffer.subarray(0, 12));
  header.writeUInt32LE(newFileLength, 8);

  const jsonHeader = Buffer.alloc(8);
  jsonHeader.writeUInt32LE(newJson.length, 0);
  jsonHeader.writeUInt32LE(0x4e4f534a, 4); // JSON

  await fs.writeFile(filePath, Buffer.concat([header, jsonHeader, newJson, rest]));
}

async function loadIndex(indexPath: string): Promise<CopyrightIndex> {
  try {
    const content = await fs.readFile(indexPath, 'utf8');
    return JSON.parse(content);
  } catch {
    return {
      generatedAt: new Date().toISOString(),
      assets: []
    };
  }
}

async function saveIndex(indexPath: string, index: CopyrightIndex) {
  index.generatedAt = new Date().toISOString();

  await fs.writeFile(indexPath, JSON.stringify(index, null, 2), 'utf8');
}

async function calculateSha256(filePath: string): Promise<string> {
  const buffer = await fs.readFile(filePath);

  return createHash('sha256').update(buffer).digest('hex');
}
