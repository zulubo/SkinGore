using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOrbitDemo : MonoBehaviour
{
    public int mouseButton = 1;

    public Transform orbitPoint;

    public float minVertical = -20;
    public float maxVertical = 60;

    public float speed;
    public float acceleration = 30;
    public float friction = 20;

    Vector2 pos;
    Vector2 vel;

    private void Start()
    {
        orbitPoint.rotation = Quaternion.identity;
        transform.parent = orbitPoint;
    }

    private void Update()
    {
        if (Input.GetMouseButton(mouseButton))
        {
            vel = Vector2.Lerp(vel, new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * speed, Time.deltaTime * acceleration);
        }
        else
        {
            vel = Vector2.Lerp(vel, Vector2.zero, Time.deltaTime * friction);
        }

        pos += vel * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minVertical, maxVertical);
        pos.x = Mathf.Repeat(pos.x, 360);

        orbitPoint.rotation = Quaternion.Euler(pos.y, pos.x, 0);
    }
}
