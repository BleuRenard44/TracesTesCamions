using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using TracesTesCamions.Views;

namespace TracesTesCamions.Views
{
    public partial class GererMarquesWindow : Window
    {
        public ObservableCollection<string> MarquesVehicule { get; }
        public ObservableCollection<string> MarquesPneumatique { get; }

        private readonly string fileVehPath;
        private readonly string filePneuPath;

        public GererMarquesWindow(
            ObservableCollection<string> marquesVeh,
            ObservableCollection<string> marquesPneu,
            string currentDataFolder)
        {
            InitializeComponent();
            MarquesVehicule = marquesVeh;
            MarquesPneumatique = marquesPneu;
            ListMarquesVehicule.ItemsSource = MarquesVehicule;
            ListMarquesPneumatique.ItemsSource = MarquesPneumatique;
            fileVehPath = Path.Combine(currentDataFolder, "marquesVehicule.json");
            filePneuPath = Path.Combine(currentDataFolder, "marquesPneu.json");
            LoadExisting();
        }

        private void LoadExisting()
        {
            if (File.Exists(fileVehPath))
            {
                var arr = JsonSerializer.Deserialize<string[]>(File.ReadAllText(fileVehPath));
                if (arr != null)
                {
                    MarquesVehicule.Clear();
                    foreach (var s in arr) MarquesVehicule.Add(s);
                }
            }
            if (File.Exists(filePneuPath))
            {
                var arr = JsonSerializer.Deserialize<string[]>(File.ReadAllText(filePneuPath));
                if (arr != null)
                {
                    MarquesPneumatique.Clear();
                    foreach (var s in arr) MarquesPneumatique.Add(s);
                }
            }
        }

        private void SaveAll()
        {
            File.WriteAllText(fileVehPath, JsonSerializer.Serialize(MarquesVehicule));
            File.WriteAllText(filePneuPath, JsonSerializer.Serialize(MarquesPneumatique));
        }

        private void AjouterVehicule_Click(object sender, RoutedEventArgs e)
            => PromptAdd(MarquesVehicule, "Nouvelle marque véhicule");

        private void SupprimerVehicule_Click(object sender, RoutedEventArgs e)
            => PromptRemove(ListMarquesVehicule, MarquesVehicule);

        private void AjouterPneu_Click(object sender, RoutedEventArgs e)
            => PromptAdd(MarquesPneumatique, "Nouvelle marque pneumatique");

        private void SupprimerPneu_Click(object sender, RoutedEventArgs e)
            => PromptRemove(ListMarquesPneumatique, MarquesPneumatique);

        private void PromptAdd(ObservableCollection<string> coll, string title)
        {
            var input = PromptDialog.Show(title, "");
            if (!string.IsNullOrWhiteSpace(input) && !coll.Contains(input))
            {
                coll.Add(input);
                SaveAll();
            }
        }

        private void PromptRemove(ListBox listBox, ObservableCollection<string> coll)
        {
            if (listBox.SelectedItem is string sel)
            {
                coll.Remove(sel);
                SaveAll();
            }
        }
    }
}
