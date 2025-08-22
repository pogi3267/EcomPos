using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities
{
    public static class DFS
    {
        private static Dictionary<string, string> visited;
        private static List<CategorySelect2Option> output;

        private static string PrefixAddition(int depth, string name)
        {
            string prefix = string.Empty;
            for (int i = 0; i < depth; i++)
            {
                prefix += "**";
            }
            return prefix + name;
        }

        private static void ParentChildRelation(ref List<CategorySelect2Option> categoryList, string currentParent, string name, int depth)
        {
            if (visited.ContainsKey(currentParent)) return;
            visited.Add(currentParent, currentParent);
            output.Add(new CategorySelect2Option { Id = currentParent.ToString(), Text = PrefixAddition(depth, name) });
            List<CategorySelect2Option> ancestorList = categoryList.FindAll(item => item.ParentId == Convert.ToInt32(currentParent));
            foreach (var item in ancestorList)
            {
                ParentChildRelation(ref categoryList, item.Id, item.Text, depth + 1);
            }
        }

        public static List<CategorySelect2Option> GetCategory(List<CategorySelect2Option> categoryList)
        {
            visited = new Dictionary<string, string>();
            output = new List<CategorySelect2Option>();
            foreach (var item in categoryList)
            {
                if (!visited.ContainsKey(item.Id))
                {
                    ParentChildRelation(ref categoryList, item.Id, item.Text, 0);
                }
            }
            return output;
        }
    }
}