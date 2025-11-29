'use client';

import { useState, useEffect } from 'react';
import { AdminRoute } from '@/components/admin-route';
import { useAuth } from '@/lib/auth';
import { api, ApiError, ProductListItem, CategoryListItem } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Package, Plus, AlertCircle, CheckCircle, Trash2, Edit, X } from 'lucide-react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

function AdminContent() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('products');

  // Products state
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [categories, setCategories] = useState<CategoryListItem[]>([]);
  const [showAddProduct, setShowAddProduct] = useState(false);
  const [showAddCategory, setShowAddCategory] = useState(false);
  const [editingProductId, setEditingProductId] = useState<string | null>(null);
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [productData, setProductData] = useState({
    name: '',
    description: '',
    quantity: 0,
    priceAmount: 0,
    priceCurrency: 'USD',
    categoryIds: [] as string[],
    mainPhotoPath: '/placeholder.jpg',
  });

  const [categoryData, setCategoryData] = useState({
    name: '',
  });

  useEffect(() => {
    loadProducts();
    loadCategories();
  }, []);

  const loadProducts = async () => {
    try {
      const data = await api.getProducts(1, 100);
      setProducts(data.items);
    } catch (err) {
      console.error('Failed to load products:', err);
    }
  };

  const loadCategories = async () => {
    try {
      const data = await api.getCategories(1, 100);
      setCategories(data.items);
    } catch (err) {
      console.error('Failed to load categories:', err);
    }
  };

  const handleProductInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setProductData({
      ...productData,
      [name]: name === 'priceAmount' || name === 'quantity' ? parseFloat(value) || 0 : value,
    });
  };

  const handleCategoryToggle = (categoryId: string) => {
    setProductData(prev => ({
      ...prev,
      categoryIds: prev.categoryIds.includes(categoryId)
        ? prev.categoryIds.filter(id => id !== categoryId)
        : [...prev.categoryIds, categoryId]
    }));
  };

  const resetProductForm = () => {
    setProductData({
      name: '',
      description: '',
      quantity: 0,
      priceAmount: 0,
      priceCurrency: 'USD',
      categoryIds: [],
      mainPhotoPath: '/placeholder.jpg',
    });
    setShowAddProduct(false);
    setEditingProductId(null);
  };

  const handleEditProduct = (product: ProductListItem) => {
    setProductData({
      name: product.name,
      description: product.description,
      quantity: product.stockQuantity,
      priceAmount: product.priceAmount,
      priceCurrency: product.priceCurrency,
      categoryIds: [],
      mainPhotoPath: product.mainPhotoPath,
    });
    setEditingProductId(product.id);
    setShowAddProduct(false);
    setError('');
    setSuccess('');
  };

  const handleSaveProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setIsLoading(true);

    try {
      if (editingProductId) {
        // Update existing product
        await api.updateProduct(editingProductId, {
          name: productData.name,
          description: productData.description,
          priceAmount: productData.priceAmount,
          priceCurrency: productData.priceCurrency,
        });
        setSuccess('Product updated successfully!');
      } else {
        // Add new product
        await api.addProduct({
          name: productData.name,
          description: productData.description,
          quantity: productData.quantity,
          price: {
            amount: productData.priceAmount,
            currency: productData.priceCurrency,
          },
          categoryIds: productData.categoryIds,
          mainPhotoPath: productData.mainPhotoPath,
        });
        setSuccess('Product added successfully!');
      }
      resetProductForm();
      await loadProducts();
    } catch (err) {
      const apiError = err as ApiError;
      if (apiError.errors) {
        const errorMessages = Object.values(apiError.errors).flat();
        setError(errorMessages.join(', '));
      } else {
        setError(apiError.detail || apiError.title || 'Failed to save product. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteProduct = async (id: string) => {
    if (!confirm('Are you sure you want to delete this product?')) return;

    try {
      await api.deleteProduct(id);
      setSuccess('Product deleted successfully!');
      await loadProducts();
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to delete product.');
    }
  };

  const handleCategoryInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setCategoryData({ name: e.target.value });
  };

  const resetCategoryForm = () => {
    setCategoryData({ name: '' });
    setShowAddCategory(false);
    setEditingCategoryId(null);
  };

  const handleEditCategory = (category: CategoryListItem) => {
    setCategoryData({ name: category.name });
    setEditingCategoryId(category.id);
    setShowAddCategory(false);
    setError('');
    setSuccess('');
  };

  const handleSaveCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setIsLoading(true);

    try {
      if (editingCategoryId) {
        // Update existing category
        await api.updateCategory(editingCategoryId, { name: categoryData.name });
        setSuccess('Category updated successfully!');
      } else {
        // Add new category
        await api.addCategory({ name: categoryData.name });
        setSuccess('Category added successfully!');
      }
      resetCategoryForm();
      await loadCategories();
    } catch (err) {
      const apiError = err as ApiError;
      if (apiError.errors) {
        const errorMessages = Object.values(apiError.errors).flat();
        setError(errorMessages.join(', '));
      } else {
        setError(apiError.detail || apiError.title || 'Failed to save category. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteCategory = async (id: string) => {
    if (!confirm('Are you sure you want to delete this category?')) return;

    try {
      await api.deleteCategory(id);
      setSuccess('Category deleted successfully!');
      await loadCategories();
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to delete category.');
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <div className="container max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="space-y-2">
            <h1 className="text-2xl sm:text-3xl font-bold tracking-tight">Admin Dashboard</h1>
            <p className="text-sm sm:text-base text-muted-foreground">
              Welcome back, {user?.token ? 'Admin' : 'Guest'}
            </p>
          </div>

          {/* Stats Cards */}
          <div className="grid gap-3 sm:gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Products</CardTitle>
                <Package className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{products.length}</div>
                <p className="text-xs text-muted-foreground">
                  {products.length === 0 ? 'No products yet' : 'Active products'}
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Categories</CardTitle>
                <Package className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{categories.length}</div>
                <p className="text-xs text-muted-foreground">
                  {categories.length === 0 ? 'No categories yet' : 'Active categories'}
                </p>
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
          </div>

          {/* Success/Error Messages */}
          {success && (
            <div className="flex items-center gap-2 p-3 text-sm text-green-600 bg-green-50 dark:bg-green-950 rounded-md">
              <CheckCircle className="h-4 w-4" />
              <span>{success}</span>
            </div>
          )}

          {error && (
            <div className="flex items-center gap-2 p-3 text-sm text-destructive bg-destructive/10 rounded-md">
              <AlertCircle className="h-4 w-4" />
              <span>{error}</span>
            </div>
          )}

          {/* Tabs for Products and Categories */}
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="products">Products</TabsTrigger>
              <TabsTrigger value="categories">Categories</TabsTrigger>
            </TabsList>

            {/* Products Tab */}
            <TabsContent value="products" className="space-y-4">
              <Card>
                <CardHeader>
                  <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                    <div className="space-y-1">
                      <CardTitle>Product Management</CardTitle>
                      <CardDescription>Add and manage your product catalog</CardDescription>
                    </div>
                    <Button
                      onClick={() => {
                        if (showAddProduct || editingProductId) {
                          // If form is showing, close it
                          resetProductForm();
                        } else {
                          // If form is hidden, show it
                          setShowAddProduct(true);
                          setEditingProductId(null);
                          setProductData({
                            name: '',
                            description: '',
                            quantity: 0,
                            priceAmount: 0,
                            priceCurrency: 'USD',
                            categoryIds: [],
                            mainPhotoPath: '/placeholder.jpg',
                          });
                        }
                        setError('');
                        setSuccess('');
                      }}
                      className="w-full sm:w-auto"
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Product
                    </Button>
                  </div>
                </CardHeader>
                <CardContent>
                  {(showAddProduct || editingProductId) && (
                    <form onSubmit={handleSaveProduct} className="space-y-4 p-3 sm:p-4 border rounded-lg bg-muted/20 mb-6">
                      <div className="flex items-center justify-between">
                        <h3 className="text-base sm:text-lg font-semibold">
                          {editingProductId ? 'Edit Product' : 'Add New Product'}
                        </h3>
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          onClick={resetProductForm}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>

                      <div className="grid gap-3 sm:gap-4 grid-cols-1 sm:grid-cols-2">
                        <div className="space-y-2">
                          <Label htmlFor="name">Product Name</Label>
                          <Input
                            id="name"
                            name="name"
                            value={productData.name}
                            onChange={handleProductInputChange}
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
                            onChange={handleProductInputChange}
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
                          onChange={handleProductInputChange}
                          placeholder="Enter product description"
                          required
                          disabled={isLoading}
                          className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        />
                      </div>

                      <div className="grid gap-3 sm:gap-4 grid-cols-1 sm:grid-cols-2">
                        <div className="space-y-2">
                          <Label htmlFor="priceAmount">Price</Label>
                          <Input
                            id="priceAmount"
                            name="priceAmount"
                            type="number"
                            step="0.01"
                            min="0"
                            value={productData.priceAmount}
                            onChange={handleProductInputChange}
                            placeholder="0.00"
                            required
                            disabled={isLoading}
                          />
                        </div>

                        {!editingProductId && (
                          <div className="space-y-2">
                            <Label htmlFor="quantity">Stock Quantity</Label>
                            <Input
                              id="quantity"
                              name="quantity"
                              type="number"
                              min="0"
                              value={productData.quantity}
                              onChange={handleProductInputChange}
                              placeholder="0"
                              required
                              disabled={isLoading}
                            />
                          </div>
                        )}
                      </div>

                      {!editingProductId && (
                        <div className="space-y-2">
                          <Label>Categories</Label>
                          <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                            {categories.map(category => (
                              <label key={category.id} className="flex items-center gap-2 cursor-pointer">
                                <input
                                  type="checkbox"
                                  checked={productData.categoryIds.includes(category.id)}
                                  onChange={() => handleCategoryToggle(category.id)}
                                  className="rounded border-gray-300"
                                  disabled={isLoading}
                                />
                                <span className="text-sm">{category.name}</span>
                              </label>
                            ))}
                          </div>
                          {categories.length === 0 && (
                            <p className="text-sm text-muted-foreground">No categories available. Create categories first.</p>
                          )}
                        </div>
                      )}

                      <div className="flex flex-col-reverse sm:flex-row gap-2 justify-end pt-2">
                        <Button
                          type="button"
                          variant="outline"
                          onClick={resetProductForm}
                          disabled={isLoading}
                          className="w-full sm:w-auto"
                        >
                          Cancel
                        </Button>
                        <Button type="submit" disabled={isLoading} className="w-full sm:w-auto">
                          {isLoading ? 'Saving...' : editingProductId ? 'Update Product' : 'Add Product'}
                        </Button>
                      </div>
                    </form>
                  )}

                  {products.length === 0 && !showAddProduct && !editingProductId ? (
                    <div className="text-center py-12 text-muted-foreground">
                      <Package className="h-12 w-12 mx-auto mb-4 opacity-50" />
                      <p>No products yet. Click &quot;Add Product&quot; to get started.</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {products.map(product => (
                        <div key={product.id} className="flex items-center justify-between p-4 border rounded-lg bg-card">
                          <div className="flex-1">
                            <h4 className="font-semibold">{product.name}</h4>
                            <p className="text-sm text-muted-foreground line-clamp-1">{product.description}</p>
                            <div className="flex gap-4 mt-2 text-sm">
                              <span className="font-medium">
                                {product.priceAmount.toFixed(2)} {product.priceCurrency}
                              </span>
                              <span className="text-muted-foreground">
                                Stock: {product.stockQuantity}
                              </span>
                            </div>
                          </div>
                          <div className="flex gap-2">
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleEditProduct(product)}
                              className="text-primary hover:text-primary"
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleDeleteProduct(product.id)}
                              className="text-destructive hover:text-destructive"
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            {/* Categories Tab */}
            <TabsContent value="categories" className="space-y-4">
              <Card>
                <CardHeader>
                  <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                    <div className="space-y-1">
                      <CardTitle>Category Management</CardTitle>
                      <CardDescription>Organize your products with categories</CardDescription>
                    </div>
                    <Button
                      onClick={() => {
                        if (showAddCategory || editingCategoryId) {
                          // If form is showing, close it
                          resetCategoryForm();
                        } else {
                          // If form is hidden, show it
                          setShowAddCategory(true);
                          setEditingCategoryId(null);
                          setCategoryData({ name: '' });
                        }
                        setError('');
                        setSuccess('');
                      }}
                      className="w-full sm:w-auto"
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Category
                    </Button>
                  </div>
                </CardHeader>
                <CardContent>
                  {(showAddCategory || editingCategoryId) && (
                    <form onSubmit={handleSaveCategory} className="space-y-4 p-3 sm:p-4 border rounded-lg bg-muted/20 mb-6">
                      <div className="flex items-center justify-between">
                        <h3 className="text-base sm:text-lg font-semibold">
                          {editingCategoryId ? 'Edit Category' : 'Add New Category'}
                        </h3>
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          onClick={resetCategoryForm}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="categoryName">Category Name</Label>
                        <Input
                          id="categoryName"
                          name="name"
                          value={categoryData.name}
                          onChange={handleCategoryInputChange}
                          placeholder="Enter category name"
                          required
                          disabled={isLoading}
                        />
                      </div>

                      <div className="flex flex-col-reverse sm:flex-row gap-2 justify-end pt-2">
                        <Button
                          type="button"
                          variant="outline"
                          onClick={resetCategoryForm}
                          disabled={isLoading}
                          className="w-full sm:w-auto"
                        >
                          Cancel
                        </Button>
                        <Button type="submit" disabled={isLoading} className="w-full sm:w-auto">
                          {isLoading ? 'Saving...' : editingCategoryId ? 'Update Category' : 'Add Category'}
                        </Button>
                      </div>
                    </form>
                  )}

                  {categories.length === 0 && !showAddCategory && !editingCategoryId ? (
                    <div className="text-center py-12 text-muted-foreground">
                      <Package className="h-12 w-12 mx-auto mb-4 opacity-50" />
                      <p>No categories yet. Click &quot;Add Category&quot; to get started.</p>
                    </div>
                  ) : (
                    <div className="grid gap-3 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
                      {categories.map(category => (
                        <div key={category.id} className="flex items-center justify-between p-4 border rounded-lg bg-card">
                          <div>
                            <h4 className="font-semibold">{category.name}</h4>
                            <p className="text-sm text-muted-foreground">
                              {category.productCount} {category.productCount === 1 ? 'product' : 'products'}
                            </p>
                          </div>
                          <div className="flex gap-2">
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleEditCategory(category)}
                              className="text-primary hover:text-primary"
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleDeleteCategory(category.id)}
                              className="text-destructive hover:text-destructive"
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>
      </div>
    </div>
  );
}

export default function AdminPage() {
  return (
    <AdminRoute>
      <AdminContent />
    </AdminRoute>
  );
}
