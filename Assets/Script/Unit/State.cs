using System;
using System.Collections.Generic;


namespace Unit
{
    public class State
    {
        private bool stateAcitve;
        public string name { get; private set; }

        public bool state
        {
            get { return stateAcitve; }
            set
            {
                if (stateAcitve == value) return;

                List<Action> targetEvents = value ? stateOnEvent : stateOffEvent;
                foreach (var action in targetEvents)
                {
                    action.Invoke();
                }

                stateAcitve = value;
            }
        }

        private List<Action> stateOnEvent;
        private List<Action> stateOffEvent; 


        public State(string stateName, bool defaultState = false)
        {
            name = stateName;
            stateAcitve = defaultState;

            stateOnEvent = new();
            stateOffEvent = new();
        }

        public void AddStateOnListener(Action newStateOnEvent)
        {
            if (stateOnEvent.Contains(newStateOnEvent)) return;
            stateOnEvent.Add(newStateOnEvent);
        }

        public void AddStateOffListener(Action newStateOffEvent)
        {
            if (stateOffEvent.Contains(newStateOffEvent)) return;
            stateOffEvent.Add(newStateOffEvent);
        }
    }
}