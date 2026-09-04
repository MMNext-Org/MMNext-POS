using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Base class for Value Objects in Domain-Driven Design.
    /// Value objects are compared by their values, not identity.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Gets the atomic values that define this value object's equality.
        /// Override this method to specify which properties participate in equality comparison.
        /// </summary>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(ValueObject? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ValueObject);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

        /// <summary>
        /// Gets all public instance properties of the value object.
        /// Useful for debugging and serialization.
        /// </summary>
        public IReadOnlyDictionary<string, object?> GetProperties()
        {
            var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p.GetValue(this));

            return properties;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var properties = GetProperties();
            var values = properties.Select(kvp => $"{kvp.Key} = {kvp.Value ?? "null"}");
            return $"{GetType().Name} {{ {string.Join(", ", values)} }}";
        }
    }
}
