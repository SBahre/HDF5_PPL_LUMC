using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hdf5DotNetTools;
using System.IO;
using System.Runtime.InteropServices;
using HDF.PInvoke;
using System.Diagnostics;

namespace LeidenUniversityLibrary
{
    public class TestClassWithArray
    {
        public double[] TestDoubles { get; set; }
        public string[] TestStrings { get; set; }
        public int TestInteger { get; set; }
        public double TestDouble { get; set; }
        public bool TestBoolean { get; set; }
        public string TestString { get; set; }
    }

    
    public class testStruct
    {
        public int[] TimeOffset { get; set; }
        public float[] Value { get; set; }
        public int[] Value2 { get; set; }
        public string[] name { get; set; }
    }
    //class not working with writeCompounds
    [StructLayout(LayoutKind.Sequential)]
    public struct wData2
    {
        public int serial_no;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string location;
        public double temperature;
        public double pressure;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string label;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct opdata
    {
        UInt16 recurs;         /* Recursion level.  0=root */
                                 /* Pointer to previous opdata */
        string addr;            /* Group address */
    };
    class Program
    {
        static void Main(string[] args)
        {

            Hdf5AcquisitionFile header= new Hdf5AcquisitionFile();
            header.Patient.Gender = "Male";
            header.Patient.BirthDate = new DateTime(1969, 1, 12);
            header.Patient.Id = "8475805";
            header.Recording.NrOfChannels = 5;
            header.Recording.SampleRate = 200;

            for (int i = 0; i < header.Recording.NrOfChannels; i++)
            {
                var chn = header.Channels[i];
                chn.Label = $"DC{(i + 1):D2}";
                chn.Dimension = "V";
                chn.Offset = 0;
                chn.Amplification = (double)(10 - -10) / (short.MaxValue - short.MinValue);
                chn.SamplingRate = header.Recording.SampleRate;
                header.Channels[i] = chn;
            }
            Console.WriteLine("Program Finished...");
            Console.ReadKey();
        }
    }
}
