﻿using UnityEngine;
using System.Collections;

public class TeslaProjectile : Projectile {

	public float spherecastWidth;
	public float segmentLength;
	public float noiseAmount;
	public LineRenderer lineRenderer;
	private Vector3 end;

	public override void Initialize () {

		Ray ray = new Ray (transform.position, velocity.normalized);
		RaycastHit hit;

		if (Physics.SphereCast (ray, spherecastWidth, out hit, range, Game.game.enemyLayer)) {
			DrawTeslaBeam (hit.point);
			end = hit.point;
			OnHit (hit);
		}else{
			DrawTeslaBeam (ray.GetPoint (range));
			end = ray.GetPoint (range);
		}
		Invoke ("ReturnToPool", 1f);
	}

	void FixedUpdate () {
		if (Random.Range (0, 50) == 0)
			DrawTeslaBeam (end);
	}

	void DrawTeslaBeam (Vector3 end) {
		lineRenderer.SetPosition (0, transform.position);
		
		int segments = (Mathf.CeilToInt (Vector3.Distance (transform.position, end) / segmentLength));
		Vector3 dir = velocity.normalized * segmentLength;
		lineRenderer.SetVertexCount (segments);

		for (int i = 1; i < segments - 1; i++) {
			lineRenderer.SetPosition (i, transform.position + dir * i + Random.insideUnitSphere * noiseAmount);
		}

		lineRenderer.SetPosition (segments - 1, end);
	}
}
