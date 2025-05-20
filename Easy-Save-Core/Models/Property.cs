using System;
using System.Collections.Generic;

namespace CLEA.EasySaveCore.Models
{
    public class Property<T>
    {
        private readonly string _name;
        private readonly string? _description;
        private T _value;

        public string Name
        {
            get => _name;
            set => throw new NotSupportedException("Name property is read-only.");
        }

        public string? Description
        {
            get => _description;
            set => throw new NotSupportedException("Description property is read-only.");
        }

        public T Value
        {
            get => _value;
            set => _value = value;
        }
        
        public Property(string name, T value, string? description = null)
        {
            _name = name;
            _value = value;
            _description = description;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (GetType() != obj.GetType()) return false;

            var other = (Property<T>)obj;

            return _name == other._name && _description == other._description &&
                   EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_name, _description);
        }
    }
}