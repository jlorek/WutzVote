using System;
using System.Globalization;
using Xamarin.Forms;

namespace WutzVote.Converters
{
	public class VotingToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string && parameter is string)
			{
				var button = (string)value;
				var voting = (string)parameter;

				if (button == voting)
				{
					return Color.FromHex("#E0F3C3");
				}
			}

			return Color.FromHex("#EEEEEE");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
