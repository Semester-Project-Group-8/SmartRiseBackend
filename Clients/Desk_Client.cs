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
        try 
        {
            return await _httpClient.GetFromJsonAsync<List<string>>(url);
        }
        catch (Exception ex)
        {
            throw new Exception("Error while getting desk ids", ex);
        }

    }

    public async Task<DeskDetails?> GetDeskDetailsAsync(string deskId)
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks/{deskId}";
        try 
        {
            return await _httpClient.GetFromJsonAsync<DeskDetails>(url);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while getting desk details for deskId: {deskId}", ex);
        }
    }

    public async Task<DeskState?> GetDeskStateAsync(string deskId)
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks/{deskId}/state";
        try
        {
            return await _httpClient.GetFromJsonAsync<DeskState>(url);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while getting desk state for deskId: {deskId}", ex);
        }
    }

    public async Task<bool> SetDeskPositionAsync(string deskId, int positionMm)
    {
        var url = $"{_baseUrl}api/v2/{_apiKey}/desks/{deskId}/state";
        var target_height = new DeskPosition { PositionMm = positionMm };

        var response = await _httpClient.PutAsJsonAsync(url, target_height);
        try
        {
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while setting desk position for deskId: {deskId}", ex);
        }
    }
}
