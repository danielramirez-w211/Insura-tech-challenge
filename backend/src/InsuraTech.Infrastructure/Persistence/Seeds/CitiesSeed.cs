using InsuraTech.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Seeds;

public static class CitiesSeed
{
    public static async Task SeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var collection = database.GetCollection<CityDocument>("cities");

        var count = await collection.CountDocumentsAsync(
            Builders<CityDocument>.Filter.Empty,
            cancellationToken: cancellationToken);

        if (count > 0)
            return;

        var cities = new List<CityDocument>
        {
            new() { Name = "Bogotá",         PostalCode = "110111", Department = "Cundinamarca"         },
            new() { Name = "Medellín",        PostalCode = "050001", Department = "Antioquia"            },
            new() { Name = "Cali",            PostalCode = "760001", Department = "Valle del Cauca"      },
            new() { Name = "Barranquilla",    PostalCode = "080001", Department = "Atlántico"            },
            new() { Name = "Cartagena",       PostalCode = "130001", Department = "Bolívar"              },
            new() { Name = "Cúcuta",          PostalCode = "540001", Department = "Norte de Santander"   },
            new() { Name = "Bucaramanga",     PostalCode = "680001", Department = "Santander"            },
            new() { Name = "Pereira",         PostalCode = "660001", Department = "Risaralda"            },
            new() { Name = "Santa Marta",     PostalCode = "470001", Department = "Magdalena"            },
            new() { Name = "Ibagué",          PostalCode = "730001", Department = "Tolima"               },
            new() { Name = "Manizales",       PostalCode = "170001", Department = "Caldas"               },
            new() { Name = "Pasto",           PostalCode = "520001", Department = "Nariño"               },
            new() { Name = "Neiva",           PostalCode = "410001", Department = "Huila"                },
            new() { Name = "Villavicencio",   PostalCode = "500001", Department = "Meta"                 },
            new() { Name = "Montería",        PostalCode = "230001", Department = "Córdoba"              },
            new() { Name = "Sincelejo",       PostalCode = "700001", Department = "Sucre"                },
            new() { Name = "Valledupar",      PostalCode = "200001", Department = "Cesar"                },
            new() { Name = "Armenia",         PostalCode = "630001", Department = "Quindío"              },
            new() { Name = "Popayán",         PostalCode = "190001", Department = "Cauca"                },
            new() { Name = "Tunja",           PostalCode = "150001", Department = "Boyacá"               },
        };

        await collection.InsertManyAsync(cities, cancellationToken: cancellationToken);
    }
}
