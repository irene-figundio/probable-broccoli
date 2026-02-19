namespace WorkBotAI.API.DTOs;

public class JobTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? CategoryId { get; set; }
    public string Gender { get; set; } = "U";
}

public class CreateJobTypeDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? CategoryId { get; set; }
    public string Gender { get; set; } = "U";
}

public class BulkCreateJobTypeDto
{
    public string Descrizione { get; set; } = string.Empty;
    public bool M { get; set; }
    public bool F { get; set; }
    public bool U { get; set; }
    public int? CategoryId { get; set; }
    public int UserId { get; set; }
}
