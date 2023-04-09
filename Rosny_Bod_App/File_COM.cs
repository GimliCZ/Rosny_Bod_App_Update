using System;
using System.IO;
using System.Text;
using System.Windows;

namespace Rosny_Bod_App
{
    public sealed class File_COM : IDisposable
    {
        private StreamWriter write;
        private StreamReader read;
        public string current_path = Directory.GetCurrentDirectory();
        public FileStream file;
        public FileMode mode;
        public string path;

        public File_COM(string filename, string foldername, bool Append)
        {
            if (Append == true)
            {
                mode = FileMode.Append;
            }

            else
            {
                mode = FileMode.Truncate;
            }
            path = current_path + "\\" + foldername + "\\" + filename;
            file = new FileStream(path, mode, FileAccess.Write, FileShare.Delete);
            write = new StreamWriter(file, Encoding.GetEncoding(1252), 1024, true);
        }
        public File_COM(string filename, string foldername)
        {
            if (!Directory.Exists(current_path + "\\" + foldername + "\\"))
            {
                Directory.CreateDirectory(current_path + "\\" + foldername + "\\");
            }
                file = new FileStream(current_path + "\\" + foldername + "\\" + filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                read = new StreamReader(file, Encoding.GetEncoding(1252));

        }

        public string Read_Data() {
           
            string data =   read.ReadToEnd();
            read.Close();
            if (read != null)
                read.Dispose();
            return data;
        }

        public void Add_Data(string data) //Zapiš do souboru data, ( s možností přepisu )
        {
            if (!File.Exists(path)) {
                file = new FileStream(path, mode, FileAccess.Write, FileShare.Delete);
                write = new StreamWriter(file, Encoding.GetEncoding(1252), 1024, true);
            }
            if (mode == FileMode.Truncate)
            {
                file.Close();
                file = new FileStream(path, mode, FileAccess.Write, FileShare.Delete);
                write = new StreamWriter(file, Encoding.GetEncoding(1252), 1024, true);
            }

            try
            {
                write.Write(data);
                write.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void File_Delete() {


            Close();
            Dispose();
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Close()
        {
            write.Flush();
            write.Close();

        }

        public void Dispose()
        {
            if (write != null)
                write.Dispose();

        }
    }
}