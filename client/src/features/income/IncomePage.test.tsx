import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/test-utils';
import IncomePage from './IncomePage';

// Mock fetchIncome so the useEffect dispatch doesn't flip loading to true
vi.mock('./incomeSlice', async () => {
  const actual = await vi.importActual('./incomeSlice');
  return {
    ...actual,
    fetchIncome: () => ({ type: 'income/fetchIncome_NOOP' }),
  };
});

describe('IncomePage', () => {
  it('renders the "Income" heading', () => {
    renderWithProviders(<IncomePage />);
    expect(screen.getByRole('heading', { name: /income/i })).toBeInTheDocument();
  });

  it('shows "Add Income" button', () => {
    renderWithProviders(<IncomePage />);
    expect(screen.getByRole('button', { name: /add income/i })).toBeInTheDocument();
  });

  it('shows loading spinner when loading', () => {
    renderWithProviders(<IncomePage />, {
      preloadedState: {
        income: {
          incomes: [],
          loading: true,
          error: null,
        },
      },
    });

    const spinner = document.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();

    // Table should not render while loading
    expect(screen.queryByText('Source')).not.toBeInTheDocument();
  });

  it('shows empty state when no incomes', () => {
    renderWithProviders(<IncomePage />, {
      preloadedState: {
        income: {
          incomes: [],
          loading: false,
          error: null,
        },
      },
    });

    expect(
      screen.getByText('No income records found. Add one to get started.')
    ).toBeInTheDocument();
  });

  it('renders table headers when not loading', () => {
    renderWithProviders(<IncomePage />, {
      preloadedState: {
        income: {
          incomes: [],
          loading: false,
          error: null,
        },
      },
    });

    expect(screen.getByText('Source')).toBeInTheDocument();
    expect(screen.getByText('Amount')).toBeInTheDocument();
    expect(screen.getByText('Date')).toBeInTheDocument();
    expect(screen.getByText('Recurring')).toBeInTheDocument();
    expect(screen.getByText('Frequency')).toBeInTheDocument();
    expect(screen.getByText('Notes')).toBeInTheDocument();
    expect(screen.getByText('Actions')).toBeInTheDocument();
  });

  it('shows income records in table when loaded', () => {
    renderWithProviders(<IncomePage />, {
      preloadedState: {
        income: {
          incomes: [
            {
              id: 1,
              source: 'Salary',
              amount: 5000,
              date: '2026-03-01',
              isRecurring: true,
              frequency: 'Monthly',
              transactionId: null,
              notes: 'Main job',
            },
            {
              id: 2,
              source: 'Freelance',
              amount: 1200,
              date: '2026-03-15',
              isRecurring: false,
              frequency: null,
              transactionId: null,
              notes: null,
            },
          ],
          loading: false,
          error: null,
        },
      },
    });

    expect(screen.getByText('Salary')).toBeInTheDocument();
    expect(screen.getByText('$5,000.00')).toBeInTheDocument();
    expect(screen.getByText('2026-03-01')).toBeInTheDocument();
    expect(screen.getByText('Monthly')).toBeInTheDocument();
    expect(screen.getByText('Main job')).toBeInTheDocument();

    expect(screen.getByText('Freelance')).toBeInTheDocument();
    expect(screen.getByText('$1,200.00')).toBeInTheDocument();
    expect(screen.getByText('2026-03-15')).toBeInTheDocument();

    // Recurring badges
    const yesBadges = screen.getAllByText('Yes');
    const noBadges = screen.getAllByText('No');
    expect(yesBadges.length).toBeGreaterThanOrEqual(1);
    expect(noBadges.length).toBeGreaterThanOrEqual(1);

    // Empty state should not be visible
    expect(
      screen.queryByText('No income records found. Add one to get started.')
    ).not.toBeInTheDocument();
  });

  it('displays error message when there is an error', () => {
    renderWithProviders(<IncomePage />, {
      preloadedState: {
        income: {
          incomes: [],
          loading: false,
          error: 'Failed to load income records',
        },
      },
    });

    expect(screen.getByText('Failed to load income records')).toBeInTheDocument();
  });
});
