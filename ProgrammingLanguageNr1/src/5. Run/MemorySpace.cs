//#define WRITE_DEBUG_INFO

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
	public class MemorySpace
	{
		public MemorySpace (string name, AST root, Scope scope, MemorySpaceNodeListCache cache)
		{
            Debug.Assert(name != null);
            Debug.Assert(root != null);
            Debug.Assert(scope != null);
			Debug.Assert(cache != null);

			m_name = name;
            m_scope = scope;
			m_cache = cache;

            //Console.WriteLine("Creating list of nodes from tree: " + root.getTreeAsString());
			
			if(m_cache.hasCachedFunction(root)) {
				//Console.WriteLine("Found cached list for " + m_name);
				m_nodes = m_cache.getList(root).ToArray();
			}
			else {
				List<AST> list = new List<AST>();
            	addToList(list, root);
				m_cache.addMemorySpaceList(list, root);
            	m_nodes = list.ToArray();
				//Console.WriteLine("Created new list for " + m_name);
			}
            
            m_currentNode = -1;

            //Console.WriteLine("New memory space " + name + " has got " + list.Count + " AST nodes in its list.");
		}

        private void addToList(List<AST> list, AST ast)
        {
            switch (ast.getTokenType())
            {
                case  Token.TokenType.FUNC_DECLARATION:
                    addToList(list, ast.getChild(2));
                    addToList(list, ast.getChild(3));
                    break;

                case Token.TokenType.IF:
                    addToList(list, ast.getChild(0));
                    list.Add(ast);
#if WRITE_DEBUG_INFO
                    Console.WriteLine(": " + ast.getTokenString() + " of type " + ast.getTokenType());
#endif
                    break;

                case Token.TokenType.LOOP:
                    list.Add(ast);
#if WRITE_DEBUG_INFO
                    Console.WriteLine(": " + ast.getTokenString() + " of type " + ast.getTokenType());
#endif
                    break;
				
				case Token.TokenType.LOOP_BLOCK:
					list.Add(ast);
#if WRITE_DEBUG_INFO
                    Console.WriteLine(": " + ast.getTokenString() + " of type " + ast.getTokenType());
#endif
                    break;

                default:
                    addChildren(list, ast);
#if WRITE_DEBUG_INFO
                    Console.WriteLine(": " + ast.getTokenString() + " of type " + ast.getTokenType());
#endif
                    list.Add(ast);
                    break;
            }
        }

        private void addChildren(List<AST> list, AST ast)
        {
            List<AST> children = ast.getChildren();
            if (children != null)
            {
                foreach (AST child in children)
                {
                    addToList(list, child);
                }
            }
        }
		
		public void setValue(string name, ReturnValue val) {
            Debug.Assert(name != null);
            Debug.Assert(val != null);

			if(m_valuesForStrings.ContainsKey(name)) {
				//Console.WriteLine("Setting the value with name " + name + " and type " + val.getReturnValueType() + " to " + val + " in " + getName());
				m_valuesForStrings[name] = val;
			} else {
				//Console.WriteLine("Setting a new value with name " + name + " and type " + val.getReturnValueType() + " to " + val + " in " + getName());
				m_valuesForStrings.Add(name, val);
			}
		}

        public bool hasValue(string name)
        {
            return m_valuesForStrings.ContainsKey(name);
        }
		
		public ReturnValue getValue(string name) {
            Debug.Assert(name != null);

			if(!m_valuesForStrings.ContainsKey(name)) {
                throw new Error("Can't find variable with name '" + name + "'");
			}
			
			return m_valuesForStrings[name];
		}

        public void PrintValues()
        {
            foreach (string name in m_valuesForStrings.Keys)
            {
                Console.WriteLine("\t\t" + name + " = " + m_valuesForStrings[name].ToString());
            }
        }
		
		public string getName() {
			return m_name;
		}

        public AST CurrentNode
        {
            get
            {
                return m_nodes[m_currentNode];
            }
        }

        public bool Next()
        {
            if (m_currentNode < m_nodes.Length - 1)
            {
                m_currentNode++;
                //Console.WriteLine(getName() + " increased iterator to " + m_currentNode);
                return true;
            }
            else
            {
                return false;
            }
        }
		
		public void MoveToStart()
        {
            m_currentNode = -1;
        }
		
        public void MoveToEnd()
        {
            m_currentNode = m_nodes.Length;
        }
		
		public void Jump(int steps)
        {
            m_currentNode += steps;
        }

        public void SetCurrentNode()
        {
            m_currentNode = m_nodes.Length;
        }

        public Scope Scope
        {
            get { return m_scope; }
        }

		string m_name;
        Dictionary<string, ReturnValue> m_valuesForStrings = new Dictionary<string, ReturnValue>();
        AST[] m_nodes;
        int m_currentNode;
        Scope m_scope;
  		MemorySpaceNodeListCache m_cache;      
	}
}

