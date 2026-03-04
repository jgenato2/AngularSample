using Microsoft.Extensions.DependencyInjection;
using Server.Application.Abstractions;
using Server.Application.Features.Auth.Commands;
using Server.Application.Features.Auth.Handlers;
using Server.Application.Features.Claims.Commands;
using Server.Application.Features.Claims.Handlers;
using Server.Application.Features.Claims.Queries;
using Server.Application.Features.HealthInsurance.Commands;
using Server.Application.Features.HealthInsurance.Handlers;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Features.Users.Commands;
using Server.Application.Features.Users.Handlers;
using Server.Application.Features.Users.Queries;
using Server.Application.Models;
using Server.Application.Services;
using Server.Domain.Entities;
using Server.Presentation.Auditing;
using Server.Presentation.Contracts;

namespace Server.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICqrsDispatcher, CqrsDispatcher>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<IUsersApplicationService, UsersApplicationService>();
        services.AddScoped<IClaimsSeedService, ClaimsSeedService>();
        services.AddScoped<IHealthInsuranceSeedService, HealthInsuranceSeedService>();
        services.AddScoped<IClaimWorkflowService, ClaimWorkflowService>();
        services.AddScoped<IClaimValidationService, ClaimValidationService>();
        services.AddScoped<IClaimAuditService, ClaimAuditService>();
        services.AddScoped<IClaimsApplicationService, ClaimsApplicationService>();
        services.AddScoped<IAppInitializer>(sp => sp.GetRequiredService<IClaimsApplicationService>());
        services.AddScoped<IHealthInsuranceWorkflowService, HealthInsuranceWorkflowService>();
        services.AddScoped<IHealthInsuranceAnalyticsService, HealthInsuranceAnalyticsService>();
        services.AddScoped<IHealthInsuranceAuditService, HealthInsuranceAuditService>();
        services.AddScoped<IHealthInsuranceApplicationService, HealthInsuranceApplicationService>();
        services.AddScoped<IAppInitializer>(sp => sp.GetRequiredService<IHealthInsuranceApplicationService>());
        services.AddScoped<ICommandHandler<RegisterCommand, OperationResult<AuthPayload>>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, OperationResult<AuthPayload>>, LoginCommandHandler>();
        services.AddScoped<IQueryHandler<ListClaimsQuery, IEnumerable<Claim>>, ListClaimsQueryHandler>();
        services.AddScoped<IQueryHandler<GetClaimByIdQuery, OperationResult<Claim>>, GetClaimByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetClaimAuditLogsQuery, OperationResult<IEnumerable<ClaimAuditLogEntry>>>, GetClaimAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllClaimsAuditLogsQuery, IEnumerable<ClaimAuditLogEntry>>, GetAllClaimsAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetClaimsListAccessAuditLogsQuery, IEnumerable<ClaimAuditLogEntry>>, GetClaimsListAccessAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetClaimStatusWorkflowQuery, ClaimStatusWorkflowModel>, GetClaimStatusWorkflowQueryHandler>();
        services.AddScoped<ICommandHandler<CreateClaimCommand, OperationResult<Claim>>, CreateClaimCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateClaimCommand, OperationResult<Claim>>, UpdateClaimCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteClaimCommand, OperationResult<bool>>, DeleteClaimCommandHandler>();
        services.AddScoped<IQueryHandler<ListUsersQuery, IEnumerable<User>>, ListUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery, OperationResult<User>>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserAuditLogsQuery, OperationResult<IEnumerable<AuditLogEntry>>>, GetUserAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllUsersAuditLogsQuery, IEnumerable<AuditLogEntry>>, GetAllUsersAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersListAccessAuditLogsQuery, IEnumerable<AuditLogEntry>>, GetUsersListAccessAuditLogsQueryHandler>();
        services.AddScoped<ICommandHandler<CreateUserCommand, OperationResult<User>>, CreateUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand, OperationResult<User>>, UpdateUserCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand, OperationResult<bool>>, DeleteUserCommandHandler>();
        services.AddScoped<IQueryHandler<ListHealthInsurancePlansQuery, IEnumerable<HealthInsurancePlanResponse>>, ListHealthInsurancePlansQueryHandler>();
        services.AddScoped<IQueryHandler<GetHealthInsurancePlanByPolicyIdQuery, OperationResult<HealthInsurancePlanResponse>>, GetHealthInsurancePlanByPolicyIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetHealthInsuranceFinancialAnalyticsQuery, OperationResult<HealthInsuranceFinancialAnalyticsResponse>>, GetHealthInsuranceFinancialAnalyticsQueryHandler>();
        services.AddScoped<IQueryHandler<GetHealthInsuranceAuditLogsQuery, OperationResult<IEnumerable<AuditLogEntry>>>, GetHealthInsuranceAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllHealthInsuranceAuditLogsQuery, IEnumerable<AuditLogEntry>>, GetAllHealthInsuranceAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetHealthInsuranceListAccessAuditLogsQuery, IEnumerable<AuditLogEntry>>, GetHealthInsuranceListAccessAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetHealthInsuranceStatusWorkflowQuery, InsuranceStatusWorkflowModel>, GetHealthInsuranceStatusWorkflowQueryHandler>();
        services.AddScoped<ICommandHandler<CreateHealthInsurancePlanCommand, OperationResult<HealthInsurancePlanResponse>>, CreateHealthInsurancePlanCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateHealthInsurancePlanCommand, OperationResult<HealthInsurancePlanResponse>>, UpdateHealthInsurancePlanCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteHealthInsurancePlanCommand, OperationResult<bool>>, DeleteHealthInsurancePlanCommandHandler>();
        return services;
    }
}
