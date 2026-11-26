import React, { useState } from 'react';
import { useToast } from '@/components/ui/Toast';

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
  const { toast } = useToast();
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
    const product = mockProducts.find((p) => p.id === selectedProductId);
    if (selectedProductId && quantity > 0 && product) {
      toast({
        title: 'Added to order',
        description: `${quantity} × ${product.name}${modifiers.length ? ` with modifiers: ${modifiers.join(', ')}` : ''}`,
        type: 'success',
      });
      // In a real app, this would update OrderContext queue
      setSelectedProductId('');
      setQuantity(1);
      setModifiers([]);
    } else {
      toast({ title: 'Missing info', description: 'Please select a product and set quantity.', type: 'error' });
    }
  };

  return (
    <div className="max-w-sm mx-auto p-4 font-sans">
      <h1 className="text-xl font-bold mb-4">Create Order</h1>
      <div className="mb-4">
        <label htmlFor="product-select" className="block mb-1">
          Product:
        </label>
        <select
          id="product-select"
          value={selectedProductId}
          onChange={handleSelectProduct}
          className="w-full px-2 py-1 mb-2 rounded border border-gray-300"
        >
          <option value="">-- Select a product --</option>
          {productOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <div className="mb-5">
        <label htmlFor="quantity" className="block mb-1 text-sm font-medium text-gray-700">
          Quantity:
        </label>
        <input
          id="quantity"
          type="number"
          min="1"
          value={quantity}
          onChange={handleQuantityChange}
          className="w-full px-3 py-2 mb-3 rounded-md border border-gray-300 focus:ring-2 focus:ring-gray-500 focus:outline-none"
        />
      </div>

      <div className="mb-4">
        <fieldset className="border border-gray-200 p-2 rounded">
          <legend className="text-sm">Modifiers:</legend>
          {[
            { label: 'Size', value: 'size' },
            { label: 'Extra Cheese', value: 'extra_cheese' },
            { label: 'Toasted', value: 'toasted' },
            { label: 'Spicy', value: 'spicy' },
          ].map((mod) => (
            <label key={mod.value} className="block mb-1">
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

      <div className="flex gap-2">
        <button
          onClick={handleAddToOrder}
          className="flex-1 bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
        >
          Add to Order
        </button>
        <button
          onClick={() => {
            setSelectedProductId('');
            setQuantity(1);
            setModifiers([]);
          }}
          className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
        >
          Clear
        </button>
      </div>
    </div>
  );
};

export default OrderCreationForm;