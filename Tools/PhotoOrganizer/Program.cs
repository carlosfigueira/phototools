using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhotoOrganizer
{
    class Program
    {
        //http://msdn.microsoft.com/en-us/library/aa916453.aspx
        enum PropertyTagType : short
        {
            Byte = 1,
            ASCII = 2,
            Short = 3,
            Long = 4,
            Rational = 5,
            Undefined = 7,
            SLONG = 9,
            SRational = 10,
        }

        const int PropertyTagDateTime = 0x0132;
        const int PropertyTagDateTimeOriginal = 0x9003;
        const int PropertyTagDateTimeDigitized = 0x9004;

        const int PropertyTagTypeByte = 1;
        const int PropertyTagTypeASCII = 2;
        const int PropertyTagTypeShort = 3;
        const int PropertyTagTypeLong = 4;
        const int PropertyTagTypeRational = 5;
        const int PropertyTagTypeUndefined = 7;
        const int PropertyTagTypeSLONG = 9;
        const int PropertyTagTypeSRational = 10;

        public const int PropertyTagGpsLatitudeRef = 0x0001;
        public const int PropertyTagGpsLatitude = 0x0002;
        public const int PropertyTagGpsLongitudeRef = 0x0003;
        public const int PropertyTagGpsLongitude = 0x0004;

        readonly static DateTime invalidDateTime = DateTime.MinValue;
        static bool debug = false;
        static bool noCopying = false;

        static TimeSpan hourAdjustment = TimeSpan.FromHours(7);

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: {0} <from folder> <to folder> [<problem files folder>] [debug] [noCopying]", Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase));
                return;
            }
            string problemFilesFolder;
            if (args.Length < 3)
            {
                problemFilesFolder = Path.Combine(args[1], "ProblemFiles");
            }
            else
            {
                problemFilesFolder = args[2];
            }
            if (!Directory.Exists(problemFilesFolder))
            {
                Directory.CreateDirectory(problemFilesFolder);
            }
            if (args.Length > 3)
            {
                for (int i = 3; i < args.Length; i++)
                {
                    if (args[i].Equals("debug", StringComparison.OrdinalIgnoreCase))
                    {
                        debug = true;
                    }
                    else if (args[i].Equals("noCopying", StringComparison.OrdinalIgnoreCase))
                    {
                        noCopying = true;
                    }
                }
            }

            StreamWriter swLogGoodFiles = File.CreateText("GoodFiles.log");
            StreamWriter swLogBadFiles = File.CreateText("BadFiles.log");
            swLogGoodFiles.AutoFlush = true;
            swLogBadFiles.AutoFlush = true;
            string[] fromFiles = Directory.GetFiles(args[0], "*.jpg", SearchOption.AllDirectories);
            Console.WriteLine("fromFiles.Length: " + fromFiles.Length);
            swLogGoodFiles.WriteLine("Searching {0} files", fromFiles.Length);
            foreach (string file in fromFiles)
            {
                DateTime datePictureTaken;
                string newFileName = GetNewFileName(file, out datePictureTaken);
                if (newFileName == null)
                {
                    swLogBadFiles.WriteLine("Cannot find date time for file {0}", file);
                    continue;
                }
                String yearAndMonth = String.Format("{0:0000}{1:00}", datePictureTaken.Year, datePictureTaken.Month);
                string toDirectory = Path.Combine(args[1], yearAndMonth);
                if (!Directory.Exists(toDirectory))
                {
                    Directory.CreateDirectory(toDirectory);
                }
                string newFullFileName = Path.Combine(toDirectory, newFileName);
                if (File.Exists(newFullFileName))
                {
                    DealWithProblemFile(toDirectory, problemFilesFolder, newFullFileName, file, swLogGoodFiles, swLogBadFiles);
                }
                else
                {
                    if (!noCopying)
                    {
                        File.Copy(file, newFullFileName);
                    }

                    swLogGoodFiles.WriteLine("File {0} --> {1}", file, newFullFileName);
                }
            }
            Console.WriteLine("Finished.");
        }

        static string GetNewFileName(string originalFileName, out DateTime datePictureTaken)
        {
            string dateTime = null;
            datePictureTaken = invalidDateTime;
            using (Bitmap bitmap = (Bitmap)Bitmap.FromFile(originalFileName))
            {
                PropertyItem[] properties = bitmap.PropertyItems;
                foreach (PropertyItem prop in properties)
                {
                    if (prop.Id == PropertyTagDateTimeOriginal)
                    {
                        Debug.Assert(prop.Type == PropertyTagTypeASCII);
                        Debug.Assert(prop.Value.Length == 20);
                        dateTime = Encoding.ASCII.GetString(prop.Value, 0, 19);
                    }
                }
                if (dateTime == null)
                {
                    foreach (PropertyItem prop in properties)
                    {
                        if (prop.Id == PropertyTagDateTimeDigitized)
                        {
                            Debug.Assert(prop.Type == PropertyTagTypeASCII);
                            Debug.Assert(prop.Value.Length == 20);
                            dateTime = Encoding.ASCII.GetString(prop.Value, 0, 19);
                        }
                    }
                }
                if (dateTime == null)
                {
                    foreach (PropertyItem prop in properties)
                    {
                        if (prop.Id == PropertyTagDateTime)
                        {
                            Debug.Assert(prop.Type == PropertyTagTypeASCII);
                            Debug.Assert(prop.Value.Length == 20);
                            dateTime = Encoding.ASCII.GetString(prop.Value, 0, 19);
                        }
                    }
                }
            }
            if (dateTime == null)
            {
                Console.WriteLine("Cannot find date time for file {0}", originalFileName);
                return null;
            }
            else
            {
                datePictureTaken = new DateTime(
                    int.Parse(dateTime.Substring(0, 4)),
                    int.Parse(dateTime.Substring(5, 2)),
                    int.Parse(dateTime.Substring(8, 2)),
                    int.Parse(dateTime.Substring(11, 2)),
                    int.Parse(dateTime.Substring(14, 2)),
                    int.Parse(dateTime.Substring(17, 2)));
                if (debug)
                {
                    datePictureTaken = datePictureTaken.Add(hourAdjustment);
                    dateTime = String.Format("{0:0000}:{1:00}:{2:00} {3:00}:{4:00}:{5:00}",
                        datePictureTaken.Year, datePictureTaken.Month, datePictureTaken.Day,
                        datePictureTaken.Hour, datePictureTaken.Minute, datePictureTaken.Second);
                }
                Console.WriteLine("{0} - {1}", originalFileName, dateTime);
                dateTime = dateTime.Replace(' ', '-').Replace(':', '-');
                StringBuilder sb = new StringBuilder();
                sb.Append("PICT_");
                sb.Append(dateTime);
                sb.Append(Path.GetExtension(originalFileName));
                string newFileName = sb.ToString();
                return newFileName;
            }
        }

        static int BytesToInt(byte[] value, int offset, int count)
        {
            int result = 0;
            int multiplier = 1;
            for (int i = 0; i < count; i++)
            {
                result += multiplier * (int)value[offset + i];
                multiplier = multiplier << 8;
            }
            return result;
        }

        static void PrintLatLong(byte[] propValue)
        {
            Debug.Assert(propValue.Length == 24);
            int degrees = BytesToInt(propValue, 0, 4);
            double dblMinutes = (double)BytesToInt(propValue, 8, 4) / (double)BytesToInt(propValue, 12, 4);
            double seconds = (double)BytesToInt(propValue, 16, 4) / (double)BytesToInt(propValue, 20, 4);
            int minutes = (int)Math.Floor(dblMinutes);
            double onlySeconds = (dblMinutes - minutes) * 60 + seconds;
            Console.Write(" - {0}m{1}'{2:.00}\" - ", degrees, minutes, onlySeconds);
            double latLong = degrees + dblMinutes / 60 + seconds / 3600;
            Console.WriteLine("{0:.00000}", latLong);
        }

        static void PrintFileProperties(string fileName)
        {
            Console.WriteLine(Path.GetFileName(fileName));
            using (Bitmap bitmap = (Bitmap)Bitmap.FromFile(fileName))
            {
                PropertyItem[] properties = bitmap.PropertyItems;
                foreach (PropertyItem prop in properties)
                {
                    PropertyTagType type = (PropertyTagType)prop.Type;
                    Console.WriteLine("   {0} - 0x{1:X4} - {2}", type, prop.Id, prop.Len);
                    if (prop.Id == Program.PropertyTagGpsLatitude)
                    {
                        Console.Write("      Lat: ");
                        PrintLatLong(prop.Value);
                    }
                    else if (prop.Id == Program.PropertyTagGpsLongitude)
                    {
                        Console.Write("      Lng: ");
                        PrintLatLong(prop.Value);
                    }
                    else if (prop.Id == Program.PropertyTagGpsLongitudeRef)
                    {
                        Console.WriteLine("      LngRef: {0}", (char)prop.Value[0]);
                    }
                    else if (prop.Id == Program.PropertyTagGpsLatitudeRef)
                    {
                        Console.WriteLine("      LatRef: {0}", (char)prop.Value[0]);
                    }
                }
            }
        }

        static void PrintDateTimeProperties(string fileName)
        {
            Console.WriteLine(Path.GetFileName(fileName));
            using (Bitmap bitmap = (Bitmap)Bitmap.FromFile(fileName))
            {
                PropertyItem[] properties = bitmap.PropertyItems;
                foreach (PropertyItem prop in properties)
                {
                    if (prop.Type == PropertyTagTypeASCII && prop.Value != null && prop.Value.Length == 20 && prop.Value[19] == 0)
                    {
                        string dateTime = Encoding.ASCII.GetString(prop.Value, 0, 19);
                        Console.WriteLine("  - Property 0x{0:X4}: {1}", prop.Id, dateTime);
                    }
                }
            }
        }

        static bool IsSameFile(string fileName1, string fileName2, out int lenFile1, out int lenFile2)
        {
            using (FileStream fs1 = File.OpenRead(fileName1))
            {
                using (FileStream fs2 = File.OpenRead(fileName2))
                {
                    lenFile1 = (int)fs1.Length;
                    lenFile2 = (int)fs2.Length;
                    if (lenFile1 != lenFile2)
                    {
                        return false;
                    }
                    byte[] buffer1 = new byte[1000];
                    byte[] buffer2 = new byte[1000];
                    int bytesRead1, bytesRead2;
                    do
                    {
                        bytesRead1 = fs1.Read(buffer1, 0, buffer1.Length);
                        bytesRead2 = fs2.Read(buffer2, 0, buffer2.Length);
                        if (bytesRead1 != bytesRead2)
                        {
                            return false;
                        }
                        for (int i = 0; i < bytesRead2; i++)
                        {
                            if (buffer1[i] != buffer2[i])
                            {
                                return false;
                            }
                        }
                    } while (bytesRead1 > 0);
                }
            }
            return true;
        }
        static string GetUniqueFileName(string baseFileName)
        {
            string fileName = Path.GetFileNameWithoutExtension(baseFileName);
            fileName += "_" + Guid.NewGuid().ToString("D");
            fileName += Path.GetExtension(baseFileName);
            return fileName;
        }

        static void DealWithProblemFile(string toDirectoryPath, string problemFilesPath,
            string existingFileName, string newFileName, StreamWriter swGood, StreamWriter swBad)
        {
            int lenOldFile, lenNewFile;
            if (!IsSameFile(existingFileName, newFileName, out lenOldFile, out lenNewFile))
            {
                swBad.WriteLine("File {0} already exists ({1}), but it's a different file; copying the larger one", newFileName, existingFileName);
                string problemFileName = Path.Combine(problemFilesPath, GetUniqueFileName(existingFileName));
                if (lenNewFile > lenOldFile)
                {
                    if (!noCopying)
                    {
                        File.Copy(existingFileName, problemFileName);
                        File.Copy(newFileName, existingFileName, true);
                    }

                    swBad.WriteLine("Replacing existing file {0} with file {1}", existingFileName, newFileName);
                }
                else
                {
                    if (!noCopying)
                    {
                        File.Copy(newFileName, problemFileName);
                    }

                    swBad.WriteLine("New file ({0}) is smaller than old one ({1}), keeping old one", newFileName, existingFileName);
                }
            }
            else
            {
                swGood.WriteLine("Skipping repeated file {0}", newFileName);
            }
        }

        static void MainPrintFileProperties()
        {
            foreach (string file in Directory.GetFiles(@"c:\temp", "*.jpg"))
            {
                PrintFileProperties(file);
            }
        }
    }
}
