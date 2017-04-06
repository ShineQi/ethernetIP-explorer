/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2017 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
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
    public struct UserAttribut
    {
        public string name;
        public CIPType type;
        public UserAttribut(string name, CIPType type)
        {
            this.name = name;
            this.type = type;
        }
    }

    public struct UserType
    {
        public  string name;
        public List<UserAttribut> Lattr;
        public UserType(string name)
        {
            this.name = name;
            Lattr = new List<UserAttribut>();
        }

        public override string ToString()
        {
            return name;
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

            UserType? ut = null;
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
                        if (ut != null) t.Add(ut.Value);
                        ut = new UserType(attdescr[0]);
                    }
                    else
                    {
                        CIPType ciptype;
                        if (Enum.TryParse<CIPType>(attdescr[1], out ciptype) == true)
                        {
                            UserAttribut at = new UserAttribut(attdescr[0], ciptype);
                            ut.Value.AddAtt(at);
                        }
                        else
                            Trace.TraceError("Error in UserTypeFile, line " + line.ToString());
                    }
                }
                if (ut != null) t.Add(ut.Value);
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

        // Compile the code provided as string, and give all the user Type ready to be used
        Type[] Compile(string code)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.ReferencedAssemblies.Add(@"System.dll");
            // some base classes are into myself !
            parameters.ReferencedAssemblies.Add(System.Environment.CurrentDirectory + @"\EnipExplorer.exe");

            parameters.GenerateExecutable = false; // DLL in memory (in fact hidden file)
            parameters.GenerateInMemory = true;    

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

        int BadNameCounter = 1;
        string GetName(string name, CodeDomProvider provider)
        {
            // Replaces bad name by an unique identifier
            if (provider.IsValidIdentifier(name))
                return name;
            else
                return "BADNAMEPROVIDED_" + (BadNameCounter++).ToString();
        }

        // Coding method : automatic C# classes generation from User descriptions
        string MakeClassCode(List<UserType> Ltype)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");

            // using & namespace
            StringBuilder sb = new StringBuilder(
                            @"using System;
                            using System.Net.EnIPStack.ObjectsLibrary;
                            namespace System.Net.EnIPStack.ObjectsLibrary {");

            foreach (UserType t in Ltype)
            {
                // Type -> class subclassof CIPBaseUserDecoder such as : class Myclass:CIPBaseUserDecoder
                sb.Append("public class " + GetName(t.name,provider) + ":CIPBaseUserDecoder{");

                // The only method DecodeAttr
                StringBuilder DecodeAttrMethod = new StringBuilder("public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b){");

                foreach (UserAttribut ua in t.Lattr)
                {
                    // Property declaration such as : public UInt16? MyField {get; set; }
                    sb.Append("public " + CIPType2dotNET[(byte)ua.type] + "? " + GetName(ua.name, provider) + " " + "{ get; set; }");
                    
                    // property decoding such as MyField=GetUInt16(ref Idx, b);
                    if (ua.type == CIPType.SHORT_STRING)
                        DecodeAttrMethod.Append(ua.name + "=GetShortString(ref Idx, b);");
                    else
                        DecodeAttrMethod.Append(ua.name + "=Get"+CIPType2dotNET[(byte)ua.type]+"(ref Idx, b);");
                }

                // call to the Finish base class method & return
                DecodeAttrMethod.Append("Finish(Idx,b);return true;}"); // end method

                sb.Append(DecodeAttrMethod);
                sb.Append("}"); // closing class
            }

            sb.Append("}"); // closing namespace
            return sb.ToString();
        }        

        public Type[] GetUserTypeDecoders(List<UserType> UserTypeList)
        {
            return Compile(MakeClassCode(UserTypeList));
        }        

    }
}
