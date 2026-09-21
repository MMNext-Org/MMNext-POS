using System;
using System.Collections.Generic;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Settings service for POS application configuration.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets the configured POS system name.
        /// </summary>
        string GetSystemName();

        /// <summary>
        /// Gets the store location/site name (e.g., "MMNextPOS Main Office").
        /// </summary>
        string GetLocationName();

        /// <summary>
        /// Gets the configured default currency code (e.g., "MMK", "THB", "USD").
        /// </summary>
        string GetDefaultCurrencyCode();
    }
}
