using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTeleportToChild : MonoBehaviour
{
    public Transform teleportTargetChild;
    public Vector3 teleportOffset = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var playerObject = other.gameObject;

            var thirdPersonContollerScript = other.GetComponent<ThirdPersonController>();
            thirdPersonContollerScript.TeleportTo(teleportTargetChild, teleportOffset); 
        }
    }
}
