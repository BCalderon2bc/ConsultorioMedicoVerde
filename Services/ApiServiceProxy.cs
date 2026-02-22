using System.Net.Http.Json;
using System.Text.Json;

public class ApiServiceProxy
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiServiceProxy(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        // Leemos la URL base (ej: https://localhost:7261/api/)
        _baseUrl = configuration.GetSection("BackendSettings:BaseUrl").Value;
    }

    // MÉTODO MAESTRO DINÁMICO
    public async Task<T> SendRequestAsync<T>(string controller, string method, HttpMethod httpMethod, object data = null)
    {
        var url = $"{_baseUrl}{controller}/{method}";
        HttpResponseMessage response;

        if (httpMethod == HttpMethod.Get)   
        {
            response = await _httpClient.GetAsync(url);
        }
        else if (httpMethod == HttpMethod.Post)
        {
            response = await _httpClient.PostAsJsonAsync(url, data ?? new { });
        }
        else if (httpMethod == HttpMethod.Put)
        {
            response = await _httpClient.PutAsJsonAsync(url, data ?? new { });
        }
        else if (httpMethod == HttpMethod.Delete)
        {
            response = await _httpClient.DeleteAsync(url);
        }
        else
        {
            throw new NotImplementedException("Método HTTP no soportado.");
        }

        // Dentro de SendRequestAsync
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            // Esto ayuda a evitar conflictos con propiedades de solo lectura
            IgnoreReadOnlyProperties = false
        };

        if (response.IsSuccessStatusCode)
        {
            var datos = await response.Content.ReadFromJsonAsync<T>(options);
            return datos;
        }

        // Si da error (como el 405 que tenías), lanzamos una excepción detallada
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error {response.StatusCode}: {errorContent}");
    }
}