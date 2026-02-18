#!/usr/bin/env node

/**
 * PWA Icon Generator Script
 *
 * Generates all required PWA icons from the base SVG.
 *
 * Prerequisites:
 *   npm install -D sharp
 *
 * Usage:
 *   node scripts/generate-icons.mjs
 */

import { readFileSync, mkdirSync, existsSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '..');
const ICONS_DIR = join(ROOT, 'public', 'icons');
const PUBLIC_DIR = join(ROOT, 'public');

const ICON_SIZES = [72, 96, 128, 144, 152, 192, 384, 512];
const SHORTCUT_SIZE = 96;
const APPLE_TOUCH_SIZE = 180;
const FAVICON_SIZES = [16, 32];

const SPLASH_SCREENS = [
  { name: 'splash-640x1136', width: 640, height: 1136 },
  { name: 'splash-750x1334', width: 750, height: 1334 },
  { name: 'splash-1242x2208', width: 1242, height: 2208 },
  { name: 'splash-1125x2436', width: 1125, height: 2436 },
  { name: 'splash-1170x2532', width: 1170, height: 2532 },
];

async function main() {
  let sharp;
  try {
    sharp = (await import('sharp')).default;
  } catch {
    console.error('❌ "sharp" is not installed. Run: npm install -D sharp');
    console.log('\nThen re-run: node scripts/generate-icons.mjs');
    process.exit(1);
  }

  if (!existsSync(ICONS_DIR)) mkdirSync(ICONS_DIR, { recursive: true });

  const svgPath = join(ICONS_DIR, 'icon-base.svg');
  if (!existsSync(svgPath)) {
    console.error(`❌ Base SVG not found at ${svgPath}`);
    process.exit(1);
  }

  const svgBuffer = readFileSync(svgPath);
  console.log('🎨 Generating PWA icons...\n');

  // Standard icons
  for (const size of ICON_SIZES) {
    const filename = `icon-${size}x${size}.png`;
    await sharp(svgBuffer).resize(size, size).png().toFile(join(ICONS_DIR, filename));
    console.log(`  ✅ icons/${filename}`);
  }

  // Maskable icon (with 10% padding for safe zone)
  const maskableSize = 512;
  const innerSize = Math.round(maskableSize * 0.8);
  const padding = Math.round((maskableSize - innerSize) / 2);
  const innerBuffer = await sharp(svgBuffer).resize(innerSize, innerSize).png().toBuffer();
  await sharp({
    create: { width: maskableSize, height: maskableSize, channels: 4, background: { r: 79, g: 70, b: 229, alpha: 1 } },
  })
    .composite([{ input: innerBuffer, left: padding, top: padding }])
    .png()
    .toFile(join(ICONS_DIR, `maskable-icon-${maskableSize}x${maskableSize}.png`));
  console.log(`  ✅ icons/maskable-icon-${maskableSize}x${maskableSize}.png`);

  // Shortcut icons
  for (const name of ['shortcut-add', 'shortcut-dashboard']) {
    await sharp(svgBuffer).resize(SHORTCUT_SIZE, SHORTCUT_SIZE).png().toFile(join(ICONS_DIR, `${name}.png`));
    console.log(`  ✅ icons/${name}.png`);
  }

  // Apple touch icon
  await sharp(svgBuffer).resize(APPLE_TOUCH_SIZE, APPLE_TOUCH_SIZE).png().toFile(join(PUBLIC_DIR, 'apple-touch-icon.png'));
  console.log(`  ✅ apple-touch-icon.png`);

  // Favicons
  for (const size of FAVICON_SIZES) {
    const filename = `favicon-${size}x${size}.png`;
    await sharp(svgBuffer).resize(size, size).png().toFile(join(PUBLIC_DIR, filename));
    console.log(`  ✅ ${filename}`);
  }

  // Splash screens (indigo background with centered icon)
  console.log('\n🖼️  Generating splash screens...\n');
  for (const { name, width, height } of SPLASH_SCREENS) {
    const iconSize = Math.round(Math.min(width, height) * 0.3);
    const iconBuffer = await sharp(svgBuffer).resize(iconSize, iconSize).png().toBuffer();
    await sharp({
      create: { width, height, channels: 4, background: { r: 79, g: 70, b: 229, alpha: 1 } },
    })
      .composite([{
        input: iconBuffer,
        left: Math.round((width - iconSize) / 2),
        top: Math.round((height - iconSize) / 2) - Math.round(height * 0.05),
      }])
      .png()
      .toFile(join(PUBLIC_DIR, `${name}.png`));
    console.log(`  ✅ ${name}.png`);
  }

  // Placeholder screenshots (just empty indigo screens)
  const SCREENSHOTS_DIR = join(PUBLIC_DIR, 'screenshots');
  if (!existsSync(SCREENSHOTS_DIR)) mkdirSync(SCREENSHOTS_DIR, { recursive: true });
  for (const name of ['dashboard', 'finance']) {
    await sharp({
      create: { width: 390, height: 844, channels: 4, background: { r: 79, g: 70, b: 229, alpha: 1 } },
    })
      .png()
      .toFile(join(SCREENSHOTS_DIR, `${name}.png`));
    console.log(`  ✅ screenshots/${name}.png`);
  }

  console.log('\n✨ All icons generated successfully!');
}

main().catch(console.error);
