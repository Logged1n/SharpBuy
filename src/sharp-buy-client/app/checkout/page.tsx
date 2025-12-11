'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import { api, Cart } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ArrowLeft, Loader2, CreditCard } from 'lucide-react';
import Link from 'next/link';
import { StripeProvider } from '@/lib/stripe-provider';
import { StripePaymentForm } from '@/components/stripe-payment-form';

export default function CheckoutPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const [cart, setCart] = useState<Cart | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // User's saved address ID from profile
  const [userAddressId, setUserAddressId] = useState<string | null>(null);

  // Shipping address form
  const [shippingAddress, setShippingAddress] = useState({
    line1: '',
    line2: '',
    city: '',
    postalCode: '',
    country: '',
  });

  // Use same address for billing
  const [useSameAddress, setUseSameAddress] = useState(true);

  // Billing address form (if different)
  const [billingAddress, setBillingAddress] = useState({
    line1: '',
    line2: '',
    city: '',
    postalCode: '',
    country: '',
  });

  // Payment
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [showPayment, setShowPayment] = useState(false);
  const [addressesValid, setAddressesValid] = useState(false);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
      return;
    }

    if (isAuthenticated) {
      loadCart();
    }
  }, [authLoading, isAuthenticated, router]);

  const loadCart = async () => {
    try {
      const cartData = await api.getCart();
      setCart(cartData);

      if (!cartData.items || cartData.items.length === 0) {
        router.push('/cart');
        return;
      }

      // Load user profile to auto-populate address
      try {
        const profile = await api.getUserProfile();
        if (profile.address && profile.addressId) {
          setUserAddressId(profile.addressId);
          setShippingAddress({
            line1: profile.address.line1,
            line2: profile.address.line2 || '',
            city: profile.address.city,
            postalCode: profile.address.postalCode,
            country: profile.address.country,
          });
        }
      } catch (profileErr) {
        console.log('Could not load user profile, using empty address');
      }

      // Create payment intent on Stripe
      const { clientSecret: secret } = await api.createPaymentIntent();
      setClientSecret(secret);
    } catch (err: any) {
      setError(err.detail || 'Failed to load checkout');
    } finally {
      setLoading(false);
    }
  };

  const handleContinueToPayment = () => {
    // Validate addresses
    if (!shippingAddress.line1 || !shippingAddress.city || !shippingAddress.postalCode || !shippingAddress.country) {
      setError('Please fill in all required shipping address fields');
      return;
    }

    if (!useSameAddress && (!billingAddress.line1 || !billingAddress.city || !billingAddress.postalCode || !billingAddress.country)) {
      setError('Please fill in all required billing address fields');
      return;
    }

    setError(null);
    setAddressesValid(true);
    setShowPayment(true);
  };

  const handlePaymentSuccess = async (paymentIntentId: string) => {
    try {
      // If user has saved address and didn't modify it, send address ID
      // Otherwise send the full address object to create a new one
      const orderId = await api.placeOrder({
        shippingAddressId: userAddressId,
        billingAddressId: useSameAddress ? userAddressId : null,
        shippingAddress: userAddressId ? null : shippingAddress,
        billingAddress: useSameAddress ? null : (userAddressId ? null : billingAddress),
        paymentIntentId: paymentIntentId,
      });

      // Redirect to confirmation
      router.push(`/order-confirmation?orderId=${orderId}`);
    } catch (err: any) {
      setError(err.message || err.detail || 'Failed to place order');
    }
  };

  const handlePaymentError = (errorMessage: string) => {
    setError(errorMessage);
  };

  const handleShippingAddressChange = (field: keyof typeof shippingAddress, value: string) => {
    setShippingAddress({ ...shippingAddress, [field]: value });
    setUserAddressId(null); // User modified address, create new one
  };

  if (authLoading || loading) {
    return (
      <div className="container mx-auto px-4 py-8 flex items-center justify-center min-h-[60vh]">
        <Loader2 className="h-8 w-8 animate-spin" />
      </div>
    );
  }

  if (!cart || !cart.items || cart.items.length === 0) {
    return null;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <Link href="/cart" className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground mb-6">
        <ArrowLeft className="h-4 w-4 mr-2" />
        Back to Cart
      </Link>

      <h1 className="text-3xl font-bold mb-8">Checkout</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Checkout Form */}
        <div className="lg:col-span-2 space-y-6">
          {/* Shipping Address */}
          <Card>
            <CardHeader>
              <CardTitle>Shipping Address</CardTitle>
              <CardDescription>Where should we deliver your order?</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <Label htmlFor="shipping-line1">Address Line 1 *</Label>
                <Input
                  id="shipping-line1"
                  value={shippingAddress.line1}
                  onChange={(e) => handleShippingAddressChange('line1', e.target.value)}
                  disabled={showPayment}
                  required
                />
              </div>
              <div>
                <Label htmlFor="shipping-line2">Address Line 2</Label>
                <Input
                  id="shipping-line2"
                  value={shippingAddress.line2}
                  onChange={(e) => handleShippingAddressChange('line2', e.target.value)}
                  disabled={showPayment}
                />
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="shipping-city">City *</Label>
                  <Input
                    id="shipping-city"
                    value={shippingAddress.city}
                    onChange={(e) => handleShippingAddressChange('city', e.target.value)}
                    disabled={showPayment}
                    required
                  />
                </div>
                <div>
                  <Label htmlFor="shipping-postalCode">Postal Code *</Label>
                  <Input
                    id="shipping-postalCode"
                    value={shippingAddress.postalCode}
                    onChange={(e) => handleShippingAddressChange('postalCode', e.target.value)}
                    disabled={showPayment}
                    required
                  />
                </div>
              </div>
              <div>
                <Label htmlFor="shipping-country">Country *</Label>
                <Input
                  id="shipping-country"
                  value={shippingAddress.country}
                  onChange={(e) => handleShippingAddressChange('country', e.target.value)}
                  disabled={showPayment}
                  required
                />
              </div>
            </CardContent>
          </Card>

          {/* Billing Address */}
          <Card>
            <CardHeader>
              <CardTitle>Billing Address</CardTitle>
              <CardDescription>Address for billing purposes</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  id="same-address"
                  checked={useSameAddress}
                  onChange={(e) => setUseSameAddress(e.target.checked)}
                  disabled={showPayment}
                  className="h-4 w-4"
                />
                <Label htmlFor="same-address" className="cursor-pointer">
                  Same as shipping address
                </Label>
              </div>

              {!useSameAddress && (
                <>
                  <div>
                    <Label htmlFor="billing-line1">Address Line 1 *</Label>
                    <Input
                      id="billing-line1"
                      value={billingAddress.line1}
                      onChange={(e) => setBillingAddress({ ...billingAddress, line1: e.target.value })}
                      disabled={showPayment}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="billing-line2">Address Line 2</Label>
                    <Input
                      id="billing-line2"
                      value={billingAddress.line2}
                      onChange={(e) => setBillingAddress({ ...billingAddress, line2: e.target.value })}
                      disabled={showPayment}
                    />
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <Label htmlFor="billing-city">City *</Label>
                      <Input
                        id="billing-city"
                        value={billingAddress.city}
                        onChange={(e) => setBillingAddress({ ...billingAddress, city: e.target.value })}
                        disabled={showPayment}
                        required
                      />
                    </div>
                    <div>
                      <Label htmlFor="billing-postalCode">Postal Code *</Label>
                      <Input
                        id="billing-postalCode"
                        value={billingAddress.postalCode}
                        onChange={(e) => setBillingAddress({ ...billingAddress, postalCode: e.target.value })}
                        disabled={showPayment}
                        required
                      />
                    </div>
                  </div>
                  <div>
                    <Label htmlFor="billing-country">Country *</Label>
                    <Input
                      id="billing-country"
                      value={billingAddress.country}
                      onChange={(e) => setBillingAddress({ ...billingAddress, country: e.target.value })}
                      disabled={showPayment}
                      required
                    />
                  </div>
                </>
              )}
            </CardContent>
          </Card>

          {/* Payment Section */}
          {!showPayment ? (
            <Button
              onClick={handleContinueToPayment}
              className="w-full"
              size="lg"
              disabled={!clientSecret}
            >
              <CreditCard className="h-4 w-4 mr-2" />
              Continue to Payment
            </Button>
          ) : (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <CreditCard className="h-5 w-5" />
                  Payment Information
                </CardTitle>
                <CardDescription>
                  Enter your payment details securely with Stripe
                </CardDescription>
              </CardHeader>
              <CardContent>
                {clientSecret && (
                  <StripeProvider clientSecret={clientSecret}>
                    <StripePaymentForm
                      amount={cart.totalAmount}
                      currency={cart.currency}
                      onSuccess={handlePaymentSuccess}
                      onError={handlePaymentError}
                    />
                  </StripeProvider>
                )}
              </CardContent>
            </Card>
          )}

          {error && (
            <div className="p-4 bg-destructive/10 border border-destructive rounded-md">
              <p className="text-sm text-destructive">{error}</p>
            </div>
          )}

          {showPayment && (
            <Button
              variant="outline"
              onClick={() => setShowPayment(false)}
              className="w-full"
            >
              Back to Addresses
            </Button>
          )}
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <Card className="sticky top-4">
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                {cart.items.map((item) => (
                  <div key={item.productId} className="flex justify-between text-sm">
                    <span className="text-muted-foreground">
                      {item.productName} × {item.quantity}
                    </span>
                    <span className="font-medium">
                      {item.currency} {item.totalPrice.toFixed(2)}
                    </span>
                  </div>
                ))}
              </div>

              <div className="border-t pt-4">
                <div className="flex justify-between text-lg font-bold">
                  <span>Total</span>
                  <span>{cart.currency} {cart.totalAmount.toFixed(2)}</span>
                </div>
              </div>

              {!showPayment && (
                <div className="pt-4 text-xs text-muted-foreground">
                  <p>You will be able to review your payment details on the next step.</p>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
