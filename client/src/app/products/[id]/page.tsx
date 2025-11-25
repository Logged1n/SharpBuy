"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { apiClient } from "@/lib/api-client";
import { useCart } from "@/hooks/use-cart";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, ShoppingCart } from "lucide-react";
import Link from "next/link";
import type { Product } from "@/types";

export default function ProductDetailPage() {
  const params = useParams();
  const router = useRouter();
  const [product, setProduct] = useState<Product | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { addToCart } = useCart();

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        const data = await apiClient.getProduct(params.id as string);
        setProduct(data);
      } catch (err) {
        setError("Failed to load product");
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchProduct();
  }, [params.id]);

  const handleAddToCart = () => {
    if (!product) return;

    addToCart({
      productId: product.id,
      productName: product.name,
      quantity,
      priceAmount: product.priceAmount,
      priceCurrency: product.priceCurrency,
    });

    router.push("/cart");
  };

  if (isLoading) {
    return (
      <div className="container py-10">
        <div className="text-center">Loading product...</div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="container py-10">
        <div className="text-center text-destructive">
          {error || "Product not found"}
        </div>
        <div className="text-center mt-4">
          <Link href="/products">
            <Button variant="outline">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Products
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-10">
      <Link href="/products">
        <Button variant="ghost" className="mb-6">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Products
        </Button>
      </Link>

      <div className="grid md:grid-cols-2 gap-10">
        <div className="aspect-square bg-muted rounded-lg flex items-center justify-center">
          <p className="text-muted-foreground">Product Image</p>
        </div>

        <div className="space-y-6">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              {product.name}
            </h1>
            <p className="text-muted-foreground mt-2">{product.description}</p>
          </div>

          <div className="flex items-baseline gap-2">
            <span className="text-3xl font-bold">
              {product.priceAmount.toFixed(2)}
            </span>
            <span className="text-xl text-muted-foreground">
              {product.priceCurrency}
            </span>
          </div>

          <div>
            <p className="text-sm text-muted-foreground">
              Availability:{" "}
              <span
                className={
                  product.stockQuantity > 0
                    ? "text-green-600 font-medium"
                    : "text-destructive font-medium"
                }
              >
                {product.stockQuantity > 0
                  ? `${product.stockQuantity} in stock`
                  : "Out of stock"}
              </span>
            </p>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Add to Cart</CardTitle>
              <CardDescription>
                Select quantity and add to your cart
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="quantity">Quantity</Label>
                <Input
                  id="quantity"
                  type="number"
                  min="1"
                  max={product.stockQuantity}
                  value={quantity}
                  onChange={(e) =>
                    setQuantity(Math.max(1, parseInt(e.target.value) || 1))
                  }
                  disabled={product.stockQuantity === 0}
                />
              </div>
              <Button
                onClick={handleAddToCart}
                disabled={product.stockQuantity === 0}
                className="w-full"
                size="lg"
              >
                <ShoppingCart className="h-5 w-5 mr-2" />
                Add to Cart
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
