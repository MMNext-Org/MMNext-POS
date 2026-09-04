using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Value Object representing a batch of serial numbers for inventory tracking.
    /// Immutable and compared by value.
    /// </summary>
    public readonly struct SerialBatch : IEquatable<SerialBatch>
    {
        private readonly ImmutableArray<SerialNumber> _serialNumbers;

        /// <summary>
        /// Creates a new SerialBatch from a collection of serial numbers.
        /// </summary>
        /// <param name="serialNumbers">The serial numbers in this batch.</param>
        /// <exception cref="ArgumentException">Thrown when the batch is invalid.</exception>
        public SerialBatch(IEnumerable<SerialNumber> serialNumbers)
        {
            var list = serialNumbers?.ToList() ?? new List<SerialNumber>();

            if (list.Count == 0)
            {
                throw new ArgumentException("Serial batch must contain at least one serial number.", nameof(serialNumbers));
            }

            if (list.Count > 10000)
            {
                throw new ArgumentException("Serial batch cannot contain more than 10,000 serial numbers.", nameof(serialNumbers));
            }

            // Check for duplicates
            var distinct = list.Distinct().ToList();
            if (distinct.Count != list.Count)
            {
                throw new ArgumentException("Serial batch contains duplicate serial numbers.", nameof(serialNumbers));
            }

            _serialNumbers = distinct.ToImmutableArray();
        }

        /// <summary>
        /// Creates a new SerialBatch from a collection of strings.
        /// </summary>
        public SerialBatch(IEnumerable<string> serialNumbers)
            : this(serialNumbers?.Select(s => new SerialNumber(s)) ?? Enumerable.Empty<SerialNumber>())
        {
        }

        /// <summary>
        /// Gets the serial numbers in this batch.
        /// </summary>
        public IReadOnlyList<SerialNumber> SerialNumbers => _serialNumbers;

        /// <summary>
        /// Gets the count of serial numbers in this batch.
        /// </summary>
        public int Count => _serialNumbers.Length;

        /// <summary>
        /// Gets whether this batch is empty.
        /// </summary>
        public bool IsEmpty => _serialNumbers.IsDefaultOrEmpty;

        /// <summary>
        /// Checks if a specific serial number is in this batch.
        /// </summary>
        public bool Contains(SerialNumber serialNumber) => _serialNumbers.Contains(serialNumber);

        /// <summary>
        /// Checks if a specific serial number (as string) is in this batch.
        /// </summary>
        public bool Contains(string serialNumber) => _serialNumbers.Any(s => s.Value.Equals(serialNumber, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets the first serial number in the batch.
        /// </summary>
        public SerialNumber First => _serialNumbers.IsDefaultOrEmpty ? SerialNumber.Empty : _serialNumbers[0];

        /// <summary>
        /// Gets the last serial number in the batch.
        /// </summary>
        public SerialNumber Last => _serialNumbers.IsDefaultOrEmpty ? SerialNumber.Empty : _serialNumbers[^1];

        /// <summary>
        /// Creates a new SerialBatch with additional serial numbers.
        /// </summary>
        public SerialBatch AddRange(IEnumerable<SerialNumber> serialNumbers)
        {
            var newList = _serialNumbers.ToList();
            newList.AddRange(serialNumbers);
            return new SerialBatch(newList);
        }

        /// <summary>
        /// Creates a new SerialBatch with a serial number removed.
        /// </summary>
        public SerialBatch Remove(SerialNumber serialNumber)
        {
            var newList = _serialNumbers.Where(s => !s.Equals(serialNumber)).ToList();
            return new SerialBatch(newList);
        }

        /// <summary>
        /// Creates a new SerialBatch with a serial number removed (by string).
        /// </summary>
        public SerialBatch Remove(string serialNumber)
        {
            var newList = _serialNumbers.Where(s => !s.Value.Equals(serialNumber, StringComparison.OrdinalIgnoreCase)).ToList();
            return new SerialBatch(newList);
        }

        /// <summary>
        /// Gets a sub-batch (page) of serial numbers.
        /// </summary>
        public SerialBatch GetPage(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;

            var paged = _serialNumbers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new SerialBatch(paged);
        }

        /// <summary>
        /// Splits this batch into multiple batches of the specified size.
        /// </summary>
        public IEnumerable<SerialBatch> Split(int batchSize)
        {
            if (batchSize < 1) batchSize = 100;

            for (int i = 0; i < _serialNumbers.Length; i += batchSize)
            {
                var batch = _serialNumbers.Skip(i).Take(batchSize).ToList();
                yield return new SerialBatch(batch);
            }
        }

        /// <inheritdoc />
        public bool Equals(SerialBatch other) => _serialNumbers.SequenceEqual(other._serialNumbers);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is SerialBatch other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var sn in _serialNumbers)
            {
                hash.Add(sn);
            }
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public override string ToString() => $"SerialBatch [{Count} items]";

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(SerialBatch left, SerialBatch right) => left.Equals(right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(SerialBatch left, SerialBatch right) => !left.Equals(right);

        /// <summary>
        /// Implicit conversion from array of SerialNumber to SerialBatch.
        /// </summary>
        public static implicit operator SerialBatch(SerialNumber[] serialNumbers) => new SerialBatch(serialNumbers);

        /// <summary>
        /// Implicit conversion from List of SerialNumber to SerialBatch.
        /// </summary>
        public static implicit operator SerialBatch(List<SerialNumber> serialNumbers) => new SerialBatch(serialNumbers);

        /// <summary>
        /// Implicit conversion from array of string to SerialBatch.
        /// </summary>
        public static implicit operator SerialBatch(string[] serialNumbers) => new SerialBatch(serialNumbers);

        /// <summary>
        /// Implicit conversion from List of string to SerialBatch.
        /// </summary>
        public static implicit operator SerialBatch(List<string> serialNumbers) => new SerialBatch(serialNumbers);
    }
}
