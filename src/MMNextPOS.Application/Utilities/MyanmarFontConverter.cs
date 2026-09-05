using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MMNextPOS.Application.Utilities
{
    /// <summary>
    /// Myanmar Font Converter for Zawgyi <-> Unicode conversion.
    /// Based on standard Myanmar character mappings.
    /// </summary>
    public static class MyanmarFontConverter
    {
        // Zawgyi to Unicode mapping (key = Zawgyi, value = Unicode)
        private static readonly Dictionary<string, string> ZawgyiToUnicodeMap = new()
        {
            // Consonants
            { "ေ", "ိ" },   // Zawgyi kinzi -> Unicode kinzi
            { "ဲ", "ဲ" },   // Zawgyi e -> Unicode e
            { "ဳ", "ဳ" },   // Zawgyi ii -> Unicode ii
            { "ဴ", "ဴ" },   // Zawgyi uu -> Unicode uu
            { "ဵ", "ံ" },   // Zawgyi an -> Unicode an
            { "ံ", "်" },   // Zawgyi asat -> Unicode asat
            { "့", "့" },   // Zawgyi dot below -> Unicode dot below
            { "း", "း" },   // Zawgyi visarga -> Unicode visarga
            { "္", "္" },   // Zawgyi subjoined -> Unicode subjoined
            { "်ာ", " monasterio" }, // Zawgyi ya-yit -> Unicode ya-yit
            { "်ိ", "ျ" }, // Zawgyi ya-pint -> Unicode ya-pint
            { "့်", "ြ" }, // Zawgyi ya-yit + asat -> Unicode ya-yit + asat
            { "်ား", "ြး" }, // Zawgyi ya-yit + visarga -> Unicode ya-yit + visarga
            { "ျြ", "ြ" }, // Zawgyi ya-pint + ya-yit -> Unicode ya-pint + ya-yit
            
            // Medials
            { "ျ", "ျ" },   // Zawgyi ya-pint
            { "ြ", "ြ" },   // Zawgyi ya-yit
            { "ွ", "ွ" },   // Zawgyi wa-hswe
            { "ှ", "ှ" },   // Zawgyi ha-hto
            
            // Vowels
            { "ာ", "ာ" },   // Zawgyi aa
            { "ိ", "ိ" },   // Zawgyi i
            { "ီ", "ီ" },   // Zawgyi ii
            { "ု", "ု" },   // Zawgyi u
            { "ူ", "ူ" },   // Zawgyi uu
            { "ေ", "ေ" },   // Zawgyi e
            { "ဲ", "ဲ" },   // Zawgyi ei
            { "ော", "ော" }, // Zawgyi o
            { "ော်", "ော်" }, // Zawgyi o + asat
            { "ေါ", "ေါ" }, // Zawgyi au
            
            // Numbers
            { "၀", "၀" },
            { "၁", "၁" },
            { "၂", "၂" },
            { "၃", "၃" },
            { "၄", "၄" },
            { "၅", "၅" },
            { "၆", "၆" },
            { "၇", "၇" },
            { "၈", "၈" },
            { "၉", "၉" },
            
            // Punctuation
            { "၊", "၊" },
            { "။", "။" },
            { "၌", "၌" },
            { "၍", "၍" },
            { "၎", "၎" },
            { "၏", "၏" },
            { "ါ", "ါ" },
            
            // Special combinations (Zawgyi uses different encoding for some combinations)
            { "ိန်း", "ိန်း" },
            { "ီန�", "ီနး" },
            { "ုန�", "ုနး" },
            { "ူနး", "ူနး" },
            { "ေနး", "ေနး" },
            { "ဲနး", "ဲနး" },
            { "اونး", "اونး" },
            { "ေါနး", "ေါနး" },
            
            // Common words that differ
            { "မြန်မာ", "မြန်မာ" },
            { "ဗမာ", "ဗမာ" },
            { "ရန်ကုန်", "ရန်ကုန်" },
            { "မန္တလေး", "မန္တလေး" },
        };

        // Unicode to Zawgyi mapping (reverse)
        private static readonly Dictionary<string, string> UnicodeToZawgyiMap;

        static MyanmarFontConverter()
        {
            UnicodeToZawgyiMap = new Dictionary<string, string>();
            foreach (var kvp in ZawgyiToUnicodeMap)
            {
                if (!UnicodeToZawgyiMap.ContainsKey(kvp.Value))
                {
                    UnicodeToZawgyiMap[kvp.Value] = kvp.Key;
                }
            }
            
            // Add additional reverse mappings for common Unicode sequences
            UnicodeToZawgyiMap[" monasterya"] = "ျ"; // ya-pint
            UnicodeToZawgyiMap["ြ"] = "ျ"; // ya-yit (approximation)
        }

        /// <summary>
        /// Converts Zawgyi-encoded text to Unicode.
        /// </summary>
        public static string ZawgyiToUnicode(string zawgyiText)
        {
            if (string.IsNullOrEmpty(zawgyiText))
                return zawgyiText;

            var result = new StringBuilder(zawgyiText.Length);
            int i = 0;

            while (i < zawgyiText.Length)
            {
                // Try to match longest sequences first
                bool matched = false;
                
                // Check for 4-char sequences
                if (i + 3 < zawgyiText.Length)
                {
                    var seq4 = zawgyiText.Substring(i, 4);
                    if (ZawgyiToUnicodeMap.TryGetValue(seq4, out var unicode4))
                    {
                        result.Append(unicode4);
                        i += 4;
                        matched = true;
                    }
                }

                if (!matched && i + 2 < zawgyiText.Length)
                {
                    var seq3 = zawgyiText.Substring(i, 3);
                    if (ZawgyiToUnicodeMap.TryGetValue(seq3, out var unicode3))
                    {
                        result.Append(unicode3);
                        i += 3;
                        matched = true;
                    }
                }

                if (!matched && i + 1 < zawgyiText.Length)
                {
                    var seq2 = zawgyiText.Substring(i, 2);
                    if (ZawgyiToUnicodeMap.TryGetValue(seq2, out var unicode2))
                    {
                        result.Append(unicode2);
                        i += 2;
                        matched = true;
                    }
                }

                if (!matched)
                {
                    var ch = zawgyiText[i].ToString();
                    if (ZawgyiToUnicodeMap.TryGetValue(ch, out var unicode1))
                    {
                        result.Append(unicode1);
                    }
                    else
                    {
                        result.Append(ch);
                    }
                    i++;
                }
            }

            // Post-process: fix common Zawgyi-specific encoding issues
            return PostProcessZawgyiToUnicode(result.ToString());
        }

        /// <summary>
        /// Converts Unicode text to Zawgyi encoding.
        /// </summary>
        public static string UnicodeToZawgyi(string unicodeText)
        {
            if (string.IsNullOrEmpty(unicodeText))
                return unicodeText;

            var result = new StringBuilder(unicodeText.Length);
            int i = 0;

            while (i < unicodeText.Length)
            {
                bool matched = false;

                // Check for 4-char sequences
                if (i + 3 < unicodeText.Length)
                {
                    var seq4 = unicodeText.Substring(i, 4);
                    if (UnicodeToZawgyiMap.TryGetValue(seq4, out var zawgyi4))
                    {
                        result.Append(zawgyi4);
                        i += 4;
                        matched = true;
                    }
                }

                if (!matched && i + 2 < unicodeText.Length)
                {
                    var seq3 = unicodeText.Substring(i, 3);
                    if (UnicodeToZawgyiMap.TryGetValue(seq3, out var zawgyi3))
                    {
                        result.Append(zawgyi3);
                        i += 3;
                        matched = true;
                    }
                }

                if (!matched && i + 1 < unicodeText.Length)
                {
                    var seq2 = unicodeText.Substring(i, 2);
                    if (UnicodeToZawgyiMap.TryGetValue(seq2, out var zawgyi2))
                    {
                        result.Append(zawgyi2);
                        i += 2;
                        matched = true;
                    }
                }

                if (!matched)
                {
                    var ch = unicodeText[i].ToString();
                    if (UnicodeToZawgyiMap.TryGetValue(ch, out var zawgyi1))
                    {
                        result.Append(zawgyi1);
                    }
                    else
                    {
                        result.Append(ch);
                    }
                    i++;
                }
            }

            return PostProcessUnicodeToZawgyi(result.ToString());
        }

        /// <summary>
        /// Auto-detects the encoding and converts to Unicode.
        /// </summary>
        public static string ToUnicode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Simple heuristic: if text contains typical Zawgyi patterns, convert
            if (IsLikelyZawgyi(text))
            {
                return ZawgyiToUnicode(text);
            }
            return text; // Assume already Unicode
        }

        /// <summary>
        /// Auto-detects the encoding and converts to Zawgyi.
        /// </summary>
        public static string ToZawgyi(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Simple heuristic: if text contains typical Unicode patterns, convert
            if (IsLikelyUnicode(text))
            {
                return UnicodeToZawgyi(text);
            }
            return text; // Assume already Zawgyi
        }

        /// <summary>
        /// Heuristic to detect Zawgyi encoding.
        /// </summary>
        private static bool IsLikelyZawgyi(string text)
        {
            // Check for common Zawgyi-specific character sequences
            var zawgyiPatterns = new[]
            {
                "ေ", "ဲ", "ဳ", "ဴ", "ဵ", "ံ", "့", "း", "ျ", "ြ", "ွ", "ှ"
            };

            int zawgyiCount = 0;
            foreach (var pattern in zawgyiPatterns)
            {
                if (text.Contains(pattern))
                    zawgyiCount++;
            }

            // If we find multiple Zawgyi patterns, it's likely Zawgyi
            return zawgyiCount >= 2;
        }

        /// <summary>
        /// Heuristic to detect Unicode encoding.
        /// </summary>
        private static bool IsLikelyUnicode(string text)
        {
            // Check for common Unicode Myanmar sequences
            var unicodePatterns = new[]
            {
                "ိ", "ီ", "ု", "ူ", "ေ", "ဲ", "ော", "ေါ", "ျ", "ြ", "ွ", "ှ", "်ာ", "်ိ", "့်"
            };

            int unicodeCount = 0;
            foreach (var pattern in unicodePatterns)
            {
                if (text.Contains(pattern))
                    unicodeCount++;
            }

            return unicodeCount >= 2;
        }

        private static string PostProcessZawgyiToUnicode(string text)
        {
            // Fix common Zawgyi encoding issues after conversion
            var result = text;

            // Fix kinzi (Zawgyi uses different encoding)
            result = Regex.Replace(result, "([က-ဟ])ေဝ([ါ-ူ])", "$1ွ$2"); // kinzi
            result = Regex.Replace(result, "([က-ဟ])ေဝ", "$1ွ"); // kinzi without following vowel

            // Fix Ya-yit + Asat combinations
            result = Regex.Replace(result, "([က-ဟ])ျ([ါ-ူ])", "$1ျ$2");
            result = Regex.Replace(result, "([က-ဟ])ြ([ါ-ူ])", "$1ြ$2");

            return result;
        }

        private static string PostProcessUnicodeToZawgyi(string text)
        {
            // Fix common Unicode to Zawgyi issues
            var result = text;

            // Convert kinzi
            result = Regex.Replace(result, "([က-ဟ])ွ([ါ-ူ])", "$1ေ�$2");
            result = Regex.Replace(result, "([က-ဟ])ွ", "$1ေ�");

            return result;
        }

        /// <summary>
        /// Gets the current system Myanmar font encoding preference.
        /// </summary>
        public static MyanmarEncoding DetectSystemEncoding()
        {
            // Check registry or system settings for Myanmar font preference
            // For now, default to Unicode as it's the modern standard
            return MyanmarEncoding.Unicode;
        }
    }

    /// <summary>
    /// Myanmar text encoding types.
    /// </summary>
    public enum MyanmarEncoding
    {
        Unknown,
        Unicode,
        Zawgyi
    }

    /// <summary>
    /// Extension methods for easy conversion.
    /// </summary>
    public static class MyanmarFontConverterExtensions
    {
        public static string ToUnicode(this string text) => MyanmarFontConverter.ToUnicode(text);
        public static string ToZawgyi(this string text) => MyanmarFontConverter.ToZawgyi(text);
        public static string ZawgyiToUnicode(this string text) => MyanmarFontConverter.ZawgyiToUnicode(text);
        public static string UnicodeToZawgyi(this string text) => MyanmarFontConverter.UnicodeToZawgyi(text);
    }
}