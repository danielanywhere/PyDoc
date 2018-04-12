//	MethodInfo.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	MethodInfoCollection																										*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of MethodInfoItem Items.
	/// </summary>
	public class MethodInfoCollection : List<MethodInfoItem>, IStatCollection
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
		//*	_Constructor																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Create a new Instance of the MethodInfoCollection Item.
		/// </summary>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		public MethodInfoCollection(IDrillableStat parentStat)
		{
			mParentStat = parentStat;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Add																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Add a new item to the collection.
		/// </summary>
		/// <param name="item">
		/// Reference to the item to be added.
		/// </param>
		public new void Add(MethodInfoItem item)
		{
			if(item.Parent == null)
			{
				item.Parent = this;
			}
			base.Add(item);
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Document																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Individually document each method in the specified collection.
		/// </summary>
		/// <param name="methods">
		/// Collection of methods to document.
		/// </param>
		/// <param name="targetFolder">
		/// Target file folder to which all files will be written.
		/// </param>
		/// <returns>
		/// Value indicating success, if true.
		/// </returns>
		public static bool Document(MethodInfoCollection methods,
			DirectoryInfo targetFolder)
		{
			FileInfoItem fi = null;				//	Working File.
			bool rv = true;
			string[] sa = new string[0];	//	General string array.
			StringBuilder sb = new StringBuilder();
			StreamWriter sw = null;				//	Text Writer.
			string ws = "";								//	Working string.

			if(methods != null && targetFolder != null && targetFolder.Exists)
			{
				//	Caller values were valid.
				foreach(MethodInfoItem mi in methods)
				{
					//	Clear the previous file contents.
					if(sb.Length > 0)
					{
						sb.Remove(0, sb.Length);
					}
					fi = mi.GetFile();
					ws = Utils.StatFileHeader;
					ws = ws.Replace("{Title}", "Method - " + mi.Name);
					sb.Append(ws);
					sb.Append("<h1>Method</h1>\r\n");
					//	Background Information.
					sb.Append("<!-- Information -->\r\n");
					sb.Append("<h2 class=\"id\">");
					sb.Append(mi.Name);
					sb.Append("</h2>\r\n");
					sb.Append("<table class=\"grid\">\r\n");
					sb.Append("<tr><th>Name</th><th class=\"buffer\">Value</th></tr>");
					sb.Append("<tr><td>Information:</td><td class=\"info\">");
					sb.Append(Utils.ToHtml(mi.Comments.ToString()));
					sb.Append("</td></tr>\r\n");
					if(fi != null)
					{
						sb.Append("<tr><td>File:</td>");
						sb.Append("<td><a href=\"");
						sb.Append(fi.GetDocName());
						sb.Append(".html");
						sb.Append("\">");
						sb.Append(fi.Filename);
						sb.Append("</a></td></tr>\r\n");
						sb.Append("<tr><td>Directory:</td><td>");
						ws = fi.DirectoryName;
						if(ws.Length == 0)
						{
							ws = "./";
						}
						sb.Append(ws);
						sb.Append("</td></tr>");
					}
					sb.Append("</table>\r\n");
					sb.Append("<!-- /Information -->\r\n");
					//	Construction Parameters.
					sb.Append("<!-- Parameters -->\r\n");
					sb.Append("<div class=\"level\">");
					sb.Append("<div class=\"title\">Parameters</div>\r\n");
					sb.Append("<p>Parameters for this method.</p>\r\n");
					if(mi.Parameters.Length > 0)
					{
						sb.Append("<blockquote>");
						sb.Append(mi.Parameters);
						sb.Append("</blockquote>\r\n");
					}
					else
					{
						sb.Append("<p class=\"gray\">(None)</p>\r\n");
					}
					sb.Append("</div>\r\n");
					sb.Append("<!-- /Parameters -->\r\n");
					//	Imports.
					sb.Append(ImportInfoCollection.Summarize(mi.Imports, mi,
						"Imports referenced directly from this method."));
					//	Classes.
					sb.Append(ClassInfoCollection.Summarize(mi.Classes, mi,
						"Member classes of this item."));
					//	Methods.
					sb.Append(MethodInfoCollection.Summarize(mi.Methods, mi,
						"Sub-methods of this method."));
					//	Variables.
					sb.Append(VariableInfoCollection.Summarize(mi.Variables, mi,
						"Variables accessed by this method."));
					//	Place the constant footer.
					sb.Append(Utils.StatFileFooter);
					//	Write the file content.
					sw = File.CreateText(targetFolder.FullName + @"\" +
						mi.GetDocName() + ".html");
					sw.WriteLine(sb.ToString());
					sw.Flush();
					sw.Close();
					sw.Dispose();
					//	Document all of the members of the method independently.
					if(mi.Classes != null && mi.Classes.Count > 0)
					{
						ClassInfoCollection.Document(mi.Classes, targetFolder);
					}
					if(mi.Methods != null && mi.Methods.Count > 0)
					{
						MethodInfoCollection.Document(mi.Methods, targetFolder);
					}
					if(mi.Variables != null && mi.Variables.Count > 0)
					{
						VariableInfoCollection.Document(mi.Variables, targetFolder);
					}
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	EnumerateClasses																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return an enumeration of the member classes of the specified instance.
		/// </summary>
		/// <param name="item">
		/// Reference to the method for which classes will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of member classes.
		/// </returns>
		public static string EnumerateClasses(MethodInfoItem item)
		{
			StringBuilder sb = new StringBuilder();

			if(item != null && item.Classes.Count > 0)
			{
				sb.Append("<ul class=\"member\">\r\n");
				foreach(ClassInfoItem ci in item.Classes)
				{
					sb.Append("<li>");
					sb.Append("<a href=\"");
					sb.Append(ci.GetDocName());
					sb.Append(".html");
					sb.Append("\">");
					sb.Append(ci.Name);
					sb.Append("</a></li>\r\n");
				}
				sb.Append("</ul>\r\n");
			}
			else
			{
				sb.Append("&nbsp;");
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	EnumerateMethods																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return an enumeration of the child methods in the specified instance.
		/// </summary>
		/// <param name="item">
		/// Reference to the method for which methods will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of member methods.
		/// </returns>
		public static string EnumerateMethods(MethodInfoItem item)
		{
			StringBuilder sb = new StringBuilder();

			if(item != null && item.Methods.Count > 0)
			{
				sb.Append("<ul class=\"member\">\r\n");
				foreach(MethodInfoItem mi in item.Methods)
				{
					sb.Append("<li>");
					sb.Append("<a href=\"");
					sb.Append(mi.GetDocName());
					sb.Append(".html");
					sb.Append("\">");
					sb.Append(mi.Name);
					sb.Append("(");
					sb.Append(mi.Parameters);
					sb.Append(")");
					sb.Append("</a></li>\r\n");
				}
				sb.Append("</ul>\r\n");
			}
			else
			{
				sb.Append("&nbsp;");
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	EnumerateVariables																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return an enumeration of the variables in the specified instance.
		/// </summary>
		/// <param name="item">
		/// Reference to the method for which variables will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of member variables.
		/// </returns>
		public static string EnumerateVariables(MethodInfoItem item)
		{
			NameValuesCollection nc = null;	//	Unique name, multiple values.
			StringBuilder sb = new StringBuilder();
			VariableInfoItem vi = null;			//	Tangible variable.

			if(item != null && item.Variables.Count > 0)
			{
				nc = new NameValuesCollection();
				foreach(VariableInfoItem vm in item.Variables)
				{
					nc.Add(vm.Name, vm.Assignment, true);
				}
				sb.Append("<ul class=\"member\">\r\n");
				foreach(NameValuesItem ni in nc)
				{
					vi = VariableInfoCollection.GetName(item.Variables, ni.Name);
					sb.Append("<li>");
					sb.Append("<a href=\"");
					sb.Append(vi.GetDocName());
					sb.Append(".html");
					sb.Append("\">");
					sb.Append((vi.Name.Length > 0 ? vi.Name : "*"));
					sb.Append("</a></li>\r\n");
				}
				sb.Append("</ul>\r\n");
			}
			else
			{
				sb.Append("&nbsp;");
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ParentStat																														*
		//*-----------------------------------------------------------------------*
		private IDrillableStat mParentStat = null;
		/// <summary>
		/// Get/Set a reference to the Parent Statistic.
		/// </summary>
		public IDrillableStat ParentStat
		{
			get { return mParentStat; }
			set { mParentStat = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse a method and its members.
		/// </summary>
		/// <param name="values">
		/// Collection of methods to which matching items will be appended.
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
		public static int Parse(MethodInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			int lc = 0;								//	List Count.
			int ll = 0;								//	Last List Position.
			int lp = 0;								//	List Position.
			int rv = index;						//	Return Value.

			if(values != null && lines != null && lines.Count > index)
			{
				//	Legal values provided.
				//	The caller already expects that this is a Def, but doesn't
				//	know whether the definition is a single line, or multiple.
				lc = lines.Count;
				lp = index;
				ll = lp;
				lp = MethodInfoItem.Parse(values, lines, lp, indent, comments);
				while(ll < lp && lp < lc)
				{
					ll = lp;
					lp = MethodInfoItem.Parse(values, lines, lp, indent, comments);
				}
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
		/// <param name="methodDescription">
		/// Description to place within the section, as it applies to the caller's
		/// context.
		/// </param>
		/// <returns>
		/// String containing formatted text that can be included in a host file.
		/// </returns>
		public static string Summarize(MethodInfoCollection collection,
			IDrillableStat parentStat, string methodDescription)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("<!-- Methods -->\r\n");
			sb.Append("<div class=\"level\"><div class=\"title\">");
			sb.Append("Methods");
			sb.Append("</div>\r\n");
			sb.Append("<p>");
			sb.Append(methodDescription);
			sb.Append("</p>\r\n");
			if(collection != null && collection.Count > 0)
			{
				sb.Append("<table class=\"grid\">\r\n");
				sb.Append("<tr><th>Name</th><th colspan=\"3\" class=\"info\">");
				sb.Append("Information</th></tr>\r\n");
				foreach(MethodInfoItem mi in collection)
				{
					sb.Append("<tr>\r\n");
					//	Method Name.
					sb.Append("<td rowspan=\"3\"><a href=\"");
					sb.Append(mi.GetDocName());
					sb.Append(".html");
					sb.Append("\">");
					sb.Append(mi.Name);
					sb.Append("</a></td>\r\n");
					//	Comments.
					sb.Append("<td colspan=\"3\" class=\"info\">");
					sb.Append(Utils.ToHtml(mi.Comments.ToString()));
					sb.Append("</td>\r\n");
					sb.Append("</tr>\r\n");
					//	Descriptor Headings.
					sb.Append("<tr>\r\n");
					sb.Append("<th class=\"class\">Classes</th>\r\n");
					sb.Append("<th class=\"method\">Methods</th>\r\n");
					sb.Append("<th class=\"variable\">Variables</th>\r\n");
					sb.Append("</tr>\r\n");
					//	Descriptors.
					sb.Append("<tr>\r\n");
					sb.Append("<td>\r\n");
					sb.Append(MethodInfoCollection.EnumerateClasses(mi));
					sb.Append("</td>\r\n");
					sb.Append("<td>\r\n");
					sb.Append(MethodInfoCollection.EnumerateMethods(mi));
					sb.Append("</td>\r\n");
					sb.Append("<td>\r\n");
					sb.Append(MethodInfoCollection.EnumerateVariables(mi));
					sb.Append("</td>\r\n");
					sb.Append("</tr>\r\n");
				}
				sb.Append("</table>\r\n");
			}
			else
			{
				sb.Append("<p class=\"gray\">(None)</p>\r\n");
			}
			sb.Append("</div>\r\n");
			sb.Append("<!-- /Methods -->\r\n");
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	MethodInfoItem																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about a Method.
	/// </summary>
	public class MethodInfoItem : ContainerBase
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
		//*	_Constructor																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Create a new Instance of the MethodInfoItem Item.
		/// </summary>
		public MethodInfoItem() : base()
		{
			DocPrefix = "_M_";
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Create a new Instance of the MethodStatItem Item.
		/// </summary>
		/// <param name="parent">
		/// Reference to the parent collection to which this item belongs.
		/// </param>
		public MethodInfoItem(MethodInfoCollection parent) : this()
		{
			Parent = parent;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse the contents of a method and its members.
		/// </summary>
		/// <param name="values">
		/// Sibling method definitions at this level.
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
		public static int Parse(MethodInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			MethodInfoItem ci = null;	//	New Method.
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
				//	This line begins a class/def definition.
				sb = new StringBuilder();
				lp = Utils.GetTail(lines, lp, ":", sb, comments);
				if(lp > -1)
				{
					//	Declaration was found.
					//	Get the full workup.
					ps = @"(?i:^(?<space>[ |\t]*)def\s+" +
					@"(?<name>[^\:\(]+)(\((?<params>[^\)]*)\)){0,1}\s*\:\s*$)";
					mi = Regex.Match(ls, ps);
					if(mi != null && mi.Success)
					{
						//	The method has been found. Create an item.
						//	Get Line, Indent, Name, and Params.
						ci = new MethodInfoItem();
						ci.LineNumber = index;
						ci.IndentLevel = Utils.GetLength(mi, "space");
						ci.Name = Utils.GetValue(mi, "name");
						ci.Parameters = Utils.GetValue(mi, "params");
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
			@"(?i:^(?<space>[ |\t]*)def\s+(?<name>[^\:\(]+))";
		/// <summary>
		/// Get the pattern for partially matching a Method.
		/// </summary>
		public static string Pattern
		{
			get { return mPattern; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
