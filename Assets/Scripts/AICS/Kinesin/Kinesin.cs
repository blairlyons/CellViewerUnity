﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.Kinesin
{
	public class Kinesin : MonoBehaviour 
	{
		//referenced parameters
		public float motorReleaseProbabilityMax = 0.9f;
		public float motorReleaseProbabilityMin = 0.01f;
		public float motorReleaseK = 30f;
		public float motorReleaseX0 = 0.65f;
		public bool useNecklinkerLogicForMotorRelease = true;
		public float ATPBindProbabilityMax = 0.9f;
		public float ATPBindProbabilityMin = 0.01f;
		public float ATPBindK = 30f;
		public float ATPBindX0 = 0.75f;
		public float ADPReleaseProbabilityMax = 0.9f;
		public float ADPReleaseProbabilityMin = 0.01f;
		public float ADPReleaseK = 30f;
		public float ADPReleaseX0 = 0.75f;
		public float motorBindingRotationTolerance = 180f;
		public bool pushOtherMotorForwardAfterSnap = true;

		//need to set when updated
		public Vector3 hipJointRotationLimits = new Vector3( 0, 0, 0 );
		public Vector3 linkJointRotationLimits = new Vector3( 87f, 0, 5f );
		public float hipsMass = 0.1f;
		public float motorMass = 0.1f;
		public float linkMass = 0.03f;

		float maxDepenetrationVelocity = 20f;

		Hips _hips;
		public Hips hips
		{
			get {
				if (_hips == null)
				{
					_hips = GetComponentInChildren<Hips>();
				}
				return _hips;
			}
		}

		List<Motor> _motors;
		public List<Motor> motors
		{
			get {
				if (_motors == null)
				{
					_motors = new List<Motor>( GetComponentsInChildren<Motor>() );
				}
				return _motors;
			}
		}

		public Motor OtherMotor (Motor motor)
		{
			return motors.Find( m => m != motor );
		}

		void Start ()
		{
			SetParameters();
			AttachHipsToMotors();
			SetRigidbodyDepenetrationVelocity();
		}

		void SetParameters ()
		{

		}

		void AttachHipsToMotors ()
		{
			Rigidbody[] lastLinks = new Rigidbody[motors.Count];
			for (int i = 0; i < motors.Count; i++)
			{
				lastLinks[i] = motors[i].neckLinker.lastLink.GetComponent<Rigidbody>();
			}
			hips.AttachToMotors( lastLinks );
		}

		void SetRigidbodyDepenetrationVelocity ()
		{
			Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody body in bodies)
			{
				body.maxDepenetrationVelocity = maxDepenetrationVelocity;
			}
		}

		void Update ()
		{
			if (motors[0].neckLinker.stretched || motors[1].neckLinker.stretched)
			{
				if (motors[0].state == MotorState.Strong && motors[1].state == MotorState.Strong)
				{
					foreach (Motor motor in motors)
					{
						if (motor.neckLinker.snapping || motor.neckLinker.bound)
						{
							if (motor.printEvents) { Debug.Log( "kinesin released necklinker " + motor.name ); }
							motor.neckLinker.Release();
						}
					}
				}
				else 
				{
					foreach (Motor motor in motors)
					{
						if (motor.state == MotorState.Strong && motor.otherMotor.bound)
						{
							if (motor.inFront)
							{
								if (motor.otherMotor.printEvents) { Debug.Log( "kinesin released " + motor.otherMotor.name ); }
								motor.otherMotor.Release();
							}
							else if (motor.neckLinker.snapping || motor.neckLinker.bound)
							{
								if (motor.printEvents) { Debug.Log( "kinesin released necklinker " + motor.name ); }
								motor.neckLinker.Release();
							}

						}
					}
				}
			}
		}
	}
}