using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    Plane ground;

    private void Awake() {
        ground = new Plane(Vector3.back, Vector3.zero);
    }

    void Update () {
        UpdateMovement();

        if (Input.GetMouseButtonDown(0)) {
            UpdateShot(Input.mousePosition);
        }
    }

    private void UpdateMovement() {
        Vector2 direction = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) {
            direction += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S)) {
            direction += Vector2.down;
        }
        if (Input.GetKey(KeyCode.A)) {
            direction += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D)) {
            direction += Vector2.right;
        }

        direction.Normalize();

        Move(direction);
    }

    private void Move(Vector2 direction) {

        //#if DEBUG_SHIT
        //if (CharacterController.Player)
        //    CharacterController.Player
        //        .MoveTo((Vector2)CharacterController.Player.transform.position 
        //                + direction * Time.deltaTime*2);
        //#endif

        if (CharacterController.Player && CharacterController.Player.gameObject.activeInHierarchy) {
            InputEvent e = new InputEvent();
            e.m_sessionId = Network.Client.ConnectionId;
            e.m_direction = direction;

            Network.Client.Send(e);
        }
        
    }

    private void UpdateShot(Vector3 pointerPosition) {
        if (CharacterController.Player == null)
            return;

        Vector2 playerPosition = CharacterController.Player.transform.position;
        Ray cameraRay = Camera.main.ScreenPointToRay(pointerPosition);
        float rayDist = 0;
        ground.Raycast(cameraRay, out rayDist);

        Vector2 targetPosition = cameraRay.origin + cameraRay.direction * rayDist;

        Vector2 direction = (targetPosition - playerPosition).normalized;
        Shoot(direction);
    }

    private void Shoot(Vector2 direction) {
        ShotEvent se = new ShotEvent();
        se.m_direction = direction;
        se.m_who = Network.Client.ConnectionId;
        se.m_reliableEventId = Network.Client.GetNewReliableEventId();
        Network.Client.Send(se); Network.Client.Send(se);

        Debug.DrawRay(CharacterController.Player.transform.position, direction);
#if DEBUG_SHIT
        if (CharacterController.Player)
            CharacterController.Player
                .Shoot(direction);
        #endif
    }
}
