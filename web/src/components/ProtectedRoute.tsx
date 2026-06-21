import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import type { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  /** Se informado, exige um destes perfis. */
  roles?: string[];
}

export default function ProtectedRoute({ children, roles }: Props) {
  const { isAuthenticated, role } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  if (roles && role && !roles.includes(role)) {
    return (
      <div className="card">
        <h2>Acesso negado</h2>
        <p>Seu perfil ({role}) não tem permissão para esta área.</p>
      </div>
    );
  }

  return <>{children}</>;
}
