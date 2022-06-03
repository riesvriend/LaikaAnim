using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using System;

 
namespace MalbersAnimations.Utilities
{

    public class TriggerTarget : MonoBehaviour 
    {
        public List<TriggerProxy> Proxies;
        public Collider m_collider;

        private void Start() => hideFlags = HideFlags.HideInInspector;

        private void OnDisable()
        {
            if (Proxies != null)
                foreach (var p in Proxies)
                {
                    if (p != null) p.TriggerExit(m_collider, false);
                }

            Proxies = new List<TriggerProxy>();     //Reset
        }

        public void AddProxy(TriggerProxy trigger,Collider col)
        {
            if (Proxies == null) Proxies = new List<TriggerProxy>();
            if (!Proxies.Contains(trigger)) Proxies.Add(trigger);

            m_collider = col;
        }

        public void RemoveProxy(TriggerProxy trigger)
        {
            if (Proxies.Contains(trigger)) Proxies.Remove(trigger);
        }

    }
}