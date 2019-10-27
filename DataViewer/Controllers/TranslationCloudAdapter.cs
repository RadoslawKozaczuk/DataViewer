using DataViewer.Interfaces;
using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace DataViewer.Controllers
{
    class TranslationCloudAdapter : ITranslationCloudAdapter
    {
        const string INVALID_LANG_DET_THRESHOLD_MSG
            = "Invalid LanguageDetectionThreshold value in appconfig. The value should be either from 0 to 1 inclusive or 'API_default'.";

        const string INVALID_TRANSLATION_METHOD_MSG
            = "Invalid or unrecognized Translation Method in appconfig. Allowed values are 'ServiceDefault', 'Base' or 'NeuralMachineTranslation'";

        // true - let Google decide, false - manual threshold
        (bool apiDefault, float threshold) _confidenceThreshold;
        readonly TranslationModel _translationModel;

        public TranslationCloudAdapter()
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

        /// <summary>
        /// Translates the given text. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the text parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the text parameter is empty or contains only white spaces, 
        /// or when the source and target languages are the same.</exception>
        public bool Translate(string text, Language source, Language target, out string translation)
        {
            // assertions
            if (text == null)
                throw new ArgumentNullException("texts");
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("text cannot be empty", "texts");
            if (source == target)
                throw new ArgumentException("The source the target languages cannot be the same.");

            translation = null;
            using TranslationClient client = TranslationClient.Create();

            try
            {
                translation = client.TranslateText(
                    text, 
                    target.ToGoogleLangId(), 
                    source.ToGoogleLangId(), 
                    _translationModel).TranslatedText;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the texts parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the texts parameter is empty.</exception>
        public bool DetectLanguages(IList<string> texts, out IList<Language?> results)
        {
            // assertions
            if (texts == null)
                throw new ArgumentNullException("texts");
            if (texts.Count == 0)
                throw new ArgumentException("texts list cannot be empty", "texts");

            using TranslationClient client = TranslationClient.Create();
            results = new List<Language?>(texts.Count);
            IList<Detection> detections;

            try
            {
                detections = client.DetectLanguages(texts);
            }
            catch
            {
                return false;
            }

            for (int i = 0; i < detections.Count; i++)
            {
                Detection d = detections[i];

                if (_confidenceThreshold.apiDefault)
                {
                    if (d.IsReliable) // we let Google decide what's reliable and what's not
                        results[i] = d.Language.ConvertGoogleLanguageIdToLanguageEnum();
                }
                else
                {
                    if (d.Confidence > _confidenceThreshold.threshold) // based on the threshold set in appconfig
                        results[i] = d.Language.ConvertGoogleLanguageIdToLanguageEnum();
                }
            }

            return true;
        }

        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// Do not use it multiple times at once (for example in a loop) as it is inefficient both in terms of money and time. 
        /// If you have a set of data to recognize, use <see cref="DetectLanguages"/> instead.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the text parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the text parameter is empty or contains only white spaces.</exception>
        public bool DetectLanguage(string text, out Language? result)
        {
            // assertion
            if (text == null)
                throw new ArgumentNullException("texts");
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("text cannot be empty", "texts");

            result = null;

            using TranslationClient client = TranslationClient.Create();
            Detection d;

            try
            {
                d = client.DetectLanguage(text);
            }
            catch
            {
                return false;
            }

            if (_confidenceThreshold.apiDefault)
            {
                if (d.IsReliable) // we let Google decide what's reliable and what's not
                    result = d.Language.ConvertGoogleLanguageIdToLanguageEnum();
            }
            else
            {
                if (d.Confidence > _confidenceThreshold.threshold) // based on the threshold set in appconfig
                    result = d.Language.ConvertGoogleLanguageIdToLanguageEnum();
            }

            return true;
        }
    }
}
