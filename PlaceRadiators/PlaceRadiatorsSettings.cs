using System.IO;
using System.Xml.Serialization;

namespace PlaceRadiators
{
    public class PlaceRadiatorsSettings
    {
        public string SelectedWindowWidthParameterName;
        public string SelectedRadiatorFamilyName;
        public string SelectedRadiatorTypeName;
        public string RadiatorWidthByButtonName;
        public string SelectedRadiatorWidthParameterName;
        public string SelectedRadiatorThicknessParameterName;

        public string PercentageLength;
        public string IndentFromLevel;
        public string IndentFromWall;

        public static PlaceRadiatorsSettings GetSettings()
        {
            PlaceRadiatorsSettings placeRadiatorsSettings = null;
            string assemblyPathAll = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string fileName = "PlaceRadiatorsSettings.xml";
            string assemblyPath = assemblyPathAll.Replace("PlaceRadiators.dll", fileName);

            if (File.Exists(assemblyPath))
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(PlaceRadiatorsSettings));
                    placeRadiatorsSettings = xSer.Deserialize(fs) as PlaceRadiatorsSettings;
                    fs.Close();
                }
            }
            else
            {
                placeRadiatorsSettings = null;
            }

            return placeRadiatorsSettings;
        }

        public void SaveSettings()
        {
            string assemblyPathAll = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string fileName = "PlaceRadiatorsSettings.xml";
            string assemblyPath = assemblyPathAll.Replace("PlaceRadiators.dll", fileName);

            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }

            using (FileStream fs = new FileStream(assemblyPath, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(PlaceRadiatorsSettings));
                xSer.Serialize(fs, this);
                fs.Close();
            }
        }
    }
}
