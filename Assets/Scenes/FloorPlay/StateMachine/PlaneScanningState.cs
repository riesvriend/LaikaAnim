using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Synchrony;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

// TODO: Make sure our animal does not collide with real objects on the floor
public class PlaneScanningState : IState
{
    const float minimumGroundPlaneSizeInM2 = 1.5f;

    private GameObject planeScanningCanvas;
    private GameObject animalToPlacePrefab;
    private GameObject carpetToPlacePrefab;
    private GameObject placedAnimal;
    private GameObject placedCarpet;

    private ARPlaneManager arPlaneManager;
    private List<ARPlane> planes;
    private ARPlane floorPlane;

    public PlaneScanningState(GameObject planeScanningCanvas, GameObject animalToPlacePrefab, GameObject carpetToPlacePrefab)
    {
        this.planeScanningCanvas = planeScanningCanvas;
        this.animalToPlacePrefab = animalToPlacePrefab;
        this.carpetToPlacePrefab = carpetToPlacePrefab;
    }

    public void EnterState()
    {
        "<<< PlaneScanningState EnterState".Log();
        planeScanningCanvas.SetActive(true);

        // reset the environment scanner, which may still have planes from a previous
        var session = Object.FindObjectsOfType<ARSession>().Single();
        session.Reset();

        arPlaneManager = Object.FindObjectsOfType<ARPlaneManager>().Single();
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        arPlaneManager.planesChanged += ArPlaneManager_planesChanged;
        this.planes = new List<ARPlane>();
        floorPlane = null;
    }

    public void ExitState()
    {
        "<<< PlaneScanningState ExitState".Log();
        planeScanningCanvas.SetActive(false);
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
        arPlaneManager.planesChanged -= ArPlaneManager_planesChanged;
    }

    public void ExecuteState()
    {
        "<<< PlaneScanningState ExecuteState".Log();
    }

    private void ArPlaneManager_planesChanged(ARPlanesChangedEventArgs e)
    {
        if (e.added?.Count > 0)
            planes.AddRange(e.added);

        if (e.removed?.Count > 0)
            foreach (var p in e.removed)
                planes.Remove(p);

        if (floorPlane == null)
        {
            var potentialGroundPlanes = planes.Where(p => p.extents.x * p.extents.y > minimumGroundPlaneSizeInM2).ToList();
            // Ground is the lowest plane
            floorPlane = potentialGroundPlanes.OrderBy(p => p.transform.position.y).FirstOrDefault();

            if (floorPlane != null && placedAnimal == null)
            {
                /*
                Floor plane position: (-0.4, -0.4, 2.3)
                Floor plane extents: (0.8, 2.0)
                Floor plane rotation: (0.0, -0.1, 0.0, 1.0)
                Floor plane local scale: (1.0, 1.0, 1.0) 
                */
                $"Floor plane position: {floorPlane.transform.position}".Log();
                $"Floor plane extents: {floorPlane.extents}".Log();
                $"Floor plane rotation: {floorPlane.transform.rotation}".Log();
                $"Floor plane local scale: {floorPlane.transform.localScale}".Log();

                placedAnimal = GameObject.Instantiate(animalToPlacePrefab);

                // Rotate the animal to face the camera
                // https://www.youtube.com/watch?v=kGykP7VZCvg&list=LL&index=3
                var camera = Object.FindObjectOfType<Camera>();
                var projectedCameraForward = Vector3.ProjectOnPlane(vector: camera.transform.forward, planeNormal: floorPlane.transform.up);
                var rotationToCamera = Quaternion.LookRotation(forward: -projectedCameraForward, upwards: Vector3.up);
                placedAnimal.transform.rotation = Quaternion.RotateTowards(
                    from: placedAnimal.transform.rotation, to: rotationToCamera, maxDegreesDelta: 360);

                placedCarpet = GameObject.Instantiate(carpetToPlacePrefab);

                //var arOrigin = Object.FindObjectsOfType<ARSessionOrigin>().Single();
                //arOrigin.MakeContentAppearAt(placedCarpet.transform, position: new Vector3(0, 0, 0));

                // Fit the carpet on the plane (ignoring the fact that we are rotating it to face the camera later on...)
                placedCarpet.transform.position = floorPlane.transform.position;
                placedCarpet.transform.localScale = new Vector3(x: floorPlane.extents.x, y: placedCarpet.transform.localScale.y, z: floorPlane.extents.y);

                // TODO: make carpet overlay the plane
                //placedCarpet.transform.rotation = floorPlane.transform.rotation;
                // Make carpet rotation face the camera
                placedCarpet.transform.rotation = Quaternion.RotateTowards(
                    from: placedAnimal.transform.rotation, to: rotationToCamera, maxDegreesDelta: 360);

                placedAnimal.transform.position = floorPlane.transform.position; // floorPlane.center;

                arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
            }
        };
    }
}
