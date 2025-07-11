using System;
using System.Collections.Generic;

namespace TracesTesCamions.Models
{
    public class Camion
    {
        public string Plaque { get; set; } = "";
        public string TypeFinancement { get; set; } = "";
        public string TypeVehicule { get; set; } = "";
        public string Marque { get; set; } = "";
        public string Nom { get; set; } = "";
        public string TypeEnergie { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public string TypeDepense { get; set; } = "";
        public string MarquePneumatique { get; set; } = "";
        public string TypeMaintenance { get; set; } = "";
        public DateTime DateAchat { get; set; }
        public DateTime DateDerniereRevision { get; set; }
        public DateTime DateProchaineRevision { get; set; }
        public string? CalendarEventId { get; set; }
        public TimeSpan? HeureProchaineRevision { get; set; }
        public string EntrepriseMaintenance { get; set; } = "";
        public string CheminDocument { get; set; } = "";


        // Historique des révisions
        public List<DateTime> HistoriqueRevisions { get; set; } = new List<DateTime>();
    }
}