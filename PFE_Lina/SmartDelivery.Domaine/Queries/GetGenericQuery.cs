using MediatR;

namespace SmartDelivery.Domaine.Queries
{
    public class GetGenericQuery<T> : IRequest<T?> where T : class
    {
        public int Id { get; set; }
        public GetGenericQuery(int id) => Id = id;
    }
}
