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
            InitializeBaseComponents();
        }

        protected virtual void InitializeBaseComponents()
        {
            _okButton = new SimpleButton
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Width = 100,
                Height = 35
            };
            _okButton.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };

            _cancelButton = new SimpleButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 35
            };

            _buttonPanel = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _buttonPanel.Controls.Add(_okButton);
            _buttonPanel.Controls.Add(_cancelButton);

            Controls.Add(_buttonPanel);

            // Layout buttons
            Layout += (s, e) =>
            {
                _okButton.Location = new Point(ClientSize.Width - 220, 12);
                _cancelButton.Location = new Point(ClientSize.Width - 110, 12);
            };
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