//	Program.cs
//	Copyright (c). 2018 Daniel Patterson, MCSD (danielanywhere).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyDoc
{
	//*-------------------------------------------------------------------------*
	//*	Program																																	*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// The main instance of the application.
	/// </summary>
	/// <remarks>
	/// This application creates readable, cross-referenced information from
	/// Python source.
	/// </remarks>
	public class Program
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
		//*	_Main																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// The command-line startup of this application.
		/// </summary>
		/// <param name="args">
		/// Command-line arguments, following the syntax of /argName[:argValue]
		/// </param>
		public static void Main(string[] args)
		{
			Program prg = new Program();
			string ps = "";			//	Parameter.
			string tl = "";			//	Lowercase parameter.

			foreach(string argi in args)
			{
				tl = argi.ToLower();
				//	Help. /?
				ps = "/?";
				if(tl == ps)
				{
					prg.mShowSyntax = true;
				}
				//	Input Path. /inp:{Pathname}
				ps = "/inp:";
				if(tl.StartsWith(ps))
				{
					prg.mInputPath = argi.Substring(ps.Length);
				}
				//	Output Path. /outp:{Pathname}
				ps = "/outp:";
				if(tl.StartsWith(ps))
				{
					prg.mOutputPath = argi.Substring(ps.Length);
				}
				//	Verbosity Level. /v{Level}
				ps = "/v:";
				if(tl.StartsWith(ps))
				{
					Program.Verbosity = Convert.ToInt32(argi.Substring(ps.Length));
				}
				//	Wait for keypress. /w
				ps = "/w";
				if(tl == ps)
				{
					prg.mWaitAfterEnd = true;
				}
			}
			if(prg.mShowSyntax)
			{
				Console.WriteLine("PyDoc.exe");
				Console.WriteLine(
					" Create cross-referenced documentation for Python source.");
				Console.WriteLine(" Usage:");
				Console.WriteLine(" PyDoc /path:{Pathname} [options]");
				Console.WriteLine(
					"   /?                 - Display this message.\r\n" +
					"   /inp:{Pathname}    - Search within this path for input.\r\n" +
					"   /outp:{Pathname}   - Write documentation files to this\r\n" +
					"                        output path.\r\n" +
					"   /v:{Level}         - Verbosity level.\r\n" +
					"                        Default=0.\r\n" +
					"   /w                 - Wait for keypress after application end."
					);
			}
			else
			{
				prg.Run();
			}
			if(prg.mWaitAfterEnd)
			{
				Console.WriteLine("Press [Enter] to continue...");
				Console.ReadLine();
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Files																																	*
		//*-----------------------------------------------------------------------*
		private FileInfoCollection mFiles = new FileInfoCollection();
		/// <summary>
		/// Get a reference to the collection of files on this instance.
		/// </summary>
		public FileInfoCollection Files
		{
			get { return mFiles; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputPath																															*
		//*-----------------------------------------------------------------------*
		private string mInputPath = "";
		/// <summary>
		/// Get/Set the Input Path.
		/// </summary>
		public string InputPath
		{
			get { return mInputPath; }
			set { mInputPath = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputPath																														*
		//*-----------------------------------------------------------------------*
		private string mOutputPath = "";
		/// <summary>
		/// Get/Set the Output Path.
		/// </summary>
		public string OutputPath
		{
			get { return mOutputPath; }
			set { mOutputPath = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Run																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the configured instance of the application.
		/// </summary>
		public void Run()
		{
			bool bc = false;								//	Flag - Continue.
			DirectoryInfo ds = null;				//	Source Directory.
			DirectoryInfo dt = null;				//	Target Directory.
			FileInfo[] fa = null;						//	Files in folder.

			try
			{
				ds = new DirectoryInfo(mInputPath);
				dt = new DirectoryInfo(mOutputPath);
				bc = true;
				if(ds.Exists)
				{
					//	Only continue if the source folder exists.
					if(!dt.Exists)
					{
						//	If the target doesn't exist, then create it.
						dt.Create();
						dt = new DirectoryInfo(mOutputPath);
					}
					if(!dt.Exists)
					{
						//	Target couldn't be created.
						Console.WriteLine("Error: Target couldn't be created...");
						bc = false;
					}
				}
				else
				{
					bc = false;
				}
				if(bc)
				{
					//	Source and Target folders exist.
					//	Get the list of all files for our language.
					fa = ds.GetFiles("*.py", SearchOption.AllDirectories);
					foreach(FileInfo fi in fa)
					{
						mFiles.Add(fi);
					}
					//	Once all files have been registered, then parse each one.
					bc = mFiles.Parse(ds);
					if(bc)
					{
						//	Prepare statistics and document the files.
						FileInfoCollection.Document(mFiles, dt);
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error in main application. " + ex.Message);
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShowSyntax																														*
		//*-----------------------------------------------------------------------*
		private bool mShowSyntax = false;
		/// <summary>
		/// Get/Set a value indicating whether to show the usage syntax.
		/// </summary>
		public bool ShowSyntax
		{
			get { return mShowSyntax; }
			set { mShowSyntax = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Verbosity																															*
		//*-----------------------------------------------------------------------*
		private static int mVerbosity = 0;
		/// <summary>
		/// Get/Set the Verbosity level of this instance.
		/// </summary>
		public static int Verbosity
		{
			get { return mVerbosity; }
			set { mVerbosity = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WaitAfterEnd																													*
		//*-----------------------------------------------------------------------*
		private bool mWaitAfterEnd = false;
		/// <summary>
		/// Get/Set a value indicating whether to wait for keypress after the end
		/// of the application.
		/// </summary>
		public bool WaitAfterEnd
		{
			get { return mWaitAfterEnd; }
			set { mWaitAfterEnd = value; }
		}
		//*-----------------------------------------------------------------------*
	}
	//*-------------------------------------------------------------------------*
}
