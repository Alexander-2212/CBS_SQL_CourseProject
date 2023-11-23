﻿using System;
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
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            UpdateCounter();
        }

        public int currentPictureId = 0;
        public int currentPictureNumber = 0;
        private DateTime lastChangedDate;

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void UpdateDateLabel(DateTime date)
        {
            this.dateLabel.Text = date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void UpdateWidthLabel(int width)
        {
            this.widthLabel.Text = $"Width: {width}";
        }

        private void UpdateHeightLabel(int height)
        {
            this.heightLabel.Text = $"Height: {height}";
        }

        private void UpdateTextLabel(string text)
        {
            this.textLabel.Text = $"Text: {text}";
        }

        private void UpdateData(bool isPrevious = true)
        {
            string nextQuery = "SELECT TOP 1 S.ID_Source, S.ImageData, E.[Date], E.Width, E.Height, E.Text " +
                           "FROM Source S INNER JOIN Emission E ON S.ID_Source = E.ID_Source " +
                           "WHERE S.ID_Source > @currentSourceId ORDER BY S.ID_Source ASC;";

            string prevQuery = "SELECT TOP 1 S.ID_Source, S.ImageData, E.[Date], E.Width, E.Height, E.Text " +
               "FROM Source S INNER JOIN Emission E ON S.ID_Source = E.ID_Source " +
               "WHERE S.ID_Source < @currentSourceId ORDER BY S.ID_Source DESC;";

            string query = isPrevious ? prevQuery : nextQuery;

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

                        if (!reader.IsDBNull(reader.GetOrdinal("Date")))
                        {
                            lastChangedDate = reader.GetDateTime(reader.GetOrdinal("Date"));
                            UpdateDateLabel(lastChangedDate);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Width")))
                        {
                            int width = reader.GetInt32(reader.GetOrdinal("Width"));
                            UpdateWidthLabel(width);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Height")))
                        {
                            int height = reader.GetInt32(reader.GetOrdinal("Height"));
                            UpdateHeightLabel(height);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Text")))
                        {
                            string text = reader.GetString(reader.GetOrdinal("Text"));
                            UpdateTextLabel(text);
                        }

                        if (isPrevious)
                        {
                            currentPictureNumber--;
                        }
                        else
                        {
                            currentPictureNumber++;
                        }
                    }
                }
            }
        }

        private void insertButton_Click(object sender, EventArgs e)
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

                lastChangedDate = File.GetLastWriteTime(path);
                UpdateDateLabel(lastChangedDate);

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
                        currentPictureNumber++;
                    }


                }

                query = "INSERT INTO Emission (ID_Source, Count, Width, Height, Text, Date) VALUES (@id, @count, @width, @height, @text, @date)";

                using (SqlCommand command = new SqlCommand(query, Program.con))
                {
                    command.Parameters.AddWithValue("@id", returnId);
                    command.Parameters.AddWithValue("@count", 1);
                    command.Parameters.AddWithValue("@width", pictureBox1.Image.Width);
                    command.Parameters.AddWithValue("@height", pictureBox1.Image.Height);
                    command.Parameters.AddWithValue("@text", "Great Picture");
                    command.Parameters.AddWithValue("@date", date);

                    command.ExecuteNonQuery();
                }

                UpdateWidthLabel(pictureBox1.Image.Width);
                UpdateHeightLabel(pictureBox1.Image.Height);
                UpdateTextLabel("Great Picture");
                UpdateDateLabel(date);


            }

            UpdateCounter();
        }

        private void deleteButton_Click(object sender, EventArgs e)
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

            //currentPictureId = 0;

            //pictureBox1.Image = null;

            if (GetTotalPictureCount() > 0 && currentPictureNumber != 1)
            {
                UpdateData();
            }
            else if (GetTotalPictureCount() > 0 && currentPictureNumber == 1)
            {
                UpdateData(false);
                currentPictureNumber--;
            }
            else
            {
                ClearControls();
            }


            UpdateCounter();
        }

        private void ClearControls()
        {
            currentPictureId = 0;
            currentPictureNumber = 0;
            pictureBox1.Image = null;
            this.dateLabel.Text = "Date";
            this.textLabel.Text = "Text:";
            this.widthLabel.Text = "Width:";
            this.heightLabel.Text = "Height:";
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            UpdateData(false);

            UpdateCounter();
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            UpdateData();

            UpdateCounter();
        }

        private void UpdateCounter()
        {
            int totalCount = GetTotalPictureCount();
            this.counterLabel.Text = $"{currentPictureNumber} OF {totalCount}";
        }

        private int GetTotalPictureCount()
        {
            string query = "SELECT COUNT(*) FROM Source";
            using (SqlCommand command = new SqlCommand(query, Program.con))
            {
                return (int)command.ExecuteScalar();
            }
        }



    }

}
