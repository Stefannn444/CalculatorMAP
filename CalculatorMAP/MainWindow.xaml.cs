﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CalculatorMAP
{
    public partial class MainWindow : Window
    {
        private AppViewModel ViewModel => DataContext as AppViewModel;
        public MainWindow()
        {
           InitializeComponent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Jîtea Ștefan-Alexandru\n10LF232", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.Display);
            ViewModel.Display = "0";
            ViewModel.ExpressionList.Clear();
            ViewModel.ExpressionList.Add("0");

        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.Display);
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText()) 
            {
                string clipboardText = Clipboard.GetText();
                if (double.TryParse(clipboardText, out double clipboardValue)) 
                {
                    ViewModel.Display = clipboardValue.ToString();
                }
                else
                {
                    MessageBox.Show("Please paste one number at a time");
                }
            }

        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            


            switch (e.Key)
            {
                // Numbers
                case Key.D0:
                case Key.NumPad0:
                    ViewModel.NumberCommand.Execute("0");
                    break;
                case Key.D1:
                case Key.NumPad1:
                    ViewModel.NumberCommand.Execute("1");
                    break;
                case Key.D2:
                case Key.NumPad2:
                    ViewModel.NumberCommand.Execute("2");
                    break;
                case Key.D3:
                case Key.NumPad3:
                    ViewModel.NumberCommand.Execute("3");
                    break;
                case Key.D4:
                case Key.NumPad4:
                    ViewModel.NumberCommand.Execute("4");
                    break;
                case Key.D5:
                case Key.NumPad5:
                    ViewModel.NumberCommand.Execute("5");
                    break;
                case Key.D6:
                case Key.NumPad6:
                    ViewModel.NumberCommand.Execute("6");
                    break;
                case Key.D7:
                case Key.NumPad7:
                    ViewModel.NumberCommand.Execute("7");
                    break;
                
                case Key.D8:
                case Key.NumPad8:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        ViewModel.BinaryOperatorCommand.Execute("*");
                    else
                        ViewModel.NumberCommand.Execute("8");
                    break;
                case Key.D9:
                case Key.NumPad9:
                    ViewModel.NumberCommand.Execute("9");
                    break;

                // Hex letters (A-F) - only in programmer mode
                case Key.A:
                     ViewModel.NumberCommand.Execute("A");
                    break;
                case Key.B:
                    ViewModel.NumberCommand.Execute("B");
                    break;
                case Key.C:
                    ViewModel.NumberCommand.Execute("C");
                    break;
                case Key.D:
                    ViewModel.NumberCommand.Execute("D");
                    break;
                case Key.E:
                    ViewModel.NumberCommand.Execute("E");
                    break;
                case Key.F:
                    ViewModel.NumberCommand.Execute("F");
                    break;

                // Operators
                case Key.Add:
                    ViewModel.BinaryOperatorCommand.Execute("+");
                    break;
                case Key.OemPlus:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        ViewModel.BinaryOperatorCommand.Execute("+");
                    else
                        ViewModel.EqualsCommand.Execute(null);
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    ViewModel.BinaryOperatorCommand.Execute("-");
                    break;
                case Key.Multiply:
                    ViewModel.BinaryOperatorCommand.Execute("*");
                    break;
                case Key.Divide:
                case Key.OemQuestion:
                    ViewModel.BinaryOperatorCommand.Execute("/");
                    break;

                // Special Keys
                case Key.Enter:
                    ViewModel.EqualsCommand.Execute(null);
                    break;
                case Key.Back:
                    ViewModel.BackspaceCommand.Execute(null);
                    break;
                case Key.Escape:
                    ViewModel.ClearCommand.Execute(null);
                    break;
                case Key.Decimal:
                case Key.OemPeriod:
                    ViewModel.DecimalCommand.Execute(null);
                    break;
            }
        }

    }
}