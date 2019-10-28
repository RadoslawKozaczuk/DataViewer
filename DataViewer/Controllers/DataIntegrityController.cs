using DataViewer.Interfaces;
using DataViewer.Models;
using System;
using System.Collections.Generic;

namespace DataViewer.Controllers
{
    class DataIntegrityController : IDataIntegrityController
    {
        readonly ITranslationCloudAdapter _cloud;
        IList<LocalizationEntry> _entries;

        public DataIntegrityController(ITranslationCloudAdapter translationCloudAdapter)
        {
            _cloud = translationCloudAdapter;
        }

        public bool? PerformFullScan(IList<LocalizationEntry> entries)
        {
            bool valid = true;
            foreach (LocalizationEntry entry in entries)
            {
                if (!ScanLocalizationEntry(entry))
                    valid = false;

                foreach (Variant variant in entry.Variants)
                {
                    if (!ScanVariant(variant))
                        valid = false;

                    foreach (TextLine textLine in variant.TextLines)
                    {
                        if (!ScanTextLine(textLine))
                            valid = false;
                    }
                }
            }

            return valid;
        }

        public bool ScanLocalizationEntry(LocalizationEntry entry)
        {
            bool valid = true;

            if (!Guid.TryParse(entry.GUID, out Guid _))
                entry.GUIDIsValid = valid = false;
            if (string.IsNullOrWhiteSpace(entry.Speaker))
                entry.SpeakerIsValid = valid = false;

            return valid;
        }

        public bool ScanVariant(Variant variant)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(variant.Name))
                variant.NameIsValid = valid = false;

            return valid;
        }

        public bool ScanTextLine(TextLine textLine)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(textLine.Text))
                textLine.TextIsValid = valid = false;

            if (textLine.Language == null)
                textLine.LanguageIsValid = valid = false;

            return valid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when entries parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when entries parameter is empty.</exception>
        public bool HealDocument(IList<LocalizationEntry> entries)
        {
            // assertions
            if (entries == null)
                throw new ArgumentNullException("entries");
            if (entries.Count == 0)
                throw new ArgumentException("entry list cannot be empty", "entries");

            _entries = entries;

            RemoveEntriesWithInvalidGUID();
            RemoveEntriesWithDuplicatedGUID();
            CorrectLanguageEntries();

            return true;
        }

        void RemoveEntriesWithInvalidGUID()
        {
            for (int i = 0; i < _entries.Count; i++)
                if (!Guid.TryParse(_entries[i].GUID, out Guid _))
                    _entries.RemoveAt(i--);
        }

        void RemoveEntriesWithDuplicatedGUID()
        {
            var guidsSeen = new HashSet<string>();
            for (int i = 0; i < _entries.Count; i++)
            {
                if (guidsSeen.Contains(_entries[i].GUID))
                    _entries.RemoveAt(i--);
                else
                    guidsSeen.Add(_entries[i].GUID);
            }
        }

        /// <summary>
        /// Returns false in case of error (for example due to no Internet connection).
        /// </summary>
        bool CorrectLanguageEntries()
        {
            var translationDataList = new List<TextLine>();
            var textsToCheck = new List<string>();

            foreach (LocalizationEntry entry in _entries)
                foreach (Variant variant in entry.Variants)
                    foreach (TextLine textLine in variant.TextLines)
                    {
                        translationDataList.Add(textLine);
                        textsToCheck.Add(textLine.Text);
                    }

            if(_cloud.DetectLanguages(textsToCheck, out IList<Language?> detections))
            {
                // apply detected data
                for (int i = 0; i < detections.Count; i++)
                {
                    Language? d = detections[i];

                    if (d == null)
                        continue; // null means either detection not possible or detected language not supported by out system

                    translationDataList[i].Language = d;
                }

                return true;
            }

            return false;
        }
    }
}
