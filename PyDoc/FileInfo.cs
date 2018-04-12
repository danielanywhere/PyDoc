//	FileInfo.cs
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
	//*	FileInfoCollection																											*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of FileInfoItem Items.
	/// </summary>
	public class FileInfoCollection : List<FileInfoItem>
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
		/// <param name="file">
		/// Reference to a System.FileInfo item to be tracked.
		/// </param>
		/// <returns>
		/// Reference to a newly created and added FileInfoItem representing the
		/// caller's file.
		/// </returns>
		public FileInfoItem Add(FileInfo file)
		{
			FileInfoItem ro = new FileInfoItem();

			ro.SystemFile = file;
			this.Add(ro);
			return ro;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Document																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Document the statistics for the loaded files.
		/// </summary>
		/// <param name="files">
		/// Collection of files to document.
		/// </param>
		/// <param name="targetFolder">
		/// Target file folder to which rendered files will be written.
		/// </param>
		/// <returns>
		/// Value indicating success, if true.
		/// </returns>
		public static bool Document(FileInfoCollection files,
			DirectoryInfo targetFolder)
		{
			bool rv = true;
			StringBuilder sb = new StringBuilder();	//	Local file string.
			StringBuilder si = new StringBuilder();	//	Index file string.
			StreamWriter sw = null;		//	Text Writer.
			string ws = "";						//	Working string.

			if(files != null && targetFolder != null && targetFolder.Exists)
			{
				//	Caller values were valid.
				ws = Utils.StatDefaultCSS;
				if(ws.Length > 0)
				{
					//	Default.css.
					sw = File.CreateText(targetFolder.FullName + @"\Default.css");
					sw.WriteLine(ws);
					sw.Flush();
					sw.Close();
					sw.Dispose();
					foreach(FileInfoItem fi in files)
					{
						//	Clear the previous file contents.
						if(Program.Verbosity >= 2)
						{
							Console.WriteLine("Documenting " + fi.Name);
						}
						if(sb.Length > 0)
						{
							sb.Remove(0, sb.Length);
						}
						ws = Utils.StatFileHeader;
						ws = ws.Replace("{Title}", "File - " + fi.Name);
						sb.Append(ws);
						sb.Append("<h1>File</h1>\r\n");
						sb.Append("<!-- Information -->\r\n");
						sb.Append("<h2 class=\"id\">");
						sb.Append(fi.Filename);
						sb.Append("</h2>\r\n");
						sb.Append("<table class=\"grid\">\r\n");
						sb.Append("<tr><th>Name</th>");
						sb.Append("<th class=\"buffer\">Value</th></tr>\r\n");
						sb.Append("<tr>\r\n");
						sb.Append("<td>Information:</td>\r\n");
						sb.Append("<td class=\"info\">");
						ws = Utils.ToHtml(fi.Comments.ToString());
						if(ws.Length == 0)
						{
							//	If there were no comments, then pad the cell.
							ws = "&nbsp;";
						}
						sb.Append(ws);
						sb.Append("</td>\r\n");
						sb.Append("</tr>\r\n");
						sb.Append("<tr><td>Directory:</td><td>");
						ws = fi.DirectoryName;
						if(ws.Length == 0)
						{
							ws = "./";
						}
						sb.Append(ws);
						sb.Append("</td></tr>\r\n");
						sb.Append("</table>\r\n");
						sb.Append("<!-- /Information -->\r\n");
						//	Imports.
						sb.Append(ImportInfoCollection.Summarize(fi.Imports, fi,
							"Packages imported by this file."));
						//	Classes.
						sb.Append(ClassInfoCollection.Summarize(fi.Classes, fi,
							"Classes defined in this file."));
						//	Methods.
						sb.Append(MethodInfoCollection.Summarize(fi.Methods, fi,
							"Methods defined in this file."));
						//	Variables.
						sb.Append(VariableInfoCollection.Summarize(fi.Variables, fi,
							"Variables defined in this file."));
						//	Place the constant footer.
						sb.Append(Utils.StatFileFooter);
						//	Write the file content.
						sw = File.CreateText(targetFolder.FullName + @"\" +
							fi.GetDocName() + ".html");
						sw.WriteLine(sb.ToString());
						sw.Flush();
						sw.Close();
						sw.Dispose();
						si.Append("<tr><td><a href=\"");
						si.Append(fi.GetDocName());
						si.Append(".html");
						si.Append("\">");
						si.Append(fi.Name);
						si.Append("</a></td>");
						si.Append("<td>");
						ws = fi.DirectoryName;
						if(ws.Length == 0)
						{
							ws = "./";
						}
						si.Append(ws);
						si.Append("</td>");
						si.Append("<td class=\"info\">");
						ws = Utils.ToHtml(fi.Comments.ToString());
						if(ws.Length == 0)
						{
							ws = "&nbsp;";
						}
						si.Append(ws);
						si.Append("</td></tr>\r\n");
						//	Document all of the members of the file independently.
						if(fi.Classes != null && fi.Classes.Count > 0)
						{
							ClassInfoCollection.Document(fi.Classes, targetFolder);
						}
						if(fi.Methods != null && fi.Methods.Count > 0)
						{
							MethodInfoCollection.Document(fi.Methods, targetFolder);
						}
						if(fi.Variables != null && fi.Variables.Count > 0)
						{
							VariableInfoCollection.Document(fi.Variables, targetFolder);
						}
					}
					//	Index.html
					if(si.Length > 0)
					{
						if(Program.Verbosity >= 2)
						{
							Console.WriteLine("Writing master index...");
						}
						if(sb.Length > 0)
						{
							sb.Remove(0, sb.Length);
						}
						ws = Utils.StatFileHeader;
						ws = ws.Replace("{Title}", "Master Index");
						sb.Append(ws);
						sb.Append("<h1>Master Index</h1>\r\n");
						sb.Append("<!-- Files -->\r\n");
						sb.Append("<table class=\"grid\">\r\n");
						sb.Append("<tr><th>Name</th>");
						sb.Append("<th>Directory</th>");
						sb.Append("<th class=\"buffer\">Remark</th></tr>\r\n");
						sb.Append(si.ToString());
						sb.Append("<!-- /Files -->\r\n");
						sb.Append(Utils.StatFileFooter);
						//	Write the file content.
						sw = File.CreateText(targetFolder.FullName + @"\" +
							"Index.html");
						sw.WriteLine(sb.ToString());
						sw.Flush();
						sw.Close();
						sw.Dispose();
					}
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse all files in the collection.
		/// </summary>
		/// <param name="projectBase">
		/// Physical base folder of the project input.
		/// </param>
		/// <returns>
		/// True if the operation was successful. Otherwise, false.
		/// </returns>
		public bool Parse(DirectoryInfo projectBase)
		{
			bool rv = false;
			foreach(FileInfoItem fi in this)
			{
				rv = FileInfoItem.Parse(fi, projectBase);
				if(!rv)
				{
					break;
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	FileInfoItem																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about a File.
	/// </summary>
	/// <remarks>
	/// This is not to be confused with the system.io.fileinfo class, which is
	/// represented here by the property SystemFile.
	/// </remarks>
	public class FileInfoItem : ContainerBase
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
		/// Create a new Instance of the FileInfoItem Item.
		/// </summary>
		public FileInfoItem() : base()
		{
			DocPrefix = "_F_";
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DirectoryName																													*
		//*-----------------------------------------------------------------------*
		private string mDirectoryName = "";
		/// <summary>
		/// Get/Set the Directory Name of this item.
		/// </summary>
		public string DirectoryName
		{
			get { return mDirectoryName; }
			set { mDirectoryName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Filename																															*
		//*-----------------------------------------------------------------------*
		private string mFilename = "";
		/// <summary>
		/// Get/Set the physical Filename of this item.
		/// </summary>
		public string Filename
		{
			get { return mFilename; }
			set { mFilename = value; }
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
		public override string GetDocName()
		{
			//	This item has no parent, so...
			StringBuilder sb = new StringBuilder();

			sb.Append("D_");
			sb.Append(Utils.ReplaceNonAN(mDirectoryName, "_"));
			sb.Append(DocPrefix);
			sb.Append(Name);
			return sb.ToString();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parse																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse the content of the file specified in SystemFile, and return the
		/// result.
		/// </summary>
		/// <param name="file">
		/// File to parse.
		/// </param>
		/// <param name="projectBase">
		/// Physical source input directory.
		/// </param>
		/// <returns>
		/// True if the operation was successful. Otherwise, false.
		/// </returns>
		public static bool Parse(FileInfoItem file, DirectoryInfo projectBase)
		{
			bool bo = false;					//	Open console line.
			ContainerInfo ct = null;	//	Recursive call container.
			string fc = "";						//	File Content.
			TextLineCollection ln = null;		//	Lines of text.
			int lp = 0;								//	List Position.
			bool rv = false;					//	Return Value.

			if(file != null)
			{
				file.ClearAll();
				//	The object name of the file is filename without extension.
				file.Name = "";
				//	The directory name is relative to the project base.
				file.mDirectoryName = "";
				file.mPath = "";

				try
				{
					if(file.mSystemFile != null && file.mSystemFile.Exists &&
						projectBase != null && projectBase.Exists)
					{
						//	Read the entire file into our master string.
						file.mFilename = file.mSystemFile.Name;
						file.mPath = file.mSystemFile.Directory.FullName;
						file.Name = Utils.RemoveFileExtension(file.mFilename, false);
						file.mDirectoryName = Utils.GetRelativePath(
							projectBase.FullName,
							file.mSystemFile.Directory.FullName).Replace("\\", "/");
						if(Program.Verbosity >= 2)
						{
							Console.Write("Parsing file: " +
								(file.mDirectoryName.Length > 0 ?
								file.mDirectoryName + " " : "") + file.Name);
							bo = true;
						}
						fc = File.ReadAllText(file.mSystemFile.FullName);
						if(fc.Length > 0)
						{
							//	Content was found.
							ln = new TextLineCollection();
							ln.AddRange(fc);
							if(Program.Verbosity >= 2)
							{
								Console.WriteLine(" " + ln.Count.ToString() + " lines...");
								bo = false;
							}
							if(ln.Count > 0)
							{
								//	At this point, each line is stored separately in the
								//	lines collection.
								ct = ContainerBase.GetContainerInfo(file, ln, lp, 0);
								lp = Utils.ParseAll(ct);
							}
							else
							{
								//	Blank source file doesn't count as error.
								lp = 0;
							}
							if(lp > -1)
							{
								rv = true;
							}
						}
						else
						{
							//	If the file is empty, there was no problem reading it.
							Console.WriteLine(" 0 lines...");
							bo = false;
							rv = true;
						}
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine("\r\nError parsing file: " + ex.Message);
				}
			}
			if(Program.Verbosity >= 2)
			{
				if(rv)
				{
					if(bo)
					{
						Console.WriteLine("");
						bo = false;
					}
				}
				else
				{
					Console.WriteLine(" Parse was unsuccessful.");
					bo = false;
				}
			}
			return rv;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Path																																	*
		//*-----------------------------------------------------------------------*
		private string mPath = "";
		/// <summary>
		/// Get/Set the Path of the file.
		/// </summary>
		public string Path
		{
			get { return mPath; }
			set { mPath = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SystemFile																														*
		//*-----------------------------------------------------------------------*
		private FileInfo mSystemFile = null;
		/// <summary>
		/// Get/Set a reference to the System FileInfo related to this file.
		/// </summary>
		public FileInfo SystemFile
		{
			get { return mSystemFile; }
			set { mSystemFile = value; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
