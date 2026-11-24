import { Organization, Branch, Order, OrderItem } from '@/types';

// Test data
const testOrganization: Organization = {
  id: 'org-1',
  name: 'Test Cafe',
  description: 'Test organization'
};

const testBranch: Branch = {
  id: 'branch-1',
  organizationId: 'org-1',
  name: 'Test Branch',
  address: '123 Test St'
};

const testOrderItem: OrderItem = {
  id: 'item-1',
  productId: 'prod-1',
  quantity: 2,
  unitPrice: 15.5
};

const testOrder: Order = {
  id: 'order-1',
  organizationId: 'org-1',
  branchId: 'branch-1',
  status: 'draft',
  total: 31.0,
  created_at: new Date().toISOString(),
  items: [testOrderItem]
};

console.log('Types are correctly imported and defined');
console.log('Organization:', testOrganization);
console.log('Branch:', testBranch);
console.log('Order Item:', testOrderItem);
console.log('Order:', testOrder);