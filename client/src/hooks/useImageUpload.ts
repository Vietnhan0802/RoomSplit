import { useState } from 'react';
import { filesApi } from '../api/files';
import { compressImage, validateFile } from '../utils/fileHelpers';
import { showToast } from '../components/ui/Toast';

export function useImageUpload() {
  const [isUploading, setIsUploading] = useState(false);

  const upload = async (file: File, folder: 'receipts' | 'avatars'): Promise<string | null> => {
    const error = validateFile(file);
    if (error) {
      showToast('error', error);
      return null;
    }

    try {
      setIsUploading(true);
      const compressed = await compressImage(file);
      const res = await filesApi.upload(compressed, folder);
      return res.data.data || null;
    } catch {
      showToast('error', 'Upload thất bại');
      return null;
    } finally {
      setIsUploading(false);
    }
  };

  return { upload, isUploading };
}
