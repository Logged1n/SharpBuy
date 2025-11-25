"use client";

import React, { createContext, useContext, useState, useEffect } from "react";
import { apiClient } from "@/lib/api-client";
import type { User, LoginRequest, RegisterRequest } from "@/types";

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Load token from localStorage on mount
    const storedToken = localStorage.getItem("token");
    const storedUser = localStorage.getItem("user");

    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  const login = async (data: LoginRequest) => {
    const response = await apiClient.login(data);
    const userData: User = {
      id: response.id,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      emailVerified: true, // Assuming verified after login
    };

    setToken(response.token);
    setUser(userData);
    localStorage.setItem("token", response.token);
    localStorage.setItem("user", JSON.stringify(userData));
  };

  const register = async (data: RegisterRequest) => {
    await apiClient.register(data);
    // After registration, user needs to verify email
    // You might want to show a message to check email
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        login,
        register,
        logout,
        isAuthenticated: !!token && !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
