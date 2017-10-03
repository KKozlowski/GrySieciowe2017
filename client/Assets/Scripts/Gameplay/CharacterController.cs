using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

	public static CharacterController Player { get; private set; }
    public LineRenderer laser;

    private void Awake() {
        //To be updated when there are more players
        Player = this;
        
    }

    private void Start() {
        laser.startColor = new Color(1, 1, 1, 0);
        laser.endColor = new Color(1, 1, 1, 0);
    }

    public void MoveTo(Vector2 newPosition) {
        transform.position = newPosition;
    }

    public void Shoot(Vector2 direction) {
        laser.SetPositions(
            new Vector3[] { direction * 0.2f, direction * 100f }
            );
        StartCoroutine(shootEffect());
    }

    public void Die() {
        Debug.Log("<dying sounds>");
    }

    private uint currentShootId = 0;

    private IEnumerator shootEffect() {
        Color startColor = new Color(1, 1, 1, 1);
        Color endColor = new Color(1, 1, 1, 0);

        uint thisShootId = ++currentShootId;

        float timeRequired = 0.3f, timePassed = 0;
        while(timePassed < timeRequired && currentShootId == thisShootId) {
            laser.startColor = laser.endColor = Color.Lerp(startColor, endColor, timePassed / timeRequired);
            timePassed += Time.deltaTime;
            yield return null;
        }

        if (currentShootId == thisShootId)
            laser.startColor = laser.endColor = endColor;
    }
}
