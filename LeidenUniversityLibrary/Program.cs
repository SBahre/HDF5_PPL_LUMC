using System;
using Hdf5DotNetTools;
using System.Runtime.InteropServices;
using HDF.PInvoke;

namespace LeidenUniversityLibrary
{
    public class testStruct
    {
        public int[] TimeOffset { get; set; }
        public float[] Value { get; set; }
        public int[] Value2 { get; set; }
        public string[] name { get; set; }
    }
    
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
    
    class Program
    {
        static void Main(string[] args)
        {
            string fileName= @"D:\Shubham\HDF5\HDFinOneLine_1434.H5";
            var fileID = Hdf5.OpenFile(fileName);
            var groupId = H5G.open(fileID,"A/B");
            Array dset2 = Hdf5.ReadDataset<Int32>(groupId, "Sequence 2",100000,200000);

            Console.WriteLine("Program Finished...");
            Console.ReadKey();
        }
    }
}
