//	VariableInfo.cs
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
	//*	VariableInfoCollection																									*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of VariableInfoItem Items.
	/// </summary>
	public class VariableInfoCollection : List<VariableInfoItem>, IStatCollection
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
		/// Create a new Instance of the VariableInfoCollection Item.
		/// </summary>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		public VariableInfoCollection(IDrillableStat parentStat)
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
		public new void Add(VariableInfoItem item)
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
		/// Individually document each variable in the specified collection.
		/// </summary>
		/// <param name="variables">
		/// Collection of variables to be documented.
		/// </param>
		/// <param name="targetFolder">
		/// Reference to the output file folder.
		/// </param>
		/// <returns>
		/// Value indicating success, if true.
		/// </returns>
		public static bool Document(VariableInfoCollection variables,
			DirectoryInfo targetFolder)
		{
			FileInfoItem fi = null;					//	Working File.
			NameValuesCollection nc = null;	//	Unique Names with multiple values.
			bool rv = true;
			string[] sa = new string[0];		//	General string array.
			StringBuilder sb = new StringBuilder();
			StreamWriter sw = null;					//	Text Writer.
			VariableInfoItem vi = null;			//	Tangible variable for drilling.
			string ws = "";									//	Working string.

			if(variables != null && targetFolder != null && targetFolder.Exists)
			{
				//	Caller values were valid.
				nc = new NameValuesCollection();
				foreach(VariableInfoItem vm in variables)
				{
					nc.Add(vm.Name, vm.Assignment, true);
				}
				foreach(NameValuesItem ni in nc)
				{
					//	Clear the previous file contents.
					if(sb.Length > 0)
					{
						sb.Remove(0, sb.Length);
					}
					vi = VariableInfoCollection.GetName(variables, ni.Name);
					if(vi != null && vi.Parent != null && vi.Parent.ParentStat != null)
					{
						fi = vi.Parent.ParentStat.GetFile();
					}
					ws = Utils.StatFileHeader;
					ws = ws.Replace("{Title}", "Variable - " +
						(ni.Name.Length > 0 ? ni.Name : "unnamed"));
					sb.Append(ws);
					sb.Append("<h1>Variable</h1>\r\n");
					sb.Append("<!-- Information -->\r\n");
					sb.Append("<h2 class=\"id\">");
					sb.Append((ni.Name.Length > 0 ? ni.Name : "unnamed"));
					sb.Append("</h2>\r\n");
					sb.Append("<table class=\"grid\">\r\n");
					sb.Append("<tr><th>Name</th>");
					sb.Append("<th class=\"buffer\">Value</th></tr>\r\n");
					sb.Append("<tr><td>Information:</td>");
					sb.Append("<td class=\"info\">");
					ws = Utils.ToHtml(
						VariableInfoCollection.GetCommentsForName(variables, ni.Name));
					if(ws.Length == 0)
					{
						ws = "&nbsp;";
					}
					sb.Append("</td></tr>\r\n");
					sb.Append("</table>\r\n");
					sb.Append("<!-- /Information -->\r\n");
					sb.Append("<!-- Assignments -->\r\n");
					sb.Append("<div class=\"level\">");
					sb.Append("<div class=\"title\">Assignments</div>\r\n");
					if(ni.Values.Count > 0)
					{
						sb.Append("<table class=\"grid bordul\">\r\n");
						foreach(string si in ni.Values)
						{
							sb.Append("<tr class=\"line\"><td>");
							sb.Append(Utils.ToHtml(si));
							sb.Append("</td></tr>\r\n");
						}
						sb.Append("</table>\r\n");
					}
					else
					{
						sb.Append("<p class=\"gray\">(None)</p>");
					}
					sb.Append("</div>\r\n");
					sb.Append("<!-- /Assignments -->\r\n");
					//	Place the constant footer.
					sb.Append(Utils.StatFileFooter);
					//	Write the file content.
					sw = File.CreateText(targetFolder.FullName + @"\" +
						vi.GetDocName() + ".html");
					sw.WriteLine(sb.ToString());
					sw.Flush();
					sw.Close();
					sw.Dispose();
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	EnumerateCalls																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a list of enumerated calls made from this variable.
		/// </summary>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		/// <param name="name">
		/// Name of the variable to enumerate for.
		/// </param>
		/// <returns>
		/// Formatted list of function calls placed by this variable.
		/// </returns>
		public static string EnumerateCalls(IDrillableStat parentStat,
			NameValuesItem name)
		{
			List<string> cc = null;			//	Class heirarchy collection.
			string cs = "";							//	Class string.
			List<int> fc = null;				//	Function starts collection.
			int lc = 0;									//	List Count.
			int lp = 0;									//	List Position.
			object ob = null;						//	Generic object returned from search.
			StringBuilder sb = new StringBuilder();
			IDrillableStat sc = parentStat;	//	Current Scope.
			VariableInfoItem vi = null;	//	Working Variable.

			if(parentStat != null && name != null && name.Values.Count > 0)
			{
				//	Here, we want to find a list of all of the methods being called,
				//	and their locations.
				foreach(string si in name.Values)
				{
					//	Each expression.
					fc = ExpressionStack.FunctionStarts(si);
					if(fc.Count > 0)
					{
						//	Function calls are present. Inspect each one.
						foreach(int fi in fc)
						{
							//	Get the class-access hierarchy.
							cc = ExpressionStack.FunctionClass(si, fi);
							//	If class-access is present, it may refer to static or
							//	instance-based access, or one of the two following forms.
							//	* - MyClass.methodA(a, b, c)
							//	* - x.methodA(a, b, c)
							//	If the item is a static reference, the class can be found
							//	by moving up the chain and searching from a higher level.
							//	If the item is an instance reference, the variable to which
							//	the original class name was assigned first needs to be
							//	resolved, then the class can be found from that reference.
							//	If the instance of the class is a reference acquired through
							//	a function call to an undocumented method, then no resolution
							//	is currently available.
							//	In this version, the documentation will lead to and stop at
							//	any method being called to assign the instance variable.
							lc = cc.Count;
							for(lp = 0; lp < lc; lp ++)
							{
								cs = cc[lp];
								if(lp == 0)
								{
									//	Drilling outward is possible in GetObjectName.
									ob = sc.GetObjectName(cs);
								}
								else
								{
									//	After initially receiving the name of the outer object,
									//	drilling back down is the necessary direction.
									ob = sc.GetMemberName(cs);
								}
								if(ob != null)
								{
									if(ob is VariableInfoItem)
									{
										vi = (VariableInfoItem)ob;
										if(vi.Parent != null)
										{
											sc = vi.Parent.ParentStat;
										}
										else
										{
											sc = null;
										}
									}
									else if(ob is IDrillableStat)
									{
										sc = (IDrillableStat)ob;
									}
									else
									{
										sc = null;
									}
								}
								if(sc == null)
								{
									break;
								}
							}
							if(sc != null)
							{
								//	The scope has been set for the method.
								ob = sc.GetMemberName(ExpressionStack.FunctionName(si, fi));
								if(ob != null && ob is IDrillableStat)
								{
									//	Member found. This could be a class instantiation, or
									//	a method call.
									sc = (IDrillableStat)ob;
									sb.Append("<li>");
									sb.Append("<a href=\"");
									sb.Append(sc.GetDocName());
									sb.Append(".html");
									sb.Append("\">");
									sb.Append(sc.Name);
									sb.Append("</a></li>\r\n");
								}
							}
						}
					}
				}
			}
			if(sb.Length > 0)
			{
				sb.Insert(0, "<ul class=\"member\">\r\n");
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
		//*	EnumerateObjects																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a list of enumerated classes referenced from this variable.
		/// </summary>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		/// <param name="name">
		/// Name of the variable for which object references will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of object references made by the specified variable.
		/// </returns>
		public static string EnumerateObjects(IDrillableStat parentStat,
			NameValuesItem name)
		{
			List<string> ex = null;			//	Entity collection.
			StringBuilder sb = new StringBuilder();
			object ob = null;						//	Generic object from search.
			IDrillableStat sc = null;		//	Working scope.

			if(parentStat != null && name != null && name.Values.Count > 0)
			{
				foreach(string si in name.Values)
				{
					//	Each expression.
					ex = ExpressionStack.ExpressionEntities(si);
					foreach(string ei in ex)
					{
						if(parentStat.IsClass(ei))
						{
							//	This is a class name.
							ob = parentStat.GetObjectName(ei);
							if(ob != null && ob is IDrillableStat)
							{
								sc = (IDrillableStat)ob;
								if(sc.DocPrefix == "_C_")
								{
									//	Class found.
									sb.Append("<li><a href=\"");
									sb.Append(sc.GetDocName());
									sb.Append(".html");
									sb.Append("\">");
									sb.Append(sc.Name);
									sb.Append("</a></li>\r\n");
								}
							}
						}
					}
				}
			}
			if(sb.Length > 0)
			{
				sb.Insert(0, "<ul class=\"member\">\r\n");
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
		//*	EnumerateReferences																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a list of enumerated variables referenced from this variable.
		/// </summary>
		/// <param name="parentStat">
		/// Reference to the parent statistics controller.
		/// </param>
		/// <param name="name">
		/// Name of the variable for which references will be enumerated.
		/// </param>
		/// <returns>
		/// Formatted list of references to other variables made by the variable
		/// of the specified name within this scope.
		/// </returns>
		public static string EnumerateReferences(IDrillableStat parentStat,
			NameValuesItem name)
		{
			List<string> ex = null;			//	Entity collection.
			StringBuilder sb = new StringBuilder();
			object ob = null;						//	Generic object from search.
			VariableInfoItem vi = null;	//	Working Variable.

			if(parentStat != null && name != null && name.Values.Count > 0)
			{
				foreach(string si in name.Values)
				{
					//	Each expression.
					ex = ExpressionStack.ExpressionEntities(si);
					foreach(string ei in ex)
					{
						if(parentStat.IsVariable(ei))
						{
							//	This is a variable name.
							ob = parentStat.GetObjectName(ei);
							if(ob != null && ob is VariableInfoItem)
							{
								vi = (VariableInfoItem)ob;
								//	Variable found.
								sb.Append("<li><a href=\"");
								sb.Append(vi.GetDocName());
								sb.Append(".html");
								sb.Append("\">");
								sb.Append(vi.Name);
								sb.Append("</a></li>\r\n");
							}
						}
					}
				}
			}
			if(sb.Length > 0)
			{
				sb.Insert(0, "<ul class=\"member\">\r\n");
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
		//*	GetCommentsForName																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return all of the comments found in the collection for the specified
		/// variable name.
		/// </summary>
		/// <param name="collection">
		/// Reference to the variables collection to be searched for matching
		/// names.
		/// </param>
		/// <param name="name">
		/// Name of the variable for which comments will be returned.
		/// </param>
		/// <returns>
		/// Concatenated string of all comments assigned to the variable of the
		/// specified name.
		/// </returns>
		public static string GetCommentsForName(VariableInfoCollection collection,
			string name)
		{
			StringBuilder sb = new StringBuilder();
			string ws = "";			//	Working string.

			if(collection != null && collection.Count > 0 && name != null)
			{
				foreach(VariableInfoItem vi in collection)
				{
					//	Match every instance of the name.
					if(vi.Name == name)
					{
						//	Matching name found.
						ws = vi.Comments.ToString();
						if(ws.Length > 0)
						{
							if(sb.Length > 0)
							{
								sb.Append("\r\n");
							}
							sb.Append(ws);
						}
					}
				}
			}
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetName																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the first item from the collection having the specified name.
		/// </summary>
		/// <param name="collection">
		/// Reference to the variables collection to search.
		/// </param>
		/// <param name="name">
		/// Name of the variable to find.
		/// </param>
		/// <returns>
		/// Reference to the first item found matching the specified name, if
		/// found. Otherwise, null.
		/// </returns>
		public static VariableInfoItem GetName(VariableInfoCollection collection,
			string name)
		{
			VariableInfoItem ro = null;		//	Return object.
			string tl = "";								//	Lowercase match.

			if(collection != null && name != null)
			{
				tl = name;
				foreach(VariableInfoItem vi in collection)
				{
					if(vi.Name == tl)
					{
						ro = vi;
						break;
					}
				}
			}
			return ro;
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
		/// Parse a variable and its constituents.
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
		/// This version only performs one find prior to returning to caller to
		/// prevent false positives on various branching logic expressions.
		/// </remarks>
		public static int Parse(VariableInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			int rv = index;			//	Return Value.

			if(values != null && lines != null && lines.Count > index)
			{
				rv = VariableInfoItem.Parse(values, lines, index, indent, comments);
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
		/// <param name="variableDescription">
		/// Description to place within the section, as it applies to the caller's
		/// context.
		/// </param>
		/// <returns>
		/// String containing formatted text that can be included in a host file.
		/// </returns>
		public static string Summarize(VariableInfoCollection collection,
			IDrillableStat parentStat, string variableDescription)
		{
			bool bf = false;							//	Flag - Found.
			NameValuesCollection nc = new NameValuesCollection();
			VariableInfoItem ri = null;		//	Reference Variable item.
			StringBuilder sb = new StringBuilder();

			sb.Append("<!-- Variables -->\r\n");
			sb.Append("<div class=\"level\"><div class=\"title\">");
			sb.Append("Variables");
			sb.Append("</div>\r\n");
			sb.Append("<p>");
			sb.Append(variableDescription);
			sb.Append("</p>\r\n");
			if(collection != null && collection.Count > 0)
			{
				bf = true;
				if(bf)
				{
					//	Get unique instances for every variable name in this
					//	context.
					foreach(VariableInfoItem vi in collection)
					{
						nc.Add(vi.Name, vi.Assignment, true);
					}
					sb.Append("<table class=\"grid\">\r\n");
					sb.Append("<tr><th>Name</th><th colspan=\"3\" class=\"info\">");
					sb.Append("Information</th></tr>\r\n");
					foreach(NameValuesItem ni in nc)
					{
						ri = GetName(collection, ni.Name);
						sb.Append("<tr>\r\n");
						//	Variable Name.
						sb.Append("<td rowspan=\"3\"><a href=\"");
						sb.Append(ri.GetDocName());
						sb.Append(".html");
						sb.Append("\">");
						sb.Append((ni.Name.Length > 0 ? ni.Name : "*"));
						sb.Append("</a></td>\r\n");
						//	Comments.
						sb.Append("<td colspan=\"3\" class=\"info\">");
						sb.Append(Utils.ToHtml(ri.Comments.ToString()));
						sb.Append("</td>\r\n");
						sb.Append("</tr>\r\n");
						//	Descriptor Headings.
						sb.Append("<tr>\r\n");
						sb.Append("<th class=\"class\">Objects</th>\r\n");
						sb.Append("<th class=\"method\">Calls</th>\r\n");
						sb.Append("<th class=\"variable\">References</th>\r\n");
						sb.Append("</tr>\r\n");
						//	Descriptors.
						sb.Append("<tr>\r\n");
						sb.Append("<td>\r\n");
						sb.Append(VariableInfoCollection.EnumerateObjects(parentStat, ni));
						sb.Append("</td>\r\n");
						sb.Append("<td>\r\n");
						sb.Append(VariableInfoCollection.EnumerateCalls(parentStat, ni));
						sb.Append("</td>\r\n");
						sb.Append("<td>\r\n");
						sb.Append(VariableInfoCollection.EnumerateReferences(
							parentStat, ni));
						sb.Append("</td>\r\n");
						sb.Append("</tr>\r\n");
					}
					sb.Append("</table>\r\n");
				}
			}
			if(!bf)
			{
				sb.Append("<p class=\"gray\">(None)</p>\r\n");
			}
			sb.Append("</div>\r\n");
			sb.Append("<!-- /Variables -->\r\n");
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	VariableInfoItem																												*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about a Variable.
	/// </summary>
	public class VariableInfoItem
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
		/// Create a new Instance of the VariableStatItem Item.
		/// </summary>
		public VariableInfoItem()
		{
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Create a new Instance of the VariableStatItem Item.
		/// </summary>
		/// <param name="parent">
		/// Reference to the parent collection to which this item belongs.
		/// </param>
		public VariableInfoItem(VariableInfoCollection parent) : this()
		{
			mParent = parent;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Assignment																														*
		//*-----------------------------------------------------------------------*
		private string mAssignment = "";
		/// <summary>
		/// Get/Set the assignment value of this item.
		/// </summary>
		public string Assignment
		{
			get { return mAssignment; }
			set { mAssignment = value; }
		}
		//*-----------------------------------------------------------------------*

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
		//*	DocPrefix																															*
		//*-----------------------------------------------------------------------*
		private string mDocPrefix = "_V_";
		/// <summary>
		/// Get/Set the Document Prefix used by items of this type.
		/// </summary>
		public string DocPrefix
		{
			get { return mDocPrefix; }
			set { mDocPrefix = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetDocName																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the full document name of this item, including the prefixes
		/// set by parent objects.
		/// </summary>
		/// <returns>
		/// Full document name of this item, including prefixes set by parent
		/// objects.
		/// </returns>
		/// <remarks>
		/// This method does not include the file extension, since documentation
		/// can be written in different file formats. The final extension will be
		/// appended by the formatter.
		/// </remarks>
		public string GetDocName()
		{
			//	All elements with potential parents can use this implementation.
			StringBuilder sb = new StringBuilder();

			if(mParent != null && mParent.ParentStat != null)
			{
				//	Get the parent prefix name, but only if 
				sb.Append(mParent.ParentStat.GetDocName());
			}
			sb.Append(mDocPrefix);
			if(mName.Length > 0)
			{
				sb.Append(Utils.ReplaceNonAN(mName, "_"));
			}
			else
			{
				sb.Append("unnamed");
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
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		private string mName = "";
		/// <summary>
		/// Get/Set the Name of the Variable.
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Operator																															*
		//*-----------------------------------------------------------------------*
		private string mOperator = "";
		/// <summary>
		/// Get/Set the operator used for this variable assignment.
		/// </summary>
		public string Operator
		{
			get { return mOperator; }
			set { mOperator = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		private VariableInfoCollection mParent = null;
		/// <summary>
		/// Get/Set a reference to the Parent collection.
		/// </summary>
		public VariableInfoCollection Parent
		{
			get { return mParent; }
			set { mParent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse the specified line, creating a Variable definition where
		/// appropriate.
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
		/// Next line index to process. This method will return the value
		/// of the index parameter if no matches were found.
		/// </returns>
		public static int Parse(VariableInfoCollection values,
			TextLineCollection lines, int index, int indent,
			CommentInfoCollection comments)
		{
			bool bf = false;		//	Flag - Found.
			VariableInfoItem ci = null;				//	New Item.
			string cs = "";			//	Comment.
			int lp = index;			//	List Position.
			string ls = "";			//	Line String.
			string lt = "";			//	Trimmed Line String.
			Match mi = null;		//	Working Match.
			string nm = "";			//	Variable Name.
			string op = "";			//	Operator.
			int rv = index;			//	Return Value.
			StringBuilder sb = null;	//	Working String.
			int sp = 0;					//	Space.

			if(values != null && lines != null && lines.Count > index)
			{
				lp = index;
				ls = lines[lp].Value;
				cs = Utils.GetCommentSingle(ls);
				if(cs.Length > 0)
				{
					ls = Utils.RemoveCommentSingle(ls);
					comments.Add(cs, lp);
				}
				lt = ls.Trim();

				mi = Regex.Match(ls, mPattern);
				if(mi != null && mi.Success)
				{
					sp = Utils.GetLength(mi, "space");
					nm = Utils.GetValue(mi, "name");
					op = Utils.GetValue(mi, "operator");
					if(Utils.GetLength(mi, "box") > 0 ||
						Utils.GetLength(mi, "continue") > 0)
					{
						//	The variable is potentially multi-line.
						lt = Utils.GetValue(mi, "content");
						if(Utils.IsTerminated(lt))
						{
							//	This item is closed. Single line.
							bf = true;
							lp ++;
						}
						else
						{
							//	Process multi-line.
							sb = new StringBuilder();
							lp = Utils.GetVariableAssignment(lines, lp, sb, comments);
							lt = sb.ToString();
							if(lp > -1)
							{
								bf = true;
							}
							else
							{
								lp = index;
							}
						}
					}
					else
					{
						lt = Utils.GetValue(mi, "content");
						bf = true;
						lp ++;
					}
					if(bf)
					{
						//	The variable is ready.
						ci = new VariableInfoItem();
						ci.mIndentLevel = sp;
						ci.mLineNumber = index;
						ci.mName = nm;
						ci.mOperator = op;
						ci.mAssignment = lt;
						if(comments.Count > 0)
						{
							ci.mComments.AddRange(comments);
							comments.Clear();
						}
						values.Add(ci);
					}
				}
				rv = lp;
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Pattern																																*
		//*-----------------------------------------------------------------------*
		private static string mPattern =
			@"(?i:^(?<space>[ |\t]*)" +
			@"(?<nameandassign>" +
			@"(?<name>(yield|return|assert))\s+|" +
			@"((?<name>[^ \(\=]+)\s*" +
			@"(?<operator>[\+\-\*\/\%\&\|]{0,1}\s*\=)\s*)){0,1}" +
			@"(?<content>" +
			"((?=[\\{\\(\\[\\\"\\\'0-9a-z\\.\\-_\\t ])" +
			@"(?<gen>[0-9a-z\.\-_\t ]*)" +
			"(?<box>[\\{\\(\\[\\\"\\\']*))(?<npt>.*?)" +
			@"(?<continue>([,\.\+\-\&\|\\/\*]|" +
			"\\\"\\\"\\\"" +
			@")*))\s*$)";
		/// <summary>
		/// Get the pattern for partially matching a Variable.
		/// </summary>
		public static string Pattern
		{
			get { return mPattern; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
