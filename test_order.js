const https = require('https');
const http = require('http');

const data = JSON.stringify({
  organizationId: 1,
  branchId: 1,
  status: 'draft',
  total: 100.00,
  items: [{ productId: 1, quantity: 2 }],
  paymentMethod: 'card',
  paymentDetails: { transactionId: 'tx123', amount: 100.00 }
});

const options = {
  hostname: 'localhost',
  port: 5000,
  path: '/orders',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Content-Length': Buffer.byteLength(data)
  }
};

const req = http.request(options, (res) => {
  let body = '';
  res.on('data', chunk => body += chunk);
  res.on('end', () => {
    console.log('Status:', res.statusCode);
    console.log('Body:', body);
  });
});

req.on('error', (e) => {
  console.error('Error:', e);
});

req.write(data);
req.end();