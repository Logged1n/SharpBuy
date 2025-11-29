'use client';

import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api, Cart, CartItem, AddItemToCartRequest } from './api';
import { useAuth } from './auth';

interface CartContextType {
  cart: Cart | null;
  itemCount: number;
  isLoading: boolean;
  addItem: (productId: string, quantity: number) => Promise<void>;
  updateQuantity: (productId: string, quantity: number) => Promise<void>;
  removeItem: (productId: string) => Promise<void>;
  clearCart: () => Promise<void>;
  refreshCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

const CART_STORAGE_KEY = 'anonymous_cart';

interface LocalCartItem {
  productId: string;
  quantity: number;
}

export function CartProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [cart, setCart] = useState<Cart | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Get anonymous cart from localStorage
  const getLocalCart = useCallback((): LocalCartItem[] => {
    if (typeof window === 'undefined') return [];
    const stored = localStorage.getItem(CART_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  }, []);

  // Save anonymous cart to localStorage
  const saveLocalCart = useCallback((items: LocalCartItem[]) => {
    if (typeof window === 'undefined') return;
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
  }, []);

  // Clear anonymous cart from localStorage
  const clearLocalCart = useCallback(() => {
    if (typeof window === 'undefined') return;
    localStorage.removeItem(CART_STORAGE_KEY);
  }, []);

  // Load cart data
  const loadCart = useCallback(async () => {
    if (authLoading) return;

    setIsLoading(true);
    try {
      if (isAuthenticated) {
        // Load cart from API for authenticated users
        const serverCart = await api.getCart();
        setCart(serverCart);

        // Merge anonymous cart if exists
        const localItems = getLocalCart();
        if (localItems.length > 0) {
          // Add local cart items to server cart
          for (const item of localItems) {
            try {
              await api.addItemToCart({
                productId: item.productId,
                quantity: item.quantity,
              });
            } catch (error) {
              console.error('Failed to merge cart item:', error);
            }
          }
          // Clear local cart after merging
          clearLocalCart();
          // Reload cart to get merged data
          const updatedCart = await api.getCart();
          setCart(updatedCart);
        }
      } else {
        // For anonymous users, create a mock cart from localStorage
        const localItems = getLocalCart();
        setCart({
          id: '',
          ownerId: '',
          items: [],
          totalAmount: 0,
          currency: 'USD',
        });
      }
    } catch (error) {
      console.error('Failed to load cart:', error);
      setCart(null);
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated, authLoading, getLocalCart, clearLocalCart]);

  // Load cart on mount and when auth status changes
  useEffect(() => {
    loadCart();
  }, [loadCart]);

  const addItem = async (productId: string, quantity: number) => {
    try {
      if (isAuthenticated) {
        // Add to server cart
        await api.addItemToCart({ productId, quantity });
        await loadCart();
      } else {
        // Add to local cart
        const localItems = getLocalCart();
        const existingIndex = localItems.findIndex(item => item.productId === productId);

        if (existingIndex >= 0) {
          localItems[existingIndex].quantity += quantity;
        } else {
          localItems.push({ productId, quantity });
        }

        saveLocalCart(localItems);
        await loadCart();
      }
    } catch (error) {
      console.error('Failed to add item to cart:', error);
      throw error;
    }
  };

  const updateQuantity = async (productId: string, quantity: number) => {
    try {
      if (isAuthenticated) {
        // Update on server
        await api.updateCartItemQuantity(productId, { quantity });
        await loadCart();
      } else {
        // Update in local cart
        const localItems = getLocalCart();
        const existingIndex = localItems.findIndex(item => item.productId === productId);

        if (existingIndex >= 0) {
          if (quantity > 0) {
            localItems[existingIndex].quantity = quantity;
          } else {
            localItems.splice(existingIndex, 1);
          }
          saveLocalCart(localItems);
          await loadCart();
        }
      }
    } catch (error) {
      console.error('Failed to update cart item:', error);
      throw error;
    }
  };

  const removeItem = async (productId: string) => {
    try {
      if (isAuthenticated) {
        // Remove from server
        await api.removeCartItem(productId);
        await loadCart();
      } else {
        // Remove from local cart
        const localItems = getLocalCart();
        const filtered = localItems.filter(item => item.productId !== productId);
        saveLocalCart(filtered);
        await loadCart();
      }
    } catch (error) {
      console.error('Failed to remove cart item:', error);
      throw error;
    }
  };

  const clearCart = async () => {
    try {
      if (isAuthenticated) {
        // Clear on server
        await api.clearCart();
        setCart(null);
      } else {
        // Clear local cart
        clearLocalCart();
        setCart({
          id: '',
          ownerId: '',
          items: [],
          totalAmount: 0,
          currency: 'USD',
        });
      }
    } catch (error) {
      console.error('Failed to clear cart:', error);
      throw error;
    }
  };

  const refreshCart = async () => {
    await loadCart();
  };

  const itemCount = cart?.items.reduce((total, item) => total + item.quantity, 0) || 0;

  return (
    <CartContext.Provider
      value={{
        cart,
        itemCount,
        isLoading,
        addItem,
        updateQuantity,
        removeItem,
        clearCart,
        refreshCart,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
}
