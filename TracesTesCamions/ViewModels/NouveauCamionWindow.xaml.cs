using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TracesTesCamions.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace TracesTesCamions.Views
{
    public partial class NouveauCamionWindow : Window
    {
        public Camion? Result { get; private set; }
        public ObservableCollection<string> Marques { get; } = new ObservableCollection<string>
        {
            "Renault", "Volvo", "Scania", "Mercedes", "MAN", "Iveco", "DAF", "Autre"
        };
        public ObservableCollection<int> Annees { get; } = new ObservableCollection<int>();

        public NouveauCamionWindow()
        {
            InitializeComponent();
            DataContext = this;
            int anneeCourante = DateTime.Now.Year;
            for (int i = anneeCourante; i >= 1980; i--)
                Annees.Add(i);
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
                MarqueBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NomBox.Text) ||
                AnneeCreationBox.SelectedItem == null ||
                !DerniereRevisionPicker.SelectedDate.HasValue ||
                !ProchaineRevisionPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TimeSpan.TryParse(HeureProchaineRevisionBox.Text, out var heureProchaineRevision))
            {
                MessageBox.Show("Veuillez saisir une heure valide au format HH:mm.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Result = new Camion
            {
                Plaque = PlaqueBox.Text.Trim(),
                Marque = MarqueBox.SelectedItem.ToString()!,
                Nom = NomBox.Text.Trim(),
                DateCreation = new DateTime((int)AnneeCreationBox.SelectedItem, 1, 1),
                DateDerniereRevision = DerniereRevisionPicker.SelectedDate.Value,
                DateProchaineRevision = ProchaineRevisionPicker.SelectedDate.Value,
                HeureProchaineRevision = heureProchaineRevision
            };
            DialogResult = true;
        }
        private void PlaqueBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as Xceed.Wpf.Toolkit.MaskedTextBox;
            if (tb == null) return;

            int selStart = tb.SelectionStart;
            string upper = tb.Text.ToUpperInvariant();
            if (tb.Text != upper)
            {
                tb.Text = upper;
                tb.SelectionStart = selStart;
            }
        }
    }
}