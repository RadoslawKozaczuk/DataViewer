using System;

namespace DataViewer
{
    /// <summary>
    /// Helps to hide the enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShouldBeHiddenAttribute : Attribute
    {
        public ShouldBeHiddenAttribute(bool isHiddenInUi)
        {
            HiddenInUi = isHiddenInUi;
        }

        public bool HiddenInUi { get; set; }
    }

    public enum Language
    {
        /// <summary>
        /// Indicates that the field is empty.
        /// </summary>
        [ShouldBeHidden(true)]
        None,

        /// <summary>
        /// Indicates that the language could not be parsed by JSON deserializer.
        /// Typically means that data is inconsistent.
        /// </summary>
        [ShouldBeHidden(true)]
        UnidentifiedOrInvalid,

        // below that are valid languages
        English_US,
        Japanease,
        French
    }
}
