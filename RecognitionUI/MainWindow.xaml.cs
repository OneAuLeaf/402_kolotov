﻿using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;

namespace RecognitionUI
{
    public partial class MainWindow : Window
    {
        IModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new RecognitionViewModel(new WPFUI());
            DataContext = viewModel;
            viewModel.State.PropertyChanged += OnStateChangeHandler;
            OnStateChangeHandler(this, null);
        }

        private void OnStateChangeHandler(object sender, PropertyChangedEventArgs args)
        {
            switch (viewModel.State.State)
            {
                case StateVM.States.UNREADY:
                        button_ChooseModel.IsEnabled = true;
                        button_ChooseDir.IsEnabled = true;
                        button_Start.IsEnabled = false;
                        button_Cancel.IsEnabled = false;
                        button_Clear.IsEnabled = true;
                        break;
                case StateVM.States.READY:
                        button_ChooseModel.IsEnabled = true;
                        button_ChooseDir.IsEnabled = true;
                        button_Start.IsEnabled = true;
                        button_Cancel.IsEnabled = false;
                        button_Clear.IsEnabled = true;
                        break;
                case StateVM.States.PROCESS:
                        button_ChooseModel.IsEnabled = false;
                        button_ChooseDir.IsEnabled = false;
                        button_Start.IsEnabled = false;
                        button_Cancel.IsEnabled = true;
                        button_Clear.IsEnabled = false;
                        break;
                case StateVM.States.CANCELLING:
                        button_ChooseModel.IsEnabled = false;
                        button_ChooseDir.IsEnabled = false;
                        button_Start.IsEnabled = false;
                        button_Cancel.IsEnabled = false;
                        button_Clear.IsEnabled = false;
                        break;
                case StateVM.States.COMPLETED:
                        button_ChooseModel.IsEnabled = true;
                        button_ChooseDir.IsEnabled = true;
                        button_Start.IsEnabled = true;
                        button_Cancel.IsEnabled = false;
                        button_Clear.IsEnabled = true;
                        break;
                case StateVM.States.CANCELED:
                        button_ChooseModel.IsEnabled = true;
                        button_ChooseDir.IsEnabled = true;
                        button_Start.IsEnabled = true;
                        button_Cancel.IsEnabled = false;
                        button_Clear.IsEnabled = true;
                        break;
            }
        }

        private void button_ChooseDir_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChooseDir();
        }
        private void button_ChooseModel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChooseModel();
        }

        private void button_Start_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Start();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }
        private void button_Clear_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Clear();
        }
    }

    public class WPFUI : IView
    {
        public string ChooseDir()
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dlg.ShowDialog() ?? false)
            {
                return dlg.SelectedPath;
            }
            return null;
        }
        public string ChooseModel()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }
            return null;
        }
    }
}
