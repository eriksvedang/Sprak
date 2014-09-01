#define MEMORY_LOG

using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace ProgrammingLanguageNr1
{
    public enum ReturnValueType
    {
        NUMBER, VOID, STRING, BOOL, ARRAY, RANGE, UNKNOWN_TYPE
    }
	
	public class ReturnValue : IComparable<ReturnValue>
	{	
		public ReturnValue()
        {
			m_returnType = ReturnValueType.VOID;
		}

        public ReturnValue(ReturnValueType type)
        {
            m_returnType = type;
			if(m_returnType == ReturnValueType.ARRAY) {
				m_arrayValue = new SortedDictionary<ReturnValue, ReturnValue>();
				#if MEMORY_LOG
				Console.WriteLine("Created array with " + m_arrayValue.Count + " items");
				#endif
			}
        }

        public ReturnValue(ReturnValueType type, object pData)
        {
            m_returnType = type;
            switch (m_returnType)
            {
            case ReturnValueType.STRING: m_stringValue = (string)pData; break;
            case ReturnValueType.NUMBER: m_numberValue = pData.GetType() == typeof(float) ? (float)pData : Convert.ToSingle(pData); break;
            case ReturnValueType.BOOL: m_boolValue = (bool)pData; break;
            case ReturnValueType.ARRAY:
	            {
					m_arrayValue = new SortedDictionary<ReturnValue, ReturnValue>();
	                int i = 0;
	                foreach(object element in (pData as IEnumerable))
	                {
	                    Type t = element.GetType();
	                    ReturnValue rv = new ReturnValue(SystemTypeToReturnValueType(t), element);
						m_arrayValue.Add(new ReturnValue(i++), rv);
	                }
					#if MEMORY_LOG
					Console.WriteLine("Created array with " + m_arrayValue.Count + " items");
					#endif
	            }
	            break;
			case ReturnValueType.RANGE:
				m_range = (Range)pData;
				break;
                default:
                    throw new Exception("Boxing error");
            }
            
        }

		public object GetValueAsObject()
        {
            switch (m_returnType)
            {
            case ReturnValueType.STRING: return m_stringValue;
            case ReturnValueType.NUMBER: return m_numberValue;
            case ReturnValueType.BOOL: return m_boolValue;
            case ReturnValueType.ARRAY: return m_arrayValue;
			case ReturnValueType.RANGE: return m_range;
			default: return null;
			}
		}

		public ReturnValue (string text)
		{
			this.StringValue = text;
			m_returnType = ReturnValueType.STRING;
		}
		
		public ReturnValue (float nr)
		{
			this.NumberValue = nr;
			m_returnType = ReturnValueType.NUMBER;
		}
		
		public ReturnValue (bool boolean)
		{
			this.BoolValue = boolean;
			m_returnType = ReturnValueType.BOOL;
		}
		
		public ReturnValue (SortedDictionary<ReturnValue, ReturnValue> arrayValue)
		{
			this.ArrayValue = arrayValue;
			m_returnType = ReturnValueType.ARRAY;
		}

		public ReturnValue (Range pRange)
		{
			this.m_range = pRange;
			m_returnType = ReturnValueType.RANGE;
		}
		
		public void setType(ReturnValueType newType) {
			m_returnType = newType;
			if(m_returnType == ReturnValueType.ARRAY && m_arrayValue == null) {
				m_arrayValue = new SortedDictionary<ReturnValue, ReturnValue>();
			}
		}
				
		public float NumberValue {
			set {
				m_numberValue = value;
				m_returnType = ReturnValueType.NUMBER;
			}
			get {
				if (m_returnType == ReturnValueType.STRING) {
					float numberValue;
					try {
						numberValue = (float)Convert.ToDouble (m_stringValue, CultureInfo.InvariantCulture);
					} catch (FormatException) {
						numberValue = 0.0f;
						//throw new Error(fe.Message);
					}
					return numberValue;
				} else if (m_returnType == ReturnValueType.BOOL) {
					return m_boolValue ? 1.0f : -1.0f;
				} else if (m_returnType == ReturnValueType.ARRAY) {
					throw new Error ("Can't convert the array " + this.ToString () + " to a number");
				} else if (m_returnType == ReturnValueType.RANGE) {
					throw new Error ("Can't convert the range " + this.ToString () + " to an array");
				}
				return m_numberValue;
			}
		}
		
		public string StringValue {
			set {
				m_stringValue = value;
				m_returnType = ReturnValueType.STRING;
			}
			get {
				return ToString();
			}
		}

		public Range RangeValue {
			set {
				m_range = value;
				m_returnType = ReturnValueType.RANGE;
			}
			get {
				if (m_returnType == ReturnValueType.RANGE) {
					return m_range;
				} else {
					throw new Error ("Can't convert " + this.ToString () + " to a range");
				}
			}
		}
		
		public bool BoolValue {
			set {
				m_boolValue = value;
				m_returnType = ReturnValueType.BOOL;
			}
			get {
				if(m_returnType == ReturnValueType.ARRAY) {
					return m_arrayValue.Count > 0;
				}
				else if(m_returnType == ReturnValueType.RANGE) {
					throw new Error ("Can't convert the range " + this.ToString () + " to a bool");
				}
				else if(m_returnType == ReturnValueType.BOOL) {
					return m_boolValue;
				}
				else if(m_returnType == ReturnValueType.STRING) {
					return m_stringValue.ToLower() == "true";
				}
				else if(m_returnType == ReturnValueType.NUMBER) {
					return m_numberValue > 0.0f;
				}
				else {
					return false;
				}
			}
		}
		
		public SortedDictionary<ReturnValue, ReturnValue> ArrayValue {
			set {
				m_arrayValue = value;
				m_returnType = ReturnValueType.ARRAY;
				#if MEMORY_LOG
				Console.WriteLine("Asigned array ref with " + m_arrayValue.Count + " items");
				#endif
			}
			get {
				if(m_returnType == ReturnValueType.ARRAY) {
					return m_arrayValue;
				}
				else if(m_returnType == ReturnValueType.RANGE) {
					throw new Exception ("Can't convert the range " + this.ToString () + " to an array");
					//TODO: should be a normal Sprak Error!
				}
				else if(m_returnType == ReturnValueType.BOOL) {
					var array = new SortedDictionary<ReturnValue, ReturnValue>();
					array.Add(new ReturnValue(0.0f), new ReturnValue(m_boolValue));
					return array;
				}
				else if(m_returnType == ReturnValueType.STRING) {
					int len = m_stringValue.Length;
					var array = new SortedDictionary<ReturnValue, ReturnValue>();
					for(int i = 0; i < len; i++) {
						string s = Convert.ToString(m_stringValue[i]);
						array.Add(new ReturnValue(i), new ReturnValue(s));
					}
					return array;
				}
				else if(m_returnType == ReturnValueType.NUMBER) {
					var array = new SortedDictionary<ReturnValue, ReturnValue>();
					array.Add(new ReturnValue(0.0f), new ReturnValue(m_numberValue));
					return array;
				}
				else {
					var array = new SortedDictionary<ReturnValue, ReturnValue>();
					return array;
				}
			}
		}

		
		public void setVoid() { m_returnType = ReturnValueType.VOID; }
		
		public ReturnValue getNewReturnValueConvertedToString() {
			return new ReturnValue(Convert.ToString(m_numberValue));
		}
		
		public ReturnValueType getReturnValueType() { return m_returnType; }
        
        public object Unpack()
        {
            switch (this.m_returnType)
            {
                case ReturnValueType.STRING:
                    return m_stringValue;
                case ReturnValueType.NUMBER:
                    return m_numberValue;
                case ReturnValueType.BOOL:
                    return m_boolValue;
                case ReturnValueType.ARRAY:
                    List<object> o = new List<object>();
                    foreach (ReturnValue r in m_arrayValue.Values)
                        o.Add(r.Unpack());
					#if MEMORY_LOG
					Console.WriteLine("Unpacked and created array with " + o.Count + " items");
					#endif
                    return o.ToArray();
                default:
                    throw new Exception("Unpack failed!");
            }
        }

        public override string ToString()
        {
            switch (this.m_returnType)
        	{ 
            case ReturnValueType.NUMBER:
                return m_numberValue.ToString(CultureInfo.InvariantCulture);
            case ReturnValueType.STRING:
                return m_stringValue.ToString();
            case ReturnValueType.VOID:
                return "void";
			case ReturnValueType.UNKNOWN_TYPE:
                return "unknown_type";
			case ReturnValueType.BOOL:
				return m_boolValue ? "true" : "false";
			case ReturnValueType.ARRAY:
				return makeArrayString();
			case ReturnValueType.RANGE:
				return m_range.ToString ();
                default:
                    throw new Exception("Type " + this.m_returnType + " not implemented!");
            }
        }

		string makeArrayString ()
		{
			if(m_arrayValue != null) {
				StringBuilder s = new StringBuilder();
				s.Append("[");
				int count = m_arrayValue.Count;
				int emergencyBreak = 0;
				//Console.WriteLine ("Keys in array: " + string.Join (", ", m_arrayValue.Keys.Select (k => "Key " + k.ToString () + " of type " + k.m_returnType).ToArray()));
				foreach(var key in m_arrayValue.Keys) {
					//Console.WriteLine ("- Looking up key " + key);
					s.Append(/*key + ":" + */m_arrayValue[key]);
					count--;
					if(count > 0) {
						s.Append(", ");
					}
					emergencyBreak++;
					if (emergencyBreak > 10) {
						s.Append ("...");
						break;
					}
				}
				s.Append("]");
				return s.ToString();
			}
			else {
				return "";
			}
		}

        public static ReturnValueType SystemTypeToReturnValueType(Type t)
        {
            if (t == typeof(void))
                return ReturnValueType.VOID;
            if (t.IsArray)
                return ReturnValueType.ARRAY;
            switch (t.Name.ToLower())
            {
                case "int":
                case "int32":
                case "single": return ReturnValueType.NUMBER;
                case "string": return ReturnValueType.STRING;
                case "boolean":
                case "bool": return ReturnValueType.BOOL; 
                default:
                    throw new Exception("ReturnValue can't handle built in type with name " + t.Name);
            }
        }

        public static ReturnValueType getReturnValueTypeFromString(string name)
        {
            switch (name.ToLower())
            {
            case "number":
                return ReturnValueType.NUMBER;
            case "string":
                return ReturnValueType.STRING;
            case "void":
                return ReturnValueType.VOID;
			case "bool":
				return ReturnValueType.BOOL;
			case "array":
				return ReturnValueType.ARRAY;
			case "range":
				return ReturnValueType.RANGE;
			case "var":
				return ReturnValueType.UNKNOWN_TYPE;
            default:
                throw new Exception("ReturnValue can't handle built in type with name " + name);
            }
        }

		public override int GetHashCode ()
		{
			if (this.m_returnType == ReturnValueType.NUMBER) {
				return (int)(this.NumberValue);
			} else if (this.m_returnType == ReturnValueType.BOOL) {
				if (this.BoolValue) {
					return 9998;
				} else {
					return 9999;
				}
			} else if (this.m_returnType == ReturnValueType.STRING) {
				int stringHash =  10000 + this.StringValue.GetHashCode () % 10000;
				//Console.WriteLine ("String hash of " + this.ToString () + " = " + stringHash);
				return stringHash;
			} else {
				return 20000 + base.GetHashCode () % 10000;
			}
		}

		public int CompareTo(ReturnValue pOther)
		{
			int diff = this.GetHashCode () - pOther.GetHashCode ();
			//Console.WriteLine ("Comparing " + this.ToString () + " with " + pOther.ToString () + ", diff = " + diff);
			return diff;
		}

		ReturnValueType m_returnType = ReturnValueType.VOID;
		float m_numberValue = 0.0f;
		string m_stringValue = "";
		bool m_boolValue = false;
		SortedDictionary<ReturnValue, ReturnValue> m_arrayValue = null;
		Range m_range;
	}
}

