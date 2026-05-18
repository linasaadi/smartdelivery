using MediatR;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Domaine.Queries
{
    public record GetKpiAdminQuery : IRequest<KpiAdminDto>;

    public record GetKpiDispatcherQuery : IRequest<KpiDispatcherDto>;
}
