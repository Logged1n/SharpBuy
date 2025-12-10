# Stripe Integration Setup

## Install Required Packages

Run the following command in the sharp-buy-client directory:

```bash
npm install @stripe/stripe-js @stripe/react-stripe-js
```

## Environment Variables

Add to `.env.local`:

```env
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_your_publishable_key_here
```

Get your publishable key from: https://dashboard.stripe.com/test/apikeys

## Backend Configuration

Add to `appsettings.Development.json`:

```json
"Stripe": {
  "SecretKey": "sk_test_your_secret_key_here",
  "PublishableKey": "pk_test_your_publishable_key_here"
}
```
