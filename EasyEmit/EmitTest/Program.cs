using System;
using System.Reflection.Emit;
using System.Reflection;
using EasyEmit.Creator;
using System.Collections.Generic;
using EasyEmit.Metadata;
using System.Linq.Expressions;

namespace EmitTest
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public  sealed class TestAttribute : Attribute
    {

        public TestAttribute()
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var lol = new AssemblyCreator("testAssembly");
            EnumCreator enumCreator = lol.CreateEnum("lol");
            enumCreator.AddEntry("lol", 1);
            enumCreator.Compile();
            TypeCreator typeCreator = lol.CreateClass("lol2");
            GenericParameterCreator genericParameter = typeCreator.SetGenericParameter("bon")[0];
            typeCreator.AddConstructor(MethodAttributes.Public, new List<Metadata>() { genericParameter});
            typeCreator.AddField("lol", genericParameter, FieldAttributes.Public);
            MethodCreator methodCreator = typeCreator.AddMethod("hey", MethodAttributes.Public, CallingConventions.HasThis);
            methodCreator.SetParameters(genericParameter);
            methodCreator.SetReturnType(genericParameter);
            methodCreator.ConfigureParameter(1, ParameterAttributes.None, "salut");
            typeCreator.AddProperty("salut", genericParameter, PropertyAttributes.HasDefault);
            Type type1 = typeCreator.Compile();
            object test = Activator.CreateInstance(type1.MakeGenericType(typeof(int)),Enum.ToObject(enumCreator,1));
            return;
        }
    }
}
