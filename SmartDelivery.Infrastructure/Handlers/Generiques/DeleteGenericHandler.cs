using MediatR;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Interface;

namespace SmartDelivery.Infrastructure.Handlers.Generiques
{
    public class DeleteGenericHandler<T> : IRequestHandler<DeleteGenericCommand<T>, bool> where T : class
    {
        private readonly IGenericRepository<T> _repository;
        public DeleteGenericHandler(IGenericRepository<T> repository) => _repository = repository;

        public async Task<bool> Handle(DeleteGenericCommand<T> requete, CancellationToken annulation)
            => await _repository.Supprimer(requete.Id);
    }
}
