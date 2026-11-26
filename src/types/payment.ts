export type PaymentStatus = 'pending' | 'confirmed' | 'failed' | 'refunded' | 'voided';

export const PaymentStatusValues: PaymentStatus[] = [
  'pending',
  'confirmed',
  'failed',
  'refunded',
  'voided',
];

export const PaymentStatusDescriptions = {
  pending: 'Payment initiated but not yet confirmed',
  confirmed: 'Payment verified and completed',
  failed: 'Payment attempt failed',
  refunded: 'Refund transaction completed',
  voided: 'Payment canceled before confirmation',
};

export default { PaymentStatusValues, PaymentStatusDescriptions };