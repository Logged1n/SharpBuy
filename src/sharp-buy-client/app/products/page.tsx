'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { api, ProductListItem, PagedResult, CategoryListItem } from '@/lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ShoppingCart, ChevronLeft, ChevronRight, Eye, Search, X } from 'lucide-react';
import { useCart } from '@/lib/cart-context';
import { Badge } from '@/components/ui/badge';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001';

export default function ProductsPage() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [categories, setCategories] = useState<CategoryListItem[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const { addItem } = useCart();

  useEffect(() => {
    loadCategories();
  }, []);

  useEffect(() => {
    loadProducts();
  }, [currentPage, searchTerm, selectedCategories]);

  const loadCategories = async () => {
    try {
      const data = await api.getCategories(1, 100);
      setCategories(data.items);
    } catch (err) {
      console.error('Failed to load categories:', err);
    }
  };

  const loadProducts = async () => {
    setIsLoading(true);
    try {
      const data: PagedResult<ProductListItem> = await api.getProducts(
        currentPage,
        12,
        searchTerm || undefined,
        selectedCategories.length > 0 ? selectedCategories : undefined
      );
      setProducts(data.items);
      setTotalPages(data.totalPages);
    } catch (err) {
      console.error('Failed to load products:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchTerm(searchInput);
    setCurrentPage(1);
  };

  const handleClearSearch = () => {
    setSearchInput('');
    setSearchTerm('');
    setCurrentPage(1);
  };

  const toggleCategory = (categoryId: string) => {
    setSelectedCategories(prev =>
      prev.includes(categoryId)
        ? prev.filter(id => id !== categoryId)
        : [...prev, categoryId]
    );
    setCurrentPage(1);
  };

  const clearFilters = () => {
    setSearchInput('');
    setSearchTerm('');
    setSelectedCategories([]);
    setCurrentPage(1);
  };

  const handleAddToCart = async (productId: string) => {
    try {
      await addItem(productId, 1);
    } catch (err) {
      console.error('Failed to add to cart:', err);
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <div className="container max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="space-y-2">
            <h1 className="text-2xl sm:text-3xl font-bold tracking-tight">Products</h1>
            <p className="text-sm sm:text-base text-muted-foreground">
              Browse our collection of products
            </p>
          </div>

          {/* Filters */}
          <Card>
            <CardContent className="pt-6">
              <div className="space-y-4">
                {/* Search */}
                <form onSubmit={handleSearch} className="flex gap-2">
                  <div className="relative flex-1">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      type="text"
                      placeholder="Search products..."
                      value={searchInput}
                      onChange={(e) => setSearchInput(e.target.value)}
                      className="pl-9"
                    />
                  </div>
                  <Button type="submit" variant="default">
                    Search
                  </Button>
                  {(searchTerm || selectedCategories.length > 0) && (
                    <Button type="button" variant="outline" onClick={clearFilters}>
                      <X className="h-4 w-4 mr-1" />
                      Clear
                    </Button>
                  )}
                </form>

                {/* Categories */}
                {categories.length > 0 && (
                  <div className="space-y-2">
                    <p className="text-sm font-medium">Categories:</p>
                    <div className="flex flex-wrap gap-2">
                      {categories.map((category) => (
                        <Badge
                          key={category.id}
                          variant={selectedCategories.includes(category.id) ? "default" : "outline"}
                          className="cursor-pointer"
                          onClick={() => toggleCategory(category.id)}
                        >
                          {category.name}
                        </Badge>
                      ))}
                    </div>
                  </div>
                )}

                {/* Active Filters Display */}
                {(searchTerm || selectedCategories.length > 0) && (
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <span>Active filters:</span>
                    {searchTerm && (
                      <Badge variant="secondary" className="gap-1">
                        Search: {searchTerm}
                        <X
                          className="h-3 w-3 cursor-pointer"
                          onClick={handleClearSearch}
                        />
                      </Badge>
                    )}
                    {selectedCategories.length > 0 && (
                      <Badge variant="secondary">
                        {selectedCategories.length} {selectedCategories.length === 1 ? 'category' : 'categories'}
                      </Badge>
                    )}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Products Grid */}
          {isLoading ? (
            <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {[...Array(8)].map((_, i) => (
                <Card key={i} className="animate-pulse">
                  <CardHeader>
                    <div className="h-48 bg-muted rounded-md" />
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-2">
                      <div className="h-4 bg-muted rounded" />
                      <div className="h-4 bg-muted rounded w-3/4" />
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          ) : products.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <ShoppingCart className="h-16 w-16 text-muted-foreground mb-4" />
                <h3 className="text-lg font-semibold mb-2">No products available</h3>
                <p className="text-muted-foreground text-center">
                  Check back later for new products
                </p>
              </CardContent>
            </Card>
          ) : (
            <>
              <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                {products.map((product) => (
                  <Card key={product.id} className="overflow-hidden hover:shadow-lg transition-shadow flex flex-col">
                    <Link href={`/products/${product.id}`}>
                      <CardHeader className="p-0 cursor-pointer">
                        <div className="relative h-48 w-full bg-muted group">
                          {product.mainPhotoPath && product.mainPhotoPath !== '/placeholder.jpg' ? (
                            <>
                              {/* eslint-disable-next-line @next/next/no-img-element */}
                              <img
                                src={`${API_BASE_URL}${product.mainPhotoPath}`}
                                alt={product.name}
                                className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                              />
                              <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                <Eye className="h-8 w-8 text-white" />
                              </div>
                            </>
                          ) : (
                            <div className="flex items-center justify-center h-full">
                              <ShoppingCart className="h-16 w-16 text-muted-foreground" />
                            </div>
                          )}
                        </div>
                      </CardHeader>
                    </Link>
                    <CardContent className="p-4 flex-1 flex flex-col">
                      <Link href={`/products/${product.id}`}>
                        <CardTitle className="text-lg mb-2 line-clamp-1 hover:text-primary cursor-pointer">
                          {product.name}
                        </CardTitle>
                      </Link>
                      <CardDescription className="text-sm mb-3 line-clamp-2 flex-1">
                        {product.description}
                      </CardDescription>
                      <div className="space-y-3">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-xl font-bold">
                              {product.priceAmount.toFixed(2)} {product.priceCurrency}
                            </p>
                            <p className="text-xs text-muted-foreground">
                              {product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock'}
                            </p>
                          </div>
                        </div>
                        <div className="flex gap-2">
                          <Link href={`/products/${product.id}`} className="flex-1">
                            <Button variant="outline" size="sm" className="w-full">
                              <Eye className="h-4 w-4 mr-1" />
                              View
                            </Button>
                          </Link>
                          <Button
                            onClick={() => handleAddToCart(product.id)}
                            disabled={product.stockQuantity === 0}
                            size="sm"
                            className="flex-1"
                          >
                            <ShoppingCart className="h-4 w-4 mr-1" />
                            Add
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-8">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                  >
                    <ChevronLeft className="h-4 w-4" />
                    Previous
                  </Button>
                  <span className="text-sm text-muted-foreground">
                    Page {currentPage} of {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                  >
                    Next
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}
