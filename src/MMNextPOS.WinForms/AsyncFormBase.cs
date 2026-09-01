using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Base class for WinForms that need async operations, cancellation handling,
    /// and consistent error/info dialogs.
    /// </summary>
    public abstract class AsyncFormBase : XtraForm, IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        protected CancellationToken CancellationToken => _cts.Token;

        protected AsyncFormBase()
        {
            // Ensure the form uses the standard wait cursor handling.
            this.UseWaitCursor = false;
        }

        protected async Task RunAsync(Func<CancellationToken, Task> asyncAction)
        {
            try
            {
                SetWaitCursor(true);
                await asyncAction(_cts.Token).ConfigureAwait(true);
            }
            catch (OperationCanceledException) { /* ignore */ }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        protected void SetWaitCursor(bool wait)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetWaitCursor(wait)));
                return;
            }
            this.UseWaitCursor = wait;
            if (wait) System.Windows.Forms.Application.DoEvents();
        }

        protected void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowError(message)));
                return;
            }
            XtraMessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void ShowInfo(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowInfo(message)));
                return;
            }
            XtraMessageBox.Show(this, message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Dispose pattern – cancels any pending async work and disposes resources.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
