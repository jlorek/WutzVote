using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace WutzVote.Converters
{
	public class StringListToCountConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			List<string> list = value as List<string>;

			if (list != null)
			{
				return list.Count;
			}

			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
