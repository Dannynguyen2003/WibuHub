# MoMo Payment Integration - User Guide

## Overview
This guide explains how to use the MoMo payment feature in WibuHub.

## Payment Flow

### 1. Shopping Cart
Users can add chapters to their shopping cart and manage quantities:
- **URL**: `/ShoppingCart/Index`
- **Features**:
  - View all items in cart
  - Update item quantities
  - Remove items
  - See total price
  - Navigate to checkout

### 2. Checkout
Review order details and enter customer information:
- **URL**: `/ShoppingCart/Checkout`
- **Features**:
  - Review all cart items
  - Enter customer details (name, phone, email, address)
  - Add order notes
  - View order summary
  - Navigate to payment

### 3. Payment Method Selection
Choose payment method (currently MoMo):
- **URL**: `/Payment/Index`
- **Features**:
  - Select MoMo e-wallet payment
  - View order summary
  - Proceed to secure payment gateway

### 4. MoMo Payment Gateway
User is redirected to MoMo's secure payment page:
- Scan QR code with MoMo app
- Or pay via MoMo app deeplink
- Complete payment within MoMo ecosystem

### 5. Payment Result
After payment, user is redirected back:
- **Success**: `/Payment/Success` - Shows success message with next steps
- **Error**: `/Payment/Error` - Shows error message with retry options

## For Developers

### Files Structure
```
WibuHub/
├── Controllers/
│   └── PaymentController.cs          # Handles payment requests
├── Views/
│   ├── Payment/
│   │   ├── Index.cshtml              # Payment method selection
│   │   ├── Success.cshtml            # Success page
│   │   └── Error.cshtml              # Error page
│   └── ShoppingCart/
│       ├── Index.cshtml              # Cart page
│       └── Checkout.cshtml           # Checkout page
└── appsettings.json                  # MoMo configuration
```

### Configuration

Update `appsettings.json` with your environment-specific URLs:

```json
{
  "MomoSettings": {
    "RedirectUrl": "https://your-domain.com/payment/success",
    "IpnUrl": "https://your-domain.com/api/payments/momo/callback"
  }
}
```

**Important Notes:**
- For local development, update `RedirectUrl` to match your local port
- For production, use your actual domain with HTTPS
- The `IpnUrl` must be publicly accessible for MoMo callbacks

### Security Features

1. **Input Validation**
   - Payment amount validated (must be positive, max 1 billion VND)
   - Order info length limited to 200 characters
   - CSRF token validation enabled

2. **Error Handling**
   - Proper error messages extracted from API responses
   - JavaScript error handling with user notifications
   - Logging of all errors for debugging

3. **Session Management**
   - Cart stored in session
   - Cart validation before payment
   - Session timeout configured (60 minutes)

### Testing

**Local Testing:**
1. Start the application:
   ```bash
   cd WibuHub
   dotnet run
   ```

2. Navigate to `/ShoppingCart/Index`

3. Add items to cart (requires ShoppingCartController implementation)

4. Proceed through checkout to payment

5. Test with MoMo sandbox credentials

**Production Deployment:**
- Update MoMo credentials in secure configuration (user secrets/environment variables)
- Configure proper redirect URLs
- Enable HTTPS
- Test callback endpoint accessibility

### Common Issues

**Issue**: "Your cart is empty" when accessing payment page
- **Solution**: Add items to cart first via ShoppingCart/Index

**Issue**: Payment redirects to localhost in production
- **Solution**: Update `RedirectUrl` in production configuration

**Issue**: MoMo callback fails
- **Solution**: Ensure `IpnUrl` is publicly accessible and using HTTPS

**Issue**: "Unable to process payment" error
- **Solution**: Check API logs for detailed error messages

### API Integration

The PaymentController calls the MoMo Payment API at:
- **Endpoint**: `POST /api/payments/momo`
- **Request**:
  ```json
  {
    "amount": 10000,
    "orderInfo": "Payment for WibuHub Order"
  }
  ```
- **Response** (Success):
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

### Next Steps

1. **Complete ShoppingCart Implementation**
   - Implement AddToCart, UpdateQuantity, RemoveCartItem actions
   - Connect to database for persistent cart storage

2. **Add Order Processing**
   - Create Order record after successful payment
   - Send confirmation emails
   - Update user's purchased chapters

3. **Enhance Payment Options**
   - Add bank transfer option
   - Add credit card payment
   - Add wallet balance payment

4. **Improve UI/UX**
   - Add loading indicators during API calls
   - Add payment progress tracking
   - Add order history page

## Support

For issues or questions:
- Check API logs for detailed error messages
- Review MoMo integration documentation
- Contact development team
