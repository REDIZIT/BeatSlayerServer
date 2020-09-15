using BeatSlayerServer.Services;
using Newtonsoft.Json;

namespace BeatSlayerServer.Controllers
{
    public class EventController
    {
        private readonly EventService eventService;

        public EventController(EventService eventService)
        {
            this.eventService = eventService;
        }

        public string GetMultiplayerEventResults()
        {
            return eventService.GetMultiplayerEventResults();
        }

        public string OnMultiplayerEventPlayed(string nick)
        {
            eventService.OnPlayerPlay(nick);
            return "Done";
        }

        public string GetStartAndEndEventTimes()
        {
            return JsonConvert.SerializeObject(eventService.GetStartAndEndEventTimes());
        }
    }
}
