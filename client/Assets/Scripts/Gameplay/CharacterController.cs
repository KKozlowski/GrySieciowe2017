﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

	public static CharacterController Player { get; private set; }
    public LineRenderer m_laser;

    private float m_power;
    public float Power { get { return m_power; } set {SetPower(value);} }

    [SerializeField]
    private bool m_isPlayer = false;
    public bool IsPlayer { get { return m_isPlayer; } }

    public void Init(bool player) {
        m_isPlayer = player;
        if (m_isPlayer)
            Player = this;
    }

    private void Start() {
        m_laser.startColor = new Color(1, 1, 1, 0);
        m_laser.endColor = new Color(1, 1, 1, 0);
    }

    public void MoveTo(Vector2 newPosition) {
        transform.position = newPosition;
    }

    public void SetPower(float power)
    {
        m_power = power;
        gameObject.SetActive(m_power>0);
        float scale = PlayerState.GetRadiusByPower(m_power)*2;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Shoot(Vector2 direction) {
        m_laser.SetPositions(
            new Vector3[] { Vector3.zero, direction * 100f }
            );
        StartCoroutine(shootEffect());
    }

    public void Die() {
        Debug.Log("<dying sounds>");
        gameObject.SetActive(false);
    }

    private uint currentShootId = 0;

    private IEnumerator shootEffect() {
        Color startColor = new Color(1, 1, 1, 1);
        Color endColor = new Color(1, 1, 1, 0);

        uint thisShootId = ++currentShootId;

        float timeRequired = 0.3f, timePassed = 0;
        while(timePassed < timeRequired && currentShootId == thisShootId) {
            m_laser.startColor = m_laser.endColor = Color.Lerp(startColor, endColor, timePassed / timeRequired);
            timePassed += Time.deltaTime;
            yield return null;
        }

        if (currentShootId == thisShootId)
            m_laser.startColor = m_laser.endColor = endColor;
    }
}
