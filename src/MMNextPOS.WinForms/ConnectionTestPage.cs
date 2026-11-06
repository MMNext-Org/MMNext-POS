using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Application.Services;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Admin page to verify the MySQL connection and inspect server metadata.
    /// Uses IConnectionTestService; results are audit-logged by the service.
    /// </summary>
    public class ConnectionTestPage : XtraUserControl
    {
        private readonly IConnectionTestService _connectionTestService;

        private SimpleButton _testButton = null!;
        private SimpleButton _serverInfoButton = null!;
        private LabelControl _resultLabel = null!;
        private MemoEdit _detailsMemo = null!;

        public ConnectionTestPage(IConnectionTestService connectionTestService)
        {
            _connectionTestService = connectionTestService ?? throw new ArgumentNullException(nameof(connectionTestService));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var titleLabel = new LabelControl
            {
                Text = "Database Connection Test",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold) },
                AutoSizeMode = LabelAutoSizeMode.None
            };
            mainLayout.Controls.Add(titleLabel, 0, 0);

            var buttonPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            _testButton = new SimpleButton
            {
                Text = "Test Connection",
                Location = new Point(0, 12),
                Width = 140,
                Height = 32
            };
            _testButton.Click += async (s, e) => await TestConnectionAsync();

            _serverInfoButton = new SimpleButton
            {
                Text = "Get Server Info",
                Location = new Point(150, 12),
                Width = 140,
                Height = 32,
                Enabled = false
            };
            _serverInfoButton.Click += async (s, e) => await LoadServerInfoAsync();

            buttonPanel.Controls.Add(_testButton);
            buttonPanel.Controls.Add(_serverInfoButton);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            var resultLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _resultLabel = new LabelControl
            {
                Text = "Press \"Test Connection\" to verify the database connection.",
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None
            };

            _detailsMemo = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { ReadOnly = true }
            };

            resultLayout.Controls.Add(_resultLabel, 0, 0);
            resultLayout.Controls.Add(_detailsMemo, 0, 1);
            mainLayout.Controls.Add(resultLayout, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private async System.Threading.Tasks.Task TestConnectionAsync()
        {
            try
            {
                this.UseWaitCursor = true;
                _testButton.Enabled = false;
                _resultLabel.Text = "Testing connection...";

                var result = await _connectionTestService.TestConnectionAsync();

                if (result.Success)
                {
                    _resultLabel.Text = $"SUCCESS - Connected in {result.LatencyMs}ms. Server version: {result.ServerVersion}";
                    _resultLabel.Appearance.ForeColor = Color.DarkGreen;
                    _serverInfoButton.Enabled = true;
                    _detailsMemo.Text = $"Latency: {result.LatencyMs} ms\r\nServer version: {result.ServerVersion}";
                }
                else
                {
                    _resultLabel.Text = $"FAILED - {result.ErrorMessage}";
                    _resultLabel.Appearance.ForeColor = Color.DarkRed;
                    _serverInfoButton.Enabled = false;
                    _detailsMemo.Text = $"Error: {result.ErrorMessage}\r\nLatency: {result.LatencyMs} ms";
                }
            }
            catch (Exception ex)
            {
                _resultLabel.Text = $"FAILED - {ex.Message}";
                _resultLabel.Appearance.ForeColor = Color.DarkRed;
                _detailsMemo.Text = ex.ToString();
            }
            finally
            {
                this.UseWaitCursor = false;
                _testButton.Enabled = true;
            }
        }

        private async System.Threading.Tasks.Task LoadServerInfoAsync()
        {
            try
            {
                this.UseWaitCursor = true;

                var info = await _connectionTestService.GetServerInfoAsync();
                var sizeBytes = await _connectionTestService.GetDatabaseSizeBytesAsync();

                _detailsMemo.Text =
                    $"Server version : {info.Version}\r\n" +
                    $"Database       : {info.Database}\r\n" +
                    $"Table count    : {info.TableCount}\r\n" +
                    $"Database size  : {sizeBytes / (1024.0 * 1024.0):F2} MB\r\n" +
                    $"Server time    : {info.ServerTime:yyyy-MM-dd HH:mm:ss} UTC";
            }
            catch (Exception ex)
            {
                _resultLabel.Text = $"Failed to read server info: {ex.Message}";
                _resultLabel.Appearance.ForeColor = Color.DarkRed;
            }
            finally
            {
                this.UseWaitCursor = false;
            }
        }
    }
}
