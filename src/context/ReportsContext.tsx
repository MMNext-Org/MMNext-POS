import { createContext, useContext, useState, useCallback } from 'react';
import {
  exportToPdf,
  prepareOrdersForExport,
  prepareShiftsForExport,
  prepareLoyaltyForExport,
  type ReportFormat,
  type ReportOptions,
  type ReportType,
} from '@/utils/export';

export type ReportRole = 'Cashier' | 'Shift Lead' | 'Branch Manager';

export interface ReportState {
  loading: boolean;
  error: string | null;
  lastExported: { type: ReportType; format: ReportFormat; at: string } | null;
}

export interface ReportsContextType {
  state: ReportState;
  canExport: (role: string | null | undefined) => boolean;
  exportReport: (options: ReportOptions, data: unknown[]) => void;
  clearError: () => void;
}

const ReportsContext = createContext<ReportsContextType | undefined>(undefined);

/**
 * Roles permitted to view and export reports.
 */
const EXPORT_ALLOWED_ROLES: ReportRole[] = ['Shift Lead', 'Branch Manager'];

export const ReportsProvider = ({
  children,
}: { children: React.ReactNode }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastExported, setLastExported] = useState<
    ReportState['lastExported']
  >(null);

  const canExport = useCallback(
    (role: string | null | undefined): boolean => {
      if (!role) return false;
      return EXPORT_ALLOWED_ROLES.includes(role as ReportRole);
    },
    [],
  );

  const exportReport = useCallback(
    (options: ReportOptions, data: unknown[]) => {
      setLoading(true);
      setError(null);

      try {
        const filename =
          options.filename ||
          `${options.type}-report-${new Date().toISOString().slice(0, 10)}`;

        let prepared: Record<string, unknown>[];

        switch (options.type) {
          case 'orders':
            prepared = prepareOrdersForExport(data);
            break;
          case 'shifts':
            prepared = prepareShiftsForExport(data);
            break;
          case 'loyalty':
            prepared = prepareLoyaltyForExport(data);
            break;
          case 'sales':
            // Sales report reuses order preparation
            prepared = prepareOrdersForExport(data);
            break;
          default:
            prepared = data as Record<string, unknown>[];
        }

        if (prepared.length === 0) {
          setError('No data available to export.');
          setLoading(false);
          return;
        }

        const headers = Object.keys(prepared[0]);
        const rows = prepared.map((r) => headers.map((h) => r[h]));

        if (options.format === 'csv') {
          const csvContent = [
            headers.join(','),
            ...rows.map((row) =>
              row.map((cell) => {
                const s = String(cell ?? '');
                if (/[",\n\r]/.test(s)) {
                  return `"${s.replace(/"/g, '""')}"`;
                }
                return s;
              }).join(','),
            ),
          ].join('\n');

          const blob = new Blob([csvContent], {
            type: 'text/csv;charset=utf-8',
          });
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${filename}.csv`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          URL.revokeObjectURL(url);
        } else {
          // PDF via print dialog
          const title =
            options.type.charAt(0).toUpperCase() + options.type.slice(1) +
            ' Report';
          exportToPdf(title, headers, rows, `${filename}.pdf`);
        }

        setLastExported({
          type: options.type,
          format: options.format,
          at: new Date().toISOString(),
        });
      } catch (err) {
        setError(
          err instanceof Error ? err.message : 'Export failed.',
        );
      } finally {
        setLoading(false);
      }
    },
    [],
  );

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const value: ReportsContextType = {
    state: { loading, error, lastExported },
    canExport,
    exportReport,
    clearError,
  };

  return (
    <ReportsContext.Provider value={value}>{children}</ReportsContext.Provider>
  );
};

export const useReports = () => {
  const context = useContext(ReportsContext);
  if (!context) {
    throw new Error('useReports must be used within ReportsProvider');
  }
  return context;
};

export default ReportsContext;
