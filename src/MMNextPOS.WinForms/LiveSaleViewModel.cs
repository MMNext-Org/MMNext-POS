using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// ViewModel for LiveSaleForm. Encapsulates UI state and business logic.
    /// </summary>
    public class LiveSaleViewModel : INotifyPropertyChanged
    {
        private readonly ISaleTempService _saleTempService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly ITranslationService _translationService;
        private readonly ISalesService _salesService; // for print receipt

        // Selected customer
        private int? _selectedCustomerId;
        public int? SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                _selectedCustomerId = value;
                OnPropertyChanged(nameof(SelectedCustomerId));
                // Update customer name if draft is loaded
                if (_currentDraft != null && _selectedCustomerId == _currentDraft.CustomerId)
                {
                    SelectedCustomerName = _currentDraft.CustomerName;
                }
            }
        }

        private string? _selectedCustomerName;
        public string? SelectedCustomerName
        {
            get => _selectedCustomerName;
            set
            {
                _selectedCustomerName = value;
                OnPropertyChanged(nameof(SelectedCustomerName));
            }
        }

        // Payment method
        private string _selectedPaymentMethod = "Cash";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged(nameof(SelectedPaymentMethod));
            }
        }

        // Line items collection (observable)
        private readonly ObservableCollection<SaleDetailViewModel> _lineItems = new();
        public IReadOnlyList<SaleDetailViewModel> LineItems => _lineItems;

        // Total amount (computed)
        public decimal TotalAmount => _lineItems.Sum(l => l.LineTotal);

        // Property changed notification
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Constructor
        public LiveSaleViewModel(
            ISaleTempService saleTempService,
            ICustomerService customerService,
            IPaymentService paymentService,
            IProductService productService,
            ITranslationService translationService,
            ISalesService salesService)
        {
            _saleTempService = saleTempService;
            _customerService = customerService;
            _paymentService = paymentService;
            _productService = productService;
            _translationService = translationService;
            _salesService = salesService;
        }

        // Draft being displayed (set by form)
        public SaleTemp? CurrentDraft { get; set; }

        // Load reference data (customers, products, payment methods)
        public async Task LoadReferenceDataAsync()
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                // In a real UI, bind the LookUpEdit's DataSource to this list
                // For now, just store for later use
            }
            catch (Exception ex)
            {
                // Handle error (e.g., show message)
            }
        }

        // Load draft (if any)
        public async Task LoadDraftAsync()
        {
            if (CurrentDraft == null) return;

            try
            {
                // Set customer selection
                if (CurrentDraft.CustomerId.HasValue)
                {
                    SelectedCustomerId = CurrentDraft.CustomerId.Value;
                    SelectedCustomerName = CurrentDraft.CustomerName;
                }

                // Populate line items (placeholder – in real app load from detail table)
                _lineItems.Clear();

                // Set total label (could be bound to TotalAmount)
                // For now just ensure TotalAmount updates when line items change
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        // Print receipt command
        public async Task PrintReceiptAsync()
        {
            if (CurrentDraft == null) return;

            try
            {
                var report = new SaleReceiptReport(
                    _salesService,
                    _productService,
                    _customerService,
                    _translationService);

                await report.PopulateAsync(CurrentDraft.Id, default);

                using var stream = new System.IO.MemoryStream();
                await report.ExportToPdf(stream);
                var receiptBytes = stream.ToArray();

                // For demo, just show a message box with the receipt text
                string receiptText = await GenerateReceiptTextAsync(CurrentDraft.Id);
                System.Windows.Forms.MessageBox.Show(this, receiptText, "Receipt", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Show error
            }
        }

        // Generate receipt text (simplified)
        private async Task<string> GenerateReceiptTextAsync(int saleId)
        {
            var report = new SaleReceiptReport(
                _salesService,
                _productService,
                _customerService,
                _translationService);

            await report.PopulateAsync(saleId, default);

            using var stream = new System.IO.MemoryStream();
            await report.ExportToPdf(stream);
            var lines = new System.Collections.Generic.List<string>
            {
                "MMNext POS - Receipt",
                "====================",
                $"Sale #: {saleId}",
                $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Customer: {CurrentDraft?.CustomerName ?? "Walk-in"}",
                "----------------------------"
            };

            // If there are line items, add them
            foreach (var line in _lineItems)
            {
                lines.Add($"{line.ProductName} x{line.Quantity} @ {line.UnitPrice:C2} = {line.LineTotal:C2}");
            }

            lines.Add("----------------------------");
            lines.Add($"Total: {TotalAmount:C2}");
            lines.Add("====================");
            lines.Add("Thank you for your purchase!");

            return string.Join(Environment.NewLine, lines);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// View model for a single line item in a sale (grid row).
    /// </summary>
    public class SaleDetailViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int AvailableStock { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;
    }
}