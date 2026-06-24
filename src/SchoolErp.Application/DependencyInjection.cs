using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Application.Students;
using SchoolErp.Application.Tenants;

namespace SchoolErp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITenantService, TenantService>();
        return services;
    }
}
