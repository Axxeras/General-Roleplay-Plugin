namespace GRPP.Extensions;

using System;
using LabApi.Features.Console;

public static class StringExtensions
{
    public static string CutStringToValue(this string stringToCut, int cutLength) // use `this` so we can call myValue.CutStringToValue instead of Extensions.StringExtensionsCutStringToValue(val)
    {
        if (!string.IsNullOrEmpty(stringToCut)) return stringToCut.Length <= cutLength ? stringToCut : stringToCut.AsSpan(0, cutLength).ToString();
        Logger.Error("Error when attempting to cut string. String was null or empty.");
        return string.Empty; // if string is null or empty, return nothing.
    }
}