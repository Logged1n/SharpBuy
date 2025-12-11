'use client';

import { useState, useEffect } from 'react';
import { api, Review, ApiError } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { Star, AlertCircle } from 'lucide-react';

interface ProductReviewsProps {
  productId: string;
}

export function ProductReviews({ productId }: ProductReviewsProps) {
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  // Review form state
  const [score, setScore] = useState(5);
  const [hoveredScore, setHoveredScore] = useState(0);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  useEffect(() => {
    loadReviews();
  }, [productId]);

  const loadReviews = async () => {
    try {
      setLoading(true);
      const data = await api.getProductReviews(productId);
      setReviews(data);
      setError('');
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to load reviews');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitReview = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');
    setSuccessMessage('');

    try {
      await api.addReview({
        productId,
        score,
        title,
        description: description || null,
      });

      setSuccessMessage('Review submitted successfully!');
      setShowReviewForm(false);
      setTitle('');
      setDescription('');
      setScore(5);

      // Reload reviews
      await loadReviews();

      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to submit review');
    } finally {
      setSubmitting(false);
    }
  };

  const isAuthenticated = typeof window !== 'undefined' && localStorage.getItem('auth_token');

  const renderStars = (rating: number, interactive: boolean = false) => {
    return (
      <div className="flex gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <Star
            key={star}
            className={`h-5 w-5 ${
              interactive ? 'cursor-pointer' : ''
            } ${
              star <= (interactive ? (hoveredScore || score) : rating)
                ? 'fill-yellow-400 text-yellow-400'
                : 'text-gray-300'
            }`}
            onClick={() => interactive && setScore(star)}
            onMouseEnter={() => interactive && setHoveredScore(star)}
            onMouseLeave={() => interactive && setHoveredScore(0)}
          />
        ))}
      </div>
    );
  };

  const averageScore = reviews.length > 0
    ? (reviews.reduce((sum, r) => sum + r.score, 0) / reviews.length).toFixed(1)
    : '0';

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <span>Customer Reviews</span>
              {reviews.length > 0 && (
                <div className="flex items-center gap-2">
                  <span className="text-2xl font-bold">{averageScore}</span>
                  {renderStars(parseFloat(averageScore))}
                  <span className="text-sm text-muted-foreground">
                    ({reviews.length} {reviews.length === 1 ? 'review' : 'reviews'})
                  </span>
                </div>
              )}
            </div>
            {isAuthenticated && !showReviewForm && (
              <Button onClick={() => setShowReviewForm(true)}>
                Write a Review
              </Button>
            )}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {/* Success Message */}
          {successMessage && (
            <div className="mb-4 p-4 bg-green-50 dark:bg-green-950 text-green-600 rounded-md">
              {successMessage}
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="mb-4 p-4 bg-destructive/10 text-destructive rounded-md flex items-center gap-2">
              <AlertCircle className="h-4 w-4" />
              {error}
            </div>
          )}

          {/* Review Form */}
          {showReviewForm && isAuthenticated && (
            <Card className="mb-6">
              <CardContent className="pt-6">
                <form onSubmit={handleSubmitReview} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Rating
                    </label>
                    {renderStars(score, true)}
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Title <span className="text-destructive">*</span>
                    </label>
                    <Input
                      value={title}
                      onChange={(e) => setTitle(e.target.value)}
                      placeholder="Summarize your review"
                      required
                      maxLength={200}
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Description (Optional)
                    </label>
                    <Textarea
                      value={description}
                      onChange={(e) => setDescription(e.target.value)}
                      placeholder="Share your experience with this product"
                      rows={4}
                      maxLength={2000}
                    />
                  </div>

                  <div className="flex gap-2">
                    <Button type="submit" disabled={submitting || !title}>
                      {submitting ? 'Submitting...' : 'Submit Review'}
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => {
                        setShowReviewForm(false);
                        setTitle('');
                        setDescription('');
                        setScore(5);
                      }}
                      disabled={submitting}
                    >
                      Cancel
                    </Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          )}

          {/* Not Authenticated Message */}
          {!isAuthenticated && (
            <div className="text-center p-6 text-muted-foreground">
              Please log in to write a review
            </div>
          )}

          {/* Loading State */}
          {loading && (
            <div className="space-y-4">
              {[1, 2, 3].map((i) => (
                <div key={i} className="animate-pulse">
                  <div className="h-6 bg-muted rounded w-1/4 mb-2"></div>
                  <div className="h-4 bg-muted rounded w-1/2 mb-2"></div>
                  <div className="h-16 bg-muted rounded"></div>
                </div>
              ))}
            </div>
          )}

          {/* Reviews List */}
          {!loading && reviews.length === 0 && (
            <div className="text-center p-6 text-muted-foreground">
              No reviews yet. Be the first to review this product!
            </div>
          )}

          {!loading && reviews.length > 0 && (
            <div className="space-y-4">
              {reviews.map((review) => (
                <Card key={review.id}>
                  <CardContent className="pt-6">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <div className="flex items-center gap-2 mb-1">
                          {renderStars(review.score)}
                          <span className="font-semibold">{review.title}</span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          By {review.userName} on{' '}
                          {new Date(review.createdAt).toLocaleDateString()}
                        </div>
                      </div>
                    </div>
                    {review.description && (
                      <p className="text-muted-foreground mt-2">
                        {review.description}
                      </p>
                    )}
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
