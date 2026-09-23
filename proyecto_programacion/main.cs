using System;
using System.Text;

namespace SimulacionExamen
{
    // Caso 2: examen de selección múltiple con 5 opciones.
    // P(S) = 0.75 (sabe la respuesta), P(C|S) = 1, P(C|S') = 1/5 (adivina).
    // P(C) = P(C|S)·P(S) + P(C|S')·P(S') = 0.75 + 0.05 = 0.80
    class Program
    {
        const double ProbabilidadSaber = 0.75;
        const int NumeroOpciones = 5;
        const int OpcionCorrecta = 1;   // opciones numeradas de 1 a 5, como en el diagrama

        static readonly double ProbabilidadAdivinar = 1.0 / NumeroOpciones;
        static readonly double ProbabilidadTeorica =
            1.0 * ProbabilidadSaber + ProbabilidadAdivinar * (1 - ProbabilidadSaber);
        static readonly double ProbabilidadSaberDadoCorrecta =
            ProbabilidadSaber / ProbabilidadTeorica;

        static readonly int[] TamanosAutomaticos = { 10, 100, 1000, 10000, 100000, 1000000 };
        static readonly Random Aleatorio = new Random();

        // Los 3 resultados posibles de cada arreglo (ramas del diagrama de árbol)
        enum Resultado { SabiaCorrecta, AdivinoCorrecta, AdivinoIncorrecta }

        static readonly Resultado[] TodosLosResultados =
            { Resultado.SabiaCorrecta, Resultado.AdivinoCorrecta, Resultado.AdivinoIncorrecta };

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            MostrarTeoria();

            bool continuar = true;
            while (continuar)
            {
                Console.WriteLine();
                Console.WriteLine("1. Modo manual (tú eliges cuántos arreglos ver)");
                Console.WriteLine("2. Modo automático (frecuencia relativa con n = 10 hasta 1.000.000)");
                Console.WriteLine("3. Salir");
                Console.Write("Opción: ");

                switch (Console.ReadLine())
                {
                    case "1": ModoManual(); break;
                    case "2": ModoAutomatico(); break;
                    case "3": continuar = false; break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
        }

        // ---------- Simulación ----------

        static Resultado SimularPregunta()
        {
            // R = random entre 0 y 1 → ¿Sabe? R <= 0.75
            if (Aleatorio.NextDouble() <= ProbabilidadSaber)
            {
                return Resultado.SabiaCorrecta;
            }
            // No sabe: R2 = random entre 1 y 5 → ¿es correcta?
            int respuestaEscogida = Aleatorio.Next(1, NumeroOpciones + 1);
            return respuestaEscogida == OpcionCorrecta
                ? Resultado.AdivinoCorrecta
                : Resultado.AdivinoIncorrecta;
        }

        static Resultado[] SimularArreglo(int cantidad)
        {
            var resultados = new Resultado[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                resultados[i] = SimularPregunta();
            }
            return resultados;
        }

        static int[] Contar(Resultado[] resultados)
        {
            var conteo = new int[TodosLosResultados.Length];
            foreach (var r in resultados)
            {
                conteo[(int)r]++;
            }
            return conteo;
        }

        static int ContarCorrectas(int[] conteo) =>
            conteo[(int)Resultado.SabiaCorrecta] + conteo[(int)Resultado.AdivinoCorrecta];

        static double ProbabilidadTeoricaDe(Resultado r) => r switch
        {
            Resultado.SabiaCorrecta => ProbabilidadSaber,                                   // 0.75
            Resultado.AdivinoCorrecta => (1 - ProbabilidadSaber) * ProbabilidadAdivinar,       // 0.05
            _ => (1 - ProbabilidadSaber) * (1 - ProbabilidadAdivinar)                          // 0.20
        };

        static string Describir(Resultado r) => r switch
        {
            Resultado.SabiaCorrecta => "Sabía    → ✔ correcta",
            Resultado.AdivinoCorrecta => "Adivinó  → ✔ correcta",
            _ => "Adivinó  → ✘ incorrecta"
        };

        // ---------- Modos ----------

        // B. Pide la cantidad de arreglos y los muestra uno a uno
        static void ModoManual()
        {
            int n = LeerEnteroPositivo("¿Cuántos arreglos aleatorios quieres observar? n = ");
            bool pausar = PreguntarSiNo("¿Mostrarlos de a uno presionando Enter? (s/n): ");
            var resultados = SimularArreglo(n);

            Console.WriteLine();
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Arreglo {i + 1,6}: {Describir(resultados[i])}");
                if (pausar) Console.ReadLine();
                else Console.WriteLine();
            }

            ImprimirTablaConteo(Contar(resultados), n);
        }

        // C. Calcula la probabilidad experimental con frecuencia relativa y tabula
        static void ModoAutomatico()
        {
            Console.WriteLine();
            Console.WriteLine("Convergencia de la frecuencia relativa:");
            Console.WriteLine();
            Console.WriteLine($"{"n",9} | {"Sabía ✔",8} | {"Adiv. ✔",8} | {"Adiv. ✘",8} | {"f(C)",7} | {"P(C)",5} | {"Error",7} | {"f(S|C)",7}");
            Console.WriteLine(new string('-', 82));

            int[] ultimoConteo = null;
            int ultimoN = 0;
            foreach (int n in TamanosAutomaticos)
            {
                var conteo = Contar(SimularArreglo(n));
                ImprimirFilaConvergencia(conteo, n);
                ultimoConteo = conteo;
                ultimoN = n;
            }

            Console.WriteLine();
            Console.WriteLine($"Conteo detallado para n = {ultimoN}:");
            ImprimirTablaConteo(ultimoConteo, ultimoN);

            Console.WriteLine();
            Console.WriteLine("A medida que n crece, f(C) se acerca a P(C) = 0.8 y el error tiende a 0");
            Console.WriteLine("(Ley de los grandes números).");
        }

        // ---------- Salida por consola ----------

        static void MostrarTeoria()
        {
            Console.WriteLine("=== Caso 2: Examen de selección múltiple (5 opciones) ===");
            Console.WriteLine("S  = el estudiante sabe la respuesta      P(S)    = 0.75");
            Console.WriteLine("S' = no la sabe (adivina)                 P(S')   = 0.25");
            Console.WriteLine("C  = responde correctamente               P(C|S)  = 1");
            Console.WriteLine("                                          P(C|S') = 1/5 = 0.2");
            Console.WriteLine();
            Console.WriteLine("P(C) = P(C|S)·P(S) + P(C|S')·P(S')");
            Console.WriteLine($"     = (1)(0.75) + (0.2)(0.25) = {ProbabilidadTeorica:F4}");
            Console.WriteLine();
            Console.WriteLine("Bayes: P(S|C) = P(C|S)·P(S) / P(C)");
            Console.WriteLine($"     = 0.75 / 0.8 = {ProbabilidadSaberDadoCorrecta:F4}");
        }

        static void ImprimirTablaConteo(int[] conteo, int n)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Resultado",-24} | {"Conteo",9} | {"Frec. relativa",14} | {"P teórica",9}");
            Console.WriteLine(new string('-', 66));

            foreach (var r in TodosLosResultados)
            {
                int cantidad = conteo[(int)r];
                Console.WriteLine($"{Describir(r),-24} | {cantidad,9} | {(double)cantidad / n,14:F4} | {ProbabilidadTeoricaDe(r),9:F4}");
            }

            int correctas = ContarCorrectas(conteo);
            Console.WriteLine(new string('-', 66));
            Console.WriteLine($"{"Total correctas (C)",-24} | {correctas,9} | {(double)correctas / n,14:F4} | {ProbabilidadTeorica,9:F4}");
            Console.WriteLine($"{"Total arreglos",-24} | {n,9} | {1.0,14:F4} | {1.0,9:F4}");
        }

        static void ImprimirFilaConvergencia(int[] conteo, int n)
        {
            int correctas = ContarCorrectas(conteo);
            double frecuencia = (double)correctas / n;
            double error = Math.Abs(frecuencia - ProbabilidadTeorica);
            double frecuenciaSabiaDadoCorrecta = correctas == 0
                ? 0
                : (double)conteo[(int)Resultado.SabiaCorrecta] / correctas;

            Console.WriteLine(
                $"{n,9} | {conteo[0],8} | {conteo[1],8} | {conteo[2],8} | " +
                $"{frecuencia,7:F4} | {ProbabilidadTeorica,5:F2} | {error,7:F4} | {frecuenciaSabiaDadoCorrecta,7:F4}");
        }

        // ---------- Entrada ----------

        static int LeerEnteroPositivo(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out int valor) && valor > 0)
                {
                    return valor;
                }
                Console.WriteLine("Escribe un número entero mayor que 0.");
            }
        }

        static bool PreguntarSiNo(string mensaje)
        {
            Console.Write(mensaje);
            string respuesta = Console.ReadLine()?.Trim().ToLower();
            return respuesta == "s" || respuesta == "si" || respuesta == "sí";
        }
    }
}
