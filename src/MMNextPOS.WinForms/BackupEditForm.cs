using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Editor for a backup setting. Also exposes run backup, history and
    /// restore actions for settings that are already saved.
    /// </summary>
    public class BackupEditForm : EditFormBase
    {
        private readonly BackupSetting _setting;
        private readonly IBackupService _service;
        private readonly bool _isNew;

        private TextEdit _nameEdit = null!;
        private TextEdit _descriptionEdit = null!;
        private ComboBoxEdit _frequencyCombo = null!;
        private TextEdit _executionTimeEdit = null!;
        private SpinEdit _retentionDaysEdit = null!;
        private TextEdit _backupPathEdit = null!;
        private ComboBoxEdit _storageTypeCombo = null!;
        private CheckEdit _includeDatabaseCheck = null!;
        private CheckEdit _includeFilesCheck = null!;
        private CheckEdit _includeLogsCheck = null!;
        private CheckEdit _includeImagesCheck = null!;
        private CheckEdit _compressBackupCheck = null!;
        private CheckEdit _encryptBackupCheck = null!;
        private TextEdit _encryptionPasswordEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        private SimpleButton _runButton = null!;
        private SimpleButton _historyButton = null!;
        private SimpleButton _restoreButton = null!;

        public BackupEditForm(BackupSetting setting, IBackupService service)
        {
            _setting = setting ?? new BackupSetting();
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _isNew = _setting.Id == 0;

            InitializeComponent();
            LoadEntityData(_setting);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Backup Setting" : "Edit Backup Setting";
            Size = new Size(640, 760);
            MinimumSize = new Size(600, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScroll = true;

            // Action toolbar (run/history/restore only make sense once saved)
            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 52,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _runButton = new SimpleButton { Text = "Run Backup", Width = 110, Height = 32, Location = new Point(10, 10), Enabled = !_isNew };
            _runButton.Click += async (s, e) => await RunBackupAsync();
            _historyButton = new SimpleButton { Text = "History", Width = 100, Height = 32, Location = new Point(130, 10), Enabled = !_isNew };
            _historyButton.Click += async (s, e) => await ShowHistoryAsync();
            _restoreButton = new SimpleButton { Text = "Restore", Width = 100, Height = 32, Location = new Point(240, 10), Enabled = !_isNew };
            _restoreButton.Click += async (s, e) => await RestoreAsync();
            toolbar.Controls.Add(_runButton);
            toolbar.Controls.Add(_historyButton);
            toolbar.Controls.Add(_restoreButton);
            Controls.Add(toolbar);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 16,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 15; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            int row = 0;

            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, row);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Description:"), 0, row);
            _descriptionEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_descriptionEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Frequency:"), 0, row);
            _frequencyCombo = CreateCombo(new[] { "Daily", "Weekly", "Monthly", "Manual" });
            mainLayout.Controls.Add(_frequencyCombo, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Execution Time (HH:mm):"), 0, row);
            _executionTimeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 5 } };
            mainLayout.Controls.Add(_executionTimeEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Retention Days:"), 0, row);
            _retentionDaysEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 3650, IsFloatValue = false } };
            mainLayout.Controls.Add(_retentionDaysEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Backup Path:"), 0, row);
            _backupPathEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_backupPathEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Storage Type:"), 0, row);
            _storageTypeCombo = CreateCombo(new[] { "Local", "FTP", "SFTP", "S3", "AzureBlob", "GoogleDrive" });
            mainLayout.Controls.Add(_storageTypeCombo, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Include Database:"), 0, row);
            _includeDatabaseCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_includeDatabaseCheck, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Include Files:"), 0, row);
            _includeFilesCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_includeFilesCheck, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Include Logs:"), 0, row);
            _includeLogsCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_includeLogsCheck, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Include Images:"), 0, row);
            _includeImagesCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_includeImagesCheck, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Compress Backup:"), 0, row);
            _compressBackupCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_compressBackupCheck, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Encrypt Backup:"), 0, row);
            _encryptBackupCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_encryptBackupCheck, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Encryption Password:"), 0, row);
            _encryptionPasswordEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100, PasswordChar = '*' } };
            mainLayout.Controls.Add(_encryptionPasswordEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Active:"), 0, row);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_isActiveCheck, 1, row++);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = !string.IsNullOrWhiteSpace(_nameEdit.Text);

            var timeParts = _executionTimeEdit.Text.Trim().Split(':');
            if (timeParts.Length == 2 &&
                int.TryParse(timeParts[0], out var hour) &&
                int.TryParse(timeParts[1], out var minute) &&
                hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59)
            {
                _executionTimeEdit.Properties.Appearance.BorderColor = SystemColors.WindowFrame;
            }
            else
            {
                isValid = false;
                _executionTimeEdit.Properties.Appearance.BorderColor = Color.Red;
            }

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var setting = (BackupSetting)entity;
            _nameEdit.Text = setting.Name;
            _descriptionEdit.Text = setting.Description;
            SelectByText(_frequencyCombo, setting.Frequency);
            _executionTimeEdit.Text = $"{setting.ExecutionTime.Hours:00}:{setting.ExecutionTime.Minutes:00}";
            _retentionDaysEdit.Value = setting.RetentionDays;
            _backupPathEdit.Text = setting.BackupPath;
            SelectByText(_storageTypeCombo, setting.StorageType);
            _includeDatabaseCheck.Checked = setting.IncludeDatabase;
            _includeFilesCheck.Checked = setting.IncludeFiles;
            _includeLogsCheck.Checked = setting.IncludeLogs;
            _includeImagesCheck.Checked = setting.IncludeImages;
            _compressBackupCheck.Checked = setting.CompressBackup;
            _encryptBackupCheck.Checked = setting.EncryptBackup;
            _encryptionPasswordEdit.Text = setting.EncryptionPassword;
            _isActiveCheck.Checked = setting.IsActive;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var setting = (BackupSetting)entity;
            setting.Name = _nameEdit.Text.Trim();
            setting.Description = _descriptionEdit.Text.Trim();
            setting.Frequency = string.IsNullOrWhiteSpace(_frequencyCombo.Text) ? "Daily" : _frequencyCombo.Text;

            if (TimeSpan.TryParse(_executionTimeEdit.Text.Trim(), out var executionTime))
                setting.ExecutionTime = executionTime;

            setting.RetentionDays = (int)_retentionDaysEdit.Value;
            setting.BackupPath = _backupPathEdit.Text.Trim();
            setting.StorageType = string.IsNullOrWhiteSpace(_storageTypeCombo.Text) ? "Local" : _storageTypeCombo.Text;
            setting.IncludeDatabase = _includeDatabaseCheck.Checked;
            setting.IncludeFiles = _includeFilesCheck.Checked;
            setting.IncludeLogs = _includeLogsCheck.Checked;
            setting.IncludeImages = _includeImagesCheck.Checked;
            setting.CompressBackup = _compressBackupCheck.Checked;
            setting.EncryptBackup = _encryptBackupCheck.Checked;
            setting.EncryptionPassword = _encryptionPasswordEdit.Text.Trim();
            setting.IsActive = _isActiveCheck.Checked;
        }

        private async Task RunBackupAsync()
        {
            if (_isNew) return;
            SetWaitCursor(true);
            try
            {
                var ok = await _service.RunBackupAsync(_setting.Id, CancellationToken);
                ShowInfo(ok
                    ? "Backup completed successfully."
                    : "Backup failed. Check the setting's last status for details.");
            }
            catch (Exception ex)
            {
                ShowError($"Backup failed: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task ShowHistoryAsync()
        {
            if (_isNew) return;
            try
            {
                var history = await _service.GetBackupHistoryAsync(_setting.Id, 50, CancellationToken);
                if (history == null || history.Count == 0)
                {
                    ShowInfo("No backup history yet.");
                    return;
                }

                var sb = new StringBuilder();
                foreach (var result in history.OrderByDescending(h => h.StartedAt).Take(20))
                {
                    sb.AppendLine($"{result.StartedAt:yyyy-MM-dd HH:mm} | {result.Status} | " +
                                  $"{(result.BackupSizeBytes > 0 ? $"{result.BackupSizeBytes / 1024.0 / 1024.0:F1} MB | " : string.Empty)}{result.FilePath}");
                    if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                        sb.AppendLine($"   Error: {result.ErrorMessage}");
                }

                ShowInfo("Recent backups:\n\n" + sb);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load history: {ex.Message}");
            }
        }

        private async Task RestoreAsync()
        {
            if (_isNew) return;
            if (!ShowConfirm("Restore from the latest backup for this setting? This will overwrite current data.", "Confirm Restore"))
                return;

            SetWaitCursor(true);
            try
            {
                var ok = await _service.RestoreAsync(_setting.Id, restorePoint: null, CancellationToken);
                ShowInfo(ok ? "Restore completed successfully." : "Restore failed. See the log for details.");
            }
            catch (Exception ex)
            {
                ShowError($"Restore failed: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private static ComboBoxEdit CreateCombo(string[] items)
        {
            var combo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            combo.Properties.Items.AddRange(items);
            return combo;
        }

        private static void SelectByText(ComboBoxEdit combo, string value)
        {
            for (int i = 0; i < combo.Properties.Items.Count; i++)
            {
                if (string.Equals(combo.Properties.Items[i]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            combo.SelectedIndex = combo.Properties.Items.Count > 0 ? 0 : -1;
        }

        private static LabelControl CreateLabel(string text)
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
