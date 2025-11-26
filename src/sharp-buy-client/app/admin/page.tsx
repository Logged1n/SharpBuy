'use client';

import { useState } from 'react';
import { ProtectedRoute } from '@/components/protected-route';
import { useAuth } from '@/lib/auth';
import { api, ApiError, AddProductRequest } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Package, Plus, AlertCircle, CheckCircle } from 'lucide-react';

function AdminContent() {
  const { user } = useAuth();
  const [showAddProduct, setShowAddProduct] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [productData, setProductData] = useState<AddProductRequest>({
    name: '',
    description: '',
    priceAmount: 0,
    priceCurrency: 'USD',
    stockQuantity: 0,
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setProductData({
      ...productData,
      [name]: name === 'priceAmount' || name === 'stockQuantity' ? parseFloat(value) || 0 : value,
    });
  };

  const handleAddProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setIsLoading(true);

    try {
      await api.addProduct(productData);
      setSuccess('Product added successfully!');
      setProductData({
        name: '',
        description: '',
        priceAmount: 0,
        priceCurrency: 'USD',
        stockQuantity: 0,
      });
      setShowAddProduct(false);
    } catch (err) {
      const apiError = err as ApiError;
      if (apiError.errors) {
        const errorMessages = Object.values(apiError.errors).flat();
        setError(errorMessages.join(', '));
      } else {
        setError(apiError.detail || apiError.title || 'Failed to add product. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container max-w-7xl mx-auto py-8 px-4 md:px-6">
      <div className="space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Admin Dashboard</h1>
          <p className="text-muted-foreground mt-2">
            Welcome back, {user?.email}
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Products</CardTitle>
              <Package className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">0</div>
              <p className="text-xs text-muted-foreground">No products yet</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Orders</CardTitle>
              <Package className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">0</div>
              <p className="text-xs text-muted-foreground">No orders yet</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Revenue</CardTitle>
              <Package className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">$0.00</div>
              <p className="text-xs text-muted-foreground">No sales yet</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Customers</CardTitle>
              <Package className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">1</div>
              <p className="text-xs text-muted-foreground">You!</p>
            </CardContent>
          </Card>
        </div>

        {/* Product Management Section */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Product Management</CardTitle>
                <CardDescription>Add and manage your product catalog</CardDescription>
              </div>
              <Button onClick={() => setShowAddProduct(!showAddProduct)}>
                <Plus className="h-4 w-4 mr-2" />
                Add Product
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {success && (
              <div className="flex items-center gap-2 p-3 mb-4 text-sm text-green-600 bg-green-50 dark:bg-green-950 rounded-md">
                <CheckCircle className="h-4 w-4" />
                <span>{success}</span>
              </div>
            )}

            {showAddProduct && (
              <form onSubmit={handleAddProduct} className="space-y-4 p-4 border rounded-lg">
                <h3 className="text-lg font-semibold">Add New Product</h3>

                {error && (
                  <div className="flex items-center gap-2 p-3 text-sm text-destructive bg-destructive/10 rounded-md">
                    <AlertCircle className="h-4 w-4" />
                    <span>{error}</span>
                  </div>
                )}

                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="name">Product Name</Label>
                    <Input
                      id="name"
                      name="name"
                      value={productData.name}
                      onChange={handleInputChange}
                      placeholder="Enter product name"
                      required
                      disabled={isLoading}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="priceCurrency">Currency</Label>
                    <Input
                      id="priceCurrency"
                      name="priceCurrency"
                      value={productData.priceCurrency}
                      onChange={handleInputChange}
                      placeholder="USD"
                      maxLength={3}
                      required
                      disabled={isLoading}
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="description">Description</Label>
                  <textarea
                    id="description"
                    name="description"
                    value={productData.description}
                    onChange={handleInputChange}
                    placeholder="Enter product description"
                    required
                    disabled={isLoading}
                    className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  />
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="priceAmount">Price</Label>
                    <Input
                      id="priceAmount"
                      name="priceAmount"
                      type="number"
                      step="0.01"
                      min="0"
                      value={productData.priceAmount}
                      onChange={handleInputChange}
                      placeholder="0.00"
                      required
                      disabled={isLoading}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="stockQuantity">Stock Quantity</Label>
                    <Input
                      id="stockQuantity"
                      name="stockQuantity"
                      type="number"
                      min="0"
                      value={productData.stockQuantity}
                      onChange={handleInputChange}
                      placeholder="0"
                      required
                      disabled={isLoading}
                    />
                  </div>
                </div>

                <div className="flex gap-2 justify-end">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setShowAddProduct(false)}
                    disabled={isLoading}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" disabled={isLoading}>
                    {isLoading ? 'Adding...' : 'Add Product'}
                  </Button>
                </div>
              </form>
            )}

            {!showAddProduct && (
              <div className="text-center py-12 text-muted-foreground">
                <Package className="h-12 w-12 mx-auto mb-4 opacity-50" />
                <p>No products yet. Click &quot;Add Product&quot; to get started.</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function AdminPage() {
  return (
    <ProtectedRoute>
      <AdminContent />
    </ProtectedRoute>
  );
}
