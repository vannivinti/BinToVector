using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;



namespace BinToCVector
{
    public partial class BinToCVector : Form
    {
        public BinToCVector()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            //Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                label1.Text = openFileDialog1.FileName;
            }

        }



        // Wite to a file a byte array, using multiple lines if necessary. 
        private void WriteMultiLineByteArray(byte[] bytes, string name, FileStream fsDestination)
        {
            const int rowSize = 20;
            const string underLine = "/*--------------------------------*/";
            int iter;
            string stringLine;
         

            AddText(fsDestination, name + " = {\n");

            for (iter = 0; iter < bytes.Length - rowSize; iter += rowSize)
            {
                AddText(fsDestination, "0x");
                stringLine = BitConverter.ToString(bytes, iter, rowSize);
                stringLine = stringLine.Replace("-", ", 0x");

                AddText(fsDestination, stringLine);
                AddText(fsDestination, ",\n");
            }

            AddText(fsDestination, "0x");
            stringLine = BitConverter.ToString(bytes, iter);
            stringLine = stringLine.Replace("-", ", 0x");

            AddText(fsDestination, stringLine);     
            AddText(fsDestination, "};\n\n\n" + underLine);
        }



        private void button2_Click(object sender, EventArgs e)
        {
            int numBytesRead = 0;
           

            // open the binary source file
            try
            {

                FileStream fsSource = new FileStream(label1.Text, FileMode.Open, FileAccess.Read);

                // Read the source file into a byte array. 
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
          

                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead. 
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached. 
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                fsSource.Close();

                int i;
                int byteOffset = 0;
               
                for (i = 0; i<numericUpDown1.Value; i++)
                {
                    byte[] data = new byte[numBytesRead / (int)numericUpDown1.Value];

                    // open the text destination file
                    FileStream fsDestination = new FileStream("BinaryVect" + i.ToString() + ".c", FileMode.Create, FileAccess.Write);
                    for(int j=0; j<(numBytesRead/(int)numericUpDown1.Value); j++)
                    {
                        data[j] = bytes[byteOffset++];
                    }

                    WriteMultiLineByteArray(data, "#include <rtems.h>\n#include <bsp.h>\n\n\n\nuint8_t vector_" + i.ToString() + "[]", fsDestination);
                    fsDestination.Close();
                }

                if( (numBytesRead % (int)numericUpDown1.Value) > 0)
                {
                    byte[] data = new byte[numBytesRead % (int)numericUpDown1.Value];

                    FileStream fsDestination = new FileStream("BinaryVect" + i.ToString() + ".c", FileMode.Create, FileAccess.Write);
                    for (int j = 0; j < (numBytesRead % (int)numericUpDown1.Value); j++)
                    {
                        data[j] = bytes[byteOffset++];
                    }

                    WriteMultiLineByteArray(data, "#include <rtems.h>\n#include <bsp.h>\n\n\n\nuint8_t vector_" + i.ToString() + "[]", fsDestination);
                    fsDestination.Close();
                }


                MessageBox.Show("DONE!");

            }
            catch (FileNotFoundException ioEx)
            {
                MessageBox.Show(ioEx.Message);
            }

        }



        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

    }
}
