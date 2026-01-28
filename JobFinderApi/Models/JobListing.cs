namespace JobFinderApi.Models;

public class JobListing
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // Indeed or Monster
    public DateTime PostedDate { get; set; }
    public bool RequiresSecurityClearance { get; set; }
}
