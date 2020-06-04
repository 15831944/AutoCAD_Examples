using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Specialized;

namespace TableCreation
{ 
    public class Commands

    {
        // Set up some formatting constants
        // for the table
        const double colWidth = 15.0;
        const double rowHeight = 3.0;
        const double textHeight = 1.0;
        const CellAlignment cellAlign = CellAlignment.MiddleCenter;

        // Helper function to set text height
        // and alignment of specific cells,
        // as well as inserting the text
        static public void SetCellText(Table tb, int row, int col, string value)
        {
            tb.Cells[row, col].Alignment = cellAlign;
            tb.Cells[row, col].TextHeight = textHeight;
            tb.Cells[row, col].Value = value;
        }

        [CommandMethod("BAT")]
        static public void BlockAttributeTable()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;


            // Ask for the name of the block to find
            PromptStringOptions opt = new PromptStringOptions("\nEnter name of block to list: ");

            PromptResult pr = ed.GetString(opt);
            if (pr.Status == PromptStatus.OK)
            {
                string blockToFind = pr.StringResult.ToUpper();
                bool embed = false;

                // Ask whether to embed or link the data
                PromptKeywordOptions pko = new PromptKeywordOptions("\nEmbed or link the attribute values: ");

                pko.AllowNone = true;
                pko.Keywords.Add("Embed");
                pko.Keywords.Add("Link");
                pko.Keywords.Default = "Embed";
                PromptResult pkr = ed.GetKeywords(pko);

                if (pkr.Status == PromptStatus.None || pkr.Status == PromptStatus.OK)
                {
                    if (pkr.Status == PromptStatus.None || pkr.StringResult == "Embed")
                        embed = true;
                    else
                        embed = false;
                }


                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Let's check the block exists
                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                    if (!bt.Has(blockToFind))
                    {
                        ed.WriteMessage("\nBlock " + blockToFind + " does not exist.");
                    }
                    else
                    {
                        // And go through looking for
                        // attribute definitions
                        StringCollection colNames = new StringCollection();

                        BlockTableRecord bd = (BlockTableRecord)tr.GetObject(bt[blockToFind], OpenMode.ForRead);

                        foreach (ObjectId adId in bd)
                        {
                            DBObject adObj = tr.GetObject(adId, OpenMode.ForRead);

                            // For each attribute definition we find...
                            AttributeDefinition ad = adObj as AttributeDefinition;

                            if (ad != null)
                            {
                                // ... we add its name to the list
                                colNames.Add(ad.Tag);
                            }
                        }

                        if (colNames.Count == 0)
                        {
                            ed.WriteMessage("\nThe block " + blockToFind + " contains no attribute definitions.");
                        }

                        else
                        {
                            // Ask the user for the insertion point
                            // and then create the table
                            PromptPointResult ppr = ed.GetPoint("\nEnter table insertion point: ");

                            if (ppr.Status == PromptStatus.OK)
                            {
                                Table tb = new Table();
                                tb.TableStyle = db.Tablestyle;
                                tb.SetSize(2, colNames.Count);

                                tb.SetRowHeight(rowHeight);
                                tb.SetColumnWidth(colWidth);
                                tb.Position = ppr.Value;

                                // Let's add our column headings
                                for (int i = 0; i < colNames.Count; i++)
                                {
                                    SetCellText(tb, 0, i, colNames[i]);
                                }

                                // Now let's search for instances of
                                // our block in the modelspace
                                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                                int rowNum = 1;
                                foreach (ObjectId objId in ms)
                                {
                                    DBObject obj = tr.GetObject(objId,OpenMode.ForRead);
                                    BlockReference br = obj as BlockReference;
                                    if (br != null)
                                    {
                                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                                        using (btr)
                                        {
                                            if (btr.Name.ToUpper() == blockToFind)
                                            {
                                                // We have found one of our blocks,
                                                // so add a row for it in the table
                                                tb.InsertRows(rowNum, rowHeight, 1);

                                                // Assume that the attribute refs
                                                // follow the same order as the
                                                // attribute defs in the block
                                                int attNum = 0;
                                                foreach (ObjectId arId in br.AttributeCollection)
                                                {
                                                    DBObject arObj = tr.GetObject(arId, OpenMode.ForRead);

                                                    AttributeReference ar = arObj as AttributeReference;
                                                    if (ar != null)
                                                    {
                                                        // Embed or link the values
                                                        string strCell;
                                                        if (embed)
                                                        {
                                                            strCell = ar.TextString;
                                                        }

                                                        else
                                                        {
                                                            string strArId = arId.ToString();
                                                            strArId = strArId.Trim(new char[] { '(', ')' });

                                                            strCell = "%<\\AcObjProp Object(" + "%<\\_ObjId " + strArId + ">%).TextString>%";
                                                        }

                                                        SetCellText(tb, rowNum, attNum, strCell);
                                                    }
                                                    attNum++;
                                                }
                                                rowNum++;
                                            }
                                        }
                                    }
                                }
                                tb.GenerateLayout();
                                ms.UpgradeOpen();
                                ms.AppendEntity(tb);
                                tr.AddNewlyCreatedDBObject(tb, true);
                                tr.Commit();
                            }
                        }
                    }
                }
            }
        }
        
        
        [CommandMethod("CRT")]
        static public void CreateTable()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            
            PromptPointResult pr = ed.GetPoint("\nEnter table insertion point: ");

            if (pr.Status == PromptStatus.OK)
            {
                Transaction tr = doc.TransactionManager.StartTransaction();

                using (tr)
                {
                    BlockTable bt =
                      (BlockTable)tr.GetObject(
                        doc.Database.BlockTableId,
                        OpenMode.ForRead
                      );

                    Table tb = new Table();
                    tb.TableStyle = db.Tablestyle;
                    tb.SetSize(5, 5);
                    
                    // Added an additional column for the block image
                    // and one for the "is dynamic" flag
                    tb.SetRowHeight(3);
                    tb.SetColumnWidth(15);
                    tb.Position = pr.Value;


                    // Create a 2-dimensional array
                    // of our table contents
                    string[,] str = new string[5, 4];
                    str[0, 0] = "Part No.";
                    str[0, 1] = "Name ";
                    str[0, 2] = "Material ";
                    str[1, 0] = "1876-1";
                    str[1, 1] = "Flange";
                    str[1, 2] = "Perspex";
                    str[2, 0] = "0985-4";
                    str[2, 1] = "Bolt";
                    str[2, 2] = "Steel";
                    str[3, 0] = "3476-K";
                    str[3, 1] = "Tile";
                    str[3, 2] = "Ceramic";
                    str[4, 0] = "8734-3";
                    str[4, 1] = "Kean";
                    str[4, 2] = "Mostly water";

                    // Use a nested loop to add and format each cell
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            tb.SetTextHeight(i, j, 1);
                            tb.SetTextString(i, j, str[i, j]);
                            tb.SetAlignment(i, j, CellAlignment.MiddleCenter);
                        }

                        // Adding title information for additional columns
                        if (i == 0)
                        {
                            tb.SetTextHeight(i, 3, 1);
                            tb.SetTextString(i, 3, "Block Preview");
                            tb.SetAlignment(i, 3, CellAlignment.MiddleCenter);
                            tb.SetTextHeight(i, 4, 1);
                            tb.SetTextString(i, 4, "Is Dynamic?");
                            tb.SetAlignment(i, 4, CellAlignment.MiddleCenter);
                        }

                        // If a block definition exists for a block of our
                        // "name" field, then let's set it in the 4th column
                        if (bt.Has(str[i, 1]))
                        {
                            ObjectId objId = bt[str[i, 1]];
                            tb.SetBlockTableRecordId(i, 3, objId, true);
                            // And then we use a field to check on whether
                            // it's a dynamic block or not
                            string strObjId = objId.ToString();
                            strObjId = strObjId.Trim(new char[] { '(', ')' });
                            
                            tb.SetTextHeight(i, 4, 1);

                            tb.SetTextString(i, 4, "%<\\AcObjProp Object(%<\\_ObjId " + strObjId + ">%).IsDynamicBlock \\f \"%bl2\">%");
                            tb.SetAlignment(i, 4, CellAlignment.MiddleCenter);
                        }
                    }

                    tb.GenerateLayout();
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    btr.AppendEntity(tb);
                    tr.AddNewlyCreatedDBObject(tb, true);
                    tr.Commit();
                }
            }
        }
    }
}
