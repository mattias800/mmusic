import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Alert } from './Alert';

describe('Alert Component', () => {
  it('renders with default variant', () => {
    render(<Alert>This is an alert</Alert>);
    const alert = screen.getByRole('alert');
    expect(alert).toBeInTheDocument();
    expect(alert.textContent).toBe('This is an alert');
    expect(alert).toHaveClass('bg-background');
  });

  it('renders with success variant', () => {
    render(<Alert variant="success">Success message</Alert>);
    const alert = screen.getByRole('alert');
    expect(alert).toBeInTheDocument();
    expect(alert).toHaveClass('bg-green-50');
  });

  it('renders with warning variant', () => {
    render(<Alert variant="warning">Warning message</Alert>);
    const alert = screen.getByRole('alert');
    expect(alert).toBeInTheDocument();
    expect(alert).toHaveClass('bg-yellow-50');
  });

  it('renders with error variant', () => {
    render(<Alert variant="error">Error message</Alert>);
    const alert = screen.getByRole('alert');
    expect(alert).toBeInTheDocument();
    expect(alert).toHaveClass('bg-red-50');
  });

  it('renders with a title', () => {
    render(
      <Alert title="Alert Title">
        This is an alert with a title
      </Alert>
    );
    const title = screen.getByText('Alert Title');
    expect(title).toBeInTheDocument();
    expect(title).toHaveClass('font-medium');
  });

  it('applies custom className', () => {
    render(
      <Alert className="custom-class">
        Alert with custom class
      </Alert>
    );
    const alert = screen.getByRole('alert');
    expect(alert).toHaveClass('custom-class');
  });
});
