using System;
namespace WutzVote
{
	public class LoadingContext : IDisposable
	{
		private readonly BasePageModel _pageModel;

		public LoadingContext(BasePageModel pageModel)
		{
			_pageModel = pageModel;

			_pageModel.Loading = true;
		}

		public void Dispose()
		{
			_pageModel.Loading = false;
		}
	}
}
