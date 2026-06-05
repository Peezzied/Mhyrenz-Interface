using System.Collections.ObjectModel;
using Mhyrenz_Interface.Features.Home.ViewModels;

namespace Mhyrenz_Interface.DesignTime
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
