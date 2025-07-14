using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TracesTesCamions.Models;

namespace TracesTesCamions.Views
{
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(Camion camion, string dossierRacine)
        {
            InitializeComponent();
            DataContext = camion;

            if (!string.IsNullOrEmpty(camion.CheminDocument))
            {
                string imagePath = Path.Combine(dossierRacine, camion.CheminDocument);

                if (File.Exists(imagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        ImageCamion.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors du chargement de l'image : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Image non trouvée : " + imagePath, "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ModifierVehicule_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Camion camion)
            {
                // Ouvre une fenêtre de modification avec les infos du camion
                var fenetreModif = new ModifierCamionWindow(camion); // À créer
                fenetreModif.Owner = this;
                if (fenetreModif.ShowDialog() == true)
                {
                    // Recharger les données si elles ont été modifiées
                    DataContext = null;
                    DataContext = camion;
                }
            }
        }

    }
}
