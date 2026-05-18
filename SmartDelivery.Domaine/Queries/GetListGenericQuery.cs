using MediatR;

namespace SmartDelivery.Domaine.Queries
{
    public class GetListGenericQuery<T> : IRequest<IEnumerable<T>> where T : class { }
}
