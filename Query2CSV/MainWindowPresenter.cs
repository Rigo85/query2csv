using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace Query2CSV
{
    class MainWindowPresenter
    {
        private readonly MainWindow mainWindow;
        private readonly string connectionString;
        private List<string> files;

        public MainWindowPresenter(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            var configuration = JToken.Parse(File.ReadAllText(
                Path.Combine(Directory.GetCurrentDirectory(), "configuration.json"),
                Encoding.UTF8));

            connectionString = configuration["connectionstring"].Value<string>();

            CreateEvents();
        }

        private void CreateEvents()
        {
            mainWindow.btLoad.Click += LoadOnClick;
            mainWindow.btExecute.Click += ExecuteOnClick;
            mainWindow.bwExecuter.DoWork += BwExecuterOnDoWork;
            mainWindow.bwExecuter.RunWorkerCompleted += BwExecuterOnRunWorkerCompleted;
            mainWindow.bwExecuter.ProgressChanged += BwExecuterOnProgressChanged;
            mainWindow.acercaDeToolStripMenuItem.Click += AboutOnClick;
            mainWindow.cerrarToolStripMenuItem.Click += CloseOnClick;
        }

        private void CloseOnClick(object sender, EventArgs e)
        {
            mainWindow.Close();
        }

        private void AboutOnClick(object sender, EventArgs e)
        {
            var year = DateTime.Now.ToString("yyyy");
            MessageBox.Show(
                "Aplicación para la generación de cartas.\n"
                            + "Versión 0.1.\n\nAutor: Rigoberto L. Salgado Reyes.\n"
                            + $"Correo: rigoberto.salgado@rjabogados.com.\n\n© {year} RJAbogados.\n"
                            + "Todos los derechos reservados.",
                "Acerca de",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BwExecuterOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void BwExecuterOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var msg = $"Ocurrió un error.\n\nDescripción:\n{e.Error.Message}";
                MessageBox.Show(msg, "Error");
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("La ejecución fue cancelada.", "Advertencia");
            }
            else
            {
                MessageBox.Show("CSV generado correctamente.", "Información");
            }

            mainWindow.btExecute.Enabled = true;
        }

        private void BwExecuterOnDoWork(object sender, DoWorkEventArgs e)
        {
            new Executer(files, connectionString);
        }

        private void ExecuteOnClick(object sender, EventArgs e)
        {
            try
            {
                mainWindow.btExecute.Enabled = false;
                mainWindow.bwExecuter.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Ha ocurrido el siguiente error:\n{ex.Message}";
                MessageBox.Show(msg, "Error");
            }
        }

        private void LoadOnClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "Consultas (*.sql)|*.sql|Todos los archivos (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                files = new List<string>(openFileDialog.FileNames);
                mainWindow.lCount.Text = Convert.ToString(files.Count);
            }
        }
    }
}
