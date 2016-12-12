using System.Collections.Generic;
using System.Text.RegularExpressions;
using PropertyChanged;
using RestSharp;
using System.Linq;
using System.Threading.Tasks;
using System;
using Xamarin.Forms;
using System.Diagnostics;
using System.Text;

namespace WutzVote
{
	[ImplementPropertyChanged]
	public class ApplicationPageModel : BasePageModel
	{
		private const string YouTubeEmbedUrl = "https://www.youtube-nocookie.com/embed/{0}?rel=0&autoplay=1";

		private const string SoundCloudEmbedUrl = "https://w.soundcloud.com/player/?url={0}"; // &auto_play=true

		private static Regex[] rxYouTube =
		{
			new Regex("\"(?<url>https?:\\/\\/(www\\.)?youtube\\.com\\/watch\\?v=(?<id>[^\"]+))\"", RegexOptions.Compiled | RegexOptions.IgnoreCase),
			new Regex("\"(?<url>https?:\\/\\/youtu\\.be\\/(?<id>[^\"]+))\"", RegexOptions.Compiled | RegexOptions.IgnoreCase),
			new Regex("\"(?<url>https?:\\/\\/(www\\.)?youtube\\.com\\/channel\\/[^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase)
		};

		private static Regex rxSoundCloud =
			new Regex("\"(?<url>https?:\\/\\/(www\\.)?soundcloud\\.com\\/(?<name>[^\"]+))\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxBandCamp =
			new Regex("\"(?<url>https?:\\/\\/(www\\.)?(?<name>[^\\.]+)\\.bandcamp\\.com)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		//https://www.youtube.com/results?search_query=Geschichte-in-liedern

		private static Regex rxYouTubeSearch =
			new Regex("\"(?<url>https?:\\/\\/(www\\.)youtube.com/results\\?search_query=[^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxVoting =
			new Regex("value=\"(?<voting>\\d)\"\\s*selected", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxAverage =
			new Regex("&Oslash;\\s+(?<average>\\d\\.\\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxVotes =
			new Regex("(?<count>\\d)\\sUser\\sgevotet", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private int _youTubeIndex = 0;

		public Band Band { get; set; }

		public BandApplication Application { get; set; }

		public string WebUrl { get; set; }

		public Command YouTubeCommand { get; set; }

		public Command VoteCommand { get; set; }

		public Command OpenWebsiteCommand { get; set; }

		private readonly RestClient _restClient;

		private readonly SessionSettings _sessionSettings;

		public ApplicationPageModel(RestClient restClient, SessionSettings sessionSettings)
		{
			_restClient = restClient;
			_sessionSettings = sessionSettings;

			SetupCommands();
		}

		private void SetupCommands()
		{
			YouTubeCommand = new Command(() =>
			{
				if (Application.YouTubeUrls.Any())
				{
					WebUrl = Application.YouTubeUrls.ElementAt(_youTubeIndex);

					_youTubeIndex++;
					if (_youTubeIndex == Application.YouTubeUrls.Count)
					{
						_youTubeIndex = 0;
					}
				}
			});

			VoteCommand = new Command(async (param) =>
			{
				string voting = param as string;
				if (!string.IsNullOrEmpty(voting))
				{
					await VoteBand(voting);
				}
			});

			OpenWebsiteCommand = new Command((param) =>
			{
				string url = param as string;
				if (!string.IsNullOrEmpty(url))
				{
					WebUrl = url;
				}
			});
		}

		public override async void Init(object initData)
		{
			base.Init(initData);

			Band = (Band)initData;
			Application = await LoadApplication(Band);

			if (Application != null)
			{
				if (Application.YouTubeUrls.Any())
				{
					YouTubeCommand.Execute(null);
				}
				else if (!string.IsNullOrEmpty(Application.SoundCloudUrl))
				{
					OpenWebsiteCommand.Execute(Application.SoundCloudUrl);
				}
				else if (!string.IsNullOrEmpty(Application.BandCampUrl))
				{
					OpenWebsiteCommand.Execute(Application.BandCampUrl);
				}
				else if (!string.IsNullOrEmpty(Application.YouTubeSearch))
				{
					OpenWebsiteCommand.Execute(Application.YouTubeSearch);
				}
			}
		}

		private async Task<BandApplication> LoadApplication(Band band)
		{
			try
			{
				using (new LoadingContext(this))
				{
					RestRequest request = new RestRequest(band.Url);
					IRestResponse response = await _restClient.ExecuteTaskAsync(request);
					Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");
					string html = iso_8859_1.GetString(response.RawBytes);

					Match maVoting = rxVoting.Match(html);
					if (maVoting.Success)
					{
						band.Voting = maVoting.Groups["voting"].Value;
					}

					Match maVotes = rxVotes.Match(html);
					if (maVotes.Success)
					{
						band.Votes = maVotes.Groups["count"].Value;
					}

					List<string> youTubeUrls = new List<string>();
					foreach (Regex rx in rxYouTube)
					{
						MatchCollection matches = rx.Matches(html);
						foreach (Match ma in matches)
						{
							string url = ma.Groups["url"].Value;

							if (url.Contains("/channel/"))
							{
								youTubeUrls.Add(url);
							}
							else
							{
								string id = ma.Groups["id"].Value;
								string playerUrl = string.Format(YouTubeEmbedUrl, id);
								youTubeUrls.Add(playerUrl);
							}
						}
					}
					youTubeUrls = youTubeUrls.Distinct().ToList();

					string soundCloudUrl = string.Empty;
					Match maSoundCloud = rxSoundCloud.Match(html);
					if (maSoundCloud.Success)
					{
						string url = maSoundCloud.Groups["url"].Value;
						soundCloudUrl = string.Format(SoundCloudEmbedUrl, url);
					}

					string bandCampUrl = string.Empty;
					Match maBandCamp = rxBandCamp.Match(html);
					if (maBandCamp.Success)
					{
						string url = maBandCamp.Groups["url"].Value;
						bandCampUrl = url;
					}

					string youTubeSearch = string.Empty;
					Match maYouTubeSearch = rxYouTubeSearch.Match(html);
					if (maYouTubeSearch.Success)
					{
						youTubeSearch = maYouTubeSearch.Groups["url"].Value;
					}
					else
					{
						Debugger.Break();
					}

					_youTubeIndex = 0;
					return new BandApplication(band)
					{
						YouTubeUrls = youTubeUrls,
						SoundCloudUrl = soundCloudUrl,
						BandCampUrl = bandCampUrl,
						YouTubeSearch = youTubeSearch
					};
				}
			}
			catch (Exception ex)
			{
				await CoreMethods.DisplayAlert("Error", ex.Message, "OK");
			}

			return null;
		}

		private async Task VoteBand(string voting)
		{
			try
			{
				using (new LoadingContext(this))
				{
					string voteUrl = string.Format(
						"index.php?site=user-bewerbungen&do=view&what=&bew_id={0}&v_bw_id={1}",
						Band.BewID,
						_sessionSettings.FestivalID);

					IRestRequest request = new RestRequest(voteUrl, Method.POST);
					request.AddParameter("vote_action", voting);
					request.AddParameter("bew_id", Band.BewID);
					request.AddParameter("submit", "Vote");

					IRestResponse response = await _restClient.ExecuteTaskAsync(request);
					Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");
					string html = iso_8859_1.GetString(response.RawBytes);

					Match maAverage = rxAverage.Match(html);
					if (maAverage.Success)
					{
						Band.Average = maAverage.Groups["average"].Value;
					}

					Match maVotes = rxVotes.Match(html);
					if (maVotes.Success)
					{
						Band.Votes = maVotes.Groups["count"].Value;
					}

					Band.Voting = voting;

					await CoreMethods.PopPageModel();
				}
			}
			catch (Exception ex)
			{
				await CoreMethods.DisplayAlert("Error", ex.Message, "OK");
			}
		}
	}
}
