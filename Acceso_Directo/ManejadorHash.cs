using System;
using System.Collections.Generic;
using System.IO;

public class ManejadorHash
{
    private string ruta = "datos.dat";
    private const int TAM_REGISTRO = 60; // 4 (ID) + 51 (String) + 1 (Bool) + 4 padding
    public const int CAPACIDAD = 100;

    public ManejadorHash()
    {
        if (!File.Exists(ruta)) ReiniciarArchivo();
    }

    private int FuncionHash(int id) => id % CAPACIDAD;

    // --- NUEVO MÉTODO CENTRALIZADO DE COLISIONES ---
    // Devuelve la posición física (0 a 99) o -1 si no hay lugar/no existe
    public int ResolverColision(int id, bool paraInsertar)
    {
        using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
            int posInicial = FuncionHash(id);

            for (int i = 0; i < CAPACIDAD; i++)
            {
                // Fórmula de Prueba Lineal
                int posActual = (posInicial + i) % CAPACIDAD;
                long offset = posActual * TAM_REGISTRO;

                // Leemos el estado
                fs.Seek(offset + 55, SeekOrigin.Begin);
                bool ocupado = br.ReadBoolean();

                // Leemos el ID
                fs.Seek(offset, SeekOrigin.Begin);
                int idLeido = br.ReadInt32();

                if (paraInsertar)
                {
                    // Si queremos insertar, nos sirve un espacio vacío o el mismo ID (para sobrescribir)
                    if (!ocupado || idLeido == id) return posActual;
                }
                else
                {
                    // Si queremos buscar o eliminar, DEBE estar ocupado y ser el ID correcto
                    if (ocupado && idLeido == id) return posActual;
                }
            }
            return -1; // -1 significa que el archivo está lleno o el dato no se encontró
        }
    }

    public void Escribir(int id, string nombre)
    {
        // Usamos nuestro nuevo método para encontrar dónde guardar
        int posFisica = ResolverColision(id, true);
        if (posFisica == -1) throw new Exception("El archivo está lleno. No hay espacio para colisiones.");

        using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Write))
        using (var bw = new BinaryWriter(fs))
        {
            long offset = posFisica * TAM_REGISTRO;
            fs.Seek(offset, SeekOrigin.Begin);
            bw.Write(id);
            bw.Write(nombre.PadRight(50).Substring(0, 50));
            bw.Write(true); // Estado: Ocupado
        }
    }

    public void Eliminar(int id)
    {
        // Usamos nuestro nuevo método para encontrar dónde está el dato
        int posFisica = ResolverColision(id, false);
        if (posFisica == -1) throw new Exception("El ID no existe o ya fue eliminado.");

        using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Write))
        using (var bw = new BinaryWriter(fs))
        {
            long offset = posFisica * TAM_REGISTRO;
            // Saltamos directo al byte del estado y lo apagamos (Eliminación lógica)
            fs.Seek(offset + 55, SeekOrigin.Begin);
            bw.Write(false);
        }
    }

    public List<Producto> LeerTodo()
    {
        var lista = new List<Producto>();
        using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
            for (int i = 0; i < CAPACIDAD; i++)
            {
                if (fs.Position + TAM_REGISTRO > fs.Length) break;
                int id = br.ReadInt32();
                string nombre = br.ReadString().Trim();
                bool estado = br.ReadBoolean();
                lista.Add(new Producto { Id = id, Nombre = nombre, Estado = estado });

                fs.Seek(4, SeekOrigin.Current); // Padding
            }
        }
        return lista;
    }

    public void ReiniciarArchivo()
    {
        using (var fs = new FileStream(ruta, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            fs.SetLength(CAPACIDAD * TAM_REGISTRO);
            for (int i = 0; i < CAPACIDAD; i++)
            {
                bw.Write(0);
                bw.Write("".PadRight(50).Substring(0, 50));
                bw.Write(false);
                bw.Write(0);
            }
        }
    }
}