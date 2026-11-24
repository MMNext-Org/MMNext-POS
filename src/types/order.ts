export type OrderStatus =
  | 'draft'
  | 'placed'
  | 'accepted'
  | 'preparing'
  | 'ready'
  | 'served'
  | 'completed'
  | 'cancelled';

export const OrderStatusDescriptions = {
  draft: 'Order is being created, not yet submitted',
  placed: 'Order has been placed by customer, pending confirmation',
  accepted: 'Order has been accepted by restaurant, preparation started',
  preparing: 'Order is being prepared by kitchen staff',
  ready: 'Order is ready for pickup or delivery',
  served: 'Order has been served to customer',
  completed: 'Order is fully completed',
  cancelled: 'Order has been cancelled by customer or restaurant',
};

export default { OrderStatus, OrderStatusDescriptions };