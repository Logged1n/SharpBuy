'use client';

import Link from 'next/link';
import { useAuth } from '@/lib/auth';
import { Button } from './ui/button';
import { ThemeToggle } from './theme-toggle';
import { MobileMenu } from './mobile-menu';
import { ShoppingCart } from 'lucide-react';

export function Navbar() {
  const { isAuthenticated, logout } = useAuth();

  return (
    <nav className="sticky top-0 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
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
            <Link
              href="/categories"
              className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
            >
              Categories
            </Link>
          </div>
        </div>

        {/* Right side: Theme Toggle, Auth Buttons, Mobile Menu */}
        <div className="flex items-center gap-2 sm:gap-3">
          {/* Theme Toggle - visible on all screen sizes */}
          <ThemeToggle />

          {/* Desktop Auth Buttons */}
          <div className="hidden md:flex items-center gap-2">
            {isAuthenticated ? (
              <>
                <Link href="/admin">
                  <Button variant="ghost" size="sm">
                    Admin
                  </Button>
                </Link>
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
