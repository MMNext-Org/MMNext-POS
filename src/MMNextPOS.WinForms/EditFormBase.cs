using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Base class for edit forms with standard OK/Cancel buttons and validation.
    /// Inherits from AsyncFormBase to get async helpers, ShowInfo, ShowError, ShowConfirm, SetWaitCursor, etc.
    /// </summary>
    public abstract class EditFormBase : AsyncFormBase
    {
        protected SimpleButton _okButton = null!;
        protected SimpleButton _cancelButton = null!;
        protected PanelControl _buttonPanel = null!;

        protected EditFormBase()
        {
            // Do not call virtual members from the constructor.
            // Derived classes must call InitializeBaseComponents() explicitly
            // after base construction completes.
        }

        /// <summary>
        /// Initializes the standard OK/Cancel button panel.
        /// Derived classes should call this from their own constructors.
        /// </summary>
        protected void InitializeBaseComponents()
        {
            _okButton = new SimpleButton
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Width = 100,
                Height = 35,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new SimpleButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 35,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            _cancelButton.Click += CancelButton_Click;

            _buttonPanel = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _buttonPanel.Controls.Add(_okButton);
            _buttonPanel.Controls.Add(_cancelButton);

            Controls.Add(_buttonPanel);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (ValidateForm())
                DialogResult = DialogResult.OK;
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Validates the form. Override in derived classes.
        /// </summary>
        protected override bool ValidateForm()
        {
            return true;
        }

        /// <summary>
        /// Loads entity data into form controls. Override in derived classes.
        /// </summary>
        public override void LoadEntityData(object entity)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Saves form data to entity. Override in derived classes.
        /// </summary>
        public override void SaveEntityData(object entity)
        {
            // Default implementation does nothing
        }
    }
}
