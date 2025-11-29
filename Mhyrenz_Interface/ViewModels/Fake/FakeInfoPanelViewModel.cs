using System.Collections.ObjectModel;

namespace Mhyrenz_Interface.ViewModels.Fake
{
    public class FakeInfoPanelViewModel
    {
        public ObservableCollection<InfoCard> InfoCards { get; set; } = new ObservableCollection<InfoCard>();

        public FakeInfoPanelViewModel()
        {
            InfoCards.Add(new InfoCard
            {
                Heading = "Today's Sales",
                Content = "1000"
            });
            InfoCards.Add(new InfoCard
            {
                Heading = "Total Profit",
                Content = "1000"
            });
            InfoCards.Add(new InfoCard
            {
                Heading = "Purchases",
                Content = "1000"
            });
        }
    }
}
