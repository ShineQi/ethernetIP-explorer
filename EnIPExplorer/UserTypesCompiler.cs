using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net.EnIPStack;
using System.IO;

namespace EnIPExplorer
{
    public class UserAttribut
    {
        public string name;
        public CIPType type;
        public UserAttribut(string name, CIPType type)
        {
            this.name = name;
            this.type = type;
        }
    }

    public class UserType
    {
        public  string name;
        public List<UserAttribut> Lattr = new List<UserAttribut>();
        public UserType(string name)
        {
            this.name = name;
        }
        public void AddAtt(UserAttribut ua)
        {
            Lattr.Add(ua);
        }
        public static bool SaveUserTypes(string filename,List<UserType> l)
        {
            try
            {
                StreamWriter sw=new StreamWriter(filename);
                foreach (UserType ut in l)
                {
                    sw.WriteLine(ut.name);
                    foreach (UserAttribut ua in ut.Lattr)
                        sw.WriteLine(ua.name + ";" + ua.type.ToString());
                }

                sw.Close();
            }
            catch
            {
                Trace.TraceError("Error while saving user decoders in file");
                return false;
            }
            return true;
        }
        public static List<UserType> LoadUserTypes(string filename)
        {
            List<UserType> t = new List<UserType>();

            UserType ut = null;
            int line = 0;

            try
            {
                StreamReader sr = new StreamReader(filename);
                while (!sr.EndOfStream)
                {
                    string content = sr.ReadLine(); line++;
                    string[] attdescr = content.Split(';');

                    if (attdescr.Length == 1)
                    {
                        if (ut != null) t.Add(ut);
                        ut = new UserType(attdescr[0]);
                    }
                    else
                    {
                        CIPType ciptype;
                        if (Enum.TryParse<CIPType>(attdescr[1], out ciptype) == true)
                        {
                            UserAttribut at = new UserAttribut(attdescr[0], ciptype);
                            ut.AddAtt(at);
                        }
                        else
                            Trace.TraceError("Error in UserTypeFile, line " + line.ToString());
                    }
                }
                if (ut != null) t.Add(ut);
                sr.Close();
            }
            catch
            {
                Trace.TraceError("Error in UserTypeFile, line " + line.ToString());
            }

            return t;
        }
    }

    public class UserTypesCompiler
    {     
        // C# type equiv and in same order than in the enum CIPType
        string[] CIPType2dotNET =
        {
            "bool",
            "sbyte",
            "Int16",
            "Int32",
            "Int64",
            "byte",
            "UInt16",
            "UInt32",
            "UInt64",
            "String",
            "String",
            "byte",
            "UInt16",
            "UInt32",
            "UInt64"
        };

        Type[] Compile(string code)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(System.Environment.CurrentDirectory + @"\EnipExplorer.exe");

            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
            if (results.Errors.Count > 0)
            {
                string errors = "User types Compilation failed:\n";
                foreach (CompilerError err in results.Errors)
                    errors += err.ToString() + "\n";

                Trace.WriteLine(errors);
                return null;
            }
            else
                return results.CompiledAssembly.GetTypes();
        }

        string MakeClassCode(List<UserType> Ltype)
        {

            StringBuilder sb = new StringBuilder(
                            @"using System;
                            using System.Net.EnIPStack.ObjectsLibrary;
                            namespace System.Net.EnIPStack.ObjectsLibrary {");
            //sb.Append("\n");
            foreach (UserType t in Ltype)
            {
                sb.Append("public class " + t.name + ":CIPBaseUserDecoder{");

                StringBuilder DecodeAttrMethod = new StringBuilder("public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b){");

                foreach (UserAttribut ua in t.Lattr)
                {
                    // Property declaration
                    sb.Append("public " +CIPType2dotNET[(byte)ua.type] + "? " + ua.name + " " + "{ get; set; }");
                    // property decoding
                    if (ua.type == CIPType.SHORT_STRING)
                        DecodeAttrMethod.Append(ua.name + "=GetShortString(ref Idx, b);");
                    else
                        DecodeAttrMethod.Append(ua.name + "=Get"+CIPType2dotNET[(byte)ua.type]+"(ref Idx, b);");
                }

                DecodeAttrMethod.Append("Finish(Idx,b);return true;}"); // end method

                sb.Append(DecodeAttrMethod);
                sb.Append("}"); // closing class
            }

            sb.Append("}"); // closing namespace
            return sb.ToString();
        }        

        public Type[] GetUserTypeDecoders()
        {
            /*
            UserAttribut ua = new UserAttribut("Essai", CIPType.INT);
            UserType ut = new UserType("MyAttributDecoder");
            ut.AddAtt(ua);

            List<UserType> lu=new List<UserType>();
            lu.Add(ut);

            Type[] t=Compile(MakeClassCode(lu));
             * */

            return Compile(MakeClassCode(UserType.LoadUserTypes("c:\\toto.txt")));
        }

    }
}
