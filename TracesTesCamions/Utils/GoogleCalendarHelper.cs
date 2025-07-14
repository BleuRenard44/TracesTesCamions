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
        private static string? _cachedCalendarId;
        private const string CalendarName = "TracesTesCamions";

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

        private static async Task<string> GetOrCreateCalendarIdAsync()
        {
            if (!string.IsNullOrEmpty(_cachedCalendarId))
                return _cachedCalendarId;

            var service = await GetGoogleCalendarServiceAsync();
            var calendarList = await service.CalendarList.List().ExecuteAsync();

            foreach (var calendar in calendarList.Items)
            {
                if (calendar.Summary == CalendarName)
                {
                    _cachedCalendarId = calendar.Id!;
                    return _cachedCalendarId;
                }
            }

            var newCalendar = new Calendar
            {
                Summary = CalendarName,
                TimeZone = "Europe/Paris"
            };

            var createdCalendar = await service.Calendars.Insert(newCalendar).ExecuteAsync();
            _cachedCalendarId = createdCalendar.Id!;
            return _cachedCalendarId;
        }

        public static async Task<string> AddRevisionEventAsync(
            string nomVehicule,
            string marque,
            string plaque,
            string entreprise,
            string typeMaintenance,
            string typeDepense,
            DateTime date,
            TimeSpan? heure)
        {
            var service = await GetGoogleCalendarServiceAsync();
            var calendarId = await GetOrCreateCalendarIdAsync();

            var startDateTime = date.Date + (heure ?? TimeSpan.Zero);
            var endDateTime = startDateTime.AddHours(1);

            var summary = $"Maintenance - {entreprise} : {nomVehicule} ({plaque})";

            var description =
$@"Véhicule : {nomVehicule}
Marque : {marque}
Plaque : {plaque}
Entreprise : {entreprise}
Type de maintenance : {typeMaintenance}
Type de dépense : {typeDepense}";

            var @event = new Event()
            {
                Summary = summary,
                Description = description,
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
                var createdEvent = await service.Events.Insert(@event, calendarId).ExecuteAsync();
                Console.WriteLine($"Événement ajouté avec ID : {createdEvent.Id}");
                return createdEvent.Id!;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de l'événement : {ex.Message}", "Erreur Google", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static async Task DeleteEventAsync(string eventId)
        {
            var service = await GetGoogleCalendarServiceAsync();
            var calendarId = await GetOrCreateCalendarIdAsync();

            try
            {
                await service.Events.Delete(calendarId, eventId).ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode != System.Net.HttpStatusCode.NotFound)
                    throw;
            }
        }

        public static async Task<bool> UpdateEventAsync(
            string eventId,
            string nomVehicule,
            string marque,
            string plaque,
            string entreprise,
            string typeMaintenance,
            string typeDepense,
            DateTime date,
            TimeSpan? heure)
        {
            try
            {
                var service = await GetGoogleCalendarServiceAsync();
                var calendarId = await GetOrCreateCalendarIdAsync();

                var existingEvent = await service.Events.Get(calendarId, eventId).ExecuteAsync();
                if (existingEvent == null) return false;

                var startDateTime = date.Date + (heure ?? TimeSpan.Zero);
                var endDateTime = startDateTime.AddHours(1);

                existingEvent.Summary = $"Maintenance - {entreprise} : {nomVehicule} ({plaque})";

                existingEvent.Description =
$@"Véhicule : {nomVehicule}
Marque : {marque}
Plaque : {plaque}
Entreprise : {entreprise}
Type de maintenance : {typeMaintenance}
Type de dépense : {typeDepense}";

                existingEvent.Start = new EventDateTime
                {
                    DateTime = startDateTime,
                    TimeZone = "Europe/Paris"
                };
                existingEvent.End = new EventDateTime
                {
                    DateTime = endDateTime,
                    TimeZone = "Europe/Paris"
                };

                await service.Events.Update(existingEvent, calendarId, eventId).ExecuteAsync();
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                throw;
            }
        }

        public static async Task EnsureGoogleAuthAsync()
        {
            await GetGoogleCalendarServiceAsync();
        }
    }
}
