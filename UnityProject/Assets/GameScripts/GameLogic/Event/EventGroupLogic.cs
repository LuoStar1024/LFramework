using LFramework;

namespace GameLogic
{
    public class EventGroupLogic
    {
        public EventGroupLogic()
        {
            GameEntry.Event.RegisterGroup(this);
        }

        public static readonly int FireClickId = EventRuntimeId.ToRuntimeId("EventGroupLogic.FireClickId");

        public void FireClick()
        {
            GameEntry.Event.Fire(FireClickId);
        }
    }
}