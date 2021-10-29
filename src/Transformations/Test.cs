using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Library;

namespace PhdCode
{
    static class Program
    {
        static void Main()
        {
            //ValidationRef.Validate();
            ValidationBib.Validate();
        }
        
        private static void Doc2Md()
        {
            var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
            var srcPath = @"D:\code\Mikhail\phd\Sources";
            Scripts.Doc2Md(pandoc, srcPath);
        }
    }
}