import Link from "next/link";
import { Button } from "@/components/ui/button";
import { ShoppingBag, Zap, Shield } from "lucide-react";

export default function Home() {
  return (
    <div className="flex flex-col">
      {/* Hero Section */}
      <section className="w-full py-12 md:py-24 lg:py-32 xl:py-48 bg-gradient-to-b from-background to-muted">
        <div className="container px-4 md:px-6">
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="space-y-2">
              <h1 className="text-3xl font-bold tracking-tighter sm:text-4xl md:text-5xl lg:text-6xl/none">
                Welcome to SharpBuy
              </h1>
              <p className="mx-auto max-w-[700px] text-muted-foreground md:text-xl">
                Your one-stop shop for quality products. Fast shipping, secure
                payments, and exceptional customer service.
              </p>
            </div>
            <div className="space-x-4">
              <Link href="/products">
                <Button size="lg">
                  <ShoppingBag className="mr-2 h-4 w-4" />
                  Shop Now
                </Button>
              </Link>
              <Link href="/register">
                <Button variant="outline" size="lg">
                  Get Started
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="w-full py-12 md:py-24 lg:py-32">
        <div className="container px-4 md:px-6">
          <div className="grid gap-10 sm:grid-cols-2 md:grid-cols-3">
            <div className="flex flex-col items-center space-y-2 border-border p-4 rounded-lg">
              <Zap className="h-12 w-12 mb-2" />
              <h3 className="text-xl font-bold">Fast Delivery</h3>
              <p className="text-sm text-muted-foreground text-center">
                Quick and reliable shipping to your doorstep
              </p>
            </div>
            <div className="flex flex-col items-center space-y-2 border-border p-4 rounded-lg">
              <Shield className="h-12 w-12 mb-2" />
              <h3 className="text-xl font-bold">Secure Payments</h3>
              <p className="text-sm text-muted-foreground text-center">
                Your transactions are safe and encrypted
              </p>
            </div>
            <div className="flex flex-col items-center space-y-2 border-border p-4 rounded-lg">
              <ShoppingBag className="h-12 w-12 mb-2" />
              <h3 className="text-xl font-bold">Quality Products</h3>
              <p className="text-sm text-muted-foreground text-center">
                Curated selection of premium items
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
