using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.WinForms;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Form for changing the current user's password.
    /// Requires the current password to be entered.
    /// </summary>
    public partial class ChangePasswordForm : AsyncFormBase
    {
        private readonly IUserService _userService;
        private readonly IUserSession _userSession;

        // UI Controls
        private PanelControl _mainPanel = null!;
        private LabelControl _titleLabel = null!;
        private LabelControl _subtitleLabel = null!;
        private TextEdit _currentPasswordEdit = null!;
        private TextEdit _newPasswordEdit = null!;
        private TextEdit _confirmPasswordEdit = null!;
        private SimpleButton _changeButton = null!;
        private SimpleButton _cancelButton = null!;
        private LabelControl _statusLabel = null!;

        public ChangePasswordForm(
            IUserService userService,
            IUserSession userSession)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "Change Password";
            this.Size = new System.Drawing.Size(440, 480);
            this.MinimumSize = new System.Drawing.Size(440, 480);
            this.MaximumSize = new System.Drawing.Size(440, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.KeyPreview = true;

            // Main panel with padding
            _mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(40, 30, 40, 30)
            };
            this.Controls.Add(_mainPanel);

            // Title
            _titleLabel = new LabelControl
            {
                Text = "Change Password",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 50,
                Appearance = { TextOptions = { HAlignment = HorzAlignment.Center } },
                Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_titleLabel);

            // Subtitle
            _subtitleLabel = new LabelControl
            {
                Text = "Enter your current and new password",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 30,
                Appearance = { TextOptions = { HAlignment = HorzAlignment.Center }, ForeColor = System.Drawing.Color.Gray },
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_subtitleLabel);

            // Spacer
            var spacer1 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 20,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer1);

            // Current password label
            var currentPasswordLabel = new LabelControl
            {
                Text = "Current Password",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(currentPasswordLabel);

            // Current password edit
            _currentPasswordEdit = new TextEdit
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Properties =
                {
                    AutoHeight = false,
                    PasswordChar = '●',
                    Padding = new Padding(10, 0, 10, 0),
                    NullValuePrompt = "Enter current password",
                    NullValuePromptShowForEmptyValue = true,
                    UseSystemPasswordChar = true
                }
            };
            _currentPasswordEdit.KeyDown += OnKeyDown;
            _mainPanel.Controls.Add(_currentPasswordEdit);

            // Spacer
            var spacer2 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 15,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer2);

            // New password label
            var newPasswordLabel = new LabelControl
            {
                Text = "New Password",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(newPasswordLabel);

            // New password edit
            _newPasswordEdit = new TextEdit
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Properties =
                {
                    AutoHeight = false,
                    PasswordChar = '●',
                    Padding = new Padding(10, 0, 10, 0),
                    NullValuePrompt = "Enter new password (min 6 characters)",
                    NullValuePromptShowForEmptyValue = true,
                    UseSystemPasswordChar = true
                }
            };
            _newPasswordEdit.KeyDown += OnKeyDown;
            _mainPanel.Controls.Add(_newPasswordEdit);

            // Spacer
            var spacer3 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 15,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer3);

            // Confirm password label
            var confirmPasswordLabel = new LabelControl
            {
                Text = "Confirm New Password",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(confirmPasswordLabel);

            // Confirm password edit
            _confirmPasswordEdit = new TextEdit
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Properties =
                {
                    AutoHeight = false,
                    PasswordChar = '●',
                    Padding = new Padding(10, 0, 10, 0),
                    NullValuePrompt = "Confirm new password",
                    NullValuePromptShowForEmptyValue = true,
                    UseSystemPasswordChar = true
                }
            };
            _confirmPasswordEdit.KeyDown += OnKeyDown;
            _mainPanel.Controls.Add(_confirmPasswordEdit);

            // Spacer
            var spacer4 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 20,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer4);

            // Change button
            _changeButton = new SimpleButton
            {
                Text = "Change Password",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point),
                Appearance =
                {
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White
                }
            };
            _changeButton.Click += OnChangeClick;
            _mainPanel.Controls.Add(_changeButton);

            // Cancel button
            _cancelButton = new SimpleButton
            {
                Text = "Cancel",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Appearance =
                {
                    BackColor = System.Drawing.Color.Transparent,
                    ForeColor = System.Drawing.Color.FromArgb(100, 100, 100)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
            };
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            _mainPanel.Controls.Add(_cancelButton);

            // Spacer
            var spacer5 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 15,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer5);

            // Status label
            _statusLabel = new LabelControl
            {
                Text = "",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 24,
                Appearance = { TextOptions = { HAlignment = HorzAlignment.Center }, ForeColor = System.Drawing.Color.Red },
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_statusLabel);

            // Focus current password field on load
            this.Load += (s, e) => _currentPasswordEdit.Focus();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OnChangeClick(_changeButton, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private async void OnChangeClick(object? sender, EventArgs e)
        {
            var currentPassword = _currentPasswordEdit.Text;
            var newPassword = _newPasswordEdit.Text;
            var confirmPassword = _confirmPasswordEdit.Text;

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                ShowStatus("Please enter your current password", true);
                _currentPasswordEdit.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowStatus("Please enter a new password", true);
                _newPasswordEdit.Focus();
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowStatus("New password must be at least 6 characters", true);
                _newPasswordEdit.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowStatus("New passwords do not match", true);
                _confirmPasswordEdit.Focus();
                return;
            }

            if (currentPassword == newPassword)
            {
                ShowStatus("New password must be different from current password", true);
                _newPasswordEdit.Focus();
                return;
            }

            _changeButton.Enabled = false;
            _cancelButton.Enabled = false;
            _currentPasswordEdit.Enabled = false;
            _newPasswordEdit.Enabled = false;
            _confirmPasswordEdit.Enabled = false;
            ShowStatus("Changing password...", false);

            try
            {
                await RunAsync(async ct =>
                {
                    if (!_userSession.IsAuthenticated)
                        throw new InvalidOperationException("No authenticated user");

                    var success = await _userService.ChangePasswordAsync(
                        _userSession.CurrentUser!.Id,
                        currentPassword,
                        newPassword,
                        ct);

                    if (!success)
                        throw new UnauthorizedAccessException("Current password is incorrect");
                });

                // Success
                ShowStatus("Password changed successfully!", false);
                await Task.Delay(1000);
                this.DialogResult = DialogResult.OK;
            }
            catch (ArgumentException ex)
            {
                ShowStatus(ex.Message, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowStatus(ex.Message, true);
                _currentPasswordEdit.Text = "";
                _currentPasswordEdit.Focus();
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to change password: {ex.Message}", true);
            }
            finally
            {
                _changeButton.Enabled = true;
                _cancelButton.Enabled = true;
                _currentPasswordEdit.Enabled = true;
                _newPasswordEdit.Enabled = true;
                _confirmPasswordEdit.Enabled = true;
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, bool>(ShowStatus), message, isError);
                return;
            }

            _statusLabel.Text = message;
            _statusLabel.Appearance.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }
    }
}
