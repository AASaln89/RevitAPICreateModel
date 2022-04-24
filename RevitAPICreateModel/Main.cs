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

            double width = UnitUtils.ConvertToInternalUnits(12000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);

            MVVM mvvm = new MVVM(commandData);

            Level level1 = mvvm.GetLevel1();
            Level level2 = mvvm.GetLevel2();
            List<XYZ> points = mvvm.SetWalls(width, depth);

            List<Wall> walls = mvvm.CreateWalls(doc, points, level1, level2);
            mvvm.InsertDoors(doc, level1, walls[0]);
            mvvm.InsertWindows(doc, level1, walls);
            mvvm.CreateRoof(doc, level2, walls);

            return Result.Succeeded;
        }
    }
}
