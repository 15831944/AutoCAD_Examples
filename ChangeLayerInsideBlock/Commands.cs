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

[assembly: CommandClass(typeof(ChangeLayerInsideBlock.Commands))]
namespace ChangeLayerInsideBlock
{
    public class Commands
    {
        [CommandMethod("CNL")]
        static public void IsolateLayer()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Collection of our selected entities
            ObjectIdCollection ids = new ObjectIdCollection();

            // The results object for our nested selection
            // (will be reused)
            PromptNestedEntityResult rs;

            // Start a transaction... will initially be used
            // to highlight the selected entities and then to
            // modify their layer
            Transaction tr = doc.TransactionManager.StartTransaction();

            using (tr)
            {
                // Loop until cancelled or completed
                do
                {
                    rs = ed.GetNestedEntity("\nSelect nested entity: ");
                    if (rs.Status == PromptStatus.OK)
                    {
                        ids.Add(rs.ObjectId);
                        HighlightSubEntity(doc, rs);
                    }
                }

                while (rs.Status == PromptStatus.OK);

                if (ids.Count > 0)
                {
                    // Get the name of our destination later
                    PromptResult pr = ed.GetString("\nNew layer for these objects: ");

                    if (pr.Status == PromptStatus.OK)
                    {
                        // Check that the layer exists
                        string newLay = pr.StringResult;

                        LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                        if (lt.Has(newLay))
                        {
                            // If so, set the layer name to be the one chosen
                            // on each of the selected entitires
                            for (int i = 0; i < ids.Count; i++)
                            {
                                Entity ent = tr.GetObject(ids[i], OpenMode.ForWrite) as Entity;
                                if (ent != null)
                                {
                                    ent.Layer = newLay;
                                }
                            }
                        }
                        else
                        {
                            ed.WriteMessage("\nLayer not found in current drawing.");
                        }
                    }
                }
                tr.Commit();

                // Regen clears highlighting and reflects the new layer
                ed.Regen();
            }
        }

        private static void HighlightSubEntity(Document doc, PromptNestedEntityResult rs)
        {
            // Extract relevant information from the prompt object
            ObjectId selId = rs.ObjectId;
            ObjectId[] objIds = rs.GetContainers();

            int len = objIds.Length;

            // Reverse the "containers" list
            ObjectId[] revIds = new ObjectId[len + 1];
            for (int i = 0; i < len; i++)
            {
                ObjectId id = (ObjectId)objIds.GetValue(len - i - 1);
                revIds.SetValue(id, i);
            }

            // Now add the selected entity to the end
            revIds.SetValue(selId, len);

            // Retrieve the sub-entity path for this entity
            SubentityId subEnt = new SubentityId(SubentityType.Null, 0);

            FullSubentityPath path = new FullSubentityPath(revIds, subEnt);

            // Open the outermost container, relying on the open
            // transaction...

            ObjectId id2 = (ObjectId)revIds.GetValue(0);
            Entity ent = id2.GetObject(OpenMode.ForRead) as Entity;

            // ... and highlight the nested entity
            if (ent != null)
                ent.Highlight(path, false);
        }
    }
}
