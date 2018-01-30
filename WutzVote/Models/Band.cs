using System.ComponentModel;

namespace WutzVote
{
    public class Band : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

		public string Name { get; set; }
		public string Url { get; set; }
		public string BewID { get; set; }
		public string Voting { get; set; }
		public string Votes { get; set; }
		public string Average { get; set; }

		public string Rating
		{
			get
			{
				string displayValue = string.Empty;

				if (!string.IsNullOrEmpty(Voting) && !string.IsNullOrEmpty(Average))
				{
					displayValue = $"Dein Voting: {Voting} - Durchschnitt: {Average}";
				}
				else if (!string.IsNullOrEmpty(Voting))
				{
					displayValue = $"Dein Voting: {Voting}";
				}
				else if (!string.IsNullOrEmpty(Average))
				{
					displayValue = $"Durchschnitt: {Average}";
				}
				else
				{
					return "Keine Bewertung";
				}

				if (!string.IsNullOrEmpty(Votes))
				{
					displayValue = displayValue + $" - Votes: {Votes}";
				}

				return displayValue;
			}
		}
	}
}
