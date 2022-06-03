using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Input/Mouse World Position")]

    public class MouseToWorld : MonoBehaviour 
    {
        public TransformReference MainCamera;
        public TransformReference MousePoint;
        public LayerReference layer = new LayerReference(-1);
        public QueryTriggerInteraction interaction = QueryTriggerInteraction.UseGlobal;
        public FloatReference MaxDistance = new FloatReference( 100f);

        private Camera m_camera;

        private void Start()
        {
            if (MainCamera.Value == null)
            {
                m_camera = MTools.FindMainCamera();

                if (m_camera)
                {
                    MainCamera = m_camera.transform;
                }
                else
                {
                    Debug.LogWarning("There's no Main Camera on the Scene");
                    enabled = false;
                }
            }
            else
            {
                m_camera = MainCamera.Value.GetComponent<Camera>();
                if (m_camera == null)
                {
                    Debug.LogWarning("There's no Main Camera on the Scene");
                    enabled = false;
                }
            }

            if (MousePoint.Value == null)
            {
                Debug.LogWarning("There's no Mouse Point Reference");
                enabled = false;
            }
        }


        private void Update()
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, MaxDistance, layer, interaction))
            {
                MousePoint.Value.position = hit.point;
            }
        }


        private void Reset()
        {
            MousePoint.Value = transform;
        }
    }
}