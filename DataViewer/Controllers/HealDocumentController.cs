using DataViewer.Interfaces;
using DataViewer.Models;
using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace DataViewer.Controllers
{
    class HealDocumentController : IHealDocumentController
    {
        const string INVALID_LANG_DET_THRESHOLD_MSG
            = "Invalid LanguageDetectionThreshold value in appconfig. The value should be either from 0 to 1 inclusive or 'API_default'.";

        const string INVALID_TRANSLATION_METHOD_MSG
            = "Invalid or unrecognized Translation Method in appconfig. Allowed values are 'ServiceDefault', 'Base' or 'NeuralMachineTranslation'";

        // true - let Google decide, false - manual threshold
        (bool apiDefault, float threshold) _confidenceThreshold;

        IList<LocalizationEntry> _entries;
        readonly TranslationModel _translationModel;

        public HealDocumentController()
        {
            if (!Enum.TryParse(ConfigurationManager.AppSettings["TranslationMethod"], out _translationModel))
                throw new ArgumentException(INVALID_TRANSLATION_METHOD_MSG);

            string langugeDetectionThresholdValue = ConfigurationManager.AppSettings["LanguageDetectionThreshold"];

            _confidenceThreshold.apiDefault = langugeDetectionThresholdValue.ToLower() == "api_default";

            if (!_confidenceThreshold.apiDefault)
            {
                if (!float.TryParse(langugeDetectionThresholdValue, out float val)
                    || val < 0
                    || val > 1)
                    throw new ArgumentException(INVALID_LANG_DET_THRESHOLD_MSG);

                _confidenceThreshold.threshold = val;
            }
        }

        public void ScanDocument(IList<LocalizationEntry> entries)
        {
            ValidateEntries(entries);
            ValidateTextLines(entries);
        }

        void ValidateEntries(IList<LocalizationEntry> entries)
        {
            foreach(LocalizationEntry entry in entries)
            {
                if (!Guid.TryParse(entry.GUID, out Guid _))
                    entry.GUIDIsValid = false;
                if (string.IsNullOrWhiteSpace(entry.Speaker))
                    entry.SpeakerIsValid = false;
                entry.Scanned = true;
            }
        }

        void ValidateTextLines(IList<LocalizationEntry> entries)
        {
            foreach (LocalizationEntry entry in entries)
            {
                if (!Guid.TryParse(entry.GUID, out Guid _))
                    entry.GUIDIsValid = false;
                if (string.IsNullOrWhiteSpace(entry.Speaker))
                    entry.SpeakerIsValid = false;
                entry.Scanned = true;
            }
        }

        public void HealDocument(IList<LocalizationEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            _entries = entries;

            RemoveEntriesWithInvalidGUID();
            RemoveEntriesWithDuplicatedGUID();
            CorrectLanguageEntries();
        }

        /// <summary>
        /// Returns null in case of error or no connection.
        /// </summary>
        public string Translate(string text, Language source, Language target)
        {
            // assertions
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");
            if (source == target)
                throw new ArgumentException("The source language and the target language can not be the same.");

            TranslationResult result;

            try
            {
                using TranslationClient client = TranslationClient.Create();
                result = client.TranslateText(text, target.ToGoogleLangId(), source.ToGoogleLangId(), _translationModel);

                return result.TranslatedText;
            }
            catch (Exception)
            {
                return null;
            }
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

        void CorrectLanguageEntries()
        {
            var translationDataList = new List<TextLine>();
            var languagesToCheck = new List<string>();

            foreach (LocalizationEntry entry in _entries)
                foreach (Variant variant in entry.Variants)
                    foreach (TextLine textLine in variant.TextLines)
                    {
                        translationDataList.Add(textLine);
                        languagesToCheck.Add(textLine.Language.ToGoogleLangId());
                    }

            using TranslationClient client = TranslationClient.Create();
            IList<Detection> detections = client.DetectLanguages(languagesToCheck);

            // apply detected data
            for (int i = 0; i < detections.Count; i++)
            {
                Detection d = detections[i];

                Language? detectedLang = detections[i].Language.ConvertGoogleLanguageIdToLanguageEnum();
                if (!detectedLang.HasValue)
                    continue; // ignore it, detected language is not supported in our system

                if (_confidenceThreshold.apiDefault)
                {
                    // we let Google decide what's reliable and what's not
                    if (d.IsReliable)
                        translationDataList[i].Language = detectedLang.Value;
                }
                else
                {
                    // we manually set the certain threshold above which we treat results as valid
                    if (d.Confidence > _confidenceThreshold.threshold)
                        translationDataList[i].Language = detectedLang.Value;
                }
            }
        }
    }
}
