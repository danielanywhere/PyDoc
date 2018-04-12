//	ClassInfo.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using	System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	ClassInfoCollection																											*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of ClassInfoItem Items.
	/// </summary>
	public class ClassInfoCollection : List<ClassInfoItem>, IStatCollection
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
		/// Create a new Instance of the ClassStatCollection Item.
		/// </summary>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		public ClassInfoCollection(IDrillableStat parentStat)
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
		public new void Add(ClassInfoItem item)
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
		/// Individually document each class in the specified collection.
		/// </summary>
		/// <param name="classes">
		/// Collection of classes to document.
		/// </param>
		/// <param name="targetFolder">
		/// Target file folder to which rendered files will be written.
		/// </param>
		/// <returns>
		/// Value indicating success, if true.
		/// </returns>
		public static bool Document(ClassInfoCollection classes,
			DirectoryInfo targetFolder)
		{
			FileInfoItem fi = null;				//	Working File.
			bool rv = true;
			string[] sa = new string[0];	//	General string array.
			StringBuilder sb = new StringBuilder();
			StreamWriter sw = null;				//	Text Writer.
			string ws = "";								//	Working string.

			if(classes != null && targetFolder != null && targetFolder.Exists)
			{
				//	Caller values were valid.
				foreach(ClassInfoItem ci in classes)
				{
					//	Clear the previous file contents.
					if(sb.Length > 0)
					{
						sb.Remove(0, sb.Length);
					}
					fi = ci.GetFile();
					ws = Utils.StatFileHeader;
					ws = ws.Replace("{Title}", "Class - " + ci.Name);
					sb.Append(ws);
					sb.Append("<h1>Class</h1>\r\n");
					//	Background Information.
					sb.Append("<!-- Information -->\r\n");
					sb.Append("<h2 class=\"id\">");
					sb.Append(ci.Name);
					sb.Append("</h2>\r\n");
					sb.Append("<table class=\"grid\">\r\n");
					sb.Append("<tr><th>Name</th><th class=\"buffer\">Value</th></tr>");
					sb.Append("<tr><td>Information:</td><td class=\"info\">");
					sb.Append(Utils.ToHtml(ci.Comments.ToString()));
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
					sb.Append("<!-- ConstructorParameters -->\r\n");
					sb.Append("<div class=\"level\">");
					sb.Append("<div class=\"title\">Constructor Parameters</div>\r\n");
					sb.Append("<p>Construction parameters for this class.</p>\r\n");
					if(ci.Parameters.Length > 0)
					{
						sb.Append("<blockquote>");
						sb.Append(ci.Parameters);
						sb.Append("</blockquote>\r\n");
					}
					else
					{
						sb.Append("<p class=\"gray\">(None)</p>\r\n");
					}
					sb.Append("</div>\r\n");
					sb.Append("<!-- /ConstructorParameters -->\r\n");
					//	Imports.
					sb.Append(ImportInfoCollection.Summarize(ci.Imports, ci,
						"Imports referenced directly from this class."));
					//	Classes.
					sb.Append(ClassInfoCollection.Summarize(ci.Classes, ci,
						"Sub-classes of this item."));
					//	Methods.
					sb.Append(MethodInfoCollection.Summarize(ci.Methods, ci,
						"Methods implemented in this class."));
					//	Variables.
					sb.Append(VariableInfoCollection.Summarize(ci.Variables, ci,
						"Variables maintained by this class."));
					//	Place the constant footer.
					sb.Append(Utils.StatFileFooter);
					//	Write the file content.
					sw = File.CreateText(targetFolder.FullName + @"\" +
						ci.GetDocName() + ".html");
					sw.WriteLine(sb.ToString());
					sw.Flush();
					sw.Close();
					sw.Dispose();
					//	Document all of the members of the file independently.
					if(ci.Classes != null && ci.Classes.Count > 0)
					{
						ClassInfoCollection.Document(ci.Classes, targetFolder);
					}
					if(ci.Methods != null && ci.Methods.Count > 0)
					{
						MethodInfoCollection.Document(ci.Methods, targetFolder);
					}
					if(ci.Variables != null && ci.Variables.Count > 0)
					{
						VariableInfoCollection.Document(ci.Variables, targetFolder);
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
		/// Return an enumeration of the child classes of the specified instance.
		/// </summary>
		/// <param name="item">
		/// Reference to the class for which sub-classes will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of member classes.
		/// </returns>
		public static string EnumerateClasses(ClassInfoItem item)
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
		/// Return an enumeration of the methods in the specified instance.
		/// </summary>
		/// <param name="item">
		/// Reference to the class for which methods will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of member methods.
		/// </returns>
		public static string EnumerateMethods(ClassInfoItem item)
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
		/// Reference to the class for which variables will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of member variables.
		/// </returns>
		public static string EnumerateVariables(ClassInfoItem item)
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
		/// Parse a class and its members.
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
		public static int Parse(ClassInfoCollection values,
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
				//	The caller already expects that this is a Class, but doesn't
				//	know whether the definition is a single line, or multiple.
				lc = lines.Count;
				lp = index;
				ll = lp;
				lp = ClassInfoItem.Parse(values, lines, lp, indent, comments);
				while(ll < lp && lp < lc)
				{
					ll = lp;
					lp = ClassInfoItem.Parse(values, lines, lp, indent, comments);
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
		/// <param name="classDescription">
		/// Description to place within the section, as it applies to the caller's
		/// context.
		/// </param>
		/// <returns>
		/// String containing formatted text that can be included in a host file.
		/// </returns>
		public static string Summarize(ClassInfoCollection collection,
			IDrillableStat parentStat, string classDescription)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("<!-- Classes -->\r\n");
			sb.Append("<div class=\"level\"><div class=\"title\">");
			sb.Append("Classes");
			sb.Append("</div>\r\n");
			sb.Append("<p>");
			sb.Append(classDescription);
			sb.Append("</p>\r\n");
			if(collection != null && collection.Count > 0)
			{
				sb.Append("<table class=\"grid\">\r\n");
				sb.Append("<tr><th>Name</th><th colspan=\"3\" class=\"info\">");
				sb.Append("Information</th></tr>\r\n");
				foreach(ClassInfoItem ci in collection)
				{
					sb.Append("<tr>\r\n");
					//	Class Name.
					sb.Append("<td rowspan=\"3\"><a href=\"");
					sb.Append(ci.GetDocName());
					sb.Append(".html");
					sb.Append("\">");
					sb.Append(ci.Name);
					sb.Append("</a></td>\r\n");
					//	Comments.
					sb.Append("<td colspan=\"3\" class=\"info\">");
					sb.Append(Utils.ToHtml(ci.Comments.ToString()));
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
					sb.Append(ClassInfoCollection.EnumerateClasses(ci));
					sb.Append("</td>\r\n");
					sb.Append("<td>\r\n");
					sb.Append(ClassInfoCollection.EnumerateMethods(ci));
					sb.Append("</td>\r\n");
					sb.Append("<td>\r\n");
					sb.Append(ClassInfoCollection.EnumerateVariables(ci));
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
			sb.Append("<!-- /Classes -->\r\n");
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ClassInfoItem																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about a class and its makeup.
	/// </summary>
	public class ClassInfoItem : ContainerBase
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
		/// Create a new Instance of the ClassInfoItem Item.
		/// </summary>
		public ClassInfoItem() : base()
		{
			DocPrefix = "_C_";
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Create a new Instance of the ClassInfoItem Item.
		/// </summary>
		public ClassInfoItem(ClassInfoCollection parent) : this()
		{
			Parent = parent;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse the contents of a class and its members.
		/// </summary>
		/// <param name="values">
		/// Sibling class definitions at this level.
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
		public static int Parse(ClassInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			ClassInfoItem ci = null;	//	New Class.
																//	Local Comments.
			CommentInfoCollection cm = new CommentInfoCollection();
			ContainerInfo ct = null;	//	Recursive Container.
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
					ps = @"(?i:^(?<space>[ |\t]*)class\s+" +
						@"(?<name>[^\:\(]+)(\((?<params>[^\)]*)\)){0,1}\s*\:\s*$)";
					mi = Regex.Match(ls, ps);
					if(mi != null && mi.Success)
					{
						//	The Class has been found. Create an item.
						//	Get Line, Indent, Name, and Params.
						ci = new ClassInfoItem();
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
					else
					{
						rv = index;
					}
				}
				//	Proceed if we are ready.
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Pattern																																*
		//*-----------------------------------------------------------------------*
		private static string mPattern =
			@"(?i:^(?<space>[ |\t]*)class\s+(?<name>[^\:\(]+))";
		/// <summary>
		/// Get the pattern for partially matching a Class.
		/// </summary>
		public static string Pattern
		{
			get { return mPattern; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
