//	CommentInfo.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	CommentInfoCollection																										*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of CommentInfoItem Items.
	/// </summary>
	public class CommentInfoCollection : List<CommentInfoItem>
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
		//*	Add																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Add an item to the collection by member values.
		/// </summary>
		/// <param name="value">
		/// Comment text to add.
		/// </param>
		/// <param name="lineNumber">
		/// Line number associated with this entry.
		/// </param>
		/// <returns>
		/// Reference to a newly created and added comment.
		/// </returns>
		public CommentInfoItem Add(string value, int lineNumber)
		{
			CommentInfoItem ro = new CommentInfoItem();

			ro.Value = value;
			ro.LineNumber = lineNumber;
			this.Add(ro);
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	AddRange																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Add a range of comments to the collection.
		/// </summary>
		/// <param name="value">
		/// Range of comments to be added, as delimited by newline characters.
		/// </param>
		/// <param name="lineNumber">
		/// Line index at which the comment was found in source.
		/// </param>
		public void AddRange(string value, int lineNumber)
		{
			int lp = 0;					//	List Position.
			string[] sa = new string[0];
			string ws = "";			//	Working String.

			if(value != null && value.Length > 0)
			{
				ws = value.Replace("\r\n", "\n");
				sa = ws.Split(new char[] { '\n' });
				lp = lineNumber;
				foreach(string si in sa)
				{
					this.Add(si, lp);
					lp ++;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse the comments at the current and following lines.
		/// </summary>
		/// <param name="values">
		/// Collection of comments to which matching items will be appended.
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
		/// <returns>
		/// The Line Index following the comments.
		/// </returns>
		/// <remarks>
		/// The line in lines[index] must start with optional space and a
		/// comment sign, or no comment will be found in this call.
		/// </remarks>
		public static int Parse(CommentInfoCollection values,
			TextLineCollection lines, int index, int indent)
		{
			int ll = 0;					//	Last List Position.
			int lp = index;			//	List Position.
			int rv = index;			//	Return Value.

			if(values != null && lines != null && lines.Count > index)
			{
				ll = lp;
				lp = CommentInfoItem.Parse(values, lines, lp, indent);
				while(ll < lp)
				{
					//	Scan all sequentially connected comments into this
					//	group.
					ll = lp;
					lp = CommentInfoItem.Parse(values, lines, lp, indent);
				}
				rv = lp;
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ToString																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the string representation of all comments in the collection.
		/// </summary>
		/// <returns>
		/// Combination of all comments in this collection, joined by newline.
		/// </returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			foreach(CommentInfoItem ci in this)
			{
				if(sb.Length > 0 && ci.Value.Length > 0)
				{
					sb.Append("\r\n");
				}
				if(ci.Value.Length > 0)
				{
					sb.Append(ci.Value);
				}
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	CommentInfoItem																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about a Comment.
	/// </summary>
	public class CommentInfoItem
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
		//*	IsComment																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the caller-specified value is a
		/// Comment.
		/// </summary>
		/// <param name="value">
		/// Text value to inspect.
		/// </param>
		/// <returns>
		/// True if the caller's value can be identified as a comment. Otherwise,
		/// false.
		/// </returns>
		public static bool IsComment(string value)
		{
			bool rv = false;			//	Return Value.
			string ws = "";				//	Working String.

			if(value != null && value.Length > 0)
			{
				ws = value.Trim();
				if(ws.StartsWith("#") || ws.StartsWith("\"\"\""))
				{
					rv = true;
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	LineNumber																														*
		//*-----------------------------------------------------------------------*
		private int mLineNumber = 0;
		/// <summary>
		/// Get/Set the Line Number at which this instance was found.
		/// </summary>
		public int LineNumber
		{
			get { return mLineNumber; }
			set { mLineNumber = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse a single Comment into the caller's collection, and return the
		/// next active line index.
		/// </summary>
		/// <param name="values">
		/// Collection of comments to which matching items will be appended.
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
		/// <returns>
		/// Next line index to process. This method will return the value
		/// of the index parameter if no matches were found.
		/// </returns>
		public static int Parse(CommentInfoCollection values,
			TextLineCollection lines, int index, int indent)
		{
			bool bf = false;		//	Flag - Found.
			int lc = 0;					//	List Count.
			int lp = 0;					//	List Position.
			string ls = "";			//	Line String.
			string lt = "";			//	Trimmed Line.
			Match mi = null;		//	Working Match.
			string ps = "";			//	Pattern String.
			int rv = index;			//	Return Value.
			StringBuilder sb = null;	//	High volume string construction.
			string tp = "\\\"\\\"\\\"";			//	Triple quote pattern.
			string tq = "\"\"\"";						//	Triple quote literal.

			if(values != null && lines != null && lines.Count > index)
			{
				ls = lines[index].Value;
				lt = ls.Trim();
				if(IsComment(lines[index].Value))
				{
					//	The item is a comment.
					lc = lines.Count;
					lp = index;
					//	Start with single-line partial pattern.
					ps = Pattern;
					mi = Regex.Match(ls, ps);
					if(mi != null && mi.Success)
					{
						//	The comment is present.
						if(Utils.GetLength(mi, "space") >= indent)
						{
							//	The comment applies at this level.
							if(Utils.GetValue(mi, "comment") == tq)
							{
								//	Multiline Comment.
								//	Redefine the match. Closing sequence is required.
								ps = @"^(?<space>[ |\t]*)" + tp + @"\s*" +
									@"(?<content>.*?)\s*" + tp;
								mi = Regex.Match(ls, ps);
								bf = true;
								if(mi != null && mi.Success)
								{
									ls = Utils.GetValue(mi, "content");
									rv = lp + 1;
								}
								else
								{
									bf = false;
									sb = new StringBuilder();
									lp = Utils.GetTail(lines, lp, tq, sb, null);
									if(lp > -1)
									{
										//	Tail was found.
										ls = sb.ToString();
										mi = Regex.Match(ls, ps);
										if(mi != null && mi.Success)
										{
											bf = true;
											ls = Utils.GetValue(mi, "content");
											rv = lp;
										}
									}
								}
								if(bf)
								{
									//	Complete multi-line comment found.
									lt = ls.Trim();
									values.Add(lt, index);
								}
								else
								{
									//	Closing sequence not found.
									rv = index;
								}
							}
							else
							{
								//	Single line.
								values.Add(Utils.GetValue(mi, "content"), index);
								rv = index + 1;
							}
						}
						else
						{
							//	This item is outside the prior level.
							rv = index;
						}
					}
					else
					{
						//	This item is not a comment.
						rv = index;
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
			"^(?<space>[ |\\t]*)(?<comment>(\\#|\\\"\\\"\\\"))\\s*(?<content>.*)";
		/// <summary>
		/// Get the partial pattern for matching a full line comment.
		/// </summary>
		public static string Pattern
		{
			get { return mPattern; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Value																																	*
		//*-----------------------------------------------------------------------*
		private string mValue = "";
		/// <summary>
		/// Get/Set the Value of this Item.
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
