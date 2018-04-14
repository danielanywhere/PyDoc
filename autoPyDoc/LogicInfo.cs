//	LogicInfo.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	LogicInfoCollection																											*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of LogicInfoItem Items.
	/// </summary>
	public class LogicInfoCollection : List<LogicInfoItem>
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
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse a conditional or branching logic item and its members.
		/// </summary>
		/// <param name="values">
		/// Collection of variables to which matching items will be appended.
		/// </param>
		/// <param name="lines">
		/// Collection of source text lines to inspect.
		/// </param>
		/// <param name="index">
		/// Current line index within the source file collection.
		/// </param>
		/// <param name="indent">
		/// Current source indent spacing from left margin.
		/// </param>
		/// <param name="comments">
		/// Collection of comments not yet assigned to objects.
		/// </param>
		/// <returns>
		/// The new index at which to continue searching for the next entity.
		/// </returns>
		/// <remarks>
		/// This version only processes a single match before returning to the
		/// caller to prevent false positives with general variable expressions.
		/// </remarks>
		public static int Parse(LogicInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			int rv = index;						//	Return Value.

			if(values != null && lines != null && lines.Count > index)
			{
				////	Legal values provided.
				////	The caller already expects that this is an unimplemented logic
				////	item, but doesn't know whether the definition is a single line,
				////	or multiple.
				rv = LogicInfoItem.Parse(values, lines, index, indent, comments);
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	LogicInfoItem																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Conditional and Branching Logic partial implementations for
	/// extensibility.
	/// </summary>
	public class LogicInfoItem : ContainerBase
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
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse the contents of a conditional or branching login item
		/// and its members.
		/// </summary>
		/// <param name="values">
		/// Sibling logic definitions at this level.
		/// </param>
		/// <param name="lines">
		/// Source text lines.
		/// </param>
		/// <param name="index">
		/// Current line index within the source.
		/// </param>
		/// <param name="indent">
		/// Current indent level.
		/// </param>
		/// <param name="comments">
		/// Collection of comments directly preceeding this class, as in the
		/// case where documentation for a class is written in front of the
		/// class declaration.
		/// </param>
		/// <returns>
		/// Next active index in the source. That value might be equal to or
		/// larger than the index parameter.
		/// </returns>
		public static int Parse(LogicInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			LogicInfoItem ci = null;	//	New Logic.
																//	Local Comments.
			CommentInfoCollection cm = new CommentInfoCollection();
			ContainerInfo ct = null;	//	Recursive call container.
			int lp = index;						//	List Position.
			string ls = "";						//	Line String.
			string lt = "";						//	Trimmed Line.
			Match mi = null;					//	Working Match.
			string ps = "";						//	Pattern String.
			int rv = index;						//	Return Value.
			StringBuilder sb = null;

			ls = lines[lp].Value;
			lt = ls.Trim();
			mi = Regex.Match(ls, mPattern);
			if(mi != null && mi.Success && Utils.GetLength(mi, "space") >= indent)
			{
				//	This line begins a logic definition.
				sb = new StringBuilder();
				lp = Utils.GetTail(lines, lp, ":", sb, comments);
				if(lp > -1)
				{
					//	Declaration was found.
					//	Get the full workup.
					ps = @"(?i:^(?<space>[ |\t]*)(?<name>(for|if|try|except))" +
						@"(\s+(?<content>(?<condition>[^\:\(]+){0,1}" +
						@"(\((?<params>[^\)]*)\)){0,1})){0,1}\s*\:\s*$)";
					mi = Regex.Match(ls, ps);
					if(mi != null && mi.Success)
					{
						//	The method has been found. Create an item.
						//	Get Line, Indent, Name, and Params.
						ci = new LogicInfoItem();
						ci.LineNumber = index;
						ci.IndentLevel = Utils.GetLength(mi, "space");
						ci.Name = Utils.GetValue(mi, "name");
						ci.Parameters = Utils.GetValue(mi, "content");
						//	This item will receive the caller's comments pack.
						ci.Comments.AddRange(comments);
						comments.Clear();
						values.Add(ci);
						ct = ContainerBase.GetContainerInfo(ci,
							lines, lp + 1, ci.IndentLevel + 1);
						rv = Utils.ParseAll(ct);
					}
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Pattern																																*
		//*-----------------------------------------------------------------------*
		private static string mPattern =
			@"(?i:^(?<space>[ |\t]*)(for|if|try|except)(\s+(?<name>[^\:\(]+)){0,1})";
		/// <summary>
		/// Get the pattern for partially matching a condiditional or branching
		/// logic pattern.
		/// </summary>
		public static string Pattern
		{
			get { return mPattern; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
