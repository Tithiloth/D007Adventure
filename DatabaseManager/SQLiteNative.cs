using System;
using System.Runtime.InteropServices;

namespace D007Adventure.DatabaseManager
{
    internal static class SQLiteNative
    {
        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open(
            string filename,
            out IntPtr database);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close(
            IntPtr database);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_prepare_v2(
            IntPtr database,
            string sql,
            int sqlLength,
            out IntPtr statement,
            IntPtr tail);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_step(
            IntPtr statement);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_finalize(
            IntPtr statement);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_count(
            IntPtr statement);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_name(
            IntPtr statement,
            int column);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_type(
            IntPtr statement,
            int column);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_int(
            IntPtr statement,
            int column);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern double sqlite3_column_double(
            IntPtr statement,
            int column);

        [DllImport(
            "sqlite3.dll",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_text(
            IntPtr statement,
            int column);

            [DllImport(
            "sqlite3.dll",
          CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr sqlite3_errmsg(
            IntPtr database);
    }
}