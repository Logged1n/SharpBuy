# SharpBuy Frontend Client

Modern e-commerce frontend built with Next.js 15, TypeScript, Tailwind CSS, and shadcn/ui.

## Tech Stack

- **Framework**: Next.js 15 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **UI Components**: shadcn/ui (Radix UI primitives)
- **Icons**: Lucide React
- **Package Manager**: npm

## Project Structure

```
client/
├── app/                      # Next.js App Router pages
│   ├── login/               # Login page
│   ├── register/            # Registration page
│   ├── products/            # Products listing and detail pages
│   │   └── [id]/           # Dynamic product detail page
│   ├── layout.tsx           # Root layout with providers
│   ├── page.tsx             # Home page
│   └── globals.css          # Global styles and CSS variables
├── components/              # React components
│   ├── ui/                 # shadcn/ui components
│   │   ├── button.tsx
│   │   ├── input.tsx
│   │   ├── card.tsx
│   │   └── label.tsx
│   ├── products/           # Product-specific components
│   │   └── product-card.tsx
│   └── navigation.tsx      # Main navigation component
├── contexts/               # React contexts
│   └── auth-context.tsx   # Authentication context
├── lib/                    # Utility functions and configurations
│   ├── utils.ts           # Utility functions (cn helper)
│   ├── config.ts          # App configuration
│   └── api-client.ts      # API client for backend communication
├── types/                  # TypeScript type definitions
│   └── index.ts           # Shared types and interfaces
├── hooks/                  # Custom React hooks (empty for now)
├── public/                 # Static assets
├── .env.local             # Environment variables (not committed)
└── .env.local.example     # Environment variables template

```

## Getting Started

### Prerequisites

- Node.js 18+ installed
- Backend API running (SharpBuy .NET API)

### Installation

1. Install dependencies:
   ```bash
   npm install
   ```

2. Set up environment variables:
   ```bash
   cp .env.local.example .env.local
   ```

3. Update `.env.local` with your backend API URL:
   ```env
   NEXT_PUBLIC_API_URL=https://localhost:7001
   ```

### Development

Run the development server:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

### Build

Create a production build:

```bash
npm run build
```

Run the production build:

```bash
npm start
```

### Linting

Run ESLint:

```bash
npm run lint
```

## Features

### Implemented

- ✅ User authentication (login/register)
- ✅ Product browsing and search
- ✅ Product detail view
- ✅ Shopping cart functionality (API integration ready)
- ✅ Responsive design
- ✅ Dark mode support (via Tailwind CSS)
- ✅ Type-safe API client
- ✅ Error handling and loading states

### Planned

- 🔄 Cart page and management
- 🔄 Checkout flow
- 🔄 Order history
- 🔄 User profile management
- 🔄 Product categories filtering
- 🔄 Product reviews and ratings
- 🔄 Search functionality
- 🔄 Wishlist

## API Integration

The frontend communicates with the SharpBuy .NET backend API using a type-safe API client located in `lib/api-client.ts`.

### Available Endpoints

#### Authentication
- `POST /users/register` - Register new user
- `POST /users/login` - Login user
- `POST /users/verify-email` - Verify email address

#### Products
- `GET /products` - Get all products
- `GET /products/{id}` - Get product by ID
- `POST /products` - Add new product (admin)
- `PUT /products/{id}/price` - Update product price (admin)

#### Cart
- `GET /cart` - Get user cart
- `POST /cart/items` - Add item to cart
- `PUT /cart/items/{id}` - Update cart item quantity
- `DELETE /cart/items/{id}` - Remove item from cart
- `DELETE /cart` - Clear cart

#### Orders
- `GET /orders` - Get user orders
- `GET /orders/{id}` - Get order by ID
- `POST /orders` - Create new order

#### Reviews
- `GET /products/{id}/reviews` - Get product reviews
- `POST /reviews` - Add product review

### Authentication Flow

1. User logs in via `/login` page
2. JWT token received from backend
3. Token stored in localStorage
4. Token automatically included in subsequent API requests
5. User can access protected routes

## Component Library

### shadcn/ui Components

The project uses shadcn/ui components for consistent and accessible UI:

- **Button**: Primary, secondary, outline, ghost variants
- **Input**: Form inputs with validation support
- **Card**: Container component with header, content, footer
- **Label**: Form labels

### Custom Components

- **Navigation**: Main navigation bar with authentication state
- **ProductCard**: Reusable product card for listings
- **AuthProvider**: Context provider for authentication state

## Styling

### Tailwind CSS

The project uses Tailwind CSS with custom configuration:

- CSS variables for theming
- Dark mode support via `class` strategy
- Custom color palette aligned with shadcn/ui
- Responsive breakpoints (sm, md, lg, xl)

### CSS Variables

Theme colors are defined as CSS variables in `app/globals.css` and can be customized:

```css
:root {
  --background: 0 0% 100%;
  --foreground: 0 0% 3.9%;
  --primary: 0 0% 9%;
  /* ... more variables */
}

.dark {
  --background: 0 0% 3.9%;
  --foreground: 0 0% 98%;
  /* ... dark mode overrides */
}
```

## Type Safety

All API responses and requests are typed using TypeScript interfaces defined in `types/index.ts`:

```typescript
interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  stockQuantity: number;
  // ...
}
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API base URL | `https://localhost:7001` |

## Contributing

1. Create a feature branch
2. Make changes
3. Test thoroughly
4. Submit pull request

## Troubleshooting

### API Connection Issues

If you're getting CORS errors:
- Ensure the backend API is running
- Check that the API URL in `.env.local` is correct
- Verify CORS is configured correctly in the backend

### Build Errors

If you encounter build errors:
- Clear `.next` directory: `rm -rf .next`
- Delete `node_modules` and reinstall: `rm -rf node_modules && npm install`
- Check for TypeScript errors: `npm run build`

### Authentication Issues

If authentication isn't working:
- Check browser console for errors
- Verify JWT token in localStorage (DevTools > Application > Local Storage)
- Ensure backend is returning valid JWT tokens

## License

Part of the SharpBuy e-commerce platform.
