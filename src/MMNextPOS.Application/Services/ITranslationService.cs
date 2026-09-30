using System;

namespace MMNextPOS.Application.Services
{
    public enum LanguageType
    {
        English,
        Myanmar
    }

    public interface ITranslationService
    {
        /// <summary>Gets the current selected language.</summary>
        LanguageType CurrentLanguage { get; }

        /// <summary>Sets the current language and notifies subscribers.</summary>
        void SetLanguage(LanguageType language);

        /// <summary>Retrieves the translated text for a given key.</summary>
        string GetText(string key);

        /// <summary>Attempts to retrieve the translated text; returns false when no translation exists for the key.</summary>
        bool TryGetText(string key, out string text);

        /// <summary>Event triggered when the language is changed.</summary>
        event Action? LanguageChanged;
    }
}
