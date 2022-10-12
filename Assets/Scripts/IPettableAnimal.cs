using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PPR
{
    public abstract class PettableAnimal : MonoBehaviour, IPettableAnimal 
    {
        public abstract void OnStartPetting();
        public abstract void OnStopPetting();
    }

    public interface IPettableAnimal
    {
        abstract void OnStartPetting();
        abstract void OnStopPetting();
    }
}
