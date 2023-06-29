using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceRadiators
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PlaceRadiatorsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Document linkDoc = null;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            //Выбор связанного файла
            RevitLinkInstanceSelectionFilter selFilterRevitLinkInstance = new RevitLinkInstanceSelectionFilter();
            Reference selRevitLinkInstance = null;
            try
            {
                selRevitLinkInstance = sel.PickObject(ObjectType.Element, selFilterRevitLinkInstance, "Выберите связанный файл!");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            IEnumerable<RevitLinkInstance> revitLinkInstance = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Where(li => li.Id == selRevitLinkInstance.ElementId)
                .Cast<RevitLinkInstance>();
            if (revitLinkInstance.Count() == 0)
            {
                TaskDialog.Show("Ravit", "Связанный файл не найден!");
                return Result.Cancelled;
            }
            linkDoc = revitLinkInstance.First().GetLinkDocument();
            Transform transform = revitLinkInstance.First().GetTransform();

            List<FamilyInstance> windowList = new List<FamilyInstance>();
            WindowInLinkSelectionFilter<FamilyInstance> selFilter = new WindowInLinkSelectionFilter<FamilyInstance>(doc);

            IList<Reference> selWindows = null;
            try
            {
                selWindows = sel.PickObjects(ObjectType.LinkedElement, selFilter, "Выберите окна!");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            foreach (Reference windowRef in selWindows)
            {
                windowList.Add(linkDoc.GetElement(windowRef.LinkedElementId) as FamilyInstance);
            }

            if(windowList.Count != 0)
            {
                List<Level> docLvlList = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType()
                    .Cast<Level>()
                    .ToList();

                List<Parameter> windowParameterList = new List<Parameter>();
                ParameterSet windowTypeParameterSet = windowList.First().Symbol.Parameters;
                foreach (Parameter parameter in windowTypeParameterSet)
                {
                    windowParameterList.Add(parameter);
                }
                windowParameterList = windowParameterList
                    .Where(p =>p.StorageType == StorageType.Double)
                    .OrderBy(p => p.Definition.Name, new AlphanumComparatorFastString()).ToList();

                List<Family> mechanicalEquipmentList = new List<Family>();
                List<Family> tmpMechanicalEquipmentList = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .Select(fs => doc.GetElement(fs.Family.Id) as Family)
                    .OrderBy(f => f.Name, new AlphanumComparatorFastString())
                    .Distinct()
                    .ToList();
                foreach(Family family in tmpMechanicalEquipmentList)
                {
                    if(mechanicalEquipmentList.FirstOrDefault(me => me.Id == family.Id) == null)
                    {
                        mechanicalEquipmentList.Add(family);
                    }
                }

                PlaceRadiatorsWPF placeRadiatorsWPF = new PlaceRadiatorsWPF(doc, windowParameterList, mechanicalEquipmentList);
                placeRadiatorsWPF.ShowDialog();
                if (placeRadiatorsWPF.DialogResult != true)
                {
                    return Result.Cancelled;
                }

                Parameter windowWidthParameter = placeRadiatorsWPF.SelectedWindowWidthParameter;
                FamilySymbol radiatorType = placeRadiatorsWPF.SelectedRadiatorType;
                Parameter radiatorWidthParameter = placeRadiatorsWPF.SelectedRadiatorWidthParameter;
                Parameter radiatorThicknessParameter = placeRadiatorsWPF.SelectedRadiatorThicknessParameter;

                int percentageLength = 50;
                int.TryParse(placeRadiatorsWPF.PercentageLength, out percentageLength);
                if(percentageLength == 0)
                {
                    percentageLength = 50;
                }

                double indentFromLevel = 0;
                double.TryParse(placeRadiatorsWPF.IndentFromLevel, out indentFromLevel);
                indentFromLevel = indentFromLevel / 304.8;

                double indentFromWall = 0;
                double.TryParse(placeRadiatorsWPF.IndentFromWall, out indentFromWall);
                indentFromWall = indentFromWall / 304.8;

                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    tg.Start("Расставить радиаторы");
                    foreach (FamilyInstance window in windowList)
                    {
                        double windowWidth = Math.Round(window.Symbol.get_Parameter(windowWidthParameter.Definition).AsDouble(), 6);
                        windowWidth = RoundUpToIncrement(windowWidth * percentageLength / 100, 100);

                        FamilySymbol targetRadiatorType = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                            .WhereElementIsElementType()
                            .Cast<FamilySymbol>()
                            .Where(fs => fs.Family.Id == radiatorType.Family.Id)
                            .Where(fs => Math.Round(fs.get_Parameter(radiatorThicknessParameter.Definition).AsDouble(), 6) 
                            == Math.Round(radiatorType.get_Parameter(radiatorThicknessParameter.Definition).AsDouble(), 6))
                            .FirstOrDefault(fs => Math.Round(fs.get_Parameter(radiatorWidthParameter.Definition).AsDouble(), 6) == Math.Round(windowWidth, 6));
                        if (targetRadiatorType == null)
                        {
                            using (Transaction t = new Transaction(doc))
                            {
                                t.Start("Новый тип радиатора");
                                targetRadiatorType = radiatorType.Duplicate($"{radiatorType.Name} L={Math.Round(windowWidth * 304.8)}") as FamilySymbol;
                                targetRadiatorType.get_Parameter(radiatorWidthParameter.Definition).Set(windowWidth);
                                t.Commit();
                            }
                        }

                        using (Transaction t = new Transaction(doc))
                        {
                            t.Start("Установка радиатора");
                            XYZ windowLocation = transform.OfPoint((window.Location as LocationPoint).Point);
                            Level closestRadiatorLevel = GetClosestRoomLevel(docLvlList, linkDoc, window);
                            XYZ radiatorLocation = new XYZ(windowLocation.X, windowLocation.Y, indentFromLevel); ;

                            FamilyInstance newRadiator = doc.Create.NewFamilyInstance(radiatorLocation, targetRadiatorType, closestRadiatorLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                            XYZ newRadiatorFacingOrientation = newRadiator.FacingOrientation;
                            XYZ windowFacingOrientation = transform.OfVector(window.FacingOrientation);
                            double angle = Math.Round(newRadiatorFacingOrientation.AngleTo(windowFacingOrientation), 6);
                            Line axis = Line.CreateBound(radiatorLocation, radiatorLocation + 1 * XYZ.BasisZ);
                            if (angle != 0)
                            {
                                ElementTransformUtils.RotateElement(doc, newRadiator.Id, axis, -angle);
                            }

                            double hostWallWidth = 0;
                            Wall hostWall = linkDoc.GetElement(window.Host.Id) as Wall;
                            if (hostWall != null)
                            {
                                hostWallWidth = hostWall.Width;
                            }
                            ElementTransformUtils.MoveElement(doc, newRadiator.Id, (hostWallWidth / 2 + indentFromWall) * windowFacingOrientation.Negate());
                            t.Commit();
                        }
                    }
                    tg.Assimilate();
                }
            }

            return Result.Succeeded;
        }
        private static Level GetClosestRoomLevel(List<Level> docLvlList, Document linkDoc, FamilyInstance window)
        {
            Level lvl = null;
            double linkFloorLevelElevation = (linkDoc.GetElement(window.LevelId) as Level).Elevation;
            double heightDifference = 10000000000;
            foreach (Level docLvl in docLvlList)
            {
                double tmpHeightDifference = Math.Abs(Math.Round(linkFloorLevelElevation, 6) - Math.Round(docLvl.Elevation, 6));
                if (tmpHeightDifference < heightDifference)
                {
                    heightDifference = tmpHeightDifference;
                    lvl = docLvl;
                }
            }
            return lvl;
        }
        private double RoundUpToIncrement(double value, double increment)
        {
            if (increment == 0)
            {
                return Math.Round(value, 6);
            }
            else
            {
                return (Math.Ceiling((value * 304.8) / increment) * increment) / 304.8;
            }
        }
    }
}
