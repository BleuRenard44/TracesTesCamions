using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TracesTesCamions.Models;
using TracesTesCamions.Utils;

namespace TracesTesCamions.Views
{
    public partial class ModifierDatesWindow : Window
    {
        private Camion _camion;

        public DateTime NouvelleDerniereRevision { get; private set; }
        public DateTime NouvelleProchaineRevision { get; private set; }
        public TimeSpan? NouvelleHeureProchaineRevision { get; private set; }

        public ModifierDatesWindow(Camion camion, DateTime derniere, DateTime prochaine)
        {
            InitializeComponent();
            _camion = camion;
            DerniereRevisionPicker.SelectedDate = derniere;
            ProchaineRevisionPicker.SelectedDate = prochaine;
            // Pré-remplit l'heure si déjà présente
            HeureProchaineRevisionBox.Text = _camion.HeureProchaineRevision?.ToString(@"hh\:mm") ?? "";
        }

        private async void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (DerniereRevisionPicker.SelectedDate == null || ProchaineRevisionPicker.SelectedDate == null)
            {
                MessageBox.Show("Veuillez saisir les deux dates.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NouvelleDerniereRevision = DerniereRevisionPicker.SelectedDate.Value;
            NouvelleProchaineRevision = ProchaineRevisionPicker.SelectedDate.Value;

            if (!string.IsNullOrWhiteSpace(HeureProchaineRevisionBox.Text))
            {
                if (!TimeSpan.TryParse(HeureProchaineRevisionBox.Text, out var heure))
                {
                    MessageBox.Show("Veuillez saisir une heure valide au format HH:mm.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                NouvelleHeureProchaineRevision = heure;
            }
            else
            {
                NouvelleHeureProchaineRevision = null;
            }

            _camion.DateDerniereRevision = NouvelleDerniereRevision;
            _camion.DateProchaineRevision = NouvelleProchaineRevision;
            _camion.HeureProchaineRevision = NouvelleHeureProchaineRevision;

            try
            {
                // Supprimer l'ancien événement s'il existe
                if (!string.IsNullOrEmpty(_camion.CalendarEventId))
                {
                    await GoogleCalendarHelper.DeleteEventAsync(_camion.CalendarEventId);
                }

                // Ajouter le nouvel événement
                string eventId = await GoogleCalendarHelper.AddRevisionEventAsync(
                    _camion.Nom,
                    _camion.Marque,
                    _camion.Plaque,
                    _camion.EntrepriseMaintenance,
                    _camion.TypeMaintenance,
                    _camion.TypeDepense,
                    _camion.DateProchaineRevision,
                    _camion.HeureProchaineRevision
                );

                _camion.CalendarEventId = eventId;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du calendrier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Ne ferme pas la fenêtre si erreur
            }

            DialogResult = true;
            Close();
        }

    }
}