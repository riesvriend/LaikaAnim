using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchrony
{
    public static class SynchronyUtils
    {
        // https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object
        public static bool IsClassOrSubclass<TPotentialBase>(this Type potentialDescendant)
        {
            var potentialBase = typeof(TPotentialBase);
            return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        }

        public static void Log(this string text)
        {
            Debug.Log(text); // Output to Unity console
            System.Diagnostics.Debug.WriteLine(text); // Output to VS.NET
        }

        public static bool IsPlayingInEditor()
        {
            return Application.isEditor && Application.isPlaying;
        }
    }
}