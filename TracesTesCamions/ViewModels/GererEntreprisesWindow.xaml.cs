using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using TracesTesCamions.Models;
using TracesTesCamions.Utils;

namespace TracesTesCamions.Views
{
    public partial class GererEntreprisesWindow : Window
    {
        public ObservableCollection<Entreprise> Entreprises { get; }

        private readonly string fichierPath;

        public GererEntreprisesWindow(ObservableCollection<Entreprise> source, string currentDataFolder)
        {
            InitializeComponent();
            Entreprises = source;
            ListEntreprises.ItemsSource = Entreprises;
            fichierPath = Path.Combine(currentDataFolder, "entreprises.json");
            LoadExisting();
        }

        private void LoadExisting()
        {
            if (File.Exists(fichierPath))
            {
                var arr = JsonSerializer.Deserialize<List<Entreprise>>(File.ReadAllText(fichierPath));
                if (arr != null)
                {
                    Entreprises.Clear();
                    foreach (var e in arr) Entreprises.Add(e);
                }
            }
        }

        private void Save()
        {
            File.WriteAllText(fichierPath, JsonSerializer.Serialize(Entreprises));
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EntrepriseDialog();
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                Entreprises.Add(dialog.Result);
                Save();
            }
        }

        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            if (ListEntreprises.SelectedItem is Entreprise sel)
            {
                var dialog = new EntrepriseDialog(sel);
                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    int idx = Entreprises.IndexOf(sel);
                    Entreprises[idx] = dialog.Result;
                    Save();
                }
            }
        }

        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if (ListEntreprises.SelectedItem is Entreprise sel)
            {
                Entreprises.Remove(sel);
                Save();
            }
        }
    }
}
