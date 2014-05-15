using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
	public class Scope
	{
		public enum ScopeType {
			MAIN_SCOPE,
			FUNCTION_SCOPE,
			LOOP_SCOPE,
			LOOP_BLOCK_SCOPE,
			IF_SCOPE
		}
		
		public Scope (ScopeType scopeType, string name)
		{
			m_scopeType = scopeType;
            m_name = name;
		}

        public Scope(ScopeType scopeType, string name, Scope enclosingScope)
        {
            Debug.Assert(name != null && name != "");
            Debug.Assert(enclosingScope != null);
			
			m_scopeType = scopeType;
            m_name = name;
            m_enclosingScope = enclosingScope;
        }
		
		public virtual Scope getEnclosingScope() {
			return m_enclosingScope;
		}
		
		public virtual string getName() {
			return m_name;
		}
		
		public Symbol resolve(string name) {
			if (m_symbols.ContainsKey(name)) {
				return m_symbols[name];
			} 
			else if (m_enclosingScope != null) {
				return m_enclosingScope.resolve(name);
			} 
			else {
				return null;
			}
		}

        public Scope resolveToScope(string name)
        {
            if (m_symbols.ContainsKey(name))
            {
                return this;
            }
            else if (m_enclosingScope != null)
            {
                return m_enclosingScope.resolveToScope(name);
            }
            else
            {
                return null;
            }
        }
		
		public virtual void define(Symbol symbol) {
			if(m_symbols.ContainsKey(symbol.getName())) {
				throw new ProgrammingLanguageNr1.Error("Program already contains a symbol called " + symbol.getName());
			}
			m_symbols.Add(symbol.getName(), symbol);
		}
		
		public virtual bool isDefined(String symbolName) {
			return m_symbols.ContainsKey(symbolName);
		}

        public override string ToString()
        {
            return m_name;

            //StringBuilder s = new StringBuilder();

            //s.Append("\nStart of " + getName());
            //int i = 0;
            //foreach (Symbol symbol in m_symbols.Values)
            //{
            //    if (i == 0)
            //    {
            //        s.Append("\n\t");
            //    }
            //    s.Append(symbol.ToString());
            //    if (i < m_symbols.Count - 1)
            //    {
            //        s.Append(", ");
            //    }
            //    i++;
            //}
            //s.Append("\nEnd of " + getName());

            //return s.ToString();
        }

        public void PushMemorySpace(MemorySpace pMemorySpace)
        {
            //Console.WriteLine("Push " + pMemorySpace.getName() + " into scope " + this.getName());
            m_memorySpaces.Push(pMemorySpace);
        }

        public void PopMemorySpace()
        {
            m_memorySpaces.Pop();
        }

        public void setValue(string name, ReturnValue val) {
            Scope scope = resolveToScope(name);
            if(scope == null) throw new Exception("scope is null, trying to set '" + name + "' to value " + val + " from scope '" + m_name + "'");
			if(scope.m_memorySpaces == null) throw new Exception("scope.m_memorySpaces is null, trying to set " + name + " from scope " + m_name);
			if(scope.m_memorySpaces.Peek() == null) throw new Exception("scope.m_memorySpaces.Peek() is null, trying to set " + name + " from scope " + m_name);
            scope.m_memorySpaces.Peek().setValue(name, val);
		}

        public ReturnValue getValue(string name) {
            Scope scope = resolveToScope(name);
            Debug.Assert(scope != null, "Can't resolve scope " + name);
			Stack<MemorySpace> memorySpaceStack = scope.m_memorySpaces;
			if(memorySpaceStack == null) { throw new Exception("memorySpaceStack is null"); }
			MemorySpace memorySpace = memorySpaceStack.Peek();
			if(memorySpace == null) { throw new Exception("memorySpace is null"); }
					
			if(!memorySpace.hasValue(name)) {
				throw new Error("Can't access '" + name + "' (calling function too early?)");
			}
            return memorySpace.getValue(name);
        }
		
		public ScopeType scopeType {
			get {
				return m_scopeType;
			}
		}

        protected Dictionary<string, Symbol> m_symbols = new Dictionary<string, Symbol>();
		protected Scope m_enclosingScope;
		protected string m_name;
        private Stack<MemorySpace> m_memorySpaces = new Stack<MemorySpace>();    
		private ScopeType m_scopeType;

        internal void ClearMemorySpaces()
        {
            m_memorySpaces.Clear();
        }
    }
}

