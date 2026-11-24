import { useState, useEffect } from 'react';
import { supabase } from '../lib/supabase';

interface Customer {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  birthday: string | null;
  preferences: Record<string, unknown>;
  consent: boolean;
  loyalty_points: number;
  created_at: string;
}

interface CustomerFormProps {
  customer: Customer | null;
  onSave: () => void;
  onClose: () => void;
}

export default function CustomerForm({ customer, onSave, onClose }: CustomerFormProps) {
  const [form, setForm] = useState({
    name: customer?.name || '',
    email: customer?.email || '',
    phone: customer?.phone || '',
    birthday: customer?.birthday || '',
    preferences: JSON.stringify(customer?.preferences || {}),
    consent: customer?.consent || false,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isNew, setIsNew] = useState(!customer);

  useEffect(() => {
    if (customer) {
      setForm({
        name: customer.name,
        email: customer.email || '',
        phone: customer.phone || '',
        birthday: customer.birthday || '',
        preferences: JSON.stringify(customer.preferences || {}),
        consent: customer.consent,
      });
      setIsNew(false);
    }
  }, [customer]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);

    let preferences: Record<string, unknown>;
    try {
      preferences = form.preferences ? JSON.parse(form.preferences) : {};
    } catch {
      setError('Invalid preferences JSON');
      setLoading(false);
      return;
    }

    const customerData = {
      name: form.name,
      email: form.email || null,
      phone: form.phone || null,
      birthday: form.birthday || null,
      preferences,
      consent: form.consent,
      updated_at: new Date().toISOString(),
    };

    try {
      if (isNew) {
        const { error: err } = await supabase.from('customers').insert(customerData);
        if (err) throw err;
      } else {
        const { error: err } = await supabase
          .from('customers')
          .update(customerData)
          .eq('id', customer!.id);
        if (err) throw err;
      }
      onSave();
      onClose();
    } catch (err: any) {
      setError(err.message);
      setLoading(false);
    }
  }

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">{isNew ? 'Add Customer' : 'Edit Customer'}</h2>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Full Name <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            required
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input
            type="email"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
          <input
            type="tel"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.phone}
            onChange={(e) => setForm({ ...form, phone: e.target.value })}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Birthday</label>
          <input
            type="date"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.birthday}
            onChange={(e) => setForm({ ...form, birthday: e.target.value })}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Preferences (JSON)</label>
          <textarea
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm"
            value={form.preferences}
            onChange={(e) => setForm({ ...form, preferences: e.target.value })}
            placeholder='{"key": "value"}'
          />
          <p className="text-xs text-gray-500 mt-1">
            Optional customer preferences stored as JSON (e.g., dietary restrictions, favorite items).
          </p>
        </div>

        <div className="flex items-start">
          <input
            type="checkbox"
            checked={form.consent}
            onChange={(e) => setForm({ ...form, consent: e.target.checked })}
            className="mt-1 h-4 w-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
          />
          <label className="ml-2 text-sm text-gray-700">
            I consent to data processing and marketing communications
          </label>
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="px-4 py-2 text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 transition-colors"
          >
            {loading ? 'Saving...' : 'Save Customer'}
          </button>
        </div>
      </form>
    </div>
  );
}