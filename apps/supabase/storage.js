const { createClient } = require('@supabase/supabase-js');

// Create Supabase client instance
const supabase = createClient(
  process.env.SUPABASE_URL || 'https://your-project.supabase.co',
  process.env.SUPABASE_SERVICE_ROLE_KEY || 'your-service-role-key'
);

// Storage configuration for organization-specific buckets
const storageConfig = {
  // Bucket naming convention: bucket_org_{org_id}
  bucketNamingConvention: 'bucket_org_{org_id}',
  
  // Storage policies configuration
  policies: {
    // Each organization can only access its own bucket
    organizationBucketAccess: {
      // Policy: Users can only access buckets belonging to their organization
      policyFunction: (orgId) => `bucket_id = 'bucket_org_${orgId}'`,
      
      // Enable RLS for storage operations
      enableRLS: true,
      
      // Additional policy conditions
      additionalConditions: [
        // Ensure the user's organization matches the bucket's organization
        'auth.jwt() ->> "organization_id" = $1',
        'auth.role() = "authenticated"'
      ]
    }
  }
};

module.exports = {
  supabase,
  storageConfig
};