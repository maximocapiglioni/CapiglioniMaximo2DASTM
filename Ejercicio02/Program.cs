using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio02
{
    public enum Genero { Drama, Comedia, Accion, CienciaFiccion, Documental, Fantasia, Terror, Romance }
    public enum TipoPaquete { Basico, Silver, Premium }

    public class Episodio
    {
        public int Numero { get; private set; }
        public string Titulo { get; private set; }
        public Episodio(int numero, string titulo)
        {
            Numero = numero; Titulo = titulo;
        }
        public override string ToString() => $"Ep{Numero}: {Titulo}";
    }

    public class Temporada
    {
        public int Numero { get; private set; }
        public List<Episodio> Episodios { get; private set; } = new List<Episodio>();
        public Temporada(int numero) { Numero = numero; }
        public override string ToString() => $"Temporada {Numero} ({Episodios.Count} eps)";
    }

    public class Serie
    {
        public string Nombre { get; private set; }
        public Genero Genero { get; private set; }
        public string Director { get; private set; }
        public double Ranking { get; private set; } // 0..5
        public List<Temporada> Temporadas { get; private set; } = new List<Temporada>();

        // Constructor 
        public Serie(string nombre, Genero genero, string director, double ranking)
        { Nombre = nombre; Genero = genero; Director = director; Ranking = ranking; }


        public Serie(string nombre, Genero genero, string director) : this(nombre, genero, director, 3.0) { }

        public override string ToString() => $"{Nombre} | {Genero} | Dir: {Director} | Rank: {Ranking:F1} | Temps: {Temporadas.Count}";
    }

    public class Canal
    {
        public string Nombre { get; private set; }
        public bool EsExclusivo { get; private set; }
        public List<Serie> Series { get; private set; } = new List<Serie>();
        public Canal(string nombre, bool exclusivo) { Nombre = nombre; EsExclusivo = exclusivo; }
        public override string ToString() => $"{Nombre} {(EsExclusivo ? "(Exclusivo)" : "")}";
    }

    public abstract class Paquete
    {
        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
        public decimal CostoBase { get; private set; } // costo propio del paquete
        public List<Canal> Canales { get; private set; } = new List<Canal>();

        protected Paquete(string codigo, string nombre, decimal costoBase)
        { Codigo = codigo; Nombre = nombre; CostoBase = costoBase; }

        public abstract TipoPaquete Tipo { get; }
        public abstract decimal IncrementoAbono(decimal abonoBase);

        public decimal ImporteParaCliente(decimal abonoBase)
        { return CostoBase + abonoBase + IncrementoAbono(abonoBase); }

        public override string ToString() => $"[{Tipo}] {Nombre} (Cod:{Codigo}) | CostoBase: $ {CostoBase:N2} | Canales: {Canales.Count}";
    }

    public class PaqueteBasico : Paquete
    {
        public PaqueteBasico(string codigo, string nombre, decimal costo) : base(codigo, nombre, costo) { }
        public override TipoPaquete Tipo => TipoPaquete.Basico;
        public override decimal IncrementoAbono(decimal abonoBase) => 0m;
    }

    public class PaqueteSilver : Paquete
    {
        public PaqueteSilver(string codigo, string nombre, decimal costo) : base(codigo, nombre, costo) { }
        public override TipoPaquete Tipo => TipoPaquete.Silver;
        public override decimal IncrementoAbono(decimal abonoBase) => Math.Round(abonoBase * 0.15m, 2);
    }

    public class PaquetePremium : Paquete
    {
        public PaquetePremium(string codigo, string nombre, decimal costo) : base(codigo, nombre, costo) { }
        public override TipoPaquete Tipo => TipoPaquete.Premium;
        public override decimal IncrementoAbono(decimal abonoBase) => Math.Round(abonoBase * 0.20m, 2);
    }

    public class Cliente
    {
        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public string Dni { get; private set; }
        public DateTime FechaNacimiento { get; private set; }
        public decimal AbonoBase { get; private set; }
        public Paquete PaqueteContratado { get; private set; }

        public Cliente(string codigo, string nombre, string apellido, string dni, DateTime fnac, decimal abonoBase)
        { Codigo = codigo; Nombre = nombre; Apellido = apellido; Dni = dni; FechaNacimiento = fnac; AbonoBase = abonoBase; }

        public string NombreCompleto => $"{Apellido}, {Nombre}";
        public void Contratar(Paquete p) { PaqueteContratado = p; }
        public void Cancelar() { PaqueteContratado = null; }

        public decimal ImporteMensual()
        { return PaqueteContratado == null ? AbonoBase : PaqueteContratado.ImporteParaCliente(AbonoBase); }

        public override string ToString() => $"{NombreCompleto} (Cod:{Codigo}, DNI:{Dni}) | AbonoBase: $ {AbonoBase:N2} | Paquete: {(PaqueteContratado?.Nombre ?? "-")}";
    }


    class Program
    {

        static List<Canal> Canales = new List<Canal>();
        static List<Paquete> Paquetes = new List<Paquete>();
        static List<Cliente> Clientes = new List<Cliente>();
        static Dictionary<string, int> VentasPorPaquete = new Dictionary<string, int>();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Seed();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== CableCo (Versión Didáctica) ===\n");
                Console.WriteLine("1) ABM Clientes");
                Console.WriteLine("2) ABM Contenido (Canales/Series)");
                Console.WriteLine("3) ABM Paquetes y Contratación");
                Console.WriteLine("4) Reportes");
                Console.WriteLine("0) Salir");
                Console.Write("Opción: ");
                var op = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (op)
                    {
                        case "1": MenuClientes(); break;
                        case "2": MenuContenido(); break;
                        case "3": MenuPaquetes(); break;
                        case "4": MenuReportes(); break;
                        case "0": return;
                        default: Console.WriteLine("Opción inválida"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }

                Pausa();
            }
        }


        static void MenuClientes()
        {
            Console.WriteLine("-- CLIENTES --");
            Console.WriteLine("1) Agregar");
            Console.WriteLine("2) Listar");
            Console.WriteLine("3) Contratar paquete");
            Console.WriteLine("4) Cancelar paquete");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            if (op == "1") AgregarCliente();
            else if (op == "2") ListarClientes();
            else if (op == "3") ContratarPaquete();
            else if (op == "4") CancelarPaquete();
            else Console.WriteLine("Opción inválida");
        }

        static void AgregarCliente()
        {
            var codigo = Read("Código");
            if (Clientes.Any(c => c.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Código duplicado");
            var nombre = Read("Nombre");
            var apellido = Read("Apellido");
            var dni = Read("DNI");
            if (Clientes.Any(c => c.Dni.Equals(dni, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("DNI ya registrado");
            var fnac = ReadDate("Fecha Nac (dd/mm/aaaa)");
            var abono = ReadMoney("Abono base");
            Clientes.Add(new Cliente(codigo, nombre, apellido, dni, fnac, abono));
            Console.WriteLine("Cliente agregado.");
        }

        static void ListarClientes()
        {
            foreach (var c in Clientes.OrderBy(x => x.Apellido))
            {
                Console.WriteLine(c);
                Console.WriteLine("   → Importe mensual: $ " + c.ImporteMensual().ToString("N2"));
            }
        }

        static void ContratarPaquete()
        {
            if (!Clientes.Any() || !Paquetes.Any()) { Console.WriteLine("Faltan clientes o paquetes."); return; }
            ListarClientesCorto();
            var codCli = Read("Código cliente");
            var cli = Clientes.FirstOrDefault(c => c.Codigo.Equals(codCli, StringComparison.OrdinalIgnoreCase));
            if (cli == null) throw new Exception("Cliente no encontrado");

            ListarPaquetesCorto();
            var codPack = Read("Código paquete");
            var pack = Paquetes.FirstOrDefault(p => p.Codigo.Equals(codPack, StringComparison.OrdinalIgnoreCase));
            if (pack == null) throw new Exception("Paquete no encontrado");

            cli.Contratar(pack);
            if (!VentasPorPaquete.ContainsKey(pack.Codigo)) VentasPorPaquete[pack.Codigo] = 0;
            VentasPorPaquete[pack.Codigo]++;
            Console.WriteLine($"Contratado: {pack.Nombre}. Importe mensual ahora: $ {cli.ImporteMensual():N2}");
        }

        static void CancelarPaquete()
        {
            ListarClientesCorto();
            var codCli = Read("Código cliente");
            var cli = Clientes.FirstOrDefault(c => c.Codigo.Equals(codCli, StringComparison.OrdinalIgnoreCase));
            if (cli == null) throw new Exception("Cliente no encontrado");
            cli.Cancelar();
            Console.WriteLine("Paquete cancelado.");
        }


        static void MenuContenido()
        {
            Console.WriteLine("-- CONTENIDO --");
            Console.WriteLine("1) Agregar canal");
            Console.WriteLine("2) Agregar serie a canal");
            Console.WriteLine("3) Agregar temporada a serie");
            Console.WriteLine("4) Agregar episodio a temporada");
            Console.WriteLine("5) Listar contenido");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            if (op == "1") AgregarCanal();
            else if (op == "2") AgregarSerie();
            else if (op == "3") AgregarTemporada();
            else if (op == "4") AgregarEpisodio();
            else if (op == "5") ListarContenido();
            else Console.WriteLine("Opción inválida");
        }

        static void AgregarCanal()
        {
            var nombre = Read("Nombre canal");
            var exclusivo = YesNo("¿Exclusivo? (s/n)");
            if (Canales.Any(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Ya existe canal");
            Canales.Add(new Canal(nombre, exclusivo));
            Console.WriteLine("Canal agregado.");
        }

        static void AgregarSerie()
        {
            if (!Canales.Any()) { Console.WriteLine("Primero agregá un canal"); return; }
            ListarCanalesCorto();
            var nombreCanal = Read("Canal destino");
            var canal = Canales.FirstOrDefault(c => c.Nombre.Equals(nombreCanal, StringComparison.OrdinalIgnoreCase));
            if (canal == null) throw new Exception("Canal no encontrado");

            var nombre = Read("Nombre serie");
            var genero = ElegirGenero();
            var director = Read("Director");
            Console.Write("Ranking (0..5, vacío = 3.0): ");
            var s = Console.ReadLine();
            Serie serie = string.IsNullOrWhiteSpace(s) ? new Serie(nombre, genero, director) : new Serie(nombre, genero, director, double.Parse(s, CultureInfo.InvariantCulture));
            canal.Series.Add(serie);
            Console.WriteLine("Serie agregada.");
        }

        static void AgregarTemporada()
        {
            var canal = ElegirCanal();
            var serie = ElegirSerie(canal);
            var n = ReadInt("> Número de temporada");
            if (serie.Temporadas.Any(t => t.Numero == n)) throw new Exception("Temporada existente");
            serie.Temporadas.Add(new Temporada(n));
            Console.WriteLine("Temporada agregada.");
        }

        static void AgregarEpisodio()
        {
            var canal = ElegirCanal();
            var serie = ElegirSerie(canal);
            var ntemp = ReadInt("> Número de temporada");
            var temp = serie.Temporadas.FirstOrDefault(t => t.Numero == ntemp);
            if (temp == null) throw new Exception("Temporada no encontrada");
            var ne = ReadInt("> Número de episodio");
            var titulo = Read("> Título");
            temp.Episodios.Add(new Episodio(ne, titulo));
            Console.WriteLine("Episodio agregado.");
        }

        static void ListarContenido()
        {
            foreach (var c in Canales)
            {
                Console.WriteLine("Canal: " + c);
                foreach (var s in c.Series)
                {
                    Console.WriteLine("  - " + s);
                    foreach (var t in s.Temporadas)
                    {
                        Console.WriteLine("     • " + t);
                        foreach (var e in t.Episodios)
                            Console.WriteLine("        · " + e);
                    }
                }
            }
        }


        static void MenuPaquetes()
        {
            Console.WriteLine("-- PAQUETES --");
            Console.WriteLine("1) Crear paquete");
            Console.WriteLine("2) Agregar canal a paquete");
            Console.WriteLine("3) Listar paquetes");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            if (op == "1") CrearPaquete();
            else if (op == "2") AgregarCanalAPaquete();
            else if (op == "3") ListarPaquetes();
            else Console.WriteLine("Opción inválida");
        }

        static void CrearPaquete()
        {
            Console.WriteLine("Tipo: 1) Básico  2) Silver  3) Premium");
            var tipo = Console.ReadLine();
            Console.Write("Código: "); var codigo = Console.ReadLine();
            Console.Write("Nombre: "); var nombre = Console.ReadLine();
            Console.Write("Costo base: "); var costo = decimal.Parse(Console.ReadLine());

            Paquete p;
            switch (tipo)
            {
                case "1":
                    p = new PaqueteBasico(codigo, nombre, costo);
                    break;
                case "2":
                    p = new PaqueteSilver(codigo, nombre, costo);
                    break;
                case "3":
                    p = new PaquetePremium(codigo, nombre, costo);
                    break;
                default:
                    Console.WriteLine("Tipo inválido");
                    return;
            }

            if (Paquetes.Any(x => x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Código de paquete duplicado");

            Paquetes.Add(p);
            Console.WriteLine("Paquete creado!");
        }


        static void AgregarCanalAPaquete()
        {
            if (!Paquetes.Any() || !Canales.Any()) { Console.WriteLine("Faltan paquetes o canales"); return; }
            ListarPaquetes();
            var cod = Read("Código paquete");
            var pack = Paquetes.FirstOrDefault(p => p.Codigo.Equals(cod, StringComparison.OrdinalIgnoreCase));
            if (pack == null) throw new Exception("Paquete no encontrado");
            ListarCanalesCorto();
            var nombreCanal = Read("Nombre canal a agregar");
            var canal = Canales.FirstOrDefault(c => c.Nombre.Equals(nombreCanal, StringComparison.OrdinalIgnoreCase));
            if (canal == null) throw new Exception("Canal no encontrado");
            if (pack.Canales.Any(c => c.Nombre.Equals(canal.Nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("El canal ya está en el paquete");
            pack.Canales.Add(canal);
            Console.WriteLine("Canal agregado al paquete.");
        }

        static void ListarPaquetes()
        {
            foreach (var p in Paquetes)
            {
                Console.WriteLine(p);
                foreach (var c in p.Canales)
                {
                    Console.WriteLine("  - " + c.Nombre);
                    foreach (var s in c.Series)
                        Console.WriteLine("     • " + s.Nombre);
                }
            }
        }


        static void MenuReportes()
        {
            Console.WriteLine("-- REPORTES --");
            Console.WriteLine("1) Contenido de cada paquete (series/temps/eps)");
            Console.WriteLine("2) Paquete de cada cliente + importes");
            Console.WriteLine("3) Total recaudado mensual");
            Console.WriteLine("4) Paquete más vendido + series");
            Console.WriteLine("5) Series con ranking > 3.5");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            if (op == "1") ReporteContenidoPaquetes();
            else if (op == "2") ReporteClientesImportes();
            else if (op == "3") ReporteTotalRecaudado();
            else if (op == "4") ReportePaqueteMasVendido();
            else if (op == "5") ReporteSeriesRanking();
            else Console.WriteLine("Opción inválida");
        }

        static void ReporteContenidoPaquetes()
        {
            foreach (var p in Paquetes)
            {
                Console.WriteLine($"\n{p}");
                foreach (var c in p.Canales)
                {
                    Console.WriteLine($"  Canal: {c}");
                    foreach (var s in c.Series)
                    {
                        Console.WriteLine($"    Serie: {s.Nombre} | Rank {s.Ranking:F1}");
                        foreach (var t in s.Temporadas)
                        {
                            Console.WriteLine($"      T{t.Numero}: {t.Episodios.Count} ep(s)");
                            foreach (var e in t.Episodios)
                                Console.WriteLine($"        - {e}");
                        }
                    }
                }
            }
        }

        static void ReporteClientesImportes()
        {
            foreach (var c in Clientes)
            {
                Console.WriteLine($"{c.NombreCompleto} | Paquete: {c.PaqueteContratado?.Nombre ?? "(ninguno)"} | Importe mensual: $ {c.ImporteMensual():N2}");
            }
        }

        static void ReporteTotalRecaudado()
        {
            var total = Clientes.Sum(c => c.ImporteMensual());
            Console.WriteLine($"Total recaudado en el mes: $ {total:N2}");
        }

        static void ReportePaqueteMasVendido()
        {
            if (!VentasPorPaquete.Any()) { Console.WriteLine("Sin ventas"); return; }
            var kv = VentasPorPaquete.OrderByDescending(x => x.Value).First();
            var pack = Paquetes.FirstOrDefault(p => p.Codigo == kv.Key);
            if (pack == null) { Console.WriteLine("Sin datos"); return; }
            Console.WriteLine($"Más vendido: {pack.Nombre} ({pack.Tipo}) - {kv.Value} venta(s)");
            foreach (var c in pack.Canales)
            {
                Console.WriteLine("  - " + c.Nombre);
                foreach (var s in c.Series) Console.WriteLine("     • " + s.Nombre);
            }
        }

        static void ReporteSeriesRanking()
        {
            var series = Canales.SelectMany(c => c.Series).Where(s => s.Ranking > 3.5).OrderByDescending(s => s.Ranking).ToList();
            if (!series.Any()) { Console.WriteLine("No hay series > 3.5"); return; }
            foreach (var s in series) Console.WriteLine($"{s.Nombre} | Rank {s.Ranking:F1} | {s.Genero} | Dir {s.Director}");
        }


        static string Read(string label)
        {
            Console.Write(label + ": ");
            var s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
        }
        static DateTime ReadDate(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (DateTime.TryParseExact(s, new[] { "dd/MM/yyyy", "d/M/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt.Date;
                Console.WriteLine("* Fecha inválida (ej 05/04/2002)");
            }
        }
        static decimal ReadMoney(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) && d >= 0) return Math.Round(d, 2);
                Console.WriteLine("* Monto inválido (>=0)");
            }
        }
        static int ReadInt(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out var n) && n > 0) return n;
                Console.WriteLine("* Número inválido (>0)");
            }
        }
        static bool YesNo(string label)
        {
            while (true)
            {
                Console.Write(label + " ");
                var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (s == "s" || s == "si" || s == "sí") return true;
                if (s == "n" || s == "no") return false;
                Console.WriteLine("* Responder s/n");
            }
        }
        static Genero ElegirGenero()
        {
            var values = Enum.GetValues(typeof(Genero)).Cast<Genero>().ToList();
            for (int i = 0; i < values.Count; i++) Console.WriteLine($" {i + 1}) {values[i]}");
            while (true)
            {
                Console.Write("Elegí género (número): ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out var n) && n >= 1 && n <= values.Count) return values[n - 1];
                Console.WriteLine("* Opción inválida");
            }
        }
        static Canal ElegirCanal()
        {
            ListarCanalesCorto();
            var nombre = Read("Canal");
            var canal = Canales.FirstOrDefault(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (canal == null) throw new Exception("Canal no encontrado");
            return canal;
        }
        static Serie ElegirSerie(Canal canal)
        {
            foreach (var s in canal.Series) Console.WriteLine(" - " + s.Nombre);
            var nombre = Read("Serie");
            var serie = canal.Series.FirstOrDefault(x => x.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (serie == null) throw new Exception("Serie no encontrada");
            return serie;
        }

        static void ListarPaquetesCorto()
        { foreach (var p in Paquetes) Console.WriteLine($"{p.Codigo} - {p.Nombre} [{p.Tipo}]"); }
        static void ListarCanalesCorto()
        { foreach (var c in Canales) Console.WriteLine("- " + c.Nombre + (c.EsExclusivo ? " (Exclusivo)" : "")); }
        static void ListarClientesCorto()
        { foreach (var c in Clientes) Console.WriteLine($"{c.Codigo} - {c.NombreCompleto}"); }

        static void Pausa()
        {
            Console.WriteLine("\nPresioná una tecla para continuar...");
            Console.ReadKey(true);
        }

        // Datos de ejemplo mínimos
        static void Seed()
        {
            var hbo = new Canal("HBO Max", true);
            var natgeo = new Canal("NatGeo", false);
            Canales.Add(hbo); Canales.Add(natgeo);

            var s1 = new Serie("Cosmos", Genero.Documental, "Ann Druyan", 4.8);
            var t1 = new Temporada(1); t1.Episodios.Add(new Episodio(1, "Standing Up"));
            s1.Temporadas.Add(t1);
            natgeo.Series.Add(s1);

            var s2 = new Serie("The Last Kingdom", Genero.Accion, "Peter Hoar", 4.4);
            var t2 = new Temporada(1); t2.Episodios.Add(new Episodio(1, "Episode 1")); t2.Episodios.Add(new Episodio(2, "Episode 2"));
            s2.Temporadas.Add(t2);
            hbo.Series.Add(s2);

            var pb = new PaqueteBasico("PK-BAS", "Básico", 0);
            pb.Canales.Add(natgeo);
            var ps = new PaqueteSilver("PK-SLV", "Silver", 1000);
            ps.Canales.Add(natgeo);
            var pp = new PaquetePremium("PK-PRM", "Premium", 2500);
            pp.Canales.Add(hbo); pp.Canales.Add(natgeo);
            Paquetes.Add(pb); Paquetes.Add(ps); Paquetes.Add(pp);
            VentasPorPaquete["PK-BAS"] = 0; VentasPorPaquete["PK-SLV"] = 0; VentasPorPaquete["PK-PRM"] = 0;

            Clientes.Add(new Cliente("C001", "Ana", "Gómez", "30111222", new DateTime(1995, 5, 12), 8000));
            Clientes.Add(new Cliente("C002", "Bruno", "Pérez", "33123456", new DateTime(1990, 10, 3), 6500));
        }
    }
}
