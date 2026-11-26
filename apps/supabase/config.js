/**
 * Supabase Configuration
 * 
 * This file contains configuration settings for the Supabase backend.
 * It is intended to be used with the Supabase JavaScript client.
 * 
 * Replace the placeholder values with actual Supabase project URL and anon key.
 * 
 * Important: Ensure that JWT tokens issued by Supabase include a claim
 * `organization_id` that matches the user's organization. This claim
 * is used by Row Level Security (RLS) policies to enforce tenant isolation.
 */

const supabaseConfig = {
  // Supabase project URL (e.g., https://your-project.europe-west1.supabase.co)
  supabaseUrl: process.env.SUPABASE_URL || 'https://your-project.supabase.co',

  // Anon public key or service role key (choose appropriate)
  anonKey: process.env.SUPABASE_ANON_KEY || 'your-anon-key',
  
  // Service role key (for server-side operations)
  serviceRoleKey: process.env.SUPABASE_SERVICE_ROLE_KEY || 'your-service-role-key',
  
  // Optional: Force the client to use the service role key for certain operations
  // (requires proper security considerations)
  useServiceRoleKey: false,
  
  // JWT Claim Configuration
  // The client will automatically attach the `organization_id` claim if present in the token.
  // Ensure your Supabase Auth settings include this claim in the token payload.
  organizationClaim: 'organization_id',
};

module.exports = supabaseConfig;