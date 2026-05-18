using MediatR;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Interface;

namespace SmartDelivery.Infrastructure.Handlers.Generiques
{
    public class PutGenericHandler<T> : IRequestHandler<PutGenericCommand<T>, T> where T : class
    {
        private readonly IGenericRepository<T> _repository;
        public PutGenericHandler(IGenericRepository<T> repository) => _repository = repository;

        public async Task<T> Handle(PutGenericCommand<T> requete, CancellationToken annulation)
            => await _repository.Modifier(requete.Entite);
    }
}
