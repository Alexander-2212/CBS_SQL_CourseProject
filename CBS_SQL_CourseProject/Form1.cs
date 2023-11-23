using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Collections;

namespace CBS_SQL_CourseProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int currentPictureId = 0;

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Images| *.png;*.jpg";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var path = openFileDialog.FileName;

                FileStream imgStream = File.OpenRead(path);
                byte[] blob = new byte[imgStream.Length];
                imgStream.Read(blob, 0, (int)imgStream.Length);
                var date = File.GetLastWriteTime(path);

                MemoryStream ms = new MemoryStream(blob, 0, blob.Length);
                ms.Write(blob, 0, blob.Length);
                pictureBox1.Image = Image.FromStream(ms, true);

                string query = "INSERT INTO Source (Name, Address, ImageData) VALUES (@name, @address, @imageData); SELECT SCOPE_IDENTITY();";

                int returnId = 0;

                using (SqlCommand command = new SqlCommand(query, Program.con))
                {
                    command.Parameters.AddWithValue("@name", path);
                    command.Parameters.AddWithValue("@address", path);
                    command.Parameters.AddWithValue("@imageData", blob);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        returnId = Convert.ToInt32(result);
                        currentPictureId = returnId;
                    }

                }

                query = "INSERT INTO Emission (ID_Source, Count, Width, Height, Text, Date) VALUES (@id, @count, @width, @height, @text, @date)";

                using (SqlCommand command = new SqlCommand(query, Program.con))
                {
                    command.Parameters.AddWithValue("@id", returnId);
                    command.Parameters.AddWithValue("@count", 1);
                    command.Parameters.AddWithValue("@width", pictureBox1.Image.Width);
                    command.Parameters.AddWithValue("@height", pictureBox1.Image.Height);
                    command.Parameters.AddWithValue("@text", "топ фотка");
                    command.Parameters.AddWithValue("@date", date);

                    command.ExecuteNonQuery();
                }


            }

        }

        private void button4_Click(object sender, EventArgs e)
        {

            string query = "DELETE FROM Emission WHERE ID_Source = @id";

            using (SqlCommand command = new SqlCommand(query, Program.con))
            {
                command.Parameters.AddWithValue("@id", currentPictureId);

                command.ExecuteNonQuery();
            }


            query = "DELETE FROM Source WHERE ID_Source = @id";

            using (SqlCommand command = new SqlCommand(query, Program.con))
            {
                command.Parameters.AddWithValue("@id", currentPictureId);

                command.ExecuteNonQuery();
            }

            currentPictureId = 0;

            pictureBox1.Image = null;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string query = "SELECT TOP 1 * FROM Source WHERE ID_Source > @currentSourceId ORDER BY ID_Source ASC;";

            using (SqlCommand command = new SqlCommand(query, Program.con))
            {
                command.Parameters.AddWithValue("@currentSourceId", currentPictureId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentPictureId = reader.GetInt32(reader.GetOrdinal("ID_Source"));

                        if (!reader.IsDBNull(reader.GetOrdinal("ImageData")))
                        {
                            byte[] buffer = new byte[reader.GetBytes(reader.GetOrdinal("ImageData"), 0, null, 0, int.MaxValue)];
                            reader.GetBytes(reader.GetOrdinal("ImageData"), 0, buffer, 0, buffer.Length);

                            MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, buffer.Length);
                            pictureBox1.Image = Image.FromStream(ms, true);
                        }
                    }
                }
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string query = "SELECT TOP 1 * FROM Source WHERE ID_Source < @currentSourceId ORDER BY ID_Source DESC;";

            using (SqlCommand command = new SqlCommand(query, Program.con))
            {
                command.Parameters.AddWithValue("@currentSourceId", currentPictureId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentPictureId = reader.GetInt32(reader.GetOrdinal("ID_Source"));

                        if (!reader.IsDBNull(reader.GetOrdinal("ImageData")))
                        {
                            byte[] buffer = new byte[reader.GetBytes(reader.GetOrdinal("ImageData"), 0, null, 0, int.MaxValue)];
                            reader.GetBytes(reader.GetOrdinal("ImageData"), 0, buffer, 0, buffer.Length);

                            MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, buffer.Length);
                            pictureBox1.Image = Image.FromStream(ms, true);
                        }
                    }
                }
            }
        }
    }
    
}
