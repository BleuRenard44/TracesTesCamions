using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TracesTesCamions.Models;

namespace TracesTesCamions.Views
{
    public partial class ModifierCamionWindow : Window
    {
        private Camion _camion;
        private string? selectedImagePath;

        public ModifierCamionWindow(Camion camion)
        {
            InitializeComponent();
            _camion = camion;
            DataContext = _camion;

            ChargerImageDepuisChemin();
        }

        private void ChargerImageDepuisChemin()
        {
            try
            {
                if (!string.IsNullOrEmpty(_camion.CheminDocument))
                {
                    string dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TracesTesCamions");
                    string fullPath = Path.Combine(dataFolder, _camion.CheminDocument);

                    if (File.Exists(fullPath))
                    {
                        BitmapImage bitmap = new();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ImagePreview.Source = bitmap;
                    }
                }
            }
            catch
            {
                // image non affichée
            }
        }

        private void ChangerImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Sélectionner une image",
                Filter = "Fichiers image (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;

                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedImagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImagePreview.Source = bitmap;
            }
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                string docsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TracesTesCamions");
                Directory.CreateDirectory(docsPath);

                string fileName = Path.GetFileName(selectedImagePath);
                string destPath = Path.Combine(docsPath, fileName);

                try
                {
                    File.Copy(selectedImagePath, destPath, true);
                    _camion.CheminDocument = fileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de la copie de l'image : " + ex.Message);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
