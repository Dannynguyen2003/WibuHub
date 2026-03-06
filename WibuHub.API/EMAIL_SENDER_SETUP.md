# Brevo EmailSender setup

`NotificationController` uses `ICustomEmailSender` (`MimeKitEmailSender`) and reads SMTP settings from `EmailSettings`.

## 1) Brevo SMTP values

- Host: `smtp-relay.brevo.com`
- Port: `587` (STARTTLS)
- SenderEmail: verified sender email in Brevo
- Username: your Brevo SMTP login
- Password: your Brevo SMTP key

## 2) Configure with user-secrets (recommended)

From `/WibuHub.API`:

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SenderEmail" "your-verified-sender@example.com"
dotnet user-secrets set "EmailSettings:Username" "your-brevo-login@example.com"
dotnet user-secrets set "EmailSettings:Password" "your-brevo-smtp-key"
```

You can also set environment variables:

- `EmailSettings__SenderEmail`
- `EmailSettings__Username`
- `EmailSettings__Password`
