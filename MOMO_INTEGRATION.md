# MoMo Payment Integration

This document describes the MoMo payment integration for WibuHub.

## Overview

The MoMo payment integration allows customers to pay for chapters using MoMo e-wallet. The implementation follows the MoMo API v2 specifications and handles:

- Payment request creation
- Callback handling for payment status updates
- Transaction status checking

## Architecture

### Components

1. **DTOs** (`WibuHub.ApplicationCore/DTOs/Shared/`)
   - `MomoPaymentRequest.cs` - Request to create a payment
   - `MomoPaymentResponse.cs` - Response from MoMo API
   - `MomoCallbackRequest.cs` - Callback payload from MoMo
   - `MomoTransactionStatusRequest.cs` - Request to check transaction status

2. **Configuration** (`WibuHub.ApplicationCore/Configuration/`)
   - `MomoSettings.cs` - MoMo configuration settings

3. **Service Layer** (`WibuHub.Service/`)
   - `Interface/IPaymentService.cs` - Payment service interface
   - `Implementations/MomoPaymentService.cs` - MoMo payment implementation

4. **API Layer** (`WibuHub.API/Controllers/`)
   - `PaymentsController.cs` - REST API endpoints for payments

5. **Database**
   - Updated `Order` entity with payment tracking fields:
     - `PaymentMethod` - e.g., "MoMo", "Cash", "Transfer"
     - `TransactionId` - MoMo transaction ID
     - `PaymentStatus` - e.g., "Pending", "Completed", "Failed"

## API Endpoints

### 1. Create MoMo Payment

**Endpoint:** `POST /api/payments/momo`

**Request Body:**
```json
{
  "amount": 10000,
  "orderInfo": "Payment for Chapter purchase",
  "orderId": "optional-order-id",
  "extraData": "optional-extra-data"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Payment created successfully",
  "data": {
    "partnerCode": "MOMO",
    "orderId": "MOMO1234567890",
    "requestId": "MOMO1234567890",
    "amount": 10000,
    "responseTime": 1234567890,
    "message": "Successful",
    "resultCode": 0,
    "payUrl": "https://test-payment.momo.vn/...",
    "deeplink": "momo://...",
    "qrCodeUrl": "https://...",
    "deeplinkMiniApp": "https://..."
  }
}
  }
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Error message",
  "resultCode": -1
}
```

### 2. MoMo Callback (IPN)

**Endpoint:** `POST /api/payments/momo/callback`

This endpoint receives payment notifications from MoMo after a customer completes payment.

**Request Body (from MoMo):**
```json
{
  "partnerCode": "MOMO",
  "orderId": "MOMO1234567890",
  "requestId": "MOMO1234567890",
  "amount": 10000,
  "orderInfo": "Payment for Chapter purchase",
  "orderType": "momo_wallet",
  "transId": 4014083433,
  "resultCode": 0,
  "message": "Successful",
  "payType": "qr",
  "responseTime": 1234567890,
  "extraData": "",
  "signature": "..."
}
```

**Response:** HTTP 204 No Content

**Result Codes:**
- `0` - Payment successful
- `9000` - Payment authorized
- Other values - Payment failed

### 3. Check Transaction Status

**Endpoint:** `GET /api/payments/momo/status/{orderId}`

**Response:**
```json
{
  "success": true,
  "data": {
    "partnerCode": "MOMO",
    "orderId": "MOMO1234567890",
    "resultCode": 0,
    "message": "Successful",
    "amount": 10000,
    ...
  }
}
```

## Configuration

Update `appsettings.json` with your MoMo credentials:

```json
{
  "MomoSettings": {
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "PartnerCode": "MOMO",
    "RedirectUrl": "https://your-domain.com/payment/success",
    "IpnUrl": "https://your-domain.com/api/payments/momo/callback",
    "RequestType": "payWithMethod",
    "ApiEndpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
    "QueryEndpoint": "https://test-payment.momo.vn/v2/gateway/api/query",
    "Lang": "vi",
    "AutoCapture": true
  }
}
```

**Important:** 
- For production, use `https://payment.momo.vn/v2/gateway/api/create`
- The `IpnUrl` must be publicly accessible for MoMo to send callbacks
- Update `RedirectUrl` to your actual success page URL

## Testing

### Local Testing

1. Start the API:
   ```bash
   cd WibuHub.API
   dotnet run
   ```

2. Create a payment request:
   ```bash
   curl -X POST http://localhost:5126/api/payments/momo \
     -H "Content-Type: application/json" \
     -d '{
       "amount": 10000,
       "orderInfo": "Test payment"
     }'
   ```

3. For testing callbacks locally, use ngrok or similar tools to expose your local server.

### Integration Testing

To test with MoMo sandbox:
1. Register for MoMo test credentials at https://developers.momo.vn/
2. Update `appsettings.json` with test credentials
3. Use ngrok to expose your callback URL
4. Create a payment and scan the QR code with MoMo test app

## Security Considerations

1. **Signature Validation**: The current implementation logs callbacks but doesn't validate signatures. For production, implement signature validation in `HandleMomoCallbackAsync`.

2. **HTTPS Required**: MoMo requires HTTPS for IPN URLs in production.

3. **Credentials**: Never commit real credentials to source control. Use environment variables or Azure Key Vault.

4. **Order ID Generation**: The system generates order IDs using timestamps. For production, ensure uniqueness across distributed systems.

## Database Schema

The integration adds the following fields to the `Order` table:

| Field | Type | Description |
|-------|------|-------------|
| PaymentMethod | varchar(50) | Payment method used (e.g., "MoMo") |
| TransactionId | varchar(100) | MoMo transaction ID |
| PaymentStatus | varchar(50) | Payment status (e.g., "Completed") |

## Error Handling

The service handles errors gracefully:
- Network errors return appropriate error messages
- Failed payments are logged
- Callbacks always return 204 to prevent MoMo retries

## Future Enhancements

1. Add signature validation for callbacks
2. Implement webhook retry mechanism
3. Add payment status page for users
4. Support for MoMo installment payments
5. Add comprehensive logging and monitoring
