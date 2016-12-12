using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PropertyChanged;
using RestSharp;
using Xamarin.Forms;
using System.Linq;
using System.Text;

namespace WutzVote
{
	[ImplementPropertyChanged]
	public class LoginPageModel : BasePageModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public Command LoginCommand { get; set; }

		private static readonly Regex rxBwId =
			new Regex(@"v_bw_id=(?<id>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly RestClient _restClient;
		private readonly SessionSettings _sessionSettings;

		public LoginPageModel(RestClient restClient, SessionSettings sessionSettings)
		{
			_restClient = restClient;
			_sessionSettings = sessionSettings;

			SetupCommands();
		}

		private void SetupCommands()
		{
			LoginCommand = new Command(async () =>
			{
				string festivalID = await Login();

				if (festivalID != null)
				{
					_sessionSettings.FestivalID = festivalID;
					await CoreMethods.PushPageModel<BandsPageModel>();
				}
			});
		}

		protected override void ViewIsAppearing(object sender, EventArgs e)
		{
			base.ViewIsAppearing(sender, e);

			Username = Acr.Settings.Settings.Local.Get("Username", string.Empty);
			Password = Acr.Settings.Settings.Local.Get("Password", string.Empty);
		}

		private async Task SaveLogin()
		{
			if (Username == Acr.Settings.Settings.Local.Get("Username", string.Empty) &&
				Password == Acr.Settings.Settings.Local.Get("Password", string.Empty))
			{
				// if user or password have not changed, dont's show the save question and continue
				return;
			}

			bool saveLogin = await CoreMethods.DisplayAlert(
				"WutzVote",
				"Sollen deine Login Daten gespeichert werden?",
				"Ja",
				"Nein");

			if (saveLogin)
			{
				Acr.Settings.Settings.Local.Set("Username", Username);
				Acr.Settings.Settings.Local.Set("Password", Password);
			}
		}

		private async Task<string> Login()
		{
			using (new LoadingContext(this))
			{
				try
				{
					RestRequest request = new RestRequest(Method.POST);
					request.AddParameter("user_id", Username);
					request.AddParameter("pass", Password);
					request.AddParameter("remember", "yes");
					request.AddParameter("login", "login");
					request.AddParameter("submit", "Login");

					IRestResponse response = await _restClient.ExecuteTaskAsync(request);
					Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");
					string html = iso_8859_1.GetString(response.RawBytes);

					MatchCollection matches = rxBwId.Matches(html);

					// bwId is the identifier for the Festival (eg. 17th Wutzdog)
					int bwId = matches
						.Cast<Match>()
						.Select(m => int.Parse(m.Groups["id"].Value))
						.DefaultIfEmpty(0)
						.Max();

					if (bwId == 0)
					{
						throw new Exception("Festivalticker ID oder Passwort falsch. Versuche es noch einmal.");
					}

					await SaveLogin();

					return Convert.ToString(bwId);
				}
				catch (Exception ex)
				{
					await CoreMethods.DisplayAlert("Error", ex.Message, "OK");
				}
			}

			return null;
		}
	}
}
