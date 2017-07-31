﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.AnimatedKinesin
{
	public abstract class Molecule : MonoBehaviour 
	{
		public bool logEvents;
		public float maxDistanceFromParent = 8f;
		public float meanStepSize = 0.2f;
		public float meanRotation = 1f;

		Rigidbody _body;
		protected Rigidbody body 
		{
			get
			{
				if (_body == null)
				{
					_body = GetComponent<Rigidbody>();
					_body.useGravity = false;
					_body.isKinematic = true;
				}
				return _body;
			}
		}

		protected void Rotate ()
		{
			transform.rotation *= Quaternion.Euler( Helpers.GetRandomVector( SampleExponentialDistribution( meanRotation ) ) );
		}

		protected void Move () 
		{
			float stepSize = SampleExponentialDistribution( meanStepSize );
			Vector3 moveStep = Helpers.GetRandomVector( stepSize );
			if (!WillCollide( moveStep ))
			{
				if (transform.parent == null || Vector3.Distance( transform.parent.position, transform.position + moveStep ) <= maxDistanceFromParent)
				{
					transform.position += moveStep;
				}
				else if (transform.parent != null)
				{
					moveStep = stepSize * (transform.parent.position - transform.position).normalized;
					if (!WillCollide( moveStep ))
					{
						transform.position += moveStep;
					}
				}
			}
		}

		bool MoveIsValid (Vector3 moveStep)
		{
			return !WillCollide( moveStep ) && (transform.parent == null || Vector3.Distance( transform.parent.position, transform.position + moveStep ) <= maxDistanceFromParent);
		}

		protected void Jitter () 
		{
			transform.position += Helpers.GetRandomVector( SampleExponentialDistribution( 0.01f ) );
		}

		protected abstract bool WillCollide (Vector3 moveStep);

		float SampleExponentialDistribution (float mean)
		{
			return Mathf.Log( Random.Range( float.Epsilon, 1f ) ) / (-1f / mean);
		}
	}
}