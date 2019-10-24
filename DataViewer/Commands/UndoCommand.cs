using DataViewer.Models;

namespace DataViewer.Commands
{
    class UndoCommand
    {
        // if something is null then it was not modified by this action
        public LocalizationEntry LocalizationEntryRef;
        public Variant VariantRef;
        public TextLine TextLineRef;

        // these are values before change
        public LocalizationEntry LocalizationEntryValue;
        public Variant VariantValue;
        public TextLine TextLineValue;

        public UndoCommand(LocalizationEntry reference, LocalizationEntry value)
        {
            LocalizationEntryRef = reference;
            LocalizationEntryValue = value;
        }

        public UndoCommand(Variant reference, Variant value)
        {
            VariantRef = reference;
            VariantValue = value;
        }

        public UndoCommand(TextLine reference, TextLine value)
        {
            TextLineRef = reference;
            TextLineValue = value;
        }

        public void ExecuteUndo()
        {
            if (LocalizationEntryValue != null)
            {
                if (LocalizationEntryValue.Speaker != null)
                    LocalizationEntryRef.Speaker = LocalizationEntryValue.Speaker;
            }

            if (VariantValue != null)
            {
                if (VariantValue.Name != null)
                    VariantRef.Name = VariantValue.Name;
            }

            if (TextLineValue != null)
            {
                if(TextLineValue.Language != Language.None)
                    TextLineRef.Language = TextLineValue.Language;

                if (TextLineValue.Text != null)
                    TextLineRef.Text = TextLineValue.Text;
            }
        }
    }
}
