using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;
using System.Collections.Generic;

namespace WutzVote
{
	public class BandsPageModel : BasePageModel
	{
        public List<Band> AllBands = new List<Band>();

		public ObservableCollection<Band> Bands { get; set; } = new ObservableCollection<Band>();

		public string Name { get; set; }

		public Band Selected
		{
			set
			{
				if (value != null)
				{
					CoreMethods.PushPageModel<ApplicationPageModel>(value);
				}
			}
		}

        public string SearchTerm
        {
            set
            {
                Filter(value);
            }
        }

		private static Regex rxBand =
            new Regex("width:93%\"><a href=\"(?<url>[^\"]+)\"\\s+>(?<name>[^<]+)<\\/a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxBandEnd =
			new Regex("</tr>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxBewId =
			new Regex(@"bew_id=(?<id>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static Regex rxAverage =
			new Regex("green\">(?<avg>\\d\\.\\d)</td>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly RestClient _restClient;
		private readonly SessionSettings _sessionSettings;

        public Command<string> SearchCommand { get; set; }

		public BandsPageModel(RestClient restClient, SessionSettings sessionSettings)
		{
			_sessionSettings = sessionSettings;
			_restClient = restClient;

			Name = sessionSettings.Festival.Name;

            SetupCommands();
		}

        private void SetupCommands()
        {
            SearchCommand = new Command<string>((string term) =>
            {
                Bands.Clear();
                foreach (var band in AllBands)
                {
                    if (band.Name.Contains(term))
                    {
                        Bands.Add(band);
                    }
                }
            });
        }

        private void Filter(string searchTerm)
        {
            Bands.Clear();
            foreach (var band in AllBands)
            {
                if (string.IsNullOrEmpty(searchTerm) || band.Name.Contains(searchTerm))
                {
                    Bands.Add(band);
                }
            }
        }

		public override async void Init(object initData)
		{
			base.Init(initData);

			await LoadBands();
		}

		private async Task LoadBands()
		{
			try
			{
				using (new LoadingContext(this))
				{
					int page = 1;
					bool newBandFound = true;

					while (newBandFound)
					{
						string listUrl = string.Format(
							"index.php?site=user-bewerbungen&v_bw_id={0}&start={1}",
							_sessionSettings.Festival.ID,
							page);

						RestRequest request = new RestRequest(listUrl, Method.GET);
						IRestResponse response = await _restClient.ExecuteTaskAsync(request);
						Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");
						string html = iso_8859_1.GetString(response.RawBytes);

						MatchCollection matches = rxBand.Matches(html);

						newBandFound = (matches.Count > 0);
						foreach (Match maBand in matches)
						{
							Match maBandEnd = rxBandEnd.Match(html, maBand.Index);
							if (!maBandEnd.Success)
							{
								break;
							}

							string url = UnescapeString(maBand.Groups["url"].Value);

							Match maBewId = rxBewId.Match(url);
							if (maBewId.Success)
							{
								string bewId = maBewId.Groups["id"].Value;

								// make absolute url relative
								url = url.Replace("https://www.festivalticker.de/my_account/", string.Empty);

								string name = UnescapeString(maBand.Groups["name"].Value);

								Match maAverage = rxAverage.Match(html, maBand.Index, maBandEnd.Index - maBand.Index);
								string average = string.Empty;
								if (maAverage.Success)
								{
									average = maAverage.Groups["avg"].Value;
								}

								var band = new Band
								{
									Name = name,
									Url = url,
									Average = average,
									BewID = bewId
								};

								// when we found a reoccuring band, we have reached the end
                                if (AllBands.Any(b => b.Url.Equals(band.Url)))
								{
									newBandFound = false;
									break;
								}

                                AllBands.Add(band);
							}
						}

						page++;
					}

                    foreach (var band in AllBands)
                    {
                        Bands.Add(band);
                    }
				}
			}
			catch (Exception ex)
			{
				await CoreMethods.DisplayAlert("Error", ex.ToString(), "OK");
			}
		}

		private string UnescapeString(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				return string.Empty;
			}

			return data
				.Replace("&amp;", "&")
				.Replace("&lt;", "<")
				.Replace("&gt;", ">")
				.Replace("&quot;", "\"")
				.Replace("&apos;", "'")
				.Replace("&auml;", "ä")
				.Replace("&uuml;", "ü")
				.Replace("&ouml;", "ö");
		}
	}
}