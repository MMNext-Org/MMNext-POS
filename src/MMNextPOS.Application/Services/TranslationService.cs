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

            // Grid column headers
            AddTranslation("Action", "Action", "လုပ်ဆောင်ချက်");
            AddTranslation("ID", "ID", "အိုင်ဒီ");
            AddTranslation("Sale", "Sale", "အရောင်း");
            AddTranslation("Sales", "Sales", "အရောင်းများ");
            AddTranslation("Sale #", "Sale #", "အရောင်းနံပါတ်");
            AddTranslation("Draft #", "Draft #", "ကြိုစာတမ်း နံပါတ်");
            AddTranslation("Purchase", "Purchase", "ဝယ်ယူမှု");
            AddTranslation("Invoice #", "Invoice #", "ငွေတောင်းခံလွှာ နံပါတ်");
            AddTranslation("Payment #", "Payment #", "ငွေပေးချေမှု နံပါတ်");
            AddTranslation("Return #", "Return #", "ပြန်ပေးမှု နံပါတ်");
            AddTranslation("Movement #", "Movement #", "စတော့ရွေ့ပြောင်း နံပါတ်");
            AddTranslation("Transfer #", "Transfer #", "လွှဲပြောင်း နံပါတ်");
            AddTranslation("Serial #", "Serial #", "စီးရီးနယ်နံပါတ်");
            AddTranslation("Product #", "Product #", "ပစ္စည်း နံပါတ်");
            AddTranslation("Expense #", "Expense #", "ကုန်ကျစရိတ် နံပါတ်");
            AddTranslation("Stock", "Stock", "စတော့");
            AddTranslation("Balance", "Balance", "လက်ကျန်");
            AddTranslation("Debit", "Debit", "ထုတ်ငွေ");
            AddTranslation("Credit", "Credit", "ဝင်ငွေ");
            AddTranslation("Count", "Count", "အရေအတွက်");
            AddTranslation("Details", "Details", "အသေးစိတ်");
            AddTranslation("Received Date", "Received Date", "လက်ခံရက်စွဲ");
            AddTranslation("Line Total", "Line Total", "ကြောင်းစုပေါင်း");
            AddTranslation("Total Cost", "Total Cost", "စုစုပေါင်းကုန်ကျစရိတ်");
            AddTranslation("Unit Price", "Unit Price", "ယူနစ်ဈေး");
            AddTranslation("Unit Cost", "Unit Cost", "ယူနစ်ကုန်ကျစရိတ်");
            AddTranslation("Timestamp", "Timestamp", "အချိန်ကိုက်");
            AddTranslation("Performed By", "Performed By", "လုပ်ဆောင်သူ");
            AddTranslation("Recommendation", "Recommendation", "အကြံပြုချက်");
            AddTranslation("From Location", "From Location", "မှ တည်နေရာ");
            AddTranslation("To Location", "To Location", "သို့ တည်နေရာ");
            AddTranslation("Transfer Date", "Transfer Date", "လွှဲပြောင်းရက်စွဲ");
            AddTranslation("Assembly", "Assembly", "ပေါင်းစပ်မှု");
            AddTranslation("Failed", "Failed", "မအောင်မြင်");
            AddTranslation("Processed", "Processed", "လုပ်ဆောင်ပြီး");
            AddTranslation("Source", "Source", "ရင်းမြစ်");
            AddTranslation("Target", "Target", "ပန်းတိုင်");
            AddTranslation("Frequency", "Frequency", "ကြိမ်နှုန်း");
            AddTranslation("Port", "Port", "ပို့က်");
            AddTranslation("SMTP Host", "SMTP Host", "SMTP ဟုစ်");
            AddTranslation("IP Address", "IP Address", "IP လိပ်စာ");
            AddTranslation("Last Login", "Last Login", "နောက်ဆုံးဝင်ရောက်ချိန်");
            AddTranslation("Last Run", "Last Run", "နောက်ဆုံး လုပ်ဆောင်ချိန်");
            AddTranslation("Last Status", "Last Status", "နောက်ဆုံး အခြေအနေ");
            AddTranslation("Schedule", "Schedule", "အစီအစဉ်");
            AddTranslation("Icon", "Icon", "အိုင်ကွန်");
            AddTranslation("Font Family", "Font Family", "ဖောင့်အမျိုးအစား");
            AddTranslation("Font Size", "Font Size", "ဖောင့်အရွယ်အစား");
            AddTranslation("Primary Color", "Primary Color", "အဓိကအရောင်");
            AddTranslation("Secondary Color", "Secondary Color", "ဒုတိယအရောင်");
            AddTranslation("Background", "Background", "နောက်ခံ");
            AddTranslation("Text Color", "Text Color", "စာသားအရောင်");
            AddTranslation("Module", "Module", "မော်ဂျူး");
            AddTranslation("Full Name", "Full Name", "အမည်အပြည့်");
            AddTranslation("Contact Person", "Contact Person", "ဆက်သွယ်ရန်ပုဂ္ဂိုလ်");
            AddTranslation("Credit Limit", "Credit Limit", "အကြွေးကန့်သတ်ချက်");
            AddTranslation("Display Order", "Display Order", "ပြသမှုအစဉ်");
            AddTranslation("Active", "Active", "အသုံးပြုပါ");
            AddTranslation("Vendor", "Vendor", "ပေးသွင်းသူ");
            AddTranslation("Net", "Net", "အသား");
            AddTranslation("Backup Path", "Backup Path", "အရန်သိမ်းရာနေရာ");
            AddTranslation("Symbol", "Symbol", "သင်္ကေတ");

            // Lookup prompts (NullText)
            AddTranslation("Select a user...", "Select a user...", "အသုံးပြုသူ ရွေးချယ်ပါ...");
            AddTranslation("Select company...", "Select company...", "ကုမ္ပဏီ ရွေးချယ်ပါ...");
            AddTranslation("Select customer...", "Select customer...", "ဖောက်သည် ရွေးချယ်ပါ...");
            AddTranslation("Select expense type...", "Select expense type...", "ကုန်ကျစရိတ်အမျိုးအစား ရွေးချယ်ပါ...");
            AddTranslation("Select location...", "Select location...", "တည်နေရာ ရွေးချယ်ပါ...");
            AddTranslation("Select method...", "Select method...", "နည်းလမ်း ရွေးချယ်ပါ...");
            AddTranslation("Select month...", "Select month...", "လ ရွေးချယ်ပါ...");
            AddTranslation("Select payment type...", "Select payment type...", "ငွေပေးချေမှုအမျိုးအစား ရွေးချယ်ပါ...");
            AddTranslation("Select product...", "Select product...", "ပစ္စည်း ရွေးချယ်ပါ...");
            AddTranslation("Select purchase...", "Select purchase...", "ဝယ်ယူမှု ရွေးချယ်ပါ...");
            AddTranslation("Select sale...", "Select sale...", "အရောင်း ရွေးချယ်ပါ...");
            AddTranslation("Select supplier...", "Select supplier...", "ပေးသွင်းသူ ရွေးချယ်ပါ...");
            AddTranslation("Select type...", "Select type...", "အမျိုးအစား ရွေးချယ်ပါ...");
            AddTranslation("Select user...", "Select user...", "အသုံးပြုသူ ရွေးချယ်ပါ...");
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
