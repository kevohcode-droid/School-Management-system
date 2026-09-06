using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Application.AuditLogs;
using SchoolErp.Application.Dashboard;
using SchoolErp.Application.Fees;
using SchoolErp.Application.Settings;
using SchoolErp.Application.Staff;
using SchoolErp.Application.Students;
using SchoolErp.Application.Tenants;

namespace SchoolErp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFeeService, FeeService>();
        return services;
    }
}
