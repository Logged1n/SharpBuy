'use client';

import Link from 'next/link';
import { useEffect, useState } from 'react';
import { useAuth } from '@/lib/auth';
import { useCart } from '@/lib/cart-context';
import { Button } from './ui/button';
import { ThemeToggle } from './theme-toggle';
import { MobileMenu } from './mobile-menu';
import { ShoppingCart, User } from 'lucide-react';
import { api } from '@/lib/api';

export function Navbar() {
  const { isAuthenticated, logout } = useAuth();
  const { itemCount } = useCart();
  const [hasAdminAccess, setHasAdminAccess] = useState(false);

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

  return (
    <nav className="sticky top-0 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60 z-50">
      <div className="flex h-16 items-center justify-between w-full px-4 md:px-8">
        {/* Left side: Logo and Desktop Navigation */}
        <div className="flex items-center gap-8">
          <Link href="/" className="flex items-center space-x-2">
            <ShoppingCart className="h-6 w-6" />
            <span className="text-xl font-bold">SharpBuy</span>
          </Link>
          <div className="hidden md:flex items-center gap-6">
            <Link
              href="/products"
              className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
            >
              Products
            </Link>
          </div>
        </div>

        {/* Right side: Cart, Theme Toggle, Auth Buttons, Mobile Menu */}
        <div className="flex items-center gap-2 sm:gap-3">
          {/* Cart Icon with Badge */}
          <Link href="/cart">
            <Button variant="ghost" size="icon" className="relative">
              <ShoppingCart className="h-5 w-5" />
              {itemCount > 0 && (
                <span className="absolute -top-1 -right-1 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-primary-foreground">
                  {itemCount > 99 ? '99+' : itemCount}
                </span>
              )}
            </Button>
          </Link>

          {/* Theme Toggle - visible on all screen sizes */}
          <ThemeToggle />

          {/* Desktop Auth Buttons */}
          <div className="hidden md:flex items-center gap-2">
            {isAuthenticated ? (
              <>
                <Link href="/profile">
                  <Button variant="ghost" size="sm">
                    <User className="h-4 w-4 mr-2" />
                    Profile
                  </Button>
                </Link>
                {hasAdminAccess && (
                  <Link href="/admin">
                    <Button variant="ghost" size="sm">
                      Admin
                    </Button>
                  </Link>
                )}
                <Button variant="outline" size="sm" onClick={logout}>
                  Logout
                </Button>
              </>
            ) : (
              <>
                <Link href="/login">
                  <Button variant="ghost" size="sm">
                    Login
                  </Button>
                </Link>
                <Link href="/register">
                  <Button size="sm">
                    Sign Up
                  </Button>
                </Link>
              </>
            )}
          </div>

          {/* Mobile Hamburger Menu */}
          <MobileMenu />
        </div>
      </div>
    </nav>
  );
}
