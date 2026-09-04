using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using System.ComponentModel; // Added for BindingList
using System.Drawing;

namespace MMNextPOS.WinForms
{
    public class CustomerImportDialog : AsyncFormBase
    {
        private readonly ICustomerService _customerService;
        private DevExpress.XtraGrid.GridControl _grid = null!;
        private GridView _view = null!;
        private TextEdit _filePathBox = null!;
        private MemoEdit _errorMemo = null!;
        private BindingList<ImportCustomerRow> _rows = new();

        public CustomerImportDialog(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            this.Text = "Import Customers";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // File selection
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid preview
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Error summary
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Buttons

            // File selection
            var filePanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var fileLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(5)
            };
            fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

            _filePathBox = new TextEdit { Dock = DockStyle.Fill, Properties = { NullValuePrompt = "Select CSV file..." } };
            var btnBrowse = new SimpleButton { Text = "Browse", Dock = DockStyle.Fill };
            btnBrowse.Click += BtnBrowse_Click;
            fileLayout.Controls.Add(_filePathBox, 0, 0);
            fileLayout.Controls.Add(btnBrowse, 1, 0);
            filePanel.Controls.Add(fileLayout);
            mainLayout.Controls.Add(filePanel, 0, 0);

            // Grid preview
            _grid = new GridControl { Dock = DockStyle.Fill };
            _view = new GridView(_grid) { OptionsSelection = { MultiSelect = false }, OptionsView = { ShowGroupPanel = false } };
            _grid.MainView = _view;
            _grid.ViewCollection.Add(_view);
            mainLayout.Controls.Add(_grid, 0, 1);

            // Error summary
            _errorMemo = new MemoEdit { Dock = DockStyle.Fill, Properties = { ReadOnly = true, Appearance = { BackColor = Color.FromArgb(255, 240, 240) } } };
            mainLayout.Controls.Add(_errorMemo, 0, 2);

            // Buttons
            var buttonPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var btnImport = new SimpleButton { Text = "Import", Location = new Point(10, 10), Width = 100, Height = 30 };
            btnImport.Click += BtnImport_Click;
            var btnCancel = new SimpleButton { Text = "Cancel", Location = new Point(130, 10), Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            mainLayout.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(mainLayout);
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select Customer Import File"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _filePathBox.EditValue = dialog.FileName;
                TryParseCsv(dialog.FileName);
            }
        }

        private void TryParseCsv(string filePath)
        {
            _rows.Clear();
            _errorMemo.Text = "";

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2) { _errorMemo.Text = "File is empty or has only a header."; return; }

                // Try to detect if first line is header
                var firstLine = lines[0].Trim();
                var hasHeader = !string.IsNullOrWhiteSpace(firstLine) && (firstLine.Contains(",") && (firstLine.Contains("Name") || firstLine.Contains("Code")));

                int startIndex = hasHeader ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        _rows.Add(new ImportCustomerRow
                        {
                            Name = parts[0].Trim(),
                            Phone = parts.Length > 1 ? parts[1].Trim() : "",
                            Email = parts.Length > 2 ? parts[2].Trim() : ""
                        });
                    }
                }

                _grid.DataSource = _rows;
                _view.BestFitColumns();
                _errorMemo.Text = $"Loaded {_rows.Count} customer(s) from file.";
            }
            catch (Exception ex)
            {
                _errorMemo.Text = $"Error parsing file: {ex.Message}";
                _rows.Clear();
                _grid.DataSource = null;
            }
        }

        private async void BtnImport_Click(object? sender, EventArgs e)
        {
            if (_rows.Count == 0) { ShowInfo("No customers to import."); return; }

            var confirm = ShowConfirm($"Import {_rows.Count} customers into the database?");
            if (!confirm) return;

            try
            {
                int imported = 0, skipped = 0;
                foreach (var row in _rows)
                {
                    // Check if customer already exists (by name or phone)
                    var allCustomers = await _customerService.GetAllAsync(CancellationToken);
                    var exists = allCustomers.FirstOrDefault(c => c.Name == row.Name || c.Phone == row.Phone);

                    if (exists != null)
                    {
                        skipped++;
                        continue;
                    }

                    var customer = new Customer
                    {
                        Code = $"CUST-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                        Name = row.Name,
                        Phone = row.Phone,
                        Email = row.Email,
                        IsActive = true
                    };

                    await _customerService.AddAsync(customer, CancellationToken);
                    imported++;
                }

                ShowInfo($"Import complete: {imported} imported, {skipped} skipped (duplicates).");
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError($"Import failed: {ex.Message}");
            }
        }

        private class ImportCustomerRow
        {
            public string Name { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { this.DialogResult = DialogResult.Cancel; return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    public class SupplierImportDialog : AsyncFormBase
    {
        private readonly ISupplierService _supplierService;
        private DevExpress.XtraGrid.GridControl _grid = null!;
        private GridView _view = null!;
        private TextEdit _filePathBox = null!;
        private MemoEdit _errorMemo = null!;
        private BindingList<ImportSupplierRow> _rows = new();

        public SupplierImportDialog(ISupplierService supplierService)
        {
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));

            this.Text = "Import Suppliers";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // File selection
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid preview
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Error summary
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Buttons

            // File selection
            var filePanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var fileLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(5)
            };
            fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

            _filePathBox = new TextEdit { Dock = DockStyle.Fill, Properties = { NullValuePrompt = "Select CSV file..." } };
            var btnBrowse = new SimpleButton { Text = "Browse", Dock = DockStyle.Fill };
            btnBrowse.Click += BtnBrowse_Click;
            fileLayout.Controls.Add(_filePathBox, 0, 0);
            fileLayout.Controls.Add(btnBrowse, 1, 0);
            filePanel.Controls.Add(fileLayout);
            mainLayout.Controls.Add(filePanel, 0, 0);

            // Grid preview
            _grid = new GridControl { Dock = DockStyle.Fill };
            _view = new GridView(_grid) { OptionsSelection = { MultiSelect = false }, OptionsView = { ShowGroupPanel = false } };
            _grid.MainView = _view;
            _grid.ViewCollection.Add(_view);
            mainLayout.Controls.Add(_grid, 0, 1);

            // Error summary
            _errorMemo = new MemoEdit { Dock = DockStyle.Fill, Properties = { ReadOnly = true, Appearance = { BackColor = Color.FromArgb(255, 240, 240) } } };
            mainLayout.Controls.Add(_errorMemo, 0, 2);

            // Buttons
            var buttonPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var btnImport = new SimpleButton { Text = "Import", Location = new Point(10, 10), Width = 100, Height = 30 };
            btnImport.Click += BtnImport_Click;
            var btnCancel = new SimpleButton { Text = "Cancel", Location = new Point(130, 10), Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            mainLayout.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(mainLayout);
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select Supplier Import File"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _filePathBox.EditValue = dialog.FileName;
                TryParseCsv(dialog.FileName);
            }
        }

        private void TryParseCsv(string filePath)
        {
            _rows.Clear();
            _errorMemo.Text = "";

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2) { _errorMemo.Text = "File is empty or has only a header."; return; }

                // Detect if first line is header
                var firstLine = lines[0].Trim();
                var hasHeader = !string.IsNullOrWhiteSpace(firstLine) && (firstLine.Contains("Name") || firstLine.Contains("Code"));

                int startIndex = hasHeader ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        _rows.Add(new ImportSupplierRow
                        {
                            Code = parts[0].Trim(),
                            Name = parts[1].Trim(),
                            City = parts.Length > 2 ? parts[2].Trim() : "",
                            Country = parts.Length > 3 ? parts[3].Trim() : ""
                        });
                    }
                }

                _grid.DataSource = _rows;
                _view.BestFitColumns();
                _errorMemo.Text = $"Loaded {_rows.Count} supplier(s) from file.";
            }
            catch (Exception ex)
            {
                _errorMemo.Text = $"Error parsing file: {ex.Message}";
                _rows.Clear();
                _grid.DataSource = null;
            }
        }

        private async void BtnImport_Click(object? sender, EventArgs e)
        {
            if (_rows.Count == 0) { ShowInfo("No suppliers to import."); return; }

            var confirm = ShowConfirm($"Import {_rows.Count} suppliers into the database?");
            if (!confirm) return;

            try
            {
                int imported = 0, skipped = 0;
                foreach (var row in _rows)
                {
                    // Check if supplier already exists (by code or name)
                    var allSuppliers = await _supplierService.GetAllAsync(CancellationToken);
                    var exists = allSuppliers.FirstOrDefault(s => s.Code == row.Code || s.Name == row.Name);

                    if (exists != null)
                    {
                        skipped++;
                        continue;
                    }

                    var supplier = new Supplier
                    {
                        Code = row.Code,
                        Name = row.Name,
                        City = string.IsNullOrWhiteSpace(row.City) ? null : row.City,
                        Country = string.IsNullOrWhiteSpace(row.Country) ? null : row.Country,
                        IsActive = true
                    };

                    await _supplierService.AddAsync(supplier, CancellationToken);
                    imported++;
                }

                ShowInfo($"Import complete: {imported} imported, {skipped} skipped (duplicates).");
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError($"Import failed: {ex.Message}");
            }
        }

        private class ImportSupplierRow
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string City { get; set; } = "";
            public string Country { get; set; } = "";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { this.DialogResult = DialogResult.Cancel; return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}