"use client";

import Link from "next/link";
import { useAuth } from "@/contexts/auth-context";
import { useCart } from "@/hooks/use-cart";
import { Button } from "@/components/ui/button";
import { ShoppingCart, User, LogOut, Package } from "lucide-react";

export function Navigation() {
  const { user, isAuthenticated, logout } = useAuth();
  const { itemCount } = useCart();

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container flex h-16 items-center justify-between">
        <div className="flex gap-6 md:gap-10">
          <Link href="/" className="flex items-center space-x-2">
            <Package className="h-6 w-6" />
            <span className="font-bold text-xl">SharpBuy</span>
          </Link>
          <nav className="flex gap-6">
            <Link
              href="/products"
              className="flex items-center text-sm font-medium text-muted-foreground hover:text-foreground"
            >
              Products
            </Link>
          </nav>
        </div>

        <div className="flex items-center gap-4">
          <Link href="/cart" className="relative">
            <Button variant="ghost" size="icon">
              <ShoppingCart className="h-5 w-5" />
              {itemCount > 0 && (
                <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-primary text-xs text-primary-foreground flex items-center justify-center">
                  {itemCount}
                </span>
              )}
            </Button>
          </Link>

          {isAuthenticated ? (
            <>
              <Link href="/profile">
                <Button variant="ghost" size="icon">
                  <User className="h-5 w-5" />
                </Button>
              </Link>
              <Button variant="ghost" size="sm" onClick={logout}>
                <LogOut className="h-4 w-4 mr-2" />
                Logout
              </Button>
              <span className="text-sm text-muted-foreground">
                {user?.firstName}
              </span>
            </>
          ) : (
            <>
              <Link href="/login">
                <Button variant="ghost">Login</Button>
              </Link>
              <Link href="/register">
                <Button>Sign Up</Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
