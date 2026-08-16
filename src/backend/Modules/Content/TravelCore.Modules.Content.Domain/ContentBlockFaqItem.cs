namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Ordered FAQ Q/A row inside an Faq block (relational; not JSONB — P08-R2).
/// </summary>
public sealed class ContentBlockFaqItem
{
    public const int QuestionMaxLength = 500;
    public const int AnswerMaxLength = 4000;

    private ContentBlockFaqItem()
    {
        Question = null!;
        Answer = null!;
    }

    private ContentBlockFaqItem(
        ContentBlockId blockId,
        string question,
        string answer,
        int sortOrder)
    {
        BlockId = blockId;
        Question = question;
        Answer = answer;
        SortOrder = sortOrder;
    }

    public ContentBlockId BlockId { get; private set; }

    public string Question { get; private set; }

    public string Answer { get; private set; }

    public int SortOrder { get; private set; }

    internal static ContentBlockFaqItem Create(
        ContentBlockId blockId,
        string question,
        string answer,
        int sortOrder)
    {
        if (blockId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentBlockId cannot be empty.", nameof(blockId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        return new ContentBlockFaqItem(
            blockId,
            NormalizeQuestion(question),
            NormalizeAnswer(answer),
            sortOrder);
    }

    internal void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        SortOrder = sortOrder;
    }

    private static string NormalizeQuestion(string question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        var trimmed = question.Trim();
        if (trimmed.Length > QuestionMaxLength)
        {
            throw new ArgumentException($"FAQ question max length is {QuestionMaxLength}.", nameof(question));
        }

        return trimmed;
    }

    private static string NormalizeAnswer(string answer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);
        var trimmed = answer.Trim();
        if (trimmed.Length > AnswerMaxLength)
        {
            throw new ArgumentException($"FAQ answer max length is {AnswerMaxLength}.", nameof(answer));
        }

        return trimmed;
    }
}
