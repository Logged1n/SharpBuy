# SharpBuy Client - Changelog

## Updates - 2025-11-26

### Updated Registration to Match Backend User Model

#### Changes Made:

1. **Updated API Types** ([lib/api.ts](lib/api.ts))
   - Added `AddressDto` interface matching backend `SharedKernel.Dtos.AddressDto`
   - Updated `RegisterRequest` interface to include all required backend fields:
     - `phoneNumber` (required)
     - `primaryAddress` (optional)
     - `additionalAddresses` (optional)

2. **Enhanced Registration Form** ([app/register/page.tsx](app/register/page.tsx))
   - Added phone number field (required)
   - Added optional address section with fields:
     - Address Line 1
     - Address Line 2 (optional)
     - City
     - Postal Code
     - Country
   - Address is only sent to backend if all required address fields are filled
   - Increased card width from `max-w-md` to `max-w-2xl` to accommodate new fields

3. **Configuration Updates**
   - Updated default API URL from `https://localhost:7001` to `https://localhost:5001`
   - Updated [SETUP.md](SETUP.md) documentation to reflect correct port and new registration fields
   - Updated `.env.local` to use port 5001

#### Backend Compatibility:

The registration form now sends data matching the backend `RegisterUserCommand`:

```csharp
public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string PhoneNumber,
    AddressDto? PrimaryAddress,
    IReadOnlyCollection<AddressDto>? AdditionalAddresses
) : ICommand<Guid>;
```

#### User Experience:

- Phone number is now a required field during registration
- Address information is optional but recommended
- Form validates all fields before submission
- Clear separation between required fields and optional address section
- Improved layout with two-column grid for city/postal code

#### API Request Example:

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "primaryAddress": {
    "line1": "123 Main St",
    "line2": "Apt 4B",
    "city": "New York",
    "postalCode": "10001",
    "country": "United States"
  },
  "additionalAddresses": null
}
```

## Previous Features

All original features remain intact:
- Landing page with hero and call-to-action
- Navbar with authentication state
- Login page with JWT authentication
- Admin dashboard with product management
- Products and categories pages
- Protected routes with authentication guard
- Full TypeScript type safety
- Responsive design with Tailwind CSS
