using MediatR;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Queries;

namespace SmartDelivery.Infrastructure.Handlers.Generiques
{
    public class GetListGenericHandler<T> : IRequestHandler<GetListGenericQuery<T>, IEnumerable<T>> where T : class
    {
        private readonly IGenericRepository<T> _repository;
        public GetListGenericHandler(IGenericRepository<T> repository) => _repository = repository;

        public async Task<IEnumerable<T>> Handle(GetListGenericQuery<T> requete, CancellationToken annulation)
            => await _repository.ObtenirTous();
    }
}
