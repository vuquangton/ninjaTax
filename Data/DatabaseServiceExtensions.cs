using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;

namespace ninjaTax.Data;

/// <summary>
/// Mở rộng IServiceCollection để cấu hình Multi-Database Provider động.
/// Hỗ trợ: Sqlite (mặc định), SqlServer, PostgreSql, MariaDb.
/// Tuân thủ:
/// - Tự động fallback về Sqlite nếu cấu hình thiếu hoặc không hợp lệ.
/// - Xác thực chuỗi kết nối và thông báo lỗi rõ ràng nếu thiếu.
/// - Ghi log chi tiết hệ quản trị CSDL được kích hoạt khi khởi động.
/// </summary>
public static class DatabaseServiceExtensions
{
    public const string DefaultProvider = "Sqlite";

    public static string AddMultiDatabaseContext(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        var rawProvider = configuration["DatabaseProvider"];
        string provider;

        if (string.IsNullOrWhiteSpace(rawProvider))
        {
            logger.LogWarning("Không tìm thấy cấu hình 'DatabaseProvider'. Tự động fallback về hệ quản trị mặc định: {DefaultProvider}", DefaultProvider);
            provider = DefaultProvider;
        }
        else
        {
            var normalized = rawProvider.Trim().ToLowerInvariant();
            provider = normalized switch
            {
                "sqlite" => "Sqlite",
                "sqlserver" or "mssql" => "SqlServer",
                "postgresql" or "postgres" or "npgsql" => "PostgreSql",
                "mariadb" or "mysql" => "MariaDb",
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(provider))
            {
                logger.LogWarning("Hệ quản trị CSDL '{RawProvider}' không được hỗ trợ. Tự động fallback về: {DefaultProvider}", rawProvider, DefaultProvider);
                provider = DefaultProvider;
            }
        }

        var connectionString = configuration.GetConnectionString(provider);

        // Fallback chuỗi kết nối cho Sqlite nếu chưa được cấu hình
        if (string.IsNullOrWhiteSpace(connectionString) && provider == "Sqlite")
        {
            connectionString = "Data Source=ninjatax.db";
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var errorMsg = $"Không tìm thấy chuỗi kết nối 'ConnectionStrings:{provider}' cho nhà cung cấp CSDL '{provider}'. Vui lòng cấu hình trong appsettings.json hoặc biến môi trường.";
            logger.LogCritical("{Error}", errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        logger.LogInformation(">>> Khởi tạo AppDbContext với Provider: [{Provider}] <<<", provider);

        services.AddDbContext<AppDbContext>(options =>
        {
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            switch (provider)
            {
                case "Sqlite":
                    options.UseSqlite(connectionString);
                    break;

                case "SqlServer":
                    options.UseSqlServer(connectionString);
                    break;

                case "PostgreSql":
                    options.UseNpgsql(connectionString);
                    break;

                case "MariaDb":
                    var serverVersion = new MariaDbServerVersion(new Version(11, 0, 0));
                    options.UseMySql(connectionString, serverVersion);
                    break;
            }
        });

        return provider;
    }
}
