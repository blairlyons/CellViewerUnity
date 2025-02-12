﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.Diffusion
{
	[RequireComponent( typeof(Rigidbody), typeof(Collider) )]
	public class DiffusingParticle : MonoBehaviour 
	{
		public Vector2 clockTimeBetweenImpulses = new Vector2( 0.1f, 0.5f );
		public float normalizedDisplacement;

		public float lastTime = -1f;
		float timeInterval;
		Vector3 startPosition;
		int samples;
		Vector3 lastPosition;

		Rigidbody _rigidbody;
		Rigidbody body
		{
			get {
				if (_rigidbody == null)
				{
					_rigidbody = GetComponent<Rigidbody>();
				}
				return _rigidbody;
			}
		}

		MeshRenderer _meshRenderer;
		MeshRenderer meshRenderer
		{
			get {
				if (_meshRenderer == null)
				{
					_meshRenderer = GetComponent<MeshRenderer>();
				}
				return _meshRenderer;
			}
		}

		ParticleFactory _factory;
		ParticleFactory factory
		{
			get {
				if (_factory == null)
				{
					_factory = GetComponentInParent<ParticleFactory>();
				}
				return _factory;
			}
		}

		MSDCalculator _calculator;
		MSDCalculator calculator
		{
			get {
				if (_calculator == null)
				{
					_calculator = GetComponentInParent<MSDCalculator>();
				}
				return _calculator;
			}
		}

		void Start ()
		{
			SetTimeInterval();
			lastPosition = transform.position;
		}

		void Update () 
		{
			if (Time.time - lastTime > timeInterval)
			{
				SetTimeInterval();

				body.velocity = body.angularVelocity = Vector3.zero;

				body.AddForce( Helpers.GetRandomVector( forceMagnitude ) );
				body.AddTorque( Helpers.GetRandomVector( torqueMagnitude ) );

				lastTime = Time.time;
				calculator.LogDisplacement( (transform.position - lastPosition).magnitude );
			}
			lastPosition = transform.position;
		}

		public void SetStartPosition ()
		{
			startPosition = transform.position;
		}

		float forceMagnitude
		{
			get {
				float meanForce = body.mass * timeInterval * DiffusionParameterInput.Instance.forceMultiplier 
					* Mathf.Sqrt( DiffusionParameterInput.Instance.diffusionCoefficient.value * 1E-4f * DiffusionParameterInput.Instance.dTime.value );
				float force = Mathf.Log( Random.Range( float.Epsilon, 1f ) ) / (-1f / meanForce);
//				factory.RecordData( meanForce, force );
				return force;
			}
		}

		float torqueMagnitude
		{
			get {
				float meanForce = body.mass * timeInterval * DiffusionParameterInput.Instance.torqueMultiplier 
					* Mathf.Sqrt( DiffusionParameterInput.Instance.diffusionCoefficient.value * 1E-4f * DiffusionParameterInput.Instance.dTime.value );
				return Mathf.Log( Random.Range( float.Epsilon, 1f ) ) / (-1f / meanForce);
			}
		}

		public float displacement
		{
			get {
				return Vector3.Distance( startPosition, transform.position );
			}
		}

		void SetTimeInterval ()
		{
			timeInterval = Random.Range( clockTimeBetweenImpulses.x, clockTimeBetweenImpulses.y );
		}

		public void SetDisplacementColor ()
		{
			normalizedDisplacement = (displacement - factory.minDisplacement) / (factory.maxDisplacement - factory.minDisplacement);
			meshRenderer.material.color = Color.HSVToRGB( normalizedDisplacement, 1f, 1f );
		}
	}
}