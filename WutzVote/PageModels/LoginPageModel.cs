using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using Xamarin.Forms;
using System.Linq;
using System.Text;
using Acr.Settings;

namespace WutzVote
{
	public class LoginPageModel : BasePageModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public Command LoginCommand { get; set; }

		private static readonly Regex rxBwId =
			new Regex("v_bw_id=(?<v_bw_id>\\d+)\"\\s+title=\"(?<name>[^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
				Festival festival = await Login();

				if (festival != null)
				{
					_sessionSettings.Festival = festival;
					await CoreMethods.PushPageModel<BandsPageModel>();
				}
			});
		}

		protected override void ViewIsAppearing(object sender, EventArgs e)
		{
			base.ViewIsAppearing(sender, e);
			LoadLogin();
		}

		private void LoadLogin()
		{
			Username = CrossSettings.Current.Get("Username", string.Empty);
			Password = CrossSettings.Current.Get("Password", string.Empty);
		}

		private async Task PromptSaveLogin()
		{
			if (LoginChanged())
			{
				bool saveLogin = await CoreMethods.DisplayAlert(
					"WutzVote",
					"Sollen deine Login Daten gespeichert werden?",
					"Ja",
					"Nein");

				if (saveLogin)
				{
					SaveLogin();
				}
			}
		}

		private bool LoginChanged()
		{
			return
				(Username != CrossSettings.Current.Get("Username", string.Empty) ||
				 Password != CrossSettings.Current.Get("Password", string.Empty));
		}

		private void SaveLogin()
		{
            CrossSettings.Current.Set("Username", Username);
            CrossSettings.Current.Set("Password", Password);
		}

		private async Task<Festival> Login()
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

                    // bwId is the identifier for the Festival (eg. 17th Wutzdog)
                    Festival latest =
                        rxBwId.Matches(html)
                        .Cast<Match>()
                        .Select(match => new Festival
                        {
                            ID = int.Parse(match.Groups["v_bw_id"].Value),
                            Name = match.Groups["name"].Value.Replace("Festival", string.Empty)
						})
						.OrderByDescending(festival => festival.ID)
					  	.FirstOrDefault();

					if (latest == null)
					{
						throw new Exception("Festivalticker ID oder Passwort falsch. Versuche es noch einmal.");
					}

					await PromptSaveLogin();

					return latest;
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
