'use client';

import { Suspense, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ShoppingCart, Loader2, AlertCircle, CheckCircle } from 'lucide-react';

function AuthCallbackContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('Processing authentication...');

  useEffect(() => {
    const handleCallback = () => {
      try {
        const token = searchParams.get('token');

        if (!token) {
          setStatus('error');
          setMessage('No authentication token received. Please try again.');
          setTimeout(() => router.push('/login'), 3000);
          return;
        }

        // Store the token in localStorage with the correct key
        localStorage.setItem('auth_token', token);
        localStorage.setItem('user', JSON.stringify({ token }));

        setStatus('success');
        setMessage('Authentication successful! Redirecting...');

        // Redirect to home page after a short delay
        // Use window.location.href to force a full page reload so AuthProvider picks up the new token
        setTimeout(() => {
          window.location.href = '/';
        }, 1500);
      } catch (error) {
        setStatus('error');
        setMessage('An error occurred during authentication. Please try again.');
        setTimeout(() => router.push('/login'), 3000);
      }
    };

    handleCallback();
  }, [searchParams, router]);

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center py-12 px-4">
      <Card className="w-full max-w-md shadow-lg">
        <CardHeader className="space-y-2 text-center pb-6">
          <div className="flex justify-center mb-2">
            <div className="flex items-center space-x-2">
              <ShoppingCart className="h-8 w-8 text-primary" />
              <span className="text-2xl font-bold">SharpBuy</span>
            </div>
          </div>
          <CardTitle className="text-2xl">
            {status === 'loading' && 'Authenticating...'}
            {status === 'success' && 'Success!'}
            {status === 'error' && 'Authentication Failed'}
          </CardTitle>
          <CardDescription>
            {status === 'loading' && 'Please wait while we complete your sign-in'}
            {status === 'success' && 'You have been successfully authenticated'}
            {status === 'error' && 'We encountered an issue during authentication'}
          </CardDescription>
        </CardHeader>
        <CardContent className="px-6">
          <div className="flex flex-col items-center gap-4">
            {status === 'loading' && (
              <Loader2 className="h-12 w-12 text-primary animate-spin" />
            )}
            {status === 'success' && (
              <CheckCircle className="h-12 w-12 text-green-600" />
            )}
            {status === 'error' && (
              <AlertCircle className="h-12 w-12 text-destructive" />
            )}
            <p className="text-center text-sm text-muted-foreground">{message}</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default function AuthCallbackPage() {
  return (
    <Suspense fallback={
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center py-12 px-4">
        <Card className="w-full max-w-md shadow-lg">
          <CardHeader className="space-y-2 text-center pb-6">
            <div className="flex justify-center mb-2">
              <div className="flex items-center space-x-2">
                <ShoppingCart className="h-8 w-8 text-primary" />
                <span className="text-2xl font-bold">SharpBuy</span>
              </div>
            </div>
            <CardTitle className="text-2xl">Authenticating...</CardTitle>
            <CardDescription>Please wait while we complete your sign-in</CardDescription>
          </CardHeader>
          <CardContent className="px-6">
            <div className="flex flex-col items-center gap-4">
              <Loader2 className="h-12 w-12 text-primary animate-spin" />
              <p className="text-center text-sm text-muted-foreground">Processing authentication...</p>
            </div>
          </CardContent>
        </Card>
      </div>
    }>
      <AuthCallbackContent />
    </Suspense>
  );
}
