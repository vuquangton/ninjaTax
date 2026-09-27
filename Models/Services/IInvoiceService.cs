using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Service contract for creating sales invoices.
/// </summary>
public interface IInvoiceService
{
    Task<HoaDonBanHang> CreateInvoiceAsync(HoaDonBanHang invoice);
}
