using UnityEngine;

public class ItemController : MonoBehaviour
{
	private Plane _dragPlane;
	private Rigidbody _rigidbody;

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_dragPlane = new Plane(Vector3.up,new Vector3(0,1.5f,0));
	}

	private void OnMouseDown()
	{
		_rigidbody.useGravity = false;
	}

	private void OnMouseDrag()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter;
		if (!_dragPlane.Raycast(ray, out enter)) return;
		var hitPoint = ray.GetPoint(enter);
		_rigidbody.velocity = (hitPoint - transform.position) * 10;
	}

	private void OnMouseUp()
	{
		_rigidbody.useGravity = true;
	}
}
