using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public class OutstandingForm : AsyncFormBase
    {
        private readonly IOutstandingService _outstandingService;
        private readonly ISupplierService _supplierService;
        private readonly ICustomerService _customerService;

        private SupplierOutstanding? _currentSupplierOutstanding;
        private CustomerOutstanding? _currentCustomerOutstanding;
        private bool _isNewMode = true;
        private bool _isSupplierOutstanding;

        private LookUpEdit _partyLookup = null!;
        private DateEdit _transactionDateEdit = null!;
        private SpinEdit _debitAmountEdit = null!;
        private SpinEdit _creditAmountEdit = null!;
        private TextEdit _descriptionEdit = null!;
        private ComboBoxEdit _statusCombo = null!;
        private SimpleButton _saveButton = null!;
        private SimpleButton _cancelButton = null!;

        public OutstandingForm(
            IOutstandingService outstandingService,
            ISupplierService supplierService,
            bool isSupplierOutstanding = true)
        {
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _customerService = null!;
            _isSupplierOutstanding = isSupplierOutstanding;

            InitializeComponent();
        }

        public OutstandingForm(
            IOutstandingService outstandingService,
            ICustomerService customerService,
            bool isSupplierOutstanding = false)
        {
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
            _supplierService = null!;
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _isSupplierOutstanding = isSupplierOutstanding;

            InitializeComponent();
        }

        public OutstandingForm(
            IOutstandingService outstandingService,
            ISupplierService supplierService,
            SupplierOutstanding outstanding,
            bool isSupplierOutstanding = true) : this(outstandingService, supplierService, isSupplierOutstanding)
        {
            _currentSupplierOutstanding = outstanding;
            _isNewMode = false;
        }

        public OutstandingForm(
            IOutstandingService outstandingService,
            ICustomerService customerService,
            CustomerOutstanding outstanding,
            bool isSupplierOutstanding = false) : this(outstandingService, customerService, isSupplierOutstanding)
        {
            _currentCustomerOutstanding = outstanding;
            _isNewMode = false;
        }

        private void InitializeComponent()
        {
            var entityName = _isSupplierOutstanding ? "Supplier" : "Customer";
            Text = _isNewMode ? $"New {entityName} Outstanding Entry" : $"Edit {entityName} Outstanding";
            Size = new Size(600, 450);
            MinimumSize = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

            var scrollPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var scrollableControl = new XtraScrollableControl { Dock = DockStyle.Fill };
            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10),
                AutoSize = true
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 6; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            }

            // Party (Supplier or Customer)
            formLayout.Controls.Add(CreateLabel($"{entityName} *:"), 0, 0);
            _partyLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = $"Select {entityName.ToLower()}...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            _partyLookup.EditValueChanged += (_, _) => ValidateForm();
            formLayout.Controls.Add(_partyLookup, 1, 0);

            // Transaction Date
            formLayout.Controls.Add(CreateLabel("Transaction Date *:"), 0, 1);
            _transactionDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                EditValue = DateTime.Today,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            _transactionDateEdit.EditValueChanged += (_, _) => ValidateForm();
            formLayout.Controls.Add(_transactionDateEdit, 1, 1);

            // Debit Amount
            var debitLabel = _isSupplierOutstanding ? "Amount Paid (Debit) *:" : "Amount Owed (Debit) *:";
            formLayout.Controls.Add(CreateLabel(debitLabel), 0, 2);
            _debitAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    MinValue = 0,
                    MaxValue = 999999999,
                    IsFloatValue = true,
                    Increment = 0.01m
                }
            };
            _debitAmountEdit.EditValueChanged += (_, _) => ValidateForm();
            formLayout.Controls.Add(_debitAmountEdit, 1, 2);

            // Credit Amount
            var creditLabel = _isSupplierOutstanding ? "Amount Owed (Credit) *:" : "Amount Paid (Credit) *:";
            formLayout.Controls.Add(CreateLabel(creditLabel), 0, 3);
            _creditAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    MinValue = 0,
                    MaxValue = 999999999,
                    IsFloatValue = true,
                    Increment = 0.01m
                }
            };
            _creditAmountEdit.EditValueChanged += (_, _) => ValidateForm();
            formLayout.Controls.Add(_creditAmountEdit, 1, 3);

            // Description
            formLayout.Controls.Add(CreateLabel("Description:"), 0, 4);
            _descriptionEdit = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MaxLength = 500 }
            };
            formLayout.Controls.Add(_descriptionEdit, 1, 4);

            // Status
            formLayout.Controls.Add(CreateLabel("Status:"), 0, 5);
            _statusCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Open", "Closed" }
                }
            };
            _statusCombo.SelectedIndex = 0;
            formLayout.Controls.Add(_statusCombo, 1, 5);

            scrollableControl.Controls.Add(formLayout);
            scrollPanel.Controls.Add(scrollableControl);
            mainLayout.Controls.Add(scrollPanel, 0, 0);

            // Button Panel
            var buttonPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            _saveButton = new SimpleButton
            {
                Text = _isNewMode ? "Save" : "Update",
                Location = new Point(10, 20),
                Width = 120,
                Height = 35,
                Enabled = false
            };
            _saveButton.Click += OnSaveClick;

            _cancelButton = new SimpleButton
            {
                Text = "Cancel",
                Location = new Point(140, 20),
                Width = 100,
                Height = 35
            };
            _cancelButton.Click += (_, _) => Close();

            buttonPanel.Controls.Add(_saveButton);
            buttonPanel.Controls.Add(_cancelButton);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            Controls.Add(mainLayout);

            Load += OnFormLoad;
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

        private async void OnFormLoad(object? sender, EventArgs e)
        {
            await LoadParties();
            if (!_isNewMode)
            {
                LoadOutstandingData();
            }
            ValidateForm();
        }

        private async Task LoadParties()
        {
            try
            {
                SetWaitCursor(true);
                if (_isSupplierOutstanding && _supplierService != null)
                {
                    var suppliers = await _supplierService.GetAllAsync(CancellationToken);
                    _partyLookup.Properties.DataSource = suppliers.Where(s => s.IsActive).ToList();
                }
                else if (!_isSupplierOutstanding && _customerService != null)
                {
                    var customers = await _customerService.GetAllAsync(CancellationToken);
                    _partyLookup.Properties.DataSource = customers.Where(c => c.IsActive).ToList();
                }

                _partyLookup.Properties.PopulateColumns();
                _partyLookup.Properties.Columns["Id"].Visible = false;
                _partyLookup.Properties.Columns["Code"].Visible = false;
                _partyLookup.Properties.Columns["Address"].Visible = false;
                _partyLookup.Properties.Columns["City"].Visible = false;
                _partyLookup.Properties.Columns["Country"].Visible = false;
                _partyLookup.Properties.Columns["Phone"].Visible = false;
                _partyLookup.Properties.Columns["Email"].Visible = false;
                _partyLookup.Properties.Columns["ContactPerson"].Visible = false;
                _partyLookup.Properties.Columns["TaxId"].Visible = false;
                _partyLookup.Properties.Columns["CreditLimit"].Visible = false;
                _partyLookup.Properties.Columns["PaymentTermDays"].Visible = false;
                _partyLookup.Properties.Columns["IsActive"].Visible = false;
                _partyLookup.Properties.Columns["CreatedAt"].Visible = false;
                _partyLookup.Properties.Columns["UpdatedAt"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load {entityName.ToLower()}s: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private string entityName => _isSupplierOutstanding ? "Supplier" : "Customer";

        private void LoadOutstandingData()
        {
            if (_isSupplierOutstanding)
            {
                if (_currentSupplierOutstanding == null) return;

                _transactionDateEdit.EditValue = _currentSupplierOutstanding.TransactionDate;
                _debitAmountEdit.Value = _currentSupplierOutstanding.DebitAmount;
                _creditAmountEdit.Value = _currentSupplierOutstanding.CreditAmount;
                _descriptionEdit.Text = _currentSupplierOutstanding.Description ?? string.Empty;
                _statusCombo.Text = _currentSupplierOutstanding.Status;

                if (_partyLookup.Properties.DataSource != null)
                {
                    _partyLookup.EditValue = _currentSupplierOutstanding.SupplierId;
                }
            }
            else
            {
                if (_currentCustomerOutstanding == null) return;

                _transactionDateEdit.EditValue = _currentCustomerOutstanding.TransactionDate;
                _debitAmountEdit.Value = _currentCustomerOutstanding.DebitAmount;
                _creditAmountEdit.Value = _currentCustomerOutstanding.CreditAmount;
                _descriptionEdit.Text = _currentCustomerOutstanding.Description ?? string.Empty;
                _statusCombo.Text = _currentCustomerOutstanding.Status;

                if (_partyLookup.Properties.DataSource != null)
                {
                    _partyLookup.EditValue = _currentCustomerOutstanding.CustomerId;
                }
            }
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            // Required: Party
            if (_partyLookup.EditValue == null || _partyLookup.EditValue == DBNull.Value)
                isValid = false;

            // Required: Transaction Date
            if (_transactionDateEdit.EditValue == null)
                isValid = false;

            // At least one of debit or credit must be > 0
            if (_debitAmountEdit.Value <= 0 && _creditAmountEdit.Value <= 0)
                isValid = false;

            _saveButton.Enabled = isValid;
            return isValid;
        }

        private async void OnSaveClick(object? sender, EventArgs e)
        {
            if (!_saveButton.Enabled) return;

            try
            {
                await RunAsync(async ct =>
                {
                    if (_isSupplierOutstanding)
                    {
                        var outstanding = _currentSupplierOutstanding ?? new SupplierOutstanding();

                        outstanding.SupplierId = Convert.ToInt32(_partyLookup.EditValue);
                        outstanding.TransactionDate = _transactionDateEdit.DateTime;
                        outstanding.DebitAmount = _debitAmountEdit.Value;
                        outstanding.CreditAmount = _creditAmountEdit.Value;
                        outstanding.Description = string.IsNullOrWhiteSpace(_descriptionEdit.Text) ? null : _descriptionEdit.Text;
                        outstanding.Status = _statusCombo.Text;

                        if (_isNewMode)
                        {
                            await _outstandingService.AddSupplierOutstandingAsync(outstanding, ct);
                            ShowInfo("Supplier outstanding saved successfully.");
                        }
                        else
                        {
                            await _outstandingService.UpdateSupplierOutstandingAsync(outstanding, ct);
                            ShowInfo("Supplier outstanding updated successfully.");
                        }
                    }
                    else
                    {
                        var outstanding = _currentCustomerOutstanding ?? new CustomerOutstanding();

                        outstanding.CustomerId = Convert.ToInt32(_partyLookup.EditValue);
                        outstanding.TransactionDate = _transactionDateEdit.DateTime;
                        outstanding.DebitAmount = _debitAmountEdit.Value;
                        outstanding.CreditAmount = _creditAmountEdit.Value;
                        outstanding.Description = string.IsNullOrWhiteSpace(_descriptionEdit.Text) ? null : _descriptionEdit.Text;
                        outstanding.Status = _statusCombo.Text;

                        if (_isNewMode)
                        {
                            await _outstandingService.AddCustomerOutstandingAsync(outstanding, ct);
                            ShowInfo("Customer outstanding saved successfully.");
                        }
                        else
                        {
                            await _outstandingService.UpdateCustomerOutstandingAsync(outstanding, ct);
                            ShowInfo("Customer outstanding updated successfully.");
                        }
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                });
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save outstanding: {ex.Message}");
            }
        }
    }
}
