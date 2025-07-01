using System.Windows;
using TracesTesCamions.Models;

namespace TracesTesCamions.Views
{
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(Camion camion)
        {
            InitializeComponent();
            DataContext = camion;
        }
    }
}