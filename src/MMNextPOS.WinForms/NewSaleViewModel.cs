using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// ViewModel for the NewSaleForm. Contains UI state and business logic.
    /// </summary>
    public class NewSaleViewModel
    {
        public ObservableCollection<SaleDetailViewModel> LineItems { get; } = new();

        private int? _customerId;
        public string CustomerName { get; private set; } = "Walk-in";

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            private set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            private set
            {
                _isSaving = value;
                OnPropertyChanged(nameof(IsSaving));
            }
        }

        private readonly ISalesService _salesService;
        private readonly IProductService _productService;
        private readonly ISaleTempService _saleTempService;
        private SaleTemp? _currentDraft;

        public NewSaleViewModel(ISalesService salesService, IProductService productService, ISaleTempService saleTempService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _saleTempService = saleTempService ?? throw new ArgumentNullException(nameof(saleTempService));
        }

        #region Public Commands

        public async Task LoadDraftAsync()
        {
            if (_currentDraft != null)
            {
                try
                {
                    CustomerName = _currentDraft.CustomerName ?? "Walk-in";
                    _customerId = _currentDraft.CustomerId;
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to load draft: {ex.Message}");
                }
            }
        }

        public async Task AddProductLineAsync(Product product)
        {
            var line = new SaleDetailViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku,
                Quantity = 1,
                UnitPrice = product.Price,
                AvailableStock = product.StockQuantity
            };
            LineItems.Add(line);
            TotalAmount = LineItems.Sum(l => l.LineTotal);
        }

        public async Task SaveSaleAsync()
        {
            if (!CanSave())
            {
                // UI validation already prevented this, but defensive check
                return;
            }

            IsSaving = true;
            try
            {
                var customerId = _customerId ?? throw new InvalidOperationException("Customer not selected");
                var sale = new Sale
                {
                    CustomerId = customerId,
                    SaleDate = DateTime.UtcNow,
                    TotalAmount = TotalAmount
                };

                var details = LineItems.Select(l => new SaleDetail
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList();

                var createdSale = await _salesService.CreateSaleAsync(sale, details);
                // Optionally delete draft if exists
                if (_currentDraft != null)
                {
                    await _saleTempService.DeleteAsync(_currentDraft.Id);
                    _currentDraft = null;
                }

                // Reset UI state
                LineItems.Clear();
                TotalAmount = 0;
                CustomerName = "Walk-in";
                _customerId = null;
                ShowInfo($"Sale #{createdSale.Id} created successfully!");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save sale: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task HoldDraftAsync()
        {
            if (LineItems.Count == 0)
            {
                ShowInfo("Please add at least one product line before holding.");
                return;
            }
            if (_customerId == null)
            {
                ShowInfo("Please select a customer before holding.");
                return;
            }

            IsSaving = true;
            try
            {
                var draft = new SaleTemp
                {
                    CustomerId = _customerId ?? 0,
                    CustomerName = CustomerName,
                    SaleDate = DateTime.UtcNow,
                    TotalAmount = TotalAmount,
                    DiscountAmount = 0,
                    TaxAmount = 0,
                    NetAmount = TotalAmount,
                    Status = "Draft",
                    Notes = "Auto-saved draft"
                };

                await _saleTempService.AddAsync(draft);
                ShowInfo("Draft saved successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to hold draft: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        #endregion

        #region Helpers

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(CustomerName) && LineItems.Count > 0 && ValidateStock();
        }

        private bool ValidateStock()
        {
            foreach (var line in LineItems)
            {
                if (line.Quantity > line.AvailableStock)
                {
                    ShowError($"Insufficient stock for {line.ProductName}. Available: {line.AvailableStock}, Requested: {line.Quantity}");
                    return false;
                }
            }
            return true;
        }

        private void ShowInfo(string message) => // Simplified; in real app use proper UI feedback
            System.Windows.Forms.MessageBox.Show(message, "Info", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

        private void ShowError(string message) => // Simplified; in real app use proper UI feedback
            System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

        #endregion

        #region INotifyPropertyChanged Implementation

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));

        #endregion
    }

    /// <summary>
    /// View model for a single line item in a sale.
    /// </summary>
    public class SaleDetailViewModel : ObservableObject
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int AvailableStock { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }

    /// <summary>
    /// Simple base class for INotifyPropertyChanged.
    /// </summary>
    public class ObservableObject : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}
