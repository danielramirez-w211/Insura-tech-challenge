using InsuraTech.Application.Cities.DTOs;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Cities.Queries.GetCities
{
    public sealed class GetCitiesHandler : IRequestHandler<GetCitiesQuery, IEnumerable<CityResponse>>
    {
        private readonly ICityRepository _cityRepository;

        public GetCitiesHandler(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<IEnumerable<CityResponse>> Handle(
            GetCitiesQuery request,
            CancellationToken cancellationToken)
        {
            var cities = await _cityRepository.GetAllAsync(cancellationToken);

            return cities.Select(c => new CityResponse(c.Name, c.PostalCode, c.Department));
        }
    }
}
