using System.Collections.Generic;

namespace MMNextPOS.Application.Services
{
    public class TranslationService : ITranslationService
    {
        public LanguageType CurrentLanguage { get; private set; } = LanguageType.English;
        public event Action? LanguageChanged;

        private readonly Dictionary<string, Dictionary<LanguageType, string>> _translations;

        public TranslationService()
        {
            _translations = new Dictionary<string, Dictionary<LanguageType, string>>();
            InitializeTranslations();
        }

        public void SetLanguage(LanguageType language)
        {
            if (CurrentLanguage != language)
            {
                CurrentLanguage = language;
                LanguageChanged?.Invoke();
            }
        }

        public string GetText(string key)
        {
            if (_translations.TryGetValue(key, out var langMap))
            {
                if (langMap.TryGetValue(CurrentLanguage, out var text))
                {
                    return text;
                }
            }
            return key; // Return key as fallback
        }

        private void InitializeTranslations()
        {
            AddTranslation("ReceiptNo", "Receipt No", "ပြေစာနံပါတ်");
            AddTranslation("Date", "Date", "နေ့စွဲ");
            AddTranslation("Customer", "Customer", "ဖောက်သည်");
            AddTranslation("Advance", "Advance Payment", "ကြိုတင်ပေးငွေ");
            AddTranslation("Item", "Item", "ပစ္စည်း");
            AddTranslation("Qty", "Qty", "အရေအတွက်");
            AddTranslation("Price", "Price", "ယူနစ်ဈေး");
            AddTranslation("Total", "Total", "စုစုပေါင်း");
            AddTranslation("Discount", "Discount", "လျှော့စျေး");
            AddTranslation("Tax", "Tax", "အခွန်");
            AddTranslation("Paid", "Paid", "ပေးချေငွေ");
            AddTranslation("Change", "Change", "ပြန်အမ်းငွေ");
            AddTranslation("ThankYou", "Thank you!", "ကျေးဇူးတင်ပါသည်!");
            AddTranslation("VisitAgain", "Please visit us again", "နောက်ထပ် လာရောက်ဝယ်ယူပါရန်");
            
            // Common UI strings
            AddTranslation("Save", "Save", "သိမ်းမည်");
            AddTranslation("Cancel", "Cancel", "ပယ်ဖျက်မည်");
            AddTranslation("ProductName", "Product Name", "ပစ္စည်းအမည်");
            AddTranslation("PriceLabel", "Price", "ဈေးနှုန်း");
            AddTranslation("QuantityLabel", "Quantity", "အရေအတွက်");
            AddTranslation("Confirm", "Confirm", "အတည်ပြုသည်");
        }

        private void AddTranslation(string key, string en, string my)
        {
            _translations[key] = new Dictionary<LanguageType, string>
            {
                { LanguageType.English, en },
                { LanguageType.Myanmar, my }
            };
        }
    }
}
