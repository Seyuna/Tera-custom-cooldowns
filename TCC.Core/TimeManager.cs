﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TCC.Data;
using TCC.Settings;
using TCC.ViewModels;
using TCC.Windows;

namespace TCC
{
    public class TimeManager : TSPropertyChanged
    {
        private readonly Dictionary<string, TeraServerTimeInfo> _serverTimezones = new Dictionary<string, TeraServerTimeInfo>
        {
            {"EU", new TeraServerTimeInfo("Central Europe Standard Time", 6, DayOfWeek.Wednesday) },
            {"NA", new TeraServerTimeInfo("Central Standard Time", 6, DayOfWeek.Tuesday) },
            {"RU", new TeraServerTimeInfo("Russian Standard Time", 6, DayOfWeek.Wednesday) },
            {"TW", new TeraServerTimeInfo("China Standard Time", 6, DayOfWeek.Wednesday) },
            {"JP", new TeraServerTimeInfo("Tokyo Standard Time", 6, DayOfWeek.Wednesday) },
            {"THA", new TeraServerTimeInfo("Indochina Time", 6, DayOfWeek.Wednesday) },
            {"KR", new TeraServerTimeInfo("Korea Standard Time", 6, DayOfWeek.Wednesday) },
            {"KR-PTS", new TeraServerTimeInfo("Korea Standard Time", 6, DayOfWeek.Wednesday) }
        };

        public const double SecondsInDay = 60 * 60 * 24;
        private const string BaseUrl = "https://tcc-web-99a64.firebaseapp.com/bam";

        private static TimeManager _instance;
        private DayOfWeek _resetDay;
        public int ResetHour;
        public static TimeManager Instance => _instance ?? (_instance = new TimeManager());
        public string CurrentRegion { get; set; }
        public int ServerHourOffsetFromLocal;
        public int ServerHourOffsetFromUtc;

        public DateTime CurrentServerTime => DateTime.Now.AddHours(ServerHourOffsetFromLocal);

        private TimeManager()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            var s = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            s.Tick += CheckNewDay;
            s.Start();
        }

        private void CheckCloseEvents()
        {
            var closeEventsCount = WindowManager.Dashboard.VM.EventGroups.Count(evGroup => evGroup.Events.Any(x => x.IsClose));
            if (closeEventsCount == 0) return;
            if(SettingsHolder.ShowNotificationBubble) WindowManager.FloatingButton.StartNotifying(closeEventsCount);

        }

        private void CheckNewDay(object sender, EventArgs e)
        {
            if (CurrentServerTime.Hour == 0 && CurrentServerTime.Minute == 0)
                WindowManager.Dashboard.VM.LoadEvents(CurrentServerTime.DayOfWeek, CurrentRegion);
            if (CurrentServerTime.Second == 0 && CurrentServerTime.Minute % 3 == 0) CheckCloseEvents();
        }

        public void SetServerTimeZone(string region)
        {
            if (string.IsNullOrEmpty(region)) return;
            CurrentRegion = region.StartsWith("EU") ? "EU" : region;

            SettingsHolder.LastRegion = region;
            if (!_serverTimezones.ContainsKey(CurrentRegion))
            {
                CurrentRegion = "EU";
                SettingsHolder.LastRegion = "EU-EN";
                TccMessageBox.Show("TCC",
                    "Current region could not be detected, so TCC will load EU-EN database. To force a specific language, use Region Override setting in Misc Settings.",
                    MessageBoxButton.OK);
            }
            var timezone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.Id == _serverTimezones[CurrentRegion].Timezone);
            ResetHour = _serverTimezones[CurrentRegion].ResetHour;
            _resetDay = _serverTimezones[CurrentRegion].ResetDay;

            if (timezone != null)
            {
                ServerHourOffsetFromUtc = timezone.IsDaylightSavingTime(DateTime.UtcNow + timezone.BaseUtcOffset)
                    ? timezone.BaseUtcOffset.Hours + 1
                    : timezone.BaseUtcOffset.Hours;
                ServerHourOffsetFromLocal = -TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours + ServerHourOffsetFromUtc;
            }

            if (WindowManager.Dashboard.VM.Markers.FirstOrDefault(x => x.Name.Equals(CurrentRegion + " server time")) == null)
            {
                WindowManager.Dashboard.VM.Markers.Add(new TimeMarker(ServerHourOffsetFromLocal, CurrentRegion + " server time"));
            }

            CheckReset();
            WindowManager.Dashboard.VM.LoadEvents(DateTime.Now.DayOfWeek, CurrentRegion);

        }

        private void CheckReset()
        {
            if (CurrentRegion == null) return;
            var todayReset = DateTime.Today.AddHours(ResetHour + ServerHourOffsetFromLocal);
            if (SettingsHolder.LastRun > todayReset || DateTime.Now < todayReset) return;
            foreach (var ch in WindowManager.Dashboard.VM.Characters)
            {
                foreach (var dg in ch.Dungeons)
                {
                    if (dg.Dungeon.Id == 9950)
                    {
                        if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday) dg.Reset();
                        else continue;
                    }
                    dg.Reset();
                }
                ch.VanguardDailiesDone = 0;
                ch.ClaimedGuardianQuests = 0;
                if (DateTime.Now.DayOfWeek == _resetDay)
                {
                    ch.VanguardWeekliesDone = 0;
                }
            }
            SettingsHolder.LastRun = DateTime.Now;
            WindowManager.Dashboard.VM.SaveCharacters();
            SettingsWriter.Save();
            if (DateTime.Now.DayOfWeek == _resetDay)
            {
                ChatWindowManager.Instance.AddTccMessage("Weekly data has been reset.");
            }

            ChatWindowManager.Instance.AddTccMessage("Daily data has been reset.");
        }


        private async Task<long> DownloadGuildBamTimestamp()
        {
            try
            {
                var sb = new StringBuilder(BaseUrl);
                sb.Append("?srv=");
                sb.Append(SessionManager.Server.ServerId);
                sb.Append("&reg=");
                sb.Append(CurrentRegion);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var c = new WebClient();
                c.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");

                var data = await c.DownloadStringTaskAsync(sb.ToString());
                return long.Parse(data);

            }
            catch
            {

                ChatWindowManager.Instance.AddTccMessage("Failed to retrieve guild BAM info.");
                WindowManager.FloatingButton.NotifyExtended("Guild BAM", "Failed to retrieve guild BAM info.", NotificationType.Error);

                return 0;
            }
        }

        public void UploadGuildBamTimestamp()
        {
            var sb = new StringBuilder(BaseUrl);
            sb.Append("?srv=");
            sb.Append(SessionManager.Server.ServerId);
            sb.Append("&reg=");
            sb.Append(CurrentRegion);
            sb.Append("&post");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var c = new WebClient();
            c.Headers.Set("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");

            try
            {
                c.UploadDataAsync(new Uri(sb.ToString()), new byte[] { });
            }
            catch
            {
                ChatWindowManager.Instance.AddTccMessage("Failed to upload guild BAM info.");
                WindowManager.FloatingButton.NotifyExtended("Guild BAM", "Failed to upload guild BAM info.", NotificationType.Error);

            }

        }
        public DateTime RetrieveGuildBamDateTime()
        {
            var t = DownloadGuildBamTimestamp();
            t.Wait();
            var ts = t.Result;
            return Utils.FromUnixTime(ts);
        }

        public void SetGuildBamTime(bool force)
        {
            foreach (var eg in WindowManager.Dashboard.VM.EventGroups.ToSyncArray().Where(x => x.RemoteCheck))
            {
                foreach (var ev in eg.Events.ToSyncArray())
                {
                    ev.UpdateFromServer(force);
                }
            }
        }

        public void SendWebhookMessageOld(bool testMessage = false)
        {
            if (!string.IsNullOrEmpty(SettingsHolder.Webhook))
            {
                var sb = new StringBuilder("{");
                sb.Append("\""); sb.Append("content"); sb.Append("\"");
                sb.Append(":");
                sb.Append("\""); sb.Append(SettingsHolder.WebhookMessage);
                if (testMessage) sb.Append(" (Test message)");
                sb.Append("\"");
                sb.Append(",");
                sb.Append("\""); sb.Append("username"); sb.Append("\"");
                sb.Append(":");
                sb.Append("\""); sb.Append("TCC"); sb.Append("\"");
                sb.Append(",");
                sb.Append("\""); sb.Append("avatar_url"); sb.Append("\"");
                sb.Append(":");
                sb.Append("\""); sb.Append("http://i.imgur.com/8IltuVz.png"); sb.Append("\"");
                sb.Append("}");

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                        client.UploadString(SettingsHolder.Webhook, "POST", sb.ToString());
                    }
                }
                catch (Exception)
                {
                    WindowManager.FloatingButton.NotifyExtended("Guild BAM", "Failed to execute Discord webhook.", NotificationType.Error);
                    ChatWindowManager.Instance.AddTccMessage("Failed to execute Discord webhook.");
                }
            }
        }
        public void SendWebhookMessage(string bamName)
        {
            if (!string.IsNullOrEmpty(SettingsHolder.Webhook))
            {
                var msg = SettingsHolder.WebhookMessage.IndexOf("{npc_name}", StringComparison.Ordinal) > -1
                    ? SettingsHolder.WebhookMessage.Replace("{npc_name}", bamName)
                    : SettingsHolder.WebhookMessage;
                var sb = new StringBuilder("{");
                sb.Append("\""); sb.Append("content"); sb.Append("\"");
                sb.Append(":");
                sb.Append("\""); sb.Append(msg); sb.Append("\"");
                sb.Append(",");
                sb.Append("\""); sb.Append("username"); sb.Append("\"");
                sb.Append(":");
                sb.Append("\""); sb.Append("TCC"); sb.Append("\"");
                sb.Append(",");
                sb.Append("\""); sb.Append("avatar_url"); sb.Append("\"");
                sb.Append(":");
                sb.Append("\""); sb.Append("http://i.imgur.com/8IltuVz.png"); sb.Append("\"");
                sb.Append("}");

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    using (var client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");

                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        client.UploadString(SettingsHolder.Webhook, "POST", sb.ToString());
                    }
                }
                catch (Exception)
                {
                    WindowManager.FloatingButton.NotifyExtended("Guild BAM", "Failed to execute Discord webhook.", NotificationType.Error);
                    ChatWindowManager.Instance.AddTccMessage("Failed to execute Discord webhook.");
                }
            }
        }

    }
}
