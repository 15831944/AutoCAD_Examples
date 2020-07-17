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
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace SimpleTable
{
    public class Commands
    {
        [CommandMethod("simpletable")]
        public void testaddtable()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                ObjectId msId = bt[BlockTableRecord.ModelSpace];

                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(msId, OpenMode.ForWrite);

                // create a table
                Table tb = new Table();
                tb.TableStyle = db.Tablestyle;

                // row number
                Int32 RowsNum = 5;

                // column number
                Int32 ColumnsNum = 5;

                // row height
                double rowheight = 3;

                // column width
                double columnwidth = 20;

                // insert rows and columns
                tb.InsertRows(0, rowheight, RowsNum);

                tb.InsertColumns(0, columnwidth, ColumnsNum);

                tb.SetRowHeight(rowheight);
                tb.SetColumnWidth(columnwidth);

                Point3d eMax = db.Extmax;
                Point3d eMin = db.Extmin;
                double CenterY = (eMax.Y + eMin.Y) * 0.5;

                tb.Position = new Point3d(10, 10, 0);

                // fill in the cell one by one
                for (int i = 0; i < RowsNum; i++)
                {
                    for (int j = 0; j < ColumnsNum; j++)
                    {
                        tb.Cells[i, j].TextHeight = 1;
                        if (i == 0 && j == 0)
                            tb.Cells[i, j].TextString = "The Title";
                        else
                            tb.Cells[i, j].TextString = i.ToString() + "," + j.ToString();

                        tb.Cells[i, j].Alignment = CellAlignment.MiddleCenter;
                    }
                }

                tb.GenerateLayout();
                btr.AppendEntity(tb);
                tr.AddNewlyCreatedDBObject(tb, true);
                tr.Commit();
            }
        }
    }
}
