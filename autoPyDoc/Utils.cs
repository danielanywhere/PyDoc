//	Utils.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	Utils																																		*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Common utilities for this application.
	/// </summary>
	public class Utils
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		/// <summary>
		/// One-time load of Assembly info. Will stay active until application
		/// shuts down.
		/// </summary>
		private static Assembly mAssembly = null;
		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	GetCommentSingle																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return any single line comment present in the caller's string.
		/// </summary>
		/// <param name="source">
		/// Source text to inspect for comments.
		/// </param>
		/// <returns>
		/// Any single-line comment found on the line.
		/// </returns>
		/// <remarks>
		/// This version is capable of looking around all quote types to find
		/// the comment at the end, ignoring any false signal found inside the
		/// quoted text.
		/// </remarks>
		public static string GetCommentSingle(string source)
		{
			int ci = -1;			//	Comment starting character index.
			ExpressionStack ex = new ExpressionStack();	//	Head/Tail counter.
			MatchCollection mc = null;	//	Match sequence.
			string ps = "";		//	Pattern.
			string rv = "";		//	Return Value.
			string ws = "";		//	Working String.

			if(source != null && source.Length > 0)
			{
				ps = "((?<q>(\\\"\\\"\\\"|\\\"|\\\'))|(?<c>\\#))";
				mc = Regex.Matches(source, ps);
				if(mc != null && mc.Count > 0)
				{
					foreach(Match mi in mc)
					{
						ws = GetValue(mi, "q");
						if(ws.Length > 0)
						{
							if(ex.Count == 0 || ex.Peek().Value != ws)
							{
								//	Either the stack is empty, or this is the opening
								//	of a different kind of quote.
								ex.Push(ws);
							}
							else if(ex.Count > 0)
							{
								//	Pop the matching item from the stack.
								ex.Pop();
							}
						}
						else if(ex.Count == 0)
						{
							//	This wasn't a quote, and the comment can be used at this
							//	level because the stack is balanced.
							//	Everything from here forward is part of the comment.
							ci = GetIndex(mi, "c");
							break;
						}
					}
				}
				else
				{
					//	No quotes or comments present.
					ci = -1;
				}
				if(ci > -1)
				{
					rv = source.Substring(ci);
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetIndex																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the source index of the specified Group in the caller-specified
		/// Match.
		/// </summary>
		/// <param name="match">
		/// Reference to the instance of the match to inspect.
		/// </param>
		/// <param name="groupName">
		/// Name of the group for which the index will be retrieved.
		/// </param>
		/// <returns>
		/// Character index of the match group in the source string, if found.
		/// Otherwise, -1.
		/// </returns>
		public static int GetIndex(Match match, string groupName)
		{
			int rv = -1;				//	Return Value.

			if(match != null && match.Success &&
				groupName != null && groupName.Length > 0 &&
				match.Groups[groupName] != null &&
				match.Groups[groupName].Value != null)
			{
				rv = match.Groups[groupName].Index;
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetLength																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the length of the specified Group in the caller-specified Match.
		/// </summary>
		/// <param name="match">
		/// Reference to the instance of the match to inspect.
		/// </param>
		/// <param name="groupName">
		/// Name of the group for which the length of the match will be retrieved.
		/// </param>
		/// <returns>
		/// Length of the matching group, if found. Otherwise, 0.
		/// </returns>
		public static int GetLength(Match match, string groupName)
		{
			int rv = 0;				//	Return Value.

			if(match != null && match.Success &&
				groupName != null && groupName.Length > 0 &&
				match.Groups[groupName] != null &&
				match.Groups[groupName].Value != null)
			{
				rv = match.Groups[groupName].Value.Length;
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetRelativePath																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the relative directory name, using the difference between the
		/// base and a comparison.
		/// </summary>
		/// <param name="basePath">
		/// Base path to serve as the root.
		/// </param>
		/// <param name="comparePath">
		/// Path to be compared with the base.
		/// </param>
		/// <returns>
		/// Difference between base and comparison paths, if a difference existed.
		/// Otherwise, an empty string.
		/// </returns>
		public static string GetRelativePath(string basePath, string comparePath)
		{
			bool bf = false;		//	Flag - Found.
			string bs = "";			//	Lowercase Base Path.
			string cs = "";			//	Lowercase Compare Path.
			int cc = 0;					//	Character Count.
			int cp = 0;					//	Character Position.
			string rv = "";			//	Return Value.

			if(basePath != null && comparePath != null)
			{
				//	Both values have been provided.
				bs = basePath.ToLower();
				cs = comparePath.ToLower();
				cc = Math.Min(bs.Length, cs.Length);
				for(cp = 0; cp < cc; cp ++)
				{
					if(bs[cp] != cs[cp])
					{
						//	We found the unequal character.
						//	Even if the entire base path was not covered, this is where
						//	the two paths diverge.
						//	The portion of the compare path to the right of this location
						//	will constitute the relative path.
						bf = true;
						break;
					}
				}
				if(!bf)
				{
					//	All characters were equal to this point.
					if(cs.Length > bs.Length)
					{
						//	The right side of the compare path will be the relative path.
						cp = bs.Length;
						bf = true;
					}
				}
				if(bf && cp > 0)
				{
					//	The portion including the current character, and everything to its
					//	right, will be used to create the relative path.
					rv = comparePath.Substring(cp);
				}
				else if(bf)
				{
					//	If all characters were different, then the compare path is the
					//	relative path.
					rv = comparePath;
				}
			}
			else if(basePath == null && comparePath != null)
			{
				//	Only the comparison is non-null.
				//	Base is assumed to be root.
				rv = comparePath;
			}
			//	If only the base is non-null, then the comparison is equal to the
			//	base, and the relative path is therefore zero.
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetResourceTextEmbedded																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the text found in the specified embedded resource.
		/// </summary>
		/// <param name="resourceName">
		/// Full name of the embedded resource. Recall that embedded resources are
		/// prefixed by the default namespace for the project. In this case, PyDoc.
		/// </param>
		/// <returns>
		/// Text from the specified embedded resource, if found. Otherwise, an
		/// empty string.
		/// </returns>
		public static string GetResourceTextEmbedded(string resourceName)
		{
			string rv = "";			//	Return Value.
			StreamReader sr = null;

			if(mAssembly == null)
			{
				//	Initialize the assembly.
				try
				{
					mAssembly = Assembly.GetExecutingAssembly();
				}
				catch { }
			}
			if(mAssembly != null)
			{
				try
				{
					sr = new StreamReader(
						mAssembly.GetManifestResourceStream(resourceName));
					rv = sr.ReadToEnd();
					sr.Close();
				}
				catch { }
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetTail																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Find and set the tail content in the caller's string builder, returning
		/// the next active line.
		/// </summary>
		/// <param name="lines">
		/// Collection of source text lines.
		/// </param>
		/// <param name="index">
		/// Current index within the source text collection.
		/// </param>
		/// <param name="tailPattern">
		/// Pattern to be matched in order to find tail.
		/// </param>
		/// <param name="target">
		/// Target content string builder.
		/// </param>
		/// <param name="comments">
		/// Comments found on the content that will need to be handled by
		/// the caller.
		/// </param>
		/// <returns>
		/// Next active line, if tail found. Otherwise, -1.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The product of this method appears in the target string.
		/// </para>
		/// <para>
		/// The comments collection may contain comments that need to be
		/// assigned to this object once it is successfully identified and
		/// instantiated.
		/// </para>
		/// </remarks>
		public static int GetTail(TextLineCollection lines, int index,
			string tailPattern, StringBuilder target,
			CommentInfoCollection comments)
		{
			bool bf = false;	//	Flag - Found.
			string cs = "";		//	Comment.
			int lc = 0;				//	List Count.
			int lp = index;		//	List Position.
			string ls = "";		//	Line String.
			string lt = "";		//	Trimmed Line String.
			Match mi = null;	//	Variable Match.
			int rv = -1;			//	Return Value.
			StringBuilder sb = null;	//	Working String.

			if(lines != null && lines.Count > index &&
				tailPattern != null && tailPattern.Length > 0 &&
				target != null)
			{
				//	All values are legitimate.
				lc = lines.Count;
				ls = lines[lp].Value;
				if(comments != null)
				{
					cs = GetCommentSingle(ls);
					if(cs.Length > 0)
					{
						ls = RemoveCommentSingle(ls);
						comments.Add(cs, lp);
					}
				}
				if(tailPattern[0] == '\'' || tailPattern[0] == '\"' &&
					!IsTerminated(ls))
				{
					//	Quote.
					sb = new StringBuilder();
					lt = ls.Trim();
					sb.Append(lt);
					while(++ lp < lc && !IsTerminated(sb.ToString()))
					{
						ls = lines[lp].Value;
						lt = ls.Trim();
						if(sb.Length > 0 && lt.Length > 0)
						{
							sb.Append(" ");
						}
						sb.Append(lt);
					}
					if(IsTerminated(sb.ToString()))
					{
						bf = true;
						target.Append(sb.ToString());
						rv = lp;
					}
				}
				else
				{
					mi = Regex.Match(ls, VariableInfoItem.Pattern);
					if(GetLength(mi, "box") > 0 && !IsTerminated(ls))
					{
						//	The line is unterminated due to expression.
						sb = new StringBuilder();
						lp = GetVariableAssignment(lines, lp, sb, comments);
						if(lp > -1)
						{
							bf = true;
							ls = sb.ToString();
							target.Append(ls);
							rv = lp;
						}
					}
					if(!bf && ls.IndexOf(tailPattern) == -1)
					{
						//	No box characters, check for tail on current line.
						target.Append(ls);
						while(++ lp < lc)
						{
							ls = lines[lp].Value;
							if(comments != null)
							{
								cs = GetCommentSingle(ls);
								if(cs.Length > 0)
								{
									ls = RemoveCommentSingle(ls);
									comments.Add(cs, lp);
								}
							}
							lt = ls.Trim();
							target.Append(" ");
							target.Append(lt);
							if(ls.IndexOf(tailPattern) > -1)
							{
								rv = lp + 1;
								break;
							}
						}
					}
				}
				if(!bf)
				{
					rv = index;
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetValue																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the value of the specified Group in the caller-specified Match.
		/// </summary>
		/// <param name="match">
		/// Reference to the instance of the match to inspect.
		/// </param>
		/// <param name="groupName">
		/// Name of the group for which the value of the match will be retrieved.
		/// </param>
		/// <returns>
		/// Value of the match found in the specified group of the provided match,
		/// if any. If no match or group were found, the return value is an empty
		/// string.
		/// </returns>
		public static string GetValue(Match match, string groupName)
		{
			string rv = "";				//	Return Value.

			if(match != null && match.Success &&
				groupName != null && groupName.Length > 0 &&
				match.Groups[groupName] != null &&
				match.Groups[groupName].Value != null)
			{
				rv = match.Groups[groupName].Value;
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetVariableAssignment																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Find and place the variable expression content in the caller's string
		/// builder, returning the next active line.
		/// </summary>
		/// <param name="lines">
		/// Reference to a collection of source lines.
		/// </param>
		/// <param name="index">
		/// Current index within the source lines collection.
		/// </param>
		/// <param name="target">
		/// Target string builder that will receive the full assignment text
		/// of the expression.
		/// </param>
		/// <param name="comments">
		/// Comments encountered that will need to be handled by the host.
		/// </param>
		/// <returns>
		/// Next active line, if assignment found. Otherwise, -1.
		/// </returns>
		public static int GetVariableAssignment(TextLineCollection lines,
			int index, StringBuilder target, CommentInfoCollection comments)
		{
			string cs = "";		//	Comment String.
			ExpressionStack ex = new ExpressionStack();
			int lc = 0;				//	List Count.
			int lp = index;		//	List Position.
			string ls = "";		//	Line String.
			string lt = "";		//	Trimmed Line String.
			MatchCollection mc = null;		//	Match List.
			Match mi = null;	//	Working Match.
			string pk = "";		//	Peek Value.
			string ps = "";		//	Pattern.
			int rv = -1;			//	Return Value.

			//	The starting line contains the variable name and
			//	at least part of the assignment. In this instance, we
			//	only want the content to the right of the equal sign,
			//	and anything in further rows, if continuation marks are
			//	found.
			//	Continuation marks include opening brackets that have not
			//	yet been closed, commas, and mathematical operators.
			if(lines != null && lines.Count > index && target != null)
			{
				//	All incoming values are legitimate.
				lc = lines.Count;
				lp = index;
				ls = lines[lp].Value;

				if(ls.Length > 0 &&
					(ls.IndexOf("=") > -1 ||
					ls.IndexOf("yield") > -1 ||
					ls.IndexOf("return") > -1))
				{
					//	Starting off, we only want content right of the assignment.
					ps = @"^.*?(yield|return|" +
						@"(?<operator>[\+\-\*\/\%\&\|]?\s*\=))\s*(?<line>.*?)\s*$";
					ls = Regex.Replace(ls, ps, "${line}");
				}
				while(lp < lc)
				{
					lt = ls.Trim();
					if(!QuoteIsOpen(target.ToString()))
					{
						cs = GetCommentSingle(lt);
						if(cs.Length > 0)
						{
							lt = RemoveCommentSingle(lt);
							comments.Add(cs, index);
						}
					}
					if(lt.Length > 0)
					{
						if(target.Length > 0)
						{
							target.Append(" ");
						}
						target.Append(lt);
					}
					if(lt.Length > 0 && !QuoteIsOpen(target.ToString()))
					{
						//	Dangling Ends.
						ps = @"(?<dangler>[,\.\+\-\&\|\*\\/](?=\s*$))";
						mi = Regex.Match(lt, ps);
					}
					else
					{
						mi = null;
					}
					if(lt.Length == 0 || (mi != null && mi.Success))
					{
						//	Line ends in a dangler. Index to next line.
						if(++ lp < lc)
						{
							ls = lines[lp].Value;
						}
					}
					else
					{
						//	Test to see if all boxes are closed.
						//	Brackets.
						ex.Clear();
						ps = "(?<box>([\\[\\{\\(\\)\\}\\]]|\\\"\\\"\\\"|\\\"|\\\'))";
						mc = Regex.Matches(target.ToString(), ps);
						foreach(Match mci in mc)
						{
							mi = mci;
							if(mi.Success && mi.Groups["box"] != null &&
								mi.Groups["box"].Value != null)
							{
								if(ex.Count > 0)
								{
									pk = ex.Peek().Value;
								}
								else
								{
									pk = "";
								}
								lt = mi.Groups["box"].Value;
								switch(lt)
								{
									case "\"\"\"":
									case "\"":
									case "'":
										if(ex.Count == 0 || pk != lt)
										{
											ex.Push(lt);
										}
										else if(ex.Count > 0)
										{
											ex.Pop();
										}
										break;
									case "(":
									case "{":
									case "[":
										if(ex.Count == 0 ||
											(pk != "'" &&
											pk.Substring(0, 1) != "\""))
										{
											ex.Push(lt);
										}
										break;
									case "]":
									case "}":
									case ")":
										if(ex.Count > 0 &&
											pk != "'" &&
											pk.Substring(0, 1) != "\"")
										{
											ex.Pop();
										}
										break;
								}
							}
						}
						if(ex.Count > 0)
						{
							//	At least one bracket is still open. Index to next line.
							if(++lp < lc)
							{
								ls = lines[lp].Value;
							}
						}
						else
						{
							//	Success.
							rv = lp + 1;
							break;
						}
					}
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	IsTerminated																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the specified string is terminated.
		/// </summary>
		/// <param name="value">
		/// Value to test for fully terminated expression.
		/// </param>
		/// <returns>
		/// True if the value has no enclosing characters, or is fully terminated.
		/// Otherwise, false.
		/// </returns>
		/// <remarks>
		/// In this environment, value termination is bound by starting and
		/// ending quotes, brackets, and parenthsis, as well as the presence or
		/// absence of trailing punctuation. For example, a comma at the end of the
		/// line indicates that more is following on the next line.
		/// </remarks>
		public static bool IsTerminated(string value)
		{
			ExpressionStack ex = new ExpressionStack();
			string lt = "";				//	Trimmed Line.
			MatchCollection mc = null;	//	Working Matches.
			string pk = "";				//	Peek Value.
			string ps = "";				//	Pattern.
			bool rv = false;			//	Return Value.

			if(value != null && value.Length > 0)
			{
				lt = value.Trim();
				if(lt.Length > 0)
				{
					//	Dangling End.
					ps = @"(?<dangler>[,\.\+\-\&\|\*\\/](?=\s*$))";
					if(!Regex.IsMatch(lt, ps))
					{
						//	No dangling ends found.
						//	Brackets.
						ps = "(?<box>([\\[\\{\\(\\)\\}\\]]|\\\"\\\"\\\"|\\\"|\\\'))";
						mc = Regex.Matches(lt, ps);
						foreach(Match mi in mc)
						{
							if(mi.Success && mi.Groups["box"] != null &&
								mi.Groups["box"].Value != null)
							{
								if(ex.Count > 0)
								{
									pk = ex.Peek().Value;
								}
								else
								{
									pk = "";
								}
								lt = mi.Groups["box"].Value;
								switch(lt)
								{
									case "\"\"\"":
									case "\"":
									case "'":
										if(ex.Count == 0 || pk != lt)
										{
											ex.Push(lt);
										}
										else if(ex.Count > 0)
										{
											ex.Pop();
										}
										break;
									case "(":
									case "{":
									case "[":
										if(ex.Count == 0 ||
										(
											pk != "'" &&
											pk.Substring(0, 1) != "\"")
										)
										{
											ex.Push(lt);
										}
										break;
									case "]":
									case "}":
									case ")":
										if(ex.Count > 0 &&
											pk != "'" &&
											pk.Substring(0, 1) != "\"")
										{
											ex.Pop();
										}
										break;
								}
							}
						}
						if(ex.Count == 0)
						{
							rv = true;
						}
					}
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ParseAll																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse all members of the current level.
		/// </summary>
		/// <param name="container">
		/// Reference to an abstract container that might have multiple classes,
		/// methods, and variables.
		/// </param>
		/// <returns>
		/// Next line index within the source. If no matches were found at this
		/// level, this value will be equal to the container.Index value.
		/// </returns>
		public static int ParseAll(ContainerInfo container)
		{
			bool bc = true;			//	Flag - Continue.
			bool bf = false;		//	Flag - Line Found.
													//	Lead-in and File Comments.
			CommentInfoCollection cm = null;
			string cs = "";			//	Current Comment.
			int id = 0;					//	Working Indent.
			int lc = 0;					//	List Count.
			int ll = 0;					//	Last List Position.
			int lp = 0;					//	List Position.
			string ls = "";			//	Line String.
			string lt = "";			//	Trimmed Line String.
			Match mi = null;		//	Working Match.
			string ps = "";			//	Pattern for Start of entry.
			int rv = -1;				//	Next active line.
			int sp = 0;					//	Spaces.

			if(container != null)
			{
				cm = new CommentInfoCollection();
				rv = lp = container.Index;
				if(container.Lines != null && container.Lines.Count > container.Index)
				{
					lc = container.Lines.Count;
					//	Get all leading comments.
					//	Comments directly following the start of an entity are
					//	automatically assigned to that entity.
					lp = CommentInfoCollection.Parse(cm, container.Lines, lp, 0);
					if(cm.Count > 0 && container.Comments != null)
					{
						container.Comments.AddRange(cm);
						cm.Clear();
					}
					ll = lp;
					id = container.Indent;
					while(lp < lc)
					{
						//	Get the current line.
						ls = container.Lines[lp].Value;
						lt = ls.Trim();
						bf = (lt.Length == 0);
						//	Comment, Import, Class, Method, or Variable.
						if(bc && !bf && container.Comments != null)
						{
							ps = CommentInfoItem.Pattern;
							mi = Regex.Match(ls, ps);
							if(mi != null && mi.Success)
							{
								//	Comment Match.
								bf = true;
								sp = GetLength(mi, "space");
								if(sp >= id)
								{
									lp = CommentInfoCollection.Parse(cm,
										container.Lines, lp, sp);
									rv = lp;
								}
								else
								{
									bc = false;
								}
							}
							else
							{
								cs = Utils.GetCommentSingle(ls);
								if(cs.Length > 0)
								{
									cm.Add(cs, lp);
									ls = Utils.RemoveCommentSingle(ls);
								}
							}
						}
						if(bc && !bf && container.Imports != null)
						{
							//	Imports.
							ps = ImportInfoItem.Pattern;
							mi = Regex.Match(ls, ps);
							if(mi != null && mi.Success)
							{
								//	Import Match.
								bf = true;
								sp = Utils.GetLength(mi, "space");
								if(sp >= id)
								{
									lp = ImportInfoCollection.Parse(container.Imports,
										container.Lines, lp, sp, cm);
								}
								else
								{
									bc = false;
								}
							}
						}
						if(bc && !bf && container.Classes != null)
						{
							//	Class.
							ps = ClassInfoItem.Pattern;
							mi = Regex.Match(ls, ps);
							if(mi != null && mi.Success)
							{
								//	Class Match.
								bf = true;
								sp = Utils.GetLength(mi, "space");
								if(sp >= id)
								{
									lp = ClassInfoCollection.Parse(container.Classes,
										container.Lines, lp, sp, cm);
								}
								else
								{
									bc = false;
								}
							}
						}
						if(bc && !bf && container.Methods != null)
						{
							//	Method.
							ps = MethodInfoItem.Pattern;
							mi = Regex.Match(ls, ps);
							if(mi != null && mi.Success)
							{
								//	Method Match.
								bf = true;
								sp = Utils.GetLength(mi, "space");
								if(sp >= id)
								{
									lp = MethodInfoCollection.Parse(container.Methods,
										container.Lines, lp, sp, cm);
								}
								else
								{
									bc = false;
								}
							}
						}
						if(bc && !bf && container.Logic != null)
						{
							//	Conditional or branching logic.
							ps = LogicInfoItem.Pattern;
							mi = Regex.Match(ls, ps);
							if(mi != null && mi.Success)
							{
								//	Logic Match.
								bf = true;
								sp = Utils.GetLength(mi, "space");
								if(sp >= id)
								{
									lp = LogicInfoCollection.Parse(container.Logic,
										container.Lines, lp, sp, cm);
								}
								else
								{
									bc = false;
								}
							}
						}
						if(bc && !bf && container.Variables != null)
						{
							//	Variable.
							ps = VariableInfoItem.Pattern;
							mi = Regex.Match(ls, ps);
							if(mi != null && mi.Success)
							{
								//	Variable Match.
								bf = true;
								sp = Utils.GetLength(mi, "space");
								if(sp >= id)
								{
									lp = VariableInfoCollection.Parse(container.Variables,
										container.Lines, lp, sp, cm);
								}
								else
								{
									bc = false;
								}
							}
						}
						if(!bc)
						{
							//	Don't continue.
							if(cm.Count > 0)
							{
								//	Add any outstanding comments to the container.
								container.Comments.AddRange(cm);
								cm.Clear();
							}
							break;
						}
						if(lp == ll)
						{
							if(lt.Length == 0 && cm.Count > 0)
							{
								//	Add any outstanding comments to the container.
								container.Comments.AddRange(cm);
								cm.Clear();
							}
							lp ++;
						}
						ll = lp;
						rv = ll;
					}
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	QuoteIsOpen																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether a quote is open at the end of the
		/// provided line.
		/// </summary>
		/// <param name="value">
		/// Value to inspect.
		/// </param>
		/// <returns>
		/// True if quoted text within the value has been left unterminated.
		/// </returns>
		public static bool QuoteIsOpen(string value)
		{
			ExpressionStack ex = new ExpressionStack();		//	Bracket stack.
			MatchCollection mc = null;	//	Working Matches.
			string ps = "";				//	Pattern.
			bool rv = false;			//	Return Value.
			string qt = "";				//	Current Quote.

			if(value != null)
			{
				ps = "(?<qt>(\\\"\\\"\\\")|(\\\"))";
				mc = Regex.Matches(value, ps);
				foreach(Match mi in mc)
				{
					qt = GetValue(mi, "qt");
					if(ex.Count == 0 || ex.Peek().Value != qt)
					{
						ex.Push(qt);
					}
					else if(ex.Count > 0)
					{
						ex.Pop();
					}
				}
				rv = (ex.Count != 0);
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RemoveCommentSingle																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the supplied line with any single line comment removed.
		/// </summary>
		/// <param name="source">
		/// Text to inspect.
		/// </param>
		/// <returns>
		/// Source text with any single-line comments removed.
		/// </returns>
		public static string RemoveCommentSingle(string source)
		{
			string rv = source;		//	Return Value.
			string ws = "";				//	Working String.

			ws = GetCommentSingle(source);
			if(ws.Length > 0)
			{
				if(ws.Length < source.Length)
				{
					rv = source.Substring(0, source.Length - ws.Length);
				}
				else
				{
					rv = "";
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RemoveFileExtension																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the caller's filename with the extension removed.
		/// </summary>
		/// <param name="value">
		/// Filename to inspect.
		/// </param>
		/// <param name="includePath">
		/// Value indicating whether any path portion found on the filename should
		/// be retained in the result.
		/// </param>
		/// <returns>
		/// Filename without extension.
		/// </returns>
		public static string RemoveFileExtension(string value, bool includePath)
		{
			Match mi = null;		//	Working Match.
			string pd = "";			//	Path delimiter.
			StringBuilder sb = new StringBuilder();	//	Working String.
			string ws = "";			//	Working String.

			if(value != null && value.Length > 0)
			{
				mi = Regex.Match(value,
					@"(?<path>((?<dir>.*(?=[\\\/]))(?<delim>[\\\/]*)){0,1}" +
					@"(?<file>[^\.]*)(?<ext>.*)$)");
				if(includePath)
				{
					ws = GetValue(mi, "dir");
					pd = GetValue(mi, "delim");
					if(ws.Length > 0 && pd.Length > 0)
					{
						sb.Append(ws);
						sb.Append(pd);
					}
				}
				sb.Append(GetValue(mi, "file"));
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ReplaceNonAN																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value where the caller's non-alphanumeric strings have been
		/// replaced.
		/// </summary>
		/// <param name="value">
		/// Value to inspect for non-alphanumeric characters.
		/// </param>
		/// <param name="replacement">
		/// Pattern to insert in place of any non-alphanumeric characters found.
		/// </param>
		/// <returns>
		/// Version of value that has had all instances of alphanumeric characters
		/// replaced with the replacement pattern.
		/// </returns>
		public static string ReplaceNonAN(string value, string replacement)
		{
			return ReplaceNonAN(value, replacement, new char[0]);
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Return a value where the caller's non-alphanumeric strings have been
		/// replaced.
		/// </summary>
		/// <param name="value">
		/// Value to inspect for non-alphanumeric characters.
		/// </param>
		/// <param name="replacement">
		/// Pattern to insert in place of any non-alphanumeric characters found.
		/// </param>
		/// <param name="exceptions">
		/// Non-alphanumeric exceptions to allow. For example, dash and underscore
		/// are commonly legal in named entities.
		/// </param>
		/// <returns>
		/// Version of value that has had all instances of alphanumeric characters
		/// replaced with the replacement pattern.
		/// </returns>
		public static string ReplaceNonAN(string value, string replacement,
			char[] exceptions)
		{
			string es = "";		//	Exception string.
			string rv = "";
			StringBuilder sb = null;

			if(value != null && value.Length > 0 && replacement != null)
			{
				sb = new StringBuilder();
				if(exceptions != null && exceptions.Length > 0)
				{
					//	Process exceptions.
					foreach(char ec in exceptions)
					{
						sb.Append(@"\");
						sb.Append(ec);
					}
					es = sb.ToString();
					sb.Remove(0, sb.Length);
				}
				sb.Append(@"(?i:(?<f>[^0-9a-z");
				sb.Append(es);
				sb.Append(@"]+))");
				rv = Regex.Replace(value, sb.ToString(), replacement);
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	StatDefaultCSS																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Get the constant Default.css content for the Statistics File.
		/// </summary>
		public static string StatDefaultCSS
		{
			get { return GetResourceTextEmbedded("PyDoc.StatDefaultCSS.txt"); }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	StatFileFooter																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Get the constant portion of the Statistics File Footer.
		/// </summary>
		public static string StatFileFooter
		{
			get { return GetResourceTextEmbedded("PyDoc.StatFooter.txt"); }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	StatFileHeader																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Get the constant portion of the Statistics File Header.
		/// </summary>
		public static string StatFileHeader
		{
			get { return GetResourceTextEmbedded("PyDoc.StatHeader.txt"); }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ToHtml																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the caller's content, formatted for HTML.
		/// </summary>
		/// <param name="content">
		/// Content to format.
		/// </param>
		/// <returns>
		/// Caller's content, valid for inclusion within HTML.
		/// </returns>
		public static string ToHtml(string content)
		{
			string rv = "";				//	Return value.

			if(content != null && content.Length > 0)
			{
				rv = content.Replace("&", "&amp;")
					.Replace("'", "&apos;")
					.Replace("\"", "&quot;")
					.Replace("<", "&lt;")
					.Replace(">", "&gt;")
					.Replace("\r\n", "<br />")
					.Replace("\n", "<br />");
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
