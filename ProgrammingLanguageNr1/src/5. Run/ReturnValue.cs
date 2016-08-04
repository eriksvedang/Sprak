//#define MEMORY_LOG

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

	public struct VoidType {
		public static VoidType voidType = new VoidType();
	};

	public struct UnknownType {
		public static UnknownType unknownType = new UnknownType();
	};

	public class ReturnValueConversions {

		public static T SafeUnwrap<T>(object[] args, int index) {
			if(args[index].GetType() == typeof(T)) {
				return (T)args[index];
			}
			else {
				throw new Error("Arg " + index + " is of wrong type (" + ReturnValueConversions.PrettyObjectType(args[index].GetType()) + 
				                "), should be " + ReturnValueConversions.PrettyObjectType(typeof(T)));
			}
		}

//		static Dictionary<Type, ReturnValueType> typeToReturnValueType = new Dictionary<Type, ReturnValueType>() {
//			{ typeof(float), ReturnValueType.NUMBER },
//			{ typeof(string), ReturnValueType.STRING },
//			{ typeof(bool), ReturnValueType.BOOL },
//			{ typeof(object[]), ReturnValueType.ARRAY },
//			{ typeof(Range), ReturnValueType.RANGE },
//			{ typeof(VoidType), ReturnValueType.VOID },
//		};
		
		public static ReturnValueType SystemTypeToReturnValueType(Type t) {
			if(t == typeof(SortedDictionary<KeyWrapper,object>)) {
				return ReturnValueType.ARRAY;
			}
			else if(t == typeof(float)) {
				return ReturnValueType.NUMBER;
			}
			else if(t == typeof(bool)) {
				return ReturnValueType.BOOL;
			}
			else if(t == typeof(string)) {
				return ReturnValueType.STRING;
			}
			else if(t == typeof(Range)) {
				return ReturnValueType.RANGE;
			}
			else if(t == typeof(VoidType)) {
				return ReturnValueType.VOID;
			}
			else {
				return ReturnValueType.UNKNOWN_TYPE;
			}
//			ReturnValueType retValType = ReturnValueType.UNKNOWN_TYPE;
//			typeToReturnValueType.TryGetValue(t, out retValType);
//			return retValType;
		}


		static Dictionary<Type, string> typeToString = new Dictionary<Type, string>() {
			{ typeof(float), "number" },
			{ typeof(string), "string" },
			{ typeof(bool), "bool" },
			{ typeof(object[]), "prim-array" },
		};
		
		static public string PrettyObjectType (Type t) 
		{
			if(t == typeof(SortedDictionary<KeyWrapper,object>)) {
				return "array";
			}
			
			string s;
			if(typeToString.TryGetValue(t, out s)) {
				return s;
			}
			
			return "unknown";
		}

		static public string PrettyStringRepresenation(object o) {

			//Console.WriteLine("Will pretty print object " + o.ToString() + " of type " + o.GetType());

			if(o.GetType() == typeof(string)) {
				//return "'" + (string)o + "'";
				return (string)o;
			}
			else if(o.GetType() == typeof(bool)) {
				return ((bool)o) ? "true" : "false";
			}
			else if(o.GetType() == typeof(float)) {
				return ((float)o).ToString(CultureInfo.InvariantCulture);
			}
			else if(o.GetType() == typeof(int)) {
				return o.ToString() + "i";
			}
			else if(o.GetType() == typeof(object[])) {
				return MakePrimitiveObjectArrayString((object[])o);
			}
			else if(o is Range) {
				return ((Range)o).ToString();
			}
			else if(o is KeyWrapper) {
				return "{" + ((KeyWrapper)o).value.ToString() + "}";
			}
//			else if(o is ReturnValueType) {
//				return o.ToString();
//			}
			else if(o is SortedDictionary<KeyWrapper,object>) {
				return MakeArrayString(o as SortedDictionary<KeyWrapper,object>);
			}
			else if(o.GetType() == typeof(UnknownType)) {
				return o.ToString();
			}

			throw new Error("Can't pretty print " + o.ToString() + " of type " + o.GetType());
		}

		static string MakePrimitiveObjectArrayString (object[] array)
		{
			if(array != null) {
				StringBuilder s = new StringBuilder();
				s.Append("@[");
				int count = array.Length;
				int emergencyBreak = 0;
				for(int i = 0; i < array.Length; i++) {
					s.Append(PrettyStringRepresenation(array[i]));
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

		static string MakeArrayString (SortedDictionary<KeyWrapper,object> array)
		{
			if(array != null) {
				StringBuilder s = new StringBuilder();
				s.Append("[");
				int count = array.Count;
				int emergencyBreak = 0;
				//Console.WriteLine ("Keys in array: " + string.Join (", ", m_arrayValue.Keys.Select (k => "Key " + k.ToString () + " of type " + k.m_returnType).ToArray()));
				foreach(var key in array.Keys) {
					//Console.WriteLine ("- Looking up key " + key);
					s.Append(/*PrettyStringRepresenation(key.value) + "=>" + */PrettyStringRepresenation(array[key]));
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

		public static object ChangeTypeBasedOnReturnValueType (object obj, ReturnValueType type)
		{
			//Console.WriteLine("Will try to change obj '" + PrettyStringRepresenation(obj) + "' of type " + obj.GetType() + " to return value type " + type);

			if(type == ReturnValueType.STRING) {
				return PrettyStringRepresenation(obj);
			}
			else if(type == ReturnValueType.NUMBER) {
				if(obj.GetType() == typeof(float)) {
					return (float)obj;
				}
				if(obj.GetType() == typeof(int)) {
					// This is a HACK since I couldn't get all obj:s to be floats, some ints were getting trough even though I tried to weed them out :/
					return (float)(int)obj;
				}
				else if(obj.GetType() == typeof(string)) {
					try {
						return (float)Convert.ToDouble ((string)obj, CultureInfo.InvariantCulture);
					}
					catch(System.FormatException) {
						throw new Error("Can't convert " + obj.ToString() + " to a number");
					}
				}
			}
			else if(type == ReturnValueType.RANGE) {
				return (Range)obj;
			}
			else if(type == ReturnValueType.ARRAY) {
				if(obj.GetType() == typeof(object[])) {
					return obj;
				}
				else if(obj.GetType() == typeof(SortedDictionary<KeyWrapper,object>)) {
					//Console.WriteLine("Not changing type of array " + PrettyStringRepresenation(obj));
					return obj;
				}
				else if(obj.GetType() == typeof(Range)) {
					return obj;
				}
				else if(obj.GetType() == typeof(string)) {
					var a = new SortedDictionary<KeyWrapper,object>();
					string s = (string)obj;
					for(int i = 0; i < s.Length; i++) {
						a.Add(new KeyWrapper((float)i), s);
					}
					return a;
				}
				else {
					throw new Error("Can't convert " + obj.ToString() + " to an array");
				}
			}
			else if(type == ReturnValueType.BOOL) {
				return (bool)obj;
			}
			else if(type == ReturnValueType.UNKNOWN_TYPE) {
				return obj;
			}

			throw new Exception("Can't change type from " + obj.GetType() + " to " + type);
		}
	}


	public struct KeyWrapper : IComparable<KeyWrapper>
	{
		public KeyWrapper(object o) {
			this.value = o;
		}

		public object value;

		public override int GetHashCode () {
			if (this.value.GetType() == typeof(int)) {
				return (int)this.value;
			}
			else if (this.value.GetType() == typeof(float)) {
				return (int)(float)this.value;
			} 
			else if (this.value.GetType() == typeof(bool)) {
				if ((bool)this.value) {
					return 9998;
				} else {
					return 9999;
				}
			} 
			else if (this.value.GetType() == typeof(string)) {
				return 10000 + ((string)value).GetHashCode () % 10000;
			} 
			else {
				return 20000 + base.GetHashCode () % 10000;
			}
		}

		public int CompareTo(KeyWrapper pOther) {
			int diff = this.GetHashCode () - pOther.GetHashCode ();
			//Console.WriteLine ("Comparing " + this.ToString () + " with " + pOther.ToString () + ", diff = " + diff);
			return diff;
		}
	}




//	
//	public class object : IComparable<object>
//	{	
//		public object()
//        {
//			m_returnType = ReturnValueType.VOID;
//		}
//
//        public object(ReturnValueType type)
//        {
//            m_returnType = type;
//			if(m_returnType == ReturnValueType.ARRAY) {
//				m_arrayValue = new SortedDictionary<KeyWrapper, object>();
//				#if MEMORY_LOG
//				Console.WriteLine("Created array with " + m_arrayValue.Count + " items");
//				#endif
//			}
//        }
//
//        public object(ReturnValueType type, object pData)
//        {
//            m_returnType = type;
//            switch (m_returnType)
//            {
//            case ReturnValueType.STRING: m_stringValue = (string)pData; break;
//            case ReturnValueType.NUMBER: m_numberValue = pData.GetType() == typeof(float) ? (float)pData : Convert.ToSingle(pData); break;
//            case ReturnValueType.BOOL: m_boolValue = (bool)pData; break;
//            case ReturnValueType.ARRAY:
//	            {
//					m_arrayValue = new SortedDictionary<KeyWrapper, object>();
//	                int i = 0;
//	                foreach(object element in (pData as IEnumerable))
//	                {
////						Console.WriteLine ("Element: " + element + " of type " + element.GetType());
////	                    Type t = element.GetType();
////
////						if (t == typeof(DictionaryEntry)) {
////							Console.WriteLine ("Entry!");
////							var entry = (DictionaryEntry)element;
////							var k = CreateAutomaticobject (entry.Key);
////							var v = CreateAutomaticobject (entry.Value);
////							m_arrayValue.Add (k, v);
////						} else {
////
////						}
//
//						m_arrayValue.Add (new object (i++), CreateAutomaticobject(element));
//	                }
//					#if MEMORY_LOG
//					Console.WriteLine("Created array with " + m_arrayValue.Count + " items");
//					#endif
//	            }
//	            break;
//			case ReturnValueType.RANGE:
//				m_range = (Range)pData;
//				break;
//            
//			default:
//				throw new Exception("Boxing error with type " + type + " and data: " + pData);
//            }
//            
//        }
//
//		public static object CreateAutomaticobject(object pData) {
//			var t = SystemTypeToReturnValueType(pData.GetType());
//			return new object(t, pData);
//		}
//
//		public object GetValueAsObject()
//        {
//            switch (m_returnType)
//            {
//            case ReturnValueType.STRING: return m_stringValue;
//            case ReturnValueType.NUMBER: return m_numberValue;
//            case ReturnValueType.BOOL: return m_boolValue;
//            case ReturnValueType.ARRAY: return m_arrayValue;
//			case ReturnValueType.RANGE: return m_range;
//			default: return null;
//			}
//		}
//
//		public object (string text)
//		{
//			this.StringValue = text;
//			m_returnType = ReturnValueType.STRING;
//		}
//		
//		public object (float nr)
//		{
//			this.NumberValue = nr;
//			m_returnType = ReturnValueType.NUMBER;
//		}
//		
//		public object (bool boolean)
//		{
//			this.BoolValue = boolean;
//			m_returnType = ReturnValueType.BOOL;
//		}
//		
//		public object (SortedDictionary<KeyWrapper, object> arrayValue)
//		{
//			this.ArrayValue = arrayValue;
//			m_returnType = ReturnValueType.ARRAY;
//		}
//
//		public object (Range pRange)
//		{
//			this.m_range = pRange;
//			m_returnType = ReturnValueType.RANGE;
//		}
//		
//		public void setType(ReturnValueType newType) {
//			m_returnType = newType;
//			if(m_returnType == ReturnValueType.ARRAY && m_arrayValue == null) {
//				m_arrayValue = new SortedDictionary<KeyWrapper, object>();
//			}
//		}
//				
//		public float NumberValue {
//			set {
//				m_numberValue = value;
//				m_returnType = ReturnValueType.NUMBER;
//			}
//			get {
//				if (m_returnType == ReturnValueType.NUMBER) {
//					return m_numberValue;
//				}
//				else if (m_returnType == ReturnValueType.STRING) {
//					float numberValue;
//					try {
//						numberValue = (float)Convert.ToDouble (m_stringValue, CultureInfo.InvariantCulture);
//					} catch (FormatException) {
//						throw new Error ("Can't convert the string " + this.ToString () + " to a number");
//					}
//					return numberValue;
//				} else if (m_returnType == ReturnValueType.BOOL) {
//					throw new Error ("Can't convert the bool " + this.ToString () + " to a number");
//				} else if (m_returnType == ReturnValueType.ARRAY) {
//					throw new Error ("Can't convert the array " + this.ToString () + " to a number");
//				} else if (m_returnType == ReturnValueType.RANGE) {
//					throw new Error ("Can't convert the range " + this.ToString () + " to an array");
//				}
//				throw new Error ("Can't convert " + this.ToString () + " to a number");
//			}
//		}
//		
//		public string StringValue {
//			set {
//				m_stringValue = value;
//				m_returnType = ReturnValueType.STRING;
//			}
//			get {
//				return ToString();
//			}
//		}
//
//		public Range RangeValue {
//			set {
//				m_range = value;
//				m_returnType = ReturnValueType.RANGE;
//			}
//			get {
//				if (m_returnType == ReturnValueType.RANGE) {
//					return m_range;
//				} else {
//					throw new Error ("Can't convert " + this.ToString () + " to a range");
//				}
//			}
//		}
//		
//		public bool BoolValue {
//			set {
//				m_boolValue = value;
//				m_returnType = ReturnValueType.BOOL;
//			}
//			get {
//				if(m_returnType == ReturnValueType.ARRAY) {
//					return m_arrayValue.Count > 0;
//				}
//				else if(m_returnType == ReturnValueType.RANGE) {
//					throw new Error ("Can't convert the range " + this.ToString () + " to a bool");
//				}
//				else if(m_returnType == ReturnValueType.BOOL) {
//					return m_boolValue;
//				}
//				else if(m_returnType == ReturnValueType.STRING) {
//					return m_stringValue.ToLower() == "true";
//				}
//				else if(m_returnType == ReturnValueType.NUMBER) {
//					return m_numberValue > 0.0f;
//				}
//				else {
//					return false;
//				}
//			}
//		}
//		
//		public SortedDictionary<KeyWrapper, object> ArrayValue {
//			set {
//				m_arrayValue = value;
//				m_returnType = ReturnValueType.ARRAY;
//				#if MEMORY_LOG
//				Console.WriteLine("Asigned array ref with " + m_arrayValue.Count + " items");
//				#endif
//			}
//			get {
//				if(m_returnType == ReturnValueType.ARRAY) {
//					return m_arrayValue;
//				}
//				else if(m_returnType == ReturnValueType.RANGE) {
//					throw new Error ("Can't convert the range " + this.ToString () + " to an array");
//				}
//				else if(m_returnType == ReturnValueType.BOOL) {
//					throw new Error ("Can't convert the bool " + this.ToString () + " to an array");
//				}
//				else if(m_returnType == ReturnValueType.STRING) {
//					int len = m_stringValue.Length;
//					var array = new SortedDictionary<KeyWrapper, object>();
//					for(int i = 0; i < len; i++) {
//						string s = Convert.ToString(m_stringValue[i]);
//						array.Add(new object(i), new object(s));
//					}
//					return array;
//				}
//				else if(m_returnType == ReturnValueType.NUMBER) {
//					throw new Error ("Can't convert the number " + this.ToString () + " to an array");
//				}
//				else {
//					throw new Error ("Can't convert the " + this.getPrettyReturnValueType() + " '" + this.ToString () + "' to an array");
//				}
//			}
//		}
//
//		
//		public void setVoid() { m_returnType = ReturnValueType.VOID; }
//		
//		public object getNewobjectConvertedToString() {
//			return new object(Convert.ToString(m_numberValue));
//		}
//		
//		public ReturnValueType getReturnValueType() { return m_returnType; }
//
//		public string getPrettyReturnValueType ()
//		{
//			switch (this.m_returnType)
//			{
//			case ReturnValueType.STRING:
//				return "string";
//			case ReturnValueType.NUMBER:
//				return "number";
//			case ReturnValueType.BOOL:
//				return "bool";
//			case ReturnValueType.ARRAY:
//				return "array";
//			case ReturnValueType.VOID:
//				return "void";
//			case ReturnValueType.RANGE:
//				return "range";
//			case ReturnValueType.UNKNOWN_TYPE:
//				return "unknown";
//			default:
//				throw new Exception("Case missed!");
//			}
//		}
//        
//        public object Unpack()
//        {
//            switch (this.m_returnType)
//            {
//                case ReturnValueType.STRING:
//                    return m_stringValue;
//                case ReturnValueType.NUMBER:
//                    return m_numberValue;
//                case ReturnValueType.BOOL:
//                    return m_boolValue;
//                case ReturnValueType.ARRAY:
//                    List<object> o = new List<object>();
//                    foreach (object r in m_arrayValue.Values)
//                        o.Add(r.Unpack());
//					#if MEMORY_LOG
//					Console.WriteLine("Unpacked and created array with " + o.Count + " items");
//					#endif
//                    return o.ToArray();
//                default:
//                    throw new Exception("Unpack failed!");
//            }
//        }
//
//        public override string ToString()
//        {
//            switch (this.m_returnType)
//        	{ 
//            case ReturnValueType.NUMBER:
//                return m_numberValue.ToString(CultureInfo.InvariantCulture);
//            case ReturnValueType.STRING:
//                return m_stringValue.ToString();
//            case ReturnValueType.VOID:
//                return "void";
//			case ReturnValueType.UNKNOWN_TYPE:
//                return "unknown_type";
//			case ReturnValueType.BOOL:
//				return m_boolValue ? "true" : "false";
//			case ReturnValueType.ARRAY:
//				return makeArrayString();
//			case ReturnValueType.RANGE:
//				return m_range.ToString ();
//                default:
//                    throw new Exception("Type " + this.m_returnType + " not implemented!");
//            }
//        }
//
//		string makeArrayString ()
//		{
//			if(m_arrayValue != null) {
//				StringBuilder s = new StringBuilder();
//				s.Append("[");
//				int count = m_arrayValue.Count;
//				int emergencyBreak = 0;
//				//Console.WriteLine ("Keys in array: " + string.Join (", ", m_arrayValue.Keys.Select (k => "Key " + k.ToString () + " of type " + k.m_returnType).ToArray()));
//				foreach(var key in m_arrayValue.Keys) {
//					//Console.WriteLine ("- Looking up key " + key);
//					s.Append(/*key + ":" + */m_arrayValue[key]);
//					count--;
//					if(count > 0) {
//						s.Append(", ");
//					}
//					emergencyBreak++;
//					if (emergencyBreak > 10) {
//						s.Append ("...");
//						break;
//					}
//				}
//				s.Append("]");
//				return s.ToString();
//			}
//			else {
//				return "";
//			}
//		}
//
//        public static ReturnValueType SystemTypeToReturnValueType(Type t)
//        {
//			if (t == typeof(void)) {
//				return ReturnValueType.VOID;
//			}
//
//			if (t.IsArray || t == typeof(SortedDictionary<KeyWrapper,object>)) {
//				return ReturnValueType.ARRAY;
//			}
//
//            switch (t.Name.ToLower())
//            {
//			case "int":
//			case "int32":
//			case "single":
//				return ReturnValueType.NUMBER;
//			case "string":
//				return ReturnValueType.STRING;
//			case "boolean":
//			case "bool":
//				return ReturnValueType.BOOL;
//			case "object":
//				return ReturnValueType.UNKNOWN_TYPE;
//            default:
//				throw new Exception("object.SystemTypeToReturnValueType can't handle built in type with name " + t.Name);
//            }
//        }
//
//        public static ReturnValueType getReturnValueTypeFromString(string name)
//        {
//            switch (name.ToLower())
//            {
//            case "number":
//                return ReturnValueType.NUMBER;
//            case "string":
//                return ReturnValueType.STRING;
//            case "void":
//                return ReturnValueType.VOID;
//			case "bool":
//				return ReturnValueType.BOOL;
//			case "array":
//				return ReturnValueType.ARRAY;
//			case "range":
//				return ReturnValueType.RANGE;
//			case "var":
//				return ReturnValueType.UNKNOWN_TYPE;
//			case "unknown_type":
//				return ReturnValueType.UNKNOWN_TYPE;
//            default:
//				throw new Exception("object.getReturnValueTypeFromString can't handle built in type with name " + name);
//            }
//        }
//
//		public override int GetHashCode ()
//		{
//			if (this.m_returnType == ReturnValueType.NUMBER) {
//				return (int)(this.NumberValue);
//			} else if (this.m_returnType == ReturnValueType.BOOL) {
//				if (this.BoolValue) {
//					return 9998;
//				} else {
//					return 9999;
//				}
//			} else if (this.m_returnType == ReturnValueType.STRING) {
//				int stringHash =  10000 + this.StringValue.GetHashCode () % 10000;
//				//Console.WriteLine ("String hash of " + this.ToString () + " = " + stringHash);
//				return stringHash;
//			} else {
//				return 20000 + base.GetHashCode () % 10000;
//			}
//		}
//
//		public int CompareTo(object pOther)
//		{
//			int diff = this.GetHashCode () - pOther.GetHashCode ();
//			//Console.WriteLine ("Comparing " + this.ToString () + " with " + pOther.ToString () + ", diff = " + diff);
//			return diff;
//		}
//
//		ReturnValueType m_returnType = ReturnValueType.VOID;
//		float m_numberValue = 0.0f;
//		string m_stringValue = "";
//		bool m_boolValue = false;
//		SortedDictionary<KeyWrapper, object> m_arrayValue = null;
//		Range m_range;
//	}
}

