'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { api, LoginRequest, LoginResponse } from './api';

interface DecodedToken {
  roles: string[];
  exp: number;
  [key: string]: unknown;
}

interface AuthContextType {
  user: LoginResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  roles: string[];
  hasRole: (role: string) => boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Helper function to decode JWT token
function decodeToken(token: string): DecodedToken | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const decoded = JSON.parse(jsonPayload);

    // Extract roles - they might be in different claim names
    const rolesString = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                       decoded['role'] ||
                       decoded['roles'] ||
                       '';

    // Roles are separated by semicolon
    const roles = typeof rolesString === 'string'
      ? rolesString.split(';').filter(Boolean)
      : Array.isArray(rolesString)
        ? rolesString
        : [];

    return {
      ...decoded,
      roles,
    };
  } catch (error) {
    console.error('Error decoding token:', error);
    return null;
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<LoginResponse | null>(null);
  const [roles, setRoles] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check for stored auth token on mount
    const token = localStorage.getItem('auth_token');
    const userStr = localStorage.getItem('user');

    if (token && userStr) {
      try {
        const userData = JSON.parse(userStr);
        const decodedToken = decodeToken(token);

        if (decodedToken) {
          setUser(userData);
          setRoles(decodedToken.roles);
        } else {
          localStorage.removeItem('auth_token');
          localStorage.removeItem('user');
        }
      } catch (error) {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('user');
      }
    }

    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginRequest) => {
    const response = await api.login(credentials);
    const decodedToken = decodeToken(response.token);

    localStorage.setItem('auth_token', response.token);
    localStorage.setItem('user', JSON.stringify(response));
    setUser(response);

    if (decodedToken) {
      setRoles(decodedToken.roles);
    }
  };

  const logout = () => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');
    setUser(null);
    setRoles([]);
  };

  const hasRole = (role: string): boolean => {
    return roles.some(r => r.toLowerCase() === role.toLowerCase());
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        roles,
        hasRole,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
