using DataViewer.Interfaces;
using DataViewer.Models;
using MoreLinq;
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

        /// <summary>
        /// Returns false when data is invalid, and true when data is valid.
        /// </summary>
        public bool PerformFullScan(IList<LocalizationEntry> entries)
        {
            _entries = entries;
            bool valid = true;

            ScanEachDataModelIndividually(ref valid);
            MarkDuplicatedGUIDs(ref valid);
            CheckLanguageMatching(ref valid);

            return valid;
        }

        /// <summary>
        /// Restores data integrity.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when entries parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when entries parameter is empty.</exception>
        public void HealDocument(IList<LocalizationEntry> entries)
        {
#if DEBUG
            // assertions
            if (entries == null)
                throw new ArgumentNullException("entries");
            if (entries.Count == 0)
                throw new ArgumentException("entry list cannot be empty", "entries");
#endif
            
            _entries = entries;

            RemoveEntriesWithInvalidGUID();
            RemoveEntriesWithDuplicatedGUID();
            CorrectLanguageEntries();
        }

        void ScanEachDataModelIndividually(ref bool valid)
        {
            foreach (LocalizationEntry entry in _entries)
            {
                if (!ScanLocalizationEntry(entry))
                    valid = false;

                foreach (Variant variant in entry.Variants)
                {
                    if (!ScanVariant(variant))
                        valid = false;

                    foreach (TextLine textLine in variant.TextLines)
                        if (!ScanTextLine(textLine))
                            valid = false;
                }
            }
        }

        void MarkDuplicatedGUIDs(ref bool valid)
        {
            var guidsSeen = new Dictionary<string, LocalizationEntry>();
            foreach (LocalizationEntry entry in _entries)
            {
                if (guidsSeen.TryGetValue(entry.GUID, out LocalizationEntry objRef))
                {
                    objRef.GUIDIsValid = false;
                    entry.GUIDIsValid = false;
                    valid = false;
                }
                else
                    guidsSeen.Add(entry.GUID, entry);
            }
        }

        void CheckLanguageMatching(ref bool valid)
        {
            var referenceList = new List<TextLine>();
            var textsToCheck = new List<string>();

            _entries.ForEach(e => e.Variants.ForEach(v => v.TextLines.ForEach(t =>
            {
                referenceList.Add(t);
                textsToCheck.Add(t.Text);
            })));

            if (!_cloud.DetectLanguages(textsToCheck, out IList<Language?> detections))
                return; // detection not possible, return control

            // apply detected data
            for (int i = 0; i < detections.Count; i++)
            {
                Language? d = detections[i];

                if (d == null)
                    continue; // null means either detection not possible or detected language not supported by our system

                if (!referenceList[i].Language.HasValue) // no value in the model
                    continue;

                if (referenceList[i].Language.Value != d.Value)
                    referenceList[i].LanguageIsValid = valid = false;
            }
        }

        bool ScanLocalizationEntry(LocalizationEntry entry)
        {
            bool valid = true;

            if (!Guid.TryParse(entry.GUID, out Guid _))
                entry.GUIDIsValid = valid = false;
            if (string.IsNullOrWhiteSpace(entry.Speaker))
                entry.SpeakerIsValid = valid = false;

            return valid;
        }

        bool ScanVariant(Variant variant)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(variant.Name))
                variant.NameIsValid = valid = false;

            return valid;
        }

        bool ScanTextLine(TextLine textLine)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(textLine.Text))
                textLine.TextIsValid = valid = false;
            if (textLine.Language == null)
                textLine.LanguageIsValid = valid = false;

            return valid;
        }

        void RemoveEntriesWithInvalidGUID()
        {
            for (int i = 0; i < _entries.Count; i++)
                if (!Guid.TryParse(_entries[i].GUID, out Guid _))
                    _entries.RemoveAt(i--);
        }

        void RemoveEntriesWithDuplicatedGUID()
        {
            var guidsSeen = new Dictionary<string, LocalizationEntry>();
            for (int i = 0; i < _entries.Count; i++)
            {
                if (guidsSeen.TryGetValue(_entries[i].GUID, out LocalizationEntry objRef))
                {
                    objRef.GUIDIsValid = true;
                    _entries.RemoveAt(i--);
                }
                else
                    guidsSeen.Add(_entries[i].GUID, _entries[i]);
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

            if(!_cloud.DetectLanguages(textsToCheck, out IList<Language?> detections))
                return false; // detection not possible, return control

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
    }
}
