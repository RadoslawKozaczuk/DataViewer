using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace DataViewer
{
    static class ExtensionMethods
    { 
        /// <summary>
        /// In order to extend this list extend <see cref="Language"/> enumerator and check the corresponding code at
        /// <a href="https://cloud.google.com/translate/docs/languages">https://cloud.google.com/translate/docs/languages</a>
        /// </summary>
        public static string ToGoogleLangId(this Language? lang) => lang switch
        {
            Language.English_US => "en",
            Language.French => "fr",
            Language.Japanease => "ja",
            _ => throw new ArgumentOutOfRangeException("lang"),
        };

        /// <summary>
        /// In order to extend this list extend <see cref="Language"/> enumerator and check the corresponding code at
        /// <a href="https://cloud.google.com/translate/docs/languages">https://cloud.google.com/translate/docs/languages</a>
        /// </summary>
        public static string ToGoogleLangId(this Language lang) => lang switch
        {
            Language.English_US => "en",
            Language.French => "fr",
            Language.Japanease => "ja",
            _ => throw new ArgumentOutOfRangeException("lang"),
        };

        /// <summary>
        /// In order to extend this list extend <see cref="Language"/> enumerator and check the corresponding code at
        /// <a href="https://cloud.google.com/translate/docs/languages">https://cloud.google.com/translate/docs/languages</a>
        /// <code/>If throwExceptionWhenNotSupported is set to true, the method will throw an exception if there is no match in the <see cref="Language"/> enumerator
        /// for the given Google language id, otherwise it will be set to null.
        /// </summary>
        public static Language? ConvertGoogleLanguageIdToLanguageEnum(this string lang, bool throwExceptionWhenNotSupported = false) => lang switch
        {
            "en" => Language.English_US,
            "fr" => Language.French,
            "ja" => Language.Japanease,
            _ => throwExceptionWhenNotSupported ? throw new ArgumentOutOfRangeException("lang") : (Language?)null,
        };

        /// <summary>
        /// Commits any 'edit' or 'new' transaction and after that refreshes the view.
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

        /// <summary>
        /// Returns for example 'E751F84EDCD7437F9F4BC89CEEDC5118'.
        /// </summary>
        public static string ToPlainUpper(this Guid guid) 
            => guid.ToString().Replace("-", "").ToUpper();

        /// <summary>
        /// Executes the given function on every cell in the given area. 
        /// Immediately returns true once the function return value is true.
        /// </summary>
        internal static bool Any<T>(this IEnumerable<T> colleciton, Func<T, bool> func)
        {
            foreach(T item in colleciton)
                if (func(item))
                    return true;

            return false;
        }
    }
}
