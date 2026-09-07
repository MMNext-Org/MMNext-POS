using System;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Helpers to render and parse the human-readable invoice number format
    /// produced by <see cref="IInvoiceNumberGenerator"/>.
    /// Format: PREFIX-YYYY-NNNNNN (six-digit zero-padded sequence).
    /// </summary>
    public static class InvoiceNumberFormat
    {
        public static string Format(string prefix, int year, long sequence)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix is required.", nameof(prefix));
            }
            if (year < 1900 || year > 9999)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be a 4-digit value.");
            }
            if (sequence < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Sequence must be non-negative.");
            }
            return $"{prefix.ToUpperInvariant()}-{year:D4}-{sequence:D6}";
        }
    }
}
