using Monopoly.CrossBridge;
using UnityEngine;

namespace Monopoly.UnityAdapter
{
    public class UnityEventBus : IEventBridge
    {
        public void Publish(string eventName, object payload)
        {
            Debug.Log($"📢 Event: {eventName} | Data: {JsonUtility.ToJson(payload)}");
        }
    }
}
