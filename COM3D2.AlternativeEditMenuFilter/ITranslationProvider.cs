namespace COM3D2.AlternativeEditMenuFilter
{
    public interface ITranslationResult
    {
        string OriginalText { get; }
        string TranslatedText { get; }
        bool IsTranslationSuccessful { get; }
    }

    public interface ITranslationAsyncResult : ITranslationResult
    {
        bool IsReady { get; }
    }

    public interface ITranslationProvider
    {
        ITranslationResult Translate(string text);

        ITranslationAsyncResult TranslateAsync(string text);

        void ResetAsyncQueue();
    }
}