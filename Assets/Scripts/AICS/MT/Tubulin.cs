﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AICS.MotorProteins;

namespace AICS.MT
{
	public class Tubulin : Molecule
	{
		public int tubulinType = -1;
		public bool hasMotorBound;
		public bool canBind = true;

		public override bool bound
		{
			get
			{
				return true;
			}
		}

		protected override void OnAwake () { }

		public void Place (Vector3 position, Vector3 lookDirection, Vector3 normal)
		{
			transform.localPosition = position;
			canBind = (position.y >= 1f);
			if (position.y <= -1f || position.magnitude > 500f)
			{
				transform.GetChild( 0 ).gameObject.SetActive( false );
			}
			transform.LookAt( transform.position + lookDirection, normal );
		}

		public override void DoCustomSimulation () { }

		public override void DoCustomReset () 
		{
			hasMotorBound = false;
		}
	}
}