﻿using UnityEngine;
using System.Collections;

public class RailgunProjectile : Projectile {

	public float spherecastWidth;

	void FixedUpdate () {
		CastSphereRay ();
		transform.position += velocity * Time.fixedDeltaTime;
	}

	public void CastSphereRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime * 2f);
		RaycastHit hit;
		
		if (Physics.SphereCast (ray, spherecastWidth, out hit, velocity.magnitude * Time.fixedDeltaTime * 2f)) {
			if (hit.collider.gameObject.layer != parent.layer && hit.collider.tag != "BulletIgnore") {
				OnHit (hit.collider, hit.point, transform.right);			
			}
			
		}
	}

}
