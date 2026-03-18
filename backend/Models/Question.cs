namespace K53PrepApp.Models;

public class Question
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;   // "Rules" | "Signs" | "Controls"
    public string SubCategory { get; set; } = string.Empty; // e.g. "Regulatory Signs"
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = string.Empty; // "A" | "B" | "C" | "D"
    public string Explanation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
