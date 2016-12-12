using System;
using FreshMvvm;
using RestSharp;
using Xamarin.Forms;

namespace WutzVote
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
			SetupIoC();

			var login = FreshPageModelResolver.ResolvePageModel<LoginPageModel>();
			var navContainer = new FreshNavigationContainer(login);
			MainPage = navContainer;
		}

		void SetupIoC()
		{
			var restClient = new RestClient("http://www.festivalticker.de/my_account/");
			restClient.CookieContainer = new System.Net.CookieContainer();
			FreshIOC.Container.Register<RestClient>(restClient);

			var sessionSettings = new SessionSettings();
			FreshIOC.Container.Register<SessionSettings>(sessionSettings);
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
