public class QuizFullUpdateServiceDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CategoryId { get; set; }
    public int TimeLimitMinutes { get; set; }
    public string Difficulty { get; set; } = "Easy";
    public List<QuestionUpdateServiceDto> Questions { get; set; } = new();
    public List<int> DeletedAnswerIds { get; set; } = new();
}