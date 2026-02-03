# Hướng Dẫn Quản Lý Roles trong MVC.Admin

## Tổng Quan

Hệ thống quản lý roles đã được tạo trong MVC.Admin portal, cho phép SuperAdmin quản lý các roles trong hệ thống.

## Các Roles Được Tạo Sẵn

### 1. **SuperAdmin** 🔴
- **Mô tả**: Super Administrator with full system access
- **Quyền hạn**: Toàn quyền trên hệ thống
- **Badge**: Đỏ - "Hệ thống"
- **Bảo vệ**: Không thể sửa hoặc xóa

### 2. **ContentManager** 🔵
- **Mô tả**: Manages content: upload and translate stories
- **Quyền hạn**: Upload truyện, dịch truyện
- **Badge**: Xanh dương - "Quản lý nội dung"
- **Có thể chỉnh sửa**: Có (tên và mô tả)

### 3. **StoryManager** 🟢
- **Mô tả**: Manages stories and chapters
- **Quyền hạn**: Quản lý truyện và chapters
- **Badge**: Xanh lá - "Quản lý truyện"
- **Có thể chỉnh sửa**: Có (tên và mô tả)

### 4. **SalesManager** ⚫
- **Mô tả**: Manages sales and orders
- **Quyền hạn**: Quản lý bán hàng và đơn hàng
- **Có thể chỉnh sửa**: Có

### 5. **Admin** 🟡
- **Mô tả**: General Administrator
- **Quyền hạn**: Quản trị viên chung
- **Badge**: Vàng - "Hệ thống"
- **Bảo vệ**: Không thể xóa (có thể sửa mô tả)

### 6. **Customer** 🟡
- **Mô tả**: Regular customer who reads and purchases stories
- **Quyền hạn**: Đọc và mua truyện
- **Badge**: Vàng - "Hệ thống"
- **Bảo vệ**: Không thể xóa

## Cách Truy Cập

### 1. Đăng Nhập Với SuperAdmin
```
URL: https://localhost:5001/Identity/Account/Login
Email: superadmin@wibuhub.com
Password: SuperAdmin@123
```

### 2. Truy Cập Trang Quản Lý Roles
- Sau khi đăng nhập, nhìn lên menu navigation
- Click vào link **"Quản lý Roles"**
- Hoặc truy cập trực tiếp: `https://localhost:5001/Roles`

## Các Chức Năng

### 1. Xem Danh Sách Roles (Index)
**URL**: `/Roles/Index`

**Hiển thị**:
- Bảng danh sách tất cả roles
- Tên role với badges màu sắc
- Mô tả role
- Số lượng người dùng có role đó
- Nút thao tác (Chi tiết, Sửa, Xóa)

**Thông tin bổ sung**:
- Panel giải thích chức năng của từng role
- Messages thành công/lỗi (tự động ẩn sau 5 giây)

### 2. Xem Chi Tiết Role (Details)
**URL**: `/Roles/Details/{roleId}`

**Hiển thị**:
- Thông tin đầy đủ của role
- Danh sách người dùng có role này
- Email và tên đầy đủ của từng user
- Nút quay lại, sửa, xóa (nếu được phép)

### 3. Tạo Role Mới (Create)
**URL**: `/Roles/Create`

**Form gồm**:
- **Tên Role**: Bắt buộc, tối đa 256 ký tự
- **Mô tả**: Tùy chọn, tối đa 500 ký tự

**Validation**:
- Tên role không được trùng
- Tên role không được để trống

**Lưu ý**:
- Role name nên ngắn gọn (ví dụ: Editor, Moderator)
- Nên có mô tả rõ ràng về quyền hạn

### 4. Chỉnh Sửa Role (Edit)
**URL**: `/Roles/Edit/{roleId}`

**Có thể sửa**:
- Tên role (nếu không trùng với role khác)
- Mô tả role

**Không thể sửa**:
- SuperAdmin role (hoàn toàn bảo vệ)

### 5. Xóa Role (Delete)
**URL**: `/Roles/Delete/{roleId}`

**Điều kiện xóa**:
- ✅ Role không phải SuperAdmin, Admin, hoặc Customer
- ✅ Role không có người dùng nào

**Không thể xóa nếu**:
- ❌ Role là hệ thống (SuperAdmin, Admin, Customer)
- ❌ Role có người dùng đang sử dụng

**Confirmation page**:
- Hiển thị thông tin role sẽ xóa
- Cảnh báo nếu có người dùng
- Nút xác nhận xóa (disabled nếu có người dùng)

## Quyền Truy Cập

### Yêu Cầu
- **Chỉ SuperAdmin** mới có quyền truy cập
- Decorator: `[Authorize(Roles = "SuperAdmin")]`
- Người dùng khác sẽ thấy Access Denied

### Navigation Menu
- Link "Quản lý Roles" chỉ hiển thị cho SuperAdmin
- Code check: `@if (User.IsInRole("SuperAdmin"))`

## Ví Dụ Sử Dụng

### Scenario 1: Tạo Role Editor Mới
1. Đăng nhập với SuperAdmin
2. Click "Quản lý Roles" trong menu
3. Click nút "Tạo Role Mới"
4. Nhập:
   - Tên: `Editor`
   - Mô tả: `Có thể chỉnh sửa nội dung truyện`
5. Click "Tạo Role"
6. Thành công! Message: "Đã tạo role 'Editor' thành công"

### Scenario 2: Chỉnh Sửa Mô Tả ContentManager
1. Trong danh sách roles, tìm ContentManager
2. Click nút "Sửa"
3. Sửa mô tả:
   - Cũ: "Manages content: upload and translate stories"
   - Mới: "Quản lý nội dung: Upload và dịch truyện tiếng Việt"
4. Click "Cập Nhật"
5. Thành công!

### Scenario 3: Xem Người Dùng Có Role SuperAdmin
1. Trong danh sách roles, tìm SuperAdmin
2. Click nút "Chi tiết"
3. Xem:
   - Thông tin role
   - Danh sách users có role SuperAdmin
   - Ví dụ: superadmin@wibuhub.com

### Scenario 4: Thử Xóa Role Có User (Không Thành Công)
1. Tạo role test: `TestRole`
2. Gán role cho 1 user
3. Thử xóa TestRole
4. Hệ thống cảnh báo: "Không thể xóa role vì có 1 người dùng đang sử dụng"
5. Nút xóa bị disabled

## Cấu Trúc Code

### Controllers
```
WibuHub/Controllers/RolesController.cs
- Index() - List roles
- Create() - Create form
- Create(CreateRoleVM) - Process create
- Edit(id) - Edit form
- Edit(id, EditRoleVM) - Process edit
- Delete(id) - Delete confirmation
- DeleteConfirmed(id) - Process delete
- Details(id) - Show details
```

### Views
```
WibuHub/Views/Roles/
├── Index.cshtml      - List all roles
├── Create.cshtml     - Create new role
├── Edit.cshtml       - Edit role
├── Delete.cshtml     - Delete confirmation
└── Details.cshtml    - Role details with users
```

### ViewModels
```
WibuHub/ViewModels/RoleVM.cs
- RoleVM           - Display role info
- CreateRoleVM     - Create role form
- EditRoleVM       - Edit role form
- RoleDetailsVM    - Details with users
- UserInRoleVM     - User info in role
```

## UI/UX Features

### Bootstrap 5 Components
- ✅ Cards for information display
- ✅ Tables for role listing
- ✅ Forms with validation
- ✅ Alerts for messages (success/error/warning/info)
- ✅ Badges for role types
- ✅ Buttons with icons
- ✅ Responsive design

### Color Scheme
- 🔴 **Danger (Red)**: SuperAdmin, Delete actions
- 🔵 **Primary (Blue)**: Create actions, user count
- 🟢 **Success (Green)**: StoryManager
- 🟡 **Warning (Yellow)**: System roles, Edit actions
- ⚫ **Info (Dark Blue)**: ContentManager, Details

### Icons (Bootstrap Icons)
- 🔍 `bi-eye` - View details
- ✏️ `bi-pencil` - Edit
- 🗑️ `bi-trash` - Delete
- ✅ `bi-check-circle` - Confirm
- ❌ `bi-x-circle` - Cancel
- ➕ `bi-plus-circle` - Add new
- ⬅️ `bi-arrow-left` - Back
- ⚠️ `bi-exclamation-triangle` - Warning
- ℹ️ `bi-info-circle` - Information

### Validation
- Client-side validation with jQuery Validate
- Server-side validation in controller
- Clear error messages in Vietnamese
- Visual feedback with `asp-validation-for`

## Testing Checklist

### ✅ Completed
- [x] Build successful (0 errors)
- [x] RolesController created with all CRUD actions
- [x] All views created (Index, Create, Edit, Delete, Details)
- [x] ViewModels created for all operations
- [x] Navigation link added (SuperAdmin only)
- [x] System roles protected from deletion
- [x] SuperAdmin role protected from editing
- [x] Role name uniqueness validation
- [x] User count display
- [x] User list in details page
- [x] Success/Error messages
- [x] Vietnamese interface

### 🔄 Ready for Manual Testing
- [ ] Login as SuperAdmin
- [ ] Navigate to Roles page
- [ ] Verify all 6 roles are displayed
- [ ] Create a new custom role
- [ ] Edit ContentManager description
- [ ] Try to edit SuperAdmin (should fail)
- [ ] View role details
- [ ] Try to delete role with users (should fail)
- [ ] Delete empty custom role
- [ ] Verify badges and colors
- [ ] Test responsive design
- [ ] Test validation errors

## Troubleshooting

### Issue: Cannot see "Quản lý Roles" in menu
**Solution**: Make sure you're logged in as SuperAdmin

### Issue: Access Denied when accessing /Roles
**Solution**: Only SuperAdmin can access. Check user role.

### Issue: Cannot delete a role
**Possible causes**:
1. Role is system role (SuperAdmin, Admin, Customer)
2. Role has users assigned to it
3. Remove all users from role first, then delete

### Issue: Cannot edit role name
**Solution**: Make sure new name doesn't conflict with existing role

## Summary

✅ **Hoàn thành**: Role management UI for MVC.Admin
✅ **Roles được hiển thị**: SuperAdmin, ContentManager, StoryManager, SalesManager, Admin, Customer
✅ **Chức năng**: Create, Read, Update, Delete roles
✅ **Bảo vệ**: System roles không thể xóa
✅ **Giao diện**: Tiếng Việt, Bootstrap 5, Responsive
✅ **Quyền truy cập**: Chỉ SuperAdmin

Hệ thống đã sẵn sàng để sử dụng! 🎉
