//	DrillableStat.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	IDrillableStat																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Type of Statistic that can be drilled down indefinitely.
	/// </summary>
	/// <remarks>
	/// In this application, the drillable entities are File, Methods, Classes,
	/// and Conditional Logic items. However, only Files, Classes, and Methods
	/// are extracted this version.
	/// </remarks>
	public interface IDrillableStat
	{
		//*-----------------------------------------------------------------------*
		//*	DocPrefix																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Get/Set the documentation prefix of this item.
		/// </summary>
		string DocPrefix { get; set; }
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
		string GetDocName();
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
		FileInfoItem GetFile();
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
		/// The search only includes direct members of the item starting the
		/// search.
		/// </remarks>
		object GetMemberName(string name);
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
		/// The search only includes branches and imports at the current level and
		/// above.
		/// </remarks>
		object GetObjectName(string name);
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
		bool IsClass(string name);
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
		bool IsVariable(string name);
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Get/Set the name of the item.
		/// </summary>
		string Name { get; set; }
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	IStatCollection																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of Statistics.
	/// </summary>
	public interface IStatCollection
	{
		/// <summary>
		/// Get/Set a reference to the Parent Statistic.
		/// </summary>
		IDrillableStat ParentStat { get; set; }
	}
	//*-------------------------------------------------------------------------*
}
