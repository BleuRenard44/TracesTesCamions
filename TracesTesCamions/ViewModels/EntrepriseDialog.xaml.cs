using System.Windows;
using TracesTesCamions.Models;

namespace TracesTesCamions.Utils
{
    public partial class EntrepriseDialog : Window
    {
        public Entreprise Result { get; private set; }

        public EntrepriseDialog(Entreprise entreprise = null)
        {
            InitializeComponent();
            if (entreprise != null)
            {
                NomBox.Text = entreprise.Nom;
                TelephoneBox.Text = entreprise.Telephone;
                EmailBox.Text = entreprise.Email;
                AdresseBox.Text = entreprise.Adresse;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NomBox.Text))
            {
                Result = new Entreprise
                {
                    Nom = NomBox.Text.Trim(),
                    Telephone = TelephoneBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    Adresse = AdresseBox.Text.Trim()
                };
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Le nom de l'entreprise est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static Entreprise Prompt(Entreprise e = null)
        {
            var dlg = new EntrepriseDialog(e);
            return dlg.ShowDialog() == true ? dlg.Result : null;
        }
    }
}
