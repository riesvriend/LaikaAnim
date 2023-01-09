using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synchrony;
using Oculus.Interaction;

public class SkinCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(
            $"OnCollisionEnter in {gameObject.FullName()} with {collision.gameObject.FullName()}"
        );
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log(
            $"OnCollisionExit in {gameObject.FullName()} with {collision.gameObject.FullName()}"
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter in {gameObject.FullName()} with {other.gameObject.FullName()}");

        var combHaptics = other.GetComponent<GrabbableObjectHaptics>();
        if (combHaptics != null)
        {
            //combHaptics.OnSkinTouchEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit in {gameObject.FullName()} with {other.gameObject.FullName()}");

        var combHaptics = other.GetComponent<GrabbableObjectHaptics>();
        if (combHaptics != null)
        {
            //combHaptics.OnSkinTouchExit();
        }
    }
}
