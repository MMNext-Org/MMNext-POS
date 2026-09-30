using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using MMNextPOS.Application.Services;

namespace MMNextPOS.WinForms.Services
{
    /// <summary>
    /// Applies <see cref="ITranslationService"/> strings to every translatable control
    /// in a form's control tree and swaps in the Pyidaungsu Myanmar font when the
    /// active language is Myanmar. Original English text and fonts are remembered
    /// per-control so a form can be restored when switching back to English.
    ///
    /// Also localizes DevExpress grid column captions and editor NullText prompts.
    ///
    /// Call <see cref="Initialize"/> once at startup (Program.cs) with the DI-resolved
    /// singleton, then <see cref="Localize"/> for each form (AsyncFormBase does this
    /// automatically and re-localizes on <see cref="LanguageChanged"/>).
    /// </summary>
    public static class FormLocalizer
    {
        private sealed class ControlState
        {
            public string Text = string.Empty;
            public string? NullText;
            public Font? Font;
        }

        // ConditionalWeakTable – remembers originals without leaking disposed controls.
        private static readonly ConditionalWeakTable<Control, ControlState> _state = new();

        // Grid columns are Components, not Controls, so they need their own store.
        private static readonly ConditionalWeakTable<GridColumn, ControlState> _columnState = new();

        // Small font cache so repeated localization runs don't allocate a new Font per control.
        private static readonly Dictionary<(float Size, FontStyle Style), Font> _myanmarFonts = new();

        private static ITranslationService? _service;

        public static ITranslationService? Service => _service;

        /// <summary>Relayed from the translation service; fired on the calling thread.</summary>
        public static event Action? LanguageChanged;

        /// <summary>Registers the singleton translation service (idempotent).</summary>
        public static void Initialize(ITranslationService service)
        {
            if (ReferenceEquals(_service, service)) return;
            if (_service != null) _service.LanguageChanged -= OnServiceLanguageChanged;
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _service.LanguageChanged += OnServiceLanguageChanged;
        }

        private static void OnServiceLanguageChanged() => LanguageChanged?.Invoke();

        /// <summary>
        /// Translates the form caption and every control in its tree for the current
        /// language. Safe to call repeatedly; originals are preserved internally.
        /// </summary>
        public static void Localize(Form form)
        {
            if (_service == null || form == null) return;
            var myanmar = _service.CurrentLanguage == LanguageType.Myanmar;

            LocalizeControl(form, myanmar, isForm: true);
            WalkControls(form, myanmar);
        }

        private static void WalkControls(Control parent, bool myanmar)
        {
            foreach (Control child in parent.Controls)
            {
                LocalizeControl(child, myanmar, isForm: false);
                if (child is GridControl grid)
                {
                    LocalizeGridColumns(grid, myanmar);
                }
                if (child.HasChildren)
                {
                    WalkControls(child, myanmar);
                }
            }
        }

        private static void LocalizeGridColumns(GridControl grid, bool myanmar)
        {
            foreach (ColumnView view in grid.Views)
            {
                foreach (GridColumn column in view.Columns)
                {
                    if (myanmar)
                    {
                        var state = _columnState.GetValue(column, c => new ControlState { Text = c.Caption });
                        if (TryTranslate(state.Text, out var translated))
                        {
                            column.Caption = translated;
                        }
                    }
                    else if (_columnState.TryGetValue(column, out var state))
                    {
                        column.Caption = state.Text;
                    }
                }
            }
        }

        private static void LocalizeControl(Control control, bool myanmar, bool isForm)
        {
            if (myanmar)
            {
                var state = _state.GetValue(control, c =>
                {
                    var s = new ControlState { Text = c.Text, Font = c.Font };
                    if (c is BaseEdit edit && edit.Properties != null)
                    {
                        s.NullText = edit.Properties.NullText;
                    }
                    return s;
                });

                if (TryTranslate(state.Text, out var translated))
                {
                    control.Text = translated;
                }

                if (control is BaseEdit baseEdit && baseEdit.Properties != null &&
                    !string.IsNullOrEmpty(state.NullText) &&
                    TryTranslate(state.NullText!, out var translatedPrompt))
                {
                    baseEdit.Properties.NullText = translatedPrompt;
                }

                if (state.Font != null && !isForm)
                {
                    control.Font = GetMyanmarFont(state.Font.Size, state.Font.Style);
                }
            }
            else if (_state.TryGetValue(control, out var state))
            {
                control.Text = state.Text;
                if (control is BaseEdit baseEdit && baseEdit.Properties != null && state.NullText != null)
                {
                    baseEdit.Properties.NullText = state.NullText;
                }
                if (state.Font != null)
                {
                    control.Font = state.Font;
                }
            }
        }

        private static Font GetMyanmarFont(float size, FontStyle style)
        {
            var key = (size, style);
            if (!_myanmarFonts.TryGetValue(key, out var font))
            {
                font = FontHelper.CreateMyanmarFont(size, style);
                _myanmarFonts[key] = font;
            }
            return font;
        }

        /// <summary>
        /// Looks up a label allowing for common suffixes such as ":", "*", "..." and "…".
        /// Any stripped suffix is re-appended to the translated text.
        /// </summary>
        private static bool TryTranslate(string original, out string result)
        {
            result = original;
            if (_service == null || string.IsNullOrWhiteSpace(original)) return false;

            var text = original.Trim();
            if (text.Length == 0) return false;

            // Exact match first (e.g. "Save", "OK").
            if (_service.TryGetText(text, out result)) return true;

            // Then try the string with trailing decorators removed ("Customer:" -> "Customer").
            var len = text.Length;
            while (len > 0 && (text[len - 1] == ':' || text[len - 1] == '*' || text[len - 1] == '.' ||
                               text[len - 1] == '…' || char.IsWhiteSpace(text[len - 1])))
            {
                len--;
            }

            if (len == text.Length) return false; // nothing stripped -> no other candidates

            var core = text.Substring(0, len).TrimEnd();
            if (core.Length == 0 || !_service.TryGetText(core, out var translatedCore)) return false;

            result = translatedCore + text.Substring(core.Length);
            return true;
        }
    }
}
