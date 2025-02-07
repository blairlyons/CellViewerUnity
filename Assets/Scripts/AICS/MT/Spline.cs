﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.MT
{
	[System.Serializable]
	public class SplinePoint
	{
		public Vector3 position;
		public Vector3 tangent;
		public Quaternion rotation;
		public float arcLength;
		public float t;

		public SplinePoint (Vector3 _position, Vector3 _tangent, Quaternion _rotation, float _arcLength, float _t)
		{
			position = _position;
			tangent = _tangent;
			rotation = _rotation;
			arcLength = _arcLength;
			t = _t;
		}
	}

	public abstract class Spline : MonoBehaviour 
	{
		public int resolutionPerPoint = 15;
		public float updateTolerance = 0.1f;
		public bool drawCurve;
		public Color lineColor = new Color( 1f, 0, 1f );
		public float lineWidth = 1f;
		public Transform[] points;
		public SplinePoint[] calculatedPoints;

		void Start ()
		{
			lastPointPositions = null;
			UpdateSpline();
		}

//		void Update ()
//		{
//			
//		}

		public bool UpdateSpline ()
		{
			if (needToUpdate)
			{
				CalculateCurve();
				UpdateDraw();
				return true;
			}
			return false;
		}

		// ---------------------------------------------- Length

		float lastLengthTime = -1f;
		float _length;
		public float length
		{
			get {
				if (Time.time - lastLengthTime > 0.1f)
				{
					_length = GetLength();
					lastLengthTime = Time.time;
				}
				return _length;
			}
		}

		public float GetLength ()
		{
			if (haveNotCalculated)
			{
				CalculateCurve();
			}
			float l = 0;
			for (int i = 1; i < calculatedPoints.Length; i++)
			{
				l += calculatedPoints[i].arcLength;
			}
			return l;
		}

		// ---------------------------------------------- Point Positions

		Vector3[] lastPointPositions;

		protected bool pointsAreSet
		{
			get {
				return points != null && n > 0;
			}
		}

		public bool needToUpdate
		{
			get {
				if (pointsAreSet)
				{
					bool changed = false;
					if (lastPointPositions == null || lastPointPositions.Length < n)
					{
						lastPointPositions = new Vector3[n];
						for (int i = 0; i < n; i++)
						{
							lastPointPositions[i] = Vector3.zero;
						}
					}
					for (int i = 0; i < n; i++)
					{
						if (Vector3.Distance( points[i].position, lastPointPositions[i] ) > updateTolerance)
						{
							changed = true;
							lastPointPositions[i] = points[i].position;
						}
					}
					return changed;
				}
				return false;
			}
		}

		// ---------------------------------------------- Drawing

		protected LineRenderer line;

		void UpdateDraw ()
		{
			if (drawCurve)
			{
				ClearLine();
				Draw();
			}
		}

		void Draw ()
		{
			line = new LineRenderer();
			line = new GameObject( "line", new System.Type[]{ typeof(LineRenderer) } ).GetComponent<LineRenderer>();
			line.material = new Material( Shader.Find( "Particles/Alpha Blended Premultiply" ) );
			line.startColor = line.endColor = lineColor;
			line.startWidth = line.endWidth = lineWidth;
			line.positionCount = calculatedPoints.Length;
			line.transform.SetParent( transform );
			line.transform.position = calculatedPoints[0].position;

			Vector3[] points = new Vector3[calculatedPoints.Length];
			for (int i = 0; i < calculatedPoints.Length; i++)
			{
				points[i] = calculatedPoints[i].position;
			}
			line.SetPositions( points );
		}

		protected void ClearLine ()
		{
			if (line != null)
			{
				Destroy( line.gameObject );
			}
		}

		void OnDrawGizmos ()
		{
			if (needToUpdate)
			{
				CalculateCurve();
			}
			DrawGizmo();
		}

		void DrawGizmo ()
		{
			for (int i = 0; i < calculatedPoints.Length - 1; i++)
			{
				Gizmos.DrawLine( calculatedPoints[i].position, calculatedPoints[i + 1].position );
			}
		}

		// ---------------------------------------------- Calculation

		public abstract int n 
		{
			get;
		}

		bool haveNotCalculated 
		{
			get
			{
				return calculatedPoints == null || calculatedPoints.Length == 0;
			}
		}

		protected abstract void PreCalculateCurve ();

		void CalculateCurve ()
		{
			PreCalculateCurve();

			calculatedPoints = new SplinePoint[resolutionPerPoint * (n - 1) + 1];
			int k = 0, segments;
			Vector3 position, tangent;
			normalTransform.rotation = Quaternion.identity;
			float arcLength = 0, t = 0;
			for (int section = 0; section < n - 1; section++)
			{
				segments = (section < n - 2 ? resolutionPerPoint : resolutionPerPoint + 1);
				for (int s = 0; s < segments; s++)
				{
					t = GetTForCalculation( section, s );
					position = CalculatePosition( section, t );
					tangent = CalculateTangent( section, t );
					if (k > 0)
					{
						arcLength = Vector3.Distance( position, calculatedPoints[k - 1].position );
					}
					calculatedPoints[k] = new SplinePoint( position, tangent, CalculateRotation( position, tangent ), arcLength, t );
					k++;
				}
			}
		}

		protected abstract float GetTForCalculation (int section, int segment);

		protected abstract Vector3 CalculatePosition (int pointIndex, float sectionT);

		protected abstract Vector3 CalculateTangent (int pointIndex, float sectionT);

		Transform _normalTransform;
		protected Transform normalTransform
		{
			get
			{
				if (_normalTransform == null)
				{
					_normalTransform = transform.Find( "Normal" );
					if (_normalTransform == null)
					{
						_normalTransform = new GameObject( "Normal" ).transform;
						_normalTransform.SetParent( transform );
						_normalTransform.localPosition = Vector3.zero;
					}
				}
				return _normalTransform;
			}
		}

		Quaternion CalculateRotation (Vector3 position, Vector3 tangent)
		{
			float dot = Mathf.Clamp( Vector3.Dot( normalTransform.forward.normalized, tangent ), -1f + Mathf.Epsilon, 1f - Mathf.Epsilon );
			float angle = 180f * Mathf.Acos( dot ) / Mathf.PI;
			Vector3 axis = Vector3.Normalize( Vector3.Cross( normalTransform.forward.normalized, tangent ) );
			normalTransform.RotateAround( position, axis, angle );
			return normalTransform.rotation;
		}

		// ---------------------------------------------- 

		CubicSplinePosition GetSplinePositionForT (float t)
		{
			float tLength = t * length;
			float currentLength = 0;
			for (int i = 1; i < calculatedPoints.Length; i++)
			{
				float arcLength = calculatedPoints[i].arcLength;
				if (tLength < currentLength + arcLength)
				{
					return new CubicSplinePosition( i - 1, (tLength - currentLength) / arcLength );
				}
				currentLength += arcLength;
			}
			return new CubicSplinePosition( calculatedPoints.Length - 2, 1f );
		}

		public Vector3 GetPosition (float t)
		{
			CubicSplinePosition splinePosition = GetSplinePositionForT( t );
			Vector3 startPosition = calculatedPoints[splinePosition.pointIndex].position;
			Vector3 endPosition = calculatedPoints[splinePosition.pointIndex + 1].position;
			return Vector3.Lerp( startPosition, endPosition, splinePosition.sectionT ) - transform.position;
		}

		public Vector3 GetTangent (float t)
		{
			CubicSplinePosition splinePosition = GetSplinePositionForT( t );
			Vector3 startTangent = calculatedPoints[splinePosition.pointIndex].tangent;
			Vector3 endTangent = calculatedPoints[splinePosition.pointIndex + 1].tangent;
			return Vector3.Lerp( startTangent, endTangent, splinePosition.sectionT );
		}

		public Vector3 GetNormal (float t)
		{
			CubicSplinePosition splinePosition = GetSplinePositionForT( t );
			Quaternion startRotation = calculatedPoints[splinePosition.pointIndex].rotation;
			Quaternion endRotation = calculatedPoints[splinePosition.pointIndex + 1].rotation;
			normalTransform.rotation = Quaternion.Slerp( startRotation, endRotation, splinePosition.sectionT );
			return normalTransform.up.normalized;
		}
	}
}
