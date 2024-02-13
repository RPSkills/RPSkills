using System;

namespace rpskills.Utils
{
    public readonly struct AbstractStructWrapper<T>
    {
        public readonly T data;
        public AbstractStructWrapper(T d)
        {
            data = d;
        }
    }

    public class OptionException: Exception
    {}
}