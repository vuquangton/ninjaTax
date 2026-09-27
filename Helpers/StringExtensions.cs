using System.Globalization;
using System.Text;

namespace ninjaTax.Helpers;

/// <summary>
/// String extension methods for Vietnamese text processing.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Removes all diacritical marks from a Vietnamese string.
    /// Handles đ/Đ explicitly since they are not decomposed by Unicode normalization.
    /// Examples:
    ///   "Nguyễn Văn A"  → "Nguyen Van A"
    ///   "đường"         → "duong"
    ///   "Đà Nẵng"       → "Da Nang"
    /// </summary>
    public static string RemoveDiacritics(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Handle đ/Đ first — they do NOT decompose via FormD
        text = text.Replace('đ', 'd').Replace('Đ', 'D');

        // Decompose combined characters into base character + combining marks
        var normalized = text.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        // Recompose (FormC) — result will be pure ASCII for Vietnamese text
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Returns true if the text contains the search term, ignoring diacritics and case.
    /// Used for Vietnamese unaccented search: "nguyen" matches "Nguyễn".
    /// </summary>
    public static bool ContainsUnaccented(this string text, string search)
    {
        if (string.IsNullOrEmpty(search)) return true;
        if (string.IsNullOrEmpty(text))  return false;

        return text.RemoveDiacritics()
                   .Contains(search.RemoveDiacritics(), StringComparison.OrdinalIgnoreCase);
    }
}
