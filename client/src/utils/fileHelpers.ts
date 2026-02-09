import imageCompression from 'browser-image-compression';

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

export function validateFile(file: File): string | null {
  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'Chỉ chấp nhận file JPG, PNG hoặc WebP';
  }
  if (file.size > MAX_FILE_SIZE) {
    return 'File không được vượt quá 5MB';
  }
  return null;
}

export async function compressImage(file: File): Promise<File> {
  const options = {
    maxSizeMB: 1,
    maxWidthOrHeight: 1920,
    useWebWorker: true,
  };
  return imageCompression(file, options);
}
