using System;
using System.Windows.Markup;

namespace OMNI.Converters
{
    public sealed class EnumtoItemsSource : MarkupExtension
    {
        private readonly Type _enumType;
        public EnumtoItemsSource(Type enumType)
        {
            _enumType = enumType;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType);
        }

    }
}
