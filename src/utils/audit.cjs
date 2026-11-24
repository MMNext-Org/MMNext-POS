const AuditLogger = {
  financialOperation(operation, details) {
    console.error('AUDIT: FINANCIAL_OPERATION', { operation, details });
  },
  orderCreation(operation, details) {
    console.error('AUDIT: ORDER_CREATION', { operation, details });
  },
  // Add other methods as needed for compatibility
};

module.exports = AuditLogger;