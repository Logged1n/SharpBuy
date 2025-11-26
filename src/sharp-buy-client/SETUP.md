# SharpBuy Client - Setup Guide

A modern e-commerce frontend built with Next.js 16, React 19, TypeScript, and Tailwind CSS.

## Features

- **Landing Page**: Hero section with call-to-action, features showcase, and responsive navbar
- **Authentication**: JWT-based login and registration with the SharpBuy backend API
- **Admin Dashboard**: Protected admin panel for product management
- **Product Management**: Add and manage products (requires authentication)
- **Responsive Design**: Mobile-first design with Tailwind CSS
- **Type Safety**: Full TypeScript support throughout the application

## Prerequisites

- Node.js 20+ installed
- SharpBuy backend API running (default: https://localhost:5001)

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

Create a `.env.local` file in the root directory:

```bash
cp .env.local.example .env.local
```

Edit `.env.local` and set your API URL:

```
NEXT_PUBLIC_API_URL=https://localhost:5001
```

### 3. Run Development Server

```bash
npm run dev
```

The application will be available at [http://localhost:3000](http://localhost:3000).

### 4. Build for Production

```bash
npm run build
npm start
```

## Project Structure

```
sharp-buy-client/
├── app/                      # Next.js 16 App Router
│   ├── page.tsx             # Landing page
│   ├── layout.tsx           # Root layout with AuthProvider
│   ├── login/               # Login page
│   ├── register/            # Registration page
│   ├── admin/               # Admin dashboard (protected)
│   ├── products/            # Products listing
│   └── categories/          # Categories page
├── components/
│   ├── navbar.tsx           # Global navigation bar
│   ├── protected-route.tsx  # Authentication guard
│   └── ui/                  # Reusable UI components
│       ├── button.tsx
│       ├── input.tsx
│       ├── card.tsx
│       └── label.tsx
├── lib/
│   ├── api.ts               # API client and types
│   ├── auth.tsx             # Authentication context
│   └── utils.ts             # Utility functions
└── public/                  # Static assets
```

## Available Pages

- **/** - Landing page with hero section and features
- **/login** - User login
- **/register** - User registration
- **/admin** - Admin dashboard (requires authentication)
- **/products** - Product catalog
- **/categories** - Product categories

## Authentication Flow

1. Users register via `/register` with required fields:
   - Email, password, first name, last name, phone number
   - Optional: Primary address (line1, line2, city, postal code, country)
2. Login at `/login` receives JWT token from backend
3. Token is stored in localStorage
4. Protected routes check authentication status
5. Token is sent in Authorization header for API requests

## API Integration

The frontend communicates with the SharpBuy backend API. Key endpoints:

- `POST /users/register` - User registration
- `POST /users/login` - User login (returns JWT token)
- `GET /users/{id}` - Get user details (authenticated)
- `GET /users/permissions` - Get user permissions (authenticated)
- `POST /products` - Add product (authenticated)

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API base URL | `https://localhost:5001` |

## Development

### Code Style

- TypeScript with strict mode enabled
- ESLint for code quality
- Tailwind CSS for styling
- shadcn/ui design system

### Key Libraries

- **Next.js 16** - React framework with App Router
- **React 19** - UI library
- **TypeScript** - Type safety
- **Tailwind CSS v4** - Utility-first CSS
- **Lucide React** - Icon library
- **class-variance-authority** - Component variants
- **clsx** & **tailwind-merge** - Class name utilities

## Backend Requirements

Ensure the SharpBuy backend is running and configured:

1. Start the backend API (usually via Aspire)
2. Verify API is accessible at the configured URL
3. Check CORS is enabled for frontend origin

## Troubleshooting

### API Connection Issues

- Verify backend is running
- Check `NEXT_PUBLIC_API_URL` in `.env.local`
- Ensure CORS is configured on backend
- Check browser console for specific errors

### Authentication Not Working

- Clear localStorage and try logging in again
- Verify JWT token is being returned from login endpoint
- Check token expiration settings in backend

### Build Errors

- Delete `.next` folder and `node_modules`
- Run `npm install` again
- Ensure Node.js version is 20+

## Production Deployment

When deploying to production:

1. Set `NEXT_PUBLIC_API_URL` to production API URL
2. Build the application: `npm run build`
3. Deploy the `.next` output folder
4. Ensure environment variables are set in hosting platform

## License

This project is part of the SharpBuy e-commerce platform.
