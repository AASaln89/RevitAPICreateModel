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

        //public void CreateRoof(Document document, Level level2, List<XYZ> points, List<Wall> walls)
        //{
        //    double wallWidth = walls[0].Width;
        //    double dt = wallWidth / 2;
        //    double dz = 0;
        //    List<XYZ> pointsRoof = new List<XYZ>();
        //    points.Add(new XYZ(-dt, -dt, dz));
        //    points.Add(new XYZ(dt, -dt, dz));
        //    points.Add(new XYZ(dt, dt, dz));
        //    points.Add(new XYZ(-dt, dt, dz));

        //    ElementId id = document.GetDefaultElementTypeId(ElementTypeGroup.RoofType);
        //    RoofType type = document.GetElement(id) as RoofType;

        //    CurveArray curveArray = new CurveArray();
        //    curveArray.Append(Line.CreateBound(pointsRoof[0], pointsRoof[1]));
        //    curveArray.Append(Line.CreateBound(pointsRoof[2], pointsRoof[3]));

        //    using (Transaction ts = new Transaction(document))
        //    {
        //        ts.Start("Create ExtrusionRoof");
        //        ReferencePlane plane = document.Create.NewReferencePlane(new XYZ(0, 0, 0), new XYZ(0, 0, 20), new XYZ(0, 20, 0), document.ActiveView);
        //        document.Create.NewExtrusionRoof(curveArray, plane, level2, type, 0, 40);
        //        ts.Commit();
        //    }
        //}

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

            //CreateRoof(document, level2, points, walls);
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
            Transaction ts = new Transaction(document, "addDoors");
            ts.Start();
            document.Create.NewFamilyInstance(point, doorType, wall, level1, StructuralType.NonStructural);
            ts.Commit();
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
            XYZ point = (point1 + point2) / 3;

            if (!windowType.IsActive)
                windowType.Activate();

            Transaction ts = new Transaction(document, "addWindows");
            ts.Start();
            document.Create.NewFamilyInstance(point, windowType, wall, level1, StructuralType.NonStructural);
            ts.Commit();
        }
    }
}
