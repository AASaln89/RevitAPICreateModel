using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
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
        public List<XYZ> SetWalls(double width, double depth)
        {
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

        public void CreateRoof(Document document, Level level2, List<Wall> walls)
        {
            var level = level2;

            RoofType roofType = new FilteredElementCollector(document)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Roof_Generic-400mm"))
                .Where(x => x.FamilyName.Equals("Basic Roof"))
                .FirstOrDefault();

            double wallWidth = walls[0].Width;
            double df = wallWidth / 2;
            double dh = level.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble();
            XYZ dt = new XYZ(-df, -df, dh);
            XYZ dz = new XYZ(0, 0, 20);
            XYZ dy = new XYZ(0, 20, 0);
            LocationCurve locationCurve = walls[0].Location as LocationCurve;
            XYZ point = locationCurve.Curve.GetEndPoint(0);
            double l = (walls[0].Location as LocationCurve).Curve.Length + df * 2;
            double w = ((walls[1].Location as LocationCurve).Curve.Length / 2) + df;
            XYZ origin = point + dt;
            XYZ vy = XYZ.BasisY;
            XYZ vz = XYZ.BasisZ;

            CurveArray curve = new CurveArray();
            curve.Append(Line.CreateBound(origin, origin + new XYZ(0, w, 5)));
            curve.Append(Line.CreateBound(origin + new XYZ(0, w, 5), origin + new XYZ(0, w * 2, 0)));

            var av = document.ActiveView;
            Transaction ts = new Transaction(document, "Create roof");
            ts.Start();

            ReferencePlane plane = document.Create.NewReferencePlane2(origin, origin - vz, origin + vy, av);

            ExtrusionRoof extrusionRoof = document.Create.NewExtrusionRoof(curve, plane, level, roofType, 0, l);

            ts.Commit();
        }

        public List<Wall> CreateWalls(Document document, List<XYZ> points, Level level1, Level level2)
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
            ts.Commit();

            return walls;
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

            Transaction ts = new Transaction(document, "addDoors");
            ts.Start();
            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!doorType.IsActive)
                doorType.Activate();
            document.Create.NewFamilyInstance(point, doorType, wall, level1, StructuralType.NonStructural);
            ts.Commit();
        }

        public void InsertWindows(Document document, Level level1, List<Wall> walls)
        {
            FamilySymbol windowType = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("910x910mm"))
                .Where(x => x.FamilyName.Equals("Windows_Sgl_Plain"))
                .FirstOrDefault();

            Transaction ts = new Transaction(document, "addWindows");
            ts.Start();
            if (!windowType.IsActive)
                windowType.Activate();
            for (int i = 1; i < 4; i++)
            {
                Wall wall = walls[i];
                LocationCurve hostCurve = wall.Location as LocationCurve;
                XYZ point1 = hostCurve.Curve.GetEndPoint(0);
                XYZ point2 = hostCurve.Curve.GetEndPoint(1);
                XYZ point = (point1 + point2) / 2;

                var window = document.Create.NewFamilyInstance(point, windowType, wall, level1, StructuralType.NonStructural);

                window.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(UnitUtils.ConvertToInternalUnits(850, UnitTypeId.Millimeters));
            }
            ts.Commit();
        }
    }
}
