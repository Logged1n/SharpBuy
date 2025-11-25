"use client";

import { useState, useEffect } from "react";
import type { CartItem } from "@/types";

interface CartState {
  items: CartItem[];
  totalAmount: number;
}

export function useCart() {
  const [cart, setCart] = useState<CartState>({ items: [], totalAmount: 0 });

  useEffect(() => {
    // Load cart from localStorage on mount
    const storedCart = localStorage.getItem("cart");
    if (storedCart) {
      setCart(JSON.parse(storedCart));
    }
  }, []);

  const addToCart = (item: Omit<CartItem, "id">) => {
    setCart((prevCart) => {
      const existingItemIndex = prevCart.items.findIndex(
        (i) => i.productId === item.productId
      );

      let newItems: CartItem[];

      if (existingItemIndex > -1) {
        // Update quantity if item exists
        newItems = prevCart.items.map((i, index) =>
          index === existingItemIndex
            ? { ...i, quantity: i.quantity + item.quantity }
            : i
        );
      } else {
        // Add new item
        newItems = [
          ...prevCart.items,
          {
            ...item,
            id: Math.random().toString(36).substr(2, 9),
          },
        ];
      }

      const totalAmount = newItems.reduce(
        (sum, item) => sum + item.priceAmount * item.quantity,
        0
      );

      const newCart = { items: newItems, totalAmount };
      localStorage.setItem("cart", JSON.stringify(newCart));
      return newCart;
    });
  };

  const removeFromCart = (itemId: string) => {
    setCart((prevCart) => {
      const newItems = prevCart.items.filter((item) => item.id !== itemId);
      const totalAmount = newItems.reduce(
        (sum, item) => sum + item.priceAmount * item.quantity,
        0
      );

      const newCart = { items: newItems, totalAmount };
      localStorage.setItem("cart", JSON.stringify(newCart));
      return newCart;
    });
  };

  const updateQuantity = (itemId: string, quantity: number) => {
    if (quantity <= 0) {
      removeFromCart(itemId);
      return;
    }

    setCart((prevCart) => {
      const newItems = prevCart.items.map((item) =>
        item.id === itemId ? { ...item, quantity } : item
      );

      const totalAmount = newItems.reduce(
        (sum, item) => sum + item.priceAmount * item.quantity,
        0
      );

      const newCart = { items: newItems, totalAmount };
      localStorage.setItem("cart", JSON.stringify(newCart));
      return newCart;
    });
  };

  const clearCart = () => {
    const emptyCart = { items: [], totalAmount: 0 };
    setCart(emptyCart);
    localStorage.setItem("cart", JSON.stringify(emptyCart));
  };

  return {
    items: cart.items,
    totalAmount: cart.totalAmount,
    addToCart,
    removeFromCart,
    updateQuantity,
    clearCart,
    itemCount: cart.items.reduce((sum, item) => sum + item.quantity, 0),
  };
}
