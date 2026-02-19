using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WorkBotAI.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly HttpClient _httpClient;

        public CategoryRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(authHeader, out var authValue))
            {
                _httpClient.DefaultRequestHeaders.Authorization = authValue;
            }
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<IEnumerable<Category>>>("api/Categories");
            return response?.Data ?? new List<Category>();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<Category>>($"api/Categories/{id}");
            return response?.Data;
        }

        public async Task<Category> CreateAsync(Category category)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Categories", category);
            var result = await response.Content.ReadFromJsonAsync<ServiceResponse<Category>>();
            return result?.Data ?? category;
        }

        public async Task UpdateAsync(Category category)
        {
            await _httpClient.PutAsJsonAsync($"api/Categories/{category.Id}", category);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/Categories/{id}");
        }
    }
}
