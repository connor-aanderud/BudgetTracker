import { NavLink } from "react-router-dom";
import type { ReactNode } from "react";

const navItems = [
  { to: "/", label: "Dashboard", icon: "📊" },
  { to: "/transactions", label: "Transactions", icon: "💳" },
  { to: "/upload", label: "Upload", icon: "📁" },
  { to: "/budgets", label: "Budgets", icon: "🎯" },
  { to: "/income", label: "Income", icon: "💰" },
  { to: "/categories", label: "Categories", icon: "🏷️" },
  { to: "/categories/merchants", label: "Merchant Rules", icon: "🏪" },
];

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  return (
    <div className="flex h-screen bg-background">
      <aside className="w-64 border-r border-border bg-card flex flex-col">
        <div className="p-6 border-b border-border">
          <h1 className="text-xl font-bold text-foreground">Budget Tracker</h1>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/"}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                }`
              }
            >
              <span>{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="flex-1 overflow-auto p-6">{children}</main>
    </div>
  );
}
