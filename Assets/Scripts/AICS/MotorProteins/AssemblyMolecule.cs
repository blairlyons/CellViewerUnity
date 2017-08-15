﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.MotorProteins
{
	// An assembly that is made up of molecules
	public abstract class AssemblyMolecule : Molecule 
	{
		public List<ComponentMolecule> componentMolecules;
		public KineticRates kineticRates;

		// should be called in Awake()
		protected void ConnectComponents ()
		{
			foreach (ComponentMolecule molecule in componentMolecules)
			{
				molecule.assembly = this as AssemblyMolecule;
				molecule.moleculeDetectors = moleculeDetectors;
			}
		}

		public override bool OtherWillCollide (Molecule molecule, Vector3 moveStep)
		{
			foreach (ComponentMolecule m in componentMolecules)
			{
				if (m as Molecule != molecule)
				{
					if (m.OtherWillCollide( molecule, moveStep ))
					{
						return true;
					}
				}
			}
			return false;
		}

		public abstract void SetParentSchemeOnComponentBind (ComponentMolecule molecule);

		public abstract void SetParentSchemeOnComponentRelease (ComponentMolecule molecule);

		public void SetMinDistanceFromParent (float min)
		{
			foreach (ComponentMolecule molecule in componentMolecules)
			{
				molecule.minDistanceFromParent = min;
			}
		}

		public void SetMaxDistanceFromParent (float max)
		{
			foreach (ComponentMolecule molecule in componentMolecules)
			{
				molecule.maxDistanceFromParent = max;
			}
		}

		public void SetMeanStepSize (float size)
		{
			foreach (ComponentMolecule molecule in componentMolecules)
			{
				molecule.meanStepSize = size;
			}
		}
	}
}