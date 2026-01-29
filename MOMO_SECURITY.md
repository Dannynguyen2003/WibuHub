# MoMo Configuration Security Notes

## Important: Credentials Management

The MoMo credentials in `appsettings.json` are **TEST CREDENTIALS** provided by MoMo for development purposes.

**NEVER commit production credentials to source control.**

## For Development

Use ASP.NET Core User Secrets for local development:

```bash
cd WibuHub.API
dotnet user-secrets init
dotnet user-secrets set "MomoSettings:AccessKey" "YOUR_ACCESS_KEY"
dotnet user-secrets set "MomoSettings:SecretKey" "YOUR_SECRET_KEY"
```

## For Production

Use one of these secure methods:

### Option 1: Environment Variables
Set environment variables:
- `MomoSettings__AccessKey`
- `MomoSettings__SecretKey`
- `MomoSettings__RedirectUrl`
- `MomoSettings__IpnUrl`

### Option 2: Azure Key Vault (Recommended for Azure deployments)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Option 3: Configuration Files (with restricted access)
Create `appsettings.Production.json` with proper file permissions and exclude from source control.

## URLs Configuration

Update these URLs before deployment:
- `RedirectUrl`: Where users are redirected after payment (e.g., `https://wibuhub.com/payment/success`)
- `IpnUrl`: Callback URL for MoMo notifications (e.g., `https://wibuhub.com/api/payments/momo/callback`)

**Important:** The IpnUrl must be:
- Publicly accessible (no localhost)
- Using HTTPS in production
- Added to MoMo's whitelist in your merchant portal

## Production Credentials

To get production credentials:
1. Register at https://business.momo.vn/
2. Complete merchant verification
3. Obtain production AccessKey and SecretKey
4. Update API endpoints to production URLs
