using Microsoft.Azure.WebJobs;
using SchedulingNotifier.Core;
using System.Threading.Tasks;

namespace SchedulingNotifier.Api
{
    public class NotifierTimeTrigger
    {
        private readonly IAppointmentService _appointmentService;

        public NotifierTimeTrigger(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [FunctionName("NotifierTimeTrigger")]
        public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
        {
            await _appointmentService.Check().ConfigureAwait(false);
        }
    }
}