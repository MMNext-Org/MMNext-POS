import { supabase } from '@/utils/supabase';

async function seed() {
  // Create sample organization and branch if not exists
  const org = await supabase
    .from('organizations')
    .upsert({ id: 'org-1', name: 'Central Café', description: 'Main location' })
    .select();

  const branch = await supabase
    .from('branches')
    .upsert({ id: 'branch-1', organizationId: 'org-1', name: 'Main Street', address: '123 Main St' })
    .select();

  // Insert sample products with stock
  const sampleProducts = [
    {
      id: 'prod-1',
      name: 'Espresso',
      description: 'Shot of espresso',
      price: 3.5,
      stock: 100,
      organizationId: 'org-1',
      branchId: 'branch-1',
    },
    {
      id: 'prod-2',
      name: 'Latte',
      description: 'Milk coffee',
      price: 4.0,
      stock: 80,
      organizationId: 'org-1',
      branchId: 'branch-1',
    },
    {
      id: 'prod-3',
      name: 'Croissant',
      description: 'Butter croissant',
      price: 2.5,
      stock: 50,
      organizationId: 'org-1',
      branchId: 'branch-1',
    },
  ];

  const { error } = await supabase
    .from('products')
    .upsert(sampleProducts);

  if (error) {
    console.error('Error seeding products:', error);
  } else {
    console.log('Seed products inserted successfully');
  }
}

seed();