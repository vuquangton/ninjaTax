namespace ninjaTax.Models.Entities;

/// <summary>
/// Exchange rate history for multi‑currency support.
/// Stores the rate from one currency to another with an effective date.
/// </summary>
public class ExchangeRateHistory
{
    public long Id { get; set; }
    public string FromCurrency { get; set; } = string.Empty; // ISO 4217 code, e.g., "USD"
    public string ToCurrency { get; set; } = string.Empty;   // ISO 4217 code, e.g., "VND"
    public decimal Rate { get; set; } // precision 19,4
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
