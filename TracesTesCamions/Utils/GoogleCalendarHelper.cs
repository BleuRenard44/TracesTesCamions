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
    public static class GoogleCalendarHelper
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
            using (FileStream stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
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
        public static async Task<string> AddRevisionEventAsync(string nom, string plaque, DateTime date, TimeSpan? heure)
        {
            var service = await GetGoogleCalendarServiceAsync();

            string summary = $"Révision : {nom} ({plaque})";
            var startDateTime = date.Date + (heure ?? TimeSpan.Zero);
            var endDateTime = startDateTime.AddHours(1);

            var @event = new Event()
            {
                Summary = summary,
                Start = new EventDateTime()
                {
                    DateTime = startDateTime,
                    TimeZone = "Europe/Paris",
                },
                End = new EventDateTime()
                {
                    DateTime = endDateTime,
                    TimeZone = "Europe/Paris",
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new[]
                    {
                        new EventReminder { Method = "popup", Minutes = 10080 },
                        new EventReminder { Method = "popup", Minutes = 4320 },
                        new EventReminder { Method = "popup", Minutes = 1440 },
                        new EventReminder { Method = "popup", Minutes = 0 }
                    }
                }
            };

            try
            {
                var createdEvent = await service.Events.Insert(@event, "primary").ExecuteAsync();
                Console.WriteLine($"Événement Google (avec heure) créé avec succès. ID : {createdEvent.Id}");
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