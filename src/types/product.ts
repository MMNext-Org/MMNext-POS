export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stock: number;
  provisional_quantity: number;
  organizationId: string;
  branchId: string;
}