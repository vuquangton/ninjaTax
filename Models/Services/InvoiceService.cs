using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;

namespace ninjaTax.Models.Services;

/// <summary>
/// Service implementation for creating sales invoices.
/// Handles invoice persistence and minimal journal posting to satisfy TT99 invariants.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;

    public InvoiceService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<HoaDonBanHang> CreateInvoiceAsync(HoaDonBanHang invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));
        // Ensure related entities are attached
        if (invoice.KhachHangId != 0)
        {
            var kh = await _context.DoiTuongs.FindAsync(invoice.KhachHangId);
            if (kh == null) throw new InvalidOperationException("Khách hàng không tồn tại");
            invoice.KhachHang = kh;
        }

        // Compute totals from line items
        invoice.TongTienHang = invoice.ChiTietBans.Sum(c => c.ThanhTien);
        invoice.TongTienThueVat = invoice.ChiTietBans.Sum(c => c.TienThueVat);
        invoice.TongThanhToan = invoice.TongTienHang + invoice.TongTienThueVat - invoice.TongTienChietKhau;

        // Add invoice
        _context.HoaDonBanHangs.Add(invoice);
        await _context.SaveChangesAsync();

        // Minimal journal posting using existing ChiTietButToan schema
        var journal = new ButToan
        {
            NgayHachToan = invoice.NgayHachToan,
            DienGiai = $"Bút toán bán hàng {invoice.SoChungTu}",
            TongNo = 0m,
            TongCo = 0m,
            ChiTietButToans = new List<ChiTietButToan>()
        };
        foreach (var line in invoice.ChiTietBans)
        {
            // Debit account (Nợ) for revenue
            var debit = new ChiTietButToan
            {
                TaiKhoanNoId = line.TaiKhoanNoId,
                TaiKhoanCoId = line.TaiKhoanDoanhThuId,
                SoTien = line.ThanhTien,
                DienGiai = $"Doanh thu dòng {line.DongSo}"
            };
            // Credit tax account
            var creditTax = new ChiTietButToan
            {
                TaiKhoanNoId = line.TaiKhoanThueId,
                TaiKhoanCoId = line.TaiKhoanDoanhThuId,
                SoTien = line.TienThueVat,
                DienGiai = $"Thuế VAT dòng {line.DongSo}"
            };
            journal.ChiTietButToans.Add(debit);
            journal.ChiTietButToans.Add(creditTax);
            journal.TongNo += debit.SoTien + creditTax.SoTien;
            journal.TongCo += debit.SoTien + creditTax.SoTien;
        }
        _context.ButToans.Add(journal);
        await _context.SaveChangesAsync();
        // Link invoice to journal for verification
        invoice.ButToanDoanhThuId = journal.Id;
        invoice.ButToanDoanhThu = journal;

        await _context.SaveChangesAsync();
        return invoice;
    }
}
