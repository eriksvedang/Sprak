using System;
using System.Collections.Generic;

namespace ProgrammingLanguageNr1
{
	public class MemorySpaceNodeListCache
	{
		public MemorySpaceNodeListCache ()
		{
		}
		
		public bool hasCachedFunction(AST rootNode) {
			return m_lists.ContainsKey(rootNode);
		}
		
		public void addMemorySpaceList(List<AST> list, AST rootNode) {
			m_lists.Add(rootNode, list);
		}
		
		public List<AST> getList(AST rootNode) {
			return m_lists[rootNode];
		}
		
		public void clear() {
			m_lists.Clear();
		}
		
		Dictionary<AST, List<AST>> m_lists = new Dictionary<AST, List<AST>>();
	}
}

