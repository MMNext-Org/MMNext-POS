import React, { useState } from 'react';

// Define product type
interface Product {
  id: string;
  name: string;
  category: string;
  price: number;
}

// Mock product list (same as in MenuCategoryView)
const mockProducts: Product[] = [
  { id: '1', name: 'Coffee', category: 'Beverages', price: 3.5 },
  { id: '2', name: 'Tea', category: 'Beverages', price: 2.5 },
  { id: '3', name: 'Juice', category: 'Beverages', price: 4.0 },
  { id: '4', name: 'Burger', category: 'Food', price: 8.0 },
  { id: '5', name: 'Fries', category: 'Food', price: 3.5 },
  { id: '6', name: 'Salad', category: 'Food', price: 6.0 },
  { id: '7', name: 'Cake', category: 'Desserts', price: 5.0 },
  { id: '8', name: 'Ice Cream', category: 'Desserts', price: 4.5 },
];

const OrderCreationForm: React.FC = () => {
  const [selectedProductId, setSelectedProductId] = useState<string>('');
  const [quantity, setQuantity] = useState<number>(1);
  const [modifiers, setModifiers] = useState<string[]>([]);

  const productOptions = mockProducts.map((p) => ({
    value: p.id,
    label: `${p.name} ($${p.price})`,
  }));

  const handleSelectProduct = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedProductId(event.target.value);
  };

  const handleQuantityChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setQuantity(parseInt(event.target.value, 10) || 1);
  };

  const handleModifierChange = (checkboxEvent: React.ChangeEvent<HTMLInputElement>) => {
    if (checkboxEvent.target.checked) {
      setModifiers([...modifiers, checkboxEvent.target.value]);
    } else {
      setModifiers(modifiers.filter((m) => m !== checkboxEvent.target.value));
    }
  };

  const handleAddToOrder = () => {
    if (selectedProductId && quantity > 0) {
      alert(
        `Added ${quantity} × ${mockProducts.find((p) => p.id === selectedProductId)?.name} to order with modifiers: ${modifiers.join(', ') || 'none'}`
      );
      // In a real app, this would update OrderContext queue
      setSelectedProductId('');
      setQuantity(1);
      setModifiers([]);
    } else {
      alert('Please select a product and set quantity.');
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <h1>Create Order</h1>
      <div style={{ marginBottom: '20px' }}>
        <label htmlFor="product-select" style={{ display: 'block', marginBottom: '5px' }}>
          Product:
        </label>
        <select
          id="product-select"
          value={selectedProductId}
          onChange={handleSelectProduct}
          style={{
            width: '100%',
            padding: '8px',
            marginBottom: '10px',
            borderRadius: '4px',
            border: '1px solid #ccc',
          }}
        >
          <option value="">-- Select a product --</option>
          {productOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <label htmlFor="quantity" style={{ display: 'block', marginBottom: '5px' }}>
          Quantity:
        </label>
        <input
          id="quantity"
          type="number"
          min="1"
          value={quantity}
          onChange={handleQuantityChange}
          style={{
            width: '100%',
            padding: '8px',
            marginBottom: '10px',
            borderRadius: '4px',
            border: '1px solid #ccc',
          }}
        />
      </div>

      <div style={{ marginBottom: '20px' }}>
        <fieldset style={{ border: '1px solid #ccc', padding: '10px' }}>
          <legend>Modifiers:</legend>
          {[
            { label: 'Size', value: 'size' },
            { label: 'Extra Cheese', value: 'extra_cheese' },
            { label: 'Toasted', value: 'toasted' },
            { label: 'Spicy', value: 'spicy' },
          ].map((mod) => (
            <label key={mod.value} style={{ display: 'block', margin: '5px 0' }}>
              <input
                type="checkbox"
                name="modifier"
                value={mod.value}
                checked={modifiers.includes(mod.value)}
                onChange={handleModifierChange}
              />{' '}
              {mod.label}
            </label>
          ))}
        </fieldset>
      </div>

      <div style={{ display: 'flex', gap: '10px' }}>
        <button
          onClick={handleAddToOrder}
          style={{
            flexGrow: 1,
            backgroundColor: '#28a745',
            color: 'white',
            border: 'none',
            padding: '10px',
            borderRadius: '4px',
            cursor: 'pointer',
          }}
        >
          Add to Order
        </button>
        <button
          onClick={() => {
            setSelectedProductId('');
            setQuantity(1);
            setModifiers([]);
          }}
          style={{
            backgroundColor: '#6c757d',
            color: 'white',
            border: 'none',
            padding: '10px',
            borderRadius: '4px',
            cursor: 'pointer',
          }}
        >
          Clear
        </button>
      </div>
    </div>
  );
};

export default OrderCreationForm;