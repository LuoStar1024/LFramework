using LFramework;

namespace GameLogic
{
    public class EventGroupUI
    {
        public EventGroupUI()
        {
            GameEntry.Event.RegisterGroup(this);
        }

        public static readonly int ReturnMenuId = EventRuntimeId.ToRuntimeId("EventGroupUI.ReturnMenuId");

        public void ReturnMenu()
        {
            GameEntry.Event.Fire(ReturnMenuId);
        }
    }
}