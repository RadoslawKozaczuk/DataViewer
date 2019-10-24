using System;
using System.Linq;
using System.Windows.Markup;

namespace DataViewer
{
    public class EnumToItemsSource : MarkupExtension
    {
        readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) 
            => Enum.GetValues(_type)
                .Cast<object>()
                .Select(e => new { Value = (int)e, DisplayName = e.ToString() });
    }
}
