import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/test-utils';
import CategoriesPage from './CategoriesPage';

// Mock fetchCategories so the useEffect dispatch doesn't flip loading to true
vi.mock('./categoriesSlice', async () => {
  const actual = await vi.importActual('./categoriesSlice');
  return {
    ...actual,
    fetchCategories: () => ({ type: 'categories/fetchCategories_NOOP' }),
  };
});

describe('CategoriesPage', () => {
  it('renders the page heading "Categories"', () => {
    renderWithProviders(<CategoriesPage />);
    expect(screen.getByRole('heading', { name: /categories/i })).toBeInTheDocument();
  });

  it('shows "Add Category" button', () => {
    renderWithProviders(<CategoriesPage />);
    expect(screen.getByRole('button', { name: /add category/i })).toBeInTheDocument();
  });

  it('renders category table headers when not loading', () => {
    renderWithProviders(<CategoriesPage />, {
      preloadedState: {
        categories: {
          categories: [],
          loading: false,
          error: null,
        },
      },
    });

    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Transactions')).toBeInTheDocument();
    expect(screen.getByText('Merchants')).toBeInTheDocument();
    expect(screen.getByText('Actions')).toBeInTheDocument();
  });

  it('shows loading spinner when loading', () => {
    renderWithProviders(<CategoriesPage />, {
      preloadedState: {
        categories: {
          categories: [],
          loading: true,
          error: null,
        },
      },
    });

    const spinner = document.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();

    // Table should NOT be rendered while loading
    expect(screen.queryByText('Name')).not.toBeInTheDocument();
  });

  it('shows empty state when no categories', () => {
    renderWithProviders(<CategoriesPage />, {
      preloadedState: {
        categories: {
          categories: [],
          loading: false,
          error: null,
        },
      },
    });

    expect(
      screen.getByText('No categories found. Add one to get started.')
    ).toBeInTheDocument();
  });

  it('shows categories in table when loaded', () => {
    renderWithProviders(<CategoriesPage />, {
      preloadedState: {
        categories: {
          categories: [
            {
              id: 1,
              name: 'Groceries',
              color: '#22c55e',
              icon: null,
              isDefault: false,
              transactionCount: 15,
              merchantCount: 3,
            },
            {
              id: 2,
              name: 'Entertainment',
              color: '#6366f1',
              icon: null,
              isDefault: true,
              transactionCount: 8,
              merchantCount: 5,
            },
          ],
          loading: false,
          error: null,
        },
      },
    });

    expect(screen.getByText('Groceries')).toBeInTheDocument();
    expect(screen.getByText('Entertainment')).toBeInTheDocument();
    expect(screen.getByText('15')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('8')).toBeInTheDocument();
    expect(screen.getByText('5')).toBeInTheDocument();
    // Default badge
    expect(screen.getByText('(default)')).toBeInTheDocument();
    // Empty state should not be visible
    expect(
      screen.queryByText('No categories found. Add one to get started.')
    ).not.toBeInTheDocument();
  });

  it('displays error message when there is an error', () => {
    renderWithProviders(<CategoriesPage />, {
      preloadedState: {
        categories: {
          categories: [],
          loading: false,
          error: 'Failed to load categories',
        },
      },
    });

    expect(screen.getByText('Failed to load categories')).toBeInTheDocument();
  });
});
