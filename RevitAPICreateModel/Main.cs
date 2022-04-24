using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPICreateModel
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            MVVM mvvm = new MVVM(commandData);

            Level level1 = mvvm.GetLevel1();
            Level level2 = mvvm.GetLevel2();

            List<XYZ> points = mvvm.CreateWalls();
            mvvm.CreateModel(doc, points, level1, level2);

            //List<Wall> walls = new List<Wall>();
            //double wallWidth = walls[0].Width;
            //double dt = wallWidth / 2;
            //double dz = 0;
            //List<XYZ> pointsRoof = new List<XYZ>();
            //points.Add(new XYZ(-dt, -dt, dz));
            //points.Add(new XYZ(dt, -dt, dz));
            //points.Add(new XYZ(dt, dt, dz));
            //points.Add(new XYZ(-dt, dt, dz));
            //points.Add(new XYZ(-dt, -dt, dz));

            //ElementId id = doc.GetDefaultElementTypeId(ElementTypeGroup.RoofType);
            //RoofType type = doc.GetElement(id) as RoofType;
            //if (type == null)
            //{
            //    TaskDialog.Show("Error", "Not RoofType");
            //    return Result.Failed;
            //}

            //CurveArray curveArray = new CurveArray();
            //curveArray.Append(Line.CreateBound(pointsRoof[0], pointsRoof[1]));
            //curveArray.Append(Line.CreateBound(pointsRoof[2], pointsRoof[3]));

            //using (Transaction ts = new Transaction(doc))
            //{
            //    ts.Start("Create ExtrusionRoof");
            //    ReferencePlane plane = doc.Create.NewReferencePlane(new XYZ(0, 0, 0), new XYZ(0, 0, 20), new XYZ(0, 20, 0), doc.ActiveView);
            //    doc.Create.NewExtrusionRoof(curveArray, plane, level2, type, 0, 40);
            //    ts.Commit();
            //}
            return Result.Succeeded;
        }
    }
}
