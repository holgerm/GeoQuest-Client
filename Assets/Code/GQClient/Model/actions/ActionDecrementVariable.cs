﻿using UnityEngine;
using System.Collections;
using GQ.Client.Model;
using System.Xml.Serialization;
using GQ.Client.Err;
using System.Xml;

namespace GQ.Client.Model
{

	public class ActionDecrementVariable : ActionAbstractWithVar
	{
		#region Functions

		public override void Execute ()
		{
			if (VarName == null) {
				Log.SignalErrorToDeveloper ("IncrementVariableAction Action without varname can not be executed. (Ignored)");
				return;
			}

			Value previousVal = Variables.GetValue (VarName);

			switch (previousVal.ValType) {
			case Value.Type.NULL:
				Variables.SetVariableValue (VarName, new Value (-1));
				break;
			case Value.Type.Integer:
				Variables.SetVariableValue (VarName, new Value (previousVal.AsInt () - 1));
				break;
			case Value.Type.Float:
				Variables.SetVariableValue (VarName, new Value (previousVal.AsDouble () - 1d));
				break;
			case Value.Type.Bool:
				Variables.SetVariableValue (VarName, new Value (false));
				break;
			case Value.Type.VarExpression:
				Log.SignalErrorToAuthor ("IncrementVariable must not be used on Variables representing Variable Names.", previousVal.ValType);
				break;
			case Value.Type.Text:
				string previousText = previousVal.AsString ();
				char lastChar = previousText [previousText.Length - 1];
				if (lastChar > 0)
					lastChar--;
				string newText = previousText.Substring (0, previousText.Length - 1) + lastChar.ToString ();
				Variables.SetVariableValue (VarName, new Value (newText));
				break;
			default:
				Log.SignalErrorToDeveloper ("IncrementVariable not implemented for value type {0}.", previousVal.ValType);
				break;
			}
		}

		#endregion
	}
}
