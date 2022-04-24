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

            return Result.Succeeded;
        }
    }
}
