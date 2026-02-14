import { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { Camera, ImagePlus, X } from 'lucide-react';
import imageCompression from 'browser-image-compression';
import type { TransactionImage } from '../../types';

interface ImageUploadSectionProps {
  images: File[];
  existingImages?: TransactionImage[];
  onAdd: (files: File[]) => void;
  onRemove: (index: number) => void;
  onRemoveExisting?: (imageId: string) => void;
  maxImages?: number;
}

export default function ImageUploadSection({
  images,
  existingImages = [],
  onAdd,
  onRemove,
  onRemoveExisting,
  maxImages = 3,
}: ImageUploadSectionProps) {
  const [previews, setPreviews] = useState<string[]>([]);

  const totalCount = existingImages.length + images.length;
  const canAdd = totalCount < maxImages;

  const onDrop = useCallback(
    async (acceptedFiles: File[]) => {
      const remaining = maxImages - totalCount;
      const filesToProcess = acceptedFiles.slice(0, remaining);

      const compressed = await Promise.all(
        filesToProcess.map((file) =>
          imageCompression(file, {
            maxSizeMB: 1,
            maxWidthOrHeight: 1920,
            useWebWorker: true,
          })
        )
      );

      const newPreviews = compressed.map((f) => URL.createObjectURL(f));
      setPreviews((prev) => [...prev, ...newPreviews]);
      onAdd(compressed);
    },
    [maxImages, totalCount, onAdd]
  );

  const handleRemove = (index: number) => {
    URL.revokeObjectURL(previews[index]);
    setPreviews((prev) => prev.filter((_, i) => i !== index));
    onRemove(index);
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': ['.jpg', '.jpeg', '.png', '.webp'] },
    disabled: !canAdd,
    maxFiles: maxImages - totalCount,
  });

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
        Ảnh ({totalCount}/{maxImages})
      </label>

      {/* Existing images */}
      {existingImages.length > 0 && (
        <div className="flex gap-2 flex-wrap">
          {existingImages.map((img) => (
            <div key={img.id} className="relative h-20 w-20 rounded-lg overflow-hidden">
              <img
                src={`/uploads${img.thumbnailUrl || img.imageUrl}`}
                alt={img.originalFileName}
                className="h-full w-full object-cover"
              />
              {onRemoveExisting && (
                <button
                  type="button"
                  onClick={() => onRemoveExisting(img.id)}
                  className="absolute right-0.5 top-0.5 rounded-full bg-black/60 p-0.5 text-white hover:bg-black/80"
                >
                  <X className="h-3 w-3" />
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {/* New image previews */}
      {previews.length > 0 && (
        <div className="flex gap-2 flex-wrap">
          {previews.map((src, i) => (
            <div key={i} className="relative h-20 w-20 rounded-lg overflow-hidden">
              <img src={src} alt={`Preview ${i + 1}`} className="h-full w-full object-cover" />
              <button
                type="button"
                onClick={() => handleRemove(i)}
                className="absolute right-0.5 top-0.5 rounded-full bg-black/60 p-0.5 text-white hover:bg-black/80"
              >
                <X className="h-3 w-3" />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Dropzone */}
      {canAdd && (
        <div
          {...getRootProps()}
          className={`flex cursor-pointer items-center justify-center gap-2 rounded-lg border-2 border-dashed p-4 transition-colors ${
            isDragActive
              ? 'border-primary-400 bg-primary-50 dark:bg-primary-900/20'
              : 'border-gray-300 hover:border-gray-400 dark:border-gray-600'
          }`}
        >
          <input {...getInputProps()} />
          <div className="flex items-center gap-3 text-sm text-gray-500">
            <Camera className="h-5 w-5" />
            <span>Chụp ảnh</span>
            <span className="text-gray-300">|</span>
            <ImagePlus className="h-5 w-5" />
            <span>Chọn từ thư viện</span>
          </div>
        </div>
      )}
    </div>
  );
}
