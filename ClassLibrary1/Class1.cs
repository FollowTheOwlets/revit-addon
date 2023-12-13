//CreateChamferCommand

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        private Floor GetSelectedSlab(Document doc)
        {
            // Получаем выбранный элемент (перекрытие)
            UIDocument uidoc = new UIDocument(doc);
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            foreach (ElementId id in selectedIds)
            {
                Element elem = doc.GetElement(id);

                if (elem is Floor slab)
                {
                    return slab;
                }
            }

            return null;
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
            List<XYZ> wallCorners = GetWallCorners(wallCurves);
            XYZ topLeftOuter = new XYZ(wallCorners[0].X - offset, wallCorners[0].Y + offset, 0);
            XYZ bottomLeftOuter = new XYZ(wallCorners[1].X - offset, wallCorners[1].Y - offset, 0);
            XYZ bottomRightOuter = new XYZ(wallCorners[2].X + offset, wallCorners[2].Y - offset, 0);
            XYZ topRightOuter = new XYZ(wallCorners[3].X + offset, wallCorners[3].Y + offset, 0);

            XYZ topLeftInner = new XYZ(wallCorners[0].X - minOffset, wallCorners[0].Y + minOffset, 0);
            XYZ bottomLeftInner = new XYZ(wallCorners[1].X - minOffset, wallCorners[1].Y - minOffset, 0);
            XYZ bottomRightInner = new XYZ(wallCorners[2].X + minOffset, wallCorners[2].Y - minOffset, 0);
            XYZ topRightInner = new XYZ(wallCorners[3].X + minOffset, wallCorners[3].Y + minOffset, 0);

            XYZ topLeft = new XYZ(wallCorners[0].X, wallCorners[0].Y, level.Elevation);
            XYZ topRight = new XYZ(wallCorners[1].X, wallCorners[1].Y, level.Elevation);
            XYZ bottomRight = new XYZ(wallCorners[2].X, wallCorners[2].Y, level.Elevation);
            XYZ bottomLeft = new XYZ(wallCorners[3].X, wallCorners[3].Y, level.Elevation);

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
                topRight, 
                bottomRight, 
                bottomLeft 
            };
            return outerPolygon;
        }

        public List<PolymeshFacet> CreateFacetsFromPoints(IList<XYZ> points)
        {
            List<PolymeshFacet> facets = new List<PolymeshFacet>();

            PolymeshFacet facet1 = new PolymeshFacet(0, 1, 4);
            facets.Add(facet1);
            PolymeshFacet facet2 = new PolymeshFacet(1, 4, 5);
            facets.Add(facet2);
            PolymeshFacet facet3 = new PolymeshFacet(1, 5, 2);
            facets.Add(facet3);
            PolymeshFacet facet4 = new PolymeshFacet(2, 5, 6);
            facets.Add(facet4);
            PolymeshFacet facet5 = new PolymeshFacet(2, 6, 3);
            facets.Add(facet5);
            PolymeshFacet facet6 = new PolymeshFacet(3, 6, 7);
            facets.Add(facet6);
            PolymeshFacet facet7 = new PolymeshFacet(3, 7 , 0);
            facets.Add(facet7);
            PolymeshFacet facet8 = new PolymeshFacet(4, 7, 0);
            facets.Add(facet8);


            PolymeshFacet facet11 = new PolymeshFacet(4, 5, 8);
            facets.Add(facet11);
            PolymeshFacet facet21 = new PolymeshFacet(5, 8, 9);
            facets.Add(facet21);
            PolymeshFacet facet31 = new PolymeshFacet(5, 9, 6);
            facets.Add(facet31);
            PolymeshFacet facet41 = new PolymeshFacet(6, 9, 10);
            facets.Add(facet41);
            PolymeshFacet facet51 = new PolymeshFacet(6, 10, 7);
            facets.Add(facet51);
            PolymeshFacet facet61 = new PolymeshFacet(7, 10, 11);
            facets.Add(facet61);
            PolymeshFacet facet71 = new PolymeshFacet(7, 11, 4);
            facets.Add(facet71);
            PolymeshFacet facet81 = new PolymeshFacet(8, 11, 4);
            facets.Add(facet81);

            return facets;
        }

        private List<XYZ> GetWallCorners(List<Curve> wallCurves)
        {
            List<XYZ> wallCorners = new List<XYZ>();

            foreach (Curve curve in wallCurves)
            {
                wallCorners.Add(curve.GetEndPoint(0));
            }

            return wallCorners;
        }

    }



}


