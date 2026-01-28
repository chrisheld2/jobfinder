using JobFinderApi.Models;
using System.Collections.Concurrent;

namespace JobFinderApi.Services;

public interface IJobStorageService
{
    Task AddJobsAsync(List<JobListing> jobs);
    Task<List<JobListing>> GetAllJobsAsync();
    Task<int> GetJobCountAsync();
    Task ClearJobsAsync();
}

public class JobStorageService : IJobStorageService
{
    private readonly ConcurrentDictionary<string, JobListing> _jobs = new();
    private readonly ILogger<JobStorageService> _logger;

    public JobStorageService(ILogger<JobStorageService> logger)
    {
        _logger = logger;
    }

    public Task AddJobsAsync(List<JobListing> jobs)
    {
        foreach (var job in jobs)
        {
            // Use URL as unique key to avoid duplicates
            var key = job.Url ?? Guid.NewGuid().ToString();
            _jobs.AddOrUpdate(key, job, (k, existing) => job);
        }

        _logger.LogInformation("Added {Count} jobs to storage. Total jobs: {Total}", jobs.Count, _jobs.Count);
        return Task.CompletedTask;
    }

    public Task<List<JobListing>> GetAllJobsAsync()
    {
        var jobs = _jobs.Values
            .OrderByDescending(j => j.PostedDate)
            .ToList();

        return Task.FromResult(jobs);
    }

    public Task<int> GetJobCountAsync()
    {
        return Task.FromResult(_jobs.Count);
    }

    public Task ClearJobsAsync()
    {
        _jobs.Clear();
        _logger.LogInformation("Cleared all jobs from storage");
        return Task.CompletedTask;
    }
}
