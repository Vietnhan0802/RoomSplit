import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import AmountInput from './AmountInput';

describe('AmountInput', () => {
  it('renders label when provided', () => {
    render(<AmountInput label="Số tiền" id="amount" />);
    expect(screen.getByText('Số tiền')).toBeInTheDocument();
  });

  it('does not render label when omitted', () => {
    render(<AmountInput id="amount" />);
    expect(screen.queryByRole('label')).not.toBeInTheDocument();
  });

  it('renders error message when error prop is set', () => {
    render(<AmountInput error="Số tiền phải lớn hơn 0" />);
    expect(screen.getByText('Số tiền phải lớn hơn 0')).toBeInTheDocument();
  });

  it('displays currency text (default "VND")', () => {
    render(<AmountInput />);
    expect(screen.getByText('VND')).toBeInTheDocument();
  });

  it('displays custom currency text', () => {
    render(<AmountInput currency="USD" />);
    expect(screen.getByText('USD')).toBeInTheDocument();
  });

  it('input has type="number"', () => {
    render(<AmountInput />);
    const input = screen.getByRole('spinbutton');
    expect(input).toHaveAttribute('type', 'number');
  });

  it('applies error border styling when error is present', () => {
    render(<AmountInput error="Required" />);
    const input = screen.getByRole('spinbutton');
    expect(input.className).toContain('border-red-500');
  });
});
