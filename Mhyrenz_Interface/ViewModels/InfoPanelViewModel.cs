using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.ViewModels
{
    public class InfoPanelViewModel: BaseViewModel
    {
        private readonly IInventoryStore _inventoryStore;

        public ObservableCollection<InfoCard> InfoCards { get; set; } = new ObservableCollection<InfoCard>();

        public InfoPanelViewModel(IInventoryStore inventoryStore)
        {
            _inventoryStore = inventoryStore;
            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;
            _inventoryStore.Loaded += InventoryStore_Loaded;

            LoadCards(
                sales: (double)_inventoryStore.Products.Sum(p => p.NetRetailPrice),
                profit: (double)_inventoryStore.Products.Sum(p => p.Item.Profit),
                purchase: _inventoryStore.Products.Sum(p => p.Purchase));
        }


        public override void Dispose()
        {
            _inventoryStore.PurchaseEvent -= InventoryStore_PurchaseEvent;
            _inventoryStore.Loaded -= InventoryStore_Loaded;
        }

        private void LoadCards(double sales, double profit, int purchase)
        {
            InfoCards.Add(new InfoCard
            {
                Type = InfoCard.CardType.Sales,
                Heading = "Today's Sales",
                Content = sales.ToString("C")
            });
            InfoCards.Add(new InfoCard
            {
                Type = InfoCard.CardType.Profit,
                Heading = "Total Profit",
                Content = profit.ToString("C")
            });
            InfoCards.Add(new InfoCard
            {
                Type = InfoCard.CardType.Purchases,
                Heading = "Purchases",
                Content = purchase.ToString("N0")
            });
        }

        private void InventoryStore_PurchaseEvent(object sender, InventoryStoreEventArgs e)
        {
            RefreshCards();
        }

        private void RefreshCards()
        {
            foreach (var item in InfoCards)
            {
                switch (item.Type)
                {
                    case InfoCard.CardType.Sales:
                        item.Content = ((double)_inventoryStore.Products.Sum(p => p.NetRetailPrice)).ToString("C");
                        break;
                    case InfoCard.CardType.Profit:
                        item.Content = ((double)_inventoryStore.Products.Sum(p => p.Item.Profit)).ToString("C");
                        break;
                    case InfoCard.CardType.Purchases:
                        item.Content = _inventoryStore.Products.Sum(p => p.Purchase).ToString("N0");
                        break;
                    default:
                        break;
                }
            }
        }

        private void InventoryStore_Loaded()
        {
            RefreshCards();
        }
    }

    public class InfoCard: BaseViewModel
    {
        public enum CardType
        {
            Sales,
            Profit,
            Purchases
        }

        public CardType Type { get; set; }
        public string Heading { get; set; }
        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
    }
}
