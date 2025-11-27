import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ShoppingBag, Shield, Truck, CreditCard } from "lucide-react";

export default function Home() {
  return (
    <div className="flex flex-col min-h-screen">
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-gradient-to-b from-primary/5 to-background py-12 sm:py-20 md:py-32">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col items-center justify-center space-y-6 sm:space-y-8 text-center">
            <div className="space-y-3 sm:space-y-4 max-w-3xl mx-auto">
              <h1 className="text-3xl font-bold tracking-tighter sm:text-4xl md:text-5xl lg:text-6xl xl:text-7xl">
                Welcome to <span className="text-primary">SharpBuy</span>
              </h1>
              <p className="mx-auto max-w-2xl text-base sm:text-lg text-muted-foreground md:text-xl">
                Discover amazing products at unbeatable prices. Your one-stop shop for everything you need.
              </p>
            </div>
            <div className="flex flex-col sm:flex-row gap-3 sm:gap-4 justify-center">
              <Link href="/products">
                <Button size="lg">
                  <ShoppingBag className="mr-2 h-5 w-5" />
                  Browse Products
                </Button>
              </Link>
              <Link href="/register">
                <Button size="lg" variant="outline">
                  Get Started
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-12 sm:py-20 md:py-32 bg-muted/30">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10 sm:mb-16">
            <h2 className="text-2xl font-bold tracking-tighter sm:text-3xl md:text-4xl lg:text-5xl">
              Why Choose SharpBuy?
            </h2>
            <p className="mt-3 sm:mt-4 text-muted-foreground text-base sm:text-lg max-w-2xl mx-auto">
              We provide the best shopping experience with premium features
            </p>
          </div>

          <div className="grid gap-4 sm:gap-6 md:gap-8 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 max-w-7xl mx-auto">
            <Card>
              <CardHeader>
                <div className="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10 mb-4">
                  <ShoppingBag className="h-6 w-6 text-primary" />
                </div>
                <CardTitle>Wide Selection</CardTitle>
                <CardDescription>
                  Thousands of products across multiple categories
                </CardDescription>
              </CardHeader>
            </Card>

            <Card>
              <CardHeader>
                <div className="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10 mb-4">
                  <Shield className="h-6 w-6 text-primary" />
                </div>
                <CardTitle>Secure Shopping</CardTitle>
                <CardDescription>
                  Your data is protected with enterprise-grade security
                </CardDescription>
              </CardHeader>
            </Card>

            <Card>
              <CardHeader>
                <div className="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10 mb-4">
                  <Truck className="h-6 w-6 text-primary" />
                </div>
                <CardTitle>Fast Delivery</CardTitle>
                <CardDescription>
                  Quick and reliable shipping to your doorstep
                </CardDescription>
              </CardHeader>
            </Card>

            <Card>
              <CardHeader>
                <div className="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10 mb-4">
                  <CreditCard className="h-6 w-6 text-primary" />
                </div>
                <CardTitle>Easy Payment</CardTitle>
                <CardDescription>
                  Multiple payment options for your convenience
                </CardDescription>
              </CardHeader>
            </Card>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-12 sm:py-20 md:py-32">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <Card className="bg-primary text-primary-foreground shadow-xl max-w-4xl mx-auto">
            <CardHeader className="text-center space-y-3 sm:space-y-4 py-8 sm:py-12 px-4 sm:px-6">
              <CardTitle className="text-2xl sm:text-3xl md:text-4xl">
                Ready to Start Shopping?
              </CardTitle>
              <CardDescription className="text-primary-foreground/80 text-base sm:text-lg">
                Join thousands of satisfied customers today
              </CardDescription>
              <div className="flex flex-col sm:flex-row gap-3 sm:gap-4 justify-center pt-2 sm:pt-4">
                <Link href="/register">
                  <Button size="lg" variant="secondary">
                    Create Account
                  </Button>
                </Link>
                <Link href="/login">
                  <Button size="lg" variant="outline" className="border-primary-foreground/20 hover:bg-primary-foreground/10">
                    Sign In
                  </Button>
                </Link>
              </div>
            </CardHeader>
          </Card>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t py-8 sm:py-12 mt-auto">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center text-xs sm:text-sm text-muted-foreground">
            <p>&copy; 2025 SharpBuy. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}
