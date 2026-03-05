import { Provider } from "react-redux";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { store } from "./store/store";
import Layout from "./shared/components/Layout";
import UploadPage from "./features/upload/UploadPage";
import CategoriesPage from "./features/categories/CategoriesPage";
import MerchantRulesPage from "./features/categories/MerchantRulesPage";
import TransactionsPage from "./features/transactions/TransactionsPage";
import IncomePage from "./features/income/IncomePage";
import BudgetSetupPage from "./features/budgets/BudgetSetupPage";
import Dashboard from "./features/dashboard/Dashboard";
import SettingsPage from "./features/settings/SettingsPage";

function App() {
  return (
    <Provider store={store}>
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/transactions" element={<TransactionsPage />} />
            <Route path="/upload" element={<UploadPage />} />
            <Route path="/budgets" element={<BudgetSetupPage />} />
            <Route path="/income" element={<IncomePage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route
              path="/categories/merchants"
              element={<MerchantRulesPage />}
            />
            <Route path="/settings" element={<SettingsPage />} />
          </Routes>
        </Layout>
      </BrowserRouter>
    </Provider>
  );
}

export default App;
