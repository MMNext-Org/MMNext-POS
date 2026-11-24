export type ReportFormat = 'csv' | 'pdf';

export type ReportType = 'orders' | 'shifts' | 'loyalty' | 'sales';

export interface ReportOptions {
  type: ReportType;
  format: ReportFormat;
  startDate?: string;
  endDate?: string;
  organizationId?: string;
  branchId?: string;
  filename?: string;
}

/**
 * Escape a value for CSV: wrap in quotes if it contains comma, quote, or newline;
 * double any internal quotes.
 */
function escapeCsv(value: unknown): string {
  if (value === null || value === undefined) return '';
  const str = String(value);
  if (/[",\n\r]/.test(str)) {
    return `"${str.replace(/"/g, '""')}"`;
  }
  return str;
}

/**
 * Convert an array of objects to a CSV string.
 */
export function toCsv<T extends Record<string, unknown>>(data: T[]): string {
  if (data.length === 0) return '';

  const headers = Object.keys(data[0]);
  const rows: string[] = [];

  rows.push(headers.map(escapeCsv).join(','));

  for (const item of data) {
    rows.push(
      headers.map((h) => escapeCsv(item[h])).join(','),
    );
  }

  return rows.join('\n');
}

/**
 * Download a string as a file with the given name and MIME type.
 */
function downloadBlob(content: string, filename: string, mimeType: string): void {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

/**
 * Export data as CSV file.
 */
export function exportToCsv<T extends Record<string, unknown>>(
  data: T[],
  filename: string,
): void {
  const csv = toCsv(data);
  downloadBlob(csv, filename, 'text/csv;charset=utf-8');
}

/**
 * Export data as PDF using the browser's print dialog (Save as PDF).
 * Renders a print-friendly HTML table and opens it in a new window.
 */
export function exportToPdf(
  title: string,
  columns: string[],
  rows: unknown[][],
  filename: string,
): void {
  const headerRow = columns
    .map((c) => `<th style="border:1px solid #ccc;padding:6px;text-align:left;background:#f5f5f5">${c}</th>`)
    .join('');

  const bodyRows = rows
    .map(
      (row) => `<tr>${row
        .map(
          (cell) => `<td style="border:1px solid #ccc;padding:6px">${String(cell ?? '')}</td>`,
        )
        .join('')}</tr>`,
    )
    .join('');

  const html = `
    <!DOCTYPE html>
    <html>
      <head>
        <meta charset="utf-8" />
        <title>${title}</title>
        <style>
          body { font-family: system-ui, -apple-system, sans-serif; font-size: 12px; padding: 20px; }
          h1 { font-size: 18px; margin-bottom: 12px; color: #111; }
          .meta { font-size: 11px; color: #666; margin-bottom: 16px; }
          table { border-collapse: collapse; width: 100%; }
          th, td { border: 1px solid #ccc; padding: 6px; text-align: left; }
          th { background: #f5f5f5; }
          tr:nth-child(even) td { background: #fafafa; }
          @media print { body { padding: 0; } }
        </style>
      </head>
      <body>
        <h1>${title}</h1>
        <div class="meta">Generated: ${new Date().toISOString()}</div>
        <table>
          <thead><tr>${headerRow}</tr></thead>
          <tbody>${bodyRows}</tbody>
        </table>
        <script>window.print();</script>
      </body>
    </html>
  `;

  const printWindow = window.open('', '_blank', 'width=800,height=600');
  if (!printWindow) {
    // Fallback: download as HTML if popup blocked
    downloadBlob(html, filename.replace('.pdf', '.html'), 'text/html;charset=utf-8');
    return;
  }

  printWindow.document.write(html);
  printWindow.document.close();
}

/**
 * Flatten a nested object into a single-level record for CSV export.
 * Arrays and objects are JSON-stringified.
 */
function flattenRecord(obj: Record<string, unknown>, prefix = ''): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    if (value === null || value === undefined) {
      result[fullKey] = '';
    } else if (Array.isArray(value)) {
      result[fullKey] = JSON.stringify(value);
    } else if (typeof value === 'object') {
      Object.assign(result, flattenRecord(value as Record<string, unknown>, fullKey));
    } else {
      result[fullKey] = value;
    }
  }
  return result;
}

/**
 * Prepare orders data for export: flatten nested items and select relevant columns.
 */
export function prepareOrdersForExport(orders: unknown[]): Record<string, unknown>[] {
  return orders.map((o) => {
    const flat = flattenRecord(o as Record<string, unknown>);
    return {
      id: flat.id,
      organizationId: flat.organizationId,
      branchId: flat.branchId,
      status: flat.status,
      total: flat.total,
      created_at: flat.created_at,
      items: flat.items || '',
    };
  });
}

/**
 * Prepare shifts data for export.
 */
export function prepareShiftsForExport(shifts: unknown[]): Record<string, unknown>[] {
  return shifts.map((s) => {
    const flat = flattenRecord(s as Record<string, unknown>);
    return {
      id: flat.id,
      organizationId: flat.organizationId,
      registrarId: flat.registrarId,
      registrarName: flat.registrarName,
      startTime: flat.startTime,
      endTime: flat.endTime || '',
      status: flat.status,
      cashIn: flat.cashIn,
      cashOut: flat.cashOut,
      total: flat.total,
      approvalStatus: flat.approvalStatus,
      tipsTotal: flat.tipsTotal,
      tipsAllocated: flat.tipsAllocated,
      createdAt: flat.createdAt,
    };
  });
}

/**
 * Prepare loyalty data for export (customers).
 */
export function prepareLoyaltyForExport(customers: unknown[]): Record<string, unknown>[] {
  return customers.map((c) => {
    const flat = flattenRecord(c as Record<string, unknown>);
    return {
      id: flat.id,
      organizationId: flat.organizationId,
      name: flat.name,
      email: flat.email || '',
      phone: flat.phone || '',
      loyaltyPoints: flat.loyaltyPoints,
      createdAt: flat.createdAt,
    };
  });
}
