using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IDPSMasker
{
    class Program
    {
        static string sold1 = null;
        static string snew1 = null;
        static string sold2 = null;
        static string snew2 = null;
        static byte[] old1 = new byte[0x10];
        static byte[] old2 = new byte[0x10];
        static byte[] new1 = new byte[0x10];
        static byte[] new2 = new byte[0x10];
        static byte[] mask = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };


        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (args.Length == 1)
            {
                Console.WriteLine("              IDPSMasker v1.0.0.4 by pink1");
                Console.WriteLine();
                get_type(args[0]);
            }
            else
            {

                Console.WriteLine("IDPSMasker v1.0.0.4 by pink1");
                Console.WriteLine();
                Console.WriteLine("Drag dump into IDPSMasker.exe to mask IDPS");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        public static string ByteArrayToHexString(byte[] ByteArray)
        {
            string HexString = "";
            for (int i = 0; i < ByteArray.Length; ++i)
                HexString += ByteArray[i].ToString("X2"); // +" ";
            return HexString;
        }

        public static string InsertStringAtInterval(string source, string insert, int interval)
        {
            StringBuilder result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + interval < source.Length)
            {
                result.Append(source.Substring(currentPosition, interval)).Append(insert);
                currentPosition += interval;
            }
            if (currentPosition < source.Length)
            {
                result.Append(source.Substring(currentPosition));
            }
            return result.ToString();
        }

        private static void get_type(string dump)
        {
            string type = "";
            Stream fin = new FileStream(dump, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader brfin = new BinaryReader(fin);
            if (fin.Length == 0x1000000) { type = "nor"; }
            else if (fin.Length == 0x10000000) { type = "nand"; }
            else if (fin.Length == 0xEF00000) { type = "emmc"; }

            File.Copy(dump, "Masked_" + dump, true);
            
            fin.Close();
            masknor("Masked_" + dump, type);
        }

        private static void masknor(string fileout, string type)
        {
            int add1 = 0;
            int add2 = 0;

            if (type == "nor")
            {
                add1 = 0x0002F070;
                add2 = 0x000303D0;
            }
            else if (type == "nand")
            {
                add1 = 0x00080870;
                add2 = 0x00081BD0;
            }
            else if (type == "emmc")
            {
                add1 = 0x00040870;
                add2 = 0x00041BD0;
            }

            if (File.Exists("Mask"))
            {
                using (BinaryReader reader = new BinaryReader(new FileStream("mask", FileMode.Open)))
                {
                    reader.Read(mask, 0, 0x06);
                    string smask = ByteArrayToHexString(mask);
                    smask = InsertStringAtInterval(smask, " ", 2);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("     Using Custom Mask: " + smask);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    reader.Close();
                }
            }

            using (BinaryReader reader = new BinaryReader(new FileStream(fileout, FileMode.Open)))
            {
                reader.BaseStream.Seek(add1, SeekOrigin.Begin);
                reader.Read(old1, 0, 0x10);

                reader.BaseStream.Seek(add2, SeekOrigin.Begin);
                reader.Read(old2, 0, 0x10);
                reader.Close();
            }
            
            using (System.IO.BinaryWriter fileWriter = new System.IO.BinaryWriter(System.IO.File.Open(fileout, System.IO.FileMode.Open)))
            {
                fileWriter.BaseStream.Position = add1 + 0x0A; // set the offset
                fileWriter.Write(mask);

                fileWriter.BaseStream.Position = add2 + 0x0A; // set the offset
                fileWriter.Write(mask);
                fileWriter.Close();
            }

            using (BinaryReader reader = new BinaryReader(new FileStream(fileout, FileMode.Open)))
            {
                reader.BaseStream.Seek(add1, SeekOrigin.Begin);
                reader.Read(new1, 0, 0x10);

                reader.BaseStream.Seek(add2, SeekOrigin.Begin);
                reader.Read(new2, 0, 0x10);
                reader.Close();
            }
            bool areEqual1 = old1.SequenceEqual(new1);
            bool areEqual2 = old2.SequenceEqual(new2);
            if (areEqual1 == true && areEqual2 == true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("    IDPS of this dump was previously masked!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            sold1 = ByteArrayToHexString(old1);
            snew1 = ByteArrayToHexString(new1);
            sold2 = ByteArrayToHexString(old2);
            snew2 = ByteArrayToHexString(new2);
            sold1 = InsertStringAtInterval(sold1, " ", 2);
            snew1 = InsertStringAtInterval(snew1, " ", 2);
            sold2 = InsertStringAtInterval(sold2, " ", 2);
            snew2 = InsertStringAtInterval(snew2, " ", 2);
                
            Console.WriteLine("          Original IDPS @ 0x000" + add1.ToString("x2").ToUpper() + ": ");
            Console.WriteLine(sold1);
            Console.WriteLine("          Masked IDPS   @ 0x000" + add1.ToString("x2").ToUpper() + ": ");
            Console.WriteLine(snew1);
            Console.WriteLine();
            Console.WriteLine("          Original IDPS @ 0x000" + add2.ToString("x2").ToUpper() + ": ");
            Console.WriteLine(sold2);
            Console.WriteLine("          Masked IDPS   @ 0x000" + add2.ToString("x2").ToUpper() + ": ");
            Console.WriteLine(snew2);
            Console.WriteLine();
            Console.WriteLine("             Press any key to exit");
            Console.ReadKey();

        }
    }
}

