﻿using UnityEngine;
using System.Collections;

namespace GQ.Client.Model.XML
{

	public class AndCondition : CompoundCondition
	{

		/// <summary>
		/// True if all contained condition are fulfilled (which is also the case if no condition at all is included). Computed in a lazy manner.
		/// </summary>
		public override bool IsFulfilled ()
		{
			bool allFulfilled = true;
			foreach (ICondition condition in containedConditions) {
				allFulfilled &= condition.IsFulfilled ();
				if (!allFulfilled)
					break;
			}
			return allFulfilled;
		}

	}
}