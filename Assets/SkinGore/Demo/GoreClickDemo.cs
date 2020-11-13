using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paints gore onto objects by clicking on them. Demo Script.
/// </summary>
public class GoreClickDemo : MonoBehaviour
{
    Camera cam;
    public float damageRadius = 0.1f;
    public float damageAmount = 1;
    public float spacing = 0.02f;

    private Vector3 lastPos;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    RaycastHit hit;
    private void Update()
    {
        if (Input.GetMouseButton(0) && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if ((lastPos - hit.point).sqrMagnitude > spacing * spacing)
            {
                if (hit.rigidbody != null)
                {
                    SkinGoreRenderer r = hit.rigidbody.GetComponent<SkinGoreRenderer>();
                    if (r != null)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            float amt = Input.GetMouseButtonDown(0) ? 1 : damageAmount;
                            r.AddDamage(hit.point, damageRadius, amt);
                            lastPos = hit.point;
                        }
                    }
                }
            }
        }
    }

    public void ResetDamage()
    {
        foreach (SkinGoreRenderer r in FindObjectsOfType<SkinGoreRenderer>())
        {
            r.ResetDamage();
        }
    }

    public void SetRadius(float r)
    {
        damageRadius = r;
    }
}
