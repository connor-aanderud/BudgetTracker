import { Provider } from "react-redux";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { store } from "./store/store";
import Layout from "./shared/components/Layout";
import UploadPage from "./features/upload/UploadPage";
import CategoriesPage from "./features/categories/CategoriesPage";
import MerchantRulesPage from "./features/categories/MerchantRulesPage";
import TransactionsPage from "./features/transactions/TransactionsPage";

const Dashboard = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Dashboard</h2>
    <p className="text-muted-foreground">Coming soon</p>
  </div>
);

const Budgets = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Budgets</h2>
    <p className="text-muted-foreground">Coming soon</p>
  </div>
);

const Income = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Income</h2>
    <p className="text-muted-foreground">Coming soon</p>
  </div>
);

function App() {
  return (
    <Provider store={store}>
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/transactions" element={<TransactionsPage />} />
            <Route path="/upload" element={<UploadPage />} />
            <Route path="/budgets" element={<Budgets />} />
            <Route path="/income" element={<Income />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route
              path="/categories/merchants"
              element={<MerchantRulesPage />}
            />
          </Routes>
        </Layout>
      </BrowserRouter>
    </Provider>
  );
}

export default App;
