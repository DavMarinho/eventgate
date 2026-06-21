import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { api, getToken, setToken } from '../api/client';

interface AuthState {
  token: string | null;
  role: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const ROLE_KEY = 'eventgate.role';

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setTokenState] = useState<string | null>(getToken());
  const [role, setRole] = useState<string | null>(localStorage.getItem(ROLE_KEY));

  const value = useMemo<AuthState>(
    () => ({
      token,
      role,
      isAuthenticated: !!token,
      async login(email, password) {
        const res = await api.login(email, password);
        setToken(res.accessToken);
        localStorage.setItem(ROLE_KEY, res.role);
        setTokenState(res.accessToken);
        setRole(res.role);
      },
      logout() {
        setToken(null);
        localStorage.removeItem(ROLE_KEY);
        setTokenState(null);
        setRole(null);
      },
    }),
    [token, role],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth deve ser usado dentro de <AuthProvider>.');
  }
  return ctx;
}
