﻿using UnityEngine;
using System.Collections;

public class SlowfieldModule : Module {

	public Transform rotator;
	public float rotateSpeed;
	public float range;

	public static float freezeMultiplier = 0.5f;
	
	void FixedUpdate () {
		rotator.Rotate (Vector3.forward * rotateSpeed * Time.deltaTime * upgradeMul);
		SlowNearby ();
	}

	void SlowNearby () {
		Collider[] nearby = Physics.OverlapSphere (transform.position, range * upgradeMul, Game.game.enemyLayer);
		for (int i = 0; i < nearby.Length; i++) {
			nearby[i].GetComponent<Enemy>().freezeMultiplier = freezeMultiplier;
		}
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireSphere (transform.position, range * upgradeMul);
	}
}
