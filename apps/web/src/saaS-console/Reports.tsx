export default function Reports() {
  return (
    <div className="min-h-screen">
      <h1 className="text-2xl font-bold mb-4">Reports</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="bg-white p-4 rounded shadow">
          <h2 className="text-xl font-semibold mb-2">Sales Summary</h2>
          <p className="text-gray-600">
            Total sales for the selected period: $12,450.00<br />
            Total transactions: 142<br />
            Average order value: $87.53
          </p>
        </div>
        
        <div className="bg-white p-4 rounded shadow">
          <h2 className="text-xl font-semibold mb-2">Inventory Levels</h2>
          <p className="text-gray-600">
            Low stock items: 5<br />
            Reorder threshold: 10 units<br />
            Total items in stock: 342
          </p>
        </div>
      </div>
    </div>
  );
}