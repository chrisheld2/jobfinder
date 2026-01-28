using JobFinderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpClient<IJobScrapingService, JobScrapingService>()
    .ConfigureHttpClient(client =>
    {
        // Set timeout - headers are now set per-request in the service for better control
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5
        };
    });

// Add job storage service as singleton to persist data across requests
builder.Services.AddSingleton<IJobStorageService, JobStorageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddLogging();

var app = builder.Build();

app.UseStaticFiles();
app.UseCors("AllowAll");

// API Routes
var api = app.MapGroup("/api/jobs");

api.MapGet("/search/{source}", SearchJobs)
    .WithName("SearchJobs")
    .WithDescription("Search for jobs from a specific source (indeed, monster, or all)");

api.MapGet("/", GetAllCachedJobs)
    .WithName("GetAllJobs")
    .WithDescription("Get all cached jobs");

api.MapPost("/", AddJobs)
    .WithName("AddJobs")
    .WithDescription("Add jobs from Chrome extension");

// Mock data endpoint for testing
api.MapGet("/mock", GetMockJobs)
    .WithName("GetMockJobs")
    .WithDescription("Get mock job data for testing the UI");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Fallback to index.html for SPA
app.MapFallback(async context =>
{
    var path = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (File.Exists(path))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(path);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }
});

await app.RunAsync();

async Task<IResult> SearchJobs(
    string source,
    IJobScrapingService jobScrapingService,
    ILogger<Program> logger)
{
    logger.LogInformation("Searching jobs from source: {Source}", source);

    var validSources = new[] { "indeed", "monster", "all" };
    if (!validSources.Contains(source, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { error = "Invalid source. Use 'indeed', 'monster', or 'all'" });
    }

    var jobs = await jobScrapingService.SearchJobsAsync(source);
    return Results.Ok(new { jobs, count = jobs.Count, timestamp = DateTime.UtcNow });
}

async Task<IResult> GetAllCachedJobs(
    IJobStorageService jobStorageService,
    ILogger<Program> logger)
{
    logger.LogInformation("Fetching all stored jobs");
    var jobs = await jobStorageService.GetAllJobsAsync();
    return Results.Ok(new { jobs, count = jobs.Count, timestamp = DateTime.UtcNow });
}

async Task<IResult> AddJobs(
    List<JobFinderApi.Models.JobListing> jobs,
    IJobStorageService jobStorageService,
    ILogger<Program> logger)
{
    logger.LogInformation("Received {Count} jobs from Chrome extension", jobs.Count);

    await jobStorageService.AddJobsAsync(jobs);

    var totalJobs = await jobStorageService.GetJobCountAsync();

    return Results.Ok(new
    {
        success = true,
        added = jobs.Count,
        total = totalJobs,
        timestamp = DateTime.UtcNow
    });
}

IResult GetMockJobs(ILogger<Program> logger)
{
    logger.LogInformation("Returning mock job data");

    var mockJobs = new[]
    {
        new JobFinderApi.Models.JobListing
        {
            Title = "Senior Full-Stack Developer - Secret Clearance Required",
            Company = "Defense Tech Solutions",
            Location = "Arlington, VA (Remote)",
            Description = "Seeking experienced full-stack developer with C# and Azure expertise. Must hold active Secret clearance. Work on cutting-edge defense applications using .NET 8, Azure DevOps, and modern frontend frameworks.",
            Url = "https://example.com/job1",
            Source = "Indeed",
            PostedDate = DateTime.UtcNow.AddDays(-2),
            RequiresSecurityClearance = true
        },
        new JobFinderApi.Models.JobListing
        {
            Title = "Cloud Application Developer (TS/SCI)",
            Company = "Federal Systems Inc",
            Location = "McLean, VA",
            Description = "Build and maintain Azure-based applications for federal clients. Required: C#, ASP.NET Core, Azure Cloud Services, Kubernetes. TS/SCI clearance required.",
            Url = "https://example.com/job2",
            Source = "Indeed",
            PostedDate = DateTime.UtcNow.AddDays(-5),
            RequiresSecurityClearance = true
        },
        new JobFinderApi.Models.JobListing
        {
            Title = "Full Stack Software Engineer - Azure",
            Company = "Cyber Defense Corp",
            Location = "Remote",
            Description = "Join our team building next-generation cybersecurity platforms. Tech stack: C#, Azure Functions, React, SQL Server. Active security clearance required.",
            Url = "https://example.com/job3",
            Source = "Monster",
            PostedDate = DateTime.UtcNow.AddDays(-1),
            RequiresSecurityClearance = true
        },
        new JobFinderApi.Models.JobListing
        {
            Title = ".NET Developer with Azure Experience",
            Company = "Intelligence Solutions LLC",
            Location = "Washington, DC",
            Description = "Develop mission-critical applications for intelligence community. C#, Azure DevOps, microservices architecture. Secret clearance minimum, TS/SCI preferred.",
            Url = "https://example.com/job4",
            Source = "Monster",
            PostedDate = DateTime.UtcNow.AddDays(-7),
            RequiresSecurityClearance = true
        },
        new JobFinderApi.Models.JobListing
        {
            Title = "Principal Software Engineer - DOD Clearance",
            Company = "Aerospace Technologies",
            Location = "Colorado Springs, CO (Hybrid)",
            Description = "Lead development of cloud-native defense systems. Expertise in C#, Azure Kubernetes Service, and secure DevSecOps practices required. Active DOD clearance mandatory.",
            Url = "https://example.com/job5",
            Source = "Indeed",
            PostedDate = DateTime.UtcNow.AddDays(-3),
            RequiresSecurityClearance = true
        }
    };

    return Results.Ok(new { jobs = mockJobs, count = mockJobs.Length, timestamp = DateTime.UtcNow });
}
