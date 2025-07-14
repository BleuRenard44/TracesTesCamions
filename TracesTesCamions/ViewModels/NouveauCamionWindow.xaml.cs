using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using TracesTesCamions.Data;
using TracesTesCamions.Models;

namespace TracesTesCamions.Views
{
    public partial class NouveauCamionWindow : Window
    {
        public Camion? Result { get; private set; }
        private string? cheminDocumentRelatif;
        private readonly string? currentDataFolder;
        private readonly ConfigurationData configData;

        public ObservableCollection<int> Annees { get; } = new ObservableCollection<int>();

        public NouveauCamionWindow(string? dataFolder, ConfigurationData config)
        {
            InitializeComponent();
            config.LoadAll();
            DataContext = this;

            currentDataFolder = dataFolder ?? throw new ArgumentNullException(nameof(dataFolder));
            configData = config;

            int anneeCourante = DateTime.Now.Year;
            for (int i = anneeCourante; i >= 1980; i--)
                Annees.Add(i);

            MarqueVehiculeBox.ItemsSource = configData.MarquesVehicule;
            AnneeCreationBox.ItemsSource = Annees;
            EntrepriseMaintenanceBox.ItemsSource = configData.EntreprisesMaintenance;
            MarquePneumatiqueBox.ItemsSource = configData.MarquesPneumatique;
            TypeDepenseBox.ItemsSource = configData.TypesDepense;

            if (configData.MarquesVehicule.Count > 0)
                MarqueVehiculeBox.SelectedIndex = 0;
            if (Annees.Count > 0)
                AnneeCreationBox.SelectedIndex = 0;
            if (configData.EntreprisesMaintenance.Count > 0)
                EntrepriseMaintenanceBox.SelectedIndex = 0;
            if (configData.MarquesPneumatique.Count > 0)
                MarquePneumatiqueBox.SelectedIndex = 0;
            if (configData.TypesDepense.Count > 0)
                TypeDepenseBox.SelectedIndex = 0;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var plaque = PlaqueBox.Text.Trim().ToUpperInvariant();
            if (!System.Text.RegularExpressions.Regex.IsMatch(plaque, @"^[A-Z]{2}-\d{3}-[A-Z]{2}$"))
            {
                MessageBox.Show("La plaque doit être au format AA-123-AA (2 lettres, 3 chiffres, 2 lettres).", "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PlaqueBox.Text) ||
                MarqueVehiculeBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NomBox.Text) ||
                AnneeCreationBox.SelectedItem == null ||
                !DateAchatPicker.SelectedDate.HasValue ||
                TypeEnergieBox.SelectedItem == null ||
                !DerniereRevisionPicker.SelectedDate.HasValue ||
                !ProchaineRevisionPicker.SelectedDate.HasValue ||
                TypeDepenseBox.SelectedItem == null ||
                MarquePneumatiqueBox.SelectedItem == null ||
                TypeMaintenanceBox.SelectedItem == null ||
                EntrepriseMaintenanceBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TimeSpan.TryParse(HeureProchaineRevisionBox.Text, out var heureProchaineRevision))
            {
                MessageBox.Show("Veuillez saisir une heure valide au format HH:mm.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string typeFinancement = FinancementAchat.IsChecked == true ? "Achat" :
                                    FinancementLeasing.IsChecked == true ? "Leasing" :
                                    FinancementLocation.IsChecked == true ? "Location" : "Achat";

            Result = new Camion
            {
                Plaque = plaque,
                TypeFinancement = typeFinancement,
                TypeVehicule = (TypeVehiculeBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                Marque = MarqueVehiculeBox.SelectedItem.ToString() ?? "",
                Nom = NomBox.Text.Trim(),
                DateCreation = new DateTime((int)AnneeCreationBox.SelectedItem, 1, 1),
                DateAchat = DateAchatPicker.SelectedDate.Value,
                TypeEnergie = (TypeEnergieBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                DateDerniereRevision = DerniereRevisionPicker.SelectedDate.Value,
                DateProchaineRevision = ProchaineRevisionPicker.SelectedDate.Value,
                HeureProchaineRevision = heureProchaineRevision,
                TypeDepense = TypeDepenseBox.SelectedItem.ToString() ?? "",
                MarquePneumatique = MarquePneumatiqueBox.SelectedItem.ToString() ?? "",
                TypeMaintenance = (TypeMaintenanceBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                EntrepriseMaintenance = EntrepriseMaintenanceBox.SelectedItem.ToString() ?? "",
                CheminDocument = cheminDocumentRelatif ?? ""
            };

            try
            {
                if (string.IsNullOrEmpty(currentDataFolder) || !Directory.Exists(currentDataFolder))
                {
                    MessageBox.Show("Le dossier de sauvegarde n'est pas valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string cheminFichier = Path.Combine(currentDataFolder, $"{Result.Plaque}.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Result, options);
                File.WriteAllText(cheminFichier, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }

        private void PlaqueBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null)
            {
                int selStart = tb.SelectionStart;
                string upper = tb.Text.ToUpperInvariant();
                if (tb.Text != upper)
                {
                    tb.Text = upper;
                    tb.SelectionStart = selStart;
                }
            }
        }

        private void BtnAddDocument_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Sélectionnez un document image"
            };

            bool? resultat = dlg.ShowDialog();

            if (resultat == true && currentDataFolder != null)
            {
                string dossierDoc = Path.Combine(currentDataFolder, "doc");

                if (!Directory.Exists(dossierDoc))
                    Directory.CreateDirectory(dossierDoc);

                // Récupérer la plaque en majuscules et sans espace
                string plaque = PlaqueBox.Text.Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(plaque))
                {
                    MessageBox.Show("Veuillez saisir la plaque avant d'ajouter un document.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string extension = Path.GetExtension(dlg.FileName);
                string destination = Path.Combine(dossierDoc, plaque + extension);

                // Si un fichier existe déjà avec ce nom, on ajoute un suffixe numérique
                int compteur = 1;
                while (File.Exists(destination))
                {
                    destination = Path.Combine(dossierDoc, $"{plaque}_{compteur}{extension}");
                    compteur++;
                }

                File.Copy(dlg.FileName, destination);

                cheminDocumentRelatif = Path.Combine("doc", Path.GetFileName(destination));

                TxtSelectedDocument.Text = $"Document ajouté : {Path.GetFileName(destination)}";
            }
            else if (currentDataFolder == null)
            {
                MessageBox.Show("Le dossier de sauvegarde n'est pas configuré.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RenameImageWithPlaque(string imagePath, string currentDataFolder, string plaque)
        {
            if (string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(currentDataFolder) || string.IsNullOrEmpty(plaque))
                return;

            if (!File.Exists(imagePath))
                return;

            string extension = Path.GetExtension(imagePath); // ex: ".jpg" ou ".png"
            string newFileName = plaque + extension;
            string newFilePath = Path.Combine(currentDataFolder, newFileName);

            File.Copy(imagePath, newFilePath, overwrite: true);
        }

    }
}
