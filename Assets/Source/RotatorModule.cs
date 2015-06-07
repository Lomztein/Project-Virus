﻿using UnityEngine;
using System.Collections;

public class RotatorModule : Module {

	public enum RotatorType { Standard, Sprayer, Spinner }

	public RotatorType type;
	public float turnSpeed;
	public float defualtRot;
	private float angleToTarget;
	private float sprayAngle = 30f;

	// Update is called once per frame
	void Update () {
		Turn ();
	}

	new void Start () {
		base.Start ();
		defualtRot = transform.eulerAngles.z;
	}

	void Turn () {
		angleToTarget = defualtRot;
		if (type == RotatorType.Standard) {

			if (parentBase)
				if (parentBase.target)
					angleToTarget = Angle.CalculateAngle (transform.position, parentBase.targetPos);

			RotateToAngle ();
		}else if (type == RotatorType.Sprayer) {
			if (EnemySpawn.waveStarted) {
				angleToTarget = defualtRot + Mathf.Sin (Time.time * (360 / turnSpeed)) * sprayAngle;
				RotateToAngle ();
			}
		}else if (type == RotatorType.Spinner && EnemySpawn.waveStarted) {
			transform.Rotate (0,0,turnSpeed * ResearchMenu.turnrateMul * Time.deltaTime);
		}
	}

	void RotateToAngle () {
		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, angleToTarget), turnSpeed * Time.deltaTime * ResearchMenu.turnrateMul * upgradeMul);
	}

	public override string ToString () {
		return "Turn speed: " + (turnSpeed * upgradeMul).ToString () + " - ";
	}
}