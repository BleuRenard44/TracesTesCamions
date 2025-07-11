using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
                string credPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TracesTesCamions", "GoogleToken");
                Directory.CreateDirectory(Path.GetDirectoryName(credPath)!);

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
                System.Windows.MessageBox.Show($"Erreur Google OAuth : {ex.Message}", "Erreur Google", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// Supprime un événement du calendrier Google par son ID.
        /// </summary>
        public static async Task DeleteEventAsync(string eventId)
        {
            var service = await GetGoogleCalendarServiceAsync();
            string calendarId = "primary";

            try
            {
                var calendarService = await GetGoogleCalendarServiceAsync();
                string userCalendarId = "primary";

                // Récupère l'événement
                var getRequest = calendarService.Events.Get(userCalendarId, eventId);
                var existingEvent = await getRequest.ExecuteAsync();

                // Supprime l'événement
                var deleteRequest = calendarService.Events.Delete(userCalendarId, eventId);
                await deleteRequest.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                // Ignore si l’événement est introuvable
                if (ex.HttpStatusCode != System.Net.HttpStatusCode.NotFound)
                    throw;
            }
        }

        public static async Task EnsureGoogleAuthAsync()
        {
            await GetGoogleCalendarServiceAsync();
        }

        public static async Task<bool> UpdateEventAsync(string eventId, string nom, string plaque, DateTime date, TimeSpan? heure)
        {
            try
            {
                var service = await GetGoogleCalendarServiceAsync();

                // Récupère l'événement existant
                var existingEvent = await service.Events.Get("primary", eventId).ExecuteAsync();
                if (existingEvent == null) return false;

                // Modifie les champs
                existingEvent.Summary = $"Révision - {nom} ({plaque})";

                if (heure.HasValue)
                {
                    var dateTime = date.Date + heure.Value;
                    existingEvent.Start = new EventDateTime { DateTime = dateTime, TimeZone = "Europe/Paris" };
                    existingEvent.End = new EventDateTime { DateTime = dateTime.AddHours(1), TimeZone = "Europe/Paris" };
                }
                else
                {
                    existingEvent.Start = new EventDateTime { Date = date.ToString("yyyy-MM-dd") };
                    existingEvent.End = new EventDateTime { Date = date.ToString("yyyy-MM-dd") };
                }

                // Met à jour l'événement
                await service.Events.Update(existingEvent, "primary", eventId).ExecuteAsync();
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                throw;
            }
        }


    }
}