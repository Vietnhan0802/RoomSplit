/* eslint-disable react-refresh/only-export-components */
import { createContext, useState, useEffect, type ReactNode } from 'react';
import type { User } from '../types';
import { authApi } from '../api/auth';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (token: string, refreshToken: string, user: User) => void;
  logout: () => void;
  updateProfile: (fullName?: string, avatar?: File) => Promise<void>;
  refreshUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType>({
  user: null,
  token: null,
  isLoading: true,
  login: () => {},
  logout: () => {},
  updateProfile: async () => {},
  refreshUser: async () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [isLoading, setIsLoading] = useState(() => !!localStorage.getItem('token'));

  useEffect(() => {
    if (!token) return;
    authApi
      .getMe()
      .then((res) => {
        if (res.data.data) {
          setUser(res.data.data);
        }
      })
      .catch(() => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        setToken(null);
      })
      .finally(() => setIsLoading(false));
  }, [token]);

  const login = (newToken: string, newRefreshToken: string, newUser: User) => {
    localStorage.setItem('token', newToken);
    localStorage.setItem('refreshToken', newRefreshToken);
    setToken(newToken);
    setUser(newUser);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    setToken(null);
    setUser(null);
  };

  const updateProfile = async (fullName?: string, avatar?: File) => {
    const formData = new FormData();
    if (fullName) formData.append('fullName', fullName);
    if (avatar) formData.append('avatar', avatar);

    const res = await authApi.updateProfile(formData);
    if (res.data.data) {
      setUser(res.data.data);
    }
  };

  const refreshUser = async () => {
    const res = await authApi.getMe();
    if (res.data.data) {
      setUser(res.data.data);
    }
  };

  return (
    <AuthContext.Provider value={{ user, token, isLoading, login, logout, updateProfile, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}
