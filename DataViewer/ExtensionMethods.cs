using System;

namespace DataViewer
{
    static class ExtensionMethods
    {
        /// <summary>
        /// In order to extend this list extend the enum and check the corresponding code at https://cloud.google.com/translate/docs/languages
        /// </summary>
        public static string ToGoogleLangId(this Language lang) => lang switch
        {
            Language.English_US => "en",
            Language.French => "fr",
            Language.Japanease => "ja",
            _ => throw new ArgumentOutOfRangeException("lang"),
        };
    }
}
