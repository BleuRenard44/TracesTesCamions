using System;
using System.Collections.Generic;

namespace TracesTesCamions.Models
{
    public class Camion
    {
        public string Plaque { get; set; } = "";
        public string Marque { get; set; } = "";
        public string Nom { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public DateTime DateDerniereRevision { get; set; }
        public DateTime DateProchaineRevision { get; set; }
        public string? CalendarEventId { get; set; }

        // Historique des révisions
        public List<DateTime> HistoriqueRevisions { get; set; } = new List<DateTime>();
    }
}
