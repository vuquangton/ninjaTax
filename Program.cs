using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Khởi tạo logger tạm thời để ghi log quá trình bootstrap CSDL
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var startupLogger = loggerFactory.CreateLogger("DatabaseStartup");

// Đăng ký kiến trúc Multi-Database (AC1, AC2, AC3, AC5)
var activeProvider = builder.Services.AddMultiDatabaseContext(builder.Configuration, startupLogger);

// Đăng ký dịch vụ Nghiệp vụ Bút toán kế toán (Core General Ledger)
builder.Services.AddScoped<IButToanService, ButToanService>();
builder.Services.AddScoped<IHachToanMuaHangService, HachToanMuaHangService>();
builder.Services.AddScoped<IHachToanBanHangService, HachToanBanHangService>();
builder.Services.AddScoped<ICongNoService, CongNoService>();
builder.Services.AddScoped<IThuChiService, ThuChiService>();
builder.Services.AddScoped<ITaiSanService, TaiSanService>();

var app = builder.Build();

app.Logger.LogInformation("Ứng dụng ninjaTax khởi động thành công với hệ quản trị CSDL: [{ActiveProvider}]", activeProvider);

// Tự động khởi tạo cấu trúc CSDL và nạp Hệ thống Tài khoản TT99 ban đầu
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ButToan}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
