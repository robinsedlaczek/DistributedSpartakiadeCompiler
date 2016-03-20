using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library1;
using Library2;

namespace TestSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(StringAmender.Amend(StringGenerator.Create()));
        }
    }
}
