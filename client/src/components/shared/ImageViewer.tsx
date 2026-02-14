import { useState, useCallback, useEffect, useRef } from 'react';
import { X, ChevronLeft, ChevronRight, Download } from 'lucide-react';

interface ImageViewerProps {
  images: { url: string; name?: string }[];
  initialIndex: number;
  isOpen: boolean;
  onClose: () => void;
}

export default function ImageViewer({ images, initialIndex, isOpen, onClose }: ImageViewerProps) {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [scale, setScale] = useState(1);
  const touchStartX = useRef(0);

  useEffect(() => {
    setCurrentIndex(initialIndex);
    setScale(1);
  }, [initialIndex, isOpen]);

  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') prev();
      if (e.key === 'ArrowRight') next();
    };
    document.addEventListener('keydown', handleKeyDown);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = '';
    };
  }, [isOpen, currentIndex]);

  const prev = useCallback(() => {
    setCurrentIndex((i) => (i > 0 ? i - 1 : images.length - 1));
    setScale(1);
  }, [images.length]);

  const next = useCallback(() => {
    setCurrentIndex((i) => (i < images.length - 1 ? i + 1 : 0));
    setScale(1);
  }, [images.length]);

  const handleWheel = (e: React.WheelEvent) => {
    e.preventDefault();
    setScale((s) => Math.min(Math.max(s + (e.deltaY > 0 ? -0.1 : 0.1), 0.5), 3));
  };

  const handleTouchStart = (e: React.TouchEvent) => {
    touchStartX.current = e.touches[0].clientX;
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    const diff = touchStartX.current - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 50) {
      if (diff > 0) next();
      else prev();
    }
  };

  const handleDownload = () => {
    const img = images[currentIndex];
    const link = document.createElement('a');
    link.href = img.url;
    link.download = img.name || 'image';
    link.click();
  };

  if (!isOpen || images.length === 0) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/90">
      {/* Header */}
      <div className="absolute left-0 right-0 top-0 z-10 flex items-center justify-between p-4">
        <span className="text-sm text-white/70">
          {currentIndex + 1} / {images.length}
        </span>
        <div className="flex items-center gap-2">
          <button
            onClick={handleDownload}
            className="rounded-full bg-white/10 p-2 text-white hover:bg-white/20"
          >
            <Download className="h-5 w-5" />
          </button>
          <button
            onClick={onClose}
            className="rounded-full bg-white/10 p-2 text-white hover:bg-white/20"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
      </div>

      {/* Navigation arrows */}
      {images.length > 1 && (
        <>
          <button
            onClick={prev}
            className="absolute left-2 z-10 rounded-full bg-white/10 p-2 text-white hover:bg-white/20"
          >
            <ChevronLeft className="h-6 w-6" />
          </button>
          <button
            onClick={next}
            className="absolute right-2 z-10 rounded-full bg-white/10 p-2 text-white hover:bg-white/20"
          >
            <ChevronRight className="h-6 w-6" />
          </button>
        </>
      )}

      {/* Image */}
      <div
        className="flex h-full w-full items-center justify-center"
        onWheel={handleWheel}
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
      >
        <img
          src={images[currentIndex].url}
          alt={images[currentIndex].name || ''}
          className="max-h-[85vh] max-w-[90vw] object-contain transition-transform duration-200"
          style={{ transform: `scale(${scale})` }}
          draggable={false}
        />
      </div>
    </div>
  );
}
