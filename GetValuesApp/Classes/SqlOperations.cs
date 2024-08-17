﻿using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using DbPeekQueryLibrary.LanguageExtensions;
using Microsoft.Data.SqlClient;

namespace GetValuesApp.Classes;
internal class SqlOperations
{
    private static string ConnectionString =>
        """
        Data Source=.\SQLEXPRESS;Initial Catalog=NorthWind2022;
        Integrated Security=True;
        Encrypt=False
        """;
    public static void GetCustomers()
    {
        using SqlConnection cn = new(ConnectionString);
        using SqlCommand cmd = new() { Connection = cn, CommandText = SqlStatements.ReadCustomers() };

        cn.Open();
        var reader = cmd.ExecuteReader();
        int fieldCount = reader.FieldCount;
        object[] resultRow = new object[fieldCount];
        int customerId = 0;
        while (reader.Read())
        {
            reader.GetSqlValues(resultRow);
            customerId = ((SqlInt32)resultRow[0]).Value;
            AnsiConsole.MarkupLine(customerId.IsOdd()
                ? $"[white]{customerId,-4}[/][white on red]{string.Join(",", resultRow[1..fieldCount])}[/]"
                : $"[white]{customerId,-4}[/]{string.Join(",", resultRow[1..fieldCount])}");
        }
    }

    //public static async Task WriteToFileAndConsoleForConnection()
    //{
    //    await using SqlConnection cn = new(ConnectionString);
    //    await cn.OpenAsync();
    //    //await cn.WriteToFile(SqlStatements.ReadCustomers(), "Customers.txt");
    //    await cn.WriteTo(SqlStatements.ReadCustomers());
    //    //await cn.WriteToConsole(SqlStatements.ReadCustomers());
    //}

    public static async Task WriteToFileAndConsoleForCommand(int contactTypeIdentifier)
    {
        await using SqlConnection cn = new(ConnectionString);
        await using SqlCommand cmd = new()
        {
            Connection = cn, 
            CommandText = SqlStatements.ReadCustomersOwners()
        };

        cmd.Parameters.Add("@ContactTypeIdentifier", SqlDbType.Int).Value = contactTypeIdentifier;

        await cn.OpenAsync();
        await cmd.Write();
    }

    public static void DataAdapterPeekParameters(int customerId)
    {
        var statement =
            """
            SELECT     CU.CompanyName,
                       CO.FirstName,
                       CO.LastName
             FROM      dbo.Customers AS CU
            INNER JOIN dbo.Contacts AS CO
               ON CU.ContactId = CO.ContactId
            WHERE      (CU.CustomerIdentifier = @CustomerIdentifier);
            """;
        using SqlConnection cn = new(ConnectionString);
        using SqlDataAdapter da = new(statement, cn);
        da.SelectCommand.Parameters.AddWithValue(
            "@CustomerIdentifier", customerId);


        var actualCommandText = da.SelectCommand.ActualCommandText();

        DataSet ds = new();
        da.Fill(ds, "Customers");
        DataTable dt = ds.Tables["Customers"];

    }


}