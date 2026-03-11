using ConsultorioMedicoVerde.Models;
using ConsultorioVerde.Web.Models;
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
            response = await _httpClient.GetAsync(url);
        else if (httpMethod == HttpMethod.Post)
            response = await _httpClient.PostAsJsonAsync(url, data ?? new { });
        else if (httpMethod == HttpMethod.Put)
            response = await _httpClient.PutAsJsonAsync(url, data ?? new { });
        else if (httpMethod == HttpMethod.Delete)
            response = await _httpClient.DeleteAsync(url);
        else
            throw new NotImplementedException("Método HTTP no soportado.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>(options);
        }

        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            var error = JsonSerializer.Deserialize<ResponseGeneric<object>>(errorContent, options);

            if (error != null && !string.IsNullOrEmpty(error.Mensaje))
            {
                throw new Exception(error.Mensaje);
            }
        }
        catch
        {
            // Si no viene en formato ResponseGeneric
        }

        throw new Exception(errorContent);
    }

    public async Task<byte[]> GetByteArrayAsync(string controller, string method)
    {
        var url = $"{_baseUrl}{controller}/{method}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

}