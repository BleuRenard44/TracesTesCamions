using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace TracesTesCamions.Views
{
    public partial class GererTypesDepenseWindow : Window
    {
        private readonly string dataFolder;
        public ObservableCollection<string> TypesDepense { get; set; }

        public GererTypesDepenseWindow(ObservableCollection<string> typesDepense, string dataFolder)
        {
            InitializeComponent();
            this.dataFolder = dataFolder;
            TypesDepense = typesDepense;
            ListTypesDepense.ItemsSource = TypesDepense;
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            var nom = Microsoft.VisualBasic.Interaction.InputBox("Nom du type de dépense :", "Ajouter un type de dépense", "");
            if (!string.IsNullOrWhiteSpace(nom) && !TypesDepense.Contains(nom))
            {
                TypesDepense.Add(nom);
                Sauvegarder();
            }
        }

        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            if (ListTypesDepense.SelectedItem is string selected)
            {
                var nouveauNom = Microsoft.VisualBasic.Interaction.InputBox("Modifier le type de dépense :", "Modifier", selected);
                if (!string.IsNullOrWhiteSpace(nouveauNom) && nouveauNom != selected && !TypesDepense.Contains(nouveauNom))
                {
                    int index = TypesDepense.IndexOf(selected);
                    TypesDepense[index] = nouveauNom;
                    Sauvegarder();
                }
            }
        }

        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if (ListTypesDepense.SelectedItem is string selected)
            {
                if (MessageBox.Show($"Supprimer « {selected} » ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    TypesDepense.Remove(selected);
                    Sauvegarder();
                }
            }
        }

        private void Sauvegarder()
        {
            string fichier = Path.Combine(dataFolder, "typesDepense.json");
            File.WriteAllText(fichier, JsonSerializer.Serialize(TypesDepense, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
