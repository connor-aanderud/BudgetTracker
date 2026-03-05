import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/test-utils';
import Dashboard from './Dashboard';
import type { DashboardState } from './dashboardSlice';

// Mock recharts to avoid jsdom rendering issues with SVG charts
vi.mock('recharts', () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  PieChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="pie-chart">{children}</div>
  ),
  Pie: () => null,
  Cell: () => null,
  BarChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="bar-chart">{children}</div>
  ),
  Bar: () => null,
  LineChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="line-chart">{children}</div>
  ),
  Line: () => null,
  XAxis: () => null,
  YAxis: () => null,
  Tooltip: () => null,
  Legend: () => null,
  CartesianGrid: () => null,
}));

// Mock fetchDashboard so the useEffect dispatch doesn't flip loading to true
vi.mock('./dashboardSlice', async () => {
  const actual = await vi.importActual('./dashboardSlice');
  return {
    ...actual,
    fetchDashboard: () => ({ type: 'dashboard/fetchDashboard_NOOP' }),
  };
});

function buildDashboardState(overrides: Partial<DashboardState> = {}): DashboardState {
  return {
    month: '2026-03',
    summary: null,
    categoryBreakdown: [],
    budgetStatus: [],
    trends: [],
    topMerchants: [],
    monthlyComparison: [],
    loading: false,
    error: null,
    ...overrides,
  };
}

describe('Dashboard', () => {
  it('renders the month picker input', () => {
    renderWithProviders(<Dashboard />, {
      preloadedState: {
        dashboard: buildDashboardState(),
      },
    });

    const monthInput = document.querySelector('input[type="month"]');
    expect(monthInput).toBeInTheDocument();
    expect(monthInput).toHaveValue('2026-03');
  });

  it('shows loading spinner when loading', () => {
    renderWithProviders(<Dashboard />, {
      preloadedState: {
        dashboard: buildDashboardState({ loading: true }),
      },
    });

    const spinner = document.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();

    // Chart section titles should NOT render while loading
    expect(screen.queryByText('Spending by Category')).not.toBeInTheDocument();
    expect(screen.queryByText('Budget vs Actual')).not.toBeInTheDocument();
  });

  it('renders summary cards when data is loaded', () => {
    renderWithProviders(<Dashboard />, {
      preloadedState: {
        dashboard: buildDashboardState({
          summary: {
            month: '2026-03',
            totalIncome: 5000,
            totalExpenses: 3200,
            netSavings: 1800,
            previousMonthExpenses: 2800,
            percentChangeExpenses: 14.3,
          },
        }),
      },
    });

    expect(screen.getByText('Total Income')).toBeInTheDocument();
    expect(screen.getByText('Total Expenses')).toBeInTheDocument();
    expect(screen.getByText('Net Savings')).toBeInTheDocument();
    expect(screen.getByText('$5,000.00')).toBeInTheDocument();
    expect(screen.getByText('$3,200.00')).toBeInTheDocument();
    expect(screen.getByText('$1,800.00')).toBeInTheDocument();
  });

  it('renders all chart section titles when data is loaded', () => {
    renderWithProviders(<Dashboard />, {
      preloadedState: {
        dashboard: buildDashboardState({
          summary: {
            month: '2026-03',
            totalIncome: 5000,
            totalExpenses: 3200,
            netSavings: 1800,
            previousMonthExpenses: null,
            percentChangeExpenses: null,
          },
          categoryBreakdown: [
            {
              categoryId: 1,
              categoryName: 'Groceries',
              categoryColor: '#22c55e',
              totalAmount: 800,
              transactionCount: 20,
              percentOfTotal: 25,
            },
          ],
          budgetStatus: [
            {
              categoryId: 1,
              categoryName: 'Groceries',
              categoryColor: '#22c55e',
              limitAmount: 1000,
              actualAmount: 800,
              remainingAmount: 200,
              overBudget: false,
              percentUsed: 80,
            },
          ],
          trends: [
            {
              month: '2026-01',
              totalAmount: 2500,
              categoryId: null,
              categoryName: null,
            },
            {
              month: '2026-02',
              totalAmount: 2800,
              categoryId: null,
              categoryName: null,
            },
          ],
          topMerchants: [
            {
              merchantId: 1,
              merchantName: 'Whole Foods',
              totalAmount: 450,
              transactionCount: 12,
            },
          ],
          monthlyComparison: [
            {
              categoryId: 1,
              categoryName: 'Groceries',
              categoryColor: '#22c55e',
              currentMonth: 800,
              previousMonth: 750,
              threeMonthAverage: 770,
            },
          ],
        }),
      },
    });

    expect(screen.getByText('Spending by Category')).toBeInTheDocument();
    expect(screen.getByText('Budget vs Actual')).toBeInTheDocument();
    expect(screen.getByText('Spending Trends')).toBeInTheDocument();
    expect(screen.getByText('Top Merchants')).toBeInTheDocument();
    expect(screen.getByText('Monthly Comparison')).toBeInTheDocument();
    expect(screen.getByText('Budget Alerts')).toBeInTheDocument();
  });

  it('renders the Dashboard heading', () => {
    renderWithProviders(<Dashboard />, {
      preloadedState: {
        dashboard: buildDashboardState(),
      },
    });

    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument();
    expect(screen.getByText('Overview of your finances')).toBeInTheDocument();
  });

  it('displays error message when there is an error', () => {
    renderWithProviders(<Dashboard />, {
      preloadedState: {
        dashboard: buildDashboardState({
          error: 'Network error occurred',
        }),
      },
    });

    expect(screen.getByText('Network error occurred')).toBeInTheDocument();
  });
});
