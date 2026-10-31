using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Backend.Models.DeskApi;

namespace Backend.Clients;

public class DeskClient
{
   private readonly HttpClient _httpClient;
   private readonly string _baseUrl;
   private readonly string _apiKey;

   public DeskClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["Wifi2Ble:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl doesnt exist");
        _apiKey = configuration["Wifi2Ble:ApiKey"] ?? throw new ArgumentNullException("ApiKey doesnt exist");
    }

    public async Task<List<string>?> GetAllDeskIdsAsync()
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks";
        return await _httpClient.GetFromJsonAsync<List<string>>(url);
    }

    public async Task<DeskDetails?> GetDeskDetailsAsync(string deskId)
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks/{deskId}";
        return await _httpClient.GetFromJsonAsync<DeskDetails>(url);
    }

    public async Task<DeskState?> GetDeskStateAsync(string deskId)
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks/{deskId}/state";
        return await _httpClient.GetFromJsonAsync<DeskState>(url);
    }

    public async Task<bool> SetDeskPositionAsync(string deskId, int positionMm)
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks/{deskId}/state";
        var target_height = new DeskPosition { PositionMm = positionMm };

        var response = await _httpClient.PutAsJsonAsync(url, target_height);
        return response.IsSuccessStatusCode;
    }
}
