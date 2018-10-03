using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoseController : MonoBehaviour
{
    private Plane _dragPlane;
    private Rigidbody _rigidbody;
    public GameObject Hose;

    private void Start()
    {
        _rigidbody = Hose.GetComponent<Rigidbody>();
        _dragPlane = new Plane(Vector3.up,new Vector3(-0.25f,1.721f,-4.25f));
    }

    private void OnMouseDrag()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;
        if (!_dragPlane.Raycast(ray, out enter)) return;
        var hitPoint = ray.GetPoint(enter);
        if (hitPoint.x > 1.15f) hitPoint = new Vector3(1.15f, hitPoint.y, hitPoint.z);
        if (hitPoint.x < -1.15f) hitPoint = new Vector3(-1.15f, hitPoint.y, hitPoint.z);
        if (hitPoint.z > -3.25f) hitPoint = new Vector3(hitPoint.x, hitPoint.y, -3.25f);
        if (hitPoint.z < -4.25f) hitPoint = new Vector3(hitPoint.x, hitPoint.y,-4.25f);
        _rigidbody.velocity = new Vector3((hitPoint.x - Hose.transform.position.x) * 8, 0, (hitPoint.z - Hose.transform.position.z)*8);
    }
}