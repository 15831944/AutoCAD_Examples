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

namespace WriteDataNOD
{
    public class Commands
    {
        // Write some data to the NOD
        //============================
        [CommandMethod("WNOD")]
        public void WriteToNOD()
        {
            Database db = new Database();
            try
            {
                // We will write to C:\Temp\Test.dwg. Make sure it exists!
                // Load it into AutoCAD
                db.ReadDwgFile(@"C:\Temp\Test.dwg",
                                System.IO.FileShare.ReadWrite, false, null);

                using (Transaction trans =
                                  db.TransactionManager.StartTransaction())
                {

                    // Find the NOD in the database
                    DBDictionary nod = (DBDictionary)trans.GetObject(
                                db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                    // We use Xrecord class to store data in Dictionaries
                    Xrecord myXrecord = new Xrecord();
                    myXrecord.Data = new ResultBuffer(
                            new TypedValue((int)DxfCode.Int16, 1234),
                            new TypedValue((int)DxfCode.Text,
                                            "This drawing has been processed"));

                    // Create the entry in the Named Object Dictionary
                    nod.SetAt("MyData", myXrecord);
                    trans.AddNewlyCreatedDBObject(myXrecord, true);

                    // Now let's read the data back and print them out
                    //  to the Visual Studio's Output window
                    ObjectId myDataId = nod.GetAt("MyData");
                    Xrecord readBack = (Xrecord)trans.GetObject(
                                                  myDataId, OpenMode.ForRead);
                    foreach (TypedValue value in readBack.Data)
                        System.Diagnostics.Debug.Print(
                                  "===== OUR DATA: " + value.TypeCode.ToString()
                                  + ". " + value.Value.ToString());
                    trans.Commit();
                } // using

                db.SaveAs(@"C:\Temp\Test.dwg", DwgVersion.Current);
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
            finally
            {
                db.Dispose();
            }

        } // End of WriteToNOD()
    }
}
