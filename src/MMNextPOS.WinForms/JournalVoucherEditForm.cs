using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Editor for a Journal Voucher: header fields plus debit/credit line grid.
    /// The list page owns the service call; this form only captures/validates data.
    /// </summary>
    public partial class JournalVoucherEditForm : EditFormBase
    {
        private TextEdit _voucherNoEdit = null!;
        private DateEdit _voucherDateEdit = null!;
        private TextEdit _referenceNoEdit = null!;
        private MemoEdit _descriptionEdit = null!;
        private GridControl _linesGrid = null!;
        private GridView _linesView = null!;
        private BindingList<JournalVoucherDetail> _lines = null!;
        private LabelControl _totalDebitLabel = null!;
        private LabelControl _totalCreditLabel = null!;
        private LabelControl _differenceLabel = null!;

        public JournalVoucherEditForm()
        {
            InitializeComponent();
            InitializeLinesGrid();
            RecomputeTotals();

            Text = "New Journal Voucher";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;

            _okButton.Text = "Create";
        }

        private void InitializeComponent()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(20)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // Rows 0-4: fixed height header fields (50 px each).
            // Row 5: lines grid (fills remaining space).
            // Row 6: line action buttons (fixed height).
            for (int i = 0; i < 5; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Voucher No
            layout.Controls.Add(CreateLabel("Voucher #:"), 0, 0);
            _voucherNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            layout.Controls.Add(_voucherNoEdit, 1, 0);

            // Voucher Date
            layout.Controls.Add(CreateLabel("Date:"), 0, 1);
            _voucherDateEdit = new DateEdit { Dock = DockStyle.Fill, EditValue = DateTime.Today };
            layout.Controls.Add(_voucherDateEdit, 1, 1);

            // Reference No
            layout.Controls.Add(CreateLabel("Reference:"), 0, 2);
            _referenceNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            layout.Controls.Add(_referenceNoEdit, 1, 2);

            // Description
            layout.Controls.Add(CreateLabel("Description:"), 0, 3);
            _descriptionEdit = new MemoEdit { Dock = DockStyle.Fill };
            layout.Controls.Add(_descriptionEdit, 1, 3);

            // Totals
            _totalDebitLabel = new LabelControl { Text = "Debit: 0.00", Dock = DockStyle.Fill };
            _totalCreditLabel = new LabelControl { Text = "Credit: 0.00", Dock = DockStyle.Fill };
            _differenceLabel = new LabelControl { Text = "Difference: 0.00", Dock = DockStyle.Fill, Appearance = { ForeColor = Color.Red } };
            var totalsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            totalsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            totalsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            totalsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            totalsPanel.Controls.Add(_totalDebitLabel, 0, 0);
            totalsPanel.Controls.Add(_totalCreditLabel, 1, 0);
            totalsPanel.Controls.Add(_differenceLabel, 2, 0);
            layout.Controls.Add(CreateLabel("Totals:"), 0, 4);
            layout.Controls.Add(totalsPanel, 1, 4);

            // Lines grid
            layout.Controls.Add(CreateLabel("Lines:"), 0, 5);
            _linesGrid = new GridControl { Dock = DockStyle.Fill };
            _linesView = new GridView();
            _linesGrid.MainView = _linesView;
            _linesGrid.ViewCollection.Add(_linesView);
            layout.Controls.Add(_linesGrid, 1, 5);

            // Line action buttons
            var buttonPanel = new PanelControl { Dock = DockStyle.Fill };
            var addLineBtn = new SimpleButton { Text = "Add Line", Dock = DockStyle.Left, Width = 90 };
            var removeLineBtn = new SimpleButton { Text = "Remove Line", Dock = DockStyle.Left, Width = 100 };
            addLineBtn.Click += (s, e) => AddLine();
            removeLineBtn.Click += (s, e) => RemoveLine();
            buttonPanel.Controls.Add(addLineBtn);
            buttonPanel.Controls.Add(removeLineBtn);
            layout.Controls.Add(buttonPanel, 1, 6);

            Controls.Add(layout);
        }

        private void InitializeLinesGrid()
        {
            _lines = new BindingList<JournalVoucherDetail>();
            _lines.ListChanged += (s, e) => RecomputeTotals();

            _linesView.Columns.Clear();
            _linesView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = nameof(JournalVoucherDetail.AccountCode), Caption = "Account Code", Width = 120 },
                new GridColumn { FieldName = nameof(JournalVoucherDetail.AccountName), Caption = "Account Name", Width = 200 },
                new GridColumn { FieldName = nameof(JournalVoucherDetail.DebitAmount), Caption = "Debit", Width = 120,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = nameof(JournalVoucherDetail.CreditAmount), Caption = "Credit", Width = 120,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = nameof(JournalVoucherDetail.Description), Caption = "Description", Width = 200 }
            });

            _linesGrid.DataSource = _lines;
            _linesView.CellValueChanged += (s, e) => RecomputeTotals();
        }

        private void AddLine()
        {
            _lines.Add(new JournalVoucherDetail
            {
                AccountCode = string.Empty,
                AccountName = string.Empty,
                DebitAmount = 0,
                CreditAmount = 0,
                LineNo = _lines.Count
            });
            _linesView.RefreshData();
        }

        private void RemoveLine()
        {
            if (_linesView.FocusedRowHandle >= 0)
            {
                var row = _linesView.GetRow(_linesView.FocusedRowHandle) as JournalVoucherDetail;
                if (row != null)
                    _lines.Remove(row);
            }
        }

        private void RecomputeTotals()
        {
            var debit = _lines.Sum(l => l.DebitAmount);
            var credit = _lines.Sum(l => l.CreditAmount);
            var diff = debit - credit;

            _totalDebitLabel.Text = $"Debit: {debit:C2}";
            _totalCreditLabel.Text = $"Credit: {credit:C2}";
            _differenceLabel.Text = $"Difference: {diff:C2}";
            _differenceLabel.Appearance.ForeColor = Math.Abs(diff) > 0.0001m ? Color.Red : Color.Green;
        }

        protected override bool ValidateForm()
        {
            var lines = _lines.ToList();
            if (lines.Count < 2)
            {
                ShowError("A journal voucher must have at least 2 lines.");
                return false;
            }

            foreach (var l in lines)
            {
                if (string.IsNullOrWhiteSpace(l.AccountCode))
                {
                    ShowError("Account code is required on every line.");
                    return false;
                }
                if (l.DebitAmount < 0 || l.CreditAmount < 0)
                {
                    ShowError("Debit and Credit cannot be negative.");
                    return false;
                }
                if (l.DebitAmount > 0 && l.CreditAmount > 0)
                {
                    ShowError("A line cannot have both Debit and Credit.");
                    return false;
                }
                if (l.DebitAmount == 0 && l.CreditAmount == 0)
                {
                    ShowError("Each line must have either Debit or Credit.");
                    return false;
                }
            }

            var totalDebit = lines.Sum(l => l.DebitAmount);
            var totalCredit = lines.Sum(l => l.CreditAmount);
            if (Math.Abs(totalDebit - totalCredit) > 0.0001m)
            {
                ShowError("Debit and Credit totals must balance.");
                return false;
            }

            return true;
        }

        public override void LoadEntityData(object entity)
        {
            if (entity is JournalVoucher v)
            {
                _voucherNoEdit.Text = v.VoucherNo;
                _voucherDateEdit.EditValue = v.VoucherDate;
                _referenceNoEdit.Text = v.ReferenceNo ?? string.Empty;
                _descriptionEdit.Text = v.Description ?? string.Empty;

                _lines.Clear();
                foreach (var d in v.Details ?? Enumerable.Empty<JournalVoucherDetail>())
                    _lines.Add(d);

                RecomputeTotals();

                // Edit vs new mode: title and OK button text.
                if (v.Id > 0)
                {
                    Text = $"Edit Voucher {v.VoucherNo}";
                    _okButton.Text = "Save";
                }
                else
                {
                    Text = "New Journal Voucher";
                    _okButton.Text = "Create";
                }
            }
        }

        public override void SaveEntityData(object entity)
        {
            if (entity is JournalVoucher v)
            {
                v.VoucherNo = _voucherNoEdit.Text.Trim();
                v.VoucherDate = _voucherDateEdit.DateTime;
                v.ReferenceNo = _referenceNoEdit.Text;
                v.Description = _descriptionEdit.Text;
                v.Details = _lines.ToList();
                // Totals are calculated by the service from the lines
            }
        }

        private LabelControl CreateLabel(string text)
        {
            return new LabelControl
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } },
                Padding = new Padding(0, 0, 10, 0)
            };
        }
    }
}
