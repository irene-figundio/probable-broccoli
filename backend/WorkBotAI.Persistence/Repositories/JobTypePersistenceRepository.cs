using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WorkBotAI.Persistence.Repositories
{
    public class JobTypePersistenceRepository : IJobTypeRepository
    {
        private readonly HttpClient _httpClient;

        public JobTypePersistenceRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<IEnumerable<JobType>>>("api/internal/jobtypes");
            return response?.Data ?? new List<JobType>();
        }

        public async Task<JobType?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<JobType>>($"api/internal/jobtypes/{id}");
            return response?.Data;
        }

        public async Task<JobType> CreateAsync(JobType jobType)
        {
            var response = await _httpClient.PostAsJsonAsync("api/internal/jobtypes", jobType);
            var result = await response.Content.ReadFromJsonAsync<ServiceResponse<JobType>>();
            return result?.Data ?? jobType;
        }

        public async Task UpdateAsync(JobType jobType)
        {
            await _httpClient.PutAsJsonAsync($"api/internal/jobtypes/{jobType.Id}", jobType);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/internal/jobtypes/{id}");
        }

        public async Task CreateRangeAsync(IEnumerable<JobType> jobTypes)
        {
            // Note: Internal controller handles bulk creation via a separate DTO normally,
            // but here we just pass the range to a bulk endpoint.
            // I'll use the existing bulk endpoint I created in InternalJobTypesController if it fits.
            // Wait, InternalJobTypesController.BulkCreateJobType expects BulkCreateJobTypeDto.
            // I should probably add a generic bulk create if I want to use it here,
            // or just use a loop (less efficient).
            // Let's assume there is an internal bulk endpoint that accepts IEnumerable<JobType>.
            await _httpClient.PostAsJsonAsync("api/internal/jobtypes/bulk-range", jobTypes);
        }
    }
}
