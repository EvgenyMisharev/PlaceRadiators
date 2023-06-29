using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlaceRadiators
{
    public partial class PlaceRadiatorsWPF : Window
    {
        Document Doc;
        public Parameter SelectedWindowWidthParameter;
        public FamilySymbol SelectedRadiatorType;
        public Parameter SelectedRadiatorWidthParameter;
        public Parameter SelectedRadiatorThicknessParameter;

        public string PercentageLength;
        public string IndentFromLevel;
        public string IndentFromWall;

        PlaceRadiatorsSettings PlaceRadiatorsSettingsItem = null;
        public PlaceRadiatorsWPF(Document doc, List<Parameter> windowParameterList, List<Family> mechanicalEquipmentList)
        {
            Doc = doc;
            InitializeComponent();

            PlaceRadiatorsSettingsItem = PlaceRadiatorsSettings.GetSettings();
            if (PlaceRadiatorsSettingsItem != null)
            {
                comboBox_WindowWidthParameter.ItemsSource = windowParameterList;
                comboBox_WindowWidthParameter.DisplayMemberPath = "Definition.Name";
                if (windowParameterList.Count != 0)
                {
                    if (windowParameterList.FirstOrDefault(wp => wp.Definition.Name == PlaceRadiatorsSettingsItem.SelectedWindowWidthParameterName) != null)
                    {
                        comboBox_WindowWidthParameter.SelectedItem = windowParameterList.FirstOrDefault(wp => wp.Definition.Name == PlaceRadiatorsSettingsItem.SelectedWindowWidthParameterName);
                    }
                    else
                    {
                        comboBox_WindowWidthParameter.SelectedItem = comboBox_WindowWidthParameter.Items[0];
                    }
                }

                comboBox_RadiatorFamilySelection.ItemsSource = mechanicalEquipmentList;
                comboBox_RadiatorFamilySelection.DisplayMemberPath = "Name";
                if (mechanicalEquipmentList.Count != 0)
                {
                    if (mechanicalEquipmentList.FirstOrDefault(me => me.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorFamilyName) != null)
                    {
                        comboBox_RadiatorFamilySelection.SelectedItem = mechanicalEquipmentList.FirstOrDefault(me => me.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorFamilyName);
                    }
                    else
                    {
                        comboBox_RadiatorFamilySelection.SelectedItem = comboBox_RadiatorFamilySelection.Items[0];
                    }
                }

                if (comboBox_RadiatorTypeSelection.Items.Count != 0)
                {
                    if (comboBox_RadiatorTypeSelection.Items.Cast<FamilySymbol>().FirstOrDefault(fs => fs.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorTypeName) != null)
                    {
                        comboBox_RadiatorTypeSelection.SelectedItem = comboBox_RadiatorTypeSelection.Items.Cast<FamilySymbol>().FirstOrDefault(fs => fs.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorTypeName);
                    }
                    else
                    {
                        comboBox_RadiatorTypeSelection.SelectedItem = comboBox_RadiatorTypeSelection.Items[0];
                    }
                }

                if (comboBox_RadiatorWidthParameter.Items.Count != 0)
                {
                    if(comboBox_RadiatorWidthParameter.Items.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorWidthParameterName) != null)
                    {
                        comboBox_RadiatorWidthParameter.SelectedItem = comboBox_RadiatorWidthParameter.Items.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorWidthParameterName);
                    }
                    else
                    {
                        comboBox_RadiatorWidthParameter.SelectedItem = comboBox_RadiatorWidthParameter.Items[0];
                    }
                }

                if (comboBox_RadiatorThicknessParameter.Items.Count != 0)
                {
                    if (comboBox_RadiatorThicknessParameter.Items.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorThicknessParameterName) != null)
                    {
                        comboBox_RadiatorThicknessParameter.SelectedItem = comboBox_RadiatorThicknessParameter.Items.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name == PlaceRadiatorsSettingsItem.SelectedRadiatorThicknessParameterName);
                    }
                    else
                    {
                        comboBox_RadiatorThicknessParameter.SelectedItem = comboBox_RadiatorThicknessParameter.Items[0];
                    }
                }

                textBox_PercentageLength.Text = PlaceRadiatorsSettingsItem.PercentageLength;
                textBox_IndentFromLevel.Text = PlaceRadiatorsSettingsItem.IndentFromLevel;
                textBox_IndentFromWall.Text = PlaceRadiatorsSettingsItem.IndentFromWall;
            }
            else
            {
                comboBox_WindowWidthParameter.ItemsSource = windowParameterList;
                comboBox_WindowWidthParameter.DisplayMemberPath = "Definition.Name";
                if (windowParameterList.Count != 0)
                {
                    comboBox_WindowWidthParameter.SelectedItem = comboBox_WindowWidthParameter.Items[0];
                }

                comboBox_RadiatorFamilySelection.ItemsSource = mechanicalEquipmentList;
                comboBox_RadiatorFamilySelection.DisplayMemberPath = "Name";
                if (mechanicalEquipmentList.Count != 0)
                {
                    comboBox_RadiatorFamilySelection.SelectedItem = comboBox_RadiatorFamilySelection.Items[0];
                }
            }
        }

        private void comboBox_RadiatorFamilySelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Family selectedRadiatorFamily = comboBox_RadiatorFamilySelection.SelectedItem as Family;
            List<ElementId> selectedRadiatorTypesList = selectedRadiatorFamily.GetFamilySymbolIds().ToList();
            List<FamilySymbol> radiatorTypesList = new List<FamilySymbol>();
            foreach (ElementId id in selectedRadiatorTypesList)
            {
                radiatorTypesList.Add(Doc.GetElement(id) as FamilySymbol);
            }
            radiatorTypesList = radiatorTypesList.OrderBy(fs => fs.Name, new AlphanumComparatorFastString()).ToList();
            
            comboBox_RadiatorTypeSelection.ItemsSource = radiatorTypesList;
            comboBox_RadiatorTypeSelection.DisplayMemberPath = "Name";
            if(radiatorTypesList.Count != 0)
            {
                comboBox_RadiatorTypeSelection.SelectedItem = comboBox_RadiatorTypeSelection.Items[0];
            }
        }
        private void comboBox_RadiatorTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FamilySymbol selectedRadiatorFamilyType = comboBox_RadiatorTypeSelection.SelectedItem as FamilySymbol;
            if(selectedRadiatorFamilyType != null)
            {
                ParameterSet selectedRadiatorFamilyTypeParameterSet = selectedRadiatorFamilyType.Parameters;
                List<Parameter> selectedRadiatorFamilyTypeParameterList = new List<Parameter>();
                foreach (Parameter parameter in selectedRadiatorFamilyTypeParameterSet)
                {
                    selectedRadiatorFamilyTypeParameterList.Add(parameter);
                }
                selectedRadiatorFamilyTypeParameterList = selectedRadiatorFamilyTypeParameterList
                    .Where(p => p.StorageType == StorageType.Double)
                    .OrderBy(p => p.Definition.Name, new AlphanumComparatorFastString()).ToList();

                comboBox_RadiatorWidthParameter.ItemsSource = selectedRadiatorFamilyTypeParameterList;
                comboBox_RadiatorWidthParameter.DisplayMemberPath = "Definition.Name";
                if (selectedRadiatorFamilyTypeParameterList.Count != 0)
                {
                    comboBox_RadiatorWidthParameter.SelectedItem = comboBox_RadiatorWidthParameter.Items[0];
                }

                comboBox_RadiatorThicknessParameter.ItemsSource = selectedRadiatorFamilyTypeParameterList;
                comboBox_RadiatorThicknessParameter.DisplayMemberPath = "Definition.Name";
                if (selectedRadiatorFamilyTypeParameterList.Count != 0)
                {
                    comboBox_RadiatorThicknessParameter.SelectedItem = comboBox_RadiatorThicknessParameter.Items[0];
                }
            }
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            DialogResult = true;
            Close();
        }
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void FillMEPParametersWPF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                SaveSettings();

                DialogResult = true;
                Close();
            }

            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void SaveSettings()
        {
            PlaceRadiatorsSettingsItem = new PlaceRadiatorsSettings();

            SelectedWindowWidthParameter = comboBox_WindowWidthParameter.SelectedItem as Parameter;
            PlaceRadiatorsSettingsItem.SelectedWindowWidthParameterName = SelectedWindowWidthParameter.Definition.Name;

            PlaceRadiatorsSettingsItem.SelectedRadiatorFamilyName = (comboBox_RadiatorFamilySelection.SelectedItem as Family).Name;

            SelectedRadiatorType = comboBox_RadiatorTypeSelection.SelectedItem as FamilySymbol;
            PlaceRadiatorsSettingsItem.SelectedRadiatorTypeName = SelectedRadiatorType.Name;

            SelectedRadiatorWidthParameter = comboBox_RadiatorWidthParameter.SelectedItem as Parameter;
            PlaceRadiatorsSettingsItem.SelectedRadiatorWidthParameterName = SelectedRadiatorWidthParameter.Definition.Name;

            SelectedRadiatorWidthParameter = comboBox_RadiatorWidthParameter.SelectedItem as Parameter;
            PlaceRadiatorsSettingsItem.SelectedRadiatorWidthParameterName = SelectedRadiatorWidthParameter.Definition.Name;

            SelectedRadiatorThicknessParameter = comboBox_RadiatorThicknessParameter.SelectedItem as Parameter;
            PlaceRadiatorsSettingsItem.SelectedRadiatorThicknessParameterName = SelectedRadiatorThicknessParameter.Definition.Name;
            
            PercentageLength = textBox_PercentageLength.Text;
            PlaceRadiatorsSettingsItem.PercentageLength = PercentageLength;

            IndentFromLevel = textBox_IndentFromLevel.Text;
            PlaceRadiatorsSettingsItem.IndentFromLevel = IndentFromLevel;

            IndentFromWall = textBox_IndentFromWall.Text;
            PlaceRadiatorsSettingsItem.IndentFromWall = IndentFromWall;

            PlaceRadiatorsSettingsItem.SaveSettings();
        }
    }
}
