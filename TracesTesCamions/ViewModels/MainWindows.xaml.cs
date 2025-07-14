using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TracesTesCamions.Data;
using TracesTesCamions.Models;
using TracesTesCamions.Utils;
using TracesTesCamions.Views;

namespace TracesTesCamions
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Camion> camions = new ObservableCollection<Camion>();
        private ConfigurationData? config;
        private string? currentDataFolder = null;
        private readonly string configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TracesTesCamions", "config.txt");

        public bool IsNouveauxVisible { get; set; } = true;
        public bool IsHistoriqueVisible => !IsNouveauxVisible;

        public ObservableCollection<string> FichiersJson { get; } = new ObservableCollection<string>();

        public ObservableCollection<Camion> Camions => camions;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadDataFolder();
            if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
            {
                System.Windows.MessageBox.Show(
                    "Aucun dossier de données n'est défini. Veuillez choisir un dossier pour enregistrer vos données.",
                    "Sélection du dossier", MessageBoxButton.OK, MessageBoxImage.Information);
                NouveauJeuDeDonnees_Click(this, new RoutedEventArgs());
            }
            // Ajoute cette ligne pour charger les véhicules existants
            ChargerVehiculesDepuisFichiers();
        }

        private void LoadDataFolder()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    config = JsonSerializer.Deserialize<ConfigurationData>(json);
                    currentDataFolder = config?.CurrentDataFolder;
                }
            }
            catch
            {
                // Ignorer les erreurs de lecture
                config = new ConfigurationData(currentDataFolder ?? "");
            }
        }


        private void SaveDataFolder()
        {
            try
            {
                config = new ConfigurationData(currentDataFolder ?? "");
                config.CurrentDataFolder = currentDataFolder;

                var dir = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir!);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(configPath, JsonSerializer.Serialize(config, options));
            }
            catch
            {
                // Ignorer les erreurs d'écriture
            }
        }


        private void BtnNouveaux_Click(object sender, RoutedEventArgs e)
        {
            PopupHistorique.IsOpen = false;
            PopupNouveaux.IsOpen = true;
            IsNouveauxVisible = true;
            OnPropertyChanged(nameof(IsNouveauxVisible));
            OnPropertyChanged(nameof(IsHistoriqueVisible));
        }

        private void BtnHistorique_Click(object sender, RoutedEventArgs e)
        {
            PopupNouveaux.IsOpen = false;
            PopupHistorique.IsOpen = true;
            IsNouveauxVisible = false;
            OnPropertyChanged(nameof(IsNouveauxVisible));
            OnPropertyChanged(nameof(IsHistoriqueVisible));
            ChargerFichiersJson();
        }

        // Nouveau jeu de données : crée un dossier et sauvegarde le chemin
        private void NouveauJeuDeDonnees_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Choisissez ou créez le dossier pour le nouveau jeu de données"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                currentDataFolder = dlg.SelectedPath;
                SaveDataFolder();
                System.Windows.MessageBox.Show($"Dossier sélectionné : {currentDataFolder}", "Nouveau jeu de données", MessageBoxButton.OK, MessageBoxImage.Information);
                ChargerVehiculesDepuisFichiers();
            }
        }

        // Nouveau véhicule : ouvre la popup, puis enregistre en JSON
        private async void NouveauVehicule_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
            {
                System.Windows.MessageBox.Show(
                    "Aucun dossier de données n'est défini. Veuillez choisir un dossier pour enregistrer vos données.",
                    "Sélection du dossier", MessageBoxButton.OK, MessageBoxImage.Information);
                NouveauJeuDeDonnees_Click(this, new RoutedEventArgs());
                return;
            }

            var win = new NouveauCamionWindow(currentDataFolder, config) { Owner = this };
            if (win.ShowDialog() == true && win.Result != null)
            {
                // Ajoute la date de révision à l’historique
                if (!win.Result.HistoriqueRevisions.Contains(win.Result.DateDerniereRevision))
                    win.Result.HistoriqueRevisions.Add(win.Result.DateDerniereRevision);

                camions.Add(win.Result);

                if (string.IsNullOrEmpty(currentDataFolder))
                {
                    System.Windows.MessageBox.Show("Veuillez d'abord créer ou sélectionner un jeu de données.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Enregistre le véhicule en JSON
                string fileName = Path.Combine(currentDataFolder, $"{win.Result.Plaque}.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(fileName, JsonSerializer.Serialize(win.Result, options));
                System.Windows.MessageBox.Show($"Véhicule enregistré dans :\n{fileName}", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ajoute un événement de révision au calendrier
                string eventId = await GoogleCalendarHelper.AddRevisionEventAsync(
                    win.Result.Nom, win.Result.Plaque, win.Result.DateProchaineRevision, win.Result.HeureProchaineRevision);
                win.Result.CalendarEventId = eventId;

                ChargerFichiersJson();
            }
        }

        private void ModifierVehicule_Click(object sender, RoutedEventArgs e)
        {
            if (VehiculesGrid.SelectedItem is Camion selectedCamion)
            {
                // Ouvre une fenêtre pour saisir les nouvelles dates
                var dialog = new ModifierDatesWindow(selectedCamion, selectedCamion.DateDerniereRevision, selectedCamion.DateProchaineRevision) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    selectedCamion.DateDerniereRevision = dialog.NouvelleDerniereRevision;
                    selectedCamion.DateProchaineRevision = dialog.NouvelleProchaineRevision;

                    // Sauvegarde la modification dans le fichier JSON
                    if (!string.IsNullOrEmpty(currentDataFolder))
                    {
                        string fileName = Path.Combine(currentDataFolder, $"{selectedCamion.Plaque}.json");
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        File.WriteAllText(fileName, JsonSerializer.Serialize(selectedCamion, options));
                    }
                    VehiculesGrid.Items.Refresh();
                }
            }
        }

        private void AjouterRevision_Click(object sender, RoutedEventArgs e)
        {
            if (VehiculesGrid.SelectedItem is Camion selectedCamion)
            {
                var dialog = new ModifierDatesWindow(selectedCamion, DateTime.Now, selectedCamion.DateProchaineRevision) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    // Ajoute la nouvelle date à l’historique
                    selectedCamion.HistoriqueRevisions.Add(dialog.NouvelleDerniereRevision);
                    // Met à jour la dernière et prochaine révision
                    selectedCamion.DateDerniereRevision = dialog.NouvelleDerniereRevision;
                    selectedCamion.DateProchaineRevision = dialog.NouvelleProchaineRevision;

                    // Sauvegarde la modification dans le fichier JSON
                    if (!string.IsNullOrEmpty(currentDataFolder))
                    {
                        string fileName = Path.Combine(currentDataFolder, $"{selectedCamion.Plaque}.json");
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        File.WriteAllText(fileName, JsonSerializer.Serialize(selectedCamion, options));
                    }
                    VehiculesGrid.Items.Refresh();
                }
            }
        }

        private async void SupprimerVehicule_Click(object sender, RoutedEventArgs e)
        {
            if (VehiculesGrid.SelectedItem is Camion selectedCamion)
            {
                if (System.Windows.MessageBox.Show($"Supprimer le véhicule {selectedCamion.Plaque} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    camions.Remove(selectedCamion);
                    // Supprime le fichier JSON associé
                    if (!string.IsNullOrEmpty(currentDataFolder))
                    {
                        string fileName = Path.Combine(currentDataFolder, $"{selectedCamion.Plaque}.json");
                        if (File.Exists(fileName))
                            File.Delete(fileName);
                    }

                    // Supprime l'événement du calendrier si nécessaire
                    if (!string.IsNullOrEmpty(selectedCamion.CalendarEventId))
                        await GoogleCalendarHelper.DeleteEventAsync(selectedCamion.CalendarEventId);

                    VehiculesGrid.Items.Refresh();
                }
            }
        }

        private void VehiculesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ModifierVehicule_Click(sender, e);
        }

        private void ChargerFichiersJson()
        {
            FichiersJson.Clear();
            if (!string.IsNullOrEmpty(currentDataFolder) && Directory.Exists(currentDataFolder))
            {
                foreach (var file in Directory.GetFiles(currentDataFolder, "*.json"))
                {
                    FichiersJson.Add(System.IO.Path.GetFileName(file));
                }
            }
        }

        private void ChargerVehiculesDepuisFichiers()
        {
            camions.Clear();
            if (!string.IsNullOrEmpty(currentDataFolder) && Directory.Exists(currentDataFolder))
            {
                foreach (var file in Directory.GetFiles(currentDataFolder, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var camion = JsonSerializer.Deserialize<Camion>(json);
                        if (camion != null)
                            camions.Add(camion);
                    }
                    catch { /* Ignorer les erreurs de lecture individuelles */ }
                }
            }
        }

        private void FichierJson_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox?.SelectedItem is Camion selectedCamion)
            {
                // Affiche les infos dans une nouvelle fenêtre ou une page dédiée
                var detailsWindow = new DetailsWindow(selectedCamion, currentDataFolder) { Owner = this };
                detailsWindow.ShowDialog();
                
            }

            listBox.SelectedItem = null;
        }

        private void RechercheBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var filtre = RechercheBox.Text?.ToLowerInvariant() ?? "";
            VehiculesGrid.ItemsSource = string.IsNullOrWhiteSpace(filtre)
                ? camions
                : new ObservableCollection<Camion>(camions.Where(c =>
                    c.Plaque.ToLowerInvariant().Contains(filtre) ||
                    c.Nom.ToLowerInvariant().Contains(filtre) ||
                    c.Marque.ToLowerInvariant().Contains(filtre)));
        }

        private void ExporterCsv_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "vehicules.csv"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
                sw.WriteLine("Plaque;Marque;Nom;Année de création;Dernière révision;Prochaine révision");
                foreach (var c in camions)
                {
                    sw.WriteLine($"{c.Plaque};{c.Marque};{c.Nom};{c.DateCreation:yyyy};{c.DateDerniereRevision:dd/MM/yy};{c.DateProchaineRevision:dd/MM/yy}");
                }
                System.Windows.MessageBox.Show("Export terminé.", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string currentVersion = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?.ToString() ?? "0.0.0";

            try
            {
                var update = await UpdateChecker.CheckForUpdateAsync(currentVersion);

                if (update != null)
                {
                    var asset = update.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe") || a.Name.EndsWith(".zip"));
                    if (asset != null)
                    {
                        var result = System.Windows.MessageBox.Show(
                            $"Une mise à jour ({update.Tag_name}) est disponible.\n\nVoulez-vous l'installer maintenant ?",
                            "Mise à jour disponible",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = asset.Browser_download_url,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch
            {
                // Ne rien faire si la connexion échoue (mode silencieux)
            }

            try
            {
                await GoogleCalendarHelper.EnsureGoogleAuthAsync();
                System.Windows.MessageBox.Show("Connecté à Google avec succès.", "Authentification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur d'authentification Google : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FichierJson_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Camion selectedCamion)
            {
                try
                {
                    var detailsWindow = new DetailsWindow(selectedCamion, currentDataFolder)
                    {
                        Owner = this
                    };
                    detailsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Erreur lors de l'ouverture du camion : {ex.Message}");
                }
            }
        }

        private void GererEntreprises_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
            {
                System.Windows.MessageBox.Show("Veuillez d'abord choisir un dossier de données.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entreprises = new ObservableCollection<Entreprise>();
            var fichierPath = Path.Combine(currentDataFolder, "entreprises.json");
            if (File.Exists(fichierPath))
            {
                var data = JsonSerializer.Deserialize<Entreprise[]>(File.ReadAllText(fichierPath));
                if (data != null)
                {
                }
            }

            var entreprisesWindow = new GererEntreprisesWindow(entreprises, currentDataFolder);

            {
            };
            entreprisesWindow.ShowDialog();
            RechargerConfiguration();
        }

        private void GererMarques_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
            {
                System.Windows.MessageBox.Show("Veuillez d'abord choisir un dossier de données.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var marquesVeh = new ObservableCollection<string>();
            var marquesPneu = new ObservableCollection<string>();

            var fileVeh = Path.Combine(currentDataFolder, "marquesVehicule.json");
            var filePneu = Path.Combine(currentDataFolder, "marquesPneu.json");

            if (File.Exists(fileVeh))
            {
                var data = JsonSerializer.Deserialize<string[]>(File.ReadAllText(fileVeh));
                if (data != null)
                {
                    foreach (var m in data)
                        marquesVeh.Add(m);
                }
            }

            if (File.Exists(filePneu))
            {
                var data = JsonSerializer.Deserialize<string[]>(File.ReadAllText(filePneu));
                if (data != null)
                {
                    foreach (var m in data)
                        marquesPneu.Add(m);
                }
            }

            var marquesWindow = new GererMarquesWindow(marquesVeh, marquesPneu, currentDataFolder)
            {
                Owner = this
            };
            marquesWindow.ShowDialog();
            RechargerConfiguration();
        }

        private void GererTypesDepense_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
            {
                System.Windows.MessageBox.Show("Veuillez d'abord choisir un dossier de données.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var typesDepense = new ObservableCollection<string>();
            var fichierPath = Path.Combine(currentDataFolder, "typesDepense.json");
            if (File.Exists(fichierPath))
            {
                var data = JsonSerializer.Deserialize<string[]>(File.ReadAllText(fichierPath));
                if (data != null)
                {
                    foreach (var t in data)
                        typesDepense.Add(t);
                }
            }

            var window = new GererTypesDepenseWindow(typesDepense, currentDataFolder)
            {
                Owner = this
            };
            window.ShowDialog();

            RechargerConfiguration();
        }

        private void RechargerConfiguration()
        {
            if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
                return;

            config = new ConfigurationData(currentDataFolder);
            config.LoadAll();

            System.Windows.MessageBox.Show("Configuration rechargée avec succès.", "Chargement", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}