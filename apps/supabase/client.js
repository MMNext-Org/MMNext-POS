const { createClient } = require('@supabase/supabase-js');
const supabaseConfig = require('./config');

// Create Supabase client instance
const supabase = createClient(supabaseConfig.supabaseUrl, supabaseConfig.anonKey);

// Optionally, you can extend the client with custom headers or interceptors
// For example, to attach the organization_id claim from JWT
// supabase.auth.on('session', (event, session) => {
//   if (session) {
//     // You can attach additional logic here if needed
//   }
// });

module.exports = supabase;