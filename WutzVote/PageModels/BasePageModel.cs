using FreshMvvm;
using PropertyChanged;

namespace WutzVote
{
	[ImplementPropertyChanged]
	public class BasePageModel : FreshBasePageModel
	{
		public bool Loading { get; set; }
	}
}
