import { supabase } from '../supabaseClient';
import { v4 as uuidv4 } from 'uuid';

export interface StockAdjustment {
  product_id: string;
  delta: number;
  reason: string;
  user_id?: string;
  transaction_id?: string;
  status: 'pending' | 'committed' | 'cancelled';
}

export const InventoryService = {
  async receiveGoods(
    items: { supplier_item_id: string; quantity: number }[],
    receiptId: string,
    productVariantId: string,
    organizationId: string
  ): Promise<void> {
    // Insert goods receipt items
    const itemsInsert = items.map(item => ({
      goods_receipt_id: receiptId,
      organization_id: organizationId,
      ingredient_id: item.supplier_item_id,
      quantity: item.quantity,
      unit_price: 0,
    }));

    const { error: itemsError } = await supabase
      .from('goods_receipt_items')
      .insert(itemsInsert);

    if (itemsError) throw itemsError;

    // Create stock movements and reconcile inventory
    for (const { supplier_item_id, quantity } of items) {
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
          organization_id: organizationId,
          product_id: productVariantId,
          delta: quantity,
          reason: 'goods_receipt',
          user_id: (await supabase.auth.getUser()).data.user?.user_metadata?.user_id ?? null,
          transaction_id: receiptId,
          status: 'pending',
        });

      // Optionally update inventory stock directly if a stock table exists
      // For now, assume stock is managed via product_variants or a dedicated stock table
    }
  },

  async handleDiscrepancy(
    goodsReceiptId: string,
    expectedQty: number,
    actualQty: number,
    productVariantId: string,
    organizationId: string
  ): Promise<void> {
    const delta = actualQty - expectedQty;
    await supabase
      .from('stock_movements')
      .insert({
        organization_id: organizationId,
        product_id: productVariantId,
        delta,
        reason: 'discrepancy',
        user_id: (await supabase.auth.getUser()).data.user?.user_metadata?.user_id ?? null,
        transaction_id: goodsReceiptId,
        status: 'pending',
      });
  },

  async processReturn(
    goodsReceiptId: string,
    returnItems: { supplier_item_id: string; quantity: number }[],
    organizationId: string
  ): Promise<void> {
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
          organization_id: organizationId,
          product_id: productVariantId,
          delta: -quantity,
          reason: 'return',
          user_id: (await supabase.auth.getUser()).data.user?.user_metadata?.user_id ?? null,
          transaction_id: goodsReceiptId,
          status: 'pending',
        });
  }

    // Mark the receipt as rejected or return status
    await supabase
      .from('goods_receipts')
      .update({ status: 'rejected' })
      .eq('id', goodsReceiptId);
  },

  async updateStock(
    productId: string,
    delta: number,
    reason: string,
    organizationId: string
  ): Promise<void> {
    // Direct stock update (assuming a products table with stock column)
    const { error: updateError } = await supabase
      .from('products')
      .update({ stock: delta > 0 ? 'stock + ?' : 'stock - ?' })
      .eq('id', productId)
      .params(delta);

    if (updateError) throw updateError;

    // Record stock movement
    await supabase
      .from('stock_movements')
      .insert({
        product_id: productId,
        delta,
        reason,
        user_id: (await supabase.auth.getUser()).data.user?.user_metadata?.user_id ?? null,
        status: 'pending',
      });
  },
};