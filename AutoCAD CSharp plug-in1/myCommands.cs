// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using System.Collections.Generic;
using System.Text;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(AutoCAD_CSharp_plug_in1.MyCommands))]

namespace AutoCAD_CSharp_plug_in1
{

	// This class is instantiated by AutoCAD for each document when
	// a command is called by the user the first time in the context
	// of a given document. In other words, non static data in this class
	// is implicitly per-document!
	public class MyCommands
	{
		// The CommandMethod attribute can be applied to any public  member 
		// function of any public class.
		// The function should take no arguments and return nothing.
		// If the method is an intance member then the enclosing class is 
		// intantiated for each document. If the member is a static member then
		// the enclosing class is NOT intantiated.
		//
		// NOTE: CommandMethod has overloads where you can provide helpid and
		// context menu.

		// Modal Command with localized name
		[CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
		public void MyCommand() // This method can have any name
		{
			// Put your command code here
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed;
			if (doc != null)
			{
				ed = doc.Editor;
				ed.WriteMessage("Hello, this is your first command.");

			}
		}

		// Modal Command with pickfirst selection
		[CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
		public void MyPickFirst() // This method can have any name
		{
			PromptSelectionResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
			if (result.Status == PromptStatus.OK)
			{
				// There are selected entities
				// Put your command using pickfirst set code here
			}
			else
			{
				// There are no selected entities
				// Put your command code here
			}
		}

		
		//Queries  the state of the Application Window and displays
		//the state in a message box to the user

		[CommandMethod("CWS")]
		public static void CurrentWindowState()
		{
			System.Windows.Forms.MessageBox.Show("The Application window is " + Application.MainWindow.WindowState.ToString(), "Window State");
			
		}

		[CommandMethod("SendACommandToAutoCAD")]
		public static void SendACommandToAutoCAD()
		{
			Document acDoc = Application.DocumentManager.MdiActiveDocument;

			//Draws a Circle and zooms to the extents 
			//or limits of the drawing

			acDoc.SendStringToExecute("._circle 2, 2, 0 4 ", true, false, false);
			acDoc.SendStringToExecute("._zoom _all ", true, false, false);
		}


		//Select All Hatches
		[CommandMethod("SelectAllHatches")]
		public static void SelectAllHatches()
		{
			// get the current document and database
			Document acDoc = Application.DocumentManager.MdiActiveDocument;
			Database CadDB = acDoc.Database;
			Editor acDocEd = acDoc.Editor;

			//create a TypedValue array to define the filter criteria
			TypedValue[] acTypValArray = new TypedValue[1];
			acTypValArray.SetValue(new TypedValue((int)DxfCode.Start, "HATCH"),0);

			//Assing the filter criteria to a SelectionFilter object
			SelectionFilter selFilter = new SelectionFilter(acTypValArray);

			//Start a Transaction
			using (Transaction acTrans = CadDB.TransactionManager.StartTransaction())
			{
				var hatchName = new List<string>();
				var hatch = new List<string>();

				//Select All (no Prompt)
				PromptSelectionResult selectionPrompt;
				selectionPrompt = acDocEd.SelectAll(selFilter);

				//If the prompt status is OK, objects were selected
				if (selectionPrompt.Status == PromptStatus.OK)
				{
					SelectionSet SelSet = selectionPrompt.Value;
					

					//Step through the objects in the Selection Set
					foreach (SelectedObject selObj in SelSet)
					{
						if (selObj != null)
						{
							Hatch acEnt = acTrans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Hatch;
							hatchName.Add(acEnt.PatternName.ToString());
							var lineId = acEnt.GetAssociatedObjectIdsAt(0)[0];
							DBObject line = acTrans.GetObject(lineId, OpenMode.ForRead);
						
							


						}
					}
					StringBuilder sb = new StringBuilder();

					foreach (string pt in hatchName)
					{
						sb.Append(pt);
						sb.Append(", ");
					}

					sb.AppendLine("And the lines");

					foreach (string pts in hatch)
					{
						sb.Append(pts);
						sb.Append(",");
					}

					Application.ShowAlertDialog(sb.ToString());


				}
				else
				{
					Application.ShowAlertDialog("Number of objects Selected: 0");
				}



			}
		}


		[CommandMethod("delABC")]
		public void DelABC()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;

			try
			{
				using (Transaction tr =
					db.TransactionManager.StartTransaction())
				{
					BlockTable BT =
						(BlockTable)tr.GetObject(db.BlockTableId,
												OpenMode.ForRead);

					TypedValue[] filterlist = new TypedValue[2];
					filterlist[0] = new TypedValue(0, "INSERT");
					filterlist[1] = new TypedValue(2, "ABC");

					SelectionFilter filter =
								new SelectionFilter(filterlist);

					PromptSelectionOptions opts =
									new PromptSelectionOptions();
					opts.MessageForAdding = "Select entities: ";

					PromptSelectionResult selRes =
									ed.GetSelection(opts, filter);

					if (selRes.Status != PromptStatus.OK)
					{
						ed.WriteMessage(
							"\nNo ABC block references selected");
						return;
					}

					if (selRes.Value.Count != 0)
					{
						SelectionSet set = selRes.Value;

						foreach (ObjectId id in set.GetObjectIds())
						{
							BlockReference oEnt =
									(BlockReference)tr.GetObject(id,
													OpenMode.ForWrite);
							oEnt.Erase();
						}
					}
					tr.Commit();
				}
			}
			catch (System.Exception ex)
			{
				ed.WriteMessage(ex.ToString());
			}
		}

		[CommandMethod("BlockIterator")]
		public static void BlockIterator_Method()
		{
			Database database = HostApplicationServices.WorkingDatabase;
			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				BlockTable blkTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
				foreach (ObjectId id in blkTable)
				{
					BlockTableRecord btRecord = (BlockTableRecord)transaction.GetObject(id, OpenMode.ForRead);
					if (btRecord.IsLayout)
					{
						if (btRecord.Name.StartsWith("metro"))
						{
							//Access to the block (not model/paper space)
							Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\nBlock name: {0}", btRecord.Name));
						}
						else						{
							Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\nBlock name: {0}", btRecord.));
						}

					}
				}

				transaction.Commit();
			}
		}
	}

}
