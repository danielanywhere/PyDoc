//	ExpressionStack.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	ExpressionStack																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Stack of ExpressionItem Items.
	/// </summary>
	public class ExpressionStack : Stack<ExpressionItem>
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		private static string mPatternClassAccess =
			@"(?i:(?<class>[a-z]+[0-9a-z-_]*)\s*\.)";
		private static string mPatternEntity =
			@"(?i:(?<entity>[a-z]+[0-9a-z-_]*))";
		private static string mPatternFunction = @"(?i:" +
			@"(?<obj>(?=[a-z]+[0-9a-z-_]*\s*\.)" +
			@"([a-z]+[0-9a-z-_]*\s*\.\s*)+){0,1}" +
			@"((?<func>[a-z]+[0-9a-z-_]*)\s*\())";
		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	ExpressionEntities																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a list of all named entities found in the caller's expression.
		/// </summary>
		/// <param name="expression">
		/// Expression to inspect.
		/// </param>
		/// <returns>
		/// Collection of all entities found in the expression, if any. If no
		/// entities were found, a empty collection is returned.
		/// </returns>
		public static List<string> ExpressionEntities(string expression)
		{
			MatchCollection mc = null;							//	Matching entities.
			List<string> ro = new List<string>();		//	Return object.

			if(expression != null)
			{
				mc = Regex.Matches(expression, mPatternEntity);
				if(mc != null && mc.Count > 0)
				{
					foreach(Match mi in mc)
					{
						ro.Add(Utils.GetValue(mi, "entity"));
					}
				}
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	FunctionClass																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the class-access hierarchy portion of a function call, if any.
		/// </summary>
		/// <param name="expression">
		/// The expression to inspect.
		/// </param>
		/// <param name="startingIndex">
		/// The starting index at which to begin searching within the expression.
		/// </param>
		/// <returns>
		/// If class-access is present, either instance-based or static, the
		/// class access sections of the call are returned. Otherwise, an empty
		/// collection is provided.
		/// </returns>
		public static List<string> FunctionClass(string expression,
			int startingIndex)
		{
			string cs = "";													//	Class-access string.
			MatchCollection mc = null;							//	Class-access matches.
			Match mf = null;												//	Function call match.
			List<string> ro = new List<string>();		//	Return object.
			string se = "";													//	Sub expression.

			if(expression != null && expression.Length > 0 &&
				startingIndex < expression.Length)
			{
				//	Partition the string.
				if(startingIndex == -1 || startingIndex == 0)
				{
					se = expression.Substring(0);
				}
				else
				{
					se = expression.Substring(startingIndex);
				}
				//	Get the first function match at the specified location.
				mf = Regex.Match(se, mPatternFunction);
				if(mf != null && mf.Success)
				{
					//	Pattern was found.
					cs = Utils.GetValue(mf, "obj").Trim();
					if(cs.Length > 0)
					{
						//	Class-access pattern is present.
						mc = Regex.Matches(cs, mPatternClassAccess);
						if(mc != null)
						{
							foreach(Match mi in mc)
							{
								ro.Add(Utils.GetValue(mi, "class"));
							}
						}
					}
				}
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	FunctionName																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the function name portion of a function call, if any.
		/// </summary>
		/// <param name="expression">
		/// The expression to inspect.
		/// </param>
		/// <param name="startingIndex">
		/// The starting index at which to begin searching within the expression.
		/// </param>
		/// <returns>
		/// Name of the function, if found. Otherwise, a blank string.
		/// </returns>
		public static string FunctionName(string expression,
			int startingIndex)
		{
			Match mf = null;							//	Function call match.
			string rv = "";								//	Return value.
			string se = "";								//	Sub expression.

			if(expression != null && expression.Length > 0 &&
				startingIndex < expression.Length)
			{
				//	Partition the string.
				if(startingIndex == -1 || startingIndex == 0)
				{
					se = expression.Substring(0);
				}
				else
				{
					se = expression.Substring(startingIndex);
				}
				//	Get the first function match at the specified location.
				mf = Regex.Match(se, mPatternFunction);
				if(mf != null && mf.Success)
				{
					//	Pattern was found.
					rv = Utils.GetValue(mf, "func").Trim();
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	FunctionStarts																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a list of function calls starting locations for the caller's
		/// expression.
		/// </summary>
		/// <param name="expression">
		/// Expression to analyze.
		/// </param>
		/// <returns>
		/// Int collection of 0-based string indices at which functions can be
		/// identified as starting, if found. Otherwise, an empty collection.
		/// </returns>
		public static List<int> FunctionStarts(string expression)
		{
			MatchCollection mc = null;				//	Function call matches.
			List<int> ro = new List<int>();		//	Return object.

			if(expression != null && expression.Length > 0)
			{
				mc = Regex.Matches(expression, mPatternFunction);
				if(mc != null && mc.Count > 0)
				{
					//	Matches were returned.
					foreach(Match mi in mc)
					{
						if(mi.Groups["func"] != null && mi.Groups["func"].Value != null &&
							mi.Groups["func"].Index > -1)
						{
							ro.Add(mi.Groups["func"].Index);
						}
					}
				}
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Push																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Push a new item onto the stack by member values.
		/// </summary>
		/// <param name="value">
		/// Text value to push to the stack.
		/// </param>
		/// <returns>
		/// Newly created ExpressionItem.
		/// </returns>
		public ExpressionItem Push(string value)
		{
			ExpressionItem ro = new ExpressionItem();

			ro.Value = value;
			this.Push(ro);
			return ro;
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ExpressionItem																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Entry in the stack.
	/// </summary>
	public class ExpressionItem
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	Value																																	*
		//*-----------------------------------------------------------------------*
		private string mValue = "";
		/// <summary>
		/// Get/Set the basic value of this item.
		/// </summary>
		public string Value
		{
			get { return mValue; }
			set { mValue = value; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
