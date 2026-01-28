using HtmlAgilityPack;
using JobFinderApi.Models;

namespace JobFinderApi.Services;

public interface IJobScrapingService
{
    Task<List<JobListing>> SearchJobsAsync(string source, CancellationToken cancellationToken = default);
}

public class JobScrapingService : IJobScrapingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JobScrapingService> _logger;

    public JobScrapingService(HttpClient httpClient, ILogger<JobScrapingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<JobListing>> SearchJobsAsync(string source, CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        try
        {
            if (source.Equals("indeed", StringComparison.OrdinalIgnoreCase))
            {
                jobs.AddRange(await SearchIndeedAsync(cancellationToken));
            }
            else if (source.Equals("monster", StringComparison.OrdinalIgnoreCase))
            {
                jobs.AddRange(await SearchMonsterAsync(cancellationToken));
            }
            else if (source.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                jobs.AddRange(await SearchIndeedAsync(cancellationToken));
                jobs.AddRange(await SearchMonsterAsync(cancellationToken));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs from {Source}", source);
        }

        return jobs;
    }

    private async Task<List<JobListing>> SearchIndeedAsync(CancellationToken cancellationToken)
    {
        var jobs = new List<JobListing>();

        try
        {
            var searchTerms = "full-stack developer C# Azure security clearance";
            var url = $"https://www.indeed.com/jobs?q={Uri.EscapeDataString(searchTerms)}&l=";

            _logger.LogInformation("Searching Indeed with URL: {Url}", url);

            // Create request with browser-like headers
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Simulate a real browser by adding common headers
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.indeed.com/");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");
            request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            _logger.LogInformation("Indeed response status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Indeed returned status {StatusCode}", response.StatusCode);
                return jobs;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Indeed returned {Length} characters", content.Length);

            // Save HTML to file for debugging
            var debugPath = Path.Combine(Path.GetTempPath(), "indeed_response.html");
            await File.WriteAllTextAsync(debugPath, content, cancellationToken);
            _logger.LogInformation("Indeed: Saved HTML to {Path}", debugPath);

            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            // Try multiple selectors since Indeed's HTML structure changes
            var jobCards = doc.DocumentNode.SelectNodes("//div[contains(@class, 'job_seen_beacon')]") ??
                          doc.DocumentNode.SelectNodes("//div[@class='resultContent']") ??
                          doc.DocumentNode.SelectNodes("//div[contains(@class, 'jobsearch-ResultsList')]//div[contains(@class, 'result')]") ??
                          doc.DocumentNode.SelectNodes("//div[contains(@class, 'job')]") ??
                          doc.DocumentNode.SelectNodes("//li[contains(@class, 'result')]") ??
                          doc.DocumentNode.SelectNodes("//article") ??
                          doc.DocumentNode.SelectNodes("//*[@data-testid='job-card']");

            _logger.LogInformation("Indeed: Found {Count} job cards", jobCards?.Count ?? 0);

            if (jobCards != null)
            {
                foreach (var card in jobCards.Take(20))
                {
                    try
                    {
                        // Try multiple selectors for title
                        var titleNode = card.SelectSingleNode(".//h2[contains(@class, 'jobTitle')]//span[@title]") ??
                                       card.SelectSingleNode(".//h2//a") ??
                                       card.SelectSingleNode(".//a[contains(@class, 'jcs-JobTitle')]");

                        var companyNode = card.SelectSingleNode(".//span[@data-testid='company-name']") ??
                                         card.SelectSingleNode(".//span[contains(@class, 'companyName')]");

                        var locationNode = card.SelectSingleNode(".//div[@data-testid='text-location']") ??
                                          card.SelectSingleNode(".//div[contains(@class, 'companyLocation')]");

                        var descriptionNode = card.SelectSingleNode(".//div[contains(@class, 'job-snippet')]") ??
                                             card.SelectSingleNode(".//div[@class='summary']");

                        var linkNode = card.SelectSingleNode(".//h2//a") ??
                                      card.SelectSingleNode(".//a[contains(@class, 'jcs-JobTitle')]");

                        if (titleNode != null && companyNode != null)
                        {
                            var title = titleNode.GetAttributeValue("title", titleNode.InnerText).Trim();
                            var company = companyNode.InnerText.Trim();
                            var location = locationNode?.InnerText.Trim() ?? "Remote";
                            var description = descriptionNode?.InnerText.Trim() ?? "";
                            var jobUrl = linkNode?.GetAttributeValue("href", "");

                            var fullText = $"{title} {description}";
                            var requiresClearance = ContainsSecurityClearanceKeyword(fullText);
                            var hasCSharp = fullText.Contains("C#", StringComparison.OrdinalIgnoreCase) ||
                                          fullText.Contains(".NET", StringComparison.OrdinalIgnoreCase);
                            var hasAzure = fullText.Contains("Azure", StringComparison.OrdinalIgnoreCase);
                            var isFullStack = fullText.Contains("full", StringComparison.OrdinalIgnoreCase) &&
                                            fullText.Contains("stack", StringComparison.OrdinalIgnoreCase);

                            _logger.LogInformation("Indeed job: {Title} - FullStack:{FS} C#:{CS} Azure:{Az} Clearance:{Cl}",
                                title, isFullStack, hasCSharp, hasAzure, requiresClearance);

                            if (isFullStack && (hasCSharp || hasAzure) && requiresClearance)
                            {
                                jobs.Add(new JobListing
                                {
                                    Title = title,
                                    Company = company,
                                    Location = location,
                                    Description = description,
                                    Url = jobUrl.StartsWith("http") ? jobUrl : $"https://www.indeed.com{jobUrl}",
                                    Source = "Indeed",
                                    PostedDate = DateTime.UtcNow,
                                    RequiresSecurityClearance = true
                                });
                                _logger.LogInformation("Added Indeed job: {Title}", title);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing Indeed job card");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Indeed");
        }

        _logger.LogInformation("Indeed: Returning {Count} jobs", jobs.Count);
        return jobs;
    }

    private async Task<List<JobListing>> SearchMonsterAsync(CancellationToken cancellationToken)
    {
        var jobs = new List<JobListing>();

        try
        {
            // Add delay to simulate human browsing (1-3 seconds)
            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1000, 3000)), cancellationToken);

            var searchTerms = "full-stack developer C# Azure security clearance";
            var url = $"https://www.monster.com/jobs/search/?q={Uri.EscapeDataString(searchTerms)}";

            _logger.LogInformation("Searching Monster with URL: {Url}", url);

            // Create request with browser-like headers
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Simulate a real browser by adding common headers
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.monster.com/");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");
            request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            _logger.LogInformation("Monster response status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Monster returned status {StatusCode}", response.StatusCode);
                return jobs;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Monster returned {Length} characters", content.Length);

            // Save HTML to file for debugging
            var debugPathMonster = Path.Combine(Path.GetTempPath(), "monster_response.html");
            await File.WriteAllTextAsync(debugPathMonster, content, cancellationToken);
            _logger.LogInformation("Monster: Saved HTML to {Path}", debugPathMonster);

            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            // Try multiple selectors
            var jobCards = doc.DocumentNode.SelectNodes("//section[contains(@class, 'card-content')]") ??
                          doc.DocumentNode.SelectNodes("//div[contains(@class, 'job-card')]") ??
                          doc.DocumentNode.SelectNodes("//article") ??
                          doc.DocumentNode.SelectNodes("//*[@data-testid='job-card']") ??
                          doc.DocumentNode.SelectNodes("//div[contains(@class, 'card')]");

            _logger.LogInformation("Monster: Found {Count} job cards", jobCards?.Count ?? 0);

            if (jobCards != null)
            {
                foreach (var card in jobCards.Take(20))
                {
                    try
                    {
                        var titleNode = card.SelectSingleNode(".//h2//a") ??
                                       card.SelectSingleNode(".//a[contains(@class, 'title')]");

                        var companyNode = card.SelectSingleNode(".//div[contains(@class, 'company')]//a") ??
                                         card.SelectSingleNode(".//span[contains(@class, 'company')]");

                        var locationNode = card.SelectSingleNode(".//div[contains(@class, 'location')]");

                        var descriptionNode = card.SelectSingleNode(".//div[contains(@class, 'summary')]") ??
                                             card.SelectSingleNode(".//div[contains(@class, 'description')]");

                        var linkNode = card.SelectSingleNode(".//h2//a");

                        if (titleNode != null && companyNode != null)
                        {
                            var title = titleNode.InnerText.Trim();
                            var company = companyNode.InnerText.Trim();
                            var location = locationNode?.InnerText.Trim() ?? "Remote";
                            var description = descriptionNode?.InnerText.Trim() ?? "";
                            var jobUrl = linkNode?.GetAttributeValue("href", "");

                            var fullText = $"{title} {description}";
                            var requiresClearance = ContainsSecurityClearanceKeyword(fullText);
                            var hasCSharp = fullText.Contains("C#", StringComparison.OrdinalIgnoreCase) ||
                                          fullText.Contains(".NET", StringComparison.OrdinalIgnoreCase);
                            var hasAzure = fullText.Contains("Azure", StringComparison.OrdinalIgnoreCase);
                            var isFullStack = fullText.Contains("full", StringComparison.OrdinalIgnoreCase) &&
                                            fullText.Contains("stack", StringComparison.OrdinalIgnoreCase);

                            _logger.LogInformation("Monster job: {Title} - FullStack:{FS} C#:{CS} Azure:{Az} Clearance:{Cl}",
                                title, isFullStack, hasCSharp, hasAzure, requiresClearance);

                            if (isFullStack && (hasCSharp || hasAzure) && requiresClearance)
                            {
                                jobs.Add(new JobListing
                                {
                                    Title = title,
                                    Company = company,
                                    Location = location,
                                    Description = description,
                                    Url = jobUrl.StartsWith("http") ? jobUrl : $"https://www.monster.com{jobUrl}",
                                    Source = "Monster",
                                    PostedDate = DateTime.UtcNow,
                                    RequiresSecurityClearance = true
                                });
                                _logger.LogInformation("Added Monster job: {Title}", title);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing Monster job card");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Monster");
        }

        _logger.LogInformation("Monster: Returning {Count} jobs", jobs.Count);
        return jobs;
    }

    private bool ContainsSecurityClearanceKeyword(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var clearanceKeywords = new[]
        {
            "security clearance",
            "secret clearance",
            "top secret",
            "ts/sci",
            "tssci",
            "TS/SCI",
            "clearance required",
            "must have clearance",
            "active clearance",
            "government security clearance",
            "dod clearance",
            "public trust",
            "clearable"
        };

        foreach (var keyword in clearanceKeywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
