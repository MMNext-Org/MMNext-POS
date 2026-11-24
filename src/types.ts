export interface Organization {
  id: string;
  name: string;
  description: string;
}

export interface Branch {
  id: string;
  organizationId: string;
  name: string;
  address: string;
  branch_code: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  organizationId: string;
  branchId: string;
  status: string;
  total: number;
  created_at: string;
  items: OrderItem[];
  // Loyalty / redemption fields (optional — orders can be created without loyalty)
  customerId?: string;
  discountAmount?: number;
  redemptionReference?: string;
  subtotal?: number;
}