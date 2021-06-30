using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Synchrony;
using System.Linq;

public class PlaneScanningState : IState
{
    private GameObject planeScanningCanvas;
    private GameObject animalToPlacePrefab;
    private GameObject placedAnimal;

    private ARPlaneManager arPlaneManager;
    private List<ARPlane> planes;
    private ARPlane floorPlane;
    [SerializeField] float minimumGroundPlaneSizeInM2 = 2.0f;

    public PlaneScanningState(GameObject planeScanningCanvas, GameObject animalToPlacePrefab)
    {
        this.planeScanningCanvas = planeScanningCanvas;
        this.animalToPlacePrefab = animalToPlacePrefab;
    }

    public void EnterState()
    {
        "<<< PlaneScanningState EnterState".Log();
        planeScanningCanvas.SetActive(true);
        arPlaneManager = Object.FindObjectsOfType<ARPlaneManager>().Single();
        this.planes = new List<ARPlane>();
        floorPlane = null;
    }

    public void ExecuteState()
    {
        "<<< PlaneScanningState ExecuteState".Log();
        arPlaneManager.planesChanged += ArPlaneManager_planesChanged;
    }

    private void ArPlaneManager_planesChanged(ARPlanesChangedEventArgs e)
    {
        if (e.added?.Count > 0)
            planes.AddRange(e.added);

        if (e.removed?.Count > 0)
            foreach (var p in e.removed)
            {
                planes.Remove(p);
            }

        if (floorPlane == null)
        {
            var potentialGroundPlanes = planes.Where(p => p.extents.x * p.extents.y > minimumGroundPlaneSizeInM2).ToList();
            // Ground is the lowest plane
            floorPlane = potentialGroundPlanes.OrderBy(p => p.transform.position.y).FirstOrDefault();

            if (floorPlane != null && placedAnimal == null)
            {
                placedAnimal = GameObject.Instantiate(animalToPlacePrefab);
                placedAnimal.transform.position = floorPlane.center;

                // Rotate the animal to face the camera
                // https://www.youtube.com/watch?v=kGykP7VZCvg&list=LL&index=3
                var camera = Object.FindObjectOfType<Camera>();
                var projectedCameraForward = -Vector3.ProjectOnPlane(camera.transform.forward, floorPlane.transform.up);
                var rotationToCamera = Quaternion.LookRotation(projectedCameraForward, Vector3.up);
                placedAnimal.transform.rotation = Quaternion.RotateTowards(from: placedAnimal.transform.rotation, to: rotationToCamera, maxDegreesDelta: 360);
            }
        };

    }

    public void ExitState()
    {
        "<<< PlaneScanningState ExitState".Log();
        planeScanningCanvas.SetActive(false);
        arPlaneManager.planesChanged -= ArPlaneManager_planesChanged;
    }
}
