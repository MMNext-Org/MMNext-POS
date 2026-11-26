import React from 'react';
import { useToast } from '@/components/ui/Toast';

// Define types
interface Product {
  id: string;
  name: string;
  category: string;
  price: number;
}

interface Category {
  name: string;
  products: Product[];
}

// Mock data - in a real app, this would come from Supabase
const categories: Category[] = [
  {
    name: 'Beverages',
    products: [
      { id: '1', name: 'Coffee', category: 'Beverages', price: 3.5 },
      { id: '2', name: 'Tea', category: 'Beverages', price: 2.5 },
      { id: '3', name: 'Juice', category: 'Beverages', price: 4.0 },
    ],
  },
  {
    name: 'Food',
    products: [
      { id: '4', name: 'Burger', category: 'Food', price: 8.0 },
      { id: '5', name: 'Fries', category: 'Food', price: 3.5 },
      { id: '6', name: 'Salad', category: 'Food', price: 6.0 },
    ],
  },
  {
    name: 'Desserts',
    products: [
      { id: '7', name: 'Cake', category: 'Desserts', price: 5.0 },
      { id: '8', name: 'Ice Cream', category: 'Desserts', price: 4.5 },
    ],
  },
];

const MenuCategoryView: React.FC = () => {
  const { toast } = useToast();

  return (
    <div className="p-4 font-sans">
      <h1 className="text-xl font-bold mb-4">Menu Categories</h1>
      {categories.map((category) => (
        <div key={category.name} className="mb-6">
          <h2 className="text-lg font-semibold mb-2">{category.name}</h2>
          <ul className="list-none p-0 space-y-2">
            {category.products.map((product) => (
              <li
                key={product.id}
                className="border border-gray-200 p-4 rounded flex justify-between items-center bg-white"
              >
                <div>
                  <span className="font-medium">{product.name}</span>
                  <span className="ml-2 text-gray-500">${product.price.toFixed(2)}</span>
                </div>
                <button
                  onClick={() => toast({ title: 'Added to order', description: product.name, type: 'success' })}
                  className="bg-coffee-brown text-white px-3 py-1 rounded hover:bg-green-700 text-sm"
                >
                  Add
                </button>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
};

export default MenuCategoryView;