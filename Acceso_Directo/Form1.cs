using System;
using System.Drawing;
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
            dgvDatos.AllowUserToAddRows = false;
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDatos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDatos.ReadOnly = true;
        }

        private void CargarGrid()
        {
            dgvDatos.DataSource = null;
            dgvDatos.DataSource = manejador.LeerTodo();

            foreach (DataGridViewRow row in dgvDatos.Rows)
            {
                if (row.DataBoundItem is Producto p && !p.Estado)
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                else
                    row.DefaultCellStyle.ForeColor = Color.Black;
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                manejador.Escribir(int.Parse(txtId.Text), txtNombre.Text);
                CargarGrid();
                MessageBox.Show("Registro agregado correctamente.");
            }
            catch (Exception ex) { MessageBox.Show("Error al agregar: " + ex.Message); }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // En acceso directo, actualizar es simplemente sobreescribir la posición
                manejador.Escribir(int.Parse(txtId.Text), txtNombre.Text);
                CargarGrid();
                MessageBox.Show("Cambios guardados en el archivo.");
            }
            catch (Exception ex) { MessageBox.Show("Error al guardar: " + ex.Message); }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                int id = int.Parse(txtId.Text);
                manejador.Eliminar(id);
                CargarGrid();
                MessageBox.Show("Registro eliminado (Eliminación Lógica). El espacio ahora está libre.");
            }
            catch (Exception ex) { MessageBox.Show("Error al eliminar: " + ex.Message); }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                int idBuscado = int.Parse(txtId.Text);

                // ¡Magia de la POO! Le pedimos al manejador que resuelva todo.
                int posEncontrada = manejador.ResolverColision(idBuscado, false);

                if (posEncontrada != -1)
                {
                    dgvDatos.ClearSelection();
                    dgvDatos.Rows[posEncontrada].Selected = true;
                    dgvDatos.FirstDisplayedScrollingRowIndex = posEncontrada; // Hace scroll a la fila
                    MessageBox.Show($"Encontrado en la posición física: {posEncontrada}");
                }
                else
                {
                    MessageBox.Show("No se encontró el registro.");
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al buscar: " + ex.Message); }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Borrar todos los datos físicos?", "Aviso", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                manejador.ReiniciarArchivo();
                CargarGrid();
            }
        }
    }
}