"use client";

import { useCart } from "@/hooks/use-cart";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Trash2, ShoppingBag } from "lucide-react";
import Link from "next/link";

export default function CartPage() {
  const { items, totalAmount, removeFromCart, updateQuantity, clearCart } =
    useCart();

  if (items.length === 0) {
    return (
      <div className="container py-10">
        <Card className="max-w-md mx-auto">
          <CardHeader className="text-center">
            <div className="mx-auto w-12 h-12 rounded-full bg-muted flex items-center justify-center mb-4">
              <ShoppingBag className="h-6 w-6 text-muted-foreground" />
            </div>
            <CardTitle>Your cart is empty</CardTitle>
            <CardDescription>
              Add some products to your cart to get started
            </CardDescription>
          </CardHeader>
          <CardFooter>
            <Link href="/products" className="w-full">
              <Button className="w-full">Browse Products</Button>
            </Link>
          </CardFooter>
        </Card>
      </div>
    );
  }

  return (
    <div className="container py-10">
      <h1 className="text-3xl font-bold tracking-tight mb-8">Shopping Cart</h1>

      <div className="grid lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-4">
          {items.map((item) => (
            <Card key={item.id}>
              <CardContent className="p-6">
                <div className="flex items-start gap-4">
                  <div className="w-20 h-20 bg-muted rounded-md flex items-center justify-center flex-shrink-0">
                    <ShoppingBag className="h-8 w-8 text-muted-foreground" />
                  </div>

                  <div className="flex-1 min-w-0">
                    <h3 className="font-semibold text-lg mb-1">
                      {item.productName}
                    </h3>
                    <p className="text-muted-foreground mb-3">
                      {item.priceAmount.toFixed(2)} {item.priceCurrency}
                    </p>

                    <div className="flex items-center gap-4">
                      <div className="flex items-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() =>
                            updateQuantity(item.id, item.quantity - 1)
                          }
                        >
                          -
                        </Button>
                        <Input
                          type="number"
                          value={item.quantity}
                          onChange={(e) =>
                            updateQuantity(
                              item.id,
                              parseInt(e.target.value) || 1
                            )
                          }
                          className="w-16 text-center"
                          min="1"
                        />
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() =>
                            updateQuantity(item.id, item.quantity + 1)
                          }
                        >
                          +
                        </Button>
                      </div>

                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => removeFromCart(item.id)}
                      >
                        <Trash2 className="h-4 w-4 mr-2" />
                        Remove
                      </Button>
                    </div>
                  </div>

                  <div className="text-right">
                    <p className="font-semibold text-lg">
                      {(item.priceAmount * item.quantity).toFixed(2)}{" "}
                      {item.priceCurrency}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}

          <Button variant="outline" onClick={clearCart} className="w-full">
            Clear Cart
          </Button>
        </div>

        <div className="lg:col-span-1">
          <Card className="sticky top-20">
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="font-medium">
                  {totalAmount.toFixed(2)}{" "}
                  {items[0]?.priceCurrency || "USD"}
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Shipping</span>
                <span className="font-medium">Calculated at checkout</span>
              </div>
              <div className="border-t pt-4">
                <div className="flex justify-between">
                  <span className="font-semibold">Total</span>
                  <span className="font-bold text-lg">
                    {totalAmount.toFixed(2)}{" "}
                    {items[0]?.priceCurrency || "USD"}
                  </span>
                </div>
              </div>
            </CardContent>
            <CardFooter className="flex flex-col gap-2">
              <Button className="w-full" size="lg">
                Proceed to Checkout
              </Button>
              <Link href="/products" className="w-full">
                <Button variant="outline" className="w-full">
                  Continue Shopping
                </Button>
              </Link>
            </CardFooter>
          </Card>
        </div>
      </div>
    </div>
  );
}
