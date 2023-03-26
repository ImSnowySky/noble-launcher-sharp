using System;
using System.Collections.Generic;
using NobleLauncher.Structures;

namespace NobleLauncher.Models
{
    class EventDispatcher
    {
        private static readonly Dictionary<EventDispatcherEvent, List<Action>> ActionHandlers = new Dictionary<EventDispatcherEvent, List<Action>>();

        public static void CreateSubscription(EventDispatcherEvent SubscribeTo, Action Action) {
            if (!ActionHandlers.ContainsKey(SubscribeTo)) {
                ActionHandlers[SubscribeTo] = new List<Action>();
            }
            ActionHandlers[SubscribeTo].Add(Action);
        }

        public static void RemoveSubscription(EventDispatcherEvent UnsubscribeFrom, Action Action) {
            var indexOfAction = ActionHandlers[UnsubscribeFrom].FindIndex(handler => handler.Equals(Action));
            if (indexOfAction == -1)
                return;

            ActionHandlers[UnsubscribeFrom].RemoveAt(indexOfAction);
        }

        public static void Dispatch(EventDispatcherEvent DispatchedEvent) {
            if (!ActionHandlers.ContainsKey(DispatchedEvent)) {
                Console.WriteLine("Dispatcher key not found: " + DispatchedEvent);
                return;
            }
            if (ActionHandlers[DispatchedEvent].Count == 0) {
                Console.WriteLine("Dispatcher handler not found: " + DispatchedEvent);
                return;
            }
            ActionHandlers[DispatchedEvent].ForEach(action => action());
        }

        public static void RemoveAllSubscriptionFromEvent(EventDispatcherEvent UnsubscribeFrom) {
            ActionHandlers[UnsubscribeFrom].Clear();
        }

        public static void RemoveAllSubscriptions() {
            ActionHandlers.Clear();
        }
    }
}
