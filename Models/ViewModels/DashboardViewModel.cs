namespace ninjaTax.Models.ViewModels;

public class DashboardViewModel
{
    public decimal DoanhThu { get; set; }
    public decimal ChiPhi { get; set; }
    public decimal PhaiThu { get; set; }
    public decimal PhaiTra { get; set; }
}

public class MonthlyCashFlowDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal NetCashFlow => Inflow - Outflow;
}

public class DebtorDto
{
    public long DoiTuongId { get; set; }
    public string MaDoiTuong { get; set; } = string.Empty;
    public string TenDoiTuong { get; set; } = string.Empty;
    public decimal DuNo { get; set; }
}
