import Link from 'next/link';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import type { Product } from '@/types';
import { ShoppingCart } from 'lucide-react';

interface ProductCardProps {
  product: Product;
  onAddToCart?: (productId: string) => void;
}

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
  return (
    <Card className="flex h-full flex-col">
      <CardHeader>
        <CardTitle className="line-clamp-1">{product.name}</CardTitle>
        <CardDescription className="line-clamp-2">
          {product.description}
        </CardDescription>
      </CardHeader>
      <CardContent className="flex-1">
        <div className="mb-4 aspect-square w-full rounded-md bg-muted" />
        <div className="space-y-2">
          <div className="flex items-baseline gap-2">
            <span className="text-2xl font-bold">
              {product.currency} {product.price.toFixed(2)}
            </span>
          </div>
          <div className="text-sm text-muted-foreground">
            {product.stockQuantity > 0 ? (
              <span className="text-green-600">
                {product.stockQuantity} in stock
              </span>
            ) : (
              <span className="text-red-600">Out of stock</span>
            )}
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex gap-2">
        <Button asChild className="flex-1">
          <Link href={`/products/${product.id}`}>View Details</Link>
        </Button>
        {onAddToCart && product.stockQuantity > 0 && (
          <Button
            variant="outline"
            size="icon"
            onClick={() => onAddToCart(product.id)}
          >
            <ShoppingCart className="h-4 w-4" />
          </Button>
        )}
      </CardFooter>
    </Card>
  );
}
