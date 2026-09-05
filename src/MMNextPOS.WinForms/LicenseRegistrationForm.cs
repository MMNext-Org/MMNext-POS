using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Application.Services;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// First-run / re-activation screen for the product license. Captures
    /// company details, validates the key shape, and binds the current
    /// device in a single <see cref="ILicenseInfoService.ActivateAsync"/>
    /// call. Returns <see cref="DialogResult.OK"/> only when activation
    /// succeeds; <see cref="DialogResult.Cancel"/> otherwise.
    /// </summary>
    public partial class LicenseRegistrationForm : AsyncFormBase
    {
        private readonly ILicenseInfoService _licenseService;
        private readonly IDeviceFingerprintService _fingerprintService;

        private LabelControl _titleLabel = null!;
        private LabelControl _subtitleLabel = null!;
        private LabelControl _statusLabel = null!;
        private TextEdit _licenseKeyEdit = null!;
        private TextEdit _companyEdit = null!;
        private TextEdit _contactEdit = null!;
        private TextEdit _emailEdit = null!;
        private TextEdit _phoneEdit = null!;
        private MemoEdit _addressEdit = null!;
        private SpinEdit _maxUsersEdit = null!;
        private SpinEdit _maxDevicesEdit = null!;
        private SpinEdit _subscriptionDaysEdit = null!;
        private TextEdit _fingerprintEdit = null!;
        private SimpleButton _activateButton = null!;
        private SimpleButton _cancelButton = null!;
        private SimpleButton _copyFingerprintButton = null!;

        private DeviceFingerprint _fingerprint;

        public LicenseRegistrationForm(
            ILicenseInfoService licenseService,
            IDeviceFingerprintService fingerprintService)
        {
            _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
            _fingerprintService = fingerprintService ?? throw new ArgumentNullException(nameof(fingerprintService));
            _fingerprint = fingerprintService.GetCurrent();

            InitializeComponent();
        }

        /// <summary>
        /// Optional pre-population from a prior <see cref="LicenseStatus"/>.
        /// Used by <c>Program.cs</c> to surface the reason the guard rejected
        /// the current install and to lock the form when cancellation is
        /// not an option (e.g. the app cannot run without a license).
        /// </summary>
        public void Prepopulate(LicenseStatus status)
        {
            if (status == null) return;

            if (status.Fingerprint != null)
            {
                _fingerprint = status.Fingerprint;
                _fingerprintEdit.Text = status.Fingerprint.Hash;
            }

            if (!status.IsValid)
            {
                ShowStatus(status.Message, isError: true);
            }

            // Pre-fill from the on-file license when we already have one
            // (re-activation flow after expiry / device wipe).
            if (status.License != null)
            {
                _companyEdit.Text = status.License.CompanyName;
                _contactEdit.Text = status.License.ContactPerson;
                _emailEdit.Text = status.License.Email;
                _phoneEdit.Text = status.License.Phone;
                _addressEdit.Text = status.License.Address;
                _maxUsersEdit.Value = status.License.MaxUsers;
                _maxDevicesEdit.Value = status.License.MaxDevices;
            }
        }

        private void InitializeComponent()
        {
            Text = "MMNext POS — License Registration";
            Size = new Size(640, 640);
            MinimumSize = new Size(640, 640);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 11; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Title banner (spans both columns)
            _titleLabel = new LabelControl
            {
                Text = "Activate MMNext POS",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point)
            };
            _subtitleLabel = new LabelControl
            {
                Text = "Enter your license key and company details. This device will be bound to the license.",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 24,
                Appearance = { ForeColor = Color.DimGray },
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            var headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 64,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            headerPanel.Controls.Add(_titleLabel);
            headerPanel.Controls.Add(_subtitleLabel);
            this.Controls.Add(headerPanel);

            // License Key
            mainLayout.Controls.Add(CreateLabel("License Key *:"), 0, 0);
            _licenseKeyEdit = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    MaxLength = 100,
                    CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
                },
                Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };
            _licenseKeyEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_licenseKeyEdit, 1, 0);

            // Company
            mainLayout.Controls.Add(CreateLabel("Company Name *:"), 0, 1);
            _companyEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            _companyEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_companyEdit, 1, 1);

            // Contact
            mainLayout.Controls.Add(CreateLabel("Contact Person:"), 0, 2);
            _contactEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_contactEdit, 1, 2);

            // Email
            mainLayout.Controls.Add(CreateLabel("Email:"), 0, 3);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_emailEdit, 1, 3);

            // Phone
            mainLayout.Controls.Add(CreateLabel("Phone:"), 0, 4);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_phoneEdit, 1, 4);

            // Address
            mainLayout.Controls.Add(CreateLabel("Address:"), 0, 5);
            _addressEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MaxLength = 300 }
            };
            mainLayout.Controls.Add(_addressEdit, 1, 5);

            // Max Users
            mainLayout.Controls.Add(CreateLabel("Max Users *:"), 0, 6);
            _maxUsersEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 1, MaxValue = 500, Increment = 1 }
            };
            _maxUsersEdit.Value = 5;
            mainLayout.Controls.Add(_maxUsersEdit, 1, 6);

            // Max Devices
            mainLayout.Controls.Add(CreateLabel("Max Devices *:"), 0, 7);
            _maxDevicesEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 1, MaxValue = 50, Increment = 1 }
            };
            _maxDevicesEdit.Value = 1;
            mainLayout.Controls.Add(_maxDevicesEdit, 1, 7);

            // Subscription Days
            mainLayout.Controls.Add(CreateLabel("Subscription Days *:"), 0, 8);
            _subscriptionDaysEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 1, MaxValue = 3650, Increment = 30 }
            };
            _subscriptionDaysEdit.Value = 365;
            mainLayout.Controls.Add(_subscriptionDaysEdit, 1, 8);

            // Device fingerprint (read-only) + copy button
            mainLayout.Controls.Add(CreateLabel("Device Fingerprint:"), 0, 9);
            var fpPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _fingerprintEdit = new TextEdit
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            _fingerprintEdit.Text = _fingerprint.Hash;
            _copyFingerprintButton = new SimpleButton
            {
                Text = "Copy",
                Width = 70,
                Height = 28
            };
            _copyFingerprintButton.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(_fingerprint.Hash);
                    ShowStatus("Fingerprint copied to clipboard.", isError: false);
                }
                catch (Exception ex)
                {
                    ShowStatus("Could not copy to clipboard: " + ex.Message, isError: true);
                }
            };
            fpPanel.Controls.Add(_fingerprintEdit);
            fpPanel.Controls.Add(_copyFingerprintButton);
            fpPanel.Layout += (s, e) =>
            {
                _fingerprintEdit.Location = new Point(0, 6);
                _fingerprintEdit.Width = fpPanel.Width - _copyFingerprintButton.Width - 8;
                _copyFingerprintButton.Location = new Point(_fingerprintEdit.Width + 8, 6);
            };
            mainLayout.Controls.Add(fpPanel, 1, 9);

            // Status label
            _statusLabel = new LabelControl
            {
                Text = string.Empty,
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.Red },
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            mainLayout.Controls.Add(_statusLabel, 0, 10);
            mainLayout.SetColumnSpan(_statusLabel, 2);

            // Buttons (custom — we want to drive OK ourselves once activation
            // succeeds, so we don't inherit EditFormBase).
            _activateButton = new SimpleButton
            {
                Text = "Activate",
                Width = 130,
                Height = 36,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
                Appearance = { BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White }
            };
            _activateButton.Click += OnActivateClick;

            _cancelButton = new SimpleButton
            {
                Text = "Exit",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 36
            };

            var buttonPanel = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            buttonPanel.Controls.Add(_activateButton);
            buttonPanel.Controls.Add(_cancelButton);
            this.Controls.Add(buttonPanel);
            this.Layout += (s, e) =>
            {
                _activateButton.Location = new Point(ClientSize.Width - 240, 10);
                _cancelButton.Location = new Point(ClientSize.Width - 100, 10);
            };

            this.Controls.Add(mainLayout);
            this.CancelButton = _cancelButton;
            this.AcceptButton = _activateButton;

            ValidateForm();
        }

        private LabelControl CreateLabel(string text) =>
            new()
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } },
                Padding = new Padding(0, 0, 10, 0)
            };

        protected override bool ValidateForm()
        {
            var keyOk = !string.IsNullOrWhiteSpace(_licenseKeyEdit.Text) && _licenseKeyEdit.Text.Trim().Length >= 8;
            var companyOk = !string.IsNullOrWhiteSpace(_companyEdit.Text);
            _activateButton.Enabled = keyOk && companyOk;
            return _activateButton.Enabled;
        }

        private void ShowStatus(string message, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, bool>(ShowStatus), message, isError);
                return;
            }
            _statusLabel.Text = message ?? string.Empty;
            _statusLabel.Appearance.ForeColor = isError ? Color.Red : Color.SeaGreen;
        }

        private async void OnActivateClick(object? sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                ShowStatus("Please enter a valid license key (8+ chars) and company name.", isError: true);
                return;
            }

            _activateButton.Enabled = false;
            _cancelButton.Enabled = false;
            ShowStatus("Activating license…", isError: false);
            SetWaitCursor(true);

            try
            {
                var request = new LicenseActivationRequest(
                    LicenseKey: _licenseKeyEdit.Text.Trim(),
                    CompanyName: _companyEdit.Text.Trim(),
                    ContactPerson: _contactEdit.Text?.Trim(),
                    Email: _emailEdit.Text?.Trim(),
                    Phone: _phoneEdit.Text?.Trim(),
                    Address: _addressEdit.Text?.Trim(),
                    MaxUsers: (int)_maxUsersEdit.Value,
                    MaxDevices: (int)_maxDevicesEdit.Value,
                    SubscriptionDays: (int)_subscriptionDaysEdit.Value);

                var license = await _licenseService.ActivateAsync(request, CancellationToken).ConfigureAwait(true);
                ShowStatus(
                    $"License activated. Expires {license.ExpiryDate:yyyy-MM-dd}.",
                    isError: false);
                this.DialogResult = DialogResult.OK;
            }
            catch (OperationCanceledException)
            {
                ShowStatus("Activation cancelled.", isError: true);
            }
            catch (Exception ex)
            {
                ShowStatus("Activation failed: " + ex.Message, isError: true);
            }
            finally
            {
                SetWaitCursor(false);
                _cancelButton.Enabled = true;
                ValidateForm();
            }
        }
    }
}
