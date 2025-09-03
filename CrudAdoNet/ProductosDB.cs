using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CrudAdoNet
{
    public class ProductosDB
    {
        /*
        private string connectionString
            = "Data Source=localhost\SQLEXPRESS01;Initial Catalog=CrudWindowsForms;" +
            "User ID=MÁXIMO;Password=Null";
        */
        private readonly string connectionString 
            =   @"Data Source=.\SQLEXPRESS01;Initial Catalog=CrudWindowsForms;
                Integrated Security=True;TrustServerCertificate=True;";


        public bool conexion()
        {
            try
            {
                SqlConnection conexion = new SqlConnection(connectionString);
                conexion.Open();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public List<Productos> ObtenerLista()
        {
            List<Productos> productos = new List<Productos>();

            string query = "select id,nombre,precio from productos";

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                SqlCommand comando = new SqlCommand(query, conexion);

                try
                {
                    conexion.Open();
                    SqlDataReader reader = comando.ExecuteReader();

                    while (reader.Read())
                    {
                        Productos producto = new Productos();
                        producto.Id = reader.GetInt32(0);
                        producto.Nombre = reader.GetString(1);
                        producto.Precio = reader.GetInt32(2);
                        productos.Add(producto);
                    }
                    reader.Close();

                    conexion.Close();
                }
                catch(Exception ex) 
                {
                    throw new Exception("Hay un error en la bd" +ex.Message);
                }
            }

            return productos;
        }
    }

    public class Productos 
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Precio { get; set; }
    }
}
