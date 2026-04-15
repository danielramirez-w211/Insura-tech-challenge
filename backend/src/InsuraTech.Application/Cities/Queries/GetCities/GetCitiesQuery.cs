using InsuraTech.Application.Cities.DTOs;
using MediatR;

namespace InsuraTech.Application.Cities.Queries.GetCities
{
    public sealed record GetCitiesQuery : IRequest<IEnumerable<CityResponse>>;
}
