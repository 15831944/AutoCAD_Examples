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

using AcAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(CommandLine.Commands))]

namespace CommandLine
{
    public class Commands
    {
        // Instance variables
        Document doc; 
        double radius;
        string layer;

        /// <summary>
        /// This method creates an instance for Commands
        /// This constructor is called once per document
        /// on first call of the method 'CommandMethod'
        /// </summary>
        public Commands()
        {
            // Initialization of private fields (initial default values)
            doc = AcAp.DocumentManager.MdiActiveDocument;
            radius = 10.0;
            layer = (string)AcAp.GetSystemVariable("clayer");
        }

        /// <summary>
        /// This command draw a Circle into the current space given: layer_name, radius_value and centre_point
        /// </summary>
        [CommandMethod("CMD_CIRCLE")]
        public void DrawCircleCmd()
        {
            var db = doc.Database;
            var ed = doc.Editor;

            // Layer choice
            var layerList = GetLayerNames(db);
            if (!layerList.Contains(layer))
            {
                layer = (string)AcAp.GetSystemVariable("clayer");
            }
            
            // Request user input for layer name
            var layerName = new PromptStringOptions("\nLayer name: ");
            layerName.DefaultValue = layer;
            layerName.UseDefaultValue = true;
            
            var layerInput = ed.GetString(layerName);
            if (layerInput.Status != PromptStatus.OK)
                return;
            
            if (!layerList.Contains(layerInput.StringResult.ToUpper()))
            {
                ed.WriteMessage(
                  $"\nNo layer named '{layerInput.StringResult}' in the layer's table.");
                return;
            }
            
            layer = layerInput.StringResult.ToUpper();

            // Request user input for radius value
            var radiusInput = new PromptDistanceOptions("\nEnter the radius: ");
            radiusInput.DefaultValue = radius;
            radiusInput.UseDefaultValue = true;
            
            var distResult = ed.GetDistance(radiusInput);
            if (distResult.Status != PromptStatus.OK)
                return;
            
            radius = distResult.Value;

            // Request user input for centre point
            var centrePoint = ed.GetPoint("\nEnter the centre point: ");
            if (centrePoint.Status == PromptStatus.OK)
            {
                // Draw the circle in current space
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    
                    using (var circle = new Circle(centrePoint.Value, Vector3d.ZAxis, radius))
                    {
                        circle.TransformBy(ed.CurrentUserCoordinateSystem);
                        circle.Layer = layerInput.StringResult;
                        curSpace.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                    }
                    tr.Commit();
                }
            }
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
