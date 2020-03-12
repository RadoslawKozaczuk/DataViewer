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
        readonly (bool apiDefault, float threshold) _confidenceThreshold;
        readonly TranslationModel _translationModel;

        public TranslationCloudAdapter()
        {
#if DEBUG
            // assertion
            if (!Enum.TryParse(ConfigurationManager.AppSettings["TranslationMethod"], out _translationModel))
                throw new ArgumentException(INVALID_TRANSLATION_METHOD_MSG);
#endif

            string langugeDetectionThresholdValue = ConfigurationManager.AppSettings["LanguageDetectionThreshold"];
            _confidenceThreshold.apiDefault = langugeDetectionThresholdValue.ToLower() == "api_default";

            if (!_confidenceThreshold.apiDefault)
            {
                if (!float.TryParse(langugeDetectionThresholdValue, out float value)
                    || value < 0
                    || value > 1)
                    throw new ArgumentException(INVALID_LANG_DET_THRESHOLD_MSG);

                _confidenceThreshold.threshold = value;
            }
        }

        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when text parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when text parameter is empty or contains only white spaces, 
        /// or when the source and target languages are the same.</exception>
        public bool Translate(string text, Language source, Language target, out string translation)
        {
#if DEBUG
            // assertions
            if (text == null)
                throw new ArgumentNullException("texts");
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("text cannot be empty", "texts");
            if (source == target)
                throw new ArgumentException("The source the target languages cannot be the same.");
#endif

            translation = null;
            using TranslationClient client = TranslationClient.Create();

            try
            {
                translation = client.TranslateText(text, target.ToGoogleLangId(), source.ToGoogleLangId(), _translationModel)
                    .TranslatedText;

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
        /// <exception cref="ArgumentNullException">Thrown when texts parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when texts parameter is empty.</exception>
        public bool DetectLanguages(IList<string> texts, out IList<Language?> results)
        {
#if DEBUG
            // assertions
            if (texts == null)
                throw new ArgumentNullException("texts");
            if (texts.Count == 0)
                throw new ArgumentException("texts list cannot be empty", "texts");
#endif

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
                Language? l = GetLanguage(detections[i]);
                if (l != null) 
                    results.Add(l);
            }

            return true;
        }

        /// <summary>
        /// This method may take up to few seconds to complete. 
        /// Returns false if the result could not be obtained (for example due to no Internet connection).
        /// Do not use it multiple times at once (for example in a loop) as it is inefficient both in terms of money and time. 
        /// If you have a set of data to recognize, use <see cref="DetectLanguages"/> instead.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when text parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when text parameter is empty or contains only white spaces.</exception>
        public bool DetectLanguage(string text, out Language? result)
        {
#if DEBUG
            // assertions
            if (text == null)
                throw new ArgumentNullException("texts");
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("text cannot be empty", "texts");
#endif

            result = null;

            using TranslationClient client = TranslationClient.Create();
            Detection detection;

            try
            {
                detection = client.DetectLanguage(text);
            }
            catch
            {
                return false;
            }

            result = GetLanguage(detection);

            return true;
        }

        Language? GetLanguage(Detection detection)
        {
            if (_confidenceThreshold.apiDefault)
            {
                if (detection.IsReliable) // we let Google decide what's reliable and what's not
                    return detection.Language.ConvertGoogleLanguageIdToLanguageEnum();
            }
            else
            {
                if (detection.Confidence > _confidenceThreshold.threshold) // based on the threshold set in appconfig
                    return detection.Language.ConvertGoogleLanguageIdToLanguageEnum();
            }

            return null;
        }
    }
}
