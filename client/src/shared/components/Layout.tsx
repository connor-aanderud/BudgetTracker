import { useState } from "react";
import { NavLink } from "react-router-dom";
import type { ReactNode } from "react";
import { Menu, Sun, Moon } from "lucide-react";
import { useTheme } from "@/shared/hooks/useTheme";

const navItems = [
  { to: "/", label: "Dashboard", icon: "\u{1F4CA}" },
  { to: "/transactions", label: "Transactions", icon: "\u{1F4B3}" },
  { to: "/upload", label: "Upload", icon: "\u{1F4C1}" },
  { to: "/budgets", label: "Budgets", icon: "\u{1F3AF}" },
  { to: "/income", label: "Income", icon: "\u{1F4B0}" },
  { to: "/categories", label: "Categories", icon: "\u{1F3F7}\uFE0F" },
  { to: "/categories/merchants", label: "Merchant Rules", icon: "\u{1F3EA}" },
  { to: "/settings", label: "Settings", icon: "\u2699\uFE0F" },
];

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const { isDark, toggle } = useTheme();

  const toggleMenu = () => setMenuOpen((prev) => !prev);
  const closeMenu = () => setMenuOpen(false);

  return (
    <div className="flex h-screen bg-background">
      {/* Mobile top bar - only shown on small screens */}
      <div className="md:hidden fixed top-0 left-0 right-0 z-30 h-14 border-b bg-card flex items-center px-4">
        <button onClick={toggleMenu}>
          <Menu className="h-5 w-5" />
        </button>
        <h1 className="ml-3 font-bold text-foreground">Budget Tracker</h1>
      </div>

      {/* Overlay for mobile when menu is open */}
      {menuOpen && (
        <div
          className="md:hidden fixed inset-0 z-40 bg-black/50"
          onClick={closeMenu}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`fixed md:static z-50 h-full w-64 border-r border-border bg-card flex flex-col transition-transform ${
          menuOpen ? "translate-x-0" : "-translate-x-full"
        } md:translate-x-0`}
      >
        <div className="p-6 border-b border-border">
          <h1 className="text-xl font-bold text-foreground">Budget Tracker</h1>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/"}
              onClick={closeMenu}
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
        {/* Footer with dark mode toggle */}
        <div className="p-4 border-t border-border">
          <button
            onClick={toggle}
            className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            {isDark ? (
              <Sun className="h-4 w-4" />
            ) : (
              <Moon className="h-4 w-4" />
            )}
            {isDark ? "Light Mode" : "Dark Mode"}
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto p-6 pt-20 md:pt-6">
        {children}
      </main>
    </div>
  );
}
