namespace Common.Models;

public class FileAnalysisResult
{
    public Guid Id { get; set; }
    public string Location { get; set; }
    public int WordCount { get; set; }
    public int ParagraphCount { get; set; }
    public int CharacterCount { get; set; }
    public string WordCloudImageLocation { get; set; }
} 