# Payment Integration for Customer Users

## Overview
The payment functionality has been integrated into the **WibuHub.MVC.Customer** project to allow regular users (customers) to make payments using MoMo e-wallet.

## Architecture

### Two Separate Projects
- **WibuHub** (Admin Project) - For admin users managing the system
- **WibuHub.MVC.Customer** (Customer Project) - For regular users browsing and purchasing

The payment functionality is now available in **both projects** with identical features but separate namespaces.

## Customer Project Structure

```
WibuHub.MVC.Customer/
├── Controllers/
│   └── PaymentController.cs         # Handles payment requests
├── ViewModels/
│   └── ShoppingCart/
│       ├── Cart.cs                   # Shopping cart model
│       └── CartItem.cs               # Individual cart item
├── ExtensionsMethod/
│   └── SessionExtensions.cs         # Session helper methods
├── Views/
│   ├── Payment/
│   │   ├── Index.cshtml             # Payment method selection
│   │   ├── Success.cshtml           # Payment success page
│   │   └── Error.cshtml             # Payment error page
│   └── ShoppingCart/
│       ├── Index.cshtml             # Shopping cart
│       └── Checkout.cshtml          # Checkout page
├── Program.cs                        # App configuration with Session & HttpClient
└── appsettings.json                 # MoMo configuration
```

## Features for Customers

### 1. Shopping Cart
- Session-based cart management
- Add/update/remove items
- View total price
- Navigate to checkout

### 2. Checkout Process
- Review cart items
- Enter customer information
- View order summary
- Proceed to payment

### 3. Payment
- Select MoMo e-wallet payment method
- Secure redirect to MoMo gateway
- Complete payment via MoMo app or QR code
- Return to success or error page

### 4. Security Features
- **[Authorize]** attribute - Only authenticated users can access payment
- **CSRF Protection** - Anti-forgery token validation
- **Input Validation** - Amount and order info validation
- **Session Management** - 60-minute session timeout
- **Error Handling** - User-friendly error messages with logging

## Configuration

### appsettings.json
```json
{
  "ApiSettings": {
    "BaseUrl": ""  // Optional: Configure for production
  },
  "MomoSettings": {
    "AccessKey": "F8BBA842ECF85",
    "SecretKey": "K951B6PE1waDMi640xX08PD3vg6EkVlz",
    "PartnerCode": "MOMO",
    "RedirectUrl": "https://localhost:7041/payment/success",
    "IpnUrl": "https://your-domain.com/api/payments/momo/callback",
    "RequestType": "payWithMethod",
    "ApiEndpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
    "QueryEndpoint": "https://test-payment.momo.vn/v2/gateway/api/query",
    "Lang": "vi",
    "AutoCapture": true
  }
}
```

### Important Notes
⚠️ **Production Deployment:**
1. Update `RedirectUrl` to your production domain
2. Update `IpnUrl` to your production callback URL
3. Move MoMo credentials to secure storage (Azure Key Vault, AWS Secrets Manager)
4. Set `ApiSettings.BaseUrl` to your API server URL
5. Never commit production credentials to source control

## Usage

### For Customers

1. **Browse and Add to Cart**
   - Navigate to stories/chapters
   - Add items to shopping cart
   - Cart stored in session

2. **Checkout**
   - Navigate to `/ShoppingCart/Checkout`
   - Review order details
   - Enter customer information

3. **Payment**
   - Navigate to `/Payment`
   - Select MoMo payment method
   - Click "Proceed to Payment"
   - Redirected to MoMo gateway

4. **Complete Payment**
   - Scan QR code with MoMo app
   - Or use MoMo deeplink
   - Complete payment in MoMo ecosystem

5. **Return to Site**
   - Success: Redirected to `/Payment/Success`
   - Error: Redirected to `/Payment/Error`

## API Integration

The Customer project calls the Payment API (in WibuHub.API project):

```
POST /api/payments/momo
{
  "amount": 10000,
  "orderInfo": "Payment for WibuHub Order"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "payUrl": "https://test-payment.momo.vn/...",
    "qrCodeUrl": "https://...",
    "deeplink": "momo://..."
  }
}
```

## Development

### Running Locally

1. **Start the API project:**
   ```bash
   cd WibuHub.API
   dotnet run
   ```

2. **Start the Customer project:**
   ```bash
   cd WibuHub.MVC.Customer
   dotnet run
   ```

3. **Access the application:**
   - Customer site: https://localhost:[port]
   - Navigate to `/Payment` (after adding items to cart)

### Testing Payment Flow

1. Add items to shopping cart (implementation needed)
2. Navigate to checkout
3. Proceed to payment
4. Test with MoMo sandbox credentials
5. Verify success/error handling

## Differences from Admin Project

| Feature | Admin Project | Customer Project |
|---------|--------------|------------------|
| Namespace | `WibuHub.MVC` | `WibuHub.MVC.Customer` |
| Purpose | Admin management | Customer shopping |
| Payment Access | Admins only | Regular users |
| Project File | `WibuHub.MVC.Admin.csproj` | `WibuHub.MVC.Customer.csproj` |

## Security Considerations

### Authentication
- `[Authorize]` attribute requires users to be logged in
- Session-based cart is user-specific
- Payment processing requires authentication

### Input Validation
- Amount: Must be positive and ≤ 1 billion VND
- Order Info: Required, max 200 characters
- CSRF tokens validated on form submissions

### Configuration Security
- **Never commit credentials** to source control
- Use User Secrets for local development
- Use Key Vault or environment variables for production
- Validate API base URLs against whitelist

## Troubleshooting

### Common Issues

**Issue**: "Your cart is empty"
- **Solution**: Add items to cart before accessing payment page

**Issue**: Payment fails immediately
- **Solution**: Check API server is running and accessible

**Issue**: Redirected to localhost in production
- **Solution**: Update `RedirectUrl` in production configuration

**Issue**: MoMo callback not received
- **Solution**: Ensure `IpnUrl` is publicly accessible via HTTPS

### Logging

Check logs for detailed error information:
- Payment creation failures
- API communication errors
- Error response parsing issues

## Next Steps

### Implementation Tasks
1. **Shopping Cart Controller** - Implement add/update/remove actions
2. **Order Processing** - Create order records after successful payment
3. **Email Notifications** - Send confirmation emails
4. **User Dashboard** - Show payment history
5. **Admin Dashboard** - View all customer payments

### Enhancements
1. Multiple payment methods (bank transfer, credit card)
2. Payment installments
3. Wallet balance
4. Discount codes
5. Order tracking

## Support

For issues or questions:
- Check application logs for detailed errors
- Review MoMo API documentation
- Contact development team
- Check `MOMO_INTEGRATION.md` for API details
