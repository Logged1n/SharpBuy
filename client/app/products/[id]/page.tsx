'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Navigation } from '@/components/navigation';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { apiClient } from '@/lib/api-client';
import type { Product, ApiError } from '@/types';
import { ShoppingCart, ArrowLeft } from 'lucide-react';

export default function ProductDetailPage() {
  const params = useParams();
  const router = useRouter();
  const productId = params.id as string;

  const [product, setProduct] = useState<Product | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [quantity, setQuantity] = useState(1);

  useEffect(() => {
    loadProduct();
  }, [productId]);

  const loadProduct = async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await apiClient.getProductById(productId);
      setProduct(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || 'Failed to load product');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddToCart = async () => {
    try {
      await apiClient.addToCart(productId, quantity);
      alert('Product added to cart!');
    } catch (err) {
      const apiError = err as ApiError;
      alert(apiError.detail || 'Failed to add to cart');
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background">
        <Navigation />
        <main className="container mx-auto px-4 py-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 w-64 rounded bg-muted" />
            <div className="grid gap-8 md:grid-cols-2">
              <div className="aspect-square rounded-lg bg-muted" />
              <div className="space-y-4">
                <div className="h-8 w-3/4 rounded bg-muted" />
                <div className="h-4 w-full rounded bg-muted" />
                <div className="h-4 w-full rounded bg-muted" />
                <div className="h-4 w-2/3 rounded bg-muted" />
              </div>
            </div>
          </div>
        </main>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="min-h-screen bg-background">
        <Navigation />
        <main className="container mx-auto px-4 py-8">
          <div className="rounded-lg border border-destructive bg-destructive/10 p-8 text-center">
            <p className="text-lg text-destructive">
              {error || 'Product not found'}
            </p>
            <Button onClick={() => router.push('/products')} className="mt-4">
              Back to Products
            </Button>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main className="container mx-auto px-4 py-8">
        <Button
          variant="ghost"
          onClick={() => router.back()}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>

        <div className="grid gap-8 md:grid-cols-2">
          <div className="aspect-square overflow-hidden rounded-lg bg-muted" />

          <div className="space-y-6">
            <div>
              <h1 className="text-3xl font-bold">{product.name}</h1>
              <div className="mt-2 flex items-baseline gap-2">
                <span className="text-3xl font-bold">
                  {product.currency} {product.price.toFixed(2)}
                </span>
              </div>
            </div>

            <div>
              <h2 className="mb-2 text-lg font-semibold">Description</h2>
              <p className="text-muted-foreground">{product.description}</p>
            </div>

            <div>
              <h2 className="mb-2 text-lg font-semibold">Availability</h2>
              {product.stockQuantity > 0 ? (
                <p className="text-green-600">
                  {product.stockQuantity} units in stock
                </p>
              ) : (
                <p className="text-red-600">Out of stock</p>
              )}
            </div>

            {product.categories && product.categories.length > 0 && (
              <div>
                <h2 className="mb-2 text-lg font-semibold">Categories</h2>
                <div className="flex flex-wrap gap-2">
                  {product.categories.map((category) => (
                    <span
                      key={category.id}
                      className="rounded-full bg-secondary px-3 py-1 text-sm"
                    >
                      {category.name}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {product.stockQuantity > 0 && (
              <Card>
                <CardHeader>
                  <CardTitle>Add to Cart</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex items-center gap-4">
                    <label htmlFor="quantity" className="text-sm font-medium">
                      Quantity:
                    </label>
                    <input
                      id="quantity"
                      type="number"
                      min="1"
                      max={product.stockQuantity}
                      value={quantity}
                      onChange={(e) =>
                        setQuantity(Math.max(1, parseInt(e.target.value) || 1))
                      }
                      className="w-20 rounded-md border border-input bg-background px-3 py-2"
                    />
                  </div>
                  <Button onClick={handleAddToCart} className="w-full">
                    <ShoppingCart className="mr-2 h-4 w-4" />
                    Add to Cart
                  </Button>
                </CardContent>
              </Card>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}
