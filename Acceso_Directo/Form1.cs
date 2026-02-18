using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acceso_Directo
{
    public partial class Form1 : Form
    {
        ManejadorHash manejador = new ManejadorHash();

        public Form1()
        {
            InitializeComponent();
            ConfigurarGrid();
            CargarGrid();
        }

        private void ConfigurarGrid()
        {
            dgvDatos.AllowUserToAddRows = false; // evita que el usuario intente agregar filas manualmente
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDatos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDatos.ReadOnly = true;
        }

        private void CargarGrid()
        {
            // Obtenemos la lista desde la lógica y la mostramos
            dgvDatos.DataSource = null;
            dgvDatos.DataSource = manejador.LeerTodo();

            // Formato visual: poner en rojo los espacios vacíos
            foreach (DataGridViewRow row in dgvDatos.Rows)
            {
                bool ocupado = (bool)row.Cells["Estado"].Value;
                if (!ocupado) row.DefaultCellStyle.ForeColor = Color.Gray;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                int id = int.Parse(txtId.Text);
                string nombre = txtNombre.Text;

                manejador.Escribir(id, nombre);
                MessageBox.Show($"Guardado en la posición Hash: {id % ManejadorHash.CAPACIDAD}");
                CargarGrid(); // Refrescar vista
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtId.Text)) return;

                int idBuscado = int.Parse(txtId.Text);
                int posInicial = idBuscado % ManejadorHash.CAPACIDAD;
                bool encontrado = false;

                dgvDatos.ClearSelection();

                // Buscamos a partir de la posición Hash (Prueba Lineal)
                for (int i = 0; i < ManejadorHash.CAPACIDAD; i++)
                {
                    int posActual = (posInicial + i) % ManejadorHash.CAPACIDAD;
                    var fila = dgvDatos.Rows[posActual];

                    int idEnFila = (int)fila.Cells["Id"].Value;
                    bool ocupado = (bool)fila.Cells["Estado"].Value;

                    if (ocupado && idEnFila == idBuscado)
                    {
                        fila.Selected = true;
                        dgvDatos.FirstDisplayedScrollingRowIndex = posActual;
                        MessageBox.Show($"Encontrado en posición física: {posActual} (Hash original: {posInicial})");
                        encontrado = true;
                        break;
                    }

                    // Si encontramos un espacio vacío, significa que el dato no existe 
                    // (porque si hubiera existido con colisión, se habría guardado aquí o después)
                    if (!ocupado) break;
                }

                if (!encontrado) MessageBox.Show("Dato no encontrado.");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void CargarDesdeCSV()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    Title = "Seleccionar Dataset en CSV"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    manejador.ReiniciarArchivo();

                    string[] lineas = File.ReadAllLines(ofd.FileName);
                    // IMPORTANTE: No usamos dgvDatos.Rows.Clear() aquí porque el Grid está enlazado.

                    // Procesamos cada línea del CSV
                    foreach (string linea in lineas.Skip(1)) // Saltamos el encabezado
                    {
                        if (string.IsNullOrWhiteSpace(linea)) continue;

                        string[] columnas = linea.Split(',');

                        if (columnas.Length >= 2)
                        {
                            int id = int.Parse(columnas[0].Trim());
                            string nombre = columnas[1].Trim();

                            // Usamos tu lógica de POO para escribir en el archivo físico
                            manejador.Escribir(id, nombre);
                        }
                    }

                    // Una vez procesado todo, llamamos a tu método existente para refrescar la vista
                    CargarGrid();

                    MessageBox.Show("Datos del CSV procesados y guardados en el archivo de acceso directo.", "Éxito");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar el CSV: " + ex.Message, "Error");
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Pedimos confirmación para evitar desastres
            DialogResult resultado = MessageBox.Show(
                "¿Estás segura de que deseas borrar TODOS los datos del archivo físico?",
                "Confirmar Limpieza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    manejador.ReiniciarArchivo();
                    CargarGrid(); // Refrescamos el DataGridView para que se vea vacío (gris)
                    MessageBox.Show("Archivo limpiado correctamente.", "Éxito");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al limpiar: " + ex.Message);
                }
            }
        }

        private void btnCargarCSV_Click_1(object sender, EventArgs e)
        {
            CargarDesdeCSV();
        }

        private void btnLimpiar_Click_1(object sender, EventArgs e)
        {
            // Pedimos confirmación para evitar desastres
            DialogResult resultado = MessageBox.Show(
                "¿Estás segura de que deseas borrar TODOS los datos del archivo físico?",
                "Confirmar Limpieza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    manejador.ReiniciarArchivo();
                    CargarGrid(); // Refrescamos el DataGridView para que se vea vacío (gris)
                    MessageBox.Show("Archivo limpiado correctamente.", "Éxito");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al limpiar: " + ex.Message);
                }
            }
        }
    }    
}
