using System.Collections.Generic;

namespace DataViewer.Interfaces
{
    public interface ITranslationCloudAdapter
    {
        bool Translate(string text, Language source, Language target, out string translation);
        bool DetectLanguages(IList<string> texts, out IList<Language?> results);
        bool DetectLanguage(string text, out Language? result);
    }
}
