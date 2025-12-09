# Generate Certificates

Generating JWT Tokens:
- `symfony console lexik:jwt:generate-keypair` to generate a private.pem and public.pem. Add them to `/config/jwt/` and add the passphrase you chose to `.env.local` (JWT_PASSPHRASE=your_secure_passphrase)