using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingLaika : MonoBehaviour
{
    readonly Vector3 rootMotionDistanceInWalkClip = new Vector3(0, 0, 0.6851512f);

    [SerializeField]
    private AnimancerComponent _Animancer;
    public AnimancerComponent Animancer => _Animancer;

    [SerializeField] private ClipTransition _Walk;
    private bool _WasMoving;
    protected bool IsMoving;


    protected virtual void Awake()
    {
        // Start paused at the beginning of the animation.
        _Animancer.Play(_Walk);
        _Animancer.Evaluate();
        _Animancer.Playable.PauseGraph();

        _Walk.Events.OnEnd = () =>
        {
            var root = transform.GetChild(0);

            //var deltaPosition = root.position; deltaPosition.y = 0;

            var deltaPosition = rootMotionDistanceInWalkClip;

            root.position -= deltaPosition;
            Debug.Log($"Remaining root x: {root.position.x} z: {root.position.z}");

            transform.position += deltaPosition;
            Debug.Log($"Transform y: {transform.position.y}");

            _Walk.State.NormalizedTime = 0;
        };

        IsMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            if (!_WasMoving)
            {
                _WasMoving = true;

                // Make sure the graph is unpaused (because we pause it when going back to sleep).
                _Animancer.Playable.UnpauseGraph();
                _Animancer.Play(_Walk);
            }
        }
    }
}
