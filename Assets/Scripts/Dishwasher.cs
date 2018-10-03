using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dishwasher : MonoBehaviour
{
    private Plane _dragPlane;
    private Rigidbody _rigidbody;
    private ParticleSystem _particleSystem;
    private List<GameObject> _dishes;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _particleSystem = GetComponent<ParticleSystem>();
        _dragPlane = new Plane(Vector3.forward, new Vector3(2.5f, 4.25f, -4.25f));
        _dishes = new List<GameObject>();
    }

    private void Update()
    {
        if (transform.position.y > 2.5f)
            transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z);
        if (transform.position.y < 1.5f)
            transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
        if (transform.position.y < 1.56f && _dishes.Count >0 && !_washing) StartCoroutine(Wash());
    }

    private bool _washing;

    private IEnumerator Wash()
    {
        _washing = true;
        Camera.main.GetComponent<AudioSource>().Play();
        _particleSystem.Play();
        _rigidbody.isKinematic = true;
        yield return new WaitForSeconds(_particleSystem.main.duration);
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.up * 2;
        yield return new WaitForSeconds(.3f);
        foreach (var dish in _dishes)
        {
            yield return new WaitForSeconds(.1f);
            dish.transform.position = new Vector3(4.25f + Random.Range(-.2f,.2f),1.5f,-4.25f);
            dish.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-.5f,.5f),Random.Range(0,.3f),10f);
        }

        _washing = false;
        _dishes.Clear();
    }

    private void OnMouseDrag()
    {
        if (_washing) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;
        if (!_dragPlane.Raycast(ray, out enter)) return;
        var hitPoint = ray.GetPoint(enter);
        if (hitPoint.y > 2.5) hitPoint = new Vector3(hitPoint.x, 2.5f, hitPoint.z);
        if (hitPoint.y < 1.5) hitPoint = new Vector3(hitPoint.x, 1.5f, hitPoint.z);
        _rigidbody.velocity = new Vector3(0, (hitPoint.y - transform.position.y) * 10, 0);
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Item") && !_washing)
        {
            _dishes.Add(c.gameObject);
        }
    }
}
