using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Value Object representing a serial number.
    /// Immutable and compared by value.
    /// </summary>
    public readonly struct SerialNumber : IEquatable<SerialNumber>, IComparable<SerialNumber>
    {
        private readonly string _value;

        /// <summary>
        /// Creates a new SerialNumber value object.
        /// </summary>
        /// <param name="value">The serial number value.</param>
        /// <exception cref="ArgumentException">Thrown when the serial number is invalid.</exception>
        public SerialNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Serial number cannot be null or empty.", nameof(value));
            }

            var trimmed = value.Trim();
            if (trimmed.Length > 50)
            {
                throw new ArgumentException("Serial number cannot exceed 50 characters.", nameof(value));
            }

            // Validate format: alphanumeric with optional hyphens, underscores
            if (!IsValidFormat(trimmed))
            {
                throw new ArgumentException("Serial number can only contain alphanumeric characters, hyphens, and underscores.", nameof(value));
            }

            _value = trimmed.ToUpperInvariant();
        }

        /// <summary>
        /// Gets the serial number value.
        /// </summary>
        public string Value => _value ?? string.Empty;

        /// <summary>
        /// Implicit conversion from string to SerialNumber.
        /// </summary>
        public static implicit operator SerialNumber(string value) => new SerialNumber(value);

        /// <summary>
        /// Implicit conversion from SerialNumber to string.
        /// </summary>
        public static implicit operator string(SerialNumber serialNumber) => serialNumber.Value;

        /// <summary>
        /// Checks if the string is a valid serial number format.
        /// </summary>
        public static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            if (trimmed.Length > 50)
                return false;

            return IsValidFormat(trimmed);
        }

        /// <summary>
        /// Tries to create a SerialNumber from a string.
        /// </summary>
        public static bool TryCreate(string value, out SerialNumber serialNumber)
        {
            if (IsValid(value))
            {
                serialNumber = new SerialNumber(value);
                return true;
            }

            serialNumber = default;
            return false;
        }

        /// <summary>
        /// Validates the format of the serial number.
        /// </summary>
        private static bool IsValidFormat(string value)
        {
            // Allow alphanumeric, hyphens, underscores
            foreach (char c in value)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc />
        public bool Equals(SerialNumber other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is SerialNumber other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _value?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;

        /// <inheritdoc />
        public int CompareTo(SerialNumber other) => string.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override string ToString() => _value ?? string.Empty;

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(SerialNumber left, SerialNumber right) => left.Equals(right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(SerialNumber left, SerialNumber right) => !left.Equals(right);

        /// <summary>
        /// Less than operator.
        /// </summary>
        public static bool operator <(SerialNumber left, SerialNumber right) => left.CompareTo(right) < 0;

        /// <summary>
        /// Greater than operator.
        /// </summary>
        public static bool operator >(SerialNumber left, SerialNumber right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Less than or equal operator.
        /// </summary>
        public static bool operator <=(SerialNumber left, SerialNumber right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// Greater than or equal operator.
        /// </summary>
        public static bool operator >=(SerialNumber left, SerialNumber right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// Creates a SerialNumber from a string, throwing if invalid.
        /// </summary>
        public static SerialNumber Parse(string value) => new SerialNumber(value);

        /// <summary>
        /// Tries to parse a string into a SerialNumber.
        /// </summary>
        public static bool TryParse(string value, out SerialNumber serialNumber) => TryCreate(value, out serialNumber);

        /// <summary>
        /// Generates a random serial number with the specified prefix and length.
        /// </summary>
        public static SerialNumber Generate(string prefix = "SN", int length = 12)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "SN";

            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var suffix = new char[length];
            for (int i = 0; i < length; i++)
            {
                suffix[i] = chars[random.Next(chars.Length)];
            }

            return new SerialNumber($"{prefix.ToUpperInvariant()}-{new string(suffix)}");
        }

        /// <summary>
        /// Gets an empty/invalid serial number (for optional fields).
        /// </summary>
        public static SerialNumber Empty => new SerialNumber("EMPTY");

        /// <summary>
        /// Checks if this serial number is empty/invalid.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(_value) || _value == "EMPTY";
    }
}
