using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreesAndGraphs.Models
{
    public class TreeNode<T>
    {
        public T Value { get; set; }
        public List<TreeNode<T>> Children { get; } = new List<TreeNode<T>>();

        public TreeNode(T value) { Value = value; }

        public void AddChild(TreeNode<T> child) => Children.Add(child);
    }

    public class Tree<T>
    {
        public TreeNode<T> Root { get; private set; }

        public Tree() { }

        public void SetRoot(TreeNode<T> root) => Root = root;

        // Preorder traversal
        public IEnumerable<T> PreOrder()
        {
            if (Root == null) yield break;
            foreach (var v in PreOrderNode(Root)) yield return v;
        }

        private IEnumerable<T> PreOrderNode(TreeNode<T> node)
        {
            yield return node.Value;
            foreach (var c in node.Children)
                foreach (var v in PreOrderNode(c)) yield return v;
        }

        // Export to DOT (tree style)
        public string ToDot(string name = "Tree")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"digraph {name} {{");
            sb.AppendLine("  node [shape=circle];");
            var id = 0;
            var map = new Dictionary<TreeNode<T>, string>();
            void Walk(TreeNode<T> n)
            {
                if (!map.ContainsKey(n)) map[n] = $"n{++id}";
                var nid = map[n];
                sb.AppendLine($"  {nid} [label=\"{n.Value}\"];");
                foreach (var c in n.Children)
                {
                    if (!map.ContainsKey(c)) map[c] = $"n{++id}";
                    sb.AppendLine($"  {nid} -> {map[c]};");
                    Walk(c);
                }
            }
            if (Root != null) Walk(Root);
            sb.AppendLine("}");
            return sb.ToString();
        }

        // Build tree from parent-child edge list lines: "parent,child"
        public static Tree<string> FromParentChildLines(string[] lines)
        {
            var nodes = new Dictionary<string, TreeNode<string>>();
            var childrenSet = new HashSet<string>();
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                var parts = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var parent = parts[0].Trim();
                    var child = parts[1].Trim();
                    if (!nodes.ContainsKey(parent)) nodes[parent] = new TreeNode<string>(parent);
                    if (!nodes.ContainsKey(child)) nodes[child] = new TreeNode<string>(child);
                    nodes[parent].AddChild(nodes[child]);
                    childrenSet.Add(child);
                }
            }
            // root is node that never appears as child; if multiple choose first
            var rootKey = nodes.Keys.FirstOrDefault(k => !childrenSet.Contains(k));
            var tree = new Tree<string>();
            if (rootKey != null) tree.SetRoot(nodes[rootKey]);
            return tree;
        }
    }
}
