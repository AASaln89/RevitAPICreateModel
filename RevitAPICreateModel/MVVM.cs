using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPICreateModel
{
    public class MVVM
    {
        public ExternalCommandData _commandData;
        public DelegateCommand saveCommand { get; }


        public MVVM(ExternalCommandData commandData)
        {
            _commandData = commandData;
        }

        public Level GetLevel1()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<Level> listLevel= new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            Level level1 = listLevel
                .Where(x => x.Name.Equals("Level 0"))
                .FirstOrDefault();

            return level1;
        }

        public Level GetLevel2()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<Level> listLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            Level level2 = listLevel
                .Where(x => x.Name.Equals("Level 1"))
                .FirstOrDefault();

            return level2;
        }
        public List<XYZ> CreateWalls()
        {
            double width = UnitUtils.ConvertToInternalUnits(12000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);

            double dx = width / 2;
            double dy = depth / 2;
            double dz = 0;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, dz));
            points.Add(new XYZ(dx, -dy, dz));
            points.Add(new XYZ(dx, dy, dz));
            points.Add(new XYZ(-dx, dy, dz));
            points.Add(new XYZ(-dx, -dy, dz));

            return points;
        }

        public void CreateModel(Document document, List<XYZ> points, Level level1, Level level2)
        {
            List<Wall> walls = new List<Wall>();
            Transaction ts = new Transaction(document);
            ts.Start("Create walls");
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(document, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }

            InsertDoors(document, level1, walls[0]);
            InsertWindows(document, level1, walls[1]);
            InsertWindows(document, level1, walls[2]);
            InsertWindows(document, level1, walls[3]);

            ts.Commit();
        }

        public void InsertDoors(Document document, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("1810x2110mm"))
                .Where(x => x.FamilyName.Equals("Doors_ExtDbl_Flush"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1+point2)/2;

            if (!doorType.IsActive)
                doorType.Activate();
        }

        public void InsertWindows(Document document, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("910x910mm"))
                .Where(x => x.FamilyName.Equals("Windows_Sgl_Plain"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!windowType.IsActive)
                windowType.Activate();
        }
    }
}
