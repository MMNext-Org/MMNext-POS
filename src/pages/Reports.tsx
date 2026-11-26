import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { useOrderContext } from '@/context/OrderContext';
import { useShiftContext } from '@/context/ShiftContext';
import { useLoyaltyContext } from '@/context/LoyaltyContext';
import { useReports } from '@/context/ReportsContext';
import { Button } from '@/components/ui/Button';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { EmptyState } from '@/components/ui/EmptyState';

type ReportType = 'orders' | 'shifts' | 'loyalty' | 'sales';
type ReportFormat = 'csv' | 'pdf';

const REPORT_TYPES: { value: ReportType; label: string }[] = [
  { value: 'orders', label: 'Orders' },
  { value: 'shifts', label: 'Shifts' },
  { value: 'loyalty', label: 'Loyalty Customers' },
  { value: 'sales', label: 'Sales Summary' },
];

const FORMATS: { value: ReportFormat; label: string }[] = [
  { value: 'csv', label: 'CSV' },
  { value: 'pdf', label: 'PDF' },
];

export const Reports = () => {
  const { user } = useAuth();
  const { orders } = useOrderContext();
  const { shifts } = useShiftContext();
  const { customers } = useLoyaltyContext();
  const { state, canExport, exportReport, clearError } = useReports();

  const userRole = user?.app_metadata?.role_name || user?.role || null;
  const canView = canExport(userRole);

  const [reportType, setReportType] = useState<ReportType>('orders');
  const [format, setFormat] = useState<ReportFormat>('csv');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  const [data, setData] = useState<unknown[]>([]);
  const [exportConfirm, setExportConfirm] = useState(false);

  // Compute data for the selected report type and date filters (derived during render)
  const reportData: unknown[] = (() => {
    let source: unknown[] = [];
    switch (reportType) {
      case 'orders':
        source = orders;
        break;
      case 'shifts':
        source = shifts;
        break;
      case 'loyalty':
        source = customers;
        break;
      case 'sales':
        source = orders;
        break;
    }

    if (startDate && endDate) {
      const start = new Date(startDate);
      const end = new Date(endDate);
      end.setHours(23, 59, 59, 999);
      return source.filter((item) => {
        const itemDate =
          (item as { created_at?: string; createdAt?: string })?.created_at ||
          (item as { created_at?: string; createdAt?: string })?.createdAt;
        if (!itemDate) return false;
        const d = new Date(itemDate);
        return d >= start && d <= end;
      });
    }
    return source;
  })();

  useEffect(() => {
    setData(reportData);
  }, [reportData]);

  const handleExport = () => {
    exportReport(
      {
        type: reportType,
        format,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        filename: `${reportType}-report`,
      },
      data,
    );
    setExportConfirm(false);
  };

  // Get column headers from first data row
  const headers: string[] = data.length > 0 ? Object.keys(data[0] as object) : [];

  const totalCount = data.length;

  if (!canView) {
    return (
      <div className="min-h-screen bg-gray-50 p-4">
        <div className="max-w-2xl mx-auto">
          <div className="bg-white rounded-lg shadow border border-gray-200 p-8 text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Reports</h1>
            <p className="text-gray-600 mb-6">
              You do not have permission to view or export reports.
            </p>
            <p className="text-sm text-gray-500 mb-6">
              Reports are restricted to Shift Lead and Branch Manager roles. Your role:{' '}
              <span className="font-medium">{userRole || 'unassigned'}</span>.
            </p>
            <Link to="/" className="text-sm text-gray-600 hover:text-gray-900 underline">
              ← Back to Home
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
  <div className="min-h-screen bg-gray-50 p-4" aria-labelledby="reports-heading">
      <div className="max-w-5xl mx-auto">
        <div className="flex justify-between items-end mb-6">
          <div>
          <h1 id="reports-heading" className="text-2xl font-bold text-gray-900">Reports & Export</h1>
            <p className="text-gray-600">
              Generate and export reports for your organization. Role:{' '}
              <span className="font-medium text-gray-800">{userRole}</span>
            </p>
          </div>
          <Link to="/" className="text-sm text-gray-600 hover:text-gray-900 underline">
            ← Back to Home
          </Link>
        </div>

        {state.error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md text-red-800 text-sm flex items-center gap-2">
            <span>⚠️</span>
            {state.error}
            <button
              onClick={clearError}
              aria-label="Dismiss error"
              className="ml-auto text-red-600 hover:text-red-800 underline text-sm"
            >
              Dismiss
            </button>
          </div>
        )}

        {state.lastExported && (
          <div className="mb-6 p-3 bg-green-50 border border-green-200 rounded-md text-green-800 text-sm">
            Exported {state.lastExported.type} as {state.lastExported.format.toUpperCase()} at{' '}
            {new Date(state.lastExported.at).toLocaleString()}
          </div>
        )}

        {/* Controls */}
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-6">
          <h2 className="text-lg font-bold mb-4">Report Options</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="flex-1">
              <label htmlFor="reports-report-type" className="block text-sm font-medium text-gray-700 mb-2">
                Report Type
              </label>
              <select
                id="reports-report-type"
                value={reportType}
                onChange={(e) => setReportType(e.target.value as ReportType)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 bg-white"
              >
                {REPORT_TYPES.map((rt) => (
                  <option key={rt.value} value={rt.value}>
                    {rt.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex-1">
              <label htmlFor="reports-format" className="block text-sm font-medium text-gray-700 mb-2">
                Format
              </label>
              <select
                id="reports-format"
                value={format}
                onChange={(e) => setFormat(e.target.value as ReportFormat)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 bg-white"
              >
                {FORMATS.map((f) => (
                  <option key={f.value} value={f.value}>
                    {f.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex-1">
              <label htmlFor="reports-start-date" className="block text-sm font-medium text-gray-700 mb-2">
                Start Date
              </label>
              <input
                id="reports-start-date"
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              />
            </div>
            <div className="flex-1">
              <label htmlFor="reports-end-date" className="block text-sm font-medium text-gray-700 mb-2">
                End Date
              </label>
              <input
                id="reports-end-date"
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              />
            </div>
          </div>
          <div className="mt-4 flex flex-col sm:flex-row gap-3">
            <Button variant="primary" onClick={() => setExportConfirm(true)} disabled={state.loading || data.length === 0}>
              {state.loading ? 'Exporting...' : `Export ${format.toUpperCase()}`}
            </Button>
            <Button variant="secondary" onClick={() => setData([])} disabled={data.length === 0}>
              Clear
            </Button>
            <span className="ml-auto text-sm text-gray-500">
              {totalCount} record{totalCount !== 1 ? 's' : ''} ready
            </span>
          </div>
        </div>

        {/* Data Preview */}
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
          <h2 className="text-lg font-bold mb-4">Data Preview</h2>
          {data.length === 0 ? (
            <EmptyState
              icon="📊"
              title="No data available"
              message="No data available for the selected report and filters."
            />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="bg-gray-100">
                  <tr>
                    {headers.map((h) => (
                      <th key={h} scope="col" className="px-4 py-2 font-semibold text-gray-600">
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {data.slice(0, 50).map((item, idx) => (
                    <tr key={idx}>
                      {headers.map((h) => (
                        <td key={h} className="px-4 py-3">
                          {String((item as Record<string, unknown>)[h] ?? '')}
                        </td>
                      ))}
                    </tr>
                  ))}
                  {data.length > 50 && (
                    <tr>
                      <td colSpan={headers.length} className="px-4 py-3 text-center text-gray-500 text-sm">
                        Showing first 50 of {data.length} records
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      <ConfirmDialog
        isOpen={exportConfirm}
        title="Export Report"
        message={`Export ${totalCount} record${totalCount !== 1 ? 's' : ''} as ${format.toUpperCase()}? This cannot be undone.`}
        confirmLabel="Export"
        cancelLabel="Cancel"
        variant="secondary"
        loading={state.loading}
        onConfirm={handleExport}
        onCancel={() => setExportConfirm(false)}
      />
    </div>
  );
};

export default Reports;
