import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { FolderOpen } from 'lucide-react';

export default function CategoriesPage() {
  return (
    <div className="container max-w-7xl mx-auto py-8 px-4 md:px-6">
      <div className="space-y-8">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Categories</h1>
          <p className="text-muted-foreground mt-2">
            Explore products by category
          </p>
        </div>

        <div className="text-center py-20">
          <Card className="max-w-md mx-auto">
            <CardHeader>
              <div className="flex justify-center mb-4">
                <FolderOpen className="h-16 w-16 text-muted-foreground/50" />
              </div>
              <CardTitle>No Categories Available</CardTitle>
              <CardDescription className="mt-2">
                Categories will appear here once they are created.
                Check back soon!
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">
                Browse our full catalog on the{' '}
                <a href="/products" className="text-primary hover:underline">
                  products page
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
