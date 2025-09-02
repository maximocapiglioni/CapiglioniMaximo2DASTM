using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio01
{
    public class Cliente
    {
        public string Dni { get; private set; }
        public string NombreCompleto { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public DateTime FechaNacimiento { get; private set; }

        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                int edad = hoy.Year - FechaNacimiento.Year;
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        public Cliente(string dni, string nombreCompleto, string telefono, string email, DateTime fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(dni)) throw new ArgumentException("El DNI es obligatorio");
            if (string.IsNullOrWhiteSpace(nombreCompleto)) throw new ArgumentException("El nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio");

            Dni = dni.Trim();
            NombreCompleto = nombreCompleto.Trim();
            Telefono = telefono.Trim();
            Email = email.Trim();
            FechaNacimiento = fechaNacimiento.Date;
        }

        public void Modificar(string nombreCompleto, string telefono, string email, DateTime fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto)) throw new ArgumentException("El nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio");

            NombreCompleto = nombreCompleto.Trim();
            Telefono = telefono.Trim();
            Email = email.Trim();
            FechaNacimiento = fechaNacimiento.Date;
        }

        public override string ToString() => $"{NombreCompleto} (DNI: {Dni}) - Tel: {Telefono} - Email: {Email} - Edad: {Edad}";
    }

    public enum TipoMovimiento { Deposito, Extraccion }

    public sealed class Movimiento
    {
        public DateTime Fecha { get; private set; }
        public TipoMovimiento Tipo { get; private set; }
        public decimal Importe { get; private set; }

        public Movimiento(DateTime fecha, TipoMovimiento tipo, decimal importe)
        {
            Fecha = fecha;
            Tipo = tipo;
            Importe = importe;
        }

        public override string ToString() => $"[{Fecha:yyyy-MM-dd HH:mm}] {Tipo} - $ {Importe:N2}";
    }

    public abstract class Cuenta
    {
        public string Codigo { get; private set; }
        public Cliente Titular { get; private set; }
        public decimal Saldo { get; protected set; }
        public List<Movimiento> Movimientos { get; private set; } = new List<Movimiento>();

        protected Cuenta(string codigo, Cliente titular)
        {
            if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("El código de cuenta es obligatorio");
            Codigo = codigo.Trim();
            Titular = titular ?? throw new ArgumentNullException(nameof(titular));
        }

        public void CambiarTitular(Cliente nuevoTitular)
        {
            Titular = nuevoTitular ?? throw new ArgumentNullException(nameof(nuevoTitular));
        }

        public void Depositar(decimal importe)
        {
            if (importe <= 0) throw new InvalidOperationException("El importe a depositar debe ser mayor a cero");
            Saldo += importe;
            Movimientos.Add(new Movimiento(DateTime.Now, TipoMovimiento.Deposito, importe));
        }

        public void Extraer(decimal importe)
        {
            if (importe <= 0) throw new InvalidOperationException("El importe a extraer debe ser mayor a cero");
            if (!PuedeExtraer(importe)) throw new InvalidOperationException("La extracción no está permitida por las condiciones de la cuenta");
            Saldo -= importe;
            Movimientos.Add(new Movimiento(DateTime.Now, TipoMovimiento.Extraccion, importe));
        }

        protected abstract bool PuedeExtraer(decimal importe);

        public override string ToString() => $"{GetType().Name} #{Codigo} | Titular: {Titular.NombreCompleto} | Saldo: $ {Saldo:N2}";
    }

    public sealed class CajaAhorro : Cuenta
    {
        public decimal TopePorExtraccion { get; private set; }

        public CajaAhorro(string codigo, Cliente titular, decimal topePorExtraccion)
            : base(codigo, titular)
        {
            if (topePorExtraccion <= 0) throw new ArgumentException("El tope por extracción debe ser mayor a cero");
            TopePorExtraccion = topePorExtraccion;
        }

        protected override bool PuedeExtraer(decimal importe)
        {
            return importe <= Saldo && importe <= TopePorExtraccion;
        }

        public override string ToString() => base.ToString() + $" | Tope extracción: $ {TopePorExtraccion:N2}";
    }

    public sealed class CuentaCorriente : Cuenta
    {
        public decimal LimiteDescubierto { get; private set; } // saldo negativo 

        public CuentaCorriente(string codigo, Cliente titular, decimal limiteDescubierto)
            : base(codigo, titular)
        {
            if (limiteDescubierto < 0) throw new ArgumentException("El límite de descubierto no puede ser negativo");
            LimiteDescubierto = limiteDescubierto;
        }

        protected override bool PuedeExtraer(decimal importe)
        {
            decimal saldoPost = Saldo - importe;
            return saldoPost >= -LimiteDescubierto;
        }

        public override string ToString() => base.ToString() + $" | Descubierto: $ {LimiteDescubierto:N2}";
    }

    public class BancoService
    {
        private readonly List<Cliente> _clientes = new List<Cliente>();
        private readonly List<Cuenta> _cuentas = new List<Cuenta>();

        public IReadOnlyList<Cliente> Clientes => _clientes.AsReadOnly();
        public IReadOnlyList<Cuenta> Cuentas => _cuentas.AsReadOnly();


        public void AgregarCliente(Cliente nuevo)
        {
            if (_clientes.Any(c => c.Dni == nuevo.Dni))
                throw new InvalidOperationException("Ya existe un cliente con ese DNI");
            _clientes.Add(nuevo);
        }

        public void ModificarCliente(string dni, string nombre, string tel, string email, DateTime fechaNac)
        {
            var cli = BuscarClientePorDni(dni) ?? throw new KeyNotFoundException("Cliente no encontrado");
            cli.Modificar(nombre, tel, email, fechaNac);
        }

        public void EliminarCliente(string dni)
        {
            var cli = BuscarClientePorDni(dni) ?? throw new KeyNotFoundException("Cliente no encontrado");
            if (_cuentas.Any(c => c.Titular.Dni == dni))
                throw new InvalidOperationException("No se puede eliminar: el cliente posee cuentas");
            _clientes.Remove(cli);
        }

        public Cliente BuscarClientePorDni(string dni)
        {
            return _clientes.FirstOrDefault(c => c.Dni.Equals(dni, StringComparison.OrdinalIgnoreCase));
        }

        public List<Cliente> BuscarClientesPorNombre(string texto)
        {
            texto = (texto ?? string.Empty).Trim();
            return _clientes
                .Where(c => c.NombreCompleto.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(c => c.NombreCompleto)
                .ToList();
        }


        public void AgregarCuenta(Cuenta cuenta)
        {
            if (_cuentas.Any(c => c.Codigo == cuenta.Codigo))
                throw new InvalidOperationException("Ya existe una cuenta con ese código");

            if (!_clientes.Any(c => c.Dni == cuenta.Titular.Dni))
                throw new InvalidOperationException("El titular debe estar registrado como cliente");
            _cuentas.Add(cuenta);
        }

        public void CambiarTitularDeCuenta(string codigoCuenta, string dniNuevoTitular)
        {
            var cta = BuscarCuentaPorCodigo(codigoCuenta) ?? throw new KeyNotFoundException("Cuenta no encontrada");
            var nuevoTitular = BuscarClientePorDni(dniNuevoTitular) ?? throw new KeyNotFoundException("El nuevo titular no existe como cliente");
            cta.CambiarTitular(nuevoTitular);
        }

        public void EliminarCuenta(string codigoCuenta)
        {
            var cta = BuscarCuentaPorCodigo(codigoCuenta) ?? throw new KeyNotFoundException("Cuenta no encontrada");
            if (cta.Saldo != 0)
                throw new InvalidOperationException("Solo se pueden eliminar cuentas con saldo igual a cero");
            _cuentas.Remove(cta);
        }

        public Cuenta BuscarCuentaPorCodigo(string codigo)
        {
            return _cuentas.FirstOrDefault(c => c.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        }

        public List<Cuenta> ListarCuentasDeCliente(string dni)
        {
            return _cuentas.Where(c => c.Titular.Dni == dni).ToList();
        }


        public void Depositar(string codigoCuenta, decimal importe)
        {
            var cta = BuscarCuentaPorCodigo(codigoCuenta) ?? throw new KeyNotFoundException("Cuenta no encontrada");
            cta.Depositar(importe);
        }

        public void Extraer(string codigoCuenta, decimal importe)
        {
            var cta = BuscarCuentaPorCodigo(codigoCuenta) ?? throw new KeyNotFoundException("Cuenta no encontrada");
            cta.Extraer(importe);
        }
    }


    internal static class Program
    {
        private static readonly BancoService banco = new BancoService();

        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            SeedEjemplo();
            while (true)
            {
                try
                {
                    MostrarMenuPrincipal();
                    Console.Write("\nElegí una opción: ");
                    var opcion = Console.ReadLine();
                    Console.WriteLine();
                    switch (opcion)
                    {
                        case "1": MenuClientes(); break;
                        case "2": MenuCuentas(); break;
                        case "3": MenuOperaciones(); break;
                        case "4": ListadoClientes(); break;
                        case "0": return;
                        default: Console.WriteLine("Opción inválida\n"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nERROR: {ex.Message}\n");
                }
                Pausa();
            }
        }

        private static void MostrarMenuPrincipal()
        {
            Console.Clear();
            Console.WriteLine("==============================");
            Console.WriteLine("   BANCO - Gestión de Cuentas  ");
            Console.WriteLine("==============================\n");
            Console.WriteLine("1) Gestión de clientes");
            Console.WriteLine("2) Gestión de cuentas");
            Console.WriteLine("3) Operaciones (depósitos / extracciones)");
            Console.WriteLine("4) Listado y búsqueda de clientes");
            Console.WriteLine("0) Salir");
        }


        private static void MenuClientes()
        {
            Console.WriteLine("--- CLIENTES ---");
            Console.WriteLine("1) Agregar");
            Console.WriteLine("2) Modificar");
            Console.WriteLine("3) Eliminar (sin cuentas)");
            Console.WriteLine("4) Buscar por DNI");
            Console.WriteLine("5) Buscar por nombre");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            switch (op)
            {
                case "1": AgregarClienteUI(); break;
                case "2": ModificarClienteUI(); break;
                case "3": EliminarClienteUI(); break;
                case "4": BuscarClienteDniUI(); break;
                case "5": BuscarClientesNombreUI(); break;
                default: Console.WriteLine("Opción inválida"); break;
            }
        }

        private static void AgregarClienteUI()
        {
            Console.WriteLine("Alta de cliente");
            string dni = LeerTextoObligatorio("DNI");
            string nombre = LeerTextoObligatorio("Nombre y apellido");
            string tel = LeerTextoObligatorio("Teléfono");
            string email = LeerTextoObligatorio("Email");
            DateTime fnac = LeerFecha("Fecha de nacimiento (dd/mm/aaaa)");

            var cliente = new Cliente(dni, nombre, tel, email, fnac);
            banco.AgregarCliente(cliente);
            Console.WriteLine("Cliente agregado correctamente.");
        }

        private static void ModificarClienteUI()
        {
            string dni = LeerTextoObligatorio("DNI del cliente a modificar");
            var cli = banco.BuscarClientePorDni(dni) ?? throw new Exception("Cliente no encontrado");
            Console.WriteLine($"Editando: {cli}");

            string nombre = LeerTextoObligatorio("Nuevo nombre y apellido");
            string tel = LeerTextoObligatorio("Nuevo teléfono");
            string email = LeerTextoObligatorio("Nuevo email");
            DateTime fnac = LeerFecha("Nueva fecha de nacimiento (dd/mm/aaaa)");
            banco.ModificarCliente(dni, nombre, tel, email, fnac);
            Console.WriteLine("Cliente modificado.");
        }

        private static void EliminarClienteUI()
        {
            string dni = LeerTextoObligatorio("DNI del cliente a eliminar");
            banco.EliminarCliente(dni);
            Console.WriteLine("Cliente eliminado.");
        }

        private static void BuscarClienteDniUI()
        {
            string dni = LeerTextoObligatorio("DNI a buscar");
            var cli = banco.BuscarClientePorDni(dni);
            if (cli == null) Console.WriteLine("No encontrado");
            else Console.WriteLine(cli);
        }

        private static void BuscarClientesNombreUI()
        {
            string q = LeerTextoObligatorio("Texto a buscar en el nombre");
            var list = banco.BuscarClientesPorNombre(q);
            if (!list.Any()) Console.WriteLine("Sin resultados");
            foreach (var c in list) Console.WriteLine(c);
        }

        private static void ListadoClientes()
        {
            Console.WriteLine("=== LISTADO DE CLIENTES ===");
            foreach (var c in banco.Clientes.OrderBy(x => x.NombreCompleto))
            {
                Console.WriteLine(c);
                var cuentas = banco.ListarCuentasDeCliente(c.Dni);
                if (cuentas.Any())
                {
                    foreach (var ct in cuentas)
                        Console.WriteLine("    - " + ct);
                }
                else Console.WriteLine("    (sin cuentas)");
            }
        }


        private static void MenuCuentas()
        {
            Console.WriteLine("--- CUENTAS ---");
            Console.WriteLine("1) Agregar cuenta");
            Console.WriteLine("2) Cambiar titular");
            Console.WriteLine("3) Eliminar cuenta (saldo 0)");
            Console.WriteLine("4) Ver cuentas por DNI del titular");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            switch (op)
            {
                case "1": AgregarCuentaUI(); break;
                case "2": CambiarTitularUI(); break;
                case "3": EliminarCuentaUI(); break;
                case "4": ListarCuentasPorClienteUI(); break;
                default: Console.WriteLine("Opción inválida"); break;
            }
        }

        private static void AgregarCuentaUI()
        {
            Console.WriteLine("Tipos de cuenta: 1) Caja de Ahorro  2) Cuenta Corriente");
            Console.Write("Elegí tipo (1/2): ");
            var tipo = Console.ReadLine();
            string codigo = LeerTextoObligatorio("Código único de cuenta");
            string dniTitular = LeerTextoObligatorio("DNI del titular (debe existir)");

            if (tipo == "1")
            {
                decimal tope = LeerDecimalPositivo("Tope por extracción");
                var titular = banco.BuscarClientePorDni(dniTitular) ?? throw new Exception("El titular no existe");
                var cta = new CajaAhorro(codigo, titular, tope);
                banco.AgregarCuenta(cta);
            }
            else if (tipo == "2")
            {
                decimal descubierto = LeerDecimalNoNegativo("Límite de descubierto (saldo negativo permitido)");
                var titular = banco.BuscarClientePorDni(dniTitular) ?? throw new Exception("El titular no existe");
                var cta = new CuentaCorriente(codigo, titular, descubierto);
                banco.AgregarCuenta(cta);
            }
            else throw new Exception("Tipo inválido");

            Console.WriteLine("Cuenta creada.");
        }

        private static void CambiarTitularUI()
        {
            string codigo = LeerTextoObligatorio("Código de cuenta");
            string dniNuevo = LeerTextoObligatorio("DNI del nuevo titular (debe existir)");
            banco.CambiarTitularDeCuenta(codigo, dniNuevo);
            Console.WriteLine("Titular actualizado.");
        }

        private static void EliminarCuentaUI()
        {
            string codigo = LeerTextoObligatorio("Código de cuenta a eliminar");
            banco.EliminarCuenta(codigo);
            Console.WriteLine("Cuenta eliminada.");
        }

        private static void ListarCuentasPorClienteUI()
        {
            string dni = LeerTextoObligatorio("DNI del titular");
            var list = banco.ListarCuentasDeCliente(dni);
            if (!list.Any()) Console.WriteLine("Sin cuentas");
            foreach (var c in list) Console.WriteLine(c);
        }


        private static void MenuOperaciones()
        {
            Console.WriteLine("--- OPERACIONES ---");
            Console.WriteLine("1) Depositar");
            Console.WriteLine("2) Extraer");
            Console.WriteLine("3) Ver movimientos de una cuenta");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            switch (op)
            {
                case "1": DepositarUI(); break;
                case "2": ExtraerUI(); break;
                case "3": VerMovimientosUI(); break;
                default: Console.WriteLine("Opción inválida"); break;
            }
        }

        private static void DepositarUI()
        {
            string codigo = LeerTextoObligatorio("Código de cuenta");
            decimal importe = LeerDecimalPositivo("Importe a depositar");
            banco.Depositar(codigo, importe);
            Console.WriteLine("Depósito realizado.");
        }

        private static void ExtraerUI()
        {
            string codigo = LeerTextoObligatorio("Código de cuenta");
            decimal importe = LeerDecimalPositivo("Importe a extraer");
            banco.Extraer(codigo, importe);
            Console.WriteLine("Extracción realizada.");
        }

        private static void VerMovimientosUI()
        {
            string codigo = LeerTextoObligatorio("Código de cuenta");
            var cta = banco.BuscarCuentaPorCodigo(codigo) ?? throw new Exception("Cuenta no encontrada");
            Console.WriteLine(cta);
            if (!cta.Movimientos.Any()) Console.WriteLine("(Sin movimientos)");
            foreach (var m in cta.Movimientos.OrderByDescending(m => m.Fecha))
                Console.WriteLine("  - " + m);
        }


        private static string LeerTextoObligatorio(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("* Campo obligatorio\n");
            }
        }

        private static DateTime LeerFecha(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (DateTime.TryParseExact(s, new[] { "dd/MM/yyyy", "d/M/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return dt.Date;
                Console.WriteLine("* Formato inválido. Ej: 25/08/2001\n");
            }
        }

        private static decimal LeerDecimalPositivo(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) && val > 0)
                    return decimal.Round(val, 2);
                Console.WriteLine("* Ingresá un número mayor a 0\n");
            }
        }

        private static decimal LeerDecimalNoNegativo(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                var s = Console.ReadLine();
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) && val >= 0)
                    return decimal.Round(val, 2);
                Console.WriteLine("* Ingresá un número mayor o igual a 0\n");
            }
        }

        private static void Pausa()
        {
            Console.WriteLine("\nPresioná una tecla para continuar...");
            Console.ReadKey();
        }

        private static void SeedEjemplo()
        {

            var c1 = new Cliente("30111222", "Ana Gómez", "351-5550001", "ana@correo.com", new DateTime(1993, 5, 10));
            var c2 = new Cliente("33123456", "Bruno Pérez", "351-5550002", "bruno@correo.com", new DateTime(1989, 11, 2));
            banco.AgregarCliente(c1);
            banco.AgregarCliente(c2);

            var a1 = new CajaAhorro("CA-0001", c1, 50000);
            var cc1 = new CuentaCorriente("CC-1001", c2, 20000);
            banco.AgregarCuenta(a1);
            banco.AgregarCuenta(cc1);

            banco.Depositar("CA-0001", 120000);
            banco.Depositar("CC-1001", 30000);
        }
    }
}
