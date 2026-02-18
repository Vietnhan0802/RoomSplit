import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import TransactionForm from './TransactionForm';

describe('TransactionForm', () => {
  const mockOnSubmit = vi.fn();

  it('renders income/expense radio buttons', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />);
    expect(screen.getByText('Thu nhập')).toBeInTheDocument();
    expect(screen.getByText('Chi tiêu')).toBeInTheDocument();
  });

  it('renders submit button with "Thêm giao dịch" text', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />);
    expect(screen.getByRole('button', { name: /Thêm giao dịch/i })).toBeInTheDocument();
  });

  it('shows loading state on button when isLoading=true', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} isLoading={true} />);
    const button = screen.getByRole('button', { name: /Thêm giao dịch/i });
    expect(button).toBeDisabled();
  });
});
