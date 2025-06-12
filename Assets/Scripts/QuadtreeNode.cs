public class QuadtreeNode
{
    public FloorRect Rect;
    public bool? IsLand; // true=land, false=water, null=mixed
    public QuadtreeNode[] Children;

    public bool IsLeaf => Children == null;

    public QuadtreeNode(FloorRect rect)
    {
        Rect = rect;
    }
    public override string ToString()
    {
        return $"QuadtreeNode({Rect.x1}, {Rect.y1}, {Rect.x2}, {Rect.y2}) - IsLand: {IsLand}";
    }

    static public string PrintFullTree(QuadtreeNode node, int depth = 0)
    {
        if (node == null) return "";

        string indent = new string(' ', depth * 2);
        string result = $"{indent}{node}\n";

        if (!node.IsLeaf)
        {
            foreach (var child in node.Children)
            {
                result += PrintFullTree(child, depth + 1);
            }
        }

        return result;
    }
}