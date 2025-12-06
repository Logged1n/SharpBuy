'use client';

import { useCart } from '@/lib/cart-context';
import { Button } from '@/components/ui/button';
import { Minus, Plus, Trash2, ShoppingBag } from 'lucide-react';
import Link from 'next/link';
import { useState } from 'react';

export default function CartPage() {
  const { cart, isLoading, updateQuantity, removeItem, clearCart } = useCart();
  const [loadingItems, setLoadingItems] = useState<Set<string>>(new Set());

  const handleUpdateQuantity = async (productId: string, newQuantity: number) => {
    if (newQuantity < 1) return;

    setLoadingItems(prev => new Set(prev).add(productId));
    try {
      await updateQuantity(productId, newQuantity);
    } catch (error) {
      console.error('Failed to update quantity:', error);
    } finally {
      setLoadingItems(prev => {
        const next = new Set(prev);
        next.delete(productId);
        return next;
      });
    }
  };

  const handleRemoveItem = async (productId: string) => {
    setLoadingItems(prev => new Set(prev).add(productId));
    try {
      await removeItem(productId);
    } catch (error) {
      console.error('Failed to remove item:', error);
    } finally {
      setLoadingItems(prev => {
        const next = new Set(prev);
        next.delete(productId);
        return next;
      });
    }
  };

  const handleClearCart = async () => {
    if (!confirm('Are you sure you want to clear your cart?')) return;

    try {
      await clearCart();
    } catch (error) {
      console.error('Failed to clear cart:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 bg-muted rounded"></div>
          <div className="h-32 bg-muted rounded"></div>
          <div className="h-32 bg-muted rounded"></div>
        </div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="container mx-auto px-4 py-16">
        <div className="max-w-md mx-auto text-center">
          <ShoppingBag className="h-24 w-24 mx-auto text-muted-foreground mb-4" />
          <h1 className="text-2xl font-bold mb-2">Your cart is empty</h1>
          <p className="text-muted-foreground mb-6">
            Start shopping to add items to your cart
          </p>
          <Link href="/products">
            <Button>Browse Products</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-3xl font-bold">Shopping Cart</h1>
        {cart.items.length > 0 && (
          <Button variant="outline" onClick={handleClearCart} size="sm">
            Clear Cart
          </Button>
        )}
      </div>

      <div className="grid lg:grid-cols-3 gap-8">
        {/* Cart Items */}
        <div className="lg:col-span-2 space-y-4">
          {cart.items.map((item) => {
            const isItemLoading = loadingItems.has(item.productId);

            return (
              <div
                key={item.productId}
                className="flex gap-4 p-4 border rounded-lg bg-card"
              >
                {/* Product Image */}
                <div className="w-24 h-24 bg-muted rounded-md flex-shrink-0 overflow-hidden">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src={`${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001'}${item.mainPhotoPath}`}
                    alt={item.productName}
                    className="w-full h-full object-cover"
                  />
                </div>

                {/* Product Details */}
                <div className="flex-1">
                  <h3 className="font-semibold">{item.productName}</h3>
                  <p className="text-sm text-muted-foreground">
                    {item.unitPrice.toFixed(2)} {item.currency}
                  </p>

                  {/* Quantity Controls */}
                  <div className="flex items-center gap-2 mt-2">
                    <Button
                      variant="outline"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => handleUpdateQuantity(item.productId, item.quantity - 1)}
                      disabled={isItemLoading || item.quantity <= 1}
                    >
                      <Minus className="h-4 w-4" />
                    </Button>
                    <span className="w-12 text-center font-medium">{item.quantity}</span>
                    <Button
                      variant="outline"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => handleUpdateQuantity(item.productId, item.quantity + 1)}
                      disabled={isItemLoading}
                    >
                      <Plus className="h-4 w-4" />
                    </Button>
                  </div>
                </div>

                {/* Price and Remove */}
                <div className="flex flex-col items-end justify-between">
                  <p className="font-bold">
                    {item.totalPrice.toFixed(2)} {item.currency}
                  </p>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-destructive hover:text-destructive"
                    onClick={() => handleRemoveItem(item.productId)}
                    disabled={isItemLoading}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            );
          })}
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <div className="border rounded-lg p-6 bg-card sticky top-20">
            <h2 className="text-xl font-bold mb-4">Order Summary</h2>

            <div className="space-y-2 mb-4">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="font-medium">
                  {cart.totalAmount.toFixed(2)} {cart.currency}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Shipping</span>
                <span className="font-medium">Calculated at checkout</span>
              </div>
            </div>

            <div className="border-t pt-4 mb-6">
              <div className="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span>
                  {cart.totalAmount.toFixed(2)} {cart.currency}
                </span>
              </div>
            </div>

            <Button className="w-full" size="lg">
              Proceed to Checkout
            </Button>

            <Link href="/products" className="block mt-4">
              <Button variant="outline" className="w-full">
                Continue Shopping
              </Button>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
