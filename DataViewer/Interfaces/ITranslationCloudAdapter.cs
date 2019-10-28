using System.Collections.Generic;

namespace DataViewer.Interfaces
{
    public interface ITranslationCloudAdapter
    {
        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// </summary>
        bool Translate(string text, Language source, Language target, out string translation);
        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// </summary>
        bool DetectLanguages(IList<string> texts, out IList<Language?> results);
        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// Do not use it multiple times at once (for example in a loop) as it is inefficient both in terms of money and time. 
        /// If you have a set of data to recognize, use <see cref="DetectLanguages"/> instead.
        /// </summary>
        bool DetectLanguage(string text, out Language? result);
    }
}
