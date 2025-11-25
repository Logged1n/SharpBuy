# SharpBuy Client

Next.js frontend client for the SharpBuy e-commerce platform.

## Tech Stack

- **Next.js 15** with App Router
- **TypeScript**
- **Tailwind CSS** with shadcn/ui
- **pnpm** for package management

## Features

- User authentication (login, register, email verification)
- Product catalog with search and filtering
- Shopping cart functionality
- User profile management
- Responsive design

## Getting Started

### Prerequisites

- Node.js 18+
- pnpm 8+

### Installation

```bash
# Install dependencies
pnpm install

# Run development server
pnpm dev
```

The app will be available at http://localhost:3000

### Environment Variables

Create a `.env.local` file:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

When running with Aspire, this is automatically configured.

## Project Structure

```
client/
├── src/
│   ├── app/              # Next.js App Router pages
│   ├── components/       # React components
│   │   └── ui/          # shadcn/ui components
│   ├── contexts/        # React contexts (auth)
│   ├── hooks/           # Custom hooks (cart)
│   ├── lib/             # Utilities and API client
│   └── types/           # TypeScript types
├── public/              # Static assets
└── package.json
```

## Available Scripts

- `pnpm dev` - Start development server
- `pnpm build` - Build for production
- `pnpm start` - Start production server
- `pnpm lint` - Run ESLint

## API Integration

The client communicates with the .NET Web API backend. The API client is located in `src/lib/api-client.ts` and handles:

- JWT authentication
- Request/response handling
- Error handling

## Authentication

JWT tokens are stored in localStorage and automatically included in API requests via the Authorization header.
