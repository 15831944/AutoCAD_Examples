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


[assembly: CommandClass(typeof(IsolateLayer.Commands))]
namespace IsolateLayer
{
    public class Commands
    {
        [CommandMethod("IL")]
        public void IsolateLayer()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Collection of selected entities
            ObjectIdCollection ids = new ObjectIdCollection();

            // The results object for nested selection
            PromptNestedEntityResult rs;

            using (Transaction tr = doc.TransactionManager.StartTransaction())
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
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                    for (int i = 0; i < ids.Count; i++)
                    {
                        // Selected drawing object
                        Entity ent = tr.GetObject(ids[i], OpenMode.ForRead) as Entity;

                        // LayerTableRecord for each selected entity
                        LayerTableRecord ltr = tr.GetObject(lt[ent.Layer], OpenMode.ForRead) as LayerTableRecord;
                        
                        db.Clayer = lt[ltr.Name];

                        LayerTable layerTable = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                        
                        foreach (ObjectId id in layerTable)
                        {
                            LayerTableRecord layerTableRecord = (LayerTableRecord)tr.GetObject(id, OpenMode.ForWrite);

                            if (layerTableRecord.Name != ltr.Name && layerTableRecord.Name != "0")
                            {
                                layerTableRecord.IsFrozen = true;
                            }
                        }
                    }
                    tr.Commit();

                    // Regen clears highlighting and reflects the new layer
                    ed.Regen();
                }
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


        /// <summary>
        ///  This method returns the list of layers
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private List<string> GetLayerNames(Database db)
        {
            var layers = new List<string>();
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId id in layerTable)
                {
                    var layer = (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    layers.Add(layer.Name.ToUpper());
                }
            }
            return layers;
        }

    }
}
