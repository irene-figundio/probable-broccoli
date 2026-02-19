using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.API.Persistence
{
    public class CategoryPersistence : ICategoryPersistence
    {
        private readonly HttpClient _httpClient;

        public CategoryPersistence(HttpClient httpClient, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(authHeader, out var authValue))
            {
                _httpClient.DefaultRequestHeaders.Authorization = authValue;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<IEnumerable<CategoryDto>>>("api/internal/categories");
            return response?.Data ?? Enumerable.Empty<CategoryDto>();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<CategoryDto>>($"api/internal/categories/{id}");
            return response?.Data;
        }

        public async Task<bool> CreateAsync(CreateCategoryDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/internal/categories", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, CategoryDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/internal/categories/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/internal/categories/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
