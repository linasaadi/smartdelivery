using MediatR;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Queries;

namespace SmartDelivery.Infrastructure.Handlers.Generiques
{
    public class GetGenericHandler<T> : IRequestHandler<GetGenericQuery<T>, T?> where T : class
    {
        private readonly IGenericRepository<T> _repository;
        public GetGenericHandler(IGenericRepository<T> repository) => _repository = repository;

        public async Task<T?> Handle(GetGenericQuery<T> requete, CancellationToken annulation)
            => await _repository.ObtenirParId(requete.Id);
    }
}
