using System;
using System.Collections.Generic;
using System.IO;

public class ManejadorHash
{
    private string ruta = "datos.dat";
    private const int TAM_REGISTRO = 60;
    // Aumentamos la capacidad para archivos más grandes
    public const int CAPACIDAD = 100;

    public ManejadorHash()
    {
        if (!File.Exists(ruta)) ReiniciarArchivo();
    }

    private int FuncionHash(int id) => id % CAPACIDAD;

    public void Escribir(int id, string nombre)
    {
        using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.ReadWrite))
        using (var bw = new BinaryWriter(fs))
        using (var br = new BinaryReader(fs))
        {
            int posInicial = FuncionHash(id);
            int posActual = posInicial;
            bool guardado = false;

            // --- MANEJO DE COLISIONES (Prueba Lineal) ---
            for (int i = 0; i < CAPACIDAD; i++)
            {
                posActual = (posInicial + i) % CAPACIDAD;
                fs.Seek(posActual * TAM_REGISTRO, SeekOrigin.Begin);

                // Leemos el encabezado para ver si está libre o es el mismo ID
                int idExistente = br.ReadInt32();
                fs.Seek(-4, SeekOrigin.Current); // Regresamos el puntero para sobreescribir si es necesario

                // Leemos el booleano de estado (está al final del registro en tu estructura anterior)
                // Nota: Para ser más exactos, leeremos el booleano que está después del string
                fs.Seek(4 + 52, SeekOrigin.Current); // Saltamos ID (4) + String (52 aprox)
                bool ocupado = br.ReadBoolean();
                fs.Seek(-(4 + 52 + 1), SeekOrigin.Current); // Volvemos al inicio del registro

                if (!ocupado || idExistente == id)
                {
                    bw.Write(id);
                    bw.Write(nombre.PadRight(50).Substring(0, 50));
                    bw.Write(true); // Estado: Ocupado
                    guardado = true;
                    break;
                }
            }

            if (!guardado) throw new Exception("¡El archivo está lleno! No se pueden manejar más colisiones.");
        }
    }

    public List<Producto> LeerTodo()
    {
        var lista = new List<Producto>();
        if (!File.Exists(ruta)) return lista;

        using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
            // Solo leemos lo que el archivo realmente contenga físicamente
            long numRegistrosEnDisco = fs.Length / TAM_REGISTRO;

            for (int i = 0; i < numRegistrosEnDisco; i++)
            {
                // Verificamos que queden suficientes bytes para leer un registro completo
                if (fs.Position + TAM_REGISTRO > fs.Length) break;

                fs.Seek(i * TAM_REGISTRO, SeekOrigin.Begin);

                int id = br.ReadInt32();
                string nombre = br.ReadString().Trim();
                bool estado = br.ReadBoolean();

                lista.Add(new Producto { Id = id, Nombre = nombre, Estado = estado });
            }
        }
        return lista;
    }

    public void ReiniciarArchivo()
    {
        try
        {
            // FileMode.Create borra el archivo anterior y crea uno nuevo de 0 bytes
            using (var fs = new FileStream(ruta, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                // 1. Forzamos al Sistema Operativo a reservar el espacio total de inmediato
                // Esto evita fragmentación y asegura que el archivo tenga el tamaño correcto
                fs.SetLength(CAPACIDAD * TAM_REGISTRO);

                // 2. Llenamos el archivo con registros "vacíos"
                for (int i = 0; i < CAPACIDAD; i++)
                {
                    // Posicionamos el puntero al inicio de cada registro por seguridad
                    fs.Seek(i * TAM_REGISTRO, SeekOrigin.Begin);

                    // Escribimos valores por defecto que nuestra lógica reconozca como "libre"
                    bw.Write(0);                                     // ID = 0

                    // Rellenamos el string con espacios para ocupar los 50 caracteres exactos
                    // El método BinaryWriter.Write(string) añade un byte de longitud al inicio
                    string relleno = "".PadRight(50);
                    bw.Write(relleno);                               // Nombre vacío (51-52 bytes reales)

                    bw.Write(false);                                 // Estado = false (Desocupado)
                }
            }
        }
        catch (IOException ex)
        {
            throw new Exception("No se pudo reiniciar el archivo porque está siendo usado: " + ex.Message);
        }
    }
}