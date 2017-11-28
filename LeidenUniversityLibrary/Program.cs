using System;
using Hdf5DotNetTools;
using System.Runtime.InteropServices;
using HDF.PInvoke;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace LeidenUniversityLibrary
{
    
    class Program
    {
        public static double[,] createDataset(int offset = 0)
        {
            var dset = new double[10, 5];
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 5; j++)
                {
                    double x = i + j * 5 + offset;
                    dset[i, j] = (j == 0) ? x : x / 10;
                }
            return dset;
        }
        static void Main(string[] args)
        {

            //-------------------Reading a CSV file---------------------------
            #region
            // Load a CSV file into an array of rows and columns.
            // Assume there may be blank lines but every line has
            // the same number of fields.
            string filenameX = @"D:\Shubham\ECG data\EXPORTED ECG DATA\Andres_M.csv";

            // Get the file's text.
            string whole_file = System.IO.File.ReadAllText(filenameX);

            // Split into lines.
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            // Allocate the data array.
            string[,] values = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }
            #endregion
            //-------------------CSV FILE CODE ends---------------------------


            // create a list of matrices
            var dsets = new List<double[,]> {
            createDataset(),
            createDataset(10),
            createDataset(20) };

            string folder = @"D:\Shubham\HDF5";
            string filename = Path.Combine(folder, "testChunks03.H5");
            long fileId = Hdf5.CreateFile(filename);
            Console.WriteLine("Do you want to compress the file? (Y/N)");
            string response = "N";
            response = Console.ReadLine();

            // create a dataset and append two more datasets to it
            using (var chunkedDset = new ChunkedDataset<double>("/test", fileId, dsets.First()))
            {
                foreach (var ds in dsets.Skip(1))
                    chunkedDset.AppendDataset(ds);
            }

            List<Int32> ROW = new List<Int32>();
            for (int r = 1; r < num_rows; r++)
                ROW.Add(Int32.Parse(values[r, 1]));
            Hdf5.WriteDataset(fileId, "Sequence 1", ROW.ToArray());

            ulong[] ChunkSZ = { 10000};
            ChunkedDataset<Int32> CD = new ChunkedDataset<Int32>("CD object",fileId,ChunkSZ);
            CD.FirstDataset(ROW.ToArray());
            ROW.Clear();

            for (int r = 1; r < num_rows; r++)
                ROW.Add(Int32.Parse(values[r, 2]));
            CD.AppendDataset(ROW.ToArray());
            ROW.Clear();
            

            Hdf5.CloseFile(fileId);

            Console.WriteLine("Program Finished...");
            Console.ReadKey();
        }
    }
}
