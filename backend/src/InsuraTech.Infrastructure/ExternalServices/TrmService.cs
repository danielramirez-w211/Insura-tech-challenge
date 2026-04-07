using System.Globalization;
using System.Text.Json;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace InsuraTech.Infrastructure.ExternalServices;

public sealed class TrmService : ITrmService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private const string CacheKey  = "trm:current";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private const string ApiUrl =
        "https://www.datos.gov.co/resource/mcec-87by.json?$limit=1&$order=vigenciadesde%20DESC";

    public TrmService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache      = cache;
    }

    public async Task<TrmResult> GetCurrentTrmAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out TrmResult? cached) && cached is not null)
            return cached;

        try
        {
            using var response = await _httpClient.GetAsync(ApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var items   = JsonSerializer.Deserialize<TrmApiItem[]>(content, JsonOptions);

            if (items is null || items.Length == 0)
                throw new TrmUnavailableException();

            var item = items[0];

            // Normalizar separadores: la API puede retornar "4.215,24" o "4215.24"
            //var valorNorm = item.valor.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(item.valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var trm)
                || trm <= 0)
                throw new TrmUnavailableException();

            var date   = DateOnly.Parse(item.vigenciadesde[..10]);
            var result = new TrmResult(trm, date);

            _cache.Set(CacheKey, result, CacheTtl);
            return result;
        }
        catch (TrmUnavailableException)
        {
            throw;
        }
        catch
        {
            throw new TrmUnavailableException();
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed class TrmApiItem
    {
        public string valor          { get; set; } = null!;
        public string vigenciadesde  { get; set; } = null!;
    }
}
