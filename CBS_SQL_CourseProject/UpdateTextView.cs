using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CBS_SQL_CourseProject
{
    public partial class UpdateTextView : Form
    {
        public UpdateTextView(int curPictureId)
        {
            InitializeComponent();
            _pictureId = curPictureId;
        }

        public void Initialize()
        {
            string text = GetPictureTextData();

            if (text == null)
            {
                this.Close();
            }
            else
            {
                textBox1.Text = text;
                LastSavedString = text;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            string textUpdate = textBox1.Text;
            string query = "UPDATE Emission SET [Text] = @textUpdate WHERE ID_Source = @pictureId";

            using (SqlCommand command = new SqlCommand(query, Program.s_connection))
            {
                command.Parameters.AddWithValue("@pictureId", _pictureId);
                command.Parameters.AddWithValue("@textUpdate", textUpdate);

                command.ExecuteNonQuery();
            }

            LastSavedString = textUpdate;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string GetPictureTextData()
        {
            string query = "SELECT [Text] FROM Emission WHERE ID_Source = @pictureId";

            using (SqlCommand command = new SqlCommand(query, Program.s_connection))
            {
                command.Parameters.AddWithValue("@pictureId", _pictureId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["Text"].ToString();
                    }
                }
            }
            
            return null;
        }

        private int _pictureId = 0;
        public string LastSavedString = "";
    }
}
