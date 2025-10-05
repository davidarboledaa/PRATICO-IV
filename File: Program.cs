using System;
using System.IO;
using TreesAndGraphs.Models;
using System.Linq;

namespace TreesAndGraphs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TreesAndGraphs - Práctica #04 ===");
            Console.WriteLine("Seleccione opción:");
            Console.WriteLine("1) Cargar grafo desde archivo (edge list) y mostrar métricas");
            Console.WriteLine("2) Cargar árbol desde archivo (parent,child) y exportar DOT");
            Console.WriteLine("3) Ejemplos pre-cargados (2 grafos, 2 árboles)");
            Console.WriteLine("0) Salir");
            var opt = Console.ReadLine();
            switch (opt)
            {
                case "1": LoadGraphInteractive(); break;
                case "2": LoadTreeInteractive(); break;
                case "3": Examples(); break;
                default: Console.WriteLine("Saliendo..."); break;
            }
        }

        static void LoadGraphInteractive()
        {
            Console.Write("Archivo de grafo (ej: graph1.txt): ");
            var path = Console.ReadLine();
            if (!File.Exists(path)) { Console.WriteLine("Archivo no existe."); return; }
            var lines = File.ReadAllLines(path);
            var g = Graph.FromEdgeList(lines, directed: false);
            Console.WriteLine(g);
            var deg = g.DegreeCentrality();
            Console.WriteLine("Degree centrality (normalizada):");
            foreach (var kv in deg.OrderByDescending(k => k.Value))
                Console.WriteLine($" - {kv.Key}: {kv.Value:F3}");
            var clos = g.ClosenessCentrality();
            Console.WriteLine("Closeness centrality:");
            foreach (var kv in clos.OrderByDescending(k => k.Value))
                Console.WriteLine($" - {kv.Key}: {kv.Value:F3}");
            Console.WriteLine("Calculando betweenness (esto puede tardar en grafos grandes)...");
            var bet = g.BetweennessCentrality();
            foreach (var kv in bet.OrderByDescending(k => k.Value))
                Console.WriteLine($" - {kv.Key}: {kv.Value:F3}");
            var dot = g.ToDot("GrafoEjemplo");
            var outDot = Path.ChangeExtension(path, ".dot");
            File.WriteAllText(outDot, dot);
            Console.WriteLine($"DOT exportado a: {outDot} (usa Graphviz: dot -Tpng {outDot} -o grafo.png)");
        }

        static void LoadTreeInteractive()
        {
            Console.Write("Archivo de árbol (parent,child por línea) (ej: tree1.txt): ");
            var path = Console.ReadLine();
            if (!File.Exists(path)) { Console.WriteLine("Archivo no existe."); return; }
            var lines = File.ReadAllLines(path);
            var tree = Tree<string>.FromParentChildLines(lines);
            if (tree == null || tree.Root == null) { Console.WriteLine("No se pudo construir árbol."); return; }
            Console.WriteLine("Preorder traversal:");
            foreach (var v in tree.PreOrder()) Console.Write(v + " ");
            Console.WriteLine();
            var outDot = Path.ChangeExtension(path, ".dot");
            File.WriteAllText(outDot, tree.ToDot("ArbolEjemplo"));
            Console.WriteLine($"DOT exportado a: {outDot} (usa Graphviz: dot -Tpng {outDot} -o arbol.png)");
        }

        static void Examples()
        {
            Console.WriteLine("Generando ejemplos de grafos y árboles en la carpeta current...");
            // Example graph 1
            var g1Lines = new[]
            {
                "# Grafo 1 - social network small",
                "A,B",
                "A,C",
                "B,C",
                "B,D",
                "C,E",
                "D,E"
            };
            File.WriteAllLines("graph1.txt", g1Lines);
            // Example graph 2
            var g2Lines = new[]
            {
                "# Grafo 2 - vuelo (ciudades)",
                "Quito,Guayaquil",
                "Quito,Cuenca",
                "Cuenca,Guayaquil",
                "Quito,Loja",
                "Loja,Cuenca"
            };
            File.WriteAllLines("graph2.txt", g2Lines);
            // Example trees
            var t1 = new[]
            {
                "# Tree 1",
                "Root,A",
                "Root,B",
                "A,C",
                "A,D",
                "B,E"
            };
            File.WriteAllLines("tree1.txt", t1);
            var t2 = new[]
            {
                "# Tree 2 - organización",
                "CEO,CTO",
                "CEO,CFO",
                "CTO,Dev1",
                "CTO,Dev2",
                "CFO,Acct1"
            };
            File.WriteAllLines("tree2.txt", t2);

            Console.WriteLine("Archivos creados: graph1.txt, graph2.txt, tree1.txt, tree2.txt");
            Console.WriteLine("Ejecutando carga automática de graph1.txt...");
            var g = Graph.FromEdgeList(g1Lines);
            Console.WriteLine(g);
            File.WriteAllText("graph1.dot", g.ToDot("G1"));
            Console.WriteLine("Exportado graph1.dot");
            var tree = Tree<string>.FromParentChildLines(t1);
            File.WriteAllText("tree1.dot", tree.ToDot("T1"));
            Console.WriteLine("Exportado tree1.dot");
            Console.WriteLine("Listo. Usa Graphviz para convertir .dot a imagen.");
        }
    }
}
