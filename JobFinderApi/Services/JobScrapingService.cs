using JobFinderApi.Models;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using System.Text.Json;

namespace JobFinderApi.Services;

public interface IJobScrapingService
{
    Task<List<JobListing>> SearchJobsAsync(string source, CancellationToken cancellationToken = default);
}

public class JobScrapingService : IJobScrapingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JobScrapingService> _logger;
    private readonly IConfiguration _configuration;

    public JobScrapingService(HttpClient httpClient, ILogger<JobScrapingService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<JobListing>> SearchJobsAsync(string source, CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        try
        {
            // Use Claude API to search for jobs
            jobs.AddRange(await SearchWithClaudeAsync(source, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs from {Source}", source);
        }

        return jobs;
    }

    private async Task<List<JobListing>> SearchWithClaudeAsync(string source, CancellationToken cancellationToken)
    {
        var jobs = new List<JobListing>();

        try
        {
            var apiKey = _configuration["ANTHROPIC_API_KEY"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("ANTHROPIC_API_KEY not found, returning sample jobs");
                return GetSampleJobs();
            }

            var client = new AnthropicClient(apiKey);

            var sourceText = source.Equals("all", StringComparison.OrdinalIgnoreCase)
                ? "Indeed and Monster"
                : source;

            var prompt = $@"Search the web for full-stack developer job listings on {sourceText} that meet ALL these criteria:
1. Require C# or .NET experience
2. Require Azure cloud experience
3. Require an active U.S. security clearance (Secret, Top Secret, TS/SCI, etc.)

For each job you find, return a JSON array with this exact structure:
[
  {{
    ""title"": ""job title"",
    ""company"": ""company name"",
    ""location"": ""city, state or Remote"",
    ""description"": ""brief job description focusing on requirements"",
    ""url"": ""full job posting URL"",
    ""source"": ""Indeed"" or ""Monster""
  }}
]

Find 5-10 real job postings that match ALL criteria. Return ONLY the JSON array, no other text.";

            _logger.LogInformation("Sending request to Claude API for source: {Source}", source);

            var messages = new List<Message>
            {
                new Message(RoleType.User, prompt)
            };

            var parameters = new MessageParameters
            {
                Messages = messages,
                MaxTokens = 4096,
                Model = "claude-3-5-sonnet-20241022",
                Stream = false,
                Temperature = 1.0m
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            if (response?.Content?.Count > 0 && response.Content[0] is Anthropic.SDK.Messaging.TextContent textContent)
            {
                var jsonText = textContent.Text;
                _logger.LogInformation("Claude API response received: {Length} characters", jsonText.Length);

                // Extract JSON array from response (Claude might include markdown formatting)
                var jsonStart = jsonText.IndexOf('[');
                var jsonEnd = jsonText.LastIndexOf(']');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    jsonText = jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }

                var jobData = JsonSerializer.Deserialize<List<ClaudeJobResult>>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jobData != null)
                {
                    foreach (var job in jobData)
                    {
                        jobs.Add(new JobListing
                        {
                            Title = job.Title ?? "Unknown Title",
                            Company = job.Company ?? "Unknown Company",
                            Location = job.Location ?? "Remote",
                            Description = job.Description ?? "",
                            Url = job.Url ?? "https://www.indeed.com",
                            Source = job.Source ?? source,
                            PostedDate = DateTime.UtcNow,
                            RequiresSecurityClearance = true
                        });
                    }

                    _logger.LogInformation("Claude API: Parsed {Count} jobs", jobs.Count);
                }
            }
            else
            {
                _logger.LogWarning("No content received from Claude API");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using Claude API to search jobs");
            // Return sample jobs as fallback
            return GetSampleJobs();
        }

        return jobs;
    }

    private List<JobListing> GetSampleJobs()
    {
        return new List<JobListing>
        {
            new JobListing
            {
                Title = "Senior Full-Stack Developer - TS/SCI Required",
                Company = "Northrop Grumman",
                Location = "McLean, VA",
                Description = "Seeking experienced full-stack developer with C#, .NET Core, Azure, and active TS/SCI clearance. Build mission-critical applications.",
                Url = "https://www.indeed.com/jobs",
                Source = "Indeed",
                PostedDate = DateTime.UtcNow.AddDays(-2),
                RequiresSecurityClearance = true
            },
            new JobListing
            {
                Title = "Full-Stack Software Engineer - Secret Clearance",
                Company = "Booz Allen Hamilton",
                Location = "Washington, DC",
                Description = "Full-stack developer role requiring C#, Azure DevOps, and active Secret clearance. Support government clients.",
                Url = "https://www.indeed.com/jobs",
                Source = "Indeed",
                PostedDate = DateTime.UtcNow.AddDays(-1),
                RequiresSecurityClearance = true
            },
            new JobListing
            {
                Title = "Azure Full-Stack Developer - Clearance Required",
                Company = "Leidos",
                Location = "Reston, VA",
                Description = "Develop cloud solutions using C#, .NET, Azure. Must have or be able to obtain Secret clearance.",
                Url = "https://www.monster.com/jobs",
                Source = "Monster",
                PostedDate = DateTime.UtcNow.AddDays(-3),
                RequiresSecurityClearance = true
            }
        };
    }

    private class ClaudeJobResult
    {
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Source { get; set; }
    }
}
