using System.Collections.Generic;
using PropertyChanged;

namespace WutzVote
{
	[ImplementPropertyChanged]
	public class BandApplication : Band
	{
		public List<string> YouTubeUrls { get; set; }
		public string YouTubeSearch { get; set; }
		public string SoundCloudUrl { get; set; }
		public string BandCampUrl { get; set; }

		public BandApplication(Band band)
		{
			Name = band.Name;
			BewID = band.BewID;
			Url = band.Url;
			Average = band.Average;
			Voting = band.Voting;
		}
	}
}
