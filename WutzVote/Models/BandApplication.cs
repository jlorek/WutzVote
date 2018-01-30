using System.Collections.Generic;

namespace WutzVote
{
	public class BandApplication : Band
	{
		public List<string> YouTubeUrls { get; set; } = new List<string>();

		public string YouTubeSearch { get; set; } = string.Empty;

		public string SoundCloudUrl { get; set; } = string.Empty;

		public string BandCampUrl { get; set; } = string.Empty;

		// workaround, since xaml's StringFormat
		// Text="{Binding Application.YouTubeUrls, Converter={StaticResource StringListToCount}, StringFormat='  YouTube ({0})  '}"
		// will trim the the ending whitespaces
		public string YouTubeButton
		{
			get { return $"  YouTube ({YouTubeUrls.Count})  "; }
		}

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
