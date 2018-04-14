//	ContainerInfo.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	ContainerBase																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Base class used to create full container items.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class doesn't include the static Parse method with the signature:
	/// <blockquote>
	/// public static int Parse(XInfoCollection values,
	/// <blockquote>
	///	 TextLineCollection lines, int index, int indent,<br />
	///	 CommentInfoCollection comments)
	/// </blockquote>
	/// </blockquote>
	/// </para>
	/// <para>
	///	or the static Pattern property with the signature:
	///	<blockquote>
	///	public static string Pattern { get { return mPattern; } }
	///	</blockquote>
	/// </para>
	/// <para>
	/// Parse method and Pattern property must both be implemented by you.
	/// </para>
	/// </remarks>
	public abstract class ContainerBase : IDrillableStat
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
		/// Create a new Instance of the ContainerBase Item.
		/// </summary>
		public ContainerBase()
		{
			mClasses = new ClassInfoCollection(this);
			mMethods = new MethodInfoCollection(this);
			mVariables = new VariableInfoCollection(this);
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Classes																																*
		//*-----------------------------------------------------------------------*
		private ClassInfoCollection mClasses = null;
		/// <summary>
		/// Get a reference to the collection of Classes appearing in this level.
		/// </summary>
		/// <remarks>
		/// These are embedded classes.
		/// </remarks>
		public ClassInfoCollection Classes
		{
			get { return mClasses; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ClearAll																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Clear all collections.
		/// </summary>
		public void ClearAll()
		{
			mClasses.Clear();
			mComments.Clear();
			mContent = "";
			mImports.Clear();
			mIndentLevel = 0;
			mLineNumber = 0;
			mLogic.Clear();
			mMethods.Clear();
			mName = "";
			mParameters = "";
			mVariables.Clear();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Comments																															*
		//*-----------------------------------------------------------------------*
		private CommentInfoCollection mComments = new CommentInfoCollection();
		/// <summary>
		/// Get a reference to the Comments for this Method.
		/// </summary>
		public CommentInfoCollection Comments
		{
			get { return mComments; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Content																																*
		//*-----------------------------------------------------------------------*
		private string mContent = "";
		/// <summary>
		/// Get/Set the base Content of this Item.
		/// </summary>
		public string Content
		{
			get { return mContent; }
			set { mContent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DocPrefix																															*
		//*-----------------------------------------------------------------------*
		private string mDocPrefix = "";
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
		public virtual string GetDocName()
		{
			//	All elements with potential parents can use this implementation.
			StringBuilder sb = new StringBuilder();

			if(mParent != null && mParent.ParentStat != null)
			{
				sb.Append(mParent.ParentStat.GetDocName());
			}
			sb.Append(mDocPrefix);
			sb.Append(mName);
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetMemberName																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a reference to a member variable instance, class definition, or
		/// method definition having the specified name.
		/// </summary>
		/// <param name="name">
		/// Name of the class or variable to search for.
		/// </param>
		/// <returns>
		/// Reference to either a variable or IDrillableStat constituting the
		/// member, if found. Otherwise, null.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The search only includes direct members of the item starting the
		/// search.
		/// </para>
		/// <para>
		/// The main difference between GetObjectName and GetMemberName is
		/// that GetObjectName will drill outward if a match is not found at this
		/// level. GetMemberName searches only at the current level.
		/// </para>
		/// </remarks>
		public object GetMemberName(string name)
		{
			object ro = null;			//	Return object.

			if(name != null && name.Length > 0)
			{
				//	Search classes.
				if(mClasses != null)
				{
					foreach(ClassInfoItem ci in mClasses)
					{
						if(ci.Name == name)
						{
							ro = ci;
							break;
						}
					}
				}
				if(ro == null)
				{
					//	Search methods.
					if(mMethods != null)
					{
						foreach(MethodInfoItem mi in mMethods)
						{
							if(mi.Name == name)
							{
								ro = mi;
								break;
							}
						}
					}
				}
				if(ro == null)
				{
					//	Search variables.
					if(mVariables != null && mVariables.Count > 0)
					{
						foreach(VariableInfoItem vi in mVariables)
						{
							if(vi.Name == name)
							{
								ro = vi;
								break;
							}
						}
					}
				}
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetObjectName																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a reference to a variable instance or class definition to
		/// set the scope for the next search.
		/// </summary>
		/// <param name="name">
		/// Name of the class or variable to search for.
		/// </param>
		/// <returns>
		/// Reference to either a variable or IDrillableStat constituting a
		/// valid scope, if found. Otherwise, null.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The search only includes branches and imports at the current level and
		/// above.
		/// </para>
		/// <para>
		/// The main difference between GetObjectName and GetMemberName is
		/// that GetObjectName will drill outward if a match is not found at this
		/// level. GetMemberName searches only at the current level.
		/// </para>
		/// </remarks>
		public object GetObjectName(string name)
		{
			object ro = null;			//	Return object.

			if(name != null && name.Length > 0)
			{
				//	Search classes.
				if(mClasses != null)
				{
					foreach(ClassInfoItem ci in mClasses)
					{
						if(ci.Name == name)
						{
							ro = ci;
							break;
						}
					}
				}
				if(ro == null)
				{
					//	Search methods.
					if(mMethods != null)
					{
						foreach(MethodInfoItem mi in mMethods)
						{
							if(mi.Name == name)
							{
								ro = mi;
								break;
							}
						}
					}
				}
				if(ro == null)
				{
					//	Search variables.
					if(mVariables != null && mVariables.Count > 0)
					{
						foreach(VariableInfoItem vi in mVariables)
						{
							if(vi.Name == name)
							{
								ro = vi;
								break;
							}
						}
					}
				}
				if(ro == null && mParent != null && mParent.ParentStat != null)
				{
					ro = mParent.ParentStat.GetObjectName(name);
				}
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetContainerInfo																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a reference to a filled Container Info describing the contents
		/// of the caller's instance.
		/// </summary>
		/// <param name="container">
		/// Abstract container having the traits of ContainerBase.
		/// </param>
		/// <param name="lines">
		/// Collection of source text lines.
		/// </param>
		/// <param name="index">
		/// Current index within the source text collection.
		/// </param>
		/// <param name="indent">
		/// Current indent level at which parsing is active.
		/// </param>
		/// <returns>
		/// Reference to the container that generically describes the contents of
		/// the caller's instance.
		/// </returns>
		public static ContainerInfo GetContainerInfo(ContainerBase container,
			TextLineCollection lines, int index, int indent)
		{
			ContainerInfo ro = null;

			if(container != null)
			{
				ro = new ContainerInfo(
					container,
					container.mClasses, container.mImports, lines, container.mLogic,
					container.mMethods, container.mVariables, index, indent,
					container.mComments);
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetFile																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a reference to the file in which this item is found.
		/// </summary>
		/// <returns>
		/// Reference to the file containing this object, if found. Otherwise,
		/// null.
		/// </returns>
		public FileInfoItem GetFile()
		{
			FileInfoItem ro = null;			//	Return object.

			if(this.GetType() == typeof(FileInfoItem))
			{
				ro = (FileInfoItem)this;
			}
			else if(mParent != null && mParent.ParentStat != null)
			{
				ro = mParent.ParentStat.GetFile();
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Imports																																*
		//*-----------------------------------------------------------------------*
		private ImportInfoCollection mImports = new ImportInfoCollection();
		/// <summary>
		/// Get a reference to the collection of Import items.
		/// </summary>
		public ImportInfoCollection Imports
		{
			get { return mImports; }
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
		//*	IsClass																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the specified name is a class in the
		/// current scope.
		/// </summary>
		/// <param name="name">
		/// Name of the entity to search for.
		/// </param>
		/// <returns>
		/// True if the specified name represents a class. Otherwise, false.
		/// </returns>
		/// <remarks>
		/// This call will drill outward if a match is not found at the current
		/// level.
		/// </remarks>
		public bool IsClass(string name)
		{
			bool rv = false;

			if(mClasses != null)
			{
				foreach(ClassInfoItem ci in mClasses)
				{
					if(ci.Name == name)
					{
						rv = true;
						break;
					}
				}
				if(!rv && mParent != null && mParent.ParentStat != null)
				{
					rv = mParent.ParentStat.IsClass(name);
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	IsVariable																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the specified name is a variable in
		/// the current scope.
		/// </summary>
		/// <param name="name">
		/// Name of the entity to search for.
		/// </param>
		/// <returns>
		/// True if the specified name represents a variable. Otherwise, false.
		/// </returns>
		public bool IsVariable(string name)
		{
			bool rv = false;

			if(mVariables != null)
			{
				foreach(VariableInfoItem vi in mVariables)
				{
					if(vi.Name == name)
					{
						rv = true;
						break;
					}
				}
				if(!rv && mParent != null && mParent.ParentStat != null)
				{
					rv = mParent.ParentStat.IsVariable(name);
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
		//*	Logic																																	*
		//*-----------------------------------------------------------------------*
		private LogicInfoCollection mLogic = new LogicInfoCollection();
		/// <summary>
		/// Get a reference to the non-implemented branching and conditional logic
		/// members of this item.
		/// </summary>
		public LogicInfoCollection Logic
		{
			get { return mLogic; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Methods																																*
		//*-----------------------------------------------------------------------*
		private MethodInfoCollection mMethods = null;
		/// <summary>
		/// Get a reference to the collection of Methods appearing in this level.
		/// </summary>
		/// <remarks>
		/// These are embedded methods.
		/// </remarks>
		public MethodInfoCollection Methods
		{
			get { return mMethods; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		private string mName = "";
		/// <summary>
		/// Get/Set the Name of this Item.
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parameters																														*
		//*-----------------------------------------------------------------------*
		private string mParameters = "";
		/// <summary>
		/// Get/Set the Parameters of this Item.
		/// </summary>
		public string Parameters
		{
			get { return mParameters; }
			set { mParameters = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		private IStatCollection mParent = null;
		/// <summary>
		/// Get/Set a reference to the Parent collection.
		/// </summary>
		public IStatCollection Parent
		{
			get { return mParent; }
			set { mParent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Variables																															*
		//*-----------------------------------------------------------------------*
		private VariableInfoCollection mVariables = null;
		/// <summary>
		/// Get a reference to the collection of Variables appearing in this level.
		/// </summary>
		public VariableInfoCollection Variables
		{
			get { return mVariables; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ContainerInfo																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Container of references used to pass information about the current level.
	/// </summary>
	public class ContainerInfo
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
		/// Create a new Instance of the ContainerInfo Item.
		/// </summary>
		public ContainerInfo()
		{
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Create a new Instance of the ContainerInfo Item.
		/// </summary>
		/// <param name="parent">
		/// A reference to the calling object, to be referenced as the parent of
		/// the following collections.
		/// </param>
		/// <param name="classes">
		/// Reference to the active collection of classes, or null.
		/// </param>
		/// <param name="imports">
		/// Reference to the active collection of imports, or null.
		/// </param>
		/// <param name="lines">
		/// Reference to the source text line collection.
		/// </param>
		/// <param name="logic">
		/// Reference to the active collection of unimplemented conditional
		/// logic entries, or null.
		/// </param>
		/// <param name="methods">
		/// Reference to the active collection of methods, or null.
		/// </param>
		/// <param name="variables">
		/// Reference to the active collection of variables and expressions, or
		/// null.
		/// </param>
		/// <param name="index">
		/// Current index within the source text lines collection.
		/// </param>
		/// <param name="indent">
		/// Current indent level under observation.
		/// </param>
		/// <param name="comments">
		/// Collection of comments to be filled with any unassigned comments
		/// found at the current level.
		/// </param>
		public ContainerInfo(
			ContainerBase parent,
			ClassInfoCollection classes,
			ImportInfoCollection imports, TextLineCollection lines,
			LogicInfoCollection logic, MethodInfoCollection methods,
			VariableInfoCollection variables, int index, int indent,
			CommentInfoCollection comments)
		{
			mParent = parent;
			mClasses = classes;
			mImports = imports;
			mLines = lines;
			mLogic = logic;
			mMethods = methods;
			mVariables = variables;
			mIndex = index;
			mIndent = indent;
			mComments = comments;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Classes																																*
		//*-----------------------------------------------------------------------*
		private ClassInfoCollection mClasses = null;
		/// <summary>
		/// Get/Set a reference to the caller's collection of classes.
		/// </summary>
		public ClassInfoCollection Classes
		{
			get { return mClasses; }
			set { mClasses = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Comments																															*
		//*-----------------------------------------------------------------------*
		private CommentInfoCollection mComments = null;
		/// <summary>
		/// Get/Set a reference to the caller's comments.
		/// </summary>
		public CommentInfoCollection Comments
		{
			get { return mComments; }
			set { mComments = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Imports																																*
		//*-----------------------------------------------------------------------*
		private ImportInfoCollection mImports = null;
		/// <summary>
		/// Get/Set a reference to the caller's import collection.
		/// </summary>
		public ImportInfoCollection Imports
		{
			get { return mImports; }
			set { mImports = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Indent																																*
		//*-----------------------------------------------------------------------*
		private int mIndent = 0;
		/// <summary>
		/// Get/Set the current Indent Level at the caller's location.
		/// </summary>
		public int Indent
		{
			get { return mIndent; }
			set { mIndent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Index																																	*
		//*-----------------------------------------------------------------------*
		private int mIndex = 0;
		/// <summary>
		/// Get/Set the current Line Index.
		/// </summary>
		public int Index
		{
			get { return mIndex; }
			set { mIndex = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Lines																																	*
		//*-----------------------------------------------------------------------*
		private TextLineCollection mLines = null;
		/// <summary>
		/// Get/Set a reference to the collection of source lines used to build
		/// items in this container.
		/// </summary>
		public TextLineCollection Lines
		{
			get { return mLines; }
			set { mLines = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Logic																																	*
		//*-----------------------------------------------------------------------*
		private LogicInfoCollection mLogic = null;
		/// <summary>
		/// Get/Set a reference to the caller's collection of conditional and
		/// branching logic.
		/// </summary>
		public LogicInfoCollection Logic
		{
			get { return mLogic; }
			set { mLogic = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Methods																																*
		//*-----------------------------------------------------------------------*
		private MethodInfoCollection mMethods = null;
		/// <summary>
		/// Get/Set a reference to the caller's collection of methods.
		/// </summary>
		public MethodInfoCollection Methods
		{
			get { return mMethods; }
			set { mMethods = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		private ContainerBase mParent = null;
		/// <summary>
		/// Get/Set a reference to the Parent object to which the members belong.
		/// </summary>
		public ContainerBase Parent
		{
			get { return mParent; }
			set { mParent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Variables																															*
		//*-----------------------------------------------------------------------*
		private VariableInfoCollection mVariables = null;
		/// <summary>
		/// Get/Set a reference to the caller's collection of variables.
		/// </summary>
		public VariableInfoCollection Variables
		{
			get { return mVariables; }
			set { mVariables = value; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
