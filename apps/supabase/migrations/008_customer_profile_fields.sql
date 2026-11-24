-- Add customer profile fields: birthday, preferences, consent
ALTER TABLE customers
ADD COLUMN birthday DATE,
ADD COLUMN preferences JSONB DEFAULT '{}'::jsonb,
ADD COLUMN consent BOOLEAN DEFAULT FALSE;

-- Add index on birthday for filtering
CREATE INDEX IF NOT EXISTS idx_customers_birthday ON customers(birthday);

-- Add comment for documentation
COMMENT ON COLUMN customers.birthday IS 'Customer date of birth';
COMMENT ON COLUMN customers.preferences IS 'Customer preferences (JSON)';
COMMENT ON COLUMN customers.consent IS 'Customer consent for data processing and marketing';