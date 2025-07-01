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

        public ModifierDatesWindow(Camion camion, DateTime derniere, DateTime prochaine)
        {
            InitializeComponent();
            _camion = camion;
            DerniereRevisionPicker.SelectedDate = derniere;
            ProchaineRevisionPicker.SelectedDate = prochaine;
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

            // Mets à jour la date sur le camion
            _camion.DateDerniereRevision = NouvelleDerniereRevision;
            _camion.DateProchaineRevision = NouvelleProchaineRevision;

            // Mets à jour l'événement Outlook
            string eventId = await OutlookCalendarHelper.AddRevisionEventAsync(_camion.Nom, _camion.Plaque, _camion.DateProchaineRevision);
            _camion.CalendarEventId = eventId;

            DialogResult = true;
        }
    }
}