public class QuestionUpdateServiceDto
{
    public int? Id { get; set; }
    public string Text { get; set; } = null!;
    public string QuestionType { get; set; } = "SingleChoice";
    public List<AnswerOptionUpdateServiceDto> AnswerOptions { get; set; } = new();
    public string? TextAnswer { get; set; }
}