﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDF.PInvoke;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace Hdf5DotNetTools
{
#if HDF5_VER1_10
    using hid_t = System.Int64;
#else
    using hid_t = System.Int32;
#endif
    public static partial class Hdf5
    {
        // information: https://www.hdfgroup.org/ftp/HDF5/examples/examples-by-api/hdf5-examples/1_8/C/H5T/h5ex_t_cmpd.c
        //or: https://www.hdfgroup.org/HDF5/doc/UG/HDF5_Users_Guide-Responsive%20HTML5/index.html#t=HDF5_Users_Guide%2FDatatypes%2FHDF5_Datatypes.htm%3Frhtocid%3Dtoc6.5%23TOC_6_8_Complex_Combinationsbc-22

        public static int WriteCompounds<T>(hid_t groupId, string name, IEnumerable<T> list) //where T : struct
        {
            Type type = typeof(T);
            var size = Marshal.SizeOf(type);
            var cnt = list.Count();

            var typeId = create_type(type);

            var log10 = (int)Math.Log10(cnt);
            ulong pow = (ulong)Math.Pow(10, log10);
            ulong c_s = Math.Min(1000, pow);
            ulong[] chunk_size = new ulong[] { c_s };

            ulong[] dims = new ulong[] { (ulong)cnt };

            long dcpl = 0;
            if (list.Count() == 0 || log10 == 0) { }
            else
            {
                dcpl = create_property(chunk_size);
            }

            // Create dataspace.  Setting maximum size to NULL sets the maximum
            // size to be the current size.
            var spaceId = H5S.create_simple(dims.Length, dims, null);

            // Create the dataset and write the compound data to it.
            var datasetId = H5D.create(groupId, name, typeId, spaceId, H5P.DEFAULT, dcpl);

            IntPtr p = Marshal.AllocHGlobal(size * (int)dims[0]);

            var ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            foreach (var strct in list)
                writer.Write(getBytes(strct));
            var bytes = ms.ToArray();

            GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var statusId = H5D.write(datasetId, typeId, spaceId, H5S.ALL, 
                H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();
            /*
             * Close and release resources.
             */
            H5D.close(datasetId);
            H5S.close(spaceId);
            H5T.close(typeId);
            H5P.close(dcpl);
            Marshal.FreeHGlobal(p);
            return statusId;
        }

        private static long create_property(ulong[] chunk_size)
        {
            var dcpl = H5P.create(H5P.DATASET_CREATE);
            H5P.set_layout(dcpl, H5D.layout_t.CHUNKED);
            H5P.set_chunk(dcpl, chunk_size.Length, chunk_size);
            H5P.set_deflate(dcpl, 6);
            return dcpl;
        }

        private static long create_type(Type t)
        {
            var size = Marshal.SizeOf(t);
            var float_size = Marshal.SizeOf(typeof(float));
            var int_size = Marshal.SizeOf(typeof(int));
            var typeId = H5T.create(H5T.class_t.COMPOUND, new IntPtr(size));

            var compoundInfo = Hdf5.GetCompoundInfo(t);
            foreach (var cmp in compoundInfo)
            {
                //Console.WriteLine(string.Format("{0}  {1}", cmp.name, cmp.datatype));
                H5T.insert(typeId, cmp.name, Marshal.OffsetOf(t, cmp.name), cmp.datatype);
            }
            return typeId;
        }
        private static IEnumerable<T> changeStrings<T>(IEnumerable<T> array, FieldInfo[] fields) where T : struct
        {
            foreach (var info in fields)
                if (info.FieldType == typeof(string))
                {
                    var attr = info.GetCustomAttributes(typeof(MarshalAsAttribute), false);
                    MarshalAsAttribute maa = (MarshalAsAttribute)attr[0];
                    object value = info.GetValue(array);
                }
            return array;
        }


        ///
        private static int calcCompoundSize(Type type, bool useIEEE, ref hid_t id)
        {
            // Create the compound datatype for the file.  Because the standard
            // types we are using for the file may have different sizes than
            // the corresponding native types
            var compoundInfo = Hdf5.GetCompoundInfo(type, useIEEE);
            var curCompound = compoundInfo.Last();
            var compoundSize = curCompound.offset + curCompound.size;
            //Create the compound datatype for memory.
            id = H5T.create(H5T.class_t.COMPOUND, new IntPtr(compoundSize));
            foreach (var cmp in compoundInfo)
                H5T.insert(id, cmp.name, new IntPtr(cmp.offset), cmp.datatype);
            return compoundSize;
        }

        public static IEnumerable<OffsetInfo> GetCompoundInfo(Type type, bool ieee = false)
        {
            //Type t = typeof(T);
            var strtype = H5T.copy(H5T.C_S1);
            int strsize = (int)H5T.get_size(strtype);
            int curSize = 0;
            List<OffsetInfo> offsets = new List<OffsetInfo>();
            foreach (var x in type.GetFields())
            {
                OffsetInfo oi = new OffsetInfo()
                {
                    name = x.Name,
                    type = x.FieldType,
                    datatype = ieee ? GetDatatypeIEEE(x.FieldType) : GetDatatype(x.FieldType),
                    size = x.FieldType == typeof(string) ? stringLength(x) : Marshal.SizeOf(x.FieldType),
                    offset = 0 + curSize
                };
                if (oi.datatype == H5T.C_S1)
                {
                    strtype = H5T.copy(H5T.C_S1);
                    H5T.set_size(strtype, new IntPtr(oi.size));
                    oi.datatype = strtype;
                }
                if (oi.datatype == H5T.STD_I64BE)
                    oi.size = oi.size * 2;
                curSize = curSize + oi.size;

                offsets.Add(oi);
            }
            H5T.close(strtype);
            return offsets;

        }

        private static int stringLength(FieldInfo fld)
        {
            var attr = fld.GetCustomAttributes(typeof(MarshalAsAttribute), false);
            MarshalAsAttribute maa = (MarshalAsAttribute)attr[0];
            var constSize = maa.SizeConst;
            return constSize;
        }

        public static IEnumerable<T> ReadCompounds<T>(hid_t groupId, string name) where T : struct
        {
            Type type = typeof(T);
            hid_t typeId = 0;
            // open dataset
            var datasetId = H5D.open(groupId, name);

            typeId = create_type(type);
            var compoundSize = Marshal.SizeOf(type);

            /*
             * Get dataspace and allocate memory for read buffer.
             */
            var spaceId = H5D.get_space(datasetId);
            int rank = H5S.get_simple_extent_ndims(spaceId);
            ulong[] dims = new ulong[rank];
            var ndims = H5S.get_simple_extent_dims(spaceId, dims, null);
            int rows = Convert.ToInt32(dims[0]);

            byte[] bytes = new byte[rows * compoundSize];
            // Read the data.
            GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            H5D.read(datasetId, typeId, spaceId, H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            int counter = 0;
            IEnumerable<T> strcts = Enumerable.Range(1, rows).Select(i =>
             {
                 byte[] select = new byte[compoundSize];
                 Array.Copy(bytes, counter, select, 0, compoundSize);
                 T s = fromBytes<T>(select);
                 counter = counter + compoundSize;
                 return s;
             });

            return strcts;
        }

    }
}
