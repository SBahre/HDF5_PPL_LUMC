using HDF.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeidenUniversityLibrary
{
    class AppendClass
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

        /*herr_t file_info(hid_t loc_id, string fName, void* opdata)
{
    H5G_stat_t statbuf;

    /*
     * Get type of the object and display its name and type.
     * The name of the object is passed to this function by 
     * the Library. Some magic :-)
     
    H5G.get_info(loc_id, fName, FALSE, &statbuf);
    switch (statbuf.type) {
    case H5G.storage_type_t.GROUP:
                    Console.WriteLine(" Object with name %s is a group \n", fName);
         break;
    case H5G_DATASET:
                    Console.WriteLine(" Object with name %s is a dataset \n", fName);
         break;
    case H5G.TYPE:
                    Console.WriteLine(" Object with name %s is a named datatype \n", fName);
         break;
    default:
                    Console.WriteLine(" Unable to identify an object ");
                    break;
    }
    return 0;
 }*/
    }
}
