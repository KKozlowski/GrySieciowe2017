using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingCamera : MonoBehaviour {

    public Transform target;

    private void Update() {
        if (target == null && CharacterController.Player != null)
            target = CharacterController.Player.transform;

        if (target == null)
            return;

        Vector3 trackedPosition = target.position + new Vector3(0, 0, -10);

        Vector2 mousePos = Input.mousePosition;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 screenCenter = screenSize * 0.5f;
        Vector3 offset = (mousePos - screenCenter);
        offset.x /= screenSize.x;
        offset.y /= screenSize.y;
        offset *= 4;

        transform.position = Vector3.Lerp(transform.position, trackedPosition + offset, 0.05f);
    }
}
