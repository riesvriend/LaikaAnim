using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oculus.Interaction
{
    public class PointableCanvasModulePatched: PointableCanvasModule
    {
        /// <summary>
        /// Patch for bug https://forums.oculusvr.com/t5/Unity-VR-Development/Pointable-Canvas-menu-in-Unity-stops-working-when-user-uses-both/td-p/997121
        /// Requires members accessed in this method to manually be made non-private
        /// </summary>
        public override void Process()
        {
            // Clone deleted pointers into in a local array to prevent
            // InvalidOperationException: Collection was modified; enumeration operation may not execute.
            var pointersToLoopOver = _pointersForDeletion.ToArray();
            // Clear the deleted pointers list so it can start collecting newly delete pointers if those occur as a side effect to this method
            _pointersForDeletion.Clear();
            foreach (Pointer pointer in pointersToLoopOver)
                ProcessPointer(pointer, forceRelease: true);

            // Clone current pointers into a local array to prevent
            // InvalidOperationException: Collection was modified; enumeration operation may not execute.
            pointersToLoopOver = _pointerMap.Values.ToArray();
            foreach (Pointer pointer in pointersToLoopOver)
                ProcessPointer(pointer);
        }
    }
}
