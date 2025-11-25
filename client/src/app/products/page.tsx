"use client";

import { useEffect, useState } from "react";
import { apiClient } from "@/lib/api-client";
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
import { ShoppingCart } from "lucide-react";
import Link from "next/link";
import type { Product } from "@/types";

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { addToCart } = useCart();

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await apiClient.getProducts();
        setProducts(data);
      } catch (err) {
        setError("Failed to load products");
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchProducts();
  }, []);

  const handleAddToCart = (product: Product) => {
    addToCart({
      productId: product.id,
      productName: product.name,
      quantity: 1,
      priceAmount: product.priceAmount,
      priceCurrency: product.priceCurrency,
    });
  };

  if (isLoading) {
    return (
      <div className="container py-10">
        <div className="text-center">Loading products...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container py-10">
        <div className="text-center text-destructive">{error}</div>
      </div>
    );
  }

  return (
    <div className="container py-10">
      <div className="flex flex-col space-y-4 mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Products</h1>
        <p className="text-muted-foreground">
          Browse our selection of quality products
        </p>
      </div>

      {products.length === 0 ? (
        <div className="text-center text-muted-foreground py-10">
          No products available at the moment.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {products.map((product) => (
            <Card key={product.id} className="flex flex-col">
              <CardHeader>
                <CardTitle className="line-clamp-1">{product.name}</CardTitle>
                <CardDescription className="line-clamp-2">
                  {product.description}
                </CardDescription>
              </CardHeader>
              <CardContent className="flex-1">
                <div className="flex items-baseline gap-2">
                  <span className="text-2xl font-bold">
                    {product.priceAmount.toFixed(2)}
                  </span>
                  <span className="text-muted-foreground">
                    {product.priceCurrency}
                  </span>
                </div>
                <p className="text-sm text-muted-foreground mt-2">
                  {product.stockQuantity > 0
                    ? `${product.stockQuantity} in stock`
                    : "Out of stock"}
                </p>
              </CardContent>
              <CardFooter className="flex gap-2">
                <Link href={`/products/${product.id}`} className="flex-1">
                  <Button variant="outline" className="w-full">
                    View Details
                  </Button>
                </Link>
                <Button
                  onClick={() => handleAddToCart(product)}
                  disabled={product.stockQuantity === 0}
                  className="flex-1"
                >
                  <ShoppingCart className="h-4 w-4 mr-2" />
                  Add to Cart
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
