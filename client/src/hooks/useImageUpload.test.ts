import { renderHook } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { useImageUpload } from './useImageUpload';

vi.mock('../api/files', () => ({
  filesApi: {
    upload: vi.fn(),
  },
}));

vi.mock('../utils/fileHelpers', () => ({
  validateFile: vi.fn().mockReturnValue(null),
  compressImage: vi.fn().mockImplementation((file: File) => Promise.resolve(file)),
}));

vi.mock('../components/ui/Toast', () => ({
  showToast: vi.fn(),
}));

describe('useImageUpload', () => {
  it('returns upload function and isUploading state', () => {
    const { result } = renderHook(() => useImageUpload());
    expect(result.current.upload).toBeDefined();
    expect(typeof result.current.upload).toBe('function');
    expect('isUploading' in result.current).toBe(true);
  });

  it('initial isUploading is false', () => {
    const { result } = renderHook(() => useImageUpload());
    expect(result.current.isUploading).toBe(false);
  });
});
