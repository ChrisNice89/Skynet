using System;

namespace Skynet.Predicates;

internal readonly struct BoolAscending :
    IStrictOrder<bool>
{
    public bool IsLess(
        bool left,
        bool right)
    {
        return !left && right;
    }
}

internal readonly struct IntAscending :
    IStrictOrder<int>
{
    public bool IsLess(
        int left,
        int right)
    {
        return left < right;
    }
}

internal readonly struct FloatAscending :
    IStrictOrder<double>
{
    public bool IsLess(
        double left,
        double right)
    {
        return left.CompareTo(right) < 0;
    }
}

internal readonly struct NumericAscending :
    IStrictOrder<decimal>
{
    public bool IsLess(
        decimal left,
        decimal right)
    {
        return left < right;
    }
}

internal readonly struct DateAscending :
    IStrictOrder<DateTime>
{
    public bool IsLess(
        DateTime left,
        DateTime right)
    {
        return left < right;
    }
}

internal readonly struct CharsAscending :
    IStrictOrder<string>
{
    public bool IsLess(
        string left,
        string right)
    {
        return string.CompareOrdinal(
            left,
            right) < 0;
    }
}
