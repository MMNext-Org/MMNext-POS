export default function TenantManagement() {
  return (
    <div className="min-h-screen">
      <h1 className="text-2xl font-bold mb-4">Tenant Management</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="bg-white p-4 rounded shadow">
          <h2 className="text-xl font-semibold mb-2">New Tenant</h2>
          <form>
            <div className="mb-4">
              <label className="block text-sm font-medium mb-1" htmlFor="tenant-name">
                Organization Name
              </label>
              <input
                id="tenant-name"
                name="tenant-name"
                type="text"
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div className="mb-4">
              <label className="block text-sm font-medium mb-1" htmlFor="domain">
                Domain Name
              </label>
              <input
                id="domain"
                name="domain"
                type="text"
                placeholder="example.com"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div className="mb-6">
              <button 
                type="submit"
                className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors"
              >
                Create Tenant
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}