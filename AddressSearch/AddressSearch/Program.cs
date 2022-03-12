using Npgsql;
using phonet4n.Core;
using System;
using System.Xml;

namespace AddressSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Phonetizer phonet = new Phonetizer(true);

            using var reader = XmlReader.Create(@"C:\Users\michael.vodep\Downloads\gemplzstr\gemplzstr.xml");

            const string query = "COPY playground.streets(gemnr, gemnam38, okz, ortname, skz, stroffi, plznr, gemnr2, zustort, code) FROM STDIN (FORMAT BINARY)";

            var streetName = string.Empty;
            var xmlValue = string.Empty;

            using (var connection = new NpgsqlConnection($"Server=localhost;Database=playground;User Id=postgres;Password=postgres;"))
            {
                connection.Open();

                var streetWriter = connection.BeginBinaryImport(query);

                while (reader.ReadToFollowing("datensatz"))
                {
                    streetWriter.StartRow();                    

                    for (int i = 0; i < 9; i++)
                    {
                        reader.ReadToFollowing("element");
                        xmlValue = reader.ReadElementContentAsString();

                        streetWriter.Write(xmlValue, NpgsqlTypes.NpgsqlDbType.Text);

                        if(i == 5)
                        {
                            streetName = xmlValue;
                        }
                    }

                    streetWriter.Write(phonet.Phonetize(streetName));
                }

                streetWriter.Complete();
                streetWriter.Close();
            }
        }
    }
}
