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

        /// <summary>Event triggered when the language is changed.</summary>
        event Action? LanguageChanged;
    }
}
