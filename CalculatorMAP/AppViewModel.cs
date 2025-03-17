using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices.Marshalling;

namespace CalculatorMAP
{
    public class AppViewModel:INotifyPropertyChanged
    {
        //TODO: Culture Info
        //todo: preferences
        //todo file about etc
        //todo: string format for decimal number
        private AppModel _appModel = new AppModel();

        private String _display = "0";
        private ObservableCollection<String> _expressionList = new ObservableCollection<String> {"0" };
        private bool _hasMemoryValues;
        private bool _isMemoryPanelVisible = false;
        private ObservableCollection<MemoryItem> _memoryValues=new ObservableCollection<MemoryItem>();

  
        private bool _isStandardMode = !Properties.Settings.Default.IsProgrammerMode;

        private CultureInfo _currentCulture = CultureInfo.CurrentCulture;
        private String _numberBase = Properties.Settings.Default.NumberBase;
        private bool _isProgrammerMode = Properties.Settings.Default.IsProgrammerMode; 
        private bool _digitGrouping = Properties.Settings.Default.DigitGrouping;

        #region Properties Assignment
        public ObservableCollection<String> ExpressionList
        {
            get => _expressionList;
            set
            {
              _expressionList = value;
                OnPropertyChanged();
            }
        }

        public String Display
        {
            get => _display;
            set
            {
                if (_display != value)
                {
                    _display = value;
                    OnPropertyChanged();

                    OnPropertyChanged(nameof(DisplayHex));
                    OnPropertyChanged(nameof(DisplayDec));
                    OnPropertyChanged(nameof(DisplayOct));
                    OnPropertyChanged(nameof(DisplayBin));

                    UpdateDisplayFormat();
                }
            }
        }

       
        public bool HasMemoryValues
        {
            get => _hasMemoryValues;
            set
            {
                if (_hasMemoryValues != value)
                {
                    _hasMemoryValues = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsMemoryPanelVisible
        {
            get => _isMemoryPanelVisible;
            set
            {
                if (_isMemoryPanelVisible != value)
                {
                    _isMemoryPanelVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<MemoryItem> MemoryValues
        {
            get=> _memoryValues;
            set
            {
                _memoryValues = value;
                OnPropertyChanged();
            }
        }
        

        public String NumberBase
        {
            get => _numberBase;
            set
            {
                if (_numberBase != value)
                {
                    // Parse using OLD base before changing
                    string oldBase = _numberBase;
                    double numericValue = 0;

                    try
                    {
                        switch (oldBase)
                        {
                            case "HEX": numericValue = Convert.ToInt32(Display, 16); break;
                            case "DEC": numericValue = double.Parse(Display.Replace(",", "")); break;
                            case "OCT": numericValue = Convert.ToInt32(Display, 8); break;
                            case "BIN": numericValue = Convert.ToInt32(Display, 2); break;
                        }

                        // Update base
                        _numberBase = value;
                        OnPropertyChanged();
                        Properties.Settings.Default.NumberBase = value;
                        Properties.Settings.Default.Save();

                        // Format using NEW base
                        int intValue = (int)numericValue;
                        switch (value)
                        {
                            case "HEX": Display = Convert.ToString(intValue, 16).ToUpper(); break;
                            case "DEC": Display = DigitGrouping ? intValue.ToString("N0") : intValue.ToString(); break;
                            case "OCT": Display = Convert.ToString(intValue, 8); break;
                            case "BIN": Display = Convert.ToString(intValue, 2); break;
                        }
                        ExpressionList.Clear();
                        ExpressionList.Add(Display);
                    }
                    catch
                    {
                        // Handle parsing errors
                        _numberBase = value;
                        OnPropertyChanged();
                        Properties.Settings.Default.NumberBase = value;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        public bool IsStandardMode
        {
            get => _isStandardMode;
            set
            {
                if (_isStandardMode != value)
                {
                    _isStandardMode = value;
                    if (value) IsProgrammerMode = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentMode));

                    UpdateDisplayFormat();
                }
            }
        }
        public bool IsProgrammerMode
        {
            get => _isProgrammerMode;
            set
            {
                if (_isProgrammerMode != value)
                {
                    _isProgrammerMode = value;
                    OnPropertyChanged();
                    Properties.Settings.Default.IsProgrammerMode = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(CurrentMode));

                    UpdateDisplayFormat();
                }
            }
        }

        public bool DigitGrouping
        {
            get => _digitGrouping;
            set
            {
                if( _digitGrouping != value)
                {
                    _digitGrouping = value;
                    OnPropertyChanged();
                    Properties.Settings.Default.DigitGrouping = value;
                    Properties.Settings.Default.Save();

                    UpdateDisplayFormat();
                }
            }
        }

        public String CurrentMode => IsProgrammerMode ? "Programmer" : "Standard";

        public string DisplayHex => FormatNumberForBase(16);
        public string DisplayDec => FormatNumberForBase(10);
        public string DisplayOct => FormatNumberForBase(8);
        public string DisplayBin => FormatNumberForBase(2);
        #endregion

        #region Commands
        public ICommand NumberCommand { get; private set; }
        public ICommand UnaryOperatorCommand { get; private set; }
        public ICommand BinaryOperatorCommand { get; private set; }
        public ICommand OperationCommand { get; private set; }
        public ICommand EqualsCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand ClearEntryCommand { get; private set; }
        public ICommand BackspaceCommand { get; private set; }
        public ICommand DecimalCommand { get; private set; }
        public ICommand NegateCommand { get; private set; }
        public ICommand MemoryClearCommand { get; private set; }
        public ICommand MemoryRecallCommand { get; private set; }
        public ICommand MemoryAddCommand { get; private set; }
        public ICommand MemorySubtractCommand { get; private set; }
        public ICommand MemoryStoreCommand { get; private set; }
        public ICommand MemoryShowCommand { get; private set; }
        public ICommand MemoryClearItemCommand { get; private set; }
        public ICommand MemoryAddItemCommand { get; private set; }
        public ICommand MemorySubtractItemCommand { get; private set; }
        public ICommand ToggleDigitGroupingCommand { get; private set; }
        public ICommand SetModeCommand { get; private set; }
        public ICommand SetNumberBaseCommand { get; private set; }
        #endregion

        #region Helper Methods
        private bool TryParseCurrentValue(out double value)
        {
            value = 0;
            Debug.WriteLine($"Trying to parse: {Display} as {NumberBase}"); // Debugging

            // Exit early for error messages
            if (!IsValidNumber(Display))
            { Debug.WriteLine($"Invalid number format: '{Display}' for base {NumberBase}");
            return false; }

            // Try to parse based on current base
            try
            {
                switch (NumberBase)
                {
                    case "HEX":
                        value = Convert.ToInt32(Display, 16);
                        return true;
                    case "DEC":
                        string cleanDisplay = Display.Replace(_currentCulture.NumberFormat.NumberGroupSeparator, "");
                        return double.TryParse(cleanDisplay, NumberStyles.Any, _currentCulture, out value);
                    case "OCT":
                        value = Convert.ToInt32(Display, 8);
                        return true;
                    case "BIN":
                        value = Convert.ToInt32(Display, 2);
                        return true;
                    default:
                        return double.TryParse(Display, NumberStyles.Any, _currentCulture, out value);
                }
            }
            catch
            {
                return false;
            }
        }
        private bool IsValidNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            char groupSeparator = _currentCulture.NumberFormat.NumberGroupSeparator[0];
            char decimalSeparator = _currentCulture.NumberFormat.NumberDecimalSeparator[0];


            switch (NumberBase)
            {
                case "HEX":
                    return value.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
                case "DEC":
                    return value.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator);
                case "OCT":
                    return value.All(c => c >= '0' && c <= '7');
                case "BIN":
                    return value.All(c => c == '0' || c == '1');
                default:
                    return value.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator);
            }
        }

        private double ConvertToDecimal(string value)
        {
            switch (NumberBase)
            {
                case "HEX": return Convert.ToInt32(value, 16);
                case "DEC": return double.Parse(value.Replace(",", ""));
                case "OCT": return Convert.ToInt32(value, 8);
                case "BIN": return Convert.ToInt32(value, 2);
                default: return double.Parse(value);
            }
        }

        private string FormatNumberForBase(int baseValue)
        {
            if (!TryParseCurrentValue(out double value))
                return "Error";

            int intValue = (int)value;

            switch (baseValue)
            {
                case 16:
                    return Convert.ToString(intValue, 16).ToUpper();
                case 10:
                    return DigitGrouping ? intValue.ToString("N0", _currentCulture) : intValue.ToString();
                case 8:
                    return Convert.ToString(intValue, 8);
                case 2:
                    return Convert.ToString(intValue, 2);
                default:
                    return intValue.ToString();
            }
        }

        private string FormatNumberForDisplayBase(string result)
        {
            if (double.TryParse(result, out double value))
            {
                int intValue = (int)value;

                switch (NumberBase)
                {
                    case "HEX": return Convert.ToString(intValue, 16).ToUpper();
                    case "DEC": return DigitGrouping ? intValue.ToString("N0", _currentCulture) : intValue.ToString();
                    case "OCT": return Convert.ToString(intValue, 8);
                    case "BIN": return Convert.ToString(intValue, 2);
                    default: return intValue.ToString();
                }
            }
            return result;
        }

        private void UpdateDisplayFormat()
        {
            // Get the current culture’s decimal separator.
            string decimalSeparator = _currentCulture.NumberFormat.NumberDecimalSeparator;

            // Use the current Display as the raw input.
            // (If you need to preserve a more exact raw input, consider storing it separately.)
            string rawInput = Display;

            // Determine if the raw input contains a decimal separator and count decimals.
            int decimalCount = 0;
            int separatorIndex = rawInput.IndexOf(decimalSeparator);
            if (separatorIndex >= 0)
            {
                // Count the number of characters after the decimal separator.
                decimalCount = rawInput.Length - separatorIndex - decimalSeparator.Length;
            }

            // Try to parse the current value.
            if (!TryParseCurrentValue(out double value))
                return; // Abort update if parsing fails

            if (IsProgrammerMode)
            {
                // In Programmer mode, formatting is based on the selected base.
                switch (NumberBase)
                {
                    case "HEX":
                        Display = Convert.ToString((int)value, 16).ToUpper();
                        break;
                    case "DEC":
                        // For programmer mode DEC, just format as an integer.
                        Display = DigitGrouping ? ((int)value).ToString("N0", _currentCulture) : ((int)value).ToString();
                        break;
                    case "OCT":
                        Display = Convert.ToString((int)value, 8);
                        break;
                    case "BIN":
                        Display = Convert.ToString((int)value, 2);
                        break;
                }
            }
            else
            {
                // Standard mode: handle floating point values.
                if (DigitGrouping)
                {
                    // If the user has typed a decimal separator...
                    if (separatorIndex >= 0)
                    {
                        // If there are no digits after the decimal (e.g. "6."), preserve the trailing separator.
                        if (decimalCount == 0)
                        {
                            Display = ((int)value).ToString("N0", _currentCulture) + decimalSeparator;
                        }
                        else
                        {
                            // Build the format string dynamically based on the number of decimals.
                            string formatSpecifier = "N" + decimalCount.ToString();
                            Display = value.ToString(formatSpecifier, _currentCulture);
                        }
                    }
                    else
                    {
                        // No decimal separator in raw input.
                        if (value == Math.Floor(value))
                            Display = ((int)value).ToString("N0", _currentCulture);
                        else
                            // Default to "N" format if the user didn't type a decimal separator.
                            Display = value.ToString("N", _currentCulture);
                    }
                }
                else
                {
                    // Without digit grouping, just use the general format.
                    Display = value.ToString("G", _currentCulture);
                }
            }

            OnPropertyChanged(nameof(Display));

            // Update the ExpressionList's last element (if it's an operand) so it stays in sync.
            if (ExpressionList.Count % 2 == 1)
            {
                ExpressionList[ExpressionList.Count - 1] = Display;
            }
        }


        private void UpdateMemoryState()
        {
            // Update memory values collection
            MemoryValues.Clear();
            var values = _appModel.GetMemoryValues();
            for (int i = 0; i < values.Count; ++i)
            {
                MemoryValues.Add(new MemoryItem { Index = i, Value = values[i] });
            }

            // Update memory availability flag
            HasMemoryValues = _appModel.HasMemoryValues();
        }
        #endregion

        #region Commands' Methods


        private void HandleUnaryOperator(string operation)
        {
            char groupSeparator = _currentCulture.NumberFormat.NumberGroupSeparator[0];
            char decimalSeparator = _currentCulture.NumberFormat.NumberDecimalSeparator[0];

            String operand = ExpressionList.Count == 0 ? Display :
                            (ExpressionList.Count == 3 ? ExpressionList[2] : ExpressionList[0]);

            if (IsProgrammerMode)
            {
                double op = ConvertToDecimal(operand);
                String result = _appModel.CalculateUnary(op.ToString(), operation);

                if (result.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator||c==groupSeparator))
                {
                    result = FormatNumberForDisplayBase(result);
                }
                Display = result;
            }
            else
            {
                String result = _appModel.CalculateUnary(operand, operation);
                Display = result;
            }

            if (!Display.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator))
            {
                ExpressionList.Clear();
                return;
            }

            switch (ExpressionList.Count)
            {
                case 0:
                    ExpressionList.Add(Display);
                    break;
                case 1:
                    ExpressionList[0] = Display;
                    break;
                case 3:
                    ExpressionList[2] = Display;
                    break;
            }
        }

        private void HandleBinaryOperator(string operation)
        {
            char groupSeparator = _currentCulture.NumberFormat.NumberGroupSeparator[0];
            char decimalSeparator = _currentCulture.NumberFormat.NumberDecimalSeparator[0];

            switch (ExpressionList.Count)
            {
                case 0:
                    ExpressionList.Add(Display);
                    ExpressionList.Add(operation);
                    break;
                case 1:
                    ExpressionList.Add(operation);
                    break;
                case 2:
                    ExpressionList[1] = operation;
                    break;
                case 3:
                    if (IsProgrammerMode)
                    {
                        // Convert to decimal for calculation
                        double op1 = ConvertToDecimal(ExpressionList[0]);
                        double op2 = ConvertToDecimal(ExpressionList[2]);
                        String result = _appModel.CalculateBinary(op1.ToString(), ExpressionList[1], op2.ToString());

                        // Format result for display
                        if (result.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator))
                        {
                            ExpressionList.Clear();
                            result = FormatNumberForDisplayBase(result);
                            ExpressionList.Add(result);
                            ExpressionList.Add(operation);
                        }
                        else
                        {
                            ExpressionList.Clear();
                        }
                        Display = result;
                    }
                    else
                    {
                        ///
                        String result = _appModel.CalculateBinary(ExpressionList[0], ExpressionList[1], ExpressionList[2]);
                        ExpressionList.Clear();
                        if (result.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator))
                        {
                            ExpressionList.Add(result);
                            ExpressionList.Add(operation);
                        }
                        ////

                        Display = result;
                    }
                    break;
            }
        }
        private void HandleNumber(string number)
        {
            if ((NumberBase == "BIN" && number != "0" && number != "1") ||
        (NumberBase == "OCT" && int.Parse(number) > 7) ||
        (NumberBase == "DEC" && !char.IsDigit(number[0])) ||       
        (NumberBase == "HEX" && !((number[0] >= '0' && number[0] <= '9') ||
                                 (number[0] >= 'A' && number[0] <= 'F'))))
            {
                return;
            }
            /////
            if (ExpressionList.Count == 0 || ExpressionList.Count == 2)
            {

                ExpressionList.Add(number);
                Display = number;

            }
            else if (ExpressionList.Count == 1 || ExpressionList.Count == 3)
            {
                if (Display.Length > 0 && Display[0] == '0')
                {
                    if (!Display.Contains("."))
                    {
                        ExpressionList[ExpressionList.Count - 1] = number;
                        Display = number;
                    }
                    else
                    {
                        ExpressionList[ExpressionList.Count - 1] += number;
                        Display += number;
                    }
                }
                else
                {
                    ExpressionList[ExpressionList.Count - 1] += number;
                    Display += number;
                }
            }
            UpdateDisplayFormat();

        }

        private void HandleEquals()
        {
            char groupSeparator = _currentCulture.NumberFormat.NumberGroupSeparator[0];
            char decimalSeparator = _currentCulture.NumberFormat.NumberDecimalSeparator[0];

            if (ExpressionList.Count < 3)
            {
                String result = ExpressionList[0];
                Display = result;
                ExpressionList.Clear();
                ExpressionList.Add(result);
            }
            else
            {
                
                if (IsProgrammerMode)
                {
                    double op1 = ConvertToDecimal(ExpressionList[0]);
                    double op2 = ConvertToDecimal(ExpressionList[2]);
                    String result = _appModel.CalculateBinary(op1.ToString(), ExpressionList[1], op2.ToString());

                    if (result.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator))
                    {
                        ExpressionList.Clear();
                        result = FormatNumberForDisplayBase(result);
                        ExpressionList.Add(result);
                    }
                    else
                    {
                        ExpressionList.Clear();
                    }
                    Display = result;
                }
                else{
                    String result = _appModel.CalculateBinary(ExpressionList[0], ExpressionList[1], ExpressionList[2]);
                    Display = result;
                    ExpressionList.Clear();
                    if (result.All(c => char.IsDigit(c) || c == '-' || c == decimalSeparator || c == groupSeparator))
                    {
                        ExpressionList.Add(result);
                    }
                }
            }
        }

        private void HandleDecimal()
        {
            string decimalSeparator = _currentCulture.NumberFormat.NumberDecimalSeparator;
            // Check if there's already a decimal separator in the current operand
            if (ExpressionList.Count > 0 && !Display.Contains(decimalSeparator))
            {
                ExpressionList[ExpressionList.Count - 1] += decimalSeparator;
                Display += decimalSeparator;
            }
        }


        private void HandleBackspace()
        {
            if (Display.Length > 1)
            {

                if (Display == ExpressionList[ExpressionList.Count - 1])
                {
                    Display = Display.Substring(0, Display.Length - 1);
                    ExpressionList[ExpressionList.Count - 1] = Display;
                }
                else
                {
                    Display = Display.Substring(0, Display.Length - 1);
                }
            }
            else
            {
                if (Display == ExpressionList[ExpressionList.Count - 1])
                {
                    Display = "0";
                    ExpressionList[ExpressionList.Count - 1] = "0";
                }
                else
                {
                    Display = "0";
                }
            }
            
        }

        private void HandleNegation()
        {
            if (Display.Length > 0 && Display[0] != '-')
            {
                Display = "-" + Display;
                if ((ExpressionList.Count) % 2 != 0)
                {
                    ExpressionList[ExpressionList.Count - 1] = '-' + ExpressionList[ExpressionList.Count - 1];
                }
                else
                {
                    ExpressionList[ExpressionList.Count - 2] = '-' + ExpressionList[ExpressionList.Count - 2];
                }
            }
            else if (Display.Length > 0 && Display[0] == '-')
            {
                Display = Display.Substring(1);
                if ((ExpressionList.Count) % 2 != 0)
                {
                    ExpressionList[ExpressionList.Count - 1] = ExpressionList[ExpressionList.Count - 1].Substring(1);
                }
                else
                {
                    ExpressionList[ExpressionList.Count - 2] = ExpressionList[ExpressionList.Count - 2].Substring(1);
                }
                
            }
        }

        private void HandleClear()
        {
            ExpressionList.Clear();
            ExpressionList.Add("0");
            Display = "0";
        }

        private void HandleClearEntry()
        {
            Display = "0";
            if (ExpressionList.Count % 2 != 0)
                ExpressionList[ExpressionList.Count - 1] = "0";
        }

        private void HandleMemoryShow()
        {
            IsMemoryPanelVisible = !IsMemoryPanelVisible;
            UpdateMemoryState();
        }

        private void HandleMemoryStore()
        {
            _appModel.MemoryStore(Double.Parse(Display));
            UpdateMemoryState();

        }

        private void HandleMemoryAdd()
        {
            _appModel.MemoryAdd(Double.Parse(Display));
            UpdateMemoryState();

        }

        private void HandleMemorySubtract()
        {
            _appModel.MemorySubtract(Double.Parse(Display));
            UpdateMemoryState();

        }

        private void HandleMemoryRecall()
        {
            String memoryValue=(_appModel.MemoryRecall()).ToString();
            Display = memoryValue;
            if (ExpressionList.Count%2==1)
            {
                ExpressionList[ExpressionList.Count - 1] = memoryValue;
            }
            else
            {
                ExpressionList.Add(memoryValue);
            }
        }

        private void HandleMemoryClear()
        {
            _appModel.MemoryClear();
            UpdateMemoryState();

        }

        private void HandleMemoryItemClear(int index)
        {
            _appModel.MemoryClear(index);
            UpdateMemoryState();

        }
        private void HandleMemoryItemAdd(int index)
        {
            _appModel.MemoryItemAdd(index,Double.Parse(Display));
            UpdateMemoryState();
        }

        private void HandleMemoryItemSubtract(int index)
        {
            _appModel.MemoryItemSubtract(index, Double.Parse(Display));
            UpdateMemoryState();
        }



        private void ToggleDigitGrouping()
        {
            DigitGrouping = !DigitGrouping;
        }

        private void SetMode(string mode)
        {
            if (mode == "Standard")
            {
                IsStandardMode = true;
                IsProgrammerMode = false;
            }
            else if (mode == "Programmer")
            {
                IsStandardMode = false;
                IsProgrammerMode = true;
            }

            
        }

        private void HandleNumberBaseChange(string baseType)
        {
            NumberBase = baseType;
        }


        #endregion

        #region VM Constructor and Command Initialization
        public AppViewModel()
        {
            // Initialize commands
            NumberCommand = new RelayCommand(param => HandleNumber(param.ToString()));
            UnaryOperatorCommand = new RelayCommand(param => HandleUnaryOperator(param.ToString()));
            BinaryOperatorCommand = new RelayCommand(param => HandleBinaryOperator(param.ToString()));
            EqualsCommand = new RelayCommand(param => HandleEquals());
            DecimalCommand = new RelayCommand(param => HandleDecimal());
            BackspaceCommand = new RelayCommand(param => HandleBackspace());
            NegateCommand = new RelayCommand(param => HandleNegation());
            ClearCommand = new RelayCommand(param => HandleClear());
            ClearEntryCommand = new RelayCommand(param => HandleClearEntry());

            MemoryStoreCommand = new RelayCommand(param => HandleMemoryStore());
            MemoryAddCommand = new RelayCommand(param => HandleMemoryAdd(),param => HasMemoryValues);
            MemorySubtractCommand = new RelayCommand(param => HandleMemorySubtract(), param => HasMemoryValues);
            MemoryRecallCommand = new RelayCommand(param => HandleMemoryRecall(),param => HasMemoryValues);
            MemoryClearCommand = new RelayCommand(param => HandleMemoryClear(), param => HasMemoryValues);
            MemoryShowCommand = new RelayCommand(param => HandleMemoryShow(), param => HasMemoryValues);

            MemoryClearItemCommand = new RelayCommand(param => HandleMemoryItemClear(Convert.ToInt32(param)));
            MemoryAddItemCommand = new RelayCommand(param => HandleMemoryItemAdd(Convert.ToInt32(param)));
            MemorySubtractItemCommand = new RelayCommand(param => HandleMemoryItemSubtract(Convert.ToInt32(param)));


            ToggleDigitGroupingCommand = new RelayCommand(param => ToggleDigitGrouping());
            SetModeCommand = new RelayCommand(param => SetMode(param.ToString()));
            SetNumberBaseCommand = new RelayCommand(param => HandleNumberBaseChange(param.ToString()));

        }
        #endregion

        #region ViewModelBase (event handler)
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
