using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorMAP
{
    internal class AppModel
    {
        private List<double> valueList=new List<double>();
       

        public String CalculateBinary(String operand1, String operation, String operand2)
        {
            double number1=Double.Parse(operand1);
            double number2=Double.Parse(operand2);
            switch (operation)
            {
                case "+":
                    return (number1+number2).ToString();
                case "-":
                    return (number1 - number2).ToString();
                case "/":
                    if (number2==0)
                    {
                        return "Error! Can't divide by 0!";
                    }
                    return (number1 / number2).ToString();
                case "*":
                    return (number1 * number2).ToString();
                case "%":
                    return (number1%number2).ToString();
                default:
                    throw new Exception("Unknown operation");
            }
        }

        public String CalculateUnary(String operand, String operation)
        {
            double number=Double.Parse(operand);
            switch (operation)
            {
                case "1/x":
                    if (number == 0)
                    {
                        return "Error! Can't divide by 0!";
                    }
                    return (1/number).ToString();
                case "x^2":
                    return (number * number).ToString();
                case "sqrt":
                    return (Math.Sqrt(number)).ToString();
                default:
                    throw new Exception("Unknown operation");

            }
        }

        public void MemoryStore(double value)
        {
            valueList.Add(value);

        }
        public void MemoryAdd(double value)
        {
            valueList[valueList.Count - 1] += value;
        }

        public void MemorySubtract(double value)
        {
            valueList[valueList.Count - 1] -= value;
        }

        public double MemoryRecall()
        {
            return valueList.Last();
        }

        public void MemoryClear(int index = -1)
        {
            if (index >= 0 && index < valueList.Count)
                valueList.RemoveAt(index);
            else
                valueList.Clear();
        }

        public bool HasMemoryValues()
        {
            return valueList.Count > 0;
        }

        public List<double> GetMemoryValues()
        {
            return valueList;
        }

        public void MemoryItemAdd(int index, double value)
        {
            valueList[index] += value;
        }

        public void MemoryItemSubtract(int index, double value)
        {
            valueList[index] -= value;

        }
    }
}
