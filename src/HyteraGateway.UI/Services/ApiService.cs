using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HyteraGateway.UI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint);
        }
        catch
        {
            return default;
        }
    }

    public async Task PostAsync<T>(string endpoint, T data)
    {
        try
        {
            await _httpClient.PostAsJsonAsync(endpoint, data);
        }
        catch
        {
            // Silently fail if server is not available
        }
    }
}
