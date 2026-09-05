using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.WinForms;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Login form with DevExpress styling and async authentication.
    /// </summary>
    public partial class LoginForm : AsyncFormBase
    {
        private readonly IUserService _userService;
        private readonly IUserSession _userSession;
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleService _roleService;

        // UI Controls
        private PanelControl _mainPanel = null!;
        private LabelControl _titleLabel = null!;
        private LabelControl _subtitleLabel = null!;
        private TextEdit _usernameEdit = null!;
        private TextEdit _passwordEdit = null!;
        private CheckEdit _rememberMeCheck = null!;
        private SimpleButton _loginButton = null!;
        private SimpleButton _cancelButton = null!;
        private LabelControl _versionLabel = null!;
        private LabelControl _statusLabel = null!;

        public LoginForm(
            IUserService userService,
            IUserSession userSession,
            IUserRoleService userRoleService,
            IRoleService roleService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
            _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "MMNext POS - Login";
            this.Size = new System.Drawing.Size(420, 520);
            this.MinimumSize = new System.Drawing.Size(420, 520);
            this.MaximumSize = new System.Drawing.Size(420, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = true;
            this.ShowInTaskbar = true;
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
                Text = "MMNext POS",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 60,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } },
                Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_titleLabel);

            // Subtitle
            _subtitleLabel = new LabelControl
            {
                Text = "Please sign in to continue",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 30,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center }, ForeColor = System.Drawing.Color.Gray },
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

            // Username label
            var usernameLabel = new LabelControl
            {
                Text = "Username",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(usernameLabel);

            // Username edit
            _usernameEdit = new TextEdit
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Properties =
                {
                    AutoHeight = false,
                    Padding = new Padding(10, 0, 10, 0),
                    NullValuePrompt = "Enter your username",
                    NullValuePromptShowForEmptyValue = true
                }
            };
            _usernameEdit.KeyDown += OnKeyDown;
            _mainPanel.Controls.Add(_usernameEdit);

            // Spacer
            var spacer2 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 15,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer2);

            // Password label
            var passwordLabel = new LabelControl
            {
                Text = "Password",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(passwordLabel);

            // Password edit
            _passwordEdit = new TextEdit
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Properties =
                {
                    AutoHeight = false,
                    PasswordChar = '●',
                    Padding = new Padding(10, 0, 10, 0),
                    NullValuePrompt = "Enter your password",
                    NullValuePromptShowForEmptyValue = true,
                    UseSystemPasswordChar = true
                }
            };
            _passwordEdit.KeyDown += OnKeyDown;
            _mainPanel.Controls.Add(_passwordEdit);

            // Spacer
            var spacer3 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 10,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer3);

            // Remember me checkbox
            _rememberMeCheck = new CheckEdit
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Remember me",
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_rememberMeCheck);

            // Spacer
            var spacer4 = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 20,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer4);

            // Login button
            _loginButton = new SimpleButton
            {
                Text = "Sign In",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point),
                Appearance =
                {
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White
                }
            };
            _loginButton.Click += OnLoginClick;
            _mainPanel.Controls.Add(_loginButton);

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
                Height = 20,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _mainPanel.Controls.Add(spacer5);

            // Status label (for error messages)
            _statusLabel = new LabelControl
            {
                Text = "",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 24,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center }, ForeColor = System.Drawing.Color.Red },
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_statusLabel);

            // Version label at bottom
            _versionLabel = new LabelControl
            {
                Text = "Version 1.0.0",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Bottom,
                Height = 24,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center }, ForeColor = System.Drawing.Color.Gray },
                Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            _mainPanel.Controls.Add(_versionLabel);

            // Focus username field on load
            this.Load += (s, e) => _usernameEdit.Focus();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OnLoginClick(_loginButton, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private async void OnLoginClick(object? sender, EventArgs e)
        {
            var username = _usernameEdit.Text?.Trim();
            var password = _passwordEdit.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowStatus("Please enter your username", true);
                _usernameEdit.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Please enter your password", true);
                _passwordEdit.Focus();
                return;
            }

            _loginButton.Enabled = false;
            _cancelButton.Enabled = false;
            _usernameEdit.Enabled = false;
            _passwordEdit.Enabled = false;
            _rememberMeCheck.Enabled = false;
            ShowStatus("Signing in...", false);
            SetWaitCursor(true);

            try
            {
                // Authenticate the user. A null result means invalid credentials.
                var user = await _userService.AuthenticateAsync(username, password, CancellationToken);

                if (user == null)
                {
                    ShowStatus("Invalid username or password", true);
                    _passwordEdit.Text = "";
                    _passwordEdit.Focus();
                    return;
                }

                // Resolve the user's roles and populate the session.
                var roles = await _userService.GetUserRolesAsync(user.Id, CancellationToken);
                _userSession.CurrentUser = user;
                _userSession.Roles = roles;

                this.DialogResult = DialogResult.OK;
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                ShowStatus($"Login failed: {ex.Message}", true);
            }
            finally
            {
                SetWaitCursor(false);
                _loginButton.Enabled = true;
                _cancelButton.Enabled = true;
                _usernameEdit.Enabled = true;
                _passwordEdit.Enabled = true;
                _rememberMeCheck.Enabled = true;
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
            _statusLabel.Appearance.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Blue;
        }
    }
}