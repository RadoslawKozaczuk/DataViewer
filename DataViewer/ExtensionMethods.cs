using System;
using System.Windows.Data;

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

        /// <summary>
        /// Performs normal refresh but before that makes sure if view is not in Editing or Adding mode and if so commits changes.
        /// </summary>
        public static void ForceCommitRefresh(this ListCollectionView view)
        {
            if (view.IsEditingItem)
                view.CommitEdit();

            if (view.IsAddingNew)
                view.CommitNew();

            view.Refresh();
        }
    }
}
