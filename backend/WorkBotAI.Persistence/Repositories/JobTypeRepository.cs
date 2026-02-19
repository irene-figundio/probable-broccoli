using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WorkBotAI.Persistence.Repositories
{
    public class JobTypeRepository : IJobTypeRepository
    {
        private readonly HttpClient _httpClient;

        public JobTypeRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(authHeader, out var authValue))
            {
                _httpClient.DefaultRequestHeaders.Authorization = authValue;
            }
        }

        public async Task<IEnumerable<JobType>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<IEnumerable<JobType>>>("api/JobTypes");
            return response?.Data ?? new List<JobType>();
        }

        public async Task<JobType?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<JobType>>($"api/JobTypes/{id}");
            return response?.Data;
        }

        public async Task<JobType> CreateAsync(JobType jobType)
        {
            var response = await _httpClient.PostAsJsonAsync("api/JobTypes", jobType);
            var result = await response.Content.ReadFromJsonAsync<ServiceResponse<JobType>>();
            return result?.Data ?? jobType;
        }

        public async Task UpdateAsync(JobType jobType)
        {
            await _httpClient.PutAsJsonAsync($"api/JobTypes/{jobType.Id}", jobType);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/JobTypes/{id}");
        }

        public async Task CreateRangeAsync(IEnumerable<JobType> jobTypes)
        {
            await _httpClient.PostAsJsonAsync("api/JobTypes/bulk-range", jobTypes);
        }
    }
}
