'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { Menu, X, User } from 'lucide-react';
import { Button } from './ui/button';
import { useAuth } from '@/lib/auth';
import { api } from '@/lib/api';

export function MobileMenu() {
  const [isOpen, setIsOpen] = useState(false);
  const { isAuthenticated, logout } = useAuth();
  const [hasAdminAccess, setHasAdminAccess] = useState(false);

  // Close menu when route changes
  useEffect(() => {
    setIsOpen(false);
  }, []);

  // Check admin access
  useEffect(() => {
    const checkAdminAccess = async () => {
      if (!isAuthenticated) {
        setHasAdminAccess(false);
        return;
      }

      try {
        const permissions = await api.getPermissions();
        const isAdmin = permissions.some(p =>
          p === 'products:write' || p === 'categories:write' || p === 'orders:write'
        );
        setHasAdminAccess(isAdmin);
      } catch (error) {
        setHasAdminAccess(false);
      }
    };

    checkAdminAccess();
  }, [isAuthenticated]);

  // Prevent body scroll when menu is open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  return (
    <>
      {/* Hamburger Button */}
      <Button
        variant="ghost"
        size="icon"
        className="md:hidden"
        onClick={() => setIsOpen(!isOpen)}
        aria-label="Toggle menu"
      >
        {isOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
      </Button>

      {/* Mobile Menu Overlay */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 md:hidden"
          onClick={() => setIsOpen(false)}
        />
      )}

      {/* Mobile Menu Panel */}
      <div
        className={`fixed top-16 right-0 bottom-0 w-64 bg-background border-l border-border z-50 transform transition-transform duration-300 ease-in-out md:hidden ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <nav className="flex flex-col p-4 space-y-2">
          {/* Navigation Links */}
          <Link
            href="/products"
            className="px-4 py-3 text-sm font-medium hover:bg-muted rounded-md transition-colors"
            onClick={() => setIsOpen(false)}
          >
            Products
          </Link>
          {/* Divider */}
          <div className="border-t border-border my-2" />

          {/* Auth Links */}
          {isAuthenticated ? (
            <>
            {!hasAdminAccess && (
              <Link
                href="/profile"
                className="px-4 py-3 text-sm font-medium hover:bg-muted rounded-md transition-colors flex items-center"
                onClick={() => setIsOpen(false)}
              >
                <User className="h-4 w-4 mr-2" />
                Profile
              </Link>
            )}
              {hasAdminAccess && (
                <Link
                  href="/admin"
                  className="px-4 py-3 text-sm font-medium hover:bg-muted rounded-md transition-colors"
                  onClick={() => setIsOpen(false)}
                >
                  Admin
                </Link>
              )}
              <button
                className="px-4 py-3 text-sm font-medium hover:bg-muted rounded-md transition-colors text-left"
                onClick={() => {
                  logout();
                  setIsOpen(false);
                }}
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <Link
                href="/login"
                className="px-4 py-3 text-sm font-medium hover:bg-muted rounded-md transition-colors"
                onClick={() => setIsOpen(false)}
              >
                Login
              </Link>
              <Link
                href="/register"
                className="px-4 py-3 text-sm font-medium hover:bg-muted rounded-md transition-colors"
                onClick={() => setIsOpen(false)}
              >
                Sign Up
              </Link>
            </>
          )}
        </nav>
      </div>
    </>
  );
}
