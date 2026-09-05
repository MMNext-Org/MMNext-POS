using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.WinForms;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Form for admin to reset another user's password.
    /// Does not require the current password.
    /// </summary>
    public partial class ResetPasswordForm : AsyncFormBase
    {
        private readonly IUserService _userService;
        private readonly IUserSession _userSession;

        // UI Controls
        private PanelControl _mainPanel = null!;
        private LabelControl _titleLabel = null!;
        private LabelControl _subtitleLabel = null!;
        private LookUpEdit _userLookup = null!;
        private TextEdit _newPasswordEdit = null!;
        private TextEdit _confirmPasswordEdit = null!;
        private CheckEdit _showPasswordCheck = null!;
        private SimpleButton _resetButton = null!;
        private SimpleButton _cancelButton = null!;
        private LabelControl _statusLabel = null!;

        private List<User> _users = new();

        public ResetPasswordForm(
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
            this.Text = "Reset User Password";
            this.Size = new System.Drawing.Size(480, 520);
            this.MinimumSize = new System.Drawing.Size(480, 520);
            this.MaximumSize = new System.Drawing.Size(480, 520);
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
                Text = "Reset User Password",
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
                Text = "Select a user and set a new password",
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

            // User selection label
            var userLabel = new LabelControl
            {
                Text = "Select User",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(userLabel);

            // User lookup
            _userLookup = new LookUpEdit
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Properties =
                {
                    AutoHeight = false,
                    Padding = new Padding(10, 0, 10, 0),
                    NullText = "Select a user...",
                    DisplayMember = "DisplayName",
                    ValueMember = "Id",
                    ShowHeader = false,
                    PopupFormMinSize = new System.Drawing.Size(400, 300),
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoComplete,
                    AutoSearchColumnIndex = 0
                }
            };
            _mainPanel.Controls.Add(_userLookup);

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
                Height = 10,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer4);

            // Show password checkbox
            _showPasswordCheck = new CheckEdit
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Show password",
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _showPasswordCheck.CheckedChanged += (s, e) =>
            {
                var useSystemChar = !_showPasswordCheck.Checked;
                _newPasswordEdit.Properties.UseSystemPasswordChar = useSystemChar;
                _confirmPasswordEdit.Properties.UseSystemPasswordChar = useSystemChar;
            };
            _mainPanel.Controls.Add(_showPasswordCheck);

            // Spacer
            var spacer5 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 20,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer5);

            // Reset button
            _resetButton = new SimpleButton
            {
                Text = "Reset Password",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point),
                Appearance =
                {
                    BackColor = System.Drawing.Color.FromArgb(200, 80, 80),
                    ForeColor = System.Drawing.Color.White
                }
            };
            _resetButton.Click += OnResetClick;
            _mainPanel.Controls.Add(_resetButton);

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
            var spacer6 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 15,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer6);

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

            // Load users on form load
            this.Load += async (s, e) => await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                await RunAsync(async ct =>
                {
                    var users = await _userService.GetAllAsync(ct);
                    _users = users.Where(u => u.IsActive && !u.IsDeleted).OrderBy(u => u.Username).ToList();

                    // Add DisplayName property for lookup
                    var displayList = _users.Select(u => new
                    {
                        Id = u.Id,
                        DisplayName = $"{u.Username} ({u.FullName})"
                    }).ToList();

                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => BindUsers(displayList)));
                    }
                    else
                    {
                        BindUsers(displayList);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load users: {ex.Message}", true);
            }
        }

        private void BindUsers<T>(IEnumerable<T> users) where T : class
        {
            _userLookup.Properties.DataSource = users;
            _userLookup.Properties.DisplayMember = "DisplayName";
            _userLookup.Properties.ValueMember = "Id";
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OnResetClick(_resetButton, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private async void OnResetClick(object? sender, EventArgs e)
        {
            var userId = _userLookup.EditValue as int?;
            var newPassword = _newPasswordEdit.Text;
            var confirmPassword = _confirmPasswordEdit.Text;

            if (!userId.HasValue)
            {
                ShowStatus("Please select a user", true);
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

            // Prevent admin from resetting their own password through this form
            if (_userSession.IsAuthenticated && userId.Value == _userSession.CurrentUser!.Id)
            {
                ShowStatus("Use 'Change Password' to change your own password", true);
                return;
            }

            _resetButton.Enabled = false;
            _cancelButton.Enabled = false;
            _userLookup.Enabled = false;
            _newPasswordEdit.Enabled = false;
            _confirmPasswordEdit.Enabled = false;
            _showPasswordCheck.Enabled = false;
            ShowStatus("Resetting password...", false);

            try
            {
                await RunAsync(async ct =>
                {
                    var success = await _userService.ResetPasswordAsync(userId.Value, newPassword, ct);

                    if (!success)
                        throw new InvalidOperationException("User not found or inactive");
                });

                // Success
                var selectedUser = _users.FirstOrDefault(u => u.Id == userId.Value);
                ShowStatus($"Password reset for '{selectedUser?.Username}' successfully!", false);
                await Task.Delay(1500);
                this.DialogResult = DialogResult.OK;
            }
            catch (ArgumentException ex)
            {
                ShowStatus(ex.Message, true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to reset password: {ex.Message}", true);
            }
            finally
            {
                _resetButton.Enabled = true;
                _cancelButton.Enabled = true;
                _userLookup.Enabled = true;
                _newPasswordEdit.Enabled = true;
                _confirmPasswordEdit.Enabled = true;
                _showPasswordCheck.Enabled = true;
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