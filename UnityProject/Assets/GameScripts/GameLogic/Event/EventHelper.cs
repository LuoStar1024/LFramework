using UnityEngine;

namespace GameLogic
{
    public static class EventHelper
    {
        private static EventGroupLogic _logic;
        private static EventGroupUI _ui;

        public static void OnInit()
        {
            _logic = new EventGroupLogic();
            _ui = new EventGroupUI();
        }

        public static void OnDestroy()
        {
            _logic = null;
            _ui = null;
        }
    }
}