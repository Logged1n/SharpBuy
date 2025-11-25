# SharpBuy Frontend Client - Setup Guide

This document describes the Next.js frontend client that has been added to the SharpBuy e-commerce platform.

## Overview

A modern, responsive Next.js 15 frontend client built with TypeScript, Tailwind CSS, and shadcn/ui components. The client integrates with the .NET Web API backend and runs as an Aspire resource in the development environment.

## Tech Stack

- **Next.js 15** - React framework with App Router
- **TypeScript** - Type-safe development
- **Tailwind CSS** - Utility-first CSS framework
- **shadcn/ui** - High-quality UI components
- **pnpm** - Fast, disk space efficient package manager
- **Monorepo** - Organized with pnpm workspaces

## Project Structure

```
SharpBuy/
├── pnpm-workspace.yaml          # Monorepo configuration
├── client/                      # Next.js frontend
│   ├── src/
│   │   ├── app/                # App Router pages
│   │   │   ├── page.tsx        # Home page
│   │   │   ├── login/          # Login page
│   │   │   ├── register/       # Registration page
│   │   │   ├── verify-email/   # Email verification
│   │   │   ├── products/       # Product listing & details
│   │   │   ├── cart/           # Shopping cart
│   │   │   └── profile/        # User profile
│   │   ├── components/         # React components
│   │   │   ├── navigation.tsx  # Navigation bar
│   │   │   └── ui/            # shadcn/ui components
│   │   ├── contexts/          # React contexts
│   │   │   └── auth-context.tsx # Authentication state
│   │   ├── hooks/             # Custom React hooks
│   │   │   └── use-cart.ts    # Shopping cart hook
│   │   ├── lib/               # Utilities
│   │   │   ├── api-client.ts  # API client
│   │   │   └── utils.ts       # Utility functions
│   │   └── types/             # TypeScript types
│   │       └── index.ts       # API types
│   ├── package.json
│   ├── tsconfig.json
│   ├── tailwind.config.ts
│   ├── next.config.ts
│   └── Dockerfile
└── src/
    └── Aspire.AppHost/         # Aspire orchestration
        └── Program.cs          # Updated with Next.js app
```

## Features Implemented

### 1. Authentication
- **Login** (`/login`) - User authentication with JWT tokens
- **Register** (`/register`) - New user registration
- **Verify Email** (`/verify-email`) - Email verification flow
- **Auth Context** - Global authentication state management
- **Protected Routes** - Profile page requires authentication

### 2. Product Management
- **Product List** (`/products`) - Browse all products
- **Product Details** (`/products/[id]`) - View product details
- **Add to Cart** - Quick add from listing or detailed add from product page

### 3. Shopping Cart
- **Cart Page** (`/cart`) - View and manage cart items
- **Quantity Management** - Update item quantities
- **Cart State** - Persisted to localStorage
- **Cart Hook** - Reusable cart management logic

### 4. User Profile
- **Profile Page** (`/profile`) - View user information
- **Email Status** - Display email verification status

### 5. UI Components
- **Navigation** - Responsive header with cart counter
- **Cards** - Product cards, form cards
- **Buttons** - Various button styles (primary, outline, ghost)
- **Forms** - Input fields, labels
- **Icons** - Lucide React icons

## API Integration

### API Client (`src/lib/api-client.ts`)

The API client provides type-safe methods for all backend endpoints:

```typescript
// Authentication
await apiClient.register({ email, password, firstName, lastName });
await apiClient.login({ email, password });
await apiClient.verifyEmail({ email, token });

// Products
const products = await apiClient.getProducts();
const product = await apiClient.getProduct(id);

// Users
const user = await apiClient.getUser(id);
const permissions = await apiClient.getUserPermissions();
```

### Authentication Flow

1. User enters credentials in login form
2. API client sends POST to `/users/login`
3. Backend returns JWT token and user data
4. Token stored in localStorage
5. Token included in Authorization header for subsequent requests
6. Auth context provides authentication state to components

### Error Handling

API errors are properly typed and displayed to users:

```typescript
interface ApiError {
  type: string;
  title: string;
  status: number;
  errors?: Record<string, string[]>;
}
```

## Aspire Integration

The Next.js app is integrated into the Aspire orchestration:

### Changes to `Aspire.AppHost/Program.cs`

```csharp
builder.AddNpmApp("SharpBuy-Client", "../../client")
    .WithReference(webApi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();
```

### How It Works

1. Aspire starts the Next.js development server (`pnpm dev`)
2. Client gets reference to Web API service
3. API URL is automatically configured via service discovery
4. All services visible in Aspire Dashboard

## Running the Application

### Option 1: With Aspire (Recommended)

```bash
# From repository root
dotnet run --project src/Aspire.AppHost
```

This starts:
- PostgreSQL database
- Papercut SMTP server
- .NET Web API
- Next.js frontend client

Access:
- Frontend: http://localhost:{assigned-port} (check Aspire Dashboard)
- API: http://localhost:{assigned-port}
- Aspire Dashboard: http://localhost:15xxx

### Option 2: Frontend Only

```bash
# From client directory
cd client
pnpm install
pnpm dev
```

Access at http://localhost:3000

**Note:** You must set `NEXT_PUBLIC_API_URL` in `.env.local` when running standalone.

## Environment Variables

### Development (Aspire)
No manual configuration needed - Aspire handles service discovery.

### Development (Standalone)
Create `client/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### Production
Set via environment variables or deployment configuration.

## Building for Production

### Next.js Build

```bash
cd client
pnpm build
pnpm start
```

### Docker Build

```bash
cd client
docker build -t sharpbuy-client .
docker run -p 3000:3000 -e NEXT_PUBLIC_API_URL=http://api:5000 sharpbuy-client
```

## Development Workflow

### Adding New Pages

1. Create page file in `src/app/[route]/page.tsx`
2. Use "use client" directive if client-side features needed
3. Import and use existing components from `@/components`
4. Add navigation link in `navigation.tsx` if needed

### Adding New API Endpoints

1. Add types to `src/types/index.ts`
2. Add method to `src/lib/api-client.ts`
3. Use in components with error handling

Example:

```typescript
// types/index.ts
export interface Review {
  id: string;
  productId: string;
  rating: number;
  comment: string;
}

// lib/api-client.ts
async getProductReviews(productId: string): Promise<Review[]> {
  return this.request<Review[]>(`/products/${productId}/reviews`);
}

// app/products/[id]/page.tsx
const reviews = await apiClient.getProductReviews(productId);
```

### Adding New Components

1. Create component in `src/components/[name].tsx`
2. Use TypeScript interfaces for props
3. Import UI components from `@/components/ui`

```typescript
interface ProductCardProps {
  product: Product;
  onAddToCart: (product: Product) => void;
}

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
  // Component implementation
}
```

## Testing

### Manual Testing Checklist

- [ ] Register new user
- [ ] Login with credentials
- [ ] Browse products
- [ ] View product details
- [ ] Add products to cart
- [ ] Update cart quantities
- [ ] Remove items from cart
- [ ] View user profile
- [ ] Logout and login again

### API Testing

Test with the API running:

1. Register: POST `/users/register`
2. Login: POST `/users/login`
3. Get products: GET `/products`
4. Protected endpoints require JWT token

## Troubleshooting

### Frontend won't start

```bash
# Clear node_modules and reinstall
rm -rf client/node_modules client/.next
cd client && pnpm install
```

### API connection errors

1. Check API is running
2. Verify `NEXT_PUBLIC_API_URL` is correct
3. Check CORS settings in Web API
4. Inspect browser console for errors

### Build errors

```bash
# Clear Next.js cache
cd client
rm -rf .next
pnpm build
```

### Styling issues

```bash
# Rebuild Tailwind
cd client
pnpm dev
```

## Future Enhancements

Potential features to add:

1. **Product Search** - Search and filter products
2. **Order History** - View past orders
3. **Checkout Flow** - Complete order placement
4. **Product Reviews** - Add and view reviews
5. **Wishlist** - Save products for later
6. **Admin Panel** - Product and user management
7. **Real-time Updates** - SignalR integration
8. **Payment Integration** - Stripe/PayPal
9. **Image Upload** - Product photo management
10. **Dark Mode** - Theme toggle

## Code Quality

### TypeScript

- Strict mode enabled
- All API responses typed
- No `any` types used

### Components

- Functional components with hooks
- Proper error boundaries
- Accessibility considerations

### State Management

- Auth context for global auth state
- Custom hooks for cart logic
- localStorage for persistence

## Security Considerations

1. **JWT Storage** - Currently in localStorage (consider httpOnly cookies for production)
2. **XSS Protection** - React escapes output by default
3. **CSRF** - Not applicable with JWT (no cookies)
4. **Input Validation** - Client-side validation + backend validation
5. **HTTPS** - Use in production

## Performance

- Server-side rendering with Next.js App Router
- Image optimization with Next.js Image component (ready to add)
- Code splitting by route
- Standalone build output for smaller Docker images

## Maintenance

### Updating Dependencies

```bash
cd client
pnpm update
```

### Checking for Security Issues

```bash
pnpm audit
```

## Support

For issues or questions:
1. Check this documentation
2. Review API documentation in CLAUDE.md
3. Check Aspire Dashboard logs
4. Review browser console errors

---

**Created:** 2025-11-25
**Version:** 1.0.0
**Framework:** Next.js 15.1.2
