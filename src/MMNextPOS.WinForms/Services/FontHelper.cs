using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;

namespace MMNextPOS.WinForms.Services
{
    /// <summary>
    /// Loads and provides the Pyidaungsu Myanmar font from disk (bundled in the
    /// Fonts/ folder at the app root). Falls back to "Myanmar Text" (Windows default)
    /// if the font files are missing.
    ///
    /// Myanmar Unicode code-points occupy U+1000 – U+109F.
    /// </summary>
    public static class FontHelper
    {
        private static PrivateFontCollection? _privateFonts;
        private static FontFamily? _pyidaungsuFamily;
        private static bool _initialized;

        static FontHelper()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _privateFonts = new PrivateFontCollection();

            // 1. Try bundled fonts in the application root / Fonts folder.
            //    When a ClickOnce or self-contained deployment is used, the Fonts
            //    folder is copied alongside the executable; for development, the
            //    "Fonts" folder sits at the repo root (two levels up from bin).
            var fontPaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "Pyidaungsu-2.5.3_Regular.ttf"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "Pyidaungsu-2.5.3_Bold.ttf"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "Pyidaungsu-2.5.3_Numbers.ttf")
            };

            foreach (var path in fontPaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        _privateFonts.AddFontFile(path);
                    }
                    catch
                    {
                        // Ignore malformed font files – we will fall back later.
                    }
                }
            }

            if (_privateFonts.Families.Length > 0)
            {
                _pyidaungsuFamily = _privateFonts.Families[0];
            }
            else
            {
                // Fallback: any font that contains Myanmar block (Myanmar Text is
                // standard on Windows 8+).
                try { _pyidaungsuFamily = new FontFamily("Myanmar Text"); }
                catch { _pyidaungsuFamily = FontFamily.GenericSansSerif; }
            }
        }

        /// <summary>The best available Myanmar-capable FontFamily (Pyidaungsu → Myanmar Text → generic).</summary>
        public static FontFamily MyanmarFamily => _pyidaungsuFamily ?? FontFamily.GenericSansSerif;

        /// <summary>Create a Pyidaungsu font with the requested size/style.</summary>
        public static Font CreateMyanmarFont(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font(MyanmarFamily, size, style);
        }

        /// <summary>Create the bold Pyidaungsu font (used for headers).</summary>
        public static Font CreateMyanmarFontBold(float size)
        {
            // If Pyidaungsu Bold was loaded second, prefer it; otherwise apply Bold style.
            return new Font(MyanmarFamily, size, FontStyle.Bold);
        }

        /// <summary>True if the bundled Pyidaungsu TTFs were found and loaded.</summary>
        public static bool HasPyidaungsu => _privateFonts != null && _privateFonts.Families.Length > 0;
    }
}
