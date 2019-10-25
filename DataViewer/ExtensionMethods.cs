using System;
using System.Windows;
using System.Windows.Data;

namespace DataViewer
{
    static class ExtensionMethods
    {
        /// <summary>
        /// In order to extend this list extend the <see cref="Language"/> enumerator and check the corresponding code at https://cloud.google.com/translate/docs/languages
        /// </summary>
        public static string ToGoogleLangId(this Language? lang) => lang switch
        {
            Language.English_US => "en",
            Language.French => "fr",
            Language.Japanease => "ja",
            _ => throw new ArgumentOutOfRangeException("lang"),
        };

        /// <summary>
        /// In order to extend this list extend the <see cref="Language"/> enumerator and check the corresponding code at https://cloud.google.com/translate/docs/languages
        /// </summary>
        public static string ToGoogleLangId(this Language lang) => lang switch
        {
            Language.English_US => "en",
            Language.French => "fr",
            Language.Japanease => "ja",
            _ => throw new ArgumentOutOfRangeException("lang"),
        };

        /// <summary>
        /// In order to extend this list extend the <see cref="Language"/> enumerator and check the corresponding code at https://cloud.google.com/translate/docs/languages
        /// If throwExceptionWhenNotSupported is set to true method will throw an exception is there is no match in the <see cref="Language"/> enumerator
        /// for the given Google language id, other wise it will be set to None.
        /// </summary>
        public static Language? ConvertGoogleLanguageIdToLanguageEnum(this string lang, bool throwExceptionWhenNotSupported = false) => lang switch
        {
            "en" => Language.English_US,
            "fr" => Language.French,
            "ja" => Language.Japanease,
            _ => throwExceptionWhenNotSupported ? throw new ArgumentOutOfRangeException("lang") : (Language?)null,
        };

        /// <summary>
        /// Performs normal refresh but before that makes sure if view is not in Editing or Adding mode and if so commits changes.
        /// </summary>
        public static void ForceCommitRefresh(this ListCollectionView view)
        {
            if (view == null)
                return;

            if (view.IsEditingItem)
                view.CommitEdit();

            if (view.IsAddingNew)
                view.CommitNew();

            view.Refresh();
        }

        public static Window GetWindow(this FrameworkElement element)
        {
            if (element == null)
                return null;

            if (element is Window)
                return (Window)element;

            if (element.Parent == null)
                return null;

            return GetWindow(element.Parent as FrameworkElement);
        }
    }
}
