#!/usr/bin/env node
const express = require('express');
const sqlite3 = require('sqlite3').verbose();
const cors = require('cors');
const path = require('path');
const Decimal = require('decimal.js');
console.log('Decimal import successful:', typeof Decimal);

const AuditLogger = require('./src/utils/audit.cjs');

const app = express();
app.use(cors());
app.use(express.json());

const db = new sqlite3.Database('./local.db', sqlite3.OPEN_READWRITE, (err) => {
  if (err) {
    console.error('Could not open database', err);
    return;
  }
  console.log('Connected to the local SQLite database.');
});

// Test route
app.get('/test', (req, res) => {
  res.json({ message: 'Backend is running' });
});

// Get all products
app.get('/products', (req, res) => {
  db.all('SELECT * FROM products', [], (err, rows) => {
    if (err) {
      console.error('Error fetching products:', err);
      res.status(500).json({ error: 'Database error' });
    } else {
      res.json(rows);
    }
  });
});

// Create a new order with enhanced idempotency protection including payment details
app.post('/orders', (req, res) => {
  console.log('DEBUG: Raw request body:', req.body);
  const { organizationId, branchId, status, total, items, paymentMethod, paymentDetails } = req.body;
  
  // Validate required parameters
  if (!organizationId || !branchId || !status || total === undefined || !items) {
    return res.status(400).json({ error: 'Missing required parameters' });
  }
  
  // Convert total to Decimal for proper financial arithmetic
    console.log('DEBUG: Decimal imported?', typeof Decimal);
  const decimalTotal = new Decimal(total);
    console.log('DEBUG: decimalTotal created:', typeof decimalTotal);
  
  // Convert total to integer cents for database storage
  const totalAmount = parseInt(decimalTotal.toString(), 10);
  
  // Serialize items for consistent comparison (sort by productId)
  const serializedItems = JSON.stringify(items.slice().sort((a, b) => a.productId.localeCompare(b.productId)));
  
  // Check if an order with these parameters already exists - including payment details for more precise matching
  const checkSql = `SELECT id, organization_id, branch_id, status, total_amount, items FROM "order" WHERE organization_id = ? AND branch_id = ? AND status = ? AND total_amount = ? AND items = ? AND (paymentMethod = ? OR ? IS NULL) AND (paymentDetails IS ? OR ? IS NULL)`;
  
  const checkValues = [
      organizationId,
    branchId,
    status,
    totalAmount,
    serializedItems,
    paymentMethod || null,
    null
  ];
  
  db.get(checkSql, checkValues, function(err, row) {
    if (err) {
      console.error('Error checking existing order:', err);
      AuditLogger.financialOperation('order_creation_failed', { 
        error: 'Database error',
        organizationId,
        branchId,
        status,
        total: decimalTotal.toFloat(),
        items
      });
      res.status(500).json({ error: 'Database error' });
    } else if (row) {
      // Existing order found - return it instead of creating new
      AuditLogger.orderCreation('order_duplicate_detected', {
        orderId: row.id,
        organizationId,
        branchId,
        status,
        total: decimalTotal.toFloat(),
        items: row.items,
        paymentMethod: row.paymentMethod,
        paymentDetails: row.paymentDetails
      });
      res.status(200).json(row);
    } else {
      // No existing order, create new one
            const sql = `INSERT INTO "order" (organization_id, branch_id, status, total_amount, created_at, items) VALUES (?, ?, ?, ?, datetime('now'), ?)`;
                        const values = [organizationId, branch_id, status, totalAmount, serializedItems];
      
      db.run(sql, values, function(err) {
        if (err) {
          console.error('Error creating order:', err);
          AuditLogger.financialOperation('order_creation_failed', { 
            error: 'Database error',
            organizationId,
            branchId,
            status,
            total: decimalTotal.toFloat(),
            items
          });
          res.status(500).json({ error: 'Database error' });
        } else {
          const orderId = this.lastID;
          
          // Create payment record if payment details are provided
          if (paymentMethod && paymentDetails) {
            const paymentSql = `INSERT INTO payments (order_id, organization_id, amount, method, status, provider_transaction_id, created_at, updated_at) VALUES (?, ?, ?, ?, 'pending', ?, datetime('now'), datetime('now'))`;
                      const paymentValues = [orderId, organizationId, totalAmount, paymentMethod, JSON.stringify(paymentDetails)];
            
            db.run(paymentSql, paymentValues, function(paymentErr) {
              if (paymentErr) {
                console.error('Error creating payment record:', paymentErr);
                AuditLogger.financialOperation('payment_creation_failed', { 
                  error: 'Database error',
                  orderId,
                  organizationId,
                  paymentMethod,
                  organizationId,
                  paymentDetails
                });
                res.status(500).json({ error: 'Payment record creation failed' });
              } else {
                AuditLogger.financialOperation('payment_created', {
                  orderId,
                  organizationId,
                  paymentId: this.lastID,
                  paymentMethod,
                  total: decimalTotal.toFloat()
                });
                
                res.status(201).json({ 
                  id: orderId,
                  paymentStatus: 'pending',
                  paymentId: this.lastID 
                });
              }
            });
          } else {
            // No payment details, still create order with pending payment status
            AuditLogger.orderCreation('order_created', {
              orderId: this.lastID,
              organizationId,
              branchId,
              status,
              total: decimalTotal.toFloat(),
              items,
              paymentMethod: paymentMethod || 'none'
            });
            
            res.status(201).json({ 
              id: this.lastID,
              paymentStatus: 'pending' 
            });
          }
        }
      });
    }
  });
});

// Get order by ID
app.get('/orders/:id', (req, res) => {
  const orderId = req.params.id;
  db.get('SELECT * FROM order WHERE id = ?', [orderId], (err, row) => {
    if (err) {
      console.error('Error fetching order:', err);
      res.status(500).json({ error: 'Database error' });
    } else if (row) {
      res.json(row);
    } else {
      res.status(404).json({ error: 'Order not found' });
    }
  });
});

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});
