using DataViewer.Models;
using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace DataViewer.Controllers
{
    class HealDocumentController
    {
        const string INVALID_LANG_DET_THRESHOLD_MSG 
            = "Invalid LanguageDetectionThreshold value in appconfig. The value should be either from 0 to 1 inclusive or 'API_default'.";

        const string INVALID_TRANSLATION_METHOD_MSG
            = "Invalid or unrecognized Translation Method in appconfig. Allowed values are 'ServiceDefault', 'Base' or 'NeuralMachineTranslation'";

        // true - let Google decide, false - manual threshold
        (bool apiDefault, float threshold) _cofidencyThreshold;

        List<LocalizationEntry> _entries;

        TranslationModel _translationModel;

        public HealDocumentController()
        {
            if(!Enum.TryParse(ConfigurationManager.AppSettings["TranslationMethod"], out _translationModel))
                throw new ArgumentException(INVALID_TRANSLATION_METHOD_MSG);
           
            string langugeDetectionThresholdValue = ConfigurationManager.AppSettings["LanguageDetectionThreshold"];

            _cofidencyThreshold.apiDefault = langugeDetectionThresholdValue.ToLower() == "api_default";

            if(!_cofidencyThreshold.apiDefault)
            {
                if (!float.TryParse(langugeDetectionThresholdValue, out float val)
                || val < 0
                || val > 1)
                    throw new ArgumentException(INVALID_LANG_DET_THRESHOLD_MSG);

                _cofidencyThreshold.threshold = val;
            }
        }

        public void HealDocument(List<LocalizationEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            _entries = entries;

            //RemoveEntriesWithInvalidGUID();
            //RemoveEntriesWithDuplicatedGUID();
            //CorrectLanguageEntries();
            MergeVariantsWithSameName();
        }

        void RemoveEntriesWithInvalidGUID()
        {
            for(int i = 0; i < _entries.Count; i++)
                if(!Guid.TryParse(_entries[i].GUID, out Guid _))
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

        void MergeVariantsWithSameName()
        {
            foreach (LocalizationEntry entry in _entries)
            {
                // holds variants from within the same entry that have the same name (empty names are ignored)
                var dict = new Dictionary<string, List<Variant>>();

                foreach (Variant variant in entry.Variants)
                {
                    string name = variant.Name;

                    if (string.IsNullOrWhiteSpace(name))
                        continue; // ignore empty, we don't want to merge entries with empty name as that would be data overinterpretation

                    // unify key
                    name = name.Trim().ToLower();

                    if (dict.ContainsKey(name))
                    {
                        var list = dict[name];
                        list.Add(variant);
                        dict[name] = list;
                    }
                    else
                    {
                        dict.Add(name, new List<Variant> { variant });
                    }
                }

                // merge them all (add all subsequent to the first one)
                foreach(List<Variant> variants in dict.Values)
                {
                    Variant first = variants[0];
                    for(int i = 1; i < variants.Count; i++)
                    {
                        first.TextLines.AddRange(variants[i].TextLines);
                        variants.RemoveAt(i--);
                    }
                }

                dict.Clear();
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

                if (_cofidencyThreshold.apiDefault)
                {
                    // we let Google decide what's reliable and what's not
                    if (d.IsReliable)
                        translationDataList[i].Language = detectedLang.Value;
                }
                else
                {
                    // we manually set the certain threshold above which we treat results as valid
                    if(d.Confidence > _cofidencyThreshold.threshold)
                        translationDataList[i].Language = detectedLang.Value;
                }
            }
        }

        public async void TranslateAsync()
        {
            //_isTranslating = true;
            //NotifyOfPropertyChange(() => CanTranslate);

            //try
            //{
            //    using TranslationClient client = TranslationClient.Create();
            //    var translationTask = new Task<TranslationResult>(() =>
            //        client.TranslateText(
            //            text: SelectedTextLine.Text,
            //            targetLanguage: TranslationLanguage.ToGoogleLangId(),
            //            sourceLanguage: SelectedTextLine.Language.ToGoogleLangId(),
            //            model: TranslationModel.NeuralMachineTranslation)
            //        );

            //    translationTask.Start();
            //    await Task.WhenAll(translationTask);

            //    SelectedTextLine.TranslatedText = translationTask.Result.TranslatedText;
            //    SelectedTextLine.TranslationLanguage = TranslationLanguage;

            //    _textLinesView.ForceCommitRefresh();
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show(
            //        "Translation error. No Internet connection or Google Translation Cloud Service is inactive.",
            //        "Translation Error",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);

            //    return;
            //}
            //finally
            //{
            //    _isTranslating = false;
            //    NotifyOfPropertyChange(() => CanTranslate);
            //}
        }
    }
}
