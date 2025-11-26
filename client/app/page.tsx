import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Navigation } from '@/components/navigation';

export default function Home() {
  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main className="container mx-auto px-4 py-16">
        <div className="flex flex-col items-center justify-center space-y-8 text-center">
          <h1 className="text-4xl font-bold tracking-tight sm:text-6xl">
            Welcome to SharpBuy
          </h1>
          <p className="max-w-2xl text-lg text-muted-foreground">
            Your modern e-commerce platform for all your shopping needs.
            Discover amazing products, enjoy seamless checkout, and track your
            orders with ease.
          </p>
          <div className="flex gap-4">
            <Button size="lg" asChild>
              <Link href="/products">Browse Products</Link>
            </Button>
            <Button size="lg" variant="outline" asChild>
              <Link href="/register">Get Started</Link>
            </Button>
          </div>
        </div>

        <div className="mt-24 grid gap-8 md:grid-cols-3">
          <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
            <h3 className="mb-2 text-xl font-semibold">Wide Selection</h3>
            <p className="text-muted-foreground">
              Browse through thousands of products across multiple categories to
              find exactly what you need.
            </p>
          </div>
          <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
            <h3 className="mb-2 text-xl font-semibold">Secure Checkout</h3>
            <p className="text-muted-foreground">
              Shop with confidence using our secure payment system and encrypted
              transactions.
            </p>
          </div>
          <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
            <h3 className="mb-2 text-xl font-semibold">Fast Delivery</h3>
            <p className="text-muted-foreground">
              Track your orders in real-time and enjoy fast, reliable delivery to
              your doorstep.
            </p>
          </div>
        </div>
      </main>
    </div>
  );
}
