﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UTA.Models;
using UTA.ViewModels;
// using ImportModule; // TODO vonshick REMOVE IT AFTER TESTING

namespace UTA.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewmodel = new MainViewModel();

        public MainView()
        {
            //TODO vonshick REMOVE IT AFTER TESTING
            // SampleProgram.ProcessSampleData(true, false, false, false); // csv
            // SampleProgram.ProcessSampleData(false, true, false, false); // utx
            // SampleProgram.ProcessSampleData(false, false, true, false); // xml
            // SampleProgram.ProcessSampleData(false, false, false, true); // xmcda
            
            InitializeComponent();
            SetBindings();
            _viewmodel.PropertyChanged += ViewmodelPropertyChanged;
            _viewmodel.GenerateCriteriaTable();
//            CriteriaDataGrid.View = GenerateGridView(_viewmodel.CriteriaTable);
            _viewmodel.GenerateAlternativesTable();
            AlternativesDataGrid.IsReadOnly = true;
            ButtonEditAlternatives.Content = "Editing is OFF";
        }

        private void SetBindings()
        {
            TextBoxAlternativeName.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeName") { Source = this });
            TextBoxAlternativeDescription.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeDescription") { Source = this });
            AlternativesDataGrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            CriteriaDataGrid.SetBinding(ListView.ItemsSourceProperty, new Binding("CriteriaTable") { Source = _viewmodel });
            RankingDataGrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
//            RankingDataGrid.DataContext = CriteriaTable
        }

        public string InputAlternativeName { get; set; }
        public string InputAlternativeDescription { get; set; }

        public void ViewmodelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AlternativesTable":
//                    AlternativesDataGrid.View = GenerateGridView(_viewmodel.AlternativesTable);
                    break;
                case "CriteriaTable":
//                    CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
                    break;
                default:
                    throw new Exception("error in prop: " + e.PropertyName);
            }
        }
        private GridView GenerateGridView(DataTable table)
        {
            GridView view = new GridView();
            foreach (DataColumn column in table.Columns)
            {
                view.Columns.Add(new GridViewColumn()
                {
                    Header = column.ColumnName,
                    DisplayMemberBinding = new Binding(column.ColumnName)
                });
            }
            return view;
        }

        public void ShowAddCriterionDialog(object sender, RoutedEventArgs routedEventArgs)
        {
            _viewmodel.ShowAddCriterionDialog();
        }

        private void AddAlternative(object sender, RoutedEventArgs e)
        {
            if (DataValidation.StringsNotEmpty(InputAlternativeName, InputAlternativeDescription))
            {
                _viewmodel.AddAlternative(InputAlternativeName, InputAlternativeDescription);
                TextBoxAlternativeName.Clear();
                TextBoxAlternativeDescription.Clear();
                InputAlternativeName = "";
                InputAlternativeDescription = "";
            }
            else
            {
                //todo notify user
            }
        }

        private void EditAlternativesSwitch(object sender, RoutedEventArgs e)
        {
            AlternativesDataGrid.IsReadOnly = !AlternativesDataGrid.IsReadOnly;
            ButtonEditAlternatives.Content = AlternativesDataGrid.IsReadOnly ? "Editing is OFF" : "Editing is ON";
        }

    }

}