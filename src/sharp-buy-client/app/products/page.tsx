import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Package } from 'lucide-react';

export default function ProductsPage() {
  return (
    <div className="container max-w-7xl mx-auto py-8 px-4 md:px-6">
      <div className="space-y-8">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Products</h1>
          <p className="text-muted-foreground mt-2">
            Browse our collection of amazing products
          </p>
        </div>

        <div className="text-center py-20">
          <Card className="max-w-md mx-auto">
            <CardHeader>
              <div className="flex justify-center mb-4">
                <Package className="h-16 w-16 text-muted-foreground/50" />
              </div>
              <CardTitle>No Products Available</CardTitle>
              <CardDescription className="mt-2">
                Products will appear here once they are added to the catalog.
                Check back soon!
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">
                Admin users can add products from the{' '}
                <a href="/admin" className="text-primary hover:underline">
                  admin dashboard
                </a>
                .
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
