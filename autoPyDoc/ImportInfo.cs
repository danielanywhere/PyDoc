//	ImportInfo.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	ImportInfoCollection																										*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of ImportInfoItem Items.
	/// </summary>
	public class ImportInfoCollection : List<ImportInfoItem>
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
		/// Add a new item to the collection by member values.
		/// </summary>
		/// <param name="library">
		/// Name of the library to import from.
		/// </param>
		/// <param name="import">
		/// Name of the object to import from the specified library, if any. If
		/// no object is specified, * is assumed.
		/// </param>
		/// <param name="indent">
		/// Current indent level at which this statement is set.
		/// </param>
		/// <param name="comments">
		/// Comments to be associated with the new item.
		/// </param>
		/// <returns>
		/// A reference to the added in the case of a single item, or the last
		/// item added in the case of multiple items added by comma delimiter.
		/// </returns>
		public ImportInfoItem Add(string library, string import, int indent,
			CommentInfoCollection comments)
		{
			string[] ia = new string[0];
			string[] la = new string[0];
			string ni = "";		//	Import Name.
			string nl = "";		//	Library Name.
			ImportInfoItem ro = null;

			if(library != null && library.Length > 0)
			{
				nl = library;
				if(import != null && import.Length > 0)
				{
					ni = import;
					if(import.IndexOf(",") > -1)
					{
						ia = import.Split(new char[] { ',' });
						foreach(string ii in ia)
						{
							ro = new ImportInfoItem();
							if(comments != null)
							{
								ro.Comments.AddRange(comments);
							}
							ro.IndentLevel = indent;
							ro.Library = nl;
							ro.ObjectName = ii.Trim();
							this.Add(ro);
						}
					}
					else
					{
						ro = new ImportInfoItem();
						if(comments != null)
						{
							ro.Comments.AddRange(comments);
						}
						ro.IndentLevel = indent;
						ro.Library = nl;
						ro.ObjectName = ni;
						this.Add(ro);
					}
				}
			}
			else if(import != null)
			{
				//	Only the import has been supplied, meaning that we are given
				//	access to the complete library.
				nl = import;
				if(nl.IndexOf(",") > -1)
				{
					la = nl.Split(new char[] { ',' });
					foreach(string li in la)
					{
						ro = new ImportInfoItem();
						if(comments != null)
						{
							ro.Comments.AddRange(comments);
						}
						ro.IndentLevel = indent;
						ro.Library = li.Trim();
						this.Add(ro);
					}
				}
				else
				{
					ro = new ImportInfoItem();
					if(comments != null)
					{
						ro.Comments.AddRange(comments);
					}
					ro.IndentLevel = indent;
					ro.Library = nl;
					this.Add(ro);
				}
			}
			if(ro == null)
			{
				ro = new ImportInfoItem();
				if(comments != null)
				{
					ro.Comments.AddRange(comments);
				}
				ro.IndentLevel = indent;
				this.Add(ro);
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse one or more sequential import statements.
		/// </summary>
		/// <param name="values">
		/// Collection of imports to which matching items will be appended.
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
		/// Next line index to process. This method will return the value
		/// of the index parameter if no matches were found.
		/// </returns>
		public static int Parse(ImportInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			int lp = index;			//	List Position.
			int rv = index;			//	Return Value.

			if(values != null && lines != null && lines.Count > index)
			{
				lp = ImportInfoItem.Parse(values, lines, lp, indent, comments);
				rv = lp;
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Summarize																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Summarize the members of this collection for inclusion in an
		/// enumeration.
		/// </summary>
		/// <param name="collection">
		/// Reference to the collection to be inspected.
		/// </param>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		/// <param name="importDescription">
		/// Description to place within the section, as it applies to the caller's
		/// context.
		/// </param>
		/// <returns>
		/// String containing formatted text that can be included in a host file.
		/// </returns>
		public static string Summarize(ImportInfoCollection collection,
			IDrillableStat parentStat, string importDescription)
		{
			StringBuilder sb = new StringBuilder();
			string ws = "";			//	Working String.

			sb.Append("<!-- Imports -->\r\n");
			sb.Append("<div class=\"level\"><div class=\"title\">");
			sb.Append("Imports");
			sb.Append("</div>\r\n");
			sb.Append("<p>");
			sb.Append(importDescription);
			sb.Append("</p>\r\n");
			if(collection != null && collection.Count > 0)
			{
				sb.Append("<table class=\"grid\">\r\n");
				sb.Append("<tr><th>Library</th><th>Object</th></tr>\r\n");
				foreach(ImportInfoItem ii in collection)
				{
					if(ii.Library.Length > 0 || ii.ObjectName.Length > 0)
					{
						sb.Append("<tr>\r\n");
						sb.Append("<td>");
						//sb.Append("<a href=\"");
						//sb.Append(ImportInfoItem.GetDocNameLibrary(ii, parentStat));
						//sb.Append(".html");
						//sb.Append("\">");
						ws = ii.Library;
						if(ws.Length == 0)
						{
							ws = "&nbsp;";
						}
						sb.Append(ws);
						//sb.Append("</a>");
						sb.Append("</td>\r\n");
						sb.Append("<td>");
						//sb.Append("<a href=\"");
						//sb.Append(ImportInfoItem.GetDocNameObject(ii, parentStat));
						//sb.Append(".html");
						//sb.Append("\">");
						ws = ii.ObjectName;
						if(ws.Length == 0)
						{
							ws = "*";
						}
						sb.Append(ws);
						//sb.Append("</a>");
						sb.Append("</td>\r\n");
						sb.Append("</tr>\r\n");
					}
				}
				sb.Append("</table>\r\n");
			}
			else
			{
				sb.Append("<p class=\"gray\">(None)</p>\r\n");
			}
			sb.Append("</div>\r\n");
			sb.Append("<!-- /Imports -->\r\n");
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ImportInfoItem																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about an Import.
	/// </summary>
	public class ImportInfoItem
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
		//*	Comments																															*
		//*-----------------------------------------------------------------------*
		private CommentInfoCollection mComments = new CommentInfoCollection();
		/// <summary>
		/// Get a reference to the Comments for this Variable.
		/// </summary>
		public CommentInfoCollection Comments
		{
			get { return mComments; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetDocNameLibrary																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the full document name of this item's library only. Include the
		/// prefixes set by parent objects.
		/// </summary>
		/// <param name="item">
		/// Reference to the item for which the document name is to be retrieved.
		/// </param>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		/// <returns>
		/// Full document name of this item, including prefixes set by parent
		/// objects.
		/// </returns>
		/// <remarks>
		/// This method does not include the file extension, since documentation
		/// can be written in different file formats. The final extension will be
		/// appended by the formatter.
		/// </remarks>
		public static string GetDocNameLibrary(ImportInfoItem item,
			IDrillableStat parentStat)
		{
			StringBuilder sb = new StringBuilder();

			if(parentStat != null)
			{
				sb.Append(parentStat.GetDocName());
			}
			if(item.Library.Length > 0)
			{
				sb.Append("_L_");
				sb.Append(item.Library);
			}
			else if(item.ObjectName.Length > 0)
			{
				sb.Append("_L_");
				sb.Append(item.ObjectName);
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetDocNameObject																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the full document name of this item's object, including the
		/// prefixes set by parent objects.
		/// </summary>
		/// <param name="item">
		/// Reference to the item for which the document name will be retrieved.
		/// </param>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		/// <returns>
		/// Full document name of this item, including prefixes set by parent
		/// objects.
		/// </returns>
		/// <remarks>
		/// This method does not include the file extension, since documentation
		/// can be written in different file formats. The final extension will be
		/// appended by the formatter.
		/// </remarks>
		public static string GetDocNameObject(ImportInfoItem item,
			IDrillableStat parentStat)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(GetDocNameLibrary(item, parentStat));
			sb.Append("_I_");
			if(item.ObjectName.Length != 0)
			{
				sb.Append(item.ObjectName);
			}
			else
			{
				sb.Append("-asterisk-");
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	IndentLevel																														*
		//*-----------------------------------------------------------------------*
		private int mIndentLevel = 0;
		/// <summary>
		/// Get/Set the Indent Level of this Item.
		/// </summary>
		public int IndentLevel
		{
			get { return mIndentLevel; }
			set { mIndentLevel = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Library																																*
		//*-----------------------------------------------------------------------*
		private string mLibrary = "";
		/// <summary>
		/// Get/Set the name of the library from which this import is read.
		/// </summary>
		public string Library
		{
			get { return mLibrary; }
			set { mLibrary = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ObjectName																														*
		//*-----------------------------------------------------------------------*
		private string mObjectName = "";
		/// <summary>
		/// Get/Set the name of the object or collection to be imported from the
		/// specified library.
		/// </summary>
		public string ObjectName
		{
			get { return mObjectName; }
			set { mObjectName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse one import statement at the current line location.
		/// </summary>
		/// <param name="values">
		/// Collection of imports to which matching items will be appended.
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
		/// Next line index to process. This method will return the value
		/// of the index parameter if no matches were found.
		/// </returns>
		public static int Parse(ImportInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			string cs = "";			//	Comment.
			int lc = 0;					//	List Count.
			int lp = index;			//	List Position.
			string ls = "";			//	Line String.
			Match mi = null;		//	Working Match.
			int sp = 0;					//	Spaces.
			int rv = index;			//	Return Value.

			if(values != null && lines != null && lines.Count > index)
			{
				lc = lines.Count;
				lp = index;
				ls = lines[lp].Value;

				mi = Regex.Match(ls, Pattern);
				while(mi != null && mi.Success)
				{
					sp = Utils.GetLength(mi, "space");
					if(sp >= indent)
					{
						values.Add(
							Utils.GetValue(mi, "libname"), Utils.GetValue(mi, "impname"),
							indent, comments);
						if(comments.Count > 0)
						{
							comments.Clear();
						}
						if(++lp < lc)
						{
							ls = lines[lp].Value;
							cs = Utils.GetCommentSingle(ls);
							if(cs.Length > 0)
							{
								ls = Utils.RemoveCommentSingle(ls);
								comments.Add(cs, lp);
							}
							mi = Regex.Match(ls, Pattern);
						}
						else
						{
							mi = null;
						}
						rv = lp;
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
			@"(?i:(?<space>[ |\t]*)" +
			@"(?<from>from\s+(?<libname>[0-9a-z\.-_]+)\s+){0,1}" +
			@"(?<import>import\s+(?<impname>.*))\s*)";
		/// <summary>
		/// Get the pattern used to match a partial import.
		/// </summary>
		public static string Pattern
		{
			get { return mPattern; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
