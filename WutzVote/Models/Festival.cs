using System.ComponentModel;

namespace WutzVote
{
    public class Festival : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

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
