'use client';

import { useEffect, useState } from 'react';
import { Navigation } from '@/components/navigation';
import { ProductCard } from '@/components/products/product-card';
import { apiClient } from '@/lib/api-client';
import type { Product, ApiError } from '@/types';
import { Button } from '@/components/ui/button';

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await apiClient.getProducts();
      setProducts(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || 'Failed to load products');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddToCart = async (productId: string) => {
    try {
      await apiClient.addToCart(productId, 1);
      // Show success message or update cart count
    } catch (err) {
      const apiError = err as ApiError;
      alert(apiError.detail || 'Failed to add to cart');
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main className="container mx-auto px-4 py-8">
        <div className="mb-8 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Products</h1>
            <p className="text-muted-foreground">
              Browse our collection of products
            </p>
          </div>
          <Button onClick={loadProducts} variant="outline">
            Refresh
          </Button>
        </div>

        {error && (
          <div className="mb-6 rounded-md bg-destructive/10 p-4 text-destructive">
            {error}
          </div>
        )}

        {isLoading ? (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {[...Array(8)].map((_, i) => (
              <div
                key={i}
                className="h-96 animate-pulse rounded-lg bg-muted"
              />
            ))}
          </div>
        ) : products.length === 0 ? (
          <div className="rounded-lg border border-dashed p-12 text-center">
            <p className="text-lg text-muted-foreground">
              No products available at the moment.
            </p>
          </div>
        ) : (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {products.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                onAddToCart={handleAddToCart}
              />
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
