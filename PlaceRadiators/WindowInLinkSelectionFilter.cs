using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace PlaceRadiators
{
    public class WindowInLinkSelectionFilter<T> : ISelectionFilter where T : Element
    {
        private Document Doc;

        public WindowInLinkSelectionFilter(Document doc)
        {
            Doc = doc;
        }

        public Document LinkedDocument { get; private set; } = null;

        public bool LastCheckedWasFromLink
        {
            get { return null != LinkedDocument; }
        }

        public bool AllowElement(Element e)
        {
            return true;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            LinkedDocument = null;

            Element e = Doc.GetElement(r);

            if (e is RevitLinkInstance)
            {
                RevitLinkInstance li = e as RevitLinkInstance;

                LinkedDocument = li.GetLinkDocument();

                e = LinkedDocument.GetElement(r.LinkedElementId);
            }
            return (e is T && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows);
        }
    }
}
