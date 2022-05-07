namespace DocGenerator
{
    public interface IObjectBuilderService
    {
        GeneratedResult GenerateResult(SyntaxContext context);
    }
}