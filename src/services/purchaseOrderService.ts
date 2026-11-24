import { supabase } from '../supabaseClient';
import { v4 as uuidv4 } from 'uuid';

export interface PurchaseOrder {
  id: string;
  organization_id: string;
  supplier_id: string;
  status: 'draft' | 'pending' | 'confirmed' | 'received' | 'cancelled';
  created_at: string;
  updated_at: string;
}

export interface PurchaseOrderItem {
  id: string;
  purchase_order_id: string;
  supplier_item_id: string; // ingredient id
  quantity: number;
  unit_price: number;
}

export const PurchaseOrderService = {
  async createPurchaseOrder(organizationId: string, supplierId: string): Promise<string> {
    const { data, error } = await supabase
      .from('purchase_orders')
      .insert({
        organization_id: organizationId,
        supplier_id: supplierId,
        status: 'draft',
      })
      .select('id')
      .single();

    if (error) throw error;
    return data.id;
  },

  async addItem(poId: string, item: PurchaseOrderItem): Promise<void> {
    const { error } = await supabase
      .from('purchase_order_items')
      .insert({
        purchase_order_id: poId,
        organization_id: item.organization_id,
        supplier_item_id: item.supplier_item_id,
        quantity: item.quantity,
        unit_price: item.unit_price,
      });

    if (error) throw error;
  },

  async receiveGoods(poId: string, receiptItems: { supplier_item_id: string; quantity: number }[], receiptId: string): Promise<void> {
    // Insert goods receipt items
    const items = receiptItems.map(item => ({
      goods_receipt_id: receiptId,
      organization_id: 'org_placeholder', // replace with actual org context
      ingredient_id: item.supplier_item_id,
      quantity: item.quantity,
      unit_price: 0,
    }));

    const { error: itemsError } = await supabase
      .from('goods_receipt_items')
      .insert(items);

    if (itemsError) throw itemsError;

    // Reconcile stock: for each item, create a stock movement 'in' and adjust inventory
    for (const { supplier_item_id, quantity } of receiptItems) {
      // Get product_variant_id for ingredient (simplified)
      const { data: productData, error: prodError } = await supabase
        .from('product_variants')
        .select('id')
        .eq('ingredient_id', supplier_item_id)
        .single();

      if (prodError) throw prodError;
      const productVariantId = productData.id;

      // Create stock movement
      await supabase
        .from('stock_movements')
        .insert({
          organization_id: 'org_placeholder',
          product_id: productVariantId,
          delta: quantity,
          reason: 'goods_receipt',
          user_id: 'current_user_id',
          transaction_id: receiptId,
          status: 'pending',
        });

      // Update inventory stock (example: using a view or direct table)
      // Assuming there's a stock table; for now just log or trigger a procedure
      // await supabase.rpc('update_stock_on_receipt', { product_id: productVariantId, quantity });
    }

    // Mark receipt as received
    const { error: updateError } = await supabase
      .from('goods_receipts')
      .update({ status: 'received' })
      .eq('id', receiptId);

    if (updateError) throw updateError;
  },

  async handleDiscrepancy(goodsReceiptId: string, expectedQty: number, actualQty: number): Promise<void> {
    const delta = actualQty - expectedQty;
    // Record a discrepancy stock movement (adjustment)
    await supabase
      .from('stock_movements')
      .insert({
        organization_id: 'org_placeholder',
        product_id: 'product_placeholder',
        delta,
        reason: 'discrepancy',
        user_id: 'current_user_id',
        transaction_id: goodsReceiptId,
        status: 'pending',
      });
  },

  async processReturn(goodsReceiptId: string, returnItems: { supplier_item_id: string; quantity: number }[]): Promise<void> {
    // Create negative stock movements for returned items
    for (const { supplier_item_id, quantity } of returnItems) {
      const { data: productData, error: prodError } = await supabase
        .from('product_variants')
        .select('id')
        .eq('ingredient_id', supplier_item_id)
        .single();

      if (prodError) throw prodError;
      const productVariantId = productData.id;

      await supabase
        .from('stock_movements')
        .insert({
          organization_id: 'org_placeholder',
          product_id: productVariantId,
          delta: -quantity,
          reason: 'return',
          user_id: 'current_user_id',
          transaction_id: goodsReceiptId,
          status: 'pending',
        });
    }

    // Optionally mark receipt as 'rejected' or 'returned'
    await supabase
      .from('goods_receipts')
      .update({ status: 'rejected' })
      .eq('id', goodsReceiptId);
  },
};