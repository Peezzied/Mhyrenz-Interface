namespace Mhyrenz_Interface.Core.MVVM
{
    public abstract class FlyoutViewModel : BaseViewModel
    {
        protected FlyoutViewModel(string title)
        {
            FlyoutTitle = title;
        }

        public string FlyoutTitle { get; }

        //public ICommand CloseFlyoutCommand { get; }
    }
}
