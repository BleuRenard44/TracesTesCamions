using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using TracesTesCamions.Models;

namespace TracesTesCamions.Data
{
    public class ConfigurationData
    {
        public ObservableCollection<string> MarquesVehicule { get; set; } = new();
        public ObservableCollection<string> MarquesPneumatique { get; set; } = new();
        public ObservableCollection<Entreprise> Entreprises { get; set; } = new();
        public ObservableCollection<string> EntreprisesMaintenance { get; set; } = new();
        public ObservableCollection<string> TypesDepense { get; set; } = new();
        public string? CurrentDataFolder { get; set; }

        private readonly string folder;

        private string baseFolder;
        private string entreprisesFilePath;
        private string marquesVehiculeFilePath;
        private string marquesPneuFilePath;
        private string typesDepenseFilePath;

        private static readonly string AppConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TracesTesCamions"
        );

        private static readonly string ConfigFilePath = Path.Combine(AppConfigDir, "config.json");

        private class AppConfiguration
        {
            public string? DataFolder { get; set; }
        }


        public ConfigurationData(string? dataFolder = null)
        {
            var config = LoadAppConfig();

            if (!string.IsNullOrWhiteSpace(dataFolder))
            {
                config.DataFolder = dataFolder;
                SaveAppConfig(config);
            }

            if (string.IsNullOrWhiteSpace(config.DataFolder) || !Directory.Exists(config.DataFolder))
            {
                if (!ChoisirDossierDonnees(out string? selectedFolder))
                {
                    System.Windows.MessageBox.Show("Aucun dossier sélectionné. L'application va se fermer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                config.DataFolder = selectedFolder;
                SaveAppConfig(config);
            }

            baseFolder = config.DataFolder!;
            folder = baseFolder;
            CurrentDataFolder = baseFolder;

            // Prépare les chemins fichiers
            entreprisesFilePath = Path.Combine(baseFolder, "entreprises.json");
            marquesVehiculeFilePath = Path.Combine(baseFolder, "marquesVehicule.json");
            marquesPneuFilePath = Path.Combine(baseFolder, "marquesPneu.json");
            typesDepenseFilePath = Path.Combine(baseFolder, "typesDepense.json");

            InitFiles();
            LoadAll();
        }


        private AppConfiguration LoadAppConfig()
        {
            if (!Directory.Exists(AppConfigDir))
                Directory.CreateDirectory(AppConfigDir);

            if (!File.Exists(ConfigFilePath))
                return new AppConfiguration();

            try
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
            }
            catch
            {
                return new AppConfiguration();
            }
        }

        private void SaveAppConfig(AppConfiguration config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la sauvegarde de la configuration : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void InitFiles()
        {
            // entreprises.json : crée un tableau JSON vide si le fichier est absent ou vide
            if (!File.Exists(entreprisesFilePath) || string.IsNullOrWhiteSpace(File.ReadAllText(entreprisesFilePath)))
            {
                File.WriteAllText(entreprisesFilePath, "[]");
            }

            // marquesVehicule.json
            if (!File.Exists(marquesVehiculeFilePath))
            {
                var defaultMarquesVehicule = new[]
                {
                    "Seat", "Renault", "Peugeot", "Dacia", "Citroën", "Opel", "Alfa Romeo", "Škoda", "Chevrolet",
                    "Porsche", "Honda", "Subaru", "Mazda", "Mitsubishi", "Lexus", "Toyota", "BMW", "Volkswagen",
                    "Suzuki", "Mercedes-Benz", "Saab", "Audi", "Kia", "Land Rover", "Dodge", "Chrysler", "Ford",
                    "Hummer", "Hyundai", "Infiniti", "Jaguar", "Jeep", "Nissan", "Volvo", "Daewoo", "Fiat", "MINI",
                    "Rover", "Smart"
                };
                File.WriteAllText(marquesVehiculeFilePath, JsonSerializer.Serialize(defaultMarquesVehicule, new JsonSerializerOptions { WriteIndented = true }));
            }

            // marquesPneu.json
            if (!File.Exists(marquesPneuFilePath))
            {
                var defaultMarquesPneu = new[]
                {
                    "Michelin", "Goodyear", "Bridgestone", "Continental"
                };
                File.WriteAllText(marquesPneuFilePath, JsonSerializer.Serialize(defaultMarquesPneu, new JsonSerializerOptions { WriteIndented = true }));
            }

            // typesDepense.json
            if (!File.Exists(typesDepenseFilePath))
            {
                var defaultTypesDepense = new[]
                {
                    "Pneumatique", "Réparation", "Vidange", "Freins"
                };
                File.WriteAllText(typesDepenseFilePath, JsonSerializer.Serialize(defaultTypesDepense, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private void Load(string filePath, ObservableCollection<string> target)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<string[]>(json);
                if (data != null)
                {
                    target.Clear();
                    foreach (var item in data)
                        target.Add(item);
                }
            }
            catch (JsonException ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la lecture du fichier {Path.GetFileName(filePath)} : {ex.Message}", "Erreur JSON", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateEntreprisesMaintenance()
        {
            EntreprisesMaintenance.Clear();
            foreach (var entreprise in Entreprises)
            {
                if (!string.IsNullOrWhiteSpace(entreprise.Nom))
                    EntreprisesMaintenance.Add(entreprise.Nom);
            }
        }


        private void LoadEntreprises()
        {
            if (!File.Exists(entreprisesFilePath))
                return;

            try
            {
                var json = File.ReadAllText(entreprisesFilePath);
                var data = JsonSerializer.Deserialize<Entreprise[]>(json);
                if (data != null)
                {
                    Entreprises.Clear();
                    foreach (var en in data)
                        Entreprises.Add(en);
                    PopulateEntreprisesMaintenance();
                }
            }
            catch (JsonException ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la lecture du fichier entreprises.json : {ex.Message}", "Erreur JSON", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save(string filePath, ObservableCollection<string> source)
        {
            try
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(source, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la sauvegarde du fichier {Path.GetFileName(filePath)} : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveEntreprises()
        {
            try
            {
                File.WriteAllText(entreprisesFilePath, JsonSerializer.Serialize(Entreprises, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la sauvegarde du fichier entreprises.json : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadAll()
        {
            Load(marquesVehiculeFilePath, MarquesVehicule);
            Load(marquesPneuFilePath, MarquesPneumatique);
            LoadEntreprises();
            Load(typesDepenseFilePath, TypesDepense);
        }

        public void SaveAll()
        {
            Save(marquesVehiculeFilePath, MarquesVehicule);
            Save(marquesPneuFilePath, MarquesPneumatique);
            SaveEntreprises();
            Save(typesDepenseFilePath, TypesDepense);
        }

        public static bool ChoisirDossierDonnees(out string? dossier)
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
            {
                dossier = dlg.SelectedPath;
                return true;
            }

            dossier = null;
            return false;
        }
    }
}
