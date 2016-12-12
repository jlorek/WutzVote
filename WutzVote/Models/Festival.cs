using System;
using PropertyChanged;

namespace WutzVote
{
	[ImplementPropertyChanged]
	public class Festival
	{
		private static readonly Festival empty = new Festival { ID = 0, Name = string.Empty };

		public static Festival Empty
		{
			get
			{
				return empty;
			}
		}

		public int ID { get; set; }
		public string Name { get; set; }
	}
}
