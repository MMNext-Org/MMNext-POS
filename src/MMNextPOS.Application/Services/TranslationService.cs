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

        public bool TryGetText(string key, out string text)
        {
            text = key;
            if (_translations.TryGetValue(key, out var langMap) &&
                langMap.TryGetValue(CurrentLanguage, out var translated))
            {
                text = translated;
                return true;
            }
            return false;
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

            // Buttons & actions
            AddTranslation("OK", "OK", "ရပါသည်");
            AddTranslation("Delete", "Delete", "ဖျက်မည်");
            AddTranslation("Edit", "Edit", "ပြင်ဆင်မည်");
            AddTranslation("New", "New", "အသစ်ဖန်တီးမည်");
            AddTranslation("Add", "Add", "ထည့်သွင်းမည်");
            AddTranslation("Refresh", "Refresh", "ပြန်လည်စစ်ဆေးမည်");
            AddTranslation("Search", "Search", "ရှာဖွေပါ");
            AddTranslation("Export CSV", "Export CSV", "CSV ထုတ်ယူမည်");
            AddTranslation("Print", "Print", "ပရင့်ထုတ်မည်");
            AddTranslation("Print Receipt", "Print Receipt", "ပြေစာပရင့်ထုတ်မည်");
            AddTranslation("Close", "Close", "ပိတ်မည်");
            AddTranslation("Clear", "Clear", "ဖျက်ရှင်းမည်");
            AddTranslation("Clear Filters", "Clear Filters", "စစ်ထုတ်မှုများ ရှင်းလင်းမည်");
            AddTranslation("Browse", "Browse", "ရွေးချယ်မည်");
            AddTranslation("Select", "Select", "ရွေးချယ်ပါ");
            AddTranslation("Generate", "Generate", "ထုတ်လုပ်မည်");
            AddTranslation("Import", "Import", "တင်သွင်းမည်");
            AddTranslation("Apply", "Apply", "အသုံးပြုမည်");
            AddTranslation("Sign In", "Sign In", "ဝင်ရောက်မည်");
            AddTranslation("Logout", "Logout", "ထွက်မည်");
            AddTranslation("Exit", "Exit", "ပိတ်ထွက်မည်");
            AddTranslation("Change Password", "Change Password", "စကားဝှက်ပြောင်းမည်");
            AddTranslation("Reset Password", "Reset Password", "စကားဝှက်အသစ်သတ်မှတ်မည်");
            AddTranslation("Hold Sale", "Hold Sale", "အရောင်းဆိုင်းထားမည်");
            AddTranslation("Resume Sale", "Resume Sale", "ဆက်ရောင်းမည်");
            AddTranslation("Void Sale", "Void Sale", "အရောင်းဖျက်သိမ်းမည်");
            AddTranslation("New Sale", "New Sale", "အရောင်းအသစ်");
            AddTranslation("Live Sale", "Live Sale", "လက်ရှိအရောင်း");
            AddTranslation("Remove", "Remove", "ဖယ်ရှားမည်");
            AddTranslation("Copy", "Copy", "ကူးယူမည်");
            AddTranslation("History", "History", "မှတ်တမ်း");
            AddTranslation("Activate", "Activate", "ဖွင့်မည်");
            AddTranslation("Restore", "Restore", "ပြန်လည်ရယူမည်");

            // Field labels
            AddTranslation("Name", "Name", "အမည်");
            AddTranslation("Code", "Code", "ကုဒ်");
            AddTranslation("Description", "Description", "ဖော်ပြချက်");
            AddTranslation("Status", "Status", "အခြေအနေ");
            AddTranslation("Phone", "Phone", "ဖုန်းနံပါတ်");
            AddTranslation("Email", "Email", "အီးမေးလ်");
            AddTranslation("Address", "Address", "လိပ်စာ");
            AddTranslation("Username", "Username", "အသုံးပြုသူအမည်");
            AddTranslation("Password", "Password", "စကားဝှက်");
            AddTranslation("Current Password", "Current Password", "လက်ရှိစကားဝှက်");
            AddTranslation("New Password", "New Password", "စကားဝှက်အသစ်");
            AddTranslation("Confirm New Password", "Confirm New Password", "စကားဝှက်အသစ်အတည်ပြုပါ");
            AddTranslation("Show password", "Show password", "စကားဝှက်ပြမည်");
            AddTranslation("Remember me", "Remember me", "မှတ်ထားမည်");
            AddTranslation("Location", "Location", "တည်နေရာ");
            AddTranslation("Company", "Company", "ကုမ္ပဏီ");
            AddTranslation("Company Name", "Company Name", "ကုမ္ပဏီအမည်");
            AddTranslation("Customer Name", "Customer Name", "ဖောက်သည်အမည်");
            AddTranslation("Customer Phone", "Customer Phone", "ဖောက်သည်ဖုန်းနံပါတ်");
            AddTranslation("Product", "Product", "ပစ္စည်း");
            AddTranslation("Supplier", "Supplier", "ပေးသွင်းသူ");
            AddTranslation("Category", "Category", "အမျိုးအစား");
            AddTranslation("Unit", "Unit", "ယူနစ်");
            AddTranslation("From", "From", "မှ");
            AddTranslation("To", "To", "သို့");
            AddTranslation("Date From", "Date From", "စတင်ရက်စွဲ");
            AddTranslation("Date To", "Date To", "ပြီးဆုံးရက်စွဲ");
            AddTranslation("Notes", "Notes", "မှတ်ချက်");
            AddTranslation("Amount", "Amount", "ပမာဏ");
            AddTranslation("Total Amount", "Total Amount", "စုစုပေါင်းပမာဏ");
            AddTranslation("Quantity", "Quantity", "အရေအတွက်");
            AddTranslation("Reason", "Reason", "အကြောင်းပြချက်");
            AddTranslation("Type", "Type", "အမျိုးအစား");
            AddTranslation("Payment", "Payment", "ငွေပေးချေမှု");
            AddTranslation("Payment Type", "Payment Type", "ငွေပေးချေမှုအမျိုးအစား");
            AddTranslation("Paid By", "Paid By", "ပေးချေသူ");
            AddTranslation("Paid Amount", "Paid Amount", "ပေးချေပြီးပမာဏ");
            AddTranslation("Rate", "Rate", "နှုန်းထား");
            AddTranslation("Exchange Rate", "Exchange Rate", "ငွေလဲလှယ်နှုန်း");
            AddTranslation("Culture Code", "Culture Code", "ယဉ်ကျေးမှုကုဒ်");
            AddTranslation("Native Name", "Native Name", "သက်ဆိုင်ရာဘာသာအမည်");
            AddTranslation("Default", "Default", "မူရင်းတန်ဖိုး");
            AddTranslation("Is Active", "Is Active", "အသုံးပြုပါ");
            AddTranslation("Method", "Method", "နည်းလမ်း");
            AddTranslation("Month", "Month", "လ");
            AddTranslation("Year", "Year", "နှစ်");
            AddTranslation("Ready", "Ready", "အသင့်ဖြစ်ပါသည်");
            AddTranslation("Loading", "Loading", "ဖွင့်နေပါသည်");
            AddTranslation("Processing...", "Processing...", "လုပ်ဆောင်နေပါသည်...");
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
