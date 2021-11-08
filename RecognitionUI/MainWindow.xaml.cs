using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
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
using Recognition;
using System.IO;

namespace RecognitionUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new RecognitionViewModel(new WPFUI(), this.Dispatcher);
            DataContext = viewModel;
        }

        private void button_ChooseDir_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChooseDir();
        }

        private void button_Start_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Start();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }
    }

    public class WPFUI : IView
    {
        public string ChooseDir()
        {
            var inputPath = @"E:\Projects\C#Labs\402_kolotov\Images";
            return inputPath;
        }
    }
}
