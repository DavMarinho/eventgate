import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function Layout() {
  const { isAuthenticated, role, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/');
  }

  return (
    <div className="app">
      <header className="topbar">
        <Link to="/" className="brand">
          Event<span>Gate</span>
        </Link>
        <nav className="nav">
          <NavLink to="/" end>
            Home
          </NavLink>
          <NavLink to="/eventos">Eventos</NavLink>
          <NavLink to="/equipe">Equipe</NavLink>
          {isAuthenticated && (
            <button className="link-btn" onClick={handleLogout}>
              Sair ({role})
            </button>
          )}
        </nav>
      </header>
      <main className="content">
        <Outlet />
      </main>
      <footer className="footer">
        <span>EventGate — Segurança + LGPD by design.</span>
        <Link to="/my-data" className="footer-link">
          Já me inscrevi
        </Link>
      </footer>
    </div>
  );
}
