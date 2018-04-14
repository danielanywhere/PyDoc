//	NameValues.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	NameValuesCollection																										*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of NameValuesItem Items.
	/// </summary>
	public class NameValuesCollection : List<NameValuesItem>
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
		/// Add a single Name and Value to the list.
		/// </summary>
		/// <param name="name">
		/// Name to add, if unique.
		/// </param>
		/// <param name="value">
		/// Value to add.
		/// </param>
		/// <param name="unique">
		/// Value indicating whether the value parameter should be added to the
		/// collection unconditionally, or only in the case that it will be unique
		/// in the Values collection.
		/// </param>
		/// <returns>
		/// Reference to a newly created NameValuesItem, if the name was unique in
		/// collection. Otherwise, a reference to the previously existing item.
		/// </returns>
		public NameValuesItem Add(string name, string value, bool unique)
		{
			bool bf = false;						//	Flag - Found.
			NameValuesItem ro = null;		//	Return object.
			string tl = "";							//	Lowercase match.

			if(name != null && value != null)
			{
				tl = name;
				foreach(NameValuesItem ni in this)
				{
					if(ni.Name == tl)
					{
						//	Name found.
						ro = ni;
						if(unique)
						{
							tl = value;
							bf = false;
							foreach(string ti in ni.Values)
							{
								if(ti == tl)
								{
									//	Duplicate value found.
									bf = true;
									break;
								}
							}
							if(!bf)
							{
								//	This item is unique.
								ni.Values.Add(value);
							}
						}
						else
						{
							ni.Values.Add(value);
						}
					}
				}
				if(ro == null)
				{
					//	Name not found.
					ro = new NameValuesItem();
					ro.Name = name;
					ro.Values.Add(value);
					this.Add(ro);
				}
			}
			return ro;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Add an item by member values, inserting multiple values, as
		/// appropriate.
		/// </summary>
		/// <param name="name">
		/// Name to be added to the collection, if new, or to be appended to if the
		/// name already exists.
		/// </param>
		/// <param name="values">
		/// Collection of values to add to the item with the specified name.
		/// </param>
		/// <param name="unique">
		/// Value indicating whether each of the values must be unique in the
		/// collection to be appended to the Values collection, or whether they
		/// will be added unconditionally.
		/// </param>
		/// <returns>
		/// Reference to a newly created NameValuesItem, if the name was unique in
		/// collection. Otherwise, a reference to the previously existing item.
		/// </returns>
		public NameValuesItem Add(string name, List<string> values, bool unique)
		{
			bool bf = false;						//	Flag - Found.
			NameValuesItem ro = null;		//	Return object.
			string tl = "";							//	Lowercase match.

			if(name != null && values != null)
			{
				tl = name;
				foreach(NameValuesItem ni in this)
				{
					if(ni.Name == tl)
					{
						//	Name found.
						ro = ni;
						if(unique)
						{
							foreach(string si in values)
							{
								tl = si;
								bf = false;
								foreach(string ti in ni.Values)
								{
									if(ti == tl)
									{
										//	Duplicate value found.
										bf = true;
										break;
									}
								}
								if(!bf)
								{
									//	This item is unique.
									ni.Values.Add(si);
								}
							}
						}
						else if(values.Count > 0)
						{
							ni.Values.AddRange(values);
						}
					}
				}
				if(ro == null)
				{
					//	No matching entry found. Add full.
					ro = new NameValuesItem();
					ro.Name = name;
					if(values.Count > 0)
					{
						ro.Values.AddRange(values);
					}
					this.Add(ro);
				}
			}
			return ro;
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	NameValuesItem																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Helper item for storing multiple values under a single name.
	/// </summary>
	public class NameValuesItem
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
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		private string mName = "";
		/// <summary>
		/// Get/Set the Name of this item.
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Values																																*
		//*-----------------------------------------------------------------------*
		private List<string> mValues = new List<string>();
		/// <summary>
		/// Get a reference to the collection of values on this item.
		/// </summary>
		public List<string> Values
		{
			get { return mValues; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
