'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { api, Product, ApiError } from '@/lib/api';
import { useCart } from '@/lib/cart-context';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft, ShoppingCart, AlertCircle, Package } from 'lucide-react';
import { ProductReviews } from '@/components/ProductReviews';
// Removed Image import - using regular img tags for uploaded images

export default function ProductDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { addItem } = useCart();

  const [product, setProduct] = useState<Product | null>(null);
  const [selectedImage, setSelectedImage] = useState<string>('');
  const [quantity, setQuantity] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [addingToCart, setAddingToCart] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const loadProduct = async () => {
      try {
        setLoading(true);
        const data = await api.getProduct(params.id as string);
        setProduct(data);
        setSelectedImage(data.mainPhotoPath);
        setError('');
      } catch (err) {
        const apiError = err as ApiError;
        setError(apiError.detail || apiError.title || 'Failed to load product');
      } finally {
        setLoading(false);
      }
    };

    if (params.id) {
      loadProduct();
    }
  }, [params.id]);

  const handleAddToCart = async () => {
    if (!product) return;

    setAddingToCart(true);
    setError('');
    setSuccessMessage('');

    try {
      await addItem(product.id, quantity);
      setSuccessMessage('Product added to cart successfully!');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to add to cart');
    } finally {
      setAddingToCart(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background">
        <div className="container max-w-7xl mx-auto py-8 px-4">
          <div className="animate-pulse space-y-6">
            <div className="h-8 bg-muted rounded w-24"></div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              <div className="aspect-square bg-muted rounded-lg"></div>
              <div className="space-y-4">
                <div className="h-10 bg-muted rounded w-3/4"></div>
                <div className="h-6 bg-muted rounded w-1/4"></div>
                <div className="h-24 bg-muted rounded"></div>
                <div className="h-12 bg-muted rounded w-full"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error && !product) {
    return (
      <div className="min-h-screen bg-background">
        <div className="container max-w-7xl mx-auto py-8 px-4">
          <Button
            variant="ghost"
            onClick={() => router.push('/products')}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Products
          </Button>
          <Card className="p-8 text-center">
            <AlertCircle className="h-12 w-12 mx-auto mb-4 text-destructive" />
            <h2 className="text-xl font-semibold mb-2">Product Not Found</h2>
            <p className="text-muted-foreground mb-4">{error}</p>
            <Button onClick={() => router.push('/products')}>
              View All Products
            </Button>
          </Card>
        </div>
      </div>
    );
  }

  if (!product) return null;

  const imageUrl = `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001'}${selectedImage}`;
  const allImages = product.photoPaths && product.photoPaths.length > 0
    ? product.photoPaths
    : [product.mainPhotoPath];

  return (
    <div className="min-h-screen bg-background">
      <div className="container max-w-7xl mx-auto py-8 px-4">
        {/* Back Button */}
        <Button
          variant="ghost"
          onClick={() => router.push('/products')}
          className="mb-6"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Products
        </Button>

        {/* Success Message */}
        {successMessage && (
          <div className="mb-6 p-4 bg-green-50 dark:bg-green-950 text-green-600 rounded-md">
            {successMessage}
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="mb-6 p-4 bg-destructive/10 text-destructive rounded-md flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            {error}
          </div>
        )}

        {/* Product Detail */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 lg:gap-12">
          {/* Image Gallery */}
          <div className="space-y-4">
            {/* Main Image */}
            <Card className="overflow-hidden">
              <CardContent className="p-0">
                <div className="relative aspect-square bg-muted">
                  <img
                    src={imageUrl}
                    alt={product.name}
                    className="w-full h-full object-cover"
                  />
                </div>
              </CardContent>
            </Card>

            {/* Thumbnail Gallery */}
            {allImages.length > 1 && (
              <div className="grid grid-cols-4 gap-2">
                {allImages.map((imagePath, index) => {
                  const thumbUrl = `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001'}${imagePath}`;
                  return (
                    <button
                      key={index}
                      onClick={() => setSelectedImage(imagePath)}
                      className={`relative aspect-square rounded-md overflow-hidden border-2 transition-colors ${
                        selectedImage === imagePath
                          ? 'border-primary'
                          : 'border-transparent hover:border-muted-foreground/50'
                      }`}
                    >
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img
                        src={thumbUrl}
                        alt={`${product.name} - Image ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    </button>
                  );
                })}
              </div>
            )}
          </div>

          {/* Product Info */}
          <div className="space-y-6">
            {/* Title & Price */}
            <div>
              <h1 className="text-3xl font-bold mb-2">{product.name}</h1>
              <div className="flex items-center gap-4 mb-4">
                <span className="text-3xl font-bold text-primary">
                  {product.priceAmount.toFixed(2)} {product.priceCurrency}
                </span>
                {product.stockQuantity > 0 ? (
                  <Badge variant="default" className="bg-green-500">
                    In Stock ({product.stockQuantity})
                  </Badge>
                ) : (
                  <Badge variant="destructive">Out of Stock</Badge>
                )}
              </div>
            </div>

            {/* Description */}
            <div>
              <h2 className="text-lg font-semibold mb-2">Description</h2>
              <p className="text-muted-foreground leading-relaxed">
                {product.description}
              </p>
            </div>

            {/* Categories */}
            {product.categories && product.categories.length > 0 && (
              <div>
                <h2 className="text-lg font-semibold mb-2">Categories</h2>
                <div className="flex flex-wrap gap-2">
                  {product.categories.map((category) => (
                    <Badge key={category.id} variant="secondary">
                      {category.name}
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            {/* Quantity & Add to Cart */}
            <Card>
              <CardContent className="p-6 space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2">
                    Quantity
                  </label>
                  <div className="flex items-center gap-3">
                    <Button
                      variant="outline"
                      size="icon"
                      onClick={() => setQuantity(Math.max(1, quantity - 1))}
                      disabled={quantity <= 1 || addingToCart}
                    >
                      -
                    </Button>
                    <span className="text-lg font-semibold w-12 text-center">
                      {quantity}
                    </span>
                    <Button
                      variant="outline"
                      size="icon"
                      onClick={() => setQuantity(Math.min(product.stockQuantity, quantity + 1))}
                      disabled={quantity >= product.stockQuantity || addingToCart}
                    >
                      +
                    </Button>
                  </div>
                </div>

                <Button
                  className="w-full"
                  size="lg"
                  onClick={handleAddToCart}
                  disabled={product.stockQuantity === 0 || addingToCart}
                >
                  {addingToCart ? (
                    <>
                      <Package className="h-5 w-5 mr-2 animate-spin" />
                      Adding to Cart...
                    </>
                  ) : (
                    <>
                      <ShoppingCart className="h-5 w-5 mr-2" />
                      Add to Cart
                    </>
                  )}
                </Button>

                {product.stockQuantity === 0 && (
                  <p className="text-sm text-destructive text-center">
                    This product is currently out of stock
                  </p>
                )}
              </CardContent>
            </Card>

            {/* Stock Info */}
            {product.stockQuantity > 0 && product.stockQuantity <= 10 && (
              <div className="p-4 bg-amber-50 dark:bg-amber-950 text-amber-600 dark:text-amber-400 rounded-md text-sm">
                <AlertCircle className="h-4 w-4 inline mr-2" />
                Only {product.stockQuantity} left in stock - order soon!
              </div>
            )}
          </div>
        </div>

        {/* Product Reviews Section */}
        <div className="mt-12">
          <ProductReviews productId={product.id} />
        </div>
      </div>
    </div>
  );
}
