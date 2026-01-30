# Two-Tier Payment System Architecture - Implementation Guide

## Tổng quan (Overview)

Hệ thống thanh toán WibuHub được thiết kế theo kiến trúc hai tầng (Two-Tier Architecture):

1. **Tầng Admin**: Quản lý cấu hình phương thức thanh toán
2. **Tầng User**: Sử dụng và thực hiện giao dịch

## Kiến trúc Database

### 1. Bảng PaymentMethods (Master Data - Admin quản lý)

Bảng này lưu trữ danh sách các phương thức thanh toán mà hệ thống hỗ trợ.

```sql
CREATE TABLE PaymentMethods (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,           -- Tên phương thức: "Ví MoMo", "VNPay"
    Code VARCHAR(50) NOT NULL UNIQUE,       -- Mã định danh: "MOMO", "VNPAY"
    IsActive BIT NOT NULL DEFAULT 1,        -- Trạng thái kích hoạt
    LogoUrl NVARCHAR(500),                  -- URL logo
    DisplayOrder INT NOT NULL DEFAULT 0,    -- Thứ tự hiển thị
    Description NVARCHAR(500)               -- Mô tả
)
```

**Dữ liệu mẫu:**
| Id | Name | Code | IsActive | LogoUrl | DisplayOrder |
|----|------|------|----------|---------|--------------|
| 1 | Ví MoMo | MOMO | 1 | https://... | 1 |
| 2 | VNPay | VNPAY | 0 | https://... | 2 |
| 3 | Chuyển khoản ngân hàng | BANK | 1 | NULL | 3 |

### 2. Bảng Transactions (Lịch sử giao dịch - User)

Bảng này lưu trữ lịch sử tất cả các giao dịch thanh toán của người dùng.

```sql
CREATE TABLE Transactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(450) NOT NULL,                -- Foreign key to User
    OrderId UNIQUEIDENTIFIER NULL,                 -- Foreign key to Orders
    Amount MONEY NOT NULL,                         -- Số tiền giao dịch
    PaymentMethodId INT NOT NULL,                  -- Foreign key to PaymentMethods
    ExternalTransactionId NVARCHAR(200),           -- Mã GD từ cổng thanh toán (VD: MoMo TransId)
    Status NVARCHAR(50) NOT NULL,                  -- Success, Failed, Pending, Cancelled
    OrderInfo NVARCHAR(500),                       -- Thông tin đơn hàng
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    Metadata NVARCHAR(MAX),                        -- JSON metadata
    
    CONSTRAINT FK_Transactions_PaymentMethods FOREIGN KEY (PaymentMethodId) 
        REFERENCES PaymentMethods(Id),
    CONSTRAINT FK_Transactions_Orders FOREIGN KEY (OrderId) 
        REFERENCES Orders(Id) ON DELETE SET NULL
)
```

**Indexes:**
```sql
CREATE INDEX IX_Transactions_UserId ON Transactions(UserId)
CREATE INDEX IX_Transactions_CreatedAt ON Transactions(CreatedAt)
CREATE INDEX IX_Transactions_Status ON Transactions(Status)
```

### 3. Bảng Orders (Cập nhật)

Bảng Orders được bổ sung foreign key tới PaymentMethods.

```sql
ALTER TABLE Orders
ADD PaymentMethodId INT NULL,
CONSTRAINT FK_Orders_PaymentMethods FOREIGN KEY (PaymentMethodId) 
    REFERENCES PaymentMethods(Id)
```

**Lưu ý:** Trường `PaymentMethod` (string) được giữ lại cho backward compatibility, nhưng nên sử dụng `PaymentMethodId` cho các tính năng mới.

## Chức năng Admin

### 1. Quản lý Payment Methods

Admin có thể thực hiện các thao tác sau:

- **Xem danh sách**: GET `/PaymentMethods`
- **Thêm mới**: GET `/PaymentMethods/Create` + POST
- **Chỉnh sửa**: GET `/PaymentMethods/Edit/{id}` + POST
- **Xóa**: GET `/PaymentMethods/Delete/{id}` + POST
- **Bật/Tắt**: POST `/PaymentMethods/ToggleActive/{id}`
- **Xem chi tiết**: GET `/PaymentMethods/Details/{id}`

### 2. Quy tắc nghiệp vụ

1. **Mã phương thức (Code) phải duy nhất**
   - Hệ thống sẽ validate khi tạo mới hoặc chỉnh sửa
   - Nên viết HOA và không dấu (VD: MOMO, VNPAY, BANK)

2. **Không thể xóa phương thức đã có giao dịch**
   - Hệ thống kiểm tra trong bảng Transactions
   - Nếu có giao dịch, khuyến nghị "Vô hiệu hóa" thay vì xóa

3. **Chỉ phương thức IsActive = true mới hiển thị cho user**
   - Admin có thể tắt tạm thời (VD: MoMo bảo trì)
   - User sẽ không thấy phương thức này khi thanh toán

### 3. UI/UX Features

- Toggle button để bật/tắt nhanh
- Confirmation dialog trước khi xóa
- Success/Error messages
- Hiển thị logo phương thức
- Status badges (Active/Inactive)
- Sorting theo DisplayOrder

## Chức năng User

### 1. Chọn phương thức thanh toán

Khi checkout, user sẽ:
1. Thấy danh sách các phương thức `IsActive = true`
2. Chọn một phương thức
3. Nhấn "Thanh toán" để chuyển đến cổng thanh toán

```csharp
// Query active payment methods
var activePaymentMethods = await _context.PaymentMethods
    .Where(pm => pm.IsActive)
    .OrderBy(pm => pm.DisplayOrder)
    .ToListAsync();
```

### 2. Tạo giao dịch

Khi user thanh toán thành công, hệ thống tạo:

1. **Order record** với `PaymentMethodId`
2. **Transaction record** với đầy đủ thông tin

```csharp
var transaction = new Transaction
{
    UserId = userId,
    OrderId = order.Id,
    Amount = order.TotalAmount,
    PaymentMethodId = paymentMethodId,
    ExternalTransactionId = momoTransId, // Từ MoMo API
    Status = "Success",
    OrderInfo = "Payment for Order #12345",
    CreatedAt = DateTime.UtcNow
};

await _context.Transactions.AddAsync(transaction);
await _context.SaveChangesAsync();
```

## Security Best Practices

### 1. Lưu trữ thông tin nhạy cảm

**⚠️ KHÔNG NÊN** lưu các thông tin sau vào database:
- MoMo SecretKey, AccessKey
- VNPay HashSecret
- API Keys của các cổng thanh toán

**✅ NÊN** lưu trong:
- `appsettings.json` (cho dev/staging)
- Environment Variables (cho production)
- Azure Key Vault / AWS Secrets Manager (cho production)

```json
// appsettings.json
{
  "MomoSettings": {
    "AccessKey": "F8BBA842ECF85",
    "SecretKey": "K951B6PE1waDMi640xX08PD3vg6EkVlz",
    "PartnerCode": "MOMO",
    "ApiEndpoint": "https://test-payment.momo.vn/v2/gateway/api/create"
  }
}
```

### 2. Authorization

```csharp
[Authorize(Roles = "Admin")]
public class PaymentMethodsController : Controller
{
    // Chỉ Admin mới truy cập được
}
```

### 3. Validation

- Anti-forgery tokens trên tất cả forms
- Unique constraint trên Code
- Check foreign key trước khi xóa

## Migration Guide

### 1. Tạo Migration

```bash
dotnet ef migrations add AddPaymentMethodsAndTransactions --project WibuHub.DataLayer --startup-project WibuHub.API
```

### 2. Update Database

```bash
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub.API
```

### 3. Seed Data

Dữ liệu mẫu sẽ tự động được thêm vào khi chạy migration lần đầu:
- MoMo (Active)
- VNPay (Inactive)
- Bank Transfer (Active)
- ZaloPay (Inactive)
- Visa/MasterCard (Inactive)

## Testing Checklist

### Admin Tests
- [ ] Tạo payment method mới
- [ ] Edit payment method
- [ ] Toggle active/inactive
- [ ] Không thể xóa method đã có transaction
- [ ] Code phải unique
- [ ] Hiển thị đúng thứ tự (DisplayOrder)

### User Tests
- [ ] Chỉ hiển thị active payment methods
- [ ] Tạo transaction khi thanh toán thành công
- [ ] Lưu đúng PaymentMethodId vào Order
- [ ] Lưu đúng ExternalTransactionId từ cổng thanh toán

## API Endpoints (For Reference)

### Admin API (Internal)
- GET `/PaymentMethods` - List all
- POST `/PaymentMethods/Create` - Create new
- POST `/PaymentMethods/Edit/{id}` - Update
- POST `/PaymentMethods/Delete/{id}` - Delete
- POST `/PaymentMethods/ToggleActive/{id}` - Toggle status

### User API (Public)
- GET `/api/payment-methods/active` - Get active methods only

## Troubleshooting

### Issue: "Cannot delete payment method"
**Cause:** Payment method đã có transactions sử dụng  
**Solution:** Vô hiệu hóa (IsActive = false) thay vì xóa

### Issue: "Code already exists"
**Cause:** Mã payment method bị trùng  
**Solution:** Sử dụng mã khác hoặc update existing record

### Issue: "User không thấy payment method"
**Cause:** IsActive = false  
**Solution:** Admin vào bật IsActive = true

## Future Enhancements

1. **Multi-currency support**: Thêm trường Currency
2. **Transaction fees**: Thêm trường Fee, FeePercentage
3. **Payment method logos**: Upload logo thay vì URL
4. **Statistics**: Dashboard hiển thị tỷ lệ sử dụng các payment methods
5. **Audit logs**: Track changes to payment methods
6. **Scheduled tasks**: Auto-disable methods khi cổng thanh toán có vấn đề

## Contact & Support

For issues or questions, refer to:
- Main documentation: `MOMO_INTEGRATION.md`
- Security guide: `MOMO_SECURITY.md`
- Customer guide: `CUSTOMER_PAYMENT_GUIDE.md`
