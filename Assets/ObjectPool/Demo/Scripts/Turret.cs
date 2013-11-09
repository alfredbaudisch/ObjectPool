using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
	public Bullet bulletPrefab;
	public Transform gun;

	void Start()
	{
		bulletPrefab.CreatePool(10);
		bulletPrefab.explosionPrefab.CreatePool(10);
	}

	void Update()
	{
		var plane = new Plane(Vector3.up, transform.position);
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float hit;
		if (plane.Raycast(ray, out hit))
		{
			var aimDirection = Vector3.Normalize(ray.GetPoint(hit) - transform.position);
			var targetRotation = Quaternion.LookRotation(aimDirection);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10 * Time.deltaTime);

			if (Input.GetMouseButtonDown(0))
				bulletPrefab.Spawn(gun.position, gun.rotation);
		}
	}
}
