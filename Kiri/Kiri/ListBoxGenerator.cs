using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kiri
{
    class ListBoxGenerator
    {
        public ListBox listBoxItem;

        public ListBoxGenerator() { }

        public ListBox getListBoxItem(Searchresult s) {
            ListBox item = new ListBox();
            item.Width = System.Windows.Application.Current.Host.Content.ActualWidth;
            item.Height = System.Windows.Application.Current.Host.Content.ActualHeight;
            item.Background = new SolidColorBrush(Colors.Black);
            item.FontSize = 30;
            item.DataContext = s;
            //item.SelectionChanged += ListBoxSelectedPlaceFrom;
            return item;
        }
    }
}
