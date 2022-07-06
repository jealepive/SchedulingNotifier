namespace SchedulingNotifier.Core
{
    public class AppSettings
    {
        public string MainUrl { get; set; }
        public string AppointmentUrlPath { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
    }
}