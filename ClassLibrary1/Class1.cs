
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RevitPlugin
{

    [Transaction(TransactionMode.Manual)]
    public class CreateChamferCommand : IExternalCommand
    {
        
        private Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            doc = uiDoc.Document;

            List<Wall> selectedWalls = GetSelectedWalls(uiDoc, doc);

            if (selectedWalls.Count >= 4)
            {
                ModifySlabGeometry(selectedWalls, 300.0, 50.0);
                return Result.Succeeded;
            }
            else
            {
                TaskDialog.Show("Ошибка", "Выберите хотя бы 4 стены для модификации перекрытия.");
                return Result.Failed;
            }
        }

        private List<Wall> GetSelectedWalls(UIDocument uiDoc, Document doc)
        {
            List<Wall> selectedWalls = new List<Wall>();

            ICollection<ElementId> selectedIds = uiDoc.Selection.GetElementIds();
            foreach (ElementId id in selectedIds)
            {
                Element elem = doc.GetElement(id) as Wall;
                if (elem != null)
                {
                    selectedWalls.Add(elem as Wall);
                }
            }

            return selectedWalls;
        }

        private void ModifySlabGeometry(List<Wall> walls, double offset, double minOffset)
        {
            Level level = doc.GetElement(walls[0].LevelId) as Level;
            if (level != null)
            {
                List<Curve> wallCurves = new List<Curve>();
                foreach (Wall wall in walls)
                {
                    LocationCurve locationCurve = wall.Location as LocationCurve;
                    if (locationCurve != null)
                    {
                        wallCurves.Add(locationCurve.Curve);
                    }
                }

                List<XYZ> points = GetPoints(wallCurves, offset, minOffset, level);
                List<PolymeshFacet> facets = CreateFacetsFromPoints(points);


                using (Transaction transaction = new Transaction(doc, "Modify Slab Geometry"))
                {
                    transaction.Start();


                    TopographySurface topoSurface = TopographySurface.Create(doc, points, facets);

                    transaction.Commit();
                }

            }
            else
            {
                TaskDialog.Show("Ошибка", "Не удалось получить уровень для перекрытия.");
            }
        }

        private List<XYZ> GetPoints(List<Curve> wallCurves, double offset, double minOffset, Level level)
        {
            Square square = GetSquare(wallCurves);

            XYZ topLeftOuter = new XYZ(square.left - offset, square.top + offset, 0);
            XYZ bottomLeftOuter = new XYZ(square.left - offset, square.bottom - offset, 0);
            XYZ bottomRightOuter = new XYZ(square.right + offset, square.bottom - offset, 0);
            XYZ topRightOuter = new XYZ(square.right + offset, square.top + offset, 0);

            XYZ topLeftInner = new XYZ(square.left - minOffset, square.top + minOffset, 0);
            XYZ bottomLeftInner = new XYZ(square.left - minOffset, square.bottom - minOffset, 0);
            XYZ bottomRightInner = new XYZ(square.right + minOffset, square.bottom - minOffset, 0);
            XYZ topRightInner = new XYZ(square.right + minOffset, square.top + minOffset, 0);

            XYZ topLeft = new XYZ(square.left, square.top, level.Elevation - 0.2);
            XYZ bottomLeft = new XYZ(square.left, square.bottom, level.Elevation - 0.2);
            XYZ bottomRight = new XYZ(square.right, square.bottom, level.Elevation - 0.2);
            XYZ topRight = new XYZ(square.right, square.top, level.Elevation - 0.2);

            List<XYZ> outerPolygon = new List<XYZ>() { 
                topLeftOuter, 
                bottomLeftOuter, 
                bottomRightOuter, 
                topRightOuter, 
                topLeftInner,
                bottomLeftInner, 
                bottomRightInner,
                topRightInner, 
                topLeft, 
                bottomLeft, 
                bottomRight, 
                topRight
            };
            return outerPolygon;
        }

        public List<PolymeshFacet> CreateFacetsFromPoints(IList<XYZ> points)
        {
            List<PolymeshFacet> facets = new List<PolymeshFacet>();

            PolymeshFacet outerFacet1 = new PolymeshFacet(0, 1, 4);
            facets.Add(outerFacet1);
            PolymeshFacet outerFacet2 = new PolymeshFacet(1, 4, 5);
            facets.Add(outerFacet2);
            PolymeshFacet outerFacet3 = new PolymeshFacet(1, 5, 2);
            facets.Add(outerFacet3);
            PolymeshFacet outerFacet4 = new PolymeshFacet(2, 5, 6);
            facets.Add(outerFacet4);
            PolymeshFacet outerFacet5 = new PolymeshFacet(2, 6, 3);
            facets.Add(outerFacet5);
            PolymeshFacet outerFacet6 = new PolymeshFacet(3, 6, 7);
            facets.Add(outerFacet6);
            PolymeshFacet outerFacet7 = new PolymeshFacet(3, 7 , 0);
            facets.Add(outerFacet7);
            PolymeshFacet outerFacet8 = new PolymeshFacet(4, 7, 0);
            facets.Add(outerFacet8);


            PolymeshFacet middleFacet1 = new PolymeshFacet(4, 5, 8);
            facets.Add(middleFacet1);
            PolymeshFacet middleFacet2 = new PolymeshFacet(5, 8, 9);
            facets.Add(middleFacet2);
            PolymeshFacet middleFacet3 = new PolymeshFacet(5, 9, 6);
            facets.Add(middleFacet3);
            PolymeshFacet middleFacet4 = new PolymeshFacet(6, 9, 10);
            facets.Add(middleFacet4);
            PolymeshFacet middleFacet5 = new PolymeshFacet(6, 10, 7);
            facets.Add(middleFacet5);
            PolymeshFacet middleFacet6 = new PolymeshFacet(7, 10, 11);
            facets.Add(middleFacet6);
            PolymeshFacet middleFacet7 = new PolymeshFacet(7, 11, 4);
            facets.Add(middleFacet7);
            PolymeshFacet middleFacet8 = new PolymeshFacet(8, 11, 4);
            facets.Add(middleFacet8);

            PolymeshFacet innerFacet1 = new PolymeshFacet(8, 9, 11);
            facets.Add(innerFacet1);
            PolymeshFacet innerFacet2 = new PolymeshFacet(10, 9, 11);
            facets.Add(innerFacet2);

            return facets;
        }

        private Square GetSquare(List<Curve> wallCurves)
        {
            Square square = new Square(0,0,0,0);

            foreach (Curve curve in wallCurves)
            {
                square.left = Math.Min(curve.GetEndPoint(0).X, square.left);
                square.right = Math.Max(curve.GetEndPoint(0).X, square.right);
                square.bottom = Math.Min(curve.GetEndPoint(0).Y, square.bottom);
                square.top = Math.Max(curve.GetEndPoint(0).Y, square.top);
            }

            return square;
        }

        class Square
        {
            public double left;
            public double top;
            public double bottom;
            public double right;

            public Square(double left, double top, double bottom, double right)
            {
                this.left = left;
                this.top = top;
                this.bottom = bottom;
                this.right = right;
            }
        }

    }



}


