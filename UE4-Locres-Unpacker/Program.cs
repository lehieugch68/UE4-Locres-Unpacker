using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UE4_Locres_Unpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UE4 Locres Unpacker by LeHieu (VietHoaGame), based on Text tool by swuforce";
            if (args.Length > 0)
            {
                foreach (string input in args)
                {
                    string ext = Path.GetExtension(input).ToLower();
                    string name = Path.GetFileName(input);
                    switch (ext)
                    {
                        case ".locres":
                            Locres.Unpack(input);
                            break;
                        case ".txt":
                            Locres.Repack(input);
                            break;
                        default:
                            Console.WriteLine($"Skip: {name}. Reason: The file format is not supported.");
                            Console.ReadKey();
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Please drag and drop Locres/Txt files into this tool to unpack/repack.");
                Console.ReadKey();
            }
        }
    }
}
