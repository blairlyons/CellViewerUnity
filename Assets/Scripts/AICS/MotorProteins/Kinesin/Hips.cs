﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.MotorProteins.Kinesin
{
	public enum HipsState 
	{
		Free,
		Locked
	}

	public class Hips : LinkerComponentMolecule 
	{
		public bool frozen = false;
		public HipsState state = HipsState.Free;
		public float snapPosition = 5.5f; // nm in front of motor pivot
		public float snapSpeed = 9000f; // degrees per simulated second
		public Follower follower;

		Vector3[] snappingArcPositions;
		public int currentSnapStep = 0;
		public bool snapping;
		public Motor lastSnappingPivot;
		float degreesPerSnapStep = 30f;
		bool justMoved = false;

		public Kinesin kinesin
		{
			get
			{
				return assembly as Kinesin;
			}
		}

		public override bool bound
		{
			get
			{
				return state == HipsState.Locked;
			}
		}

		protected override void OnAwake () { }

		public override void DoCustomSimulation ()
		{
			justMoved = false;
			if (!frozen && kinesin.hasBound)
			{
				if (snapping)
				{
					UpdateSnap();
				}
				else 
				{
					DoRandomRotation();
					DoRandomWalk();
				}
				Animate( snapping );
			}
			follower.Follow();
		}

		protected override void InteractWithBindingPartners () { }

		// --------------------------------------------------------------------------------------------------- Random walk

		void DoRandomRotation ()
		{
			if (!bound && !rotating)
			{
				RotateRandomly();
			}
		}

		public override void DoRandomWalk ()
		{
			if (!bound)
			{
				if (!moving)
				{
					int i = 0;
					bool retry = false;
					bool success = false;
					while (!success && i < MolecularEnvironment.Instance.maxIterationsPerStep)
					{
						success = MoveRandomly( retry );
						retry = true;
						i++;
					}

					if (!success)
					{
						Jitter( 0.1f );
					}
				}
			}
			else
			{
				Jitter();
			}
		}

		// --------------------------------------------------------------------------------------------------- Snap

		public void StartSnap (Motor motor)
		{
			if (!frozen && !(motor == lastSnappingPivot && state == HipsState.Locked) && !motor.otherMotor.bound)
			{
				if (motor.otherMotor.binding)
				{
					motor.otherMotor.CancelTubulinBind();
				}
				snappingArcPositions = CalculateSnapArcPositions( motor );
				lastSnappingPivot = motor;
				currentSnapStep = 0;
				state = HipsState.Locked;
				snapping = true;
				justMoved = true;
				MoveTo( snappingArcPositions[0], true );
				body.isKinematic = true;
			}
		}

		Vector3[] CalculateSnapArcPositions (Motor motor)
		{
			Vector3 snappedPosition = SnappedPosition( motor );
			if (Vector3.Distance( snappedPosition, transform.position ) > 1f)
			{
				Vector3 motorToCurrentPosition = transform.position - motor.transform.position;
				Vector3 motorToSnappedPosition = snappedPosition - motor.transform.position;
				float angle = (180f / Mathf.PI) * Mathf.Acos( Mathf.Clamp( Vector3.Dot( motorToCurrentPosition.normalized, motorToSnappedPosition.normalized ), -1f, 1f ) );

				Vector3[] arcPositions = null;
				if (angle > 90f)
				{
					Vector3 motorToNorthPole = 3f * motor.transform.up;

					Vector3[] arcPositions1 = CalculateArcPositions( motor.transform.position, motorToCurrentPosition, motorToNorthPole );
					Vector3[] arcPositions2 = CalculateArcPositions( motor.transform.position, motorToNorthPole, motorToSnappedPosition );

					arcPositions = new Vector3[ arcPositions1.Length + arcPositions2.Length ];
					arcPositions1.CopyTo( arcPositions, 0 );
					arcPositions2.CopyTo( arcPositions, arcPositions1.Length );
				}
				else 
				{
					arcPositions = CalculateArcPositions( motor.transform.position, motorToCurrentPosition, motorToSnappedPosition );
				}
				return arcPositions;
			}
			else 
			{
				return new Vector3[1] {snappedPosition};
			}
		}

		Vector3 SnappedPosition (Motor strongMotor)
		{
			return strongMotor.transform.position + Mathf.Min( snapPosition, maxDistanceFromParent ) * -strongMotor.transform.right;
		}

		Vector3[] CalculateArcPositions (Vector3 pivotPosition, Vector3 startLocalPosition, Vector3 goalLocalPosition)
		{
			float totalAngle = Mathf.Rad2Deg * Mathf.Acos( Mathf.Clamp( Vector3.Dot( startLocalPosition.normalized, goalLocalPosition.normalized ), -1f, 1f ) );
			int steps = Mathf.Max( Mathf.RoundToInt( totalAngle / degreesPerSnapStep ), 1 );
			float dAngle = totalAngle / steps;
			float dLength = (goalLocalPosition.magnitude - startLocalPosition.magnitude) / steps;
			Vector3 axis = Vector3.Cross( startLocalPosition.normalized, goalLocalPosition.normalized ).normalized;
			Vector3[] arcPositions = new Vector3[steps];

			for (int i = 0; i < steps - 1; i++)
			{
				arcPositions[i] = pivotPosition + (startLocalPosition.magnitude + (i + 1f) * dLength) * (Quaternion.AngleAxis( (i + 1f) * dAngle, axis ) * startLocalPosition.normalized);
			}
			arcPositions[arcPositions.Length - 1] = pivotPosition + goalLocalPosition;
			return arcPositions;
		}

//		protected override void OnFinishMove () { }

		void UpdateSnap ()
		{
			if (currentSnapStep + 1 < snappingArcPositions.Length)
			{
				if (!moving)
				{
					currentSnapStep++;
					MoveTo( snappingArcPositions[currentSnapStep], true );
				}
			}
			else
			{
				snapping = moving = false;
			}
		}

		public void SetFree (Motor releasedMotor)
		{
			if (lastSnappingPivot == releasedMotor)
			{
				snapping = moving = false;
				state = HipsState.Free;
				body.isKinematic = false;
			}
		}

		// --------------------------------------------------------------------------------------------------- Reset

		public override void DoCustomReset ()
		{
			state = HipsState.Free;
			secondParent = null;
		}
	}
}