using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.API.Persistence
{
    public class JobTypePersistence : IJobTypePersistence
    {
        private readonly HttpClient _httpClient;

        public JobTypePersistence(HttpClient httpClient, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(authHeader, out var authValue))
            {
                _httpClient.DefaultRequestHeaders.Authorization = authValue;
            }
        }

        public async Task<IEnumerable<JobTypeDto>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<IEnumerable<JobTypeDto>>>("api/internal/jobtypes");
            return response?.Data ?? Enumerable.Empty<JobTypeDto>();
        }

        public async Task<JobTypeDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<JobTypeDto>>($"api/internal/jobtypes/{id}");
            return response?.Data;
        }

        public async Task<bool> CreateAsync(CreateJobTypeDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/internal/jobtypes", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> BulkCreateAsync(BulkCreateJobTypeDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/internal/jobtypes/bulk", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, CreateJobTypeDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/internal/jobtypes/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/internal/jobtypes/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
