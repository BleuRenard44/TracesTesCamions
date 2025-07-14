using Microsoft.Graph.Models;
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
            LoadEntreprises();
        }

        private void LoadEntreprises()
        {
            if (File.Exists(fichierPath))
            {
                try
                {
                    var json = File.ReadAllText(fichierPath);
                    var arr = JsonSerializer.Deserialize<Entreprise[]>(json);
                    if (arr != null)
                    {
                        Entreprises.Clear();
                        foreach (var e in arr) Entreprises.Add(e);
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des entreprises : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveEntreprises()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Entreprises.ToArray(), options);
            File.WriteAllText(fichierPath, json);
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EntrepriseDialog();
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                Entreprises.Add(dialog.Result);
                SaveEntreprises();
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
                    SaveEntreprises();
                }
            }
        }

        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if (ListEntreprises.SelectedItem is Entreprise sel)
            {
                Entreprises.Remove(sel);
                SaveEntreprises();
            }
        }
    }
}
