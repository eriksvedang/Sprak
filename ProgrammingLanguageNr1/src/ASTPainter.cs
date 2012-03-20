#define CANT_HANDLE_FANCY_CHARS

using System;
using System.Collections.Generic;
using System.Text;

namespace ProgrammingLanguageNr1
{
    public class ASTPainter
    {
        private AST _root;
        private AST[] allNodes;
        private AST _currentNode;
        private AST _currentParentNode;
        private bool _currentNodeIsLastChild = true;
		private bool _printExecutions;
		
		public bool PrintExecutions {
			set {
				_printExecutions = value;
        	}
		}
		
		public AST currentNode
        {
            set 
            {
                _currentNode = value;
                _currentParentNode = GetParent(_currentNode);
                _currentNodeIsLastChild = IsLastChild(_currentNode, _currentParentNode == null ? _root : _currentParentNode);
            }
            get
            {
                return _currentNode;
            }
        }
        public AST currentParentNode
        {
            get { return _currentParentNode; }
        }
        public bool currentNodeIsLastChild
        {
            get { return _currentNodeIsLastChild; }
        }


        public void PaintAST(AST pRoot)
        {
            _root = pRoot;
            //build a list of all nodes
            List<AST> tmpAllNodes = new List<AST>();
            BuildNodeList(pRoot, tmpAllNodes);
            allNodes = tmpAllNodes.ToArray();

            //collect all leaf nodes
            List<AST> leafNodes = new List<AST>();
            GetLeafNodes(pRoot, leafNodes);

            foreach (AST leaf in allNodes)
            {
                Console.WriteLine(BuildRow(leaf, pRoot));
            }
        }
       
        private void GetLeafNodes(AST pCurrentNode, List<AST> pList)
        {
            List<AST> tmpList = pCurrentNode.getChildren();
            if (tmpList == null || tmpList.Count <= 0)
            {
                pList.Add(pCurrentNode);
            }
            else
            {
                foreach (AST child in tmpList)
                {
                    GetLeafNodes(child, pList);
                }
            }
        }
        private string GetNodeString( AST pNode )
        {
			string result;
			result = /*pNode.getTokenType().ToString() + " : " + */ pNode.ToString();
			if(_printExecutions) {
				result += " : " + pNode.Executions;
			}
            return result;
        }
        private string BuildRow(AST pLeaf, AST pRoot)
        {
            currentNode = pLeaf;
            StringBuilder resultString = new StringBuilder();
            if (currentNode == _root)
            {
                PrependString(resultString, GetNodeString(currentNode) );
            }
            else if (currentNode.getChildren().Count <= 0) {
#if CANT_HANDLE_FANCY_CHARS
				PrependString(resultString, "-------- " + GetNodeString(currentNode) );
#else
                PrependString(resultString, "──────── " + GetNodeString(currentNode) );
#endif
			}
            else
#if CANT_HANDLE_FANCY_CHARS
                PrependString(resultString, "-------- " + GetNodeString(currentNode) );
#else
				PrependString(resultString, "──────┬─ " + GetNodeString(currentNode) );
#endif
         
            if (currentNodeIsLastChild)
            {
#if CANT_HANDLE_FANCY_CHARS
                PrependString(resultString, "      |");
#else
				PrependString(resultString, "      └");
#endif				
            }
            else
            {
#if CANT_HANDLE_FANCY_CHARS
                PrependString(resultString, "      |");
#else 
				PrependString(resultString, "      ├");
#endif
            }
            currentNode = currentParentNode;
            while (currentParentNode != null)
            {
                if (currentNodeIsLastChild)
                {
					PrependString(resultString, "       ");
                }
                else
                {
#if CANT_HANDLE_FANCY_CHARS
                    PrependString(resultString, "      |");
#else
					PrependString(resultString, "      │");
#endif
                }
                currentNode = currentParentNode;
            }

            return resultString.ToString();
        }

        private void PrependString(StringBuilder pStringBuilder, string pString)
        {
            pStringBuilder.Insert(0, pString);
        }

        private AST GetParent(AST pLeaf)
        {
            foreach (AST a in allNodes)
            {
                foreach (AST child in a.getChildren())
                {
                    if (child == pLeaf)
                        return a;
                }
            }
            return null;
        }
        private void BuildNodeList(AST pCurrentList, List<AST> pList)
        {
            pList.Add(pCurrentList);
            foreach(AST a in pCurrentList.getChildren())
            {
                BuildNodeList(a, pList);
            }
        }
        
        private bool IsLastChild(AST pNode, AST pParent)
        {
            if (pParent == null)
                return true;
            if (pParent.getChildren().IndexOf(pNode) == pParent.getChildren().Count - 1)
                return true;
            return false;
        }
       
    }
}
