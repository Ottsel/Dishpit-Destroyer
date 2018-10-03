using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class PlayerController : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigidbody;
    private const float AnimTime = 3f;

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100) ||
            !hit.transform.gameObject.CompareTag("Station") || !_navMeshAgent.enabled) return;

        var target = new Vector3(hit.transform.position.x, 1, hit.transform.position.z);
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.destination = target;
    }

    private bool _colliding;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Item") || _colliding || collision.relativeVelocity.magnitude < .3f) return;
        _colliding = true;
        _navMeshAgent.enabled = false;
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(collision.relativeVelocity);
        StartCoroutine(Fall());
    }

    private IEnumerator Fall()
    {
        var prevPosition = transform.position;
        var prevRotation = transform.rotation;

        yield return new WaitForSeconds(.5f);
        while (_rigidbody.velocity.magnitude > .05f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
        _rigidbody.isKinematic = true;
        while (1.04f - transform.position.y > .01f)
        {
            transform.position = Vector3.Lerp(transform.position, prevPosition, Time.deltaTime * AnimTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, prevRotation, Time.deltaTime * AnimTime);
            yield return null;
        }

        transform.position = prevPosition;
        transform.rotation = prevRotation;
        _navMeshAgent.enabled = true;
        _colliding = false;
    }
}