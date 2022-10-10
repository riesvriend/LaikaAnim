using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PPP
{
    public interface IPettableAnimal
    {
        GameObject gameObject { get;  }
        void OnStartPetting();
        void OnStopPetting();
    }
}
