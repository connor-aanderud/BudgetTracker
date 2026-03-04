import { Provider } from 'react-redux';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { store } from './store/store';
import Layout from './shared/components/Layout';

const Dashboard = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Dashboard</h2>
    <p className="text-muted-foreground">Coming soon</p>
  </div>
);

const Transactions = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Transactions</h2>
    <p className="text-muted-foreground">Coming soon</p>
  </div>
);

const Upload = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Upload Statement</h2>
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

const Categories = () => (
  <div>
    <h2 className="text-2xl font-bold mb-4">Categories</h2>
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
            <Route path="/transactions" element={<Transactions />} />
            <Route path="/upload" element={<Upload />} />
            <Route path="/budgets" element={<Budgets />} />
            <Route path="/income" element={<Income />} />
            <Route path="/categories" element={<Categories />} />
          </Routes>
        </Layout>
      </BrowserRouter>
    </Provider>
  );
}

export default App;
