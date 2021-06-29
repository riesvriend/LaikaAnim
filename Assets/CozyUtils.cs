using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cozy
{
    public static class CozyUtils
    {
        // https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object
        public static bool IsClassOrSubclass<TPotentialBase>(this Type potentialDescendant)
        {
            var potentialBase = typeof(TPotentialBase);
            return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        }

    }
}