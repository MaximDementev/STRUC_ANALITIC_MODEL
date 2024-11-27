using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Reference = Autodesk.Revit.DB.Reference;
using System.Collections.Generic;

namespace STRUC_ANALITIC_MODEL
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class STRUC_ANALITIC_MODEL : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            IList<Reference> selectedReferences = new List<Reference>();

            ICollection<ElementId> selectedElementIds = uiDoc.Selection.GetElementIds();
            
            if (selectedElementIds.Count == 0)
            {
                selectedReferences = uiDoc.Selection.PickObjects(ObjectType.Element,
                    "Выберите элементы для отключения аналитической модели");
            }

            else
            {
                foreach (ElementId id in selectedElementIds)
                {
                    Element element = uiDoc.Document.GetElement(id); 
                    Reference reference = new Reference(element);   
                    selectedReferences.Add(reference);
                }
            }
            if (selectedReferences.Count == 0)
            {
                TaskDialog.Show("Ошибка", $"Не было выбрано ни одного элемента");
                return Result.Failed; 
            }

            using (Transaction trans = new Transaction(doc, "Отключение аналитической модели"))
            {
                int countModifiedElements = 0;
                trans.Start();
                foreach (Reference reference in selectedReferences)
                {
                    Element element = doc.GetElement(reference);

                    if (element != null && !element.get_Parameter(BuiltInParameter.STRUCTURAL_ANALYTICAL_MODEL).IsReadOnly)
                    {
                        Parameter analyticalModelParam = element.get_Parameter(BuiltInParameter.STRUCTURAL_ANALYTICAL_MODEL);
                        if (analyticalModelParam != null && 
                            analyticalModelParam.IsReadOnly == false && 
                            analyticalModelParam.AsInteger()!=0)
                        {
                            analyticalModelParam.Set(0); 
                            countModifiedElements ++ ;
                        }
                    }
                }

                TaskDialog.Show("Успешно", $"Отключена аналитическая модель у {countModifiedElements} элементов");

                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
}
