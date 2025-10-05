using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreesAndGraphs.Models
{
    public class Graph
    {
        // adjacency list
        private readonly Dictionary<string, HashSet<string>> _adj;

        public bool Directed { get; }

        public Graph(bool directed = false)
        {
            Directed = directed;
            _adj = new Dictionary<string, HashSet<string>>();
        }

        public void AddNode(string id)
        {
            if (!_adj.ContainsKey(id)) _adj[id] = new HashSet<string>();
        }

        public void AddEdge(string a, string b)
        {
            AddNode(a);
            AddNode(b);
            _adj[a].Add(b);
            if (!Directed)
                _adj[b].Add(a);
        }

        public IEnumerable<string> Nodes => _adj.Keys;

        public IEnumerable<(string, string)> Edges()
        {
            var seen = new HashSet<string>();
            foreach (var kv in _adj)
            {
                var u = kv.Key;
                foreach (var v in kv.Value)
                {
                    if (Directed)
                        yield return (u, v);
                    else
                    {
                        // avoid duplicates for undirected
                        var key = u.CompareTo(v) <= 0 ? $"{u}|{v}" : $"{v}|{u}";
                        if (!seen.Contains(key))
                        {
                            seen.Add(key);
                            yield return (u, v);
                        }
                    }
                }
            }
        }

        public int Degree(string node)
        {
            if (!_adj.ContainsKey(node)) return 0;
            return _adj[node].Count;
        }

        // BFS distances from source
        public Dictionary<string, int> BFS(string source)
        {
            var dist = new Dictionary<string, int>();
            var q = new Queue<string>();
            foreach (var n in Nodes) dist[n] = int.MaxValue;
            if (!dist.ContainsKey(source)) return dist;
            dist[source] = 0;
            q.Enqueue(source);
            while (q.Count > 0)
            {
                var u = q.Dequeue();
                foreach (var v in _adj[u])
                {
                    if (dist[v] == int.MaxValue)
                    {
                        dist[v] = dist[u] + 1;
                        q.Enqueue(v);
                    }
                }
            }
            return dist;
        }

        // Degree centrality: normalized degree
        public Dictionary<string, double> DegreeCentrality()
        {
            var n = _adj.Count;
            var dict = new Dictionary<string, double>();
            foreach (var node in Nodes)
            {
                dict[node] = n > 1 ? (double)Degree(node) / (n - 1) : 0.0;
            }
            return dict;
        }

        // Closeness centrality (based on BFS distances)
        public Dictionary<string, double> ClosenessCentrality()
        {
            var dict = new Dictionary<string, double>();
            var n = _adj.Count;
            foreach (var node in Nodes)
            {
                var dist = BFS(node);
                var sum = dist.Values.Where(d => d < int.MaxValue).Sum();
                if (sum > 0)
                    dict[node] = (n - 1) / (double)sum;
                else
                    dict[node] = 0;
            }
            return dict;
        }

        // Betweenness centrality (Brandes simple implementation)
        public Dictionary<string, double> BetweennessCentrality()
        {
            var nodes = Nodes.ToList();
            var CB = nodes.ToDictionary(v => v, v => 0.0);
            foreach (var s in nodes)
            {
                var stack = new Stack<string>();
                var pred = nodes.ToDictionary(v => v, v => new List<string>());
                var sigma = nodes.ToDictionary(v => v, v => 0.0);
                var dist = nodes.ToDictionary(v => v, v => -1);
                sigma[s] = 1;
                dist[s] = 0;
                var q = new Queue<string>();
                q.Enqueue(s);
                while (q.Count > 0)
                {
                    var v = q.Dequeue();
                    stack.Push(v);
                    foreach (var w in _adj[v])
                    {
                        if (dist[w] < 0)
                        {
                            dist[w] = dist[v] + 1;
                            q.Enqueue(w);
                        }
                        if (dist[w] == dist[v] + 1)
                        {
                            sigma[w] += sigma[v];
                            pred[w].Add(v);
                        }
                    }
                }
                var delta = nodes.ToDictionary(v => v, v => 0.0);
                while (stack.Count > 0)
                {
                    var w = stack.Pop();
                    foreach (var v in pred[w])
                    {
                        delta[v] += (sigma[v] / sigma[w]) * (1 + delta[w]);
                    }
                    if (w != s)
                        CB[w] += delta[w];
                }
            }
            // normalization optional for undirected graphs: divide by 2
            if (!Directed)
            {
                foreach (var k in nodes) CB[k] /= 2.0;
            }
            return CB;
        }

        // Export to DOT format
        public string ToDot(string graphName = "G")
        {
            var sb = new StringBuilder();
            var header = Directed ? "digraph" : "graph";
            var conn = Directed ? "->" : "--";
            sb.AppendLine($"{header} {graphName} {{");
            foreach (var n in Nodes)
            {
                sb.AppendLine($"  \"{n}\";");
            }
            foreach (var (u, v) in Edges())
            {
                sb.AppendLine($"  \"{u}\" {conn} \"{v}\";");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Nodes:");
            foreach (var n in Nodes) sb.AppendLine($" - {n}");
            sb.AppendLine("Edges:");
            foreach (var (u, v) in Edges()) sb.AppendLine($" - {u} {(Directed ? "->" : "--")} {v}");
            return sb.ToString();
        }

        // Load from a simple edge list text format (each line: nodeA,nodeB)
        public static Graph FromEdgeList(string[] lines, bool directed = false)
        {
            var g = new Graph(directed);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                var parts = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    g.AddEdge(parts[0].Trim(), parts[1].Trim());
                }
                else if (parts.Length == 1)
                {
                    g.AddNode(parts[0].Trim());
                }
            }
            return g;
        }
    }
}
