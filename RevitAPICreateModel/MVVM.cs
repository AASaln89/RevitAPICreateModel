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
        public void CreateWalls(Level level1, Level level2)
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            double width = UnitUtils.ConvertToInternalUnits(12000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);

            double dx = width / 2;
            double dy = depth / 2;
            double dz = 0;

            List<Wall> walls = new List<Wall>();

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, dz));
            points.Add(new XYZ(dx, -dy, dz));
            points.Add(new XYZ(dx, dy, dz));
            points.Add(new XYZ(-dx, dy, dz));
            points.Add(new XYZ(-dx, -dy, dz));

            Transaction ts = new Transaction(doc);
            ts.Start("Create walls");
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }
            ts.Commit();
        }
    }
}
