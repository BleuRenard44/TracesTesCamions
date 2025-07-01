using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TracesTesCamions.Utils
{
    public static class OutlookCalendarHelper
    {
        private static CalendarService? _googleCalendarService;

        /// <summary>
        /// Initialise et retourne le service Google Calendar authentifié.
        /// </summary>
        private static async Task<CalendarService> GetGoogleCalendarServiceAsync()
        {
            if (_googleCalendarService != null)
                return _googleCalendarService;

            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            _googleCalendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TracesTesCamions",
            });

            return _googleCalendarService;
        }

        /// <summary>
        /// Ajoute un événement de révision au calendrier Google et retourne l'ID de l'événement.
        /// </summary>
        public static async Task<string> AddRevisionEventAsync(string nom, string plaque, DateTime date)
        {
            var service = await GetGoogleCalendarServiceAsync();

            // Format du titre
            string summary = $"Révision : {nom} ({plaque})";

            // Pour un événement sur la journée entière, utiliser la propriété Date (sans heure)
            var eventDate = date.Date.ToString("yyyy-MM-dd");

            var @event = new Event()
            {
                Summary = summary,
                Start = new EventDateTime()
                {
                    Date = eventDate,
                    TimeZone = "Europe/Paris",
                },
                End = new EventDateTime()
                {
                    // Google Calendar considère End comme exclusif, donc +1 jour
                    Date = date.Date.AddDays(1).ToString("yyyy-MM-dd"),
                    TimeZone = "Europe/Paris",
                },
            };

            try
            {
                var createdEvent = await service.Events.Insert(@event, "primary").ExecuteAsync();
                Console.WriteLine($"Événement Google (journée complète) créé avec succès. ID : {createdEvent.Id}");
                return createdEvent.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création de l'événement Google : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Supprime un événement du calendrier Google par son ID.
        /// </summary>
        public static async Task DeleteEventAsync(string eventId)
        {
            var service = await GetGoogleCalendarServiceAsync();

            try
            {
                await service.Events.Delete("primary", eventId).ExecuteAsync();
                Console.WriteLine($"Événement Google avec l'ID {eventId} supprimé avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression de l'événement Google : {ex.Message}");
                throw;
            }
        }
    }
}