using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Base class for WinForms that need async operations, cancellation handling,
    /// and consistent error/info/confirm dialogs.
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

        /// <summary>
        /// Show a progress message with a wait cursor.
        /// </summary>
        protected void ShowProgress(string message = "Processing...")
        {
            ShowInfo(message);
            SetWaitCursor(true);
        }

        /// <summary>
        /// Hide the progress indicator and restore cursor.
        /// </summary>
        protected void HideProgress()
        {
            SetWaitCursor(false);
        }

        /// <summary>
        /// Show a confirmation dialog and return the user's choice.
        /// </summary>
        protected bool ShowConfirm(string message, string caption = "Confirm")
        {
            if (this.InvokeRequired)
            {
                return (bool)Invoke(new Func<string, string, bool>(ShowConfirm), message, caption);
            }
            var result = XtraMessageBox.Show(this, message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
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

        /// <summary>
        /// Cancel any pending async operation.
        /// </summary>
        protected void CancelAsync() => _cts.Cancel();

        // Virtual methods for derived forms to override
        protected virtual bool ValidateForm() => true;

        public virtual void LoadEntityData(object entity) { }

        public virtual void SaveEntityData(object entity) { }

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
