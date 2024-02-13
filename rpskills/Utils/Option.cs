

using System;
using System.ComponentModel;
using System.Reflection.Metadata;

namespace rpskills.Utils
{
    public class Option<T>
    {
        private readonly AbstractStructWrapper<T> Value;
        public readonly bool HasValue;

        public Option(T val)
        {
            HasValue = val != null;
            Value = new AbstractStructWrapper<T>(val);
        }

        public T Unwrap()
        {
            if (!HasValue)
            {
                throw new OptionException();
            }
            return Value.data;
        }

    }
}