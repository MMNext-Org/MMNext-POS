import React from 'react';

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
  return (
    <div style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <h1>Menu Categories</h1>
      {categories.map((category) => (
        <div key={category.name} style={{ marginBottom: '30px' }}>
          <h2>{category.name}</h2>
          <ul style={{ listStyle: 'none', padding: 0 }}>
            {category.products.map((product) => (
              <li
                key={product.id}
                style={{
                  border: '1px solid #ccc',
                  margin: '10px 0',
                  padding: '10px',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
              >
                <span>{product.name}</span>
                <span>${product.price.toFixed(2)}</span>
                <button
                  onClick={() => alert(`Add ${product.name} to order`)}
                  style={{
                    backgroundColor: '#007bff',
                    color: 'white',
                    border: 'none',
                    padding: '5px 10px',
                    cursor: 'pointer',
                    borderRadius: '4px',
                  }}
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